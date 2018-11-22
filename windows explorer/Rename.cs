using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace windows_explorer
{
    public partial class Rename : Form
    {
        private string newName;
        private bool isChange;
        public string NewName
        {
            get
            {
                return newName;
            }
            set
            {
                newName = value;
            }
        }
        public bool Change
        { get { return isChange; } }

        public Rename()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            isChange = false;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            newName = textBox1.Text;
            isChange = true;
            this.Close();
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                newName = textBox1.Text;
                isChange = true;
                this.Close();
            }
        }

        private void Rename_Load(object sender, EventArgs e)
        {
            textBox1.Text = newName;
            isChange = false;
        }
    }
}
