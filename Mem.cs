using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mem
{
    public class Mem
    {
        public static int m_iNumberOfBytesRead = 0;
        public static int m_iNumberOfBytesWritten = 0;
        public static Process m_Process;
        public static int m_pProcessHandle;
        /// <summary>
        /// Base address of the main module of the target process.
        /// </summary>
        public static ulong BaseAddress;
        private const ulong PROCESS_VM_OPERATION = 8;
        private const ulong PROCESS_VM_READ = 16;
        private const ulong PROCESS_VM_WRITE = 32;

        // For process resolution (WorldToScreen)
        public static int windowWidth = 0;
        public static int windowHeight = 0;

        /// <summary>
        /// Initializes the Mem class for a specified process.
        /// </summary>
        /// <param name="ProcessName">Name of the target process.</param>
        public static void Initialize(string ProcessName)
        {
            if ((ulong)Process.GetProcessesByName(ProcessName).Length > 0U)
            {
                //Console.WriteLine((ulong)Process.GetProcessesByName(ProcessName).Length > 0U);
                Mem.m_Process = Process.GetProcessesByName(ProcessName)[0];
                Mem.BaseAddress = (ulong)Process.GetProcessesByName(ProcessName)[0].MainModule.BaseAddress.ToInt32();
                Console.WriteLine("Process " + Mem.m_Process.MainWindowTitle + " successfully finded!");
            }
            else
            {
                Console.WriteLine("Open the application/game first!");
                Environment.Exit(1);
            }
            Mem.m_pProcessHandle = Mem.OpenProcess(56, false, Mem.m_Process.Id);
        }
        /// <summary>
        /// Initializes the Mem class for a specified process using window handle.
        /// </summary>
        /// <param name="ProcessName">Name of the target process.</param>
        public static void InitWindow(string ProcessName)
        {
            uint pid;
            IntPtr hwnd = Mem.FindWindow(ProcessName, 0);
            GetWindowThreadProcessId(hwnd, out pid); //Get the process id and place it in pid
            Mem.m_pProcessHandle = OpenProcess(16, false, (int)pid); //Get permission to read
            if (hwnd == IntPtr.Zero)
            {
                Console.WriteLine("Open the application/game first!");
                Environment.Exit(1);
            }
            if (m_pProcessHandle == IntPtr.Zero)
            {
                Console.WriteLine("Cannot obtain process.");
                Thread.Sleep(2000);
                Environment.Exit(1);
            }
        }
        /// <summary>
        /// Converts a byte array to a structure of the specified type.
        /// </summary>
        /// <typeparam name="T">Type of the structure.</typeparam>
        /// <param name="bytes">Byte array to convert.</param>
        /// <returns>The converted structure.</returns>
        private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle gcHandle = GCHandle.Alloc((object)bytes, GCHandleType.Pinned);
            try
            {
                return (T)Marshal.PtrToStructure(gcHandle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                gcHandle.Free();
            }
        }
        /// <summary>
        /// Converts a sturcture to a byte array
        /// </summary>
        /// <typeparam name="T">Type of the structure.</typeparam>
        /// <param name="bytes">Byte array to convert.</param>
        /// <returns>The converted byte array.</returns>
        private static byte[] StructureToByteArray(object obj)
        {
            int length = Marshal.SizeOf(obj);
            byte[] destination = new byte[length];
            IntPtr num = Marshal.AllocHGlobal(length);
            Marshal.StructureToPtr(obj, num, true);
            Marshal.Copy(num, destination, 0, length);
            Marshal.FreeHGlobal(num);
            return destination;
        }
        /// <summary>
        /// Retrieves the size of the target process window.
        /// </summary>
        /// <param name="ProcessName">Name of the target process.</param>
        /// <returns>True if successful (Rewrite windowWidth and windowHeight variables); otherwise, false.</returns>
        public static bool GetWindowSize(string ProcessName)
        {
            Process[] processes = Process.GetProcessesByName(ProcessName);

            if (processes.Length > 0)
            {
                Process Process = processes[0];
                IntPtr mainWindowHandle = Process.MainWindowHandle;
                RECT windowRect;

                if (GetWindowRect(mainWindowHandle, out windowRect))
                {
                    windowWidth = windowRect.Right - windowRect.Left;
                    windowHeight = windowRect.Bottom - windowRect.Top;
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to retrieve {ProcessName}.");
                    return false;
                }
            }
            else
            {
                Console.WriteLine($"{ProcessName} is not running.");
                return false;
            }
        }

        /// <summary>
        /// Reads a value of the specified structure type from the memory of the target process at the given address.
        /// </summary>
        /// <typeparam name="T">Type of the structure to read.</typeparam>
        /// <param name="Address">Memory address to read from.</param>
        /// <returns>The read structure value.</returns>
        public static T ReadMemory<T>(nint Address) where T : struct
        {
            byte[] buffer = new byte[Marshal.SizeOf(typeof(T))];
            Mem.ReadProcessMemory(Mem.m_pProcessHandle, Address, buffer, buffer.Length, ref Mem.m_iNumberOfBytesRead);
            return Mem.ByteArrayToStructure<T>(buffer);
        }

        /// <summary>
        /// Writes the specified structure value to the memory of the target process at the given address.
        /// </summary>
        /// <typeparam name="T">Type of the structure to write.</typeparam>
        /// <param name="Address">Memory address to write to.</param>
        /// <param name="Value">Structure value to write.</param>
        public static void WriteMemory<T>(int Address, object Value) where T : struct
        {
            byte[] byteArray = Mem.StructureToByteArray(Value);
            Mem.WriteProcessMemory(Mem.m_pProcessHandle, Address, byteArray, byteArray.Length, out Mem.m_iNumberOfBytesWritten);
        }


        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("kernel32.dll")]
        private static extern int OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess, nint lpBaseAddress, byte[] buffer, int size, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(int hProcess, int lpBaseAddress, byte[] buffer, int size, out int lpNumberOfBytesWritten);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, int lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        
    }
}
