#if INTERACTIVE
#load "Utility.fsx"
#load "Menu.fsx"
#load "Play.fsx"
#load "LevelSelect.fsx"
#load "Tutorial.fsx"
#load "Editor.fsx"
#endif

open System.Windows.Forms
open System.Drawing
open System
open BB.Controls

//_MENU_____________________//
let Menu = new Form(Text="Menu", TopMost=true)
let m = new BB.Controls.Home(Dock=DockStyle.Fill)
Menu.Controls.Add(m)
Menu.Show()

//_PLAY_____________________//
let Play = new Form(Text="Play", TopMost=true, Width=600, Height=350)
let p = new BB.Controls.Bouncy_Ball(Dock=DockStyle.Fill)
Play.Controls.Add(p)
//Play.Show()

//_LEVEL SELECT_____________//
let Levels = new Form(Text="Levels", TopMost=true)
let l = new BB.Controls.LevelSelect(Dock=DockStyle.Fill)
Levels.Controls.Add(l)
//Levels.Show()

//_TUTORIAL_________________//
let Tutorial = new Form(Text="Tutorial", TopMost=true, Width=600, Height=350)
let t = new BB.Controls.BouncyTutorial(Dock=DockStyle.Fill)
Tutorial.Controls.Add(t)
//Tutorial.Show()

//_EDITOR___________________//
let Editor = new Form(Text="Editor", TopMost=true)
let e = new BB.Controls.BouncyEditor(Dock=DockStyle.Fill)
Editor.Controls.Add(e)
//Editor.Show()

m.OnPlayPress.Add(fun _ ->
    Menu.Hide()
    p.setLevel 1
    Play.Show()
    )

m.OnLevelsPress.Add(fun _ ->
    Menu.Hide()
    Levels.Show()
    )

m.OnTutorialPress.Add(fun _ ->
    Menu.Hide()
    t.Reset()
    Tutorial.Show()
    )

m.OnExitPress.Add(fun _ ->
    Menu.Hide()
    Menu.Dispose()
    Play.Dispose()
    Levels.Dispose()
    Tutorial.Dispose()
    Editor.Dispose()
    )

m.OnEditorPress.Add(fun _ ->
    Menu.Hide()
    Editor.Show()
    )

p.OnEscPress.Add(fun _ ->
    Play.Hide()
    Menu.Show()
    )

l.OnExitPress.Add(fun _ ->
    Levels.Hide()
    Menu.Show()
    )

l.OnLevelSelected.Add(fun event ->
    let levelEvent = event :?> BB.Controls.LevelEventArgs
    Levels.Hide()
    p.setLevel levelEvent.Level
    Play.Show()
    )

t.OnEscPress.Add(fun _ ->
    Tutorial.Hide()
    Menu.Show()
    )

e.OnEscPress.Add(fun _ ->
    Editor.Hide()
    Menu.Show()
    )


[<System.STAThread>]
while true do 
    Application.DoEvents()


//[<System.STAThread>]
//do
  // EVENT LOOP
  //while true do
  //  Application.DoEvents()
//  Application.Run(f)

  // Temporizzazione di VideoGame
  //while true do
    //let t = System.DateTime.Now
    // Update fisica
    // Update AI
    //Application.DoEvents()
    //let dt = System.DateTime.Now - t
    //let asg=16 - int(dt.TotalMilliseconds)
    //let mutable asd = 2
    //if(asg>2) then asd <- asg
    //System.Threading.Thread.Sleep(int(asd))