using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;

namespace LayoutSwitcher
{
    internal static class Program
    {
        private const int HcAction = 0;
        private const int WhKeyboardLl = 13;
        private const int WmKeydown = 0x0100;
        private const int WmKeyup = 0x0101;
        private static IntPtr _hookHandle = IntPtr.Zero;
        private static Bar _bar;
        private static bool _kWin, _kSpace;

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, KbHook lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hhwnd, uint msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll")]
        private static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [STAThread]
        private static void Main()
        {
            _kWin = false;
            _kSpace = false;
            try
            {
                using (var proc = Process.GetCurrentProcess())
                using (var curModule = proc.MainModule)
                {
                    var moduleHandle = GetModuleHandle(curModule.ModuleName);
                    _hookHandle = SetWindowsHookEx(WhKeyboardLl, IgnoreWin_Space, moduleHandle, 0);
                }
                _bar = new Bar();
                BuildMenu();
                Application.Run();
            }
            finally
            {
                UnhookWindowsHookEx(_hookHandle);
            }
        }

        private static void BuildMenu()
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
            notifyIcon1.Text = "LayoutSwitcher";
            notifyIcon1.Visible = true;
        }

        static void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        static IntPtr IgnoreWin_Space(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool spacePressed = false;
            var keyInfo = (KbHookParam)Marshal.PtrToStructure(lParam, typeof(KbHookParam));

            if (nCode == HcAction)
            {
                if ((int)wParam == WmKeydown)
                {
                    if (keyInfo.VkCode == (int)Keys.Space)
                    {
                        spacePressed = true;
                        _kSpace = true;
                    }
                    else
                    {
                        _kSpace = false;
                    }

                    // нажат одновременно левый виндовс
                    if (GetAsyncKeyState(Keys.LWin) < 0)
                    {
                        _kWin = true;
                    }
                    else
                    {
                        _kWin = false;
                    }

                    if (_kWin && _kSpace)
                    {
                        if (spacePressed)
                        {
                            _bar.SetLanguage();
                            _bar.Show(); // сбивает фокус, пофиксим в конструкторе
                            return (IntPtr)1; //just ignore the key press
                        }
                    }
                }
            }
            if ((int)wParam == WmKeyup)
            {
                if (keyInfo.VkCode == (int)Keys.LWin)
                {
                    _kWin = false;
                    _bar.DoHide();
                    var hex = _bar.GetHex();
                    const uint wmInputLangChangeRequest = 0x0050;
                    const uint KLF_ACTIVATE = 1;
                    PostMessage(GetForegroundWindow(), wmInputLangChangeRequest, IntPtr.Zero, LoadKeyboardLayout(hex, KLF_ACTIVATE));
                }
            }

            return CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private delegate IntPtr KbHook(int nCode, IntPtr wParam, [In] IntPtr lParam);

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct KbHookParam
        {
            public readonly int VkCode;
            private readonly int ScanCode;
            private readonly int Flags;
            private readonly int Time;
            private readonly IntPtr Extra;
        }
    }
}
