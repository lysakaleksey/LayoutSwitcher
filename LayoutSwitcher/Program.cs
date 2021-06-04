using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Drawing;

namespace LayoutSwitcher
{
    internal static class Program
    {
        private const int WmInputLangChangeRequest = 0x0050;
        private const int WmKeydown = 0x0100;
        private const int WhKeyboardLl = 13;
        private const int WmKeyup = 0x0101;
        private const int KlfActivate = 1;
        private const int HcAction = 0;
        private static IntPtr _hookHandle = IntPtr.Zero;
        private static bool _kWin, _kSpace;
        private static Bar _bar;

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
                    var moduleHandle = Helper.GetModuleHandle(curModule.ModuleName);
                    _hookHandle = Helper.SetWindowsHookEx(WhKeyboardLl, IgnoreWin_Space, moduleHandle, 0);
                }

                var notifyIcon1 = new NotifyIcon();
                var contextMenuStrip1 = new ContextMenuStrip();
                var exitToolStripMenuItem = new ToolStripMenuItem();
                exitToolStripMenuItem.Name = "exitToolStripMenuItem";
                exitToolStripMenuItem.Size = new Size(155, 38);
                exitToolStripMenuItem.Text = "Exit";
                exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
                contextMenuStrip1.ImageScalingSize = new Size(32, 32);
                contextMenuStrip1.Items.AddRange(new ToolStripItem[] {exitToolStripMenuItem});
                contextMenuStrip1.Name = "contextMenuStrip1";
                contextMenuStrip1.Size = new Size(156, 80);
                notifyIcon1.ContextMenuStrip = contextMenuStrip1;
                notifyIcon1.Icon = Properties.Resources.StatusIcon;
                notifyIcon1.Text = "LayoutSwitcher";
                notifyIcon1.Visible = true;

                var appId = Helper.GetForegroundWindow();
                _bar = new Bar(appId);
                Application.Run();
            }
            finally
            {
                Helper.UnhookWindowsHookEx(_hookHandle);
            }
        }

        private static void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private static IntPtr IgnoreWin_Space(int nCode, IntPtr wParam, IntPtr lParam)
        {
            var spacePressed = false;
            var keyInfo = (KbHookParam) Marshal.PtrToStructure(lParam, typeof(KbHookParam));

            if (nCode == HcAction)
            {
                if ((int) wParam == WmKeydown)
                {
                    if (keyInfo.VkCode == (int) Keys.Space)
                    {
                        spacePressed = true;
                        _kSpace = true;
                    }
                    else
                    {
                        _kSpace = false;
                    }

                    _kWin = Helper.GetAsyncKeyState(Keys.LWin) < 0 || Helper.GetAsyncKeyState(Keys.RWin) < 0;
                    if (_kWin && _kSpace)
                    {
                        if (spacePressed)
                        {
                            Debug.WriteLine("Program. Left Win + Space pressed");
                            var appId = Helper.GetForegroundWindow();
                            _bar.SwitchLanguage(appId);
                            _bar.Show();
                            return (IntPtr) 1; //just ignore the key press
                        }
                    }
                }
            }

            if ((int) wParam == WmKeyup)
            {
                if (keyInfo.VkCode == (int) Keys.LWin || keyInfo.VkCode == (int) Keys.RWin)
                {
                    _kWin = false;
                    if (_bar.Visible)
                    {
                        var switchContext = _bar.DoHide();
                        var layout = Helper.LoadKeyboardLayout(switchContext.LayoutHex, KlfActivate);
                        Helper.PostMessage(Helper.GetForegroundWindow(), WmInputLangChangeRequest, IntPtr.Zero, layout);
                        Debug.WriteLine("Program. Changed Keyboard Language " + switchContext);
                    }
                }
            }

            return Helper.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

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