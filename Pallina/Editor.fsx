namespace BB.Controls

open System.Drawing
open System.Drawing.Drawing2D
open System.Windows.Forms

#if INTERACTIVE
#load "Utility.fsx"
#endif

type BouncyEditor() as this =
    inherit UserControl()

    let mutable ball = new Ball()
    let blocks = new ResizeArray<Block>()
    let hitBlocks = new ResizeArray<Block>()
    let coins = new ResizeArray<Coin>()

    let mutable pressRight = false
    let mutable pressLeft = false

    let ballTimer = new Timer(Interval=20)

    let mutable coinsLeft = 1
    let mutable win = false
    let mutable winBounces = 0
    let mutable step = 6
    let mutable blockPlacing = false
    let mutable ballPlacing = false
    let mutable coinPlacing = false
    let mutable placedCoin = new Coin(new PointF(0.f, 0.f))
    let mutable placedBlock = new Block(new PointF(0.f, 0.f))

    let back = new Event<System.EventArgs>()

    let font = new Font("Arial", 18.f)

    let ko = fun _ ->
        match step with
        | 0 -> ball.Position <- new PointF(25.f, 180.f)
        | 1 -> ball.Position <- new PointF(140.f, 180.f)
        | 2 | 3 -> ball.Position <- new PointF(30.f, 90.f)
        | _ -> ()

    let updateLevel = fun _ ->
        match step with
        | 0 ->
            blocks.Clear()
            coins.Clear()
            coinsLeft <- 1
            for i in 1..15 do
                blocks.Add(new Block(new PointF(float32(i)*15.f, 210.f)))
                blocks.Add(new Block(new PointF(240.f, 0.f), 15.f, 225.f))
                blocks.Add(new Block(new PointF(0.f, 0.f), 15.f, 225.f))
            ball.Position <- new PointF(25.f, 180.f)
        | 1 ->
            for i in 1..7  do blocks.Add(new Block(new PointF((float32(i+2)*15.f), (float32(i+6)*15.f))))
            blocks.Add(new Block(new PointF(15.f, 105.f)))
            blocks.Add(new Block(new PointF(30.f, 105.f)))
        | 2 ->
            blocks.Add(new Block(new PointF(60.f, 105.f), 30.f, 15.f))
            blocks.Add(new Block(new PointF(165.f, 105.f), 75.f, 15.f))
            coins.Add(new Coin(new PointF(220.f, 70.f)))
            for i in 3..7 do blocks.Add(new Spike(new PointF(float32(i+3)*15.f, 110.f)))
        | 3 ->
            blocks.Add(new Block(new PointF(90.f, 50.f), 60.f, 15.f))
            blocks.Add(new JumpBlock(new PointF(60.f, 90.f)))
        | _ -> ()

    do 
        this.SetStyle(ControlStyles.DoubleBuffer ||| ControlStyles.AllPaintingInWmPaint, true)


        ballTimer.Tick.Add(fun _ ->
            // check rimbalzo su mattone
            let hitBlock = ref placedBlock
            //------------------------
           
           

            let mutable ballPosition = [|ball.Position|]
            //w2v.TransformPoints(ballPosition)
            
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
                    //scendendo la pallina colpisce...
                        match ((!hitBlock):Block) with
                        | :? Spike -> //spine
                            if step = 2 then
                                step <- step + 1
                                updateLevel()
                            ko()
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
                            //printfn "ball at %f %f" ball.Position.X ball.Position.Y
                            //printfn "block at %f %f" (!hitBlock).Position.X (!hitBlock).Position.Y

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

            if ball.Position.Y>500.f && not win then
                //pallina uscita, dallo schermo: hai perso.
                ko()
            //disegno
            coins |> Seq.iter(fun coin ->
                let ballRect = new RectangleF(ball.Position, new SizeF(ball.Diameter, ball.Diameter))
                let coinRect = new RectangleF(coin.Position, new SizeF(coin.Diameter, coin.Diameter))
                if(ballRect.IntersectsWith(coinRect) && coin.IsCollected = false) then
                    coin.IsCollected <- true
                    coinsLeft <- coinsLeft - 1
                    win <- coinsLeft = 0
                    printfn "coins left %d" coinsLeft
                    if step = 4 then step <- step + 1
            )
            this.Invalidate()

            match step with
            | 0 ->
                if ball.Position.X>200.f then
                    step <- step + 1
                    updateLevel()
            | 1 ->
                if ball.Position.X<30.f then
                    step <- step + 1
                    updateLevel()
            | 3 ->
                if ball.Position.X>165.f then
                    step <- step + 1
                    updateLevel()
            | _ -> ()

            //ballTimer.Stop()
            )


    override this.OnPaint e =
        let g = e.Graphics
        
        g.SmoothingMode <- SmoothingMode.HighQuality

        g.DrawString("Press h for help!", font, Brushes.Black, new PointF(10.f, 10.f))
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

        if blockPlacing then
            match placedBlock with
            | :? Spike ->
                let spike = placedBlock :?> Spike
                spike.paint(g)
            | :? JumpBlock ->
                let jb = placedBlock :?> JumpBlock
                jb.paint(g)
            | _ -> placedBlock.paint(g)

        if coinPlacing then
            placedCoin.paint(g)
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


        match step with
        | 0 -> g.DrawString("Press d to move right...", font, Brushes.Black, new PointF(10.f, 230.f))
        | 1 -> g.DrawString("... and press a to move left", font, Brushes.Black, new PointF(10.f, 230.f))
        | 2 -> g.DrawString("Try to catch the golden coin!", font, Brushes.Black, new PointF(10.f, 230.f))
        | 3 ->
            g.DrawString("Stepping on spikes will cause the level to reset", font, Brushes.Black, new PointF(10.f, 230.f))
            g.DrawString("Try the green jump-blocks to find another way", font, Brushes.Black, new PointF(10.f, 250.f))
        | 4 -> g.DrawString("Collect all the golden coins to advance level", font, Brushes.Black, new PointF(10.f, 230.f))
        | 5 ->
            g.DrawString("Tutorial completed", font, Brushes.Black, new PointF(10.f, 230.f))
            g.DrawString("Press esc to go back to the menu", font, Brushes.Black, new PointF(10.f, 250.f))
        | _ -> ()


    member this.OnEscPress = back.Publish


    override this.OnKeyDown e =
        match e.KeyCode with
        |Keys.Escape -> 
            if ballTimer.Enabled then ballTimer.Stop()
            back.Trigger(new System.EventArgs())
        |Keys.A -> if ballTimer.Enabled then pressLeft <- true
        |Keys.D -> if ballTimer.Enabled then pressRight <- true
        |Keys.P -> if ballTimer.Enabled then ballTimer.Stop() else ballTimer.Start()
        |Keys.B ->
            if blockPlacing = false && ballPlacing = false then //block
                let mousePosition = this.PointToClient(Control.MousePosition)
                let mousePositionF = new PointF(float32(mousePosition.X)-(7.5f), float32(mousePosition.Y)-(7.5f))
                placedBlock <- new Block(mousePositionF)
                blockPlacing <- true
        |Keys.V ->
            if blockPlacing = false && ballPlacing = false then //spike
                let mousePosition = this.PointToClient(Control.MousePosition)
                let mousePositionF = new PointF(float32(mousePosition.X)-(7.5f), float32(mousePosition.Y)-(7.5f))
                placedBlock <- new Spike(mousePositionF)
                blockPlacing <- true
        |Keys.C ->
            if blockPlacing = false && ballPlacing = false then //jump
                let mousePosition = this.PointToClient(Control.MousePosition)
                let mousePositionF = new PointF(float32(mousePosition.X)-(7.5f), float32(mousePosition.Y)-(7.5f))
                placedBlock <- new JumpBlock(mousePositionF)
                blockPlacing <- true
        |Keys.X ->
            if ballPlacing = false && blockPlacing = false then //spike
                let mousePosition = this.PointToClient(Control.MousePosition)
                let mousePositionF = new PointF(float32(mousePosition.X)-(3.5f), float32(mousePosition.Y)-(3.5f))
                ball.Position <- mousePositionF
                ballPlacing <- true
        |Keys.Z ->            
            if ballPlacing = false && blockPlacing = false then //coin
                let mousePosition = this.PointToClient(Control.MousePosition)
                let mousePositionF = new PointF(float32(mousePosition.X)-(5.f), float32(mousePosition.Y)-(5.f))
                placedCoin.Position <- mousePositionF
                coinPlacing <- true
        |Keys.H ->
            MessageBox.Show("B to create a new block\nV to create a new spike\nC to create a new jump-block\nX to move the ball\nZ to create a new coin\nMouse click to place the object\nP to pause/unpause the game", "Instructions") |> ignore
        |_ -> ()

        this.Invalidate()

    override this.OnKeyUp e =
        match e.KeyCode with
        |Keys.A -> pressLeft <- false
        |Keys.D -> pressRight <- false
        |_ -> ()

    override this.OnMouseClick e =
        if blockPlacing then
            blocks.Add(placedBlock)
            blockPlacing <- false
            this.Invalidate()
        if ballPlacing then
            ballPlacing <- false
            this.Invalidate()
        if coinPlacing then
            coins.Add(placedCoin)
            coinsLeft <- coinsLeft + 1
            coinPlacing <- false
            this.Invalidate()

    override this.OnMouseMove e =
        if blockPlacing then
            match placedBlock with
            | :? Spike ->
                let spike = placedBlock :?> Spike
                spike.Position <- new PointF(float32(e.X)-(7.5f), float32(e.Y)-(7.5f))
                this.Invalidate()
            | :? JumpBlock ->
                let jb = placedBlock :?> JumpBlock
                jb.Position <- new PointF(float32(e.X)-(7.5f), float32(e.Y)-(7.5f))
                this.Invalidate()
            | _ ->
                placedBlock.Position <- new PointF(float32(e.X)-(7.5f), float32(e.Y)-(7.5f))
            //printfn "sposto @ %f - %f" placedBlock.Position.X placedBlock.Position.Y
                this.Invalidate()
        if ballPlacing then
            let nextBall = new Ball()
            nextBall.Position <- new PointF(float32(e.X)-(3.5f), float32(e.Y)-(3.5f))
            if blocks |> Seq.exists(fun block ->
                nextBall.HitTest(block)
                ) then ()
                else ball.Position <- new PointF(float32(e.X)-(3.5f), float32(e.Y)-(3.5f))
            this.Invalidate()
        if coinPlacing then
            let nextCoin = new Coin(new PointF(float32(e.X)-(5.f), float32(e.Y)-(5.f)))
            if blocks |> Seq.exists(fun block ->
                nextCoin.HitTest(block)
                ) then ()
                else placedCoin.Position <- new PointF(float32(e.X)-(5.f), float32(e.Y)-(5.f))
            this.Invalidate()

    member this.Reset = fun () ->
        step <- 0
        updateLevel()
        ballTimer.Start()
        //if ballTimer.Enabled = false then ballTimer.Start()