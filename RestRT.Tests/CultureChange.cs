using System;
using System.Globalization;
using System.Threading;

namespace RestRT.Tests
{
    public class CultureChange : IDisposable
    {
        //public CultureInfo PreviousCulture { get; private set; }
        public string PreviousCulture { get; private set; }

        public CultureChange(string culture)
        {
            if (culture == null)
                throw new ArgumentNullException("culture");

            //PreviousCulture = Thread.CurrentThread.CurrentCulture;
            //Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);

            PreviousCulture = Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;
            Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = culture;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (PreviousCulture != null)
            {
                Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = PreviousCulture;

                //Thread.CurrentThread.CurrentCulture = PreviousCulture;
                //PreviousCulture = null;
            }
        }

        #endregion
    }
}