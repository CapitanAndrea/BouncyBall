namespace BB.Controls

open System.Windows.Forms
open System.Drawing
open System.Drawing.Drawing2D

#if INTERACTIVE
#load "Utility.fsx"
#endif

//Definizione gioco//
type Bouncy_Ball() as this =
    inherit UserControl()
    //Strutture dati
    let mutable ball = new Ball()
    let blocks = new ResizeArray<Block>()
    let hitBlocks = new ResizeArray<Block>()
    let coins = new ResizeArray<Coin>()
    let spikes = new ResizeArray<Spike>()
    let jumpBlocks = new ResizeArray<JumpBlock>()

    let mutable pressRight = false
    let mutable pressLeft = false

    let ballTimer = new Timer(Interval=20)

    let mutable w2v = new Drawing2D.Matrix()
    let mutable v2w = new Drawing2D.Matrix()

    let mutable coinsLeft = 1
    let mutable win = false
    let mutable level = 3
    let mutable winBounces = 0

    let back = new Event<System.EventArgs>()

    //Livelli
    let firstLevel = fun _ ->
        blocks.Clear()
        spikes.Clear()
        coins.Clear()
        jumpBlocks.Clear()

        ball.Position <- new PointF(130.f, 60.f)
        ball.VY <- 0.f
        //aggiungo i mattoni
        coins.Add(new Coin(new PointF(0.f, 80.f)))
        coinsLeft <- 1
        win <- false
        winBounces <- 0
        for i in 0 .. 10 do
            let y = 100.f
            let x = i*15
            (*if i%2 = 0 then blocks.Add(new Block(new PointF(float32(x), y+20.f)))
            else blocks.Add(new Block(Color.Crimson, new PointF(float32(x), y)))*)
            blocks.Add(new Block(new PointF(float32(x), y)))
        //blocks.Add(new Block(new PointF(40.f, 40.f), 200.f, 20.f))
        //blocks.Add(new Block(new PointF(00.f, 40.f), 20.f, 60.f))
        //printfn "load level complete"
        //blocks.Add(new Spike(new PointF(50.f, 160.f)))
        //blocks.Add(new JumpBlock(new PointF(30.f, 160.f)))
        ballTimer.Start()

    let secondLevel = fun _ ->
        //clear
        blocks.Clear()
        spikes.Clear()
        coins.Clear()
        jumpBlocks.Clear()

        ball.Position <- new PointF(200.f, 150.f)
        ball.VY <- 0.f

        coins.Add(new Coin(new PointF(0.f, 200.f)))
        coinsLeft <- 1
        win <- false
        winBounces <- 0
        for i in 0 .. 20 do
            let y = 220.f
            let x = i*15
            (*if i%2 = 0 then blocks.Add(new Block(new PointF(float32(x), y+20.f)))
            else blocks.Add(new Block(Color.Crimson, new PointF(float32(x), y)))*)
            blocks.Add(new Block(new PointF(float32(x), y)))
        //blocks.Add(new Block(new PointF(40.f, 40.f), 200.f, 20.f))
        //blocks.Add(new Block(new PointF(00.f, 40.f), 20.f, 60.f))
        //printfn "load level complete"
        blocks.Add(new Block(new PointF(15.f, 175.f), 15.f, 45.f))
        blocks.Add(new Spike(new PointF(15.f, 165.f)))
        blocks.Add(new JumpBlock(new PointF(30.f, 205.f)))
        ballTimer.Start()

    let thirdLevel = fun () ->
        //clear
        blocks.Clear()
        spikes.Clear()
        coins.Clear()
        jumpBlocks.Clear()

        blocks.Add(new Block(new PointF(135.f, 150.f)))
        coins.Add(new Coin(new PointF(135.f, 120.f)))
        blocks.Add(new Block(new PointF(120.f, 100.f), 15.f, 50.f))

        ball.Position <- new PointF(20.f, 200.f)
        blocks.Add(new Block(new PointF(15.f, 210.f), 30.f, 15.f))
        blocks.Add(new Spike(new PointF(45.f, 215.f)))
        blocks.Add(new Block(new PointF(60.f, 210.f)))
        blocks.Add(new Spike(new PointF(75.f, 215.f)))
        coins.Add(new Coin(new PointF(73.f, 180.f)))
        blocks.Add(new Block(new PointF(90.f, 210.f)))
        blocks.Add(new Spike(new PointF(105.f, 215.f)))
        blocks.Add(new Block(new PointF(120.f, 210.f), 30.f, 15.f))
        blocks.Add(new JumpBlock(new PointF(150.f, 225.f)))
        blocks.Add(new Spike(new PointF(165.f, 215.f)))
        blocks.Add(new Spike(new PointF(180.f, 215.f)))

        blocks.Add(new Block(new PointF(15.f, 285.f), 60.f, 15.f))
        coins.Add(new Coin(new PointF(50.f, 260.f)))

        coinsLeft <- 3
        ballTimer.Start()


    //knock-out!
    let ko = fun level ->
        printfn("KO!")
        ball.VY <- 0.f
        if ballTimer.Enabled then ballTimer.Stop()
        match level with
        | 1 -> firstLevel()
        | 2 -> secondLevel()
        | 3 -> thirdLevel()
        | _ -> ()
        
    //
    do
        this.SetStyle(ControlStyles.DoubleBuffer ||| ControlStyles.AllPaintingInWmPaint, true)

        thirdLevel()
        ballTimer.Stop()

        ballTimer.Tick.Add(fun _ ->
            //printfn "ball at %f - %f" ballPosition.[0].X ballPosition.[0].Y
            if winBounces > 2 then
                level <- level + 1
                match level with
                | 1 -> firstLevel()
                | 2 -> secondLevel()
                | 3 -> thirdLevel()
                | _ -> ()
            // check rimbalzo su mattone
            let hitBlock = ref blocks.[0]
            //------------------------
           
           //blocks |> Seq.
           

            let mutable ballPosition = [|ball.Position|]
            
            //-------------------
            let nextPositionY = new PointF(ballPosition.[0].X, ballPosition.[0].Y+ball.VY+0.5f)
            let mutable newYPos = 0.f
            let nextBall = new Ball()
            nextBall.Position <- nextPositionY
            //cerco tutti i blocchi colpiti dalla palla nella prossima posizione
            blocks |> Seq.iter(fun block -> if nextBall.HitTest(block) then hitBlocks.Add(block))

            if(hitBlocks.Count > 0) then
                hitBlock := hitBlocks.[0]
                //assegno ad hitblock quello più alto nel caso la velocità sia > 0
                if ball.VY > 0.f then
                    hitBlocks |> Seq.iter(fun block ->
                        if block.Position.Y < (!hitBlock).Position.Y then hitBlock := block
                        )
                else
                    hitBlocks |> Seq.iter(fun block ->
                        if block.Position.Y > (!hitBlock).Position.Y then hitBlock := block
                        )
                hitBlocks.Clear()
                let distance = (!hitBlock).Distance(ballPosition.[0].Y+ball.Diameter)
                //tempo che impiega la palla a colpire il mattone
                let fallingTime = -ball.VY + sqrt((pown (ball.VY) 2) + 2.f*distance)
                let remainingTime = 20.f-fallingTime
                let remainingTick = remainingTime/20.f
                //inverto la velocità lungo l'asse y
                if ball.VY>0.f then
                //printfn "hitblock is %f - %f because y will be %f" (!hitBlock).Position.X (!hitBlock).Position.Y nextPositionY.Y
                //scendendo la pallina colpisce...
                    match ((!hitBlock):Block) with
                    | :? Spike -> //spine
                        ko level
                        newYPos <- ball.Position.Y
                        //newYPos <- ball.Position.Y
                    | :? JumpBlock -> //blocco salto
                        let jump = (!hitBlock) :?> JumpBlock
                        ball.VY <- -jump.JumpSpeed
                        let newPosition = (!hitBlock).Position.Y - 10.f + ball.VY*remainingTick + (0.5f* pown remainingTick 2)
                        newYPos <- newPosition
                        //printfn "on a jump block"
                    | _ -> //blocco normale
                        ball.VY <- -ball.MaxVerticalSpeed
                        let newPosition = (!hitBlock).Position.Y - 10.f + ball.VY*remainingTick + (0.5f* pown remainingTick 2)
                        newYPos <- newPosition

                else 
                    ball.VY <- 0.f
                    newYPos <- (!hitBlock).Position.Y + (!hitBlock).Height + 1.f

            else //update ball speed
                newYPos <- nextPositionY.Y
                ball.VY <- ball.VY + 1.f

            let mutable newXPos = ball.Position.X
            if pressRight then
                let nextPositionX = new PointF(ball.Position.X+2.f, newYPos)
                let nextBall = new Ball()
                nextBall.Position <- nextPositionX
                if(blocks |> Seq.exists(fun block ->
                    if nextBall.HitTest(block) then
                        hitBlock := block
                        true
                    else
                        false
                    )
                ) then (
                    let pos = (!hitBlock).Position
                    newXPos <- pos.X - ball.Diameter - 1.f
                )
                else newXPos <- nextPositionX.X

            if pressLeft then
                let nextPositionX = new PointF(ball.Position.X-2.f, newYPos)
                let nextBall = new Ball()
                nextBall.Position <- nextPositionX
                if(blocks |> Seq.exists(fun block ->
                    if nextBall.HitTest(block) then
                        hitBlock := block
                        true
                    else
                        false
                    )
                ) then (
                    let pos = (!hitBlock).Position
                    newXPos <- pos.X + (!hitBlock).Width + 1.f
                    )
                else newXPos <- nextPositionX.X


            (*blocks |> Seq.iter(fun block ->
                let bPos = block.Position
                printf "%f " bPos.X
                printfn " %f" bPos.Y
                )*)
            //printf "ball is at %f" ballPosition.[0].X
            //printfn " - %f" newYPos
            //printf "new ball x is "
            //printfn "%f" ballPosition.[0].X
            ball.Position <- new PointF(newXPos, newYPos)

            if ballPosition.[0].Y>500.f && not win then
                //pallina uscita, dallo schermo: hai perso.
                ko level
            //disegno
            coins |> Seq.iter(fun coin ->
                let ballRect = new RectangleF(ball.Position, new SizeF(ball.Diameter, ball.Diameter))
                let coinRect = new RectangleF(coin.Position, new SizeF(coin.Diameter, coin.Diameter))
                if(ballRect.IntersectsWith(coinRect) && coin.IsCollected = false) then
                    coin.IsCollected <- true
                    coinsLeft <- coinsLeft - 1
                    win <- coinsLeft = 0
                    printfn "coins left %d" coinsLeft
            )
            this.Invalidate()
            //ballTimer.Stop()
            )

    override this.OnPaint e =
        let g = e.Graphics
        
        g.SmoothingMode <- SmoothingMode.HighQuality

        //disegno blocchi
        blocks |> Seq.iter(fun block ->
            match block with
            | :? Spike ->
                let spike = block :?> Spike
                spike.paint(g)
            | :? JumpBlock ->
                let jb = block :?> JumpBlock
                jb.paint(g)
            | _ -> block.paint(g)
            )

        //disegno monete
        coins |> Seq.iter(fun coin ->
            if(coin.IsCollected = false) then
                coin.paint(g)
            )

        //disegno pallina
        let ballPosition = ball.Position
        //printfn "ball is @ %f - %f" ballPosition.X ballPosition.Y
        g.FillEllipse(Brushes.DeepSkyBlue, ballPosition.X, ballPosition.Y, ball.Diameter, ball.Diameter)
        try
            g.DrawEllipse(Pens.Black, ballPosition.X, ballPosition.Y, ball.Diameter, ball.Diameter)
        with
            | :? System.OverflowException -> printf "%f - " ball.Position.X; printfn "%f" ball.Position.Y 


        if win then
            g.DrawString("HAI VINTO", new Font("Arial", 20.f), Brushes.Black, 100.f, 200.f)


    member this.OnEscPress = back.Publish


    override this.OnKeyDown e =
        match e.KeyCode with
        |Keys.Escape -> 
            if ballTimer.Enabled then ballTimer.Stop()
            back.Trigger(new System.EventArgs())
        |Keys.A -> pressLeft <- true
        |Keys.D -> pressRight <- true
        |Keys.P -> if ballTimer.Enabled then ballTimer.Stop() else ballTimer.Start()
        |_ -> ()

    override this.OnKeyUp e =
        match e.KeyCode with
        |Keys.A -> pressLeft <- false
        |Keys.D -> pressRight <- false
        |_ -> ()

    member this.setLevel = fun x ->
        level <- x
        ko x