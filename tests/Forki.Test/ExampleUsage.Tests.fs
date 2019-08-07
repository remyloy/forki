module Forki.Test.ExampleUsage

open System
open System.IO
open Expecto
open Forki

let shellExecutable = FileInfo(@"C:\Windows\System32\cmd.exe")

[<Tests>]
let tests =
  testList "Tests" [
    test "Echo server" {
      let childProcess =
        ChildProcess.Start(
          FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet","dotnet.exe")),
          "Echo.Server.dll log.txt",
          DirectoryInfo(@"..\..\..\Echo.Server\Debug\netcoreapp2.2"))
      childProcess.In.WriteLine("Hello world")
      let stdout = childProcess.Out.ReadLine()
      childProcess.Close()
      let exitCode = childProcess.WaitForExit()
      Expect.equal stdout "Hello world" "Echoed string is wrong"
      Expect.equal exitCode (Some 0) "Exit code is wrong"
    }
    test "Echo client" {
      let childProcess =
        ChildProcess.Start(
          FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "dotnet","dotnet.exe")),
          "Echo.Client.dll",
          DirectoryInfo(@"..\..\..\Echo.Client\Debug\netcoreapp2.2"))
      childProcess.In.WriteLine("Hello")
      childProcess.In.WriteLine("world")
      childProcess.Close()
      let stdout = childProcess.Out.ReadToEnd()
      let exitCode = childProcess.WaitForExit()
      Expect.equal exitCode (Some 0) "Exit code is wrong"
      Expect.equal stdout "Hello\r\nworld\r\n" "Echoed string is wrong"
    }
  ]
