using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;

namespace LayoutSwitcher
{
    static class Program
    {
        const int HC_ACTION = 0;
        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        static IntPtr HookHandle = IntPtr.Zero;
        static Bar Bar;

        static Boolean kWin, kSpace;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, KbHook lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hhwnd, uint msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll")]
        static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [STAThread]
        static void Main()
        {
            kWin = false;
            kSpace = false;
            try
            {
                using (var proc = Process.GetCurrentProcess())
                using (var curModule = proc.MainModule)
                {
                    var moduleHandle = GetModuleHandle(curModule.ModuleName);
                    HookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, IgnoreWin_Space, moduleHandle, 0);
                }
                Bar = new Bar();
                BuildMenu();
                Application.Run();
            }
            finally
            {
                UnhookWindowsHookEx(HookHandle);
            }
        }

        static void BuildMenu()
        {
            NotifyIcon notifyIcon1 = new NotifyIcon();
            ContextMenuStrip contextMenuStrip1 = new ContextMenuStrip();
            ToolStripMenuItem exitToolStripMenuItem = new ToolStripMenuItem();
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(155, 38);
            exitToolStripMenuItem.Text = "Exit";
            exitToolStripMenuItem.Click += new EventHandler(ExitToolStripMenuItem_Click);
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(32, 32);
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] {exitToolStripMenuItem});
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(156, 80);
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = Properties.Resources.StatusIcon;
            notifyIcon1.Text = "KeyboardLayoutRetainer";
            notifyIcon1.Visible = true;
        }

        static void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        static IntPtr IgnoreWin_Space(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Boolean spacePressed = false;
            var keyInfo = (KbHookParam)Marshal.PtrToStructure(lParam, typeof(KbHookParam));

            if (nCode == HC_ACTION)
            {
                if ((int)wParam == WM_KEYDOWN)
                {
                    if (keyInfo.VkCode == (int)Keys.Space)
                    {
                        spacePressed = true;
                        kSpace = true;
                    }
                    else
                    {
                        kSpace = false;
                    }

                    // нажат одновременно левый виндовс
                    if (GetAsyncKeyState(Keys.LWin) < 0)
                    {
                        kWin = true;
                    }
                    else
                    {
                        kWin = false;
                    }

                    if (kWin && kSpace)
                    {
                        if (spacePressed)
                        {
                            Bar.SetLanguage();
                            Bar.Show(); // сбивает фокус, пофиксим в конструкторе
                            return (IntPtr)1; //just ignore the key press
                        }
                    }
                }
            }
            if ((int)wParam == WM_KEYUP)
            {
                if (keyInfo.VkCode == (int)Keys.LWin)
                {
                    kWin = false;
                    Bar.DoHide();
                    string HEX = Bar.GetHex();
                    uint WM_INPUTLANGCHANGEREQUEST = 0x0050;
                    uint KLF_ACTIVATE = 1;
                    PostMessage(GetForegroundWindow(), WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, LoadKeyboardLayout(HEX, KLF_ACTIVATE));
                }
            }

            return CallNextHookEx(HookHandle, nCode, wParam, lParam);
        }

        delegate IntPtr KbHook(int nCode, IntPtr wParam, [In] IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        struct KbHookParam
        {
            public readonly int VkCode;
            public readonly int ScanCode;
            public readonly int Flags;
            public readonly int Time;
            public readonly IntPtr Extra;
        }
    }
}
