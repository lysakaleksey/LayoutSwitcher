using System;

namespace LayoutSwitcher
{
    public class SwitchContext
    {
        public int LayoutId;
        public string LayoutHex;
        
        public override string ToString()
        {
            return "SwitchContext(layoutId=" + LayoutId + ", layoutHex=" + LayoutHex + ")";
        }
    }
}