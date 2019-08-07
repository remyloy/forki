namespace Forki

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Text
open System.Text.RegularExpressions

[<AutoOpen>]
module private Option =
  let (<*>) opt f = Option.map f opt
  let fullname (di: DirectoryInfo) = di.FullName

/// <summary>
/// Provides an abstraction over System.Diagnostics.Process for working with
/// child processes.
/// </summary>
/// <remarks>
/// Is not usable on other platforms than windows at the moment.
/// </remarks>
type ChildProcess(proc: Process) =
  static let sigint = SigInt.Factory.create()

  /// Gets the unique identifier of the associated process.
  member this.ProcessId = 
    proc.Id

  /// Gets the native handle of the associated process.
  member this.Handle =
    proc.Handle

  /// Gets the streamwriter to send data into the child process.
  member this.In =
    proc.StandardInput

  /// Gets the streamreader to receive data from the child process.
  member this.Out =
    proc.StandardOutput

  /// <summary>
  /// Sends SigInt to the child process.
  /// </summary>
  /// <remarks>
  /// Be aware that this works differently than you might expect on Windows.
  /// The signal is propagated through processes sharing the same console.
  /// For example let's assume your using dotnet watch to run some code which
  /// uses SendCtrlC. In that case you will be surprised to notice that the
  /// watcher exits, too.
  /// </remarks>    
  member this.SendCtrlC() =
    Trace.Verbose("Sending Ctrl+C")
    sigint(proc)

  /// <summary>
  /// Close is supposed to be an alternative to SentCtrlC.
  /// Closing the input stream the child process might be handled by the child
  /// process in a way that leads to its termination.
  /// </summary>
  /// <remarks>
  /// For example if your child process uses Console.In.ReadLine() to retrieve
  /// its input data, the ReadLine() method will return null once the parent
  /// process closed the input stream.
  /// See the example implementation of Echo.Server in the repository for how
  /// this can be done.
  /// </remarks>
  member this.Close() =
    this.In.Close()

  /// Waits for the child process to exit.
  /// Returns an int option containing the exit code of the child process,
  /// if it exitted during the given timeout or if no timeout was specified.
  member this.WaitForExit(?timeout) =
    match timeout with
    | Some timeout ->
      Trace.Verbose("Waiting for {1} ms for process {0} to exit", proc.Id, timeout)
      let hasProcessExitted = proc.WaitForExit(timeout)
      if hasProcessExitted then
        Trace.Verbose("Process {0} exitted", proc.Id)
        Some proc.ExitCode
      else
        Trace.Verbose("Waiting for process {0} to exit timed out after {1} ms", proc.Id, timeout)
        None
    | None ->
      Trace.Verbose("Waiting for process {0} to exit", proc.Id)
      proc.WaitForExit()
      Trace.Verbose("Process {0} exitted", proc.Id)
      Some proc.ExitCode

  /// Loosely mimics the behavior of how cmd looks up the executable to execute, given only its name.
  static member SearchPathForExecutable(exe: string, ?envpath: string) =
    let path = 
      let p = defaultArg envpath (Environment.GetEnvironmentVariable("PATH"))
      if p = null then String.Empty else p
    Trace.Verbose("EnviromentVariable PATH={0}", path)
    let paths = HashSet<_>(path.Split(';'))
    let candidates =
      seq {
        for path in paths do
          let fi = FileInfo(Path.Combine(path, exe))
          if fi.Exists then yield fi
      }
      |> Seq.toArray
    Trace.Verbose("Candiates={0}", String.Join(";", candidates))
    match candidates with
    | [| fi |] ->
      fi
    | [||] -> 
      failwithf "No executable %s found in path" exe
    | _ ->
      failwithf "Multiple matching executables %s found in path" exe

  /// Parses the command and it splits it into the executable and the arguments
  /// passed along to the executable. The working directory is either the provided
  /// directory or the directory of the executable or the current directory.
  static member ParseCommand(command: string, ?workingDirectory: DirectoryInfo) =
    let regex = Regex("""^(?:"([^"]+(?="))|([^\s]+))["]{0,1} +(.+)$""")
    let tokens = regex.Match(command)
    let (exe, args) =
      if not tokens.Success then
        failwithf "Could not parse command line %s" command
      else if tokens.Groups.[1].Success && tokens.Groups.[3].Success then
        (tokens.Groups.[1].Value, tokens.Groups.[3].Value)
      else if tokens.Groups.[2].Success && tokens.Groups.[3].Success then
        tokens.Groups.[2].Value, tokens.Groups.[3].Value
      else 
        failwithf "Could not parse command line %s" command
    Trace.Verbose("Parsing intermediate result executable={0} arguments={1}", exe, args)

    let exe =
      if not (exe.EndsWith(".exe")) then exe + ".exe"
      else exe

    if Path.IsPathRooted(exe) then
      let fi = FileInfo(exe)
      let di = defaultArg workingDirectory fi.Directory
      fi, args, di
    else
      let fi = ChildProcess.SearchPathForExecutable exe
      let di = defaultArg workingDirectory (DirectoryInfo Environment.CurrentDirectory)
      fi, args, di

  static member CreateStartInfo(exe: FileInfo, ?args: string, ?workingDirectory: DirectoryInfo) =
    if not exe.Exists then raise (ArgumentException(sprintf "Executable %s does not exist." exe.FullName))
    if workingDirectory.IsSome && not workingDirectory.Value.Exists then raise (ArgumentException(sprintf "WorkingDirectory %s does not exist." workingDirectory.Value.FullName))
    ProcessStartInfo(
      exe.FullName, 
      Arguments = defaultArg args null, 
      WorkingDirectory = defaultArg (workingDirectory <*> fullname) null,
      UseShellExecute = false, 
      CreateNoWindow = false,
      RedirectStandardInput = true,
      RedirectStandardOutput = true,
      RedirectStandardError = true,
      StandardOutputEncoding = Encoding.UTF8,
      StandardErrorEncoding = Encoding.UTF8)

  /// Convenience overload for starting a child process by giving a command line like in a shell.
  static member Start(command: string) =
    let (exe, args, workingDirectory) = ChildProcess.ParseCommand(command)
    Trace.Verbose("Start child process with command={0} {1} in {2}", exe, args, workingDirectory)
    let startInfo = ChildProcess.CreateStartInfo(exe, args)
    let proc = Process.Start(startInfo)
    ChildProcess(proc)

  /// Start a child process by passing in the arguments explicitly.
  static member Start(exe: FileInfo, ?args: string, ?workingDirectory: DirectoryInfo) =
    let args = defaultArg args String.Empty
    let workingDirectory = defaultArg workingDirectory (DirectoryInfo Environment.CurrentDirectory)
    Trace.Verbose("Start child process with command={0} {1} in {2}", exe.FullName, args, workingDirectory.FullName)
    let startInfo = ChildProcess.CreateStartInfo(exe, args, workingDirectory)
    let proc = Process.Start(startInfo)
    ChildProcess(proc)
