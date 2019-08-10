# Easily manage and communicate with child processes

Named forki cause of the fork() system call in unix and its workalikes.
Standard .net has the low-level types and operations to do this,
but I've reimplemented this kind of abstractions so often in different projects.

Example usage:
```
// Invoke the shell processor to execute the dir command.
let childProcess = ChildProcess.Start("cmd.exe /C dir / B")
// Read everything from the output stream.
let stdout = childProcess.Out.ReadToEnd()
// Wait for the process to exit.
let exitCode = childProcess.WaitForExit()
```

# Logging
Logging is implemented using the System.Diagnostics.TraceSource.
The name of the trace source is Forki.

# Restrictions
The implementation makes assumptions based on my Windows experience.
So I assume that this will cause problems when using it on Linux or macOS.

Tested on Windows 10 (1803) with dotnet core sdk 2.2.401.
Untested on Linux and macOS.

# Package structure
* Forki: The core package staying as low-level as possible while still proving some sensible abstraction
* Forki.Reactive: Provides extension for using the child process in a RX environment
