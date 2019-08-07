open System
open System.IO
open Expecto
open System.Reflection

[<EntryPoint>]
let main argv =
  Environment.CurrentDirectory <- FileInfo(Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath).Directory.FullName
  let config = defaultConfig
  runTestsInAssembly config argv
