using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;

namespace LayoutSwitcher
{
    public partial class Bar : Form
    {
        private readonly Dictionary<IntPtr, AppLangContext> _contexts;
        private readonly List<InputLanguage> _languages;
        private AppLangContext _app;

        // Show a Form without stealing focus
        protected override bool ShowWithoutActivation => true;

        public Bar(IntPtr appId)
        {
            InitializeComponent();
            _contexts = new Dictionary<IntPtr, AppLangContext>();
            _languages = new List<InputLanguage>();
            foreach (InputLanguage lang in InputLanguage.InstalledInputLanguages)
            {
                var title = lang.Culture.Parent.NativeName.ToUpper();
                var code = lang.Culture.ToString();
                var id = lang.Culture.KeyboardLayoutId;
                _languages.Add(lang);
                Debug.WriteLine("Bar. title=" + title + ", code=" + code + ", id=" + id + ", hex=" + id.ToString("X8"));
            }

            _app = InitAppContext(appId);
            Debug.WriteLine("Bar. Final context " + _app);
        }

        public SwitchContext DoHide()
        {
            Debug.WriteLine("DoHide. Input " + _app);
            var layoutId = _languages[_app.Curr].Culture.KeyboardLayoutId;
            var layoutHex = layoutId.ToString("X8");
            _app.Counter = 0;

            var switchContext = new SwitchContext {LayoutId = layoutId, LayoutHex = layoutHex};

            Hide();

            Debug.WriteLine("DoHide. Output " + switchContext);

            return switchContext;
        }

        public void SwitchLanguage(IntPtr appId)
        {
            //Debug.WriteLine("SwitchLanguage. AppId " + appId);
            if (_contexts.ContainsKey(appId))
            {
                _app = _contexts[appId];
                Debug.WriteLine("SwitchLanguage. Restored context " + _app);
            }
            else
            {
                _app = InitAppContext(appId);
                Debug.WriteLine("SwitchLanguage. Created context " + _app);
            }

            lblLanguage.Text = _languages[_app.Curr].Culture.Parent.NativeName.ToUpper();
            Debug.WriteLine("SwitchLanguage. Old language " + lblLanguage.Text);

            _app.Counter++;
            if (_app.Counter == 1) // Pick previous
            {
                var prevIndex = _app.Prev;
                _app.Prev = _app.Curr;
                _app.Curr = prevIndex;
                Debug.WriteLine("SwitchLanguage. Case 1 " + _app);
            }
            else // Pressed second time, need to pick next in the list
            {
                _app.Prev = _app.Curr;
                if (_app.Curr < _languages.Count - 1)
                {
                    _app.Curr += 1;
                    Debug.WriteLine("SwitchLanguage. Case 2: " + _app);
                }
                else
                {
                    _app.Curr = 0;
                    Debug.WriteLine("SwitchLanguage. Case 3: " + _app);
                }
            }

            lblLanguage.Text = _languages[_app.Curr].Culture.Parent.NativeName.ToUpper();
            Debug.WriteLine("SwitchLanguage. New language " + lblLanguage.Text);
        }

        private AppLangContext InitAppContext(IntPtr appId)
        {
            Debug.WriteLine("InitAppContext. AppId " + appId);
            InputLanguage currLang = Helper.GetKeyboardLanguage(appId);
            Debug.WriteLine("InitAppContext. Keyboard language " + currLang.Culture.Parent.NativeName.ToUpper());

            var context = new AppLangContext {AppId = appId, Curr = _languages.IndexOf(currLang)};
            if (context.Curr == 0) // Edge case. I want next after default
            {
                context.Prev = 1;
            }
            else if (context.Curr < _languages.Count() - 1)
            {
                context.Prev = context.Curr - 1;
            }
            else
            {
                context.Prev = 0;
            }

            Debug.WriteLine("InitAppContext. Created context " + context);
            _contexts.Add(appId, context);
            return context;
        }
    }
}