namespace BB.Controls

open System.Media
open System.Windows.Forms
open System.Drawing
open System.Drawing.Drawing2D
open System

#if INTERACTIVE
#load "Utility.fsx"
#endif

//Definizione evento per il click su un bottone per la scelta di livello
type LevelEventArgs(selectedLevel:int) =
    inherit System.EventArgs()

    let level = selectedLevel

    member this.Level
        with get() = level

type LevelChoser(l:int) =
    inherit LWControl()

    let size = new SizeF(30.f, 30.f)
    let level = l
    let area = new Drawing2D.GraphicsPath()
    let ellipse = new RectangleF(new PointF(0.f, 0.f), size)
    
    do
        area.AddEllipse(ellipse)

    override this.GetSize () =
        new SizeF(size.Width, size.Height)

    override this.IsInside p =
        area.IsVisible p

    override this.OnPaint g =
        g.FillEllipse((if this.IsOver then Brushes.Gold else Brushes.DeepSkyBlue), ellipse)
        g.DrawEllipse(Pens.Black, ellipse)
        g.DrawString(level.ToString(), new Font ("Arial", 18.f), Brushes.Black, new PointF(4.f, 3.f))


type LevelSelect() as this =
    inherit UserControl()

    let levels : LWControl array =[|
        new LevelChoser(1)
        new LevelChoser(2)
        new LevelChoser(3)
    |]

    let pressed_level= new Event<System.EventArgs>()
    let pressed_exit= new Event<System.EventArgs>()

    do
     this.SetStyle(ControlStyles.OptimizedDoubleBuffer ||| ControlStyles.AllPaintingInWmPaint, true)

    member this.OnLevelSelected = pressed_level.Publish
    member this.OnExitPress = pressed_exit.Publish

    override this.OnKeyDown e =
        match e.KeyCode with
        | Keys.Escape ->
            pressed_exit.Trigger(new System.EventArgs())
        | _ -> ()

    override this.OnMouseUp e =
        try
            let clickedButton = levels |> Seq.findIndex(fun l -> l.HitTest(new PointF(single(e.X), single(e.Y))))
            printfn "%d" clickedButton
            let event = new LevelEventArgs(clickedButton + 1)
            pressed_level.Trigger event
        with
            | :? System.Collections.Generic.KeyNotFoundException -> ()

    override this.OnMouseMove e =
        let invalidate = ref false
        levels |> Seq.iter (fun l -> 
            if l.IsOver then invalidate := true
            l.IsOver <-false)
        levels
        |> Seq.filter(fun l -> l.Captured || l.HitTest(new PointF(single(e.X), single(e.Y))))
        |> Seq.iter (fun l ->
            l.IsOver <- true )
        if !invalidate then this.Invalidate()

    override this.OnResize _ =
        levels.[0].Position <- new PointF(15.f, 30.f)
        levels.[1].Position <- new PointF(60.f, 30.f)
        levels.[2].Position <- new PointF(105.f, 30.f)
        this.Invalidate()

    override this.OnPaint e =
        let g = e.Graphics
        levels |> Seq.iter(fun l -> l.InternalPaint(g))