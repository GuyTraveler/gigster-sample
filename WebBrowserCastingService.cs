using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace BRK.Common.Utility
{
    public static class WebBrowserCastingService
    {
        [ComImport]
        [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IWebBrowserServiceProvider
        {
            [return: MarshalAs(UnmanagedType.Interface)]
            object QueryService(ref Guid guidService, ref Guid riid);
        }

        static readonly Guid SID_WebBrowserApp = new Guid("0002DF05-0000-0000-C000-000000000046");
        public static SHDocVw.IWebBrowser2 Cast(System.Windows.Controls.WebBrowser wb)
        {
            if (wb != null && wb.Document != null)
            {
                IWebBrowserServiceProvider serviceProvider = (IWebBrowserServiceProvider)wb.Document;

                if (serviceProvider != null)
                {
                    Guid serviceGuid = SID_WebBrowserApp;
                    Guid iid = typeof(SHDocVw.IWebBrowser2).GUID;

                    return (SHDocVw.IWebBrowser2)serviceProvider.QueryService(ref serviceGuid, ref iid);
                }
            }

            return null;
        }
    }
}
