using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace LayoutSwitcher
{
    public static class Helper
    {
        public delegate IntPtr KbHook(int nCode, IntPtr wParam, [In] IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, KbHook lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern bool PostMessage(IntPtr hhwnd, uint msg, IntPtr wparam, IntPtr lparam);

        [DllImport("user32.dll")]
        public static extern IntPtr LoadKeyboardLayout(string pwszKLID, uint Flags);

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hwnd, IntPtr proccess);

        [DllImport("user32.dll")]
        public static extern IntPtr GetKeyboardLayout(uint thread);
        
        public static InputLanguage GetKeyboardLanguage(IntPtr appId)
        {
            var process = GetWindowThreadProcessId(appId, IntPtr.Zero);
            var keyboardLayoutId = GetKeyboardLayout(process).ToInt32() & 0xFFFF;
            foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
            {
                if (keyboardLayoutId == lang.Culture.KeyboardLayoutId)
                {
                    return lang;
                }
            }

            return null;
        }
    }
}