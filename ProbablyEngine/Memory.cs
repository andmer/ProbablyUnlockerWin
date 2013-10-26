using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace MemoryControl
{
    public class MemC
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer, uint nSize, out int lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);
        [DllImport("kernel32.dll")]
        public static extern int ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, uint size, out IntPtr lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        public static extern unsafe bool VirtualProtect(IntPtr lpAddress, uint dwSize, uint flNewProtect, uint* lpflOldProtect);
        [DllImport("kernel32")]
        public static extern UInt32 VirtualAlloc(UInt32 lpStartAddr, UInt32 size, UInt32 flAllocationType, UInt32 flProtect);
        [DllImport("kernel32")]
        public static extern bool VirtualFree(IntPtr lpAddress, UInt32 dwSize, UInt32 dwFreeType);
        [DllImport("kernel32")]
        public static extern IntPtr CreateThread(UInt32 lpThreadAttributes, UInt32 dwStackSize, UInt32 lpStartAddress, IntPtr param, UInt32 dwCreationFlags, ref UInt32 lpThreadId);
        [DllImport("kernel32")]
        public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);
        [DllImport("kernel32")]
        public static extern IntPtr GetModuleHandle(string moduleName);
        [DllImport("kernel32")]
        public static extern UInt32 GetProcAddress(IntPtr hModule, string procName);
        [DllImport("kernel32")]
        public static extern UInt32 LoadLibrary(string lpFileName);
        [DllImport("kernel32")]
        public static extern UInt32 GetLastError();
        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESSOR_INFO
        {
            public UInt32 dwMax;
            public UInt32 id0;
            public UInt32 id1;
            public UInt32 id2;

            public UInt32 dwStandard;
            public UInt32 dwFeature;

            // If AMD
            public UInt32 dwExt;
        }

        public enum AccessProcessTypes
        {
            PROCESS_CREATE_PROCESS = 0x80,
            PROCESS_CREATE_THREAD = 2,
            PROCESS_DUP_HANDLE = 0x40,
            PROCESS_QUERY_INFORMATION = 0x400,
            PROCESS_SET_INFORMATION = 0x200,
            PROCESS_SET_QUOTA = 0x100,
            PROCESS_SET_SESSIONID = 4,
            PROCESS_TERMINATE = 1,
            PROCESS_VM_OPERATION = 8,
            PROCESS_VM_READ = 0x10,
            PROCESS_VM_WRITE = 0x20
        }

        public enum VirtualProtectAccess
        {
            PAGE_EXECUTE = 0x10,
            PAGE_EXECUTE_READ = 0x20,
            PAGE_EXECUTE_READWRITE = 0x40,
            PAGE_EXECUTE_WRITECOPY = 0x80,
            PAGE_NOACCESS = 0x01,
            PAGE_READONLY = 0x02,
            PAGE_READWRITE = 0x04,
            PAGE_WRITECOPY = 0x08,
            PAGE_GUARD = 0x100,
            PAGE_NOCACHE = 0x200,
            PAGE_WRITECOMBINE = 0x400
        }

        public enum VirtualProtectSize
        {
            INT = sizeof(int),
            FLOAT = sizeof(float),
            DOUBLE = sizeof(double),
            CHAR = sizeof(char)
        }

        public static IntPtr cProcessHandle;

        public static void cOpenProcess(string ProcessX)
        {
            var ApplicationXYZ = Process.GetProcessesByName(ProcessX)[0];
            AccessProcessTypes toAccess = AccessProcessTypes.PROCESS_VM_WRITE | AccessProcessTypes.PROCESS_VM_READ |
                                          AccessProcessTypes.PROCESS_VM_OPERATION;
            cProcessHandle = OpenProcess((uint)toAccess, 1, (uint)ApplicationXYZ.Id);
        }

        public static void cOpenProcessId(int ProcessX)
        {
            var ApplicationXYZ = Process.GetProcessById(ProcessX);
            AccessProcessTypes toAccess = AccessProcessTypes.PROCESS_VM_WRITE | AccessProcessTypes.PROCESS_VM_READ |
                                          AccessProcessTypes.PROCESS_VM_OPERATION;
            cProcessHandle = OpenProcess((uint)toAccess, 1, (uint)ApplicationXYZ.Id);
        }

        public static void cCloseProcess()
        {
            try
            {
                CloseHandle(cProcessHandle);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static int getProcessID(string AppX)
        {
            var AppToID = Process.GetProcessesByName(AppX)[0];
            int ID = AppToID.Id;
            return ID;
        }

        private static void WriteThis(IntPtr Address, byte[] XBytesToWrite, int nSizeToWrite)
        {
            int writtenBytes;
            WriteProcessMemory(cProcessHandle, (uint)Address, XBytesToWrite, (uint)nSizeToWrite, out writtenBytes);
        }

        public static void WriteXNOP(int desiredAddress, int noOfNOPsToWrite)
        {
            byte aNOP = 0x90;
            List<byte> nopList = new List<byte>();
            for (int i = 0; i < noOfNOPsToWrite; i++)
                nopList.Add(aNOP);
            byte[] nopBuffer = nopList.ToArray();
            WriteThis((IntPtr)desiredAddress, nopBuffer, noOfNOPsToWrite);
        }

        public static void WriteXInt(int desiredAddrsss, int valToWrite)
        {
            byte[] valueToWrite = BitConverter.GetBytes(valToWrite);
            WriteThis((IntPtr)desiredAddrsss, valueToWrite, valueToWrite.Length);
        }

        public static void WriteXFloat(int desiredAddrsss, float valToWrite)
        {
            byte[] valueToWrite = BitConverter.GetBytes(valToWrite);
            WriteThis((IntPtr)desiredAddrsss, valueToWrite, valueToWrite.Length);
        }

        public static void WriteXDouble(int desiredAddrsss, double valToWrite)
        {
            byte[] valueToWrite = BitConverter.GetBytes(valToWrite);
            WriteThis((IntPtr)desiredAddrsss, valueToWrite, valueToWrite.Length);
        }

        public static void WriteXString(int desiredAddrsss, string valToWrite)
        {
            byte[] valueToWrite = Encoding.ASCII.GetBytes(valToWrite);
            WriteThis((IntPtr)desiredAddrsss, valueToWrite, valueToWrite.Length);
        }

        public static unsafe void WriteXBytes(int desiredAddress, byte[] bytesToWrite)
        {
            uint protection;
            VirtualProtect((IntPtr)desiredAddress, (uint)bytesToWrite.Length,  (uint)0x40, &protection);
            WriteThis((IntPtr)desiredAddress, bytesToWrite, bytesToWrite.Length);
            VirtualProtect((IntPtr)desiredAddress, (uint)bytesToWrite.Length, protection, &protection);
        }

        public static byte[] readXBytes(int desiredAddress, int noOfBytesToRead)
        {
            byte[] buffer = new byte[noOfBytesToRead];
            IntPtr noOfBytesRead;
            ReadProcessMemory(cProcessHandle, (IntPtr)desiredAddress, buffer, (uint)noOfBytesToRead, out noOfBytesRead);
            return buffer;
        }

        public static int readXInt(int desiredAddress)
        {
            byte[] buffer = new byte[0xFF];
            IntPtr noOfBytesRead;
            ReadProcessMemory(cProcessHandle, (IntPtr)desiredAddress, buffer, (uint)4, out noOfBytesRead);
            return BitConverter.ToInt32(buffer, 0);
        }

        public static float readXFloat(int desiredAddress)
        {
            byte[] buffer = new byte[0xFF];
            IntPtr noOfBytesRead;
            ReadProcessMemory(cProcessHandle, (IntPtr)desiredAddress, buffer, (uint)4, out noOfBytesRead);
            return BitConverter.ToSingle(buffer, 0);
        }

        public static double readXDouble(int desiredAddress)
        {
            byte[] buffer = new byte[0xFF];
            IntPtr noOfBytesRead;
            ReadProcessMemory(cProcessHandle, (IntPtr)desiredAddress, buffer, (uint)4, out noOfBytesRead);
            return BitConverter.ToDouble(buffer, 0);
        }

        public static string readXString(int desiredAddress, int sizeOfString)
        {
            byte[] buffer = new byte[sizeOfString];
            IntPtr noOfBytesRead;
            ReadProcessMemory(cProcessHandle, (IntPtr)desiredAddress, buffer, (uint)sizeOfString, out noOfBytesRead);
            return buffer.ToString();
        }

    }
}