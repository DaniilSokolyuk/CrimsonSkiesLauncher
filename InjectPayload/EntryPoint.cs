using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace InjectPayload
{
    public class InjectionEntryPoint : EasyHook.IEntryPoint
    {
        readonly ServerInterface _server;

        readonly Queue<string> _messageQueue = new Queue<string>();

        public InjectionEntryPoint(
            EasyHook.RemoteHooking.IContext context,
            string channelName)
        {
            // Connect to server object using provided channel name
            _server = EasyHook.RemoteHooking.IpcConnectClient<ServerInterface>(channelName);

            // If Ping fails then the Run method will be not be called
            _server.Ping();
        }

        public void Run(EasyHook.RemoteHooking.IContext context, string channelName)
        {
            // Injection is now complete and the server interface is connected
            _server.IsInstalled(EasyHook.RemoteHooking.GetCurrentProcessId());

            // Install hooks

            var createFontA = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("gdi32.dll", "CreateFontA"),
                new CreateFontA_Delegate(CreateFontAHook),
                this);

            var createFontInderect = EasyHook.LocalHook.Create(
                EasyHook.LocalHook.GetProcAddress("gdi32.dll", "CreateFontIndirectA"),
                new CreateFontIndirect_Delegate(CreateFontIndirect_Hook),
                this);

            // Activate hooks on all threads except the current thread
            createFontA.ThreadACL.SetExclusiveACL(new Int32[] { 0 });
            createFontInderect.ThreadACL.SetExclusiveACL(new Int32[] { 0 });

            _server.ReportMessage("Hooks installed");

            // Wake up the process (required if using RemoteHooking.CreateAndInject)
            EasyHook.RemoteHooking.WakeUpProcess();

            try
            {
                // Loop until closes (i.e. IPC fails)
                while (true)
                {
                    System.Threading.Thread.Sleep(500);

                    string[] queued;

                    lock (_messageQueue)
                    {
                        queued = _messageQueue.ToArray();
                        _messageQueue.Clear();
                    }

                    if (queued.Length > 0)
                    {
                        _server.ReportMessages(queued);
                    }
                    else
                    {
                        _server.Ping();
                    }
                }
            }
            catch
            {
                // Ping() or ReportMessages() will raise an exception if host is unreachable
            }

            // Remove hooks
            createFontA.Dispose();
            createFontInderect.Dispose();

            // Finalise cleanup of hooks
            EasyHook.LocalHook.Release();
        }

        private static string[] OkFonts = new[] { "Arial", "Times New Roman", "Courier New" };

        #region CreateFontA Hook

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        delegate IntPtr CreateFontA_Delegate(
                     int nHeight,
                     int nWidth,
                     int nEscapement,
                     int nOrientation,
                     int fnWeight,
                     uint fdwItalic,
                     uint fdwUnderline,
                     uint fdwStrikeOut,
                     uint fdwCharSet,
                     uint fdwOutputPrecision,
                     uint fdwClipPrecision,
                     uint fdwQuality,
                     uint fdwPitchAndFamily,
                     string lpszFace);

        [DllImport("gdi32.dll")]
        static extern IntPtr CreateFontA(
            int nHeight,
            int nWidth,
            int nEscapement,
            int nOrientation,
            int fnWeight,
            uint fdwItalic,
            uint fdwUnderline,
            uint fdwStrikeOut,
            uint fdwCharSet,
            uint fdwOutputPrecision,
            uint fdwClipPrecision,
            uint fdwQuality,
            uint fdwPitchAndFamily,
            string lpszFace);

        IntPtr CreateFontAHook(
              int nHeight,
              int nWidth,
              int nEscapement,
              int nOrientation,
              int fnWeight,
              uint fdwItalic,
              uint fdwUnderline,
              uint fdwStrikeOut,
              uint fdwCharSet,
              uint fdwOutputPrecision,
              uint fdwClipPrecision,
              uint fdwQuality,
              uint fdwPitchAndFamily,
              string lpszFace)
        {
#if DEBUG
            try
            {
                lock (_messageQueue)
                {
                    if (_messageQueue.Count < 1000)
                    {
                        _messageQueue.Enqueue(
                            string.Format($"[{EasyHook.RemoteHooking.GetCurrentProcessId()}:{EasyHook.RemoteHooking.GetCurrentThreadId()}]: CreateFontA, {lpszFace}"));
                    }
                }
            }
            catch
            {
            }
#endif

            fdwQuality = (uint)FontQuality.DEFAULT_QUALITY;
            fdwCharSet = (uint)FontCharSet.RUSSIAN_CHARSET;
            if (!OkFonts.Contains(lpszFace))
            {
                lpszFace = "Arial";
            }

            // now call the original API...
            return CreateFontA(
                nHeight,
                nWidth,
                nEscapement,
                nOrientation,
                fnWeight,
                 fdwItalic,
                 fdwUnderline,
                 fdwStrikeOut,
                 fdwCharSet,
                 fdwOutputPrecision,
                 fdwClipPrecision,
                 fdwQuality,
                 fdwPitchAndFamily,
                 lpszFace);
        }

        #endregion

        #region CreateFontIndirectA Hook
        [DllImport("gdi32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr CreateFontIndirectA(
            [In, MarshalAs(UnmanagedType.LPStruct)]
            LOGFONT lplf   // characteristics
        );

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Auto, SetLastError = true)]
        delegate IntPtr CreateFontIndirect_Delegate(LOGFONT lplf);


        IntPtr CreateFontIndirect_Hook(LOGFONT lplf)
        {
#if DEBUG
            try
            {
                lock (_messageQueue)
                {
                    if (_messageQueue.Count < 1000)
                    {
                        _messageQueue.Enqueue(string.Format($"[{EasyHook.RemoteHooking.GetCurrentProcessId()}:{EasyHook.RemoteHooking.GetCurrentThreadId()}]: CreateFontIndirect_Hook {lplf}"));
                    }
                }
            }
            catch
            {
            }
#endif

            lplf.lfQuality = FontQuality.DEFAULT_QUALITY;
            lplf.lfCharSet = FontCharSet.RUSSIAN_CHARSET;
            if (!OkFonts.Contains(lplf.lfFaceName))
            {
                lplf.lfFaceName = "Arial";
            }
            // now call the original API...
            return CreateFontIndirectA(lplf);
        }

        // if we specify CharSet.Auto instead of CharSet.Ansi, then the string will be unreadable
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public class LOGFONT
        {
            public int lfHeight;
            public int lfWidth;
            public int lfEscapement;
            public int lfOrientation;
            public FontWeight lfWeight;
            [MarshalAs(UnmanagedType.U1)]
            public bool lfItalic;
            [MarshalAs(UnmanagedType.U1)]
            public bool lfUnderline;
            [MarshalAs(UnmanagedType.U1)]
            public bool lfStrikeOut;
            public FontCharSet lfCharSet;
            public FontPrecision lfOutPrecision;
            public FontClipPrecision lfClipPrecision;
            public FontQuality lfQuality;
            public FontPitchAndFamily lfPitchAndFamily;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string lfFaceName;

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("LOGFONT\n");
                sb.AppendFormat("   lfHeight: {0}\n", lfHeight);
                sb.AppendFormat("   lfWidth: {0}\n", lfWidth);
                sb.AppendFormat("   lfEscapement: {0}\n", lfEscapement);
                sb.AppendFormat("   lfOrientation: {0}\n", lfOrientation);
                sb.AppendFormat("   lfWeight: {0}\n", lfWeight);
                sb.AppendFormat("   lfItalic: {0}\n", lfItalic);
                sb.AppendFormat("   lfUnderline: {0}\n", lfUnderline);
                sb.AppendFormat("   lfStrikeOut: {0}\n", lfStrikeOut);
                sb.AppendFormat("   lfCharSet: {0}\n", lfCharSet);
                sb.AppendFormat("   lfOutPrecision: {0}\n", lfOutPrecision);
                sb.AppendFormat("   lfClipPrecision: {0}\n", lfClipPrecision);
                sb.AppendFormat("   lfQuality: {0}\n", lfQuality);
                sb.AppendFormat("   lfPitchAndFamily: {0}\n", lfPitchAndFamily);
                sb.AppendFormat("   lfFaceName: {0}\n", lfFaceName);

                return sb.ToString();
            }
        }

        public enum FontWeight : int
        {
            FW_DONTCARE = 0,
            FW_THIN = 100,
            FW_EXTRALIGHT = 200,
            FW_LIGHT = 300,
            FW_NORMAL = 400,
            FW_MEDIUM = 500,
            FW_SEMIBOLD = 600,
            FW_BOLD = 700,
            FW_EXTRABOLD = 800,
            FW_HEAVY = 900,
        }
        public enum FontCharSet : byte
        {
            ANSI_CHARSET = 0,
            DEFAULT_CHARSET = 1,
            SYMBOL_CHARSET = 2,
            SHIFTJIS_CHARSET = 128,
            HANGEUL_CHARSET = 129,
            HANGUL_CHARSET = 129,
            GB2312_CHARSET = 134,
            CHINESEBIG5_CHARSET = 136,
            OEM_CHARSET = 255,
            JOHAB_CHARSET = 130,
            HEBREW_CHARSET = 177,
            ARABIC_CHARSET = 178,
            GREEK_CHARSET = 161,
            TURKISH_CHARSET = 162,
            VIETNAMESE_CHARSET = 163,
            THAI_CHARSET = 222,
            EASTEUROPE_CHARSET = 238,
            RUSSIAN_CHARSET = 204,
            MAC_CHARSET = 77,
            BALTIC_CHARSET = 186,
        }
        public enum FontPrecision : byte
        {
            OUT_DEFAULT_PRECIS = 0,
            OUT_STRING_PRECIS = 1,
            OUT_CHARACTER_PRECIS = 2,
            OUT_STROKE_PRECIS = 3,
            OUT_TT_PRECIS = 4,
            OUT_DEVICE_PRECIS = 5,
            OUT_RASTER_PRECIS = 6,
            OUT_TT_ONLY_PRECIS = 7,
            OUT_OUTLINE_PRECIS = 8,
            OUT_SCREEN_OUTLINE_PRECIS = 9,
            OUT_PS_ONLY_PRECIS = 10,
        }
        public enum FontClipPrecision : byte
        {
            CLIP_DEFAULT_PRECIS = 0,
            CLIP_CHARACTER_PRECIS = 1,
            CLIP_STROKE_PRECIS = 2,
            CLIP_MASK = 0xf,
            CLIP_LH_ANGLES = (1 << 4),
            CLIP_TT_ALWAYS = (2 << 4),
            CLIP_DFA_DISABLE = (4 << 4),
            CLIP_EMBEDDED = (8 << 4),
        }
        public enum FontQuality : byte
        {
            DEFAULT_QUALITY = 0,
            DRAFT_QUALITY = 1,
            PROOF_QUALITY = 2,
            NONANTIALIASED_QUALITY = 3,
            ANTIALIASED_QUALITY = 4,
            CLEARTYPE_QUALITY = 5,
            CLEARTYPE_NATURAL_QUALITY = 6,
        }
        [Flags]
        public enum FontPitchAndFamily : byte
        {
            DEFAULT_PITCH = 0,
            FIXED_PITCH = 1,
            VARIABLE_PITCH = 2,
            FF_DONTCARE = (0 << 4),
            FF_ROMAN = (1 << 4),
            FF_SWISS = (2 << 4),
            FF_MODERN = (3 << 4),
            FF_SCRIPT = (4 << 4),
            FF_DECORATIVE = (5 << 4),
        }


        #endregion
    }
}