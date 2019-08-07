namespace Forki

open System
open System.Diagnostics

type internal Trace() =
    static let trace = TraceSource("Forki", SourceLevels.Verbose)
    static member Verbose(msg, [<ParamArray>] args) = trace.TraceEvent(TraceEventType.Verbose, 0, msg, args)
