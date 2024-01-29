using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpSelfDelete
{
    internal unsafe class Program
    {
        /*
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr CreateFileA(
            string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            uint lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            uint hTemplateFile
        );
        */

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

        /*
        private enum FileInfoByHandleClass : uint
        {
            FileBasicInfo = 0x0,
            FileStandardInfo = 0x1,
            FileNameInfo = 0x2,
            FileRenameInfo = 0x3,
            FileDispositionInfo = 0x4,
            FileAllocationInfo = 0x5,
            FileEndOfFileInfo = 0x6,
            FileStreamInfo = 0x7,
            FileCompressionInfo = 0x8,
            FileAttributeTagInfo = 0x9,
            FileIdBothDirectoryInfo = 0xa,
            FileIdBothDirectoryRestartInfo = 0xb,
            FileRemoteProtocolInfo = 0xd,
            FileFullDirectoryInfo = 0xe,
            FileFullDirectoryRestartInfo = 0xf,
            FileStorageInfo = 0x10,
            FileAlignmentInfo = 0x11,
            FileIdInfo = 0x12,
            FileIdExtdDirectoryInfo = 0x13,
            FileIdExtdDirectoryRestartInfo = 0x14,
            FileDispositionInfoEx = 0x15,
            FileRenameInfoEx = 0x16,
        }*/

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

        [DllImport("ws2_32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern Int32 WSAGetLastError();

        /*
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int SetFileInformationByHandle(
            int hFile,
            uint FileInformationClass,
            IntPtr lpFileInformation,
            int dwBufferSize
        );*/

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern int SetFileInformationByHandle(
            IntPtr hFile,
            FileInformationClass FileInformationClass, 
            // ref FILE_RENAME_INFO FileInformation,
            IntPtr FileInformation,
            Int32 dwBufferSize
        );

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(
            IntPtr handle
        );


        const uint DELETE = (uint)0x00010000L;
        const uint SYNCHRONIZE = (uint)0x00100000L;
        const uint FILE_SHARE_READ = 0x00000001;
        const uint OPEN_EXISTING = 3;
        // const uint FileRenameInfo = (uint)FileInfoByHandleClass.FileRenameInfo;

        //[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]

        [StructLayout(LayoutKind.Sequential)]
        public struct filerenameinfo_struct
        {
            /*
            public bool ReplaceIfExists;
            public IntPtr RootDirectory;
            public uint FileNameLength;
            */
            public bool ReplaceIfExists;
            public IntPtr RootDirectory;
            public uint FileNameLength;
            // [MarshalAs(UnmanagedType.LPWStr)] public string filename;
            // [FieldOffset(16)][MarshalAs(UnmanagedType.LPWStr)] public string filename;
            public fixed byte filename[255];
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct filedispositioninfo_struct
        {
            public bool DeleteFile;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [PreserveSig]
        public static extern uint GetModuleFileName
        (
            [In] IntPtr hModule,
            [Out] StringBuilder lpFilename,
            [In][MarshalAs(UnmanagedType.U4)] int nSize
        );


        static void Main(string[] args)
        {
            // IntPtr fname = IntPtr.Zero;
            StringBuilder fname = new System.Text.StringBuilder(256);
            GetModuleFileName(IntPtr.Zero, fname, 256*2);
            Console.WriteLine(fname);
            string filename = fname.ToString();
            Console.WriteLine(filename);
            // System.Environment.Exit(0);
            // string filename = "C:\\Users\\ricardo\\Desktop\\SharpSelfDelete\\SharpSelfDelete\\bin\\x64\\Release\\SharpSelfDelete.exe::$DATA";
            string new_name = ":Maldev";
            // string new_name = "C:\\Users\\ricardo\\Desktop\\test2.txt:ADS_name2:$DATA";

            IntPtr hFile = CreateFileW(filename, DELETE | SYNCHRONIZE, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);           
            int last_error = WSAGetLastError();
            Console.WriteLine("File handle: \t" + hFile);
            Console.WriteLine("GetLastError: \t" + last_error);
            if (last_error != 0 || hFile == IntPtr.Zero) {
                Console.WriteLine("[-] Error calling CreateFileW");
                System.Environment.Exit(0);
            }

            filerenameinfo_struct fri = new filerenameinfo_struct();
            fri.ReplaceIfExists = true;
            fri.RootDirectory = IntPtr.Zero;
            uint FileNameLength = (uint)(new_name.Length * 2);
            fri.FileNameLength = FileNameLength;
            // fri.filename = new_name;
            //int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(FILE_RENAME_INFO)) // == 24
            // Console.WriteLine("FileNameLength:\t" + FileNameLength);
            int size = 24 + (new_name.Length + 1) * 2;

            unsafe
            {
                filerenameinfo_struct* ptr = &fri;
                IntPtr raw = (IntPtr)ptr;
                Console.WriteLine("Address: \t0x" + raw.ToString("x"));
                Console.WriteLine("Size: \t\t" + size);
                
                byte* p = fri.filename;
                byte[] filename_arr = Encoding.Unicode.GetBytes(new_name);
                foreach (byte b in filename_arr)
                {
                    *p = b;
                    p += 1;
                }

                // Console.ReadKey();
                int sfibh_res = SetFileInformationByHandle(hFile, FileInformationClass.FileRenameInfo, raw, size);
                Console.WriteLine("SetFileInformationByHandle result:\t" + sfibh_res);
                last_error = WSAGetLastError();
                Console.WriteLine("GetLastError: \t"+ last_error); // https://learn.microsoft.com/en-us/windows/win32/debug/system-error-codes--0-499-
                if (sfibh_res == 0) {
                    Console.WriteLine("[-] Error calling SetFileInformationByHandle");
                    //System.Environment.Exit(0);
                }
            }
            bool ch_res = CloseHandle(hFile);
            Console.WriteLine("Close Handle result:\t" + ch_res);

            // Delete
            Console.WriteLine("\nDeleting file...");
            IntPtr hFile2 = CreateFileW(filename, DELETE | SYNCHRONIZE, FILE_SHARE_READ, 0, OPEN_EXISTING, 0, 0);
            last_error = WSAGetLastError();
            Console.WriteLine("File handle: \t" + hFile2);
            Console.WriteLine("GetLastError: \t" + last_error);
            if (last_error != 0 || hFile == IntPtr.Zero)
            {
                Console.WriteLine("[-] Error calling CreateFileW");
                System.Environment.Exit(0);
            }

            filedispositioninfo_struct fdi = new filedispositioninfo_struct();
            fdi.DeleteFile = true;

            unsafe
            {
                filedispositioninfo_struct* pfdi = &fdi;
                IntPtr addr_fdi = (IntPtr)pfdi;
                int size_fdi = System.Runtime.InteropServices.Marshal.SizeOf(typeof(filedispositioninfo_struct));
                //Console.WriteLine("size_fdi " + size_fdi);

                // Console.ReadKey();
                int sfibh_res = SetFileInformationByHandle(hFile2, FileInformationClass.FileDispositionInfo, addr_fdi, size_fdi);
                Console.WriteLine("SetFileInformationByHandle result:\t" + sfibh_res);

                last_error = WSAGetLastError();
                Console.WriteLine("GetLastError: \t" + last_error);
                
                if (sfibh_res == 0 || last_error != 0)
                {
                    Console.WriteLine("[-] Error calling SetFileInformationByHandle");
                    System.Environment.Exit(0);
                }
            }
            bool ch_res2 = CloseHandle(hFile2);
            Console.WriteLine("Close Handle result:\t" + ch_res2);
        }
    }
}
