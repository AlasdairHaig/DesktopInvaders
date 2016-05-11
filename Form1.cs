using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;

namespace DesktopInvaders
{
    public partial class Form1 : Form
    {
        private List<DirectoryInfo> folders = new List<DirectoryInfo>();
        private List<FileInfo> files = new List<FileInfo>();
        private const int MAX_PATH = 260;
        private const UInt32 SPI_GETDESKWALLPAPER = 0x73;

        private Image background;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SystemParametersInfo(UInt32 uAction, int uParam, string lpvParam, int fuWinIni);

        public Form1()
        {
            InitializeComponent();

            getFiles();

            background = Wallpaper();
            this.BackgroundImage = background;

            Taskbar bar = new Taskbar();
            getTaskbar(bar);
        }

        private void getTaskbar(Taskbar bar)
        {
            Rectangle rect = new Rectangle(bar.Location, bar.Size);
            Bitmap bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.CopyFromScreen(rect.Left, rect.Top, 0, 0, bmp.Size, CopyPixelOperation.SourceCopy);
            //bmp.Save(@"C:\\Users\b.wood\Documents", ImageFormat.Jpeg);
            
        }

        private Image Wallpaper()
        {
            //string wallpaper = Registry.GetValue(@"HKEY_CURRENT_USER\Control Panel\Desktop", "OriginalWallpaper", 0).ToString();
            //pictureBox1.Image = Image.FromFile(wallpaper);

            string currentWallpaper = new string('\0', MAX_PATH);
            SystemParametersInfo(SPI_GETDESKWALLPAPER, currentWallpaper.Length, currentWallpaper, 0);

            return Image.FromFile(currentWallpaper.Substring(0, currentWallpaper.IndexOf('\0')));
        }

        public void getFiles()
        {
            DirectoryInfo dir = new DirectoryInfo(@"C:\Users\b.wood\Desktop");

            foreach (FileInfo f in dir.GetFiles("*.*"))
            {
                files.Add(f);
            }

            foreach (DirectoryInfo g in dir.GetDirectories())
            {
                folders.Add(g);
            }
        }
    }


    public enum TaskbarPosition
    {
        Unknown = -1,
        Left,
        Top,
        Right,
        Bottom,
    }

    public sealed class Taskbar
    {
        private const string ClassName = "Shell_TrayWnd";

        public Rectangle Bounds
        {
            get;
            private set;
        }
        public TaskbarPosition Position
        {
            get;
            private set;
        }
        public Point Location
        {
            get
            {
                return this.Bounds.Location;
            }
        }
        public Size Size
        {
            get
            {
                return this.Bounds.Size;
            }
        }
        //Always returns false under Windows 7
        public bool AlwaysOnTop
        {
            get;
            private set;
        }
        public bool AutoHide
        {
            get;
            private set;
        }

        public Taskbar()
        {
            IntPtr taskbarHandle = User32.FindWindow(Taskbar.ClassName, null);

            APPBARDATA data = new APPBARDATA();
            data.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));
            data.hWnd = taskbarHandle;
            IntPtr result = Shell32.SHAppBarMessage(ABM.GetTaskbarPos, ref data);
            if (result == IntPtr.Zero)
                throw new InvalidOperationException();

            this.Position = (TaskbarPosition)data.uEdge;
            this.Bounds = Rectangle.FromLTRB(data.rc.left, data.rc.top, data.rc.right, data.rc.bottom);

            data.cbSize = (uint)Marshal.SizeOf(typeof(APPBARDATA));
            result = Shell32.SHAppBarMessage(ABM.GetState, ref data);
            int state = result.ToInt32();
            this.AlwaysOnTop = (state & ABS.AlwaysOnTop) == ABS.AlwaysOnTop;
            this.AutoHide = (state & ABS.Autohide) == ABS.Autohide;
        }
    }

    public enum ABM : uint
    {
        New = 0x00000000,
        Remove = 0x00000001,
        QueryPos = 0x00000002,
        SetPos = 0x00000003,
        GetState = 0x00000004,
        GetTaskbarPos = 0x00000005,
        Activate = 0x00000006,
        GetAutoHideBar = 0x00000007,
        SetAutoHideBar = 0x00000008,
        WindowPosChanged = 0x00000009,
        SetState = 0x0000000A,
    }

    public enum ABE : uint
    {
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3
    }

    public static class ABS
    {
        public const int Autohide = 0x0000001;
        public const int AlwaysOnTop = 0x0000002;
    }

    public static class Shell32
    {
        [DllImport("shell32.dll", SetLastError = true)]
        public static extern IntPtr SHAppBarMessage(ABM dwMessage, [In] ref APPBARDATA pData);
    }

    public static class User32
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public ABE uEdge;
        public RECT rc;
        public int lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
