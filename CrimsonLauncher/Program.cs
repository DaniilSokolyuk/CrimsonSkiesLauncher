using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace CrimsonLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
#if !DEBUG
            var handle = GetConsoleWindow();
            ShowWindow(handle, 0);
#endif

            string targetExe = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "crimson.exe");

#if DEBUG
            targetExe = "C:\\Program Files (x86)\\Crimson Skies\\crimson.exe";
#endif

            // Will contain the name of the IPC server channel
            string channelName = null;

            // Create the IPC server
            EasyHook.RemoteHooking.IpcCreateServer<InjectPayload.ServerInterface>(ref channelName, System.Runtime.Remoting.WellKnownObjectMode.Singleton);

            // Get the full path to the assembly we want to inject into the target process
            string injectionLibrary = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "InjectPayload.dll");

            EasyHook.RemoteHooking.CreateAndInject(
                targetExe,          // executable to run
                "",                 // command line arguments for target
                0,                  // additional process creation flags to pass to CreateProcess 
                EasyHook.InjectionOptions.DoNotRequireStrongName, // allow injectionLibrary to be unsigned
                injectionLibrary,   // 32-bit library to inject (if target is 32-bit) 
                injectionLibrary,   // 64-bit library to inject (if target is 64-bit)
                out var targetPID,      // retrieve the newly created process ID
                channelName         // the parameters to pass into injected library
                                    // ...
            );

            var process = Process.GetProcessById(targetPID);
            process.EnableRaisingEvents = true;
            process.Exited += (sender, eventArgs) => Environment.Exit(0);

            Console.WriteLine("<Press any key to exit>");
            Console.ReadKey();
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}