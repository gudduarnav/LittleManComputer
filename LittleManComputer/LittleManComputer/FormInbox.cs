using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LittleManComputer
{
    public partial class FormInbox : Form
    {
        public static int value = 0;

        public FormInbox()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                FormInbox.value = int.Parse(tbValue.Text);
                if (FormInbox.value < 0)
                {
                    FormInbox.value = 0;
                }
                if (FormInbox.value > 999)
                {
                    FormInbox.value %= 1000;
                }
            }
            catch (Exception)
            {
                FormInbox.value = 0;
            }
            finally
            {
                this.Dispose();
            }
        }

        private void tbValue_TextChanged(object sender, EventArgs e)
        {
        }

        private void FormInbox_Load(object sender, EventArgs e)
        {
            this.tbValue.Text = FormInbox.value.ToString();
        }

        private void tbValue_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                try
                {
                    FormInbox.value = int.Parse(tbValue.Text);
                    if (FormInbox.value < 0)
                    {
                        FormInbox.value = 0;
                    }
                    if (FormInbox.value > 999)
                    {
                        FormInbox.value %= 1000;
                    }
                }
                catch (Exception)
                {
                    FormInbox.value = 0;
                }
                finally
                {
                    this.Dispose();
                }


            }
        }
    }
}
