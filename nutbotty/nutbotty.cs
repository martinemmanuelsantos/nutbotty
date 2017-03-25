using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace nutbotty
{
    public partial class NutBotty : Form
    {

        public RichTextBox chatBox { get { return chatTextBox; } }
        public RichTextBox consoleBox { get { return consoleTextBox; } }

        public NutBotty()
        {
            InitializeComponent();
        }

        private void NutBotty_FormClosing(object sender, FormClosingEventArgs e)
        {
            System.Environment.Exit(1);
        }

        private void chatTextBox_TextChanged(object sender, EventArgs e)
        {
            chatTextBox.SelectionStart = chatTextBox.Text.Length;
            chatTextBox.ScrollToCaret();
        }

        private void consoleTextBox_TextChanged(object sender, EventArgs e)
        {
            consoleBox.SelectionStart = consoleBox.Text.Length;
            consoleBox.ScrollToCaret();
        }
    }
}
