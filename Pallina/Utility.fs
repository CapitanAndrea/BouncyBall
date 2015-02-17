namespace BB.Controls

open System.Media
open System.Windows
open System.Windows.Forms
open System.Drawing
open System

//Definizione controlli leggeri usati per i bottoni nel menù e nella schermata di selezione livelli
[<AbstractClass>]
type LWControl() =
  let mutable position = new PointF()
  let mutable isValid = true
  let mutable captured = false
  let mutable over = false

  let mousedown = new Event<MouseEventArgs>()
  let mousemove = new Event<MouseEventArgs>()
  let mouseup = new Event<MouseEventArgs>()

  member this.Captured = captured
  member this.MouseDown = mousedown.Publish
  member this.MouseMove = mousemove.Publish
  member this.MouseUp = mouseup.Publish

  member this.IsOver
    with get() = over
    and set(v) = over <- v

  member this.IsValid
    with get() = isValid

  member this.Position
    with get() = position
    and set(v) = position <- v

  member this.Invalidate() = isValid <- false
  
  member this.InternalPaint (g:Graphics) =
    let s = g.Save()
    g.TranslateTransform(position.X, position.Y)
    this.OnPaint g
    g.Restore(s)
    isValid <- true

  member this.HitTest (p:PointF) =
    this.IsInside(new PointF(p.X - position.X, p.Y - position.Y))

  abstract GetSize : unit -> SizeF
  abstract IsInside : PointF -> bool
  abstract OnPaint : Graphics -> unit
  abstract OnMouseDown : MouseEventArgs -> unit
  abstract OnMouseMove : MouseEventArgs -> unit
  abstract OnMouseUp : MouseEventArgs -> unit

  default this.OnMouseDown e = 
    captured <- true
    mousedown.Trigger(e)
  default this.OnMouseMove e = mousemove.Trigger(e)
  default this.OnMouseUp e = 
    captured <- false
    mouseup.Trigger(e)

    //Definizione blocco//
type Block(pos:PointF, wid:float32, hei:float32) =
    
    let mutable position = pos
    let mass = 3.f
    let width = wid
    let height = hei
    let mutable pointless = false

    member this.Position
        with get() = position
        and set(p) = position <- p

    member this.Mass
        with get() = mass

    member this.Distance
        with get(y) = pos.Y-y

    member this.Height
        with get() = height

    member this.Width
        with get() = width

    member this.IsInside(p:PointF):bool=
        pointless <- false
        if(p.X>=position.X && p.X<=position.X+width) then
            if(p.Y>=position.Y && p.Y<=position.Y+height) then
                pointless <- true
        pointless

    member this.paint(g:Graphics) =
        g.FillRectangle(Brushes.Crimson, position.X, position.Y, width, height)
        g.DrawRectangle(Pens.Black, position.X, position.Y, width, height)

    new(pos:PointF) = Block(pos, 15.f, 15.f)

//Definizione pallina//
type Ball()=
    
    let mutable vx = 0.f
    let mutable vy = 0.f
    let maxSpeed = 5.f
    let mutable pointless = false

    let mutable position = new PointF(0.f, 0.f)

    let mass = 1.f
    let diameter = 7.f

    member this.VX
        with get() = vx
        and set(v) = vx <- v

    member this.VY
        with get() = vy
        and set(v) = vy <- v
        
    member this.Position
        with get() = position
        and set(p) = position <- p

    member this.Speed
        with get() = new PointF(vx, vy)

    member this.Mass
        with get() = mass

    member this.MaxVerticalSpeed
        with get() = maxSpeed

    member this.Diameter
        with get () = diameter

    member this.HitTest(brick:Block) :bool =
        pointless <- false
        let topLeft = new PointF(position.X, position.Y)
        let topRight = new PointF(position.X+diameter, position.Y)
        let bottomLeft = new PointF(position.X, position.Y+diameter)
        let bottomRight = new PointF(position.X+diameter, position.Y+diameter)
        if (brick.IsInside(topLeft)) then pointless <- true
        else if (brick.IsInside(topRight)) then pointless <- true
        else if (brick.IsInside(bottomLeft)) then pointless <- true
        else if (brick.IsInside(bottomRight)) then pointless <- true
        pointless

//Definizione moneta//
type Coin(pos:PointF) = 
    let mutable position = new PointF(pos.X+5.f, pos.Y+5.f)
    let mutable collected = false
    let diameter = 10.f
    let mutable pointless = false

    member this.Position
        with get() = position
        and set(p) = position <- p

    member this.IsCollected
        with get() = collected
        and set(collect) = collected <- collect

    member this.Diameter
        with get() = diameter

    member this.paint(g:Graphics) =
        g.FillEllipse(Brushes.Gold, position.X, position.Y, diameter, diameter)
        g.DrawEllipse(Pens.Black, position.X, position.Y, diameter, diameter)

    member this.HitTest(brick:Block) :bool =
        pointless <- false
        let topLeft = new PointF(position.X-diameter/2.f, position.Y-diameter/2.f)
        let topRight = new PointF(position.X+diameter/2.f, position.Y-diameter/2.f)
        let bottomLeft = new PointF(position.X-diameter/2.f, position.Y+diameter/2.f)
        let bottomRight = new PointF(position.X+diameter/2.f, position.Y+diameter/2.f)
        if (brick.IsInside(topLeft)) then pointless <- true
        else if (brick.IsInside(topRight)) then pointless <- true
        else if (brick.IsInside(bottomLeft)) then pointless <- true
        else if (brick.IsInside(bottomRight)) then pointless <- true
        pointless

//Definizione blocchi con punte//
type Spike(pos:PointF) =
    inherit Block(pos, 15.f, 10.f)
    
    let image = Image.FromFile(String.Concat(__SOURCE_DIRECTORY__, "/punte.png"))

    member this.paint(g:Graphics) =
        //printfn "cyao %f %f" position.X position.Y
        g.DrawImage(image, new RectangleF(base.Position.X, base.Position.Y, 15.f, 5.f))
        g.FillRectangle(Brushes.Crimson, base.Position.X, base.Position.Y+5.f, base.Width, 5.f)
        g.DrawRectangle(Pens.Black, base.Position.X, base.Position.Y+5.f, base.Width, 5.f)

//Definizione blocchi super-salto//
type JumpBlock(pos:PointF)=
    inherit Block(pos)

    let mutable position = pos
    let jump = 10.f
    let image = Image.FromFile(String.Concat(__SOURCE_DIRECTORY__, "/jump.png"))

    member this.JumpSpeed
        with get() = jump

    member this.paint(g:Graphics) =
        g.FillRectangle(Brushes.DarkGreen, base.Position.X, base.Position.Y, base.Width, base.Height)
        g.DrawRectangle(Pens.Black, base.Position.X, base.Position.Y, base.Width, base.Height)
        g.DrawImage(image, new RectangleF(base.Position.X, base.Position.Y, base.Width, base.Height))