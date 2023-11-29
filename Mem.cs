using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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
        /// <summary>
        /// Allocates a block of virtual memory within a target process using the VirtualAllocEx function.
        /// </summary>
        /// <param name="size">The size, in bytes, of the memory block to allocate.</param>
        /// <returns>
        /// An IntPtr representing the base address of the allocated memory block. 
        /// If the allocation fails, the function returns IntPtr.Zero.
        /// </returns>
        /// <remarks>
        /// The VirtualAllocEx function is used to reserve and commit a region of virtual memory 
        /// within the specified process. The allocation type includes both committing and reserving 
        /// memory, and the memory protection allows reading, writing, and executing operations.
        /// Proper error handling is implemented, and an error message is displayed if the allocation fails.
        /// </remarks>
        /// <example>
        /// <code>
        /// IntPtr allocatedMemory = AllocMem(0x100); // Allocates 256 bytes of memory
        /// if (allocatedMemory != IntPtr.Zero)
        /// {
        ///     // Use allocatedMemory for further memory operations in the target process
        /// }
        /// </code>
        /// </example>
        public static IntPtr AllocMem(IntPtr size)
        {
            IntPtr allocAddress = VirtualAllocEx(m_pProcessHandle, IntPtr.Zero, size, AllocationType.Commit | AllocationType.Reserve, MemoryProtection.ExecuteReadWrite);

            if (allocAddress == IntPtr.Zero)
            {
                Console.WriteLine("Memory allocation failed.");
            }

            return allocAddress;
        }
        /// <summary>
        /// Frees a block of virtual memory within a target process using the VirtualFreeEx function.
        /// </summary>
        /// <param name="address">The base address of the region of pages to be freed.</param>
        /// <param name="size">The size of the region of memory to free, in bytes. If set to 0, the entire region is freed.</param>
        /// <returns>
        /// Returns true if the operation succeeds; otherwise, false. 
        /// If the operation fails, the function prints an error message to the console.
        /// </returns>
        /// <remarks>
        /// The VirtualFreeEx function is used to release a region of virtual memory within the specified process.
        /// When freeing memory with FreeType.Release, setting the size to 0 indicates that the entire region
        /// starting from the specified address should be released.
        /// Proper error handling is implemented, and an error message is displayed if the operation fails.
        /// </remarks>
        /// <example>
        /// <code>
        /// bool success = FreeMem(address, 0); // Frees the entire region of memory starting from the specified address
        /// if (success)
        /// {
        ///     // Memory successfully freed
        /// }
        /// </code>
        /// </example>

        public static bool FreeMem(IntPtr address, IntPtr size)
        {
            bool success = VirtualFreeEx(m_pProcessHandle, address, size, FreeType.Release);

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine("VirtualFreeEx failed with error code: " + error + ", Try with size: 0");
            }

            return success;
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

        [DllImport("kernel32.dll")]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr dwSize, AllocationType flAllocationType, MemoryProtection flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr dwSize, FreeType dwFreeType);

        [Flags]
        public enum AllocationType
        {
            Commit = 0x1000,
            Reserve = 0x2000,
            Decommit = 0x4000,
            Release = 0x8000,
            Reset = 0x80000,
            Physical = 0x400000,
            TopDown = 0x100000,
            WriteWatch = 0x200000,
            LargePages = 0x20000000
        }

        [Flags]
        public enum MemoryProtection
        {
            Execute = 0x10,
            ExecuteRead = 0x20,
            ExecuteReadWrite = 0x40,
            ExecuteWriteCopy = 0x80,
            NoAccess = 0x01,
            ReadOnly = 0x02,
            ReadWrite = 0x04,
            WriteCopy = 0x08,
            GuardModifierflag = 0x100,
            NoCacheModifierflag = 0x200,
            WriteCombineModifierflag = 0x400
        }

        public enum FreeType
        {
            Decommit = 0x4000,
            Release = 0x8000
        }
    }
}
