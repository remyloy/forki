namespace Forki.Job

open System
open System.Runtime.InteropServices

module JobFactory =
    let Create() : Job =
        if RuntimeInformation.IsOSPlatform(OSPlatform.Windows) then
            new Forki.Job.Win32.Job() :> Job
        else
            raise (PlatformNotSupportedException())
