using Ping9719.MindeoScanner;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        ScannerCode scannerCode;
        private void button1_Click(object sender, EventArgs e)
        {
            scannerCode = new ScannerCode();
            scannerCode.Open(textBox1.Text);

            scannerCode.ScanMess += ScannerCode_ScanMess;
        }

        private void ScannerCode_ScanMess(object sender, string mess)
        {
            textBox1.Text += @"
" + mess;
        }
    }
}
