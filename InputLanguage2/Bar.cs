using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Globalization;
using System.Diagnostics;

namespace InputLanguage2
{
    public partial class Bar : Form
    {
        List<InputLanguage> inputLanguages;
        //List<string> langTitles;
        //List<string> langCodes;
        string retHex;
        int prev_index = -1;
        int counter = 0;
        int pos = 0;


        // Show a Form without stealing focus
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        public Bar()
        {
            InitializeComponent();

            inputLanguages = new List<InputLanguage>();
            //langTitles = new List<string>();
            //langCodes = new List<string>();
            foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
            {
                string title = lang.Culture.Parent.NativeName.ToUpper();
                string code = lang.Culture.ToString();
                int id = lang.Culture.KeyboardLayoutId;
                //langTitles.Add(title);
                //langCodes.Add(code);
                inputLanguages.Add(lang);
                Debug.WriteLine("Bar. title=" + title + ", code=" + code + ", id=" + id + ", hex=" +  id.ToString("X8"));
            }

            InputLanguage currLang = InputLanguage.CurrentInputLanguage;
            Debug.WriteLine("Bar. currLang=" + currLang.Culture);

            pos = inputLanguages.IndexOf(currLang); // текущий язык
            if (pos < inputLanguages.Count() - 1)
                prev_index = pos + 1; 
            else
                prev_index = 0; // прошлый язык будет следующим в списке
            Debug.WriteLine("Bar. pos=" + pos);
            Debug.WriteLine("Bar. prev_index=" + prev_index);
        }

        public string GetHex()
        {
            Debug.WriteLine("getHex. retHex=" + retHex);
            return retHex;
        }

        public void DoHide()
        {
            //string langCode = inputLanguages[pos];
            //Debug.WriteLine("DoHide. langCode=" + langCode);

            //int intValue = new CultureInfo(langCode).KeyboardLayoutId;
            int intValue = inputLanguages[pos].Culture.KeyboardLayoutId;
            Debug.WriteLine("DoHide. KeyboardLayoutId=" + intValue);

            string hex = intValue.ToString("X8");
            Debug.WriteLine("DoHide. hex=" + hex);

            retHex = hex;
            counter = 0;
            Hide();
        }

        public void SetLanguage()
        {
            counter++;
            lblLanguage.Text = InputLanguage.CurrentInputLanguage.Culture.Parent.NativeName.ToUpper();
            Debug.WriteLine("SetLanguage. TextOld=" + lblLanguage.Text);

            if (counter == 1) // берем предыдущий
            {
                int t = prev_index;
                prev_index = pos;
                pos = t;
            }
            else // если нажали второй раз
            {
                prev_index = pos;
                if (pos > 0)
                    pos -= 1;
                else
                    pos = inputLanguages.Count() - 1;
            }

            lblLanguage.Text = inputLanguages[pos].Culture.Parent.NativeName.ToUpper();
            Debug.WriteLine("SetLanguage. TextNew=" + lblLanguage.Text);
        }
    }
}
