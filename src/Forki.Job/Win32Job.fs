namespace Forki.Job.Win32

// https://stackoverflow.com/questions/6266820/working-example-of-createjobobject-setinformationjobobject-pinvoke-in-net

open System
open System.Diagnostics
open System.Runtime.InteropServices

[<StructLayout(LayoutKind.Sequential)>]
type internal IO_COUNTERS =
    struct
        val mutable ReadOperationCount: UInt64
        val mutable WriteOperationCount: UInt64 
        val mutable OtherOperationCount:UInt64 
        val mutable ReadTransferCount:UInt64 
        val mutable WriteTransferCount:UInt64 
        val mutable OtherTransferCount:UInt64 
    end

[<StructLayout(LayoutKind.Sequential)>]
type internal JOBOBJECT_BASIC_LIMIT_INFORMATION =
    struct
        val mutable PerProcessUserTimeLimit:Int64 
        val mutable PerJobUserTimeLimit:Int64 
        val mutable LimitFlags:UInt32
        val mutable MinimumWorkingSetSize:UIntPtr
        val mutable MaximumWorkingSetSize:UIntPtr
        val mutable ActiveProcessLimit:UInt32
        val mutable Affinity:UIntPtr 
        val mutable PriorityClass:UInt32 
        val mutable SchedulingClass:UInt32
    end

[<StructLayout(LayoutKind.Sequential)>]
type internal SECURITY_ATTRIBUTES =
    struct 
        val mutable nLength:UInt32 
        val mutable lpSecurityDescriptor:IntPtr
        val mutable bInheritHandle:Int32 
    end

[<StructLayout(LayoutKind.Sequential)>]
type internal JOBOBJECT_EXTENDED_LIMIT_INFORMATION =
    struct 
    val mutable BasicLimitInformation:JOBOBJECT_BASIC_LIMIT_INFORMATION 
    val mutable IoInfo:IO_COUNTERS 
    val mutable ProcessMemoryLimit:UIntPtr 
    val mutable JobMemoryLimit:UIntPtr 
    val mutable PeakProcessMemoryUsed:UIntPtr 
    val mutable PeakJobMemoryUsed:UIntPtr 
    end

type internal JobObjectInfoType =
    | AssociateCompletionPortInformation = 7
    | BasicLimitInformation = 2
    | BasicUIRestrictions = 4
    | EndOfJobTimeInformation = 6
    | ExtendedLimitInformation = 9
    | SecurityLimitInformation = 5
    | GroupInformation = 11

type internal Job() =
    inherit Forki.Job.Job()
    let mutable handle = IntPtr.Zero
    let mutable disposed = false

    [<DllImport("kernel32.dll", CharSet = CharSet.Unicode)>]
    static extern IntPtr CreateJobObject(IntPtr a, string lpName);

    [<DllImport("kernel32.dll")>]
    static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, UInt32 cbJobObjectInfoLength);

    [<DllImport("kernel32.dll", SetLastError = true)>]
    static extern bool AssignProcessToJobObject(IntPtr job, IntPtr proc);

    [<DllImport("kernel32.dll", SetLastError = true)>]
    static extern [<return: MarshalAs(UnmanagedType.Bool)>]bool CloseHandle(IntPtr hObject);
    
    do
        handle <- CreateJobObject(IntPtr.Zero, null)
        let info = new JOBOBJECT_BASIC_LIMIT_INFORMATION(LimitFlags = 0x2000u)
        let extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION(BasicLimitInformation = info)

        let length = Marshal.SizeOf(typeof<JOBOBJECT_EXTENDED_LIMIT_INFORMATION>);
        let extendedInfoPtr = Marshal.AllocHGlobal(length);
        Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

        if not (SetInformationJobObject(handle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, uint32 length)) then
            failwithf "Unable to set information. Error: %d" (Marshal.GetLastWin32Error())
        
    member private this.Dispose(disposing) =
        if not disposed then
            if not disposing then
                this.Close()
                disposed <- true

    member this.Close() =
        CloseHandle(handle) |> ignore
        handle <- IntPtr.Zero

    override this.AddProcess(processHandle: IntPtr) =
        AssignProcessToJobObject(handle, processHandle)

    member this.AddProcess(processId: int) =
        this.AddProcess(Process.GetProcessById(processId).Handle)

    interface IDisposable with
        member this.Dispose() =
            this.Dispose(true)
            GC.SuppressFinalize(this)
