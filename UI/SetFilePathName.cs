using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SB1Util.UI
{
    public class SetFilePathName
    {

        private class WindowWrapper : System.Windows.Forms.IWin32Window
        {
            private IntPtr _hwnd;

            // Propriedade
            public virtual IntPtr Handle
            {
                get { return _hwnd; }
            }

            // Construtor
            public WindowWrapper(IntPtr handle)
            {
                _hwnd = handle;
            }
        }


        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        OpenFileDialog fileDialog;
        FolderBrowserDialog oBrowserDialog;


        public SetFilePathName()
        {
            fileDialog = new OpenFileDialog();
            oBrowserDialog = new FolderBrowserDialog();
        }

        public bool ShowNewFolderButton
        {
            get { return oBrowserDialog.ShowNewFolderButton; }
            set { oBrowserDialog.ShowNewFolderButton = value; }
        }

        public string SelectedPath
        {
            get { return oBrowserDialog.SelectedPath; }
            set { oBrowserDialog.SelectedPath = value; }
        }

        public string SelectedFile
        {
            get { return fileDialog.FileName; }
            set { fileDialog.FileName = value; }
        }
        
        public string[] SelectedFiles
        {
            get { return fileDialog.FileNames; }
            //set { fileDialog.FileNames = value; }
        }


        public System.Environment.SpecialFolder RootFolder
        {
            get { return oBrowserDialog.RootFolder; }
            set { oBrowserDialog.RootFolder = value; }
        }

        public void SetPathName()
        {
            IntPtr ptr = GetForegroundWindow();
            WindowWrapper oWindow = new WindowWrapper(ptr);
            if (oBrowserDialog.ShowDialog(oWindow) != DialogResult.OK)
            {
                if (oBrowserDialog.SelectedPath != null)
                {
                    oBrowserDialog.SelectedPath = string.Empty;
                }
            }
            oWindow = null;
        }

        public void SetFileName()
        {
            IntPtr ptr = GetForegroundWindow();
            WindowWrapper oWindow = new WindowWrapper(ptr);
            if (fileDialog.ShowDialog(oWindow) != DialogResult.OK)
            {
                if (fileDialog.FileName != null)
                {
                    fileDialog.FileName = string.Empty;
                }
            }
            oWindow = null;
        }


    }
}
