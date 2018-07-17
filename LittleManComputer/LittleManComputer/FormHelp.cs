using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace LittleManComputer
{
    public partial class FormHelp : Form
    {
        public FormHelp()
        {
            InitializeComponent();
        }

        private void FormHelp_Load(object sender, EventArgs e)
        {
            try
            {
                webBrowser1.Navigate("http://en.wikipedia.org/w/index.php?title=Little_man_computer&printable=yes");
            }
            catch (Exception)
            {
                this.Dispose();
            }
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            try
            {
                this.Text = webBrowser1.Document.Title;
            }
            catch (Exception)
            {
            }
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            try
            {
                this.Text = webBrowser1.Document.Title;
            }
            catch (Exception)
            {
            }

        }
    }
}
