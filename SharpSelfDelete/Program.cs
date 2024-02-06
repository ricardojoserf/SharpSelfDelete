using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpSelfDelete
{
    internal unsafe class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFileW(
            [MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            uint lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            uint hTemplateFile
        );

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Int32 WSAGetLastError();

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern int SetFileInformationByHandle(
            IntPtr hFile,
            FileInformationClass FileInformationClass,
            IntPtr FileInformation,
            Int32 dwBufferSize
        );

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(
            IntPtr handle
        );

        [DllImport("kernel32.dll", SetLastError = true)]
        [PreserveSig]
        public static extern uint GetModuleFileName
        (
            [In] IntPtr hModule,
            [Out] StringBuilder lpFilename,
            [In][MarshalAs(UnmanagedType.U4)] int nSize
        );

        enum FileInformationClass : int
        {
            FileBasicInfo = 0,
            FileStandardInfo = 1,
            FileNameInfo = 2,
            FileRenameInfo = 3,
            FileDispositionInfo = 4,
            FileAllocationInfo = 5,
            FileEndOfFileInfo = 6,
            FileStreamInfo = 7,
            FileCompressionInfo = 8,
            FileAttributeTagInfo = 9,
            FileIdBothDirectoryInfo = 10, // 0xA
            FileIdBothDirectoryRestartInfo = 11, // 0xB
            FileIoPriorityHintInfo = 12, // 0xC
            FileRemoteProtocolInfo = 13, // 0xD
            FileFullDirectoryInfo = 14, // 0xE
            FileFullDirectoryRestartInfo = 15, // 0xF
            FileStorageInfo = 16, // 0x10
            FileAlignmentInfo = 17, // 0x11
            FileIdInfo = 18, // 0x12
            FileIdExtdDirectoryInfo = 19, // 0x13
            FileIdExtdDirectoryRestartInfo = 20, // 0x14
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct filerenameinfo_struct
        {
            public bool ReplaceIfExists;
            public IntPtr RootDirectory;
            public uint FileNameLength;
            public fixed byte filename[255];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct filedispositioninfo_struct
        {
            public bool DeleteFile;
        }

        const uint DELETE = (uint)0x00010000L;
        const uint SYNCHRONIZE = (uint)0x00100000L;
        const uint FILE_SHARE_READ = 0x00000001;
        const uint OPEN_EXISTING = 3;
        const int MAX_PATH = 256;

        static void Main(string[] args)
        {
            StringBuilder fname = new System.Text.StringBuilder(MAX_PATH);
            GetModuleFileName(IntPtr.Zero, fname, MAX_PATH);
            string filename = fname.ToString();
            string new_name = ":Random";
            Console.WriteLine("[+] File Name: \t\t\t\t" + filename);
            Console.WriteLine("[+] New Alternate Data Stream: \t\t" + new_name);

            // Renaming
            Console.WriteLine("[+] RENAMING FILE...");

            // Handle to current file
            IntPtr hFile = CreateFileW(filename, DELETE | SYNCHRONIZE, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
            Console.WriteLine("[+] CreateFileW File handle: \t\t" + hFile);
            int last_error = WSAGetLastError();
            if (last_error != 0 || hFile == IntPtr.Zero) {
                Console.WriteLine("[-] Error calling CreateFileW");
                System.Environment.Exit(0);
            }

            // Creating FILE_RENAME_INFO struct
            filerenameinfo_struct fri = new filerenameinfo_struct();
            fri.ReplaceIfExists = true;
            fri.RootDirectory = IntPtr.Zero;
            uint FileNameLength = (uint)(new_name.Length * 2);
            fri.FileNameLength = FileNameLength;
            int size = Marshal.SizeOf(typeof(filerenameinfo_struct)) + (new_name.Length + 1) * 2;

            IntPtr fri_addr = IntPtr.Zero;

            unsafe
            {
                // Get Address of FILE_RENAME_INFO struct
                filerenameinfo_struct* pfri = &fri;
                fri_addr = (IntPtr)pfri;
                Console.WriteLine("[+] FILE_RENAME_INFO struct address: \t0x" + fri_addr.ToString("x"));

                // Copy new file name (bytes) to filename member in FILE_RENAME_INFO struct
                byte* p = fri.filename;
                byte[] filename_arr = Encoding.Unicode.GetBytes(new_name);
                foreach (byte b in filename_arr)
                {
                    *p = b;
                    p += 1;
                }
            }
            // Rename file calling SetFileInformationByHandle
            int sfibh_res = SetFileInformationByHandle(hFile, FileInformationClass.FileRenameInfo, fri_addr, size);
            Console.WriteLine("[+] SetFileInformationByHandle result:\t" + sfibh_res);
            last_error = WSAGetLastError();
            if (sfibh_res == 0)
            {
                Console.WriteLine("[-] Error calling SetFileInformationByHandle");
                System.Environment.Exit(0);
            }

            // Close handle to finally rename file
            bool ch_res = CloseHandle(hFile);
            Console.WriteLine("[+] Close Handle Result:\t\t" + ch_res);

            // Deleting
            Console.WriteLine("[+] DELETING FILE...");

            // Handle to current file
            IntPtr hFile2 = CreateFileW(filename, DELETE | SYNCHRONIZE, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
            last_error = WSAGetLastError();
            Console.WriteLine("[+] CreateFileW File handle: \t\t" + hFile2);
            if (last_error != 0 || hFile == IntPtr.Zero)
            {
                Console.WriteLine("[-] Error calling CreateFileW");
                System.Environment.Exit(0);
            }

            // Creating FILE_DISPOSITION_INFO struct
            filedispositioninfo_struct fdi = new filedispositioninfo_struct();
            fdi.DeleteFile = true;
            IntPtr fdi_addr = IntPtr.Zero;
            int size_fdi = Marshal.SizeOf(typeof(filedispositioninfo_struct));

            unsafe
            {
                // Get Address of FILE_DISPOSITION_INFO struct
                filedispositioninfo_struct* pfdi = &fdi;
                fdi_addr = (IntPtr)pfdi;
                Console.WriteLine("[+] FILE_DISPOSITION_INFO struct addr: \t0x" + fdi_addr.ToString("x"));
            }

            // Rename file calling SetFileInformationByHandle
            int sfibh_res2 = SetFileInformationByHandle(hFile2, FileInformationClass.FileDispositionInfo, fdi_addr, size_fdi);
            Console.WriteLine("[+] SetFileInformationByHandle result:\t" + sfibh_res2);
            last_error = WSAGetLastError();
            if (sfibh_res == 0 || last_error != 0)
            {
                Console.WriteLine("[-] Error calling SetFileInformationByHandle");
                System.Environment.Exit(0);
            }

            // Close handle to finally delete file
            bool ch_res2 = CloseHandle(hFile2);
            Console.WriteLine("[+] Close Handle result:\t\t" + ch_res2);
        }
    }
}
