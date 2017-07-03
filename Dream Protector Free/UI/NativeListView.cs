using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Dream_Protector_Free.UI
{
    public class NativeListView : ListView
    {
        protected override void CreateHandle()
        {
            base.CreateHandle();
            NativeMethods.SetWindowTheme(this.Handle, "explorer", null);
        }
    }
}
