//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Runtime.InteropServices;
//using System.Diagnostics;
//using System.Collections;
//using System.Threading;

//namespace SB1Util.win32API
//{
//    public class WinApi
//    {
//        private delegate void exemploDelegate();
//        /// <summary>
//        /// Classe que configura e recebe informações da API do Windows
//        /// </summary>
//        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
//        public class OpenDialogArgs
//        {
//            public int structSize = 0;
//            public IntPtr dlgOwner = IntPtr.Zero;
//            public IntPtr instance = IntPtr.Zero;

//            public String filter = null;
//            public String customFilter = null;
//            public int maxCustFilter = 0;
//            public int filterIndex = 0;

//            public String file = null;
//            public int maxFile = 0;

//            public String fileTitle = null;
//            public int maxFileTitle = 0;

//            public String initialDir = null;

//            public String title = null;

//            public int flags = 0;
//            public short fileOffset = 0;
//            public short fileExtension = 0;

//            public String defExt = null;

//            public IntPtr custData = IntPtr.Zero;
//            public IntPtr hook = IntPtr.Zero;

//            public String templateName = null;

//            public IntPtr reservedPtr = IntPtr.Zero;
//            public int reservedInt = 0;
//            public int flagsEx = 0;
//        }

//        /// <summary>
//        /// Classe que manipula as caixas de diálogo do Windows via API
//        /// </summary>
//        public class Win32Dialog
//        {
//            static Thread ThreadFileControl = new Thread(new ParameterizedThreadStart(searchWindow));

//            private const int HWND_TOPMOST = -1;
//            private const int SWP_NOMOVE = 0x0002;
//            private const int SWP_NOSIZE = 0x0001;

//            private delegate void searchWindowHandler(object WindowName);
//            private delegate OpenDialogArgs OpenFileHandler(string Filter, string InitialDir, string DefaultExtension, string Files2Search, string Title);


//            [DllImport("Comdlg32.dll", CharSet = CharSet.Auto)]
//            private static extern bool GetOpenFileName([In, Out] OpenDialogArgs ofn);

//            [DllImport("user32.dll", SetLastError = true)]
//            [return: MarshalAs(UnmanagedType.Bool)]
//            private static extern bool SetWindowPos(IntPtr hWnd,
//                int hWndInsertAfter, int x, int y, int cx, int cy, int uFlags);


//            [DllImport("user32.dll", EntryPoint = "FindWindow", SetLastError = true)]
//            static extern IntPtr FindWindowByCaption(IntPtr ZeroOnly, string lpWindowName);

//            [DllImport("user32.dll")]
//            public static extern int FindWindow(string lpClassName, string lpWindowName);
//            /// <summary>
//            /// Abre uma caixa de diálogo de procura de arquivos via API do Windows
//            /// </summary>
//            /// <param name="Filter">Serve para filtrar os tipos de arquivos que serão exibidos</param>            
//            /// <param name="InitialDir"></param>
//            /// <param name="DefaultExtension"></param>
//            /// <param name="Files2Search"></param>
//            /// <param name="Title"></param>
//            /// <returns></returns>
//            public static OpenDialogArgs OpenFileDialog(string Filter, string InitialDir, string DefaultExtension, string Files2Search, string Title = "")
//            {
//                ThreadFileControl.Start(Title);
//                OpenDialogArgs f = OpenFileHand(Filter, InitialDir, DefaultExtension, Files2Search, Title);
//                return f;
//            }

//            private static searchWindowHandler searchWindow = delegate(object WindowName)
//            {
//                while (true)
//                {
//                    IntPtr ptr = FindWindowByCaption(IntPtr.Zero, (string)WindowName);

//                    if (ptr.ToInt32() > 0)
//                    {
//                        SetWindowPos(ptr, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE);
//                        ThreadFileControl.Abort();
//                        break;
//                    }
//                }
//            };

//            private static OpenFileHandler OpenFileHand = delegate(string Filter, string InitialDir, string DefaultExtension, string Files2Search, string Title)
//            {

//                OpenDialogArgs ofn = new OpenDialogArgs();

//                ofn.structSize = Marshal.SizeOf(ofn);

//                ofn.filter = Filter;
//                ofn.file = new String(new char[256]);
//                ofn.maxFile = ofn.file.Length;
//                ofn.fileTitle = new String(new char[64]);
//                ofn.maxFileTitle = ofn.fileTitle.Length;
//                ofn.initialDir = InitialDir;
//                ofn.title = Title;
//                ofn.defExt = DefaultExtension;
//                ofn.flags = 0x00000100;
//                ofn.file = Files2Search;
//                //ofn.dlgOwner = ProcessObj.Handle;               

//                GetOpenFileName(ofn);

//                return ofn;

//            };


//        }
//    }


//}
