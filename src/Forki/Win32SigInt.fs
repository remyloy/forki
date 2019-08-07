namespace Forki.SigInt.Win32

open Forki
open System.Runtime.InteropServices

type internal CtrlTypes =
  | CTRL_C_EVENT = 0u
  | CTRL_BREAK_EVENT = 1u
  | CTRL_CLOSE_EVENT = 2u
  | CTRL_LOGOFF_EVENT = 5u
  | CTRL_SHUTDOWN_EVENT = 6u

type internal Native() =
    [<DllImport("kernel32.dll", SetLastError = true)>]
    static extern bool AttachConsole(uint32 dwProcessId);
    
    [<DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)>]
    static extern bool FreeConsole();

    [<DllImport("kernel32.dll")>]
    static extern [<return: MarshalAs(UnmanagedType.Bool)>]bool GenerateConsoleCtrlEvent(CtrlTypes dwCtrlEvent, uint32 dwProcessGroupId)

    static member GenerateConsoleCtrlCEvent(processId) =
        //if AttachConsole(uint32 processId) then
        //    let generated = GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0u)
        //    Trace.Verbose("GenerateConsoleCtrlCEvent={0}", generated)
        //    FreeConsole() |> ignore
        //else
        let generated = GenerateConsoleCtrlEvent(CtrlTypes.CTRL_C_EVENT, 0u)
        Trace.Verbose("GenerateConsoleCtrlCEvent={0}", generated)

