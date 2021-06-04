using System;

namespace LayoutSwitcher
{
    public class AppLangContext
    {
        public IntPtr AppId;
        public int Counter;
        public int Prev;
        public int Curr;

        public override string ToString()
        {
            return "AppLangContext(AppId=" + AppId +
                   ", counter=" + Counter +
                   ", prev=" + Prev +
                   ", curr=" + Curr + ")";
        }
    }
}