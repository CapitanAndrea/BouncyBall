namespace BB.Controls

open System.Media
open System.Windows.Forms
open System.Drawing
open System.Drawing.Drawing2D


#if INTERACTIVE
#load "Utility.fsx"
#endif

type HomeButton(text:string) =
    inherit LWControl()

    let area = new Drawing2D.GraphicsPath()

    do
        area.AddLines([| new Point(0, 0); new Point(90, 0); new Point(120, 15); new Point(90, 30); new Point(0, 30)|])
    
    override this.GetSize () =
        new SizeF(100.f, 30.f)

    override this.IsInside p =
        let r = new Region(area)
        r.IsVisible(p)

    override this.OnPaint g =
        g.FillPath((if this.IsOver then Brushes.Crimson else Brushes.Brown), area)
        g.DrawPath(Pens.Black, area)
        g.DrawString(text, new Font ("Arial", 18.f), Brushes.Black, new PointF(3.f, 3.f))
        


type Home() as this =
    inherit UserControl()

    let press_start = false
    let press_info = false

    let buttons : LWControl array =[|
        new HomeButton("Play")
        new HomeButton("Tutorial")
        new HomeButton("Levels")
        new HomeButton("Editor")
    |]

    let pressed_play = new Event<System.EventArgs>()
    let pressed_tutorial = new Event<System.EventArgs>()
    let pressed_levels = new Event<System.EventArgs>()
    let pressed_exit = new Event<System.EventArgs>()
    let pressed_editor = new Event<System.EventArgs>()

    do
     this.SetStyle(ControlStyles.OptimizedDoubleBuffer ||| ControlStyles.AllPaintingInWmPaint, true)

    member this.OnPlayPress = pressed_play.Publish
    member this.OnTutorialPress = pressed_tutorial.Publish
    member this.OnLevelsPress = pressed_levels.Publish
    member this.OnExitPress = pressed_exit.Publish
    member this.OnEditorPress = pressed_editor.Publish

    override this.OnKeyDown e =
        match e.KeyCode with
        | Keys.Escape ->
            pressed_exit.Trigger(new System.EventArgs())
        | _ -> ()

    override this.OnMouseUp e =
        try
            let clickedButton = buttons |> Seq.findIndex(fun b -> b.HitTest(new PointF(single(e.X), single(e.Y))))
            match (clickedButton) with 
            | 0 -> pressed_play.Trigger(e)
            | 1 -> pressed_tutorial.Trigger(e)
            | 2 -> pressed_levels.Trigger(e)
            | 3 -> pressed_editor.Trigger(e)
            | _ -> ()
        with
            | :? System.Collections.Generic.KeyNotFoundException -> ()

    override this.OnMouseMove e =
        let invalidate = ref false
        buttons |> Seq.iter (fun b -> 
            if b.IsOver then invalidate := true
            b.IsOver <-false)
        buttons
        |> Seq.filter(fun b -> b.Captured || b.HitTest(new PointF(single(e.X), single(e.Y))))
        |> Seq.iter (fun b ->
            b.IsOver <- true )
        if !invalidate then this.Invalidate()

    override this.OnResize _ =
        buttons.[0].Position <- new PointF(0.f, 30.f)
        buttons.[1].Position <- new PointF(0.f, 90.f)
        buttons.[2].Position <- new PointF(0.f, 150.f)
        buttons.[3].Position <- new PointF(0.f, 210.f)
        this.Invalidate()

    override this.OnPaint e =
        let g = e.Graphics
        buttons |> Seq.iter(fun b -> b.InternalPaint(g))