using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security;

using MemoryControl;

namespace ProbablyEngine
{
    public partial class Form1 : Form
    {

        private int SelectedProcessID;
        private List<Processes> dataSource;

        public class Processes
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        int address = (int) 0x8B1695;
        byte[] patch = {0xEB, 0x36};

        public Form1()
        {
            InitializeComponent();
            WinSparkle.win_sparkle_set_appcast_url("http://probablyengine.com/appcast-win.xml");
            WinSparkle.win_sparkle_init();
        }

        private void OnApplicationExit(object sender, EventArgs e) {
            WinSparkle.win_sparkle_cleanup();
        }

        private void populateList()
        {
            Process[] processes = Process.GetProcessesByName("wow");
            if (processes.Length > 0)
            {
                dataSource = new List<Processes>();
                foreach (var process in processes)
                {
                    dataSource.Add(
                        new Processes()
                        {
                            Id = process.Id,
                            Name = String.Format("{0}.exe - {1}", process.ProcessName, process.Id)
                        }
                    );
                }
                comboBox1.DataSource = dataSource;
                comboBox1.DisplayMember = "Name";
                comboBox1.ValueMember = "Id";
                comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
                comboBox1.Enabled = true;
            }
            else
            {
                comboBox1.Enabled = false;
                comboBox1.Text = "Game Not Open";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            populateList();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            populateList();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MemC.cOpenProcessId(SelectedProcessID);
            int rebase = (int)Process.GetProcessById(SelectedProcessID).MainModule.BaseAddress + address;
            MemC.WriteXBytes(rebase, patch);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedProcessID = dataSource[comboBox1.SelectedIndex].Id;
            button2.Enabled = true;
        }

        private void statusText_Click(object sender, EventArgs e)
        {

        }
    }
}
