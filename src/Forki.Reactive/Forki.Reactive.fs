namespace Forki.Reactive

open Forki
open System
open System.Reactive.Disposables
open System.Reactive.Linq

[<AutoOpen>]
module Extensions =
  type ChildProcess with
    member this.ObserveOut =
      Observable.Create(fun (obs: IObserver<string>) ->
        async {
          let mutable loop = true
          while loop do
            let! line = this.Out.ReadLineAsync() |> Async.AwaitTask
            if line = null then
              loop <- false
              obs.OnCompleted()
            else
              obs.OnNext(line)
        }
        |> Async.Start
        Disposable.Empty)
