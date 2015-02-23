namespace BB.Controls

open System.Drawing
open System.Drawing.Drawing2D
open System.Windows.Forms
open System

#if INTERACTIVE
#load "Utility.fsx"
#endif

type BouncyEditor() as this =
    inherit UserControl()

    let mutable ball = new Ball()
    let blocks = new ResizeArray<Block>()
    let hitBlocks = new ResizeArray<Block>()
    let coins = new ResizeArray<Coin>()

    let gravity = 0.6f

    let mutable pressRight = false
    let mutable pressLeft = false

    let ballTimer = new Timer(Interval=20)

    let mutable coinsLeft = 1
    let mutable win = false
    let mutable winBounces = 0
    let mutable blockPlacing = false
    let mutable ballPlacing = false
    let mutable coinPlacing = false
    let mutable placedCoin = new Coin(new PointF(0.f, 0.f))
    let mutable placedBlock = new Block(new PointF(0.f, 0.f))

    let back = new Event<System.EventArgs>()

    let font = new Font("Arial", 18.f)

    do 
        this.SetStyle(ControlStyles.DoubleBuffer ||| ControlStyles.AllPaintingInWmPaint, true)


        ballTimer.Tick.Add(fun _ ->
            // check rimbalzo su mattone
            let hitBlock = ref placedBlock
            //------------------------
           
           

            let mutable ballPosition = [|ball.Position|]
            //w2v.TransformPoints(ballPosition)
            
            //-------------------
            let nextPositionY = new PointF(ball.Position.X, ball.Position.Y+ball.VY+gravity/2.f)
            let mutable newYPos = 0.f
            let nextBall = new Ball()
            nextBall.Position <- nextPositionY
            //cerco tutti i blocchi colpiti dalla palla nella prossima posizione
            blocks |> Seq.iter(fun block -> if nextBall.HitTest(block) then hitBlocks.Add(block))
            if(hitBlocks.Count > 0) then
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

            //disegno
            coins |> Seq.iter(fun coin ->
                if(coin.collectTest(ball) && coin.IsCollected = false) then
                    coin.IsCollected <- true
                    coinsLeft <- coinsLeft - 1
                    win <- coinsLeft = 0
                    printfn "coins left %d" coinsLeft
            )
            this.Invalidate()
            //printfn "ball %f" ball.Position.Y
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
            if blockPlacing = false && ballPlacing = false && coinPlacing = false then //block
                let mousePosition = this.PointToClient(Control.MousePosition)
                let mousePositionF = new PointF(float32(mousePosition.X)-(7.5f), float32(mousePosition.Y)-(7.5f))
                placedBlock <- new Block(mousePositionF)
                blockPlacing <- true
        |Keys.V ->
            if blockPlacing = false && ballPlacing = false && coinPlacing = false then //spike
                let mousePosition = this.PointToClient(Control.MousePosition)
                let mousePositionF = new PointF(float32(mousePosition.X)-(7.5f), float32(mousePosition.Y)-(7.5f))
                placedBlock <- new Spike(mousePositionF)
                blockPlacing <- true
        |Keys.C ->
            if blockPlacing = false && ballPlacing = false && coinPlacing = false then //jump
                let mousePosition = this.PointToClient(Control.MousePosition)
                let mousePositionF = new PointF(float32(mousePosition.X)-(7.5f), float32(mousePosition.Y)-(7.5f))
                placedBlock <- new JumpBlock(mousePositionF)
                blockPlacing <- true
        |Keys.X ->
            if ballPlacing = false && blockPlacing = false && coinPlacing = false then //ball
                let mousePosition = this.PointToClient(Control.MousePosition)
                let mousePositionF = new PointF(float32(mousePosition.X)-(3.5f), float32(mousePosition.Y)-(3.5f))
                let nextBall = new Ball()
                nextBall.Position <- mousePositionF
                if blocks |> Seq.exists(fun block ->
                    nextBall.HitTest(block)
                    ) then ()
                    else
                        ball.Position <- mousePositionF
                        ballPlacing <- true
        |Keys.Z ->            
            if ballPlacing = false && blockPlacing = false && coinPlacing = false then //coin
                let mousePosition = this.PointToClient(Control.MousePosition)
                let mousePositionF = new PointF(float32(mousePosition.X)-(5.f), float32(mousePosition.Y)-(5.f))
                placedCoin <- new Coin(mousePositionF)
                if blocks |> Seq.exists(fun block ->
                    placedCoin.HitTest(block)
                    ) then ()
                    else
                        coinsLeft <- coinsLeft + 1
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
        match e.Button with
        | MouseButtons.Right ->
            blockPlacing <- false
            coinPlacing <- false
            ballPlacing <- false
            this.Invalidate()
        | MouseButtons.Left ->
            if blockPlacing then
                blocks.Add(placedBlock)
                blockPlacing <- false
                this.Invalidate()
            else if ballPlacing then
                ballPlacing <- false
                this.Invalidate()
            else if coinPlacing then
                coins.Add(placedCoin)
                coinsLeft <- coinsLeft + 1
                coinPlacing <- false
                this.Invalidate()
            else
                let floatPosition = new PointF(float32(e.X), float32(e.Y))
                placedBlock <- blocks.FindLast(fun block ->
                    block.IsInside(floatPosition)
                    )
                if blocks.Remove(placedBlock) then blockPlacing <- true
                else
                    placedCoin <- coins.FindLast(fun coin ->
                        coin.IsInside(new PointF(float32(e.X), float32(e.Y)))
                        )
                    if coins.Remove(placedCoin) then
                        coinPlacing <- true
                        placedCoin.Position <- new PointF(floatPosition.X-placedCoin.Diameter, floatPosition.Y-placedCoin.Diameter)
        |_ ->()


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
            ball.VY <- 0.f
            this.Invalidate()
        if coinPlacing then
            let nextCoin = new Coin(new PointF(float32(e.X)-(5.f), float32(e.Y)-(5.f)))
            if blocks |> Seq.exists(fun block ->
                nextCoin.HitTest(block)
                ) then ()
                else placedCoin.Position <- new PointF(float32(e.X)-(5.f), float32(e.Y)-(5.f))
            this.Invalidate()

    member this.Reset = fun () ->
        ball.Position <- new PointF(0.f, 0.f)
        blocks.Clear()
        hitBlocks.Clear()
        coins.Clear()
        if ballTimer.Enabled = false then ballTimer.Stop()