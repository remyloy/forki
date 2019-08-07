namespace Forki.Job

open System
open Forki

[<AbstractClass>]
type Job() =
    abstract member AddProcess: processHandle: IntPtr -> bool

[<AutoOpen>]
module JobExtensions =
    type ChildProcess with
        static member StartJob(command: string, job: Job) =
            Trace.Verbose("Start child process as job with command={0}", command)
            let proc = ChildProcess.Start(command)
            job.AddProcess(proc.Handle) |> ignore
            proc
