using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Threading;

namespace LittleManComputer
{
    public partial class FormLMC : Form
    {
        public FormLMC()
        {
            InitializeComponent();
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FormHelp f = new FormHelp();
                f.Show();
            }
            catch (Exception)
            {
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Dispose();
        }

        private void FormLMC_Load(object sender, EventArgs e)
        {
            try
            {
                dgvSource.ColumnCount = 4;
                dgvSource.Columns[0].Name = "Memory";
                dgvSource.Columns[1].Name = "Name";
                dgvSource.Columns[2].Name = "Instr.";
                dgvSource.Columns[3].Name = "Value";

                dgvSource.Columns[0].ValueType = typeof(int);
                dgvSource.Columns[1].ValueType = typeof(string);
                dgvSource.Columns[2].ValueType = typeof(string);
                dgvSource.Columns[3].ValueType = typeof(string);

                ((DataGridViewTextBoxColumn)dgvSource.Columns[2]).MaxInputLength = 3;

                dgvSource.Columns[0].ReadOnly = true;
                dgvSource.Columns[1].ReadOnly = false;
                dgvSource.Columns[2].ReadOnly = false;
                dgvSource.Columns[3].ReadOnly = false;

                dgvBin.ColumnCount = 2;
                dgvBin.Columns[0].Name = "Memory";
                dgvBin.Columns[1].Name = "ICode";

                dgvBin.Columns[0].ValueType = typeof(int);
                dgvBin.Columns[1].ValueType = typeof(int);

                ((DataGridViewTextBoxColumn) dgvBin.Columns[1]).MaxInputLength = 3;

                dgvBin.Columns[0].ReadOnly = true;
                dgvBin.Columns[1].ReadOnly = true;

                for (int i = 0; i < 100; i++)
                {
                    dgvSource.Rows.Add(new object[] { i, "", "", "" });
                    dgvBin.Rows.Add(new object[] { i, 0 });

                    Application.DoEvents();
                }
            }
            catch (Exception)
            {
            }
        }

        private void VerifyCell()
        {
            foreach (DataGridViewRow rw in dgvSource.Rows)
            {
                foreach (DataGridViewCell cl in rw.Cells)
                {
                    if (cl.ValueType == typeof(string))
                    {
                        try
                        {
                            string temp = (string)cl.Value;
                            if(temp == null)
                                throw new Exception("null");
                        }
                        catch (Exception)
                        {
                            cl.Value = " ";
                        }
                    }

                    Application.DoEvents();
                }
            }
        }

        private void runToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clear();
                Err("Application started...");
                dgvSource.Columns[1].ReadOnly = true;
                dgvSource.Columns[2].ReadOnly = true;
                dgvSource.Columns[3].ReadOnly = true;
                Application.DoEvents();

                VerifyCell();
                Assemble();
                Execute();

            }
            catch (Exception ex)
            {
                Err(ex.Message);
            }
            finally
            {
                dgvSource.Columns[1].ReadOnly = false;
                dgvSource.Columns[2].ReadOnly = false;
                dgvSource.Columns[3].ReadOnly = false;
            }
        }

        private void Assemble()
        {
            Err("Assembling code...");
            
            // Detect Memory Name
            Hashtable label = new Hashtable();

            foreach (DataGridViewRow rw in dgvSource.Rows)
            {
                int addr = (int) rw.Cells[0].Value;
                string lbl = rw.Cells[1].Value.ToString().Trim();


                if (lbl.Length > 0)
                {
                    int tempi;
                    if (int.TryParse(lbl, out tempi))
                    {
                        throw new Exception(string.Format("Error at {0} : Label name cannot be an Integer.", addr));
                    }

                    if (label.Count < 1)
                    {
                        label.Add(lbl, addr);
                    }
                    else if (label.Contains(lbl))
                    {
                        throw new Exception(string.Format("Error at {0} : {1} label already in use ({1} = {2})", addr, lbl, label[lbl]));
                    }
                    else
                    {
                        label.Add(lbl, addr);
                    }
                }

                string instr = rw.Cells[2].Value.ToString().ToUpper();
                string val = rw.Cells[3].Value.ToString().Trim();

                if ((instr == "DAT") || (instr.Length<1))
                {
                    int rval = 0;
                    try
                    {
                        rval = int.Parse(val);

                    }
                    catch (Exception)
                    {

                    }
                    
                    if ((rval / 1000) != 0)
                    {
                            throw new Exception(string.Format("Error at {0} : Data value must be within 000 to 999", addr));
                    }

                    if (rval < 0)
                    {
                        throw new Exception(string.Format("Error at {0} : Data value must be positive. Negative value not allowed", addr));
                    }

                    dgvBin.Rows[addr].Cells[1].Value = (object) rval;
                }


                Application.DoEvents();
            }


            // Assemble instruction
            foreach (DataGridViewRow rw in dgvSource.Rows)
            {
                int addr = (int)rw.Cells[0].Value;

                string instr = rw.Cells[2].Value.ToString().Trim().ToUpper();
                string val = rw.Cells[3].Value.ToString().Trim();

                if (instr == "ADD")
                {
                    if (val == "")
                    {
                        throw new Exception(string.Format("Error at {0} : Data address required", addr));
                    }
                    if ((label.Count>0) && label.Contains(val))
                    {
                        int mn = (int)label[val];
                        mn += 100;
                        dgvBin.Rows[addr].Cells[1].Value = (object)mn;
 
                    }
                    else
                    {
                        throw new Exception(string.Format("Error at {0} : Cannot find label {1}", addr, val));
                    }
                }


                else if (instr == "SUB")
                {
                    if (val == "")
                    {
                        throw new Exception(string.Format("Error at {0} : Data address required", addr));
                    }

                    if ((label.Count > 0) && label.Contains(val))
                    {
                        int mn = (int)label[val];
                        mn += 200;
                        dgvBin.Rows[addr].Cells[1].Value = (object)mn;

                    }
                    else
                    {
                        throw new Exception(string.Format("Error at {0} : Cannot find label {1}", addr, val));
                    }
                }

                else if (instr == "STA")
                {
                    if (val == "")
                    {
                        throw new Exception(string.Format("Error at {0} : Data address required", addr));
                    }

                    if ((label.Count > 0) && label.Contains(val))
                    {
                        int mn = (int)label[val];
                        mn += 300;
                        dgvBin.Rows[addr].Cells[1].Value = (object)mn;

                    }
                    else
                    {
                        throw new Exception(string.Format("Error at {0} : Cannot find label {1}",addr,  val));
                    }
                }

                else if (instr == "LDA")
                {
                    if (val == "")
                    {
                        throw new Exception(string.Format("Error at {0} : Data address required", addr));
                    }

                    if ((label.Count > 0) && label.Contains(val))
                    {
                        int mn = (int)label[val];
                        mn += 500;
                        dgvBin.Rows[addr].Cells[1].Value = (object)mn;

                    }
                    else
                    {
                        throw new Exception(string.Format("Error at {0} : Cannot find label {1}", addr, val));
                    }
                }

                else if (instr == "BRA")
                {
                    if (val == "")
                    {
                        throw new Exception(string.Format("Error at {0} : Address required", addr));
                    }

                    if ((label.Count > 0) && label.Contains(val))
                    {
                        int mn = (int)label[val];
                        mn += 600;
                        dgvBin.Rows[addr].Cells[1].Value = (object)mn;

                    }
                    else
                    {
                        throw new Exception(string.Format("Error at {0} : Cannot find label {1}", addr, val));
                    }
                }

                else if (instr == "BRZ")
                {
                    if (val == "")
                    {
                        throw new Exception(string.Format("Error at {0} : Address required", addr));
                    }

                    if ((label.Count > 0) && label.Contains(val))
                    {
                        int mn = (int)label[val];
                        mn += 700;
                        dgvBin.Rows[addr].Cells[1].Value = (object)mn;

                    }
                    else
                    {
                        throw new Exception(string.Format("Error at {0} : Cannot find label {1}",addr, val));
                    }
                }

                else if (instr == "BRP")
                {
                    if (val == "")
                    {
                        throw new Exception(string.Format("Error at {0} : Address required", addr));
                    }

                    if ((label.Count > 0) && label.Contains(val))
                    {
                        int mn = (int)label[val];
                        mn += 800;
                        dgvBin.Rows[addr].Cells[1].Value = (object)mn;

                    }
                    else
                    {
                        throw new Exception(string.Format("Error at {0} : Cannot find label {1}", addr, val));
                    }
                }

                else if (instr == "INP")
                {
                    if (val != "")
                    {
                        throw new Exception(string.Format("Error at {0} : Data Address is not required here", addr));
                    }

                    int mn = 901;
                    dgvBin.Rows[addr].Cells[1].Value = (object)mn;

                }

                else if (instr == "OUT")
                {
                    if (val != "")
                    {
                        throw new Exception(string.Format("Error at {0} : Data Address is not required here", addr));
                    }

                    int mn = 902;
                    dgvBin.Rows[addr].Cells[1].Value = (object)mn;

                }
                else if (instr == "HLT")
                {
                    if (val != "")
                    {
                        throw new Exception(string.Format("Error at {0} : Data Address is not required here", addr));
                    }

                    int mn = 0;
                    dgvBin.Rows[addr].Cells[1].Value = (object)mn;

                }
                else if (instr == "COB")
                {
                    if (val != "")
                    {
                        throw new Exception(string.Format("Error at {0} : Data Address is not required here", addr));
                    }

                    int mn = 0;
                    dgvBin.Rows[addr].Cells[1].Value = (object)mn;

                }
                else if (instr == "")
                {
                }
                else if (instr == "DAT")
                {
                }
                else
                {
                    throw new Exception(string.Format("Error at {0} : Unknown instruction.", addr));
                }

                Application.DoEvents();
            }



            Err("Code assembled successfully.");

        }

        private delegate void _Clear();
        private void Clear()
        {
            if (InvokeRequired)
            {
               
                this.Invoke((_Clear) Clear);
                return;
            }

            rtbError.Clear();
        }

        private void Err(string err)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(Err), new object[] { err });
                return;
            }


            try
            {
                StringBuilder sb = new StringBuilder();
                if (rtbError.Text.Trim().Length < 1)
                {
                    sb.AppendFormat("{0}", err);
                }
                else
                {
                    sb.AppendFormat("{0}\n", err);
                    sb.Append(rtbError.Text);
                }
                rtbError.Text = sb.ToString();
                Application.DoEvents();
            }
            catch (Exception )
            {
                Clear();
                Err(err);
            }
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clear();

                foreach (DataGridViewRow rw in dgvSource.Rows)
                {
                    rw.Cells[1].Value = (object) "";
                    rw.Cells[2].Value = (object)"";
                    rw.Cells[3].Value = (object)"";
                }

                foreach (DataGridViewRow rw in dgvBin.Rows)
                {
                    rw.Cells[1].Value = (object)0;
                }
            }
            catch (Exception )
            {
                
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                saveFileDialog1.ShowDialog(); 
            }
            catch (Exception ex)
            {
                Err(ex.Message);
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                StringBuilder sb = new StringBuilder();

                foreach (DataGridViewRow rw in dgvSource.Rows)
                {
                    foreach(DataGridViewCell cl in rw.Cells)
                    {
                        sb.AppendLine(cl.Value.ToString());
                    }
                }

                using (StreamWriter sw = new StreamWriter(saveFileDialog1.FileName))
                {
                    sw.Write(sb.ToString());
                    sw.Flush();
                    sw.Close();
                }
                
            }
            catch (Exception)
            {
            }
        }

        private void loadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                openFileDialog1.ShowDialog();
            }
            catch (Exception)
            {
            }
        }

        private void openFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            try
            {
                using (StreamReader sr = new StreamReader(openFileDialog1.FileName))
                {
                    while (!sr.EndOfStream)
                    {
                        int addr = int.Parse(sr.ReadLine());
                        string lbl = sr.ReadLine().Trim();
                        string instr = sr.ReadLine().Trim();
                        string val = sr.ReadLine().Trim();

                        dgvSource.Rows[addr].Cells[1].Value = (object) lbl;
                        dgvSource.Rows[addr].Cells[2].Value = (object) instr;
                        dgvSource.Rows[addr].Cells[3].Value = (object) val;
                    }
                    sr.Close();
                }
            }
            catch (Exception)
            {
            }

        }



        private Thread th = null;
        private void Execute()
        {
            Err("Executing...");

            if (th != null)
            {
                th.Abort();
                th = null;
            }

            th = new Thread(new ThreadStart(ThreadProc));
            th.Start();

        }


        private void SetPCAC(int pc, int ac)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate { SetPCAC(pc, ac); });
                return;
            }

            lblPC.Text = pc.ToString();
            lblAC.Text = ac.ToString();            
            Application.DoEvents();
        }

        private int MemRead(int addr)
        {
            int data = 0;
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    data = MemRead(addr);
                });
            }
            else
            {
                data = (int) dgvBin.Rows[addr].Cells[1].Value;
            }

            return data;
        }

        private void MemWrite(int addr, int val)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    MemWrite(addr, val);
                });
            }
            else
            {
                dgvBin.Rows[addr].Cells[1].Value = (object) val;
            }
        }

        private void Outbox(int value)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    Outbox(value);
                });

            }
            else
            {
                string str = string.Format("Out = \n\t{0}\n", value);
                Err(str);
                MessageBox.Show(str, "Outbox", MessageBoxButtons.OK);
            }
        }

        private int Inbox()
        {
            int data=0;

            if (InvokeRequired)
            {
                Invoke((MethodInvoker)delegate
                {
                    data = Inbox();
                });

            }
            else
            {
                FormInbox ib = new FormInbox();
                ib.ShowDialog();
                data = FormInbox.value;


                string str = string.Format("In = \n\t{0}\n", data);
                Err(str);
            }
            return data;
        }


        private void ThreadProc()
        {
            try
            {
                int ac=0;
                for (int pc = 0; pc < 100; )
                {
                    SetPCAC(pc, ac);
                    ac %= 1000;

                    int data = MemRead(pc);
                    pc++;
                    int instr = data/100;
                    int val = data % 100;

                    if (data == 000)
                    {
                        break;
                    }
                    else if (instr == 1)
                    {
                        ac += MemRead(val);
                    }
                    else if (instr == 2)
                    {
                        ac -= MemRead(val);
                    }
                    else if (instr == 3)
                    {
                        MemWrite(val, ac);
                    }
                    else if (instr == 5)
                    {
                        ac = MemRead(val);
                    }
                    else if (instr == 6)
                    {
                        pc = val;
                    }
                    else if (instr == 7)
                    {
                        if (ac == 0)
                        {
                            pc = val;
                        }
                    }
                    else if (instr == 8)
                    {
                        if (ac > 0)
                        {
                            pc = val;
                        }
                    }
                    else if (data == 901)
                    {
                        // INP
                        ac = Inbox();
                    }
                    else if (data == 902)
                    {
                        // OUT
                        Outbox(ac);
                    }

                    ac %= 1000;
                    SetPCAC(pc, ac);
                }
            }
            catch (Exception)
            {
            }
            finally
            {


                Err("Finished Execution.");
                th = null;
            }

        }

        private void breakToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if(th != null)
                    th.Abort();
            }
            catch (Exception )
            {
            }
        }

    }
}
