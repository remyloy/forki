namespace Forki.SigInt

open System
open System.Diagnostics
open System.Runtime.InteropServices
open Forki

module internal Factory =
    let create () =
        if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
            // Subscribing to Console.CancelKeyPress changes the behavior of the program, when pressing Ctrl+C.
            // See SetConsoleCtrlHandler: (https://docs.microsoft.com/en-us/windows/console/setconsolectrlhandler)
            // [...] This attribute of ignoring or processing CTRL+C is inherited by child processes.
            let mutable sendingSigInt = false
            Console.CancelKeyPress.Add <| fun args -> 
                Trace.Verbose("Received {0}", args.SpecialKey)
                if args.SpecialKey = ConsoleSpecialKey.ControlC && sendingSigInt then
                    args.Cancel <- true
                    sendingSigInt <- false

            fun (proc : Process) -> 
                sendingSigInt <- true
                Forki.SigInt.Win32.Native.GenerateConsoleCtrlCEvent(proc.Id)
        else
            fun _ -> raise (PlatformNotSupportedException())