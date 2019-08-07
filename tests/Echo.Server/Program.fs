open System
open System.Diagnostics
open System.IO
open System.Reflection

let trace = TraceSource("Echo.Server", SourceLevels.Verbose)

let configureTrace fileOpt =
  let baseDir = FileInfo(Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath).Directory.FullName
  let logfile = defaultArg fileOpt (Path.Combine(baseDir, sprintf "log-%d.txt" (Process.GetCurrentProcess().Id)))
  let stream = new FileStream(logfile, FileMode.Create, FileAccess.Write, FileShare.Write)
  let listener = new TextWriterTraceListener(stream)
  listener.TraceOutputOptions <- TraceOptions.DateTime
  trace.Listeners.Add(listener) |> ignore

let suppressDefaultSigIntHandler () =
  Console.CancelKeyPress.Add <| fun args -> 
    trace.TraceEvent(TraceEventType.Information, 0, "{0} received", args.SpecialKey)
    args.Cancel <- true

let loop () =
  let mutable loop = true

  while loop do        
    let raw = Console.In.Read()
    trace.TraceEvent(TraceEventType.Verbose, 0, "RawInput={0}", raw)

    if raw >= 0 then
      try
        let ch = Convert.ToChar(raw)
        Console.Out.Write(ch)
      with :? OverflowException as ex ->
        trace.TraceEvent(TraceEventType.Verbose, 0, ex.ToString())
        loop <- false
    else 
      trace.TraceEvent(TraceEventType.Verbose, 0, "End of stream reached")
      loop <- false

[<EntryPoint>]
let main argv =
  configureTrace (argv |> Array.tryItem 0)
  suppressDefaultSigIntHandler ()
  trace.TraceEvent(TraceEventType.Information, 0, "Server started")
  loop ()
  trace.TraceEvent(TraceEventType.Information, 0, "Server stopped")
  trace.Flush()
  0
