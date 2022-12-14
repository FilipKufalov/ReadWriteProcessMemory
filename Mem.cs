using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Mem
{
    public class Mem
    {
        public static int m_iNumberOfBytesRead = 0;
        public static int m_iNumberOfBytesWritten = 0;
        public static Process m_Process;
        public static IntPtr m_pProcessHandle;
        public static ulong BaseAddress;
        private const ulong PROCESS_VM_OPERATION = 8;
        private const ulong PROCESS_VM_READ = 16;
        private const ulong PROCESS_VM_WRITE = 32;

        public static void Initialize(string ProcessName)
        {
            if ((ulong)Process.GetProcessesByName(ProcessName).Length > 0U)
            {
                //Console.WriteLine((ulong)Process.GetProcessesByName(ProcessName).Length > 0U);
                Mem.m_Process = Process.GetProcessesByName(ProcessName)[0];
                Mem.BaseAddress = (ulong)Process.GetProcessesByName(ProcessName)[0].MainModule.BaseAddress.ToInt64();
                Console.WriteLine("Process " + Mem.m_Process.MainWindowTitle + " successfully finded!");
            }
            else
            {
                Console.WriteLine("Open the application/game first!");
                Environment.Exit(1);
            }
            Mem.m_pProcessHandle = Mem.OpenProcess(56, false, Mem.m_Process.Id);
        }

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

        public static T ReadMemory<T>(ulong Adress) where T : struct
        {
            byte[] numArray = new byte[Marshal.SizeOf(typeof(T))];
            Mem.ReadProcessMemory((int)Mem.m_pProcessHandle, Adress, numArray, numArray.Length, ref Mem.m_iNumberOfBytesRead);
            return Mem.ByteArrayToStructure<T>(numArray);
        }

        public static void WriteMemory<T>(ulong Adress, object Value) where T : struct
        {
            byte[] byteArray = Mem.StructureToByteArray(Value);
            Mem.WriteProcessMemory((int)Mem.m_pProcessHandle, Adress, byteArray, byteArray.Length, out Mem.m_iNumberOfBytesWritten);
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess, ulong lpBaseAddress, byte[] buffer, int size, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        private static extern bool WriteProcessMemory(int hProcess, ulong lpBaseAddress, byte[] buffer, int size, out int lpNumberOfBytesWritten);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, int lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
    }
}
