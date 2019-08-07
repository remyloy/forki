open System
open Forki
open System.Threading.Tasks

let readAndSend (childProcess : ChildProcess) =
  Console.CancelKeyPress.Add <| fun args -> args.Cancel <- true
  async {
    let mutable loop = true
    while loop do
      let! input = Console.In.ReadLineAsync() |> Async.AwaitTask
      if input = null then 
        childProcess.Close()
        loop <- false
      else 
        childProcess.In.WriteLine(input)
  }

let waitForResponseAndPrint (childProcess : ChildProcess) =
  async {
    let mutable loop = true
    while loop do
      let! output = childProcess.Out.ReadLineAsync() |> Async.AwaitTask
      if output = null then loop <- false
      else Console.WriteLine(output)
    return childProcess.WaitForExit()
  }

[<EntryPoint>]
let main argv =
    let childProcess = ChildProcess.Start(@"dotnet ..\..\..\Echo.Server\debug\netcoreapp2.2\Echo.Server.dll")
    let tasks =
        [| readAndSend childProcess |> Async.StartAsTask :> Task
         ; waitForResponseAndPrint childProcess |> Async.StartAsTask :> Task
        |]
    let exitCode = childProcess.WaitForExit()
    match exitCode with
    | Some code when code = 0 ->
      Task.WaitAll(tasks)
      code
    | Some code ->
      code
    | None ->
      failwith "Cannot happen"
