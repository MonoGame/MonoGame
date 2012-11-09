namespace ${Namespace}
open MonoMac.AppKit
open MonoMac.Foundation

type AppDelegate() = 
    inherit NSApplicationDelegate()
    
    override x.FinishedLaunching(notification) =
        let game = new ${ProjectName}Game()
        game.Run()
    
    override x.ApplicationShouldTerminateAfterLastWindowClosed(sender) =
        true
 
module main =         
    [<EntryPoint>]
    let main args =
        NSApplication.Init ()
        using (new NSAutoreleasePool()) (fun n -> 
            NSApplication.SharedApplication.Delegate <- new AppDelegate()
            NSApplication.Main(args) )
        0