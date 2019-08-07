module Forki.Test.Reactive

open System
open System.Collections.Generic
open System.IO
open System.Threading.Tasks
open Expecto
open Forki
open Forki.Reactive

let shellExecutable = FileInfo(@"C:\Windows\System32\cmd.exe")

[<Tests>]
let tests =
  testList "Reactive Tests" [
    test "Capture standard out " {
      let childProcess = ChildProcess.Start("cmd /C dir /B")
      let stdOut = List<_>()
      let completed = TaskCompletionSource<_>()
      use _ = childProcess.ObserveOut.Subscribe(Action<_>(fun s -> stdOut.Add(s) |> ignore), fun () -> completed.SetResult(Unchecked.defaultof<obj>))
      completed.Task.Wait()
      Expect.containsAll
        stdOut
        [| "Forki.dll"; "Forki.pdb"; "Forki.Test.dll"; "Forki.Test.pdb"; "Forki.Test.deps.json"; "Forki.Test.runtimeconfig.dev.json"; "Forki.Test.runtimeconfig.json" |] 
        "The directory listing should be equal"
    }
  ]
