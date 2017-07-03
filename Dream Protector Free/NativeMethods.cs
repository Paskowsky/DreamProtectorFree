using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Dream_Protector_Free
{
    static class NativeMethods
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        internal extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName,
                                              string pszSubIdList);
    }
}
