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

    let gravity = 0.6f

    let mutable pressRight = false
    let mutable pressLeft = false

    let ballTimer = new Timer(Interval=20)
    let mutable animate = false

    let mutable coinsLeft = 1
    let mutable win = false
    let mutable level = 1
    let mutable winBounces = 0

    let back = new Event<System.EventArgs>()

    //Livelli
    let firstLevel = fun _ ->
        blocks.Clear()
        coins.Clear()

        ball.Position <- new PointF(130.f, 60.f)
        ball.VY <- 0.f
        //aggiungo i mattoni
        coins.Add(new Coin(new PointF(5.f, 80.f)))
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
        coins.Clear()

        ball.Position <- new PointF(200.f, 150.f)
        ball.VY <- 0.f

        coins.Add(new Coin(new PointF(50.f, 180.f)))
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
        blocks.Add(new Block(new PointF(90.f, 175.f), 15.f, 45.f))
        blocks.Add(new Spike(new PointF(90.f, 165.f)))
        blocks.Add(new JumpBlock(new PointF(105.f, 205.f)))
        ballTimer.Start()

    let thirdLevel = fun () ->
        //clear
        blocks.Clear()
        coins.Clear()

        blocks.Add(new Block(new PointF(135.f, 150.f)))
        coins.Add(new Coin(new PointF(137.f, 120.f)))
        blocks.Add(new Block(new PointF(120.f, 100.f), 15.f, 50.f))
        ball.Position <- new PointF(20.f, 200.f)
        blocks.Add(new Block(new PointF(15.f, 210.f), 30.f, 15.f))
        blocks.Add(new Spike(new PointF(45.f, 215.f)))
        blocks.Add(new Block(new PointF(60.f, 210.f)))
        blocks.Add(new Spike(new PointF(75.f, 215.f)))
        coins.Add(new Coin(new PointF(75.f, 180.f)))
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
    let ko = fun levelToLoad ->
        printfn "KO! %d" levelToLoad
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

        firstLevel()
        ballTimer.Stop()

        ballTimer.Tick.Add(fun _ ->
            //printfn "ball at %f - %f" ballPosition.[0].X ballPosition.[0].Y
            if winBounces > 2 then
                win <- false
                winBounces <- 0
                level <- level + 1
                match level with
                | 1 -> firstLevel()
                | 2 -> secondLevel()
                | 3 -> thirdLevel()
                | _ -> ()
            // check rimbalzo su mattone
            let hitBlock = ref blocks.[0]
            //------------------------

            let mutable ballPosition = [|ball.Position|]
            
            //-------------------
            let nextPositionY = new PointF(ball.Position.X, ball.Position.Y+ball.VY+gravity/2.f)
            let mutable newYPos = 0.f
            let nextBall = new Ball()
            nextBall.Position <- nextPositionY
            //cerco tutti i blocchi colpiti dalla palla nella prossima posizione
            blocks |> Seq.iter(fun block -> if nextBall.HitTest(block) then hitBlocks.Add(block))
            if(hitBlocks.Count > 0) then
                if win then winBounces <- winBounces + 1
                hitBlock := hitBlocks.[0]
                match ball.VY >= 0.f with
                |true -> //pallina discendente
                    hitBlocks |> Seq.iter(fun block ->
                        if block.Position.Y > (!hitBlock).Position.Y then hitBlock := block
                        )

                    hitBlocks.Clear()
                    let distance = (!hitBlock).DistanceFromTop(ballPosition.[0].Y+ball.Diameter)
                    //tempo che impiega la palla a colpire il mattone
                    let fallingTime = -ball.VY + sqrt((pown (ball.VY) 2) + 2.f*distance)
                    let remainingTime = 20.f-fallingTime
                    let remainingTick = remainingTime/20.f
                    match ((!hitBlock):Block) with
                    | :? Spike -> //spine
                        if not win then
                            ko level
                        newYPos <- ball.Position.Y
                        //newYPos <- ball.Position.Y
                    | :? JumpBlock -> //blocco salto
                        let jump = (!hitBlock) :?> JumpBlock
                        let newPosition = (!hitBlock).Position.Y - ball.Diameter - jump.JumpSpeed*remainingTick + (gravity/2.f* pown remainingTick 2)
                        newYPos <- newPosition
                        ball.VY <- -jump.JumpSpeed + gravity*remainingTick
                    | _ -> //blocco normale
                        let newPosition = (!hitBlock).Position.Y - ball.Diameter - ball.MaxVerticalSpeed*remainingTick + (gravity/2.f* pown remainingTick 2)
                        newYPos <- newPosition
                        ball.VY <- -ball.MaxVerticalSpeed + gravity*remainingTick
                        //printfn "ball at %f %f" ball.Position.X ball.Position.Y
                        //printfn "block at %f %f" (!hitBlock).Position.X (!hitBlock).Position.Y
                |false -> //pallina ascendente
                    hitBlocks |> Seq.iter(fun block ->
                        if block.Position.Y < (!hitBlock).Position.Y then hitBlock := block
                        )
                    hitBlocks.Clear()
                    let distance = (!hitBlock).DistanceFromBot(ball.Position.Y)
                    //tempo che impiega la palla a colpire il mattone
                    let fallingTime = -ball.VY + sqrt((pown (ball.VY) 2) + 2.f*distance)
                    let remainingTime = 20.f-fallingTime
                    let remainingTick = remainingTime/20.f
                    newYPos <- (!hitBlock).Position.Y+(!hitBlock).Height + (gravity/2.f* pown remainingTick 2)
                    ball.VY <- gravity*remainingTick

            else //update ball speed
                newYPos <- nextPositionY.Y
                ball.VY <- ball.VY + gravity

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

            ball.Position <- new PointF(newXPos, newYPos)

            if ballPosition.[0].Y>500.f && not win then
                //pallina uscita, dallo schermo: hai perso.
                ko level
            if ballPosition.[0].Y>500.f && win then
                win <- false
                winBounces <- 0
                level <- level + 1
                match level with
                | 1 -> firstLevel()
                | 2 -> secondLevel()
                | 3 -> thirdLevel()
                | _ -> ()

            //disegno
            coins |> Seq.iter(fun coin ->
                if(coin.collectTest(ball) && coin.IsCollected = false) then
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


        //if win then
           // g.DrawString("HAI VINTO", new Font("Arial", 20.f), Brushes.Black, 100.f, 200.f)


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
        animate <- true
