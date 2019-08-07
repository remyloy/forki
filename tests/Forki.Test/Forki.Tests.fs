module Forki.Test.Core

open System
open System.IO
open Expecto
open Forki

let shellExecutable = FileInfo(@"C:\Windows\System32\cmd.exe")

[<Tests>]
let tests =
  testList "Tests" [
    test "Basic usage" {
      let childProcess = ChildProcess.Start("cmd /C dir /B")
      let stdout = childProcess.Out.ReadToEnd()
      let lines = stdout.Split([| Environment.NewLine |], StringSplitOptions.None)
      Expect.containsAll
        lines
        [| "Forki.dll"; "Forki.pdb"; "Forki.Test.dll"; "Forki.Test.pdb"; "Forki.Test.deps.json"; "Forki.Test.runtimeconfig.dev.json"; "Forki.Test.runtimeconfig.json" |]
        "The directory listing should be equal"
    }
    test "Explict args" {
      let childProcess = ChildProcess.Start(shellExecutable, "/C dir /B")
      let stdout = childProcess.Out.ReadToEnd()
      let lines = stdout.Split([| Environment.NewLine |], StringSplitOptions.None)
      
      Expect.containsAll
        lines
        [| "Forki.dll"; "Forki.pdb"; "Forki.Test.dll"; "Forki.Test.pdb"; "Forki.Test.deps.json"; "Forki.Test.runtimeconfig.dev.json"; "Forki.Test.runtimeconfig.json" |]
        "The directory listing should be equal"
    }
    test "Given invalid executable throws exception" {
      Expect.throwsT<ArgumentException>(fun () -> ChildProcess.Start(FileInfo("ThisIsNotARealFile")) |> ignore) ""
    }
    test "Given invalid workingDirectory throws exception" {
      Expect.throwsT<ArgumentException>(fun () -> ChildProcess.Start(shellExecutable, workingDirectory = DirectoryInfo("ThisIsNotARealDirectory")) |> ignore) ""
    }
  ]