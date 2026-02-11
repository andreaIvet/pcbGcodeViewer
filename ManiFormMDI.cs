using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.IO.Ports;
using static MainForm;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Drawing.Printing;
using static System.Runtime.CompilerServices.RuntimeHelpers;

public class MainFormMDI : Form
{
    public System.IO.Ports.SerialPort sp = new SerialPort();
    private ConsolleControl tlog = new ConsolleControl(7);
    private Queue<Gcode> connbuf = new Queue<Gcode>();
    public bool serialBusy = false;

    private void onSVG_imp(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "Immagine Vettoriale|*.svg";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            ParamListDialog svgconfig = new ParamListDialog("OK", "Gcode Viewer");
            TextBox txtMPasso = new TextBox();
            TextBox txtProf = new TextBox();
            TextBox txtscale = new TextBox();
            txtMPasso.Text = "5";
            txtProf.Text = "0,01";
            txtscale.Text = "1";
            svgconfig.pr.AddParam(new ParamListItemControl("max step", txtMPasso, 20));
            svgconfig.pr.AddParam(new ParamListItemControl("deep ", txtProf, 20));
            svgconfig.pr.AddParam(new ParamListItemControl("scale ", txtscale, 20));
            if (svgconfig.ShowDialog() == DialogResult.OK)
            {
                MainForm f = new MainForm();
                f.MdiParent = this;
                f.ImportSVG(ofd.FileName, double.Parse(txtMPasso.Text), double.Parse(txtProf.Text), double.Parse(txtscale.Text));
                f.Show();
            }
        }
    }

    private void onGBR_imp(object sender, EventArgs e)
    {
        /*OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "file Forature|*.drl";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            ParamListDialog svgconfig = new ParamListDialog("OK", "Gcode Viewer");
            TextBox txtProf = new TextBox();
            TextBox txtscale = new TextBox();
            txtProf.Text = "0,01";
            txtscale.Text = "1";
            svgconfig.pr.AddParam(new ParamListItemControl("profondita ", txtProf, 20));
            svgconfig.pr.AddParam(new ParamListItemControl("scala ", txtscale, 20));
            if (svgconfig.ShowDialog() == DialogResult.OK)
            {
                MainForm f = new MainForm();
                f.MdiParent = this;
                List<Gcode> data = new List<Gcode>();
                f.ImportDrill(ofd.FileName, double.Parse(txtProf.Text), double.Parse(txtscale.Text), data);
                f.filltext(data);
                f.Show();
            }
        }*/

        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "file GBR|*.gbr";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            if (ofd.FileName.Contains("-"))
            {
                MainForm f = (MainForm)this.ActiveMdiChild;
                if (f == null)
                {
                    f = new MainForm();
                    f.MdiParent = this;
                }

                string fndrill = ofd.FileName.Split('-')[0];
                string fn = ofd.FileName.Split('.')[0];
                Process pr = new Process();
                pr.StartInfo.FileName = "pcb2gcode";
                pr.StartInfo.Arguments = " --back " + ofd.FileName + " --drill " + fndrill + "-PTH.drl --back-output " + fn + "-bk.ngc --drill-output "+fn + "-drill.ngc";
                try
                {
                    Console.WriteLine(pr.StartInfo.FileName + pr.StartInfo.Arguments);
                    pr.Start();
                }
                catch (Exception ee)
                {
                    Console.WriteLine(ee);
                    return;
                }
                while (!pr.HasExited) Thread.Sleep(500);
                onAggiungi(fn + "-bk.ngc",f);
                onAggiungi(fn + "-drill.ngc",f);
                f.txtcode.Items.Add(new Gcode("M5"));
                f.txtcode.Items.Add(new Gcode("M2"));
                f.Show();
            }
        }
    }

    private void onSVG_exp(object sender, EventArgs e)
    {
        SaveFileDialog ofd = new SaveFileDialog();
        ofd.Filter = "Immagine Vettoriale|*.svg";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            MainForm f = (MainForm)this.ActiveMdiChild;
            if (f != null)
            {
                f.ExportToSVG(ofd.FileName);
            }
        }
    }

    private void onNEW(object sender, EventArgs e)
    {
        MainForm f = new MainForm();
        f.MdiParent = this;
        f.Show();
    }

    private void onLOAD(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "File gCode|*.ngc";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            MainForm f = new MainForm();
            f.MdiParent = this;
            f.LoadGC(ofd.FileName);
            f.Show();
        }
    }

    private void onAggiungi(string FileName,MainForm f)
    {
        StreamReader fg = new StreamReader(FileName);
        string li;
        f.txtcode.SuspendLayout();
        while ((li = fg.ReadLine()) != null)
        {
            if (li.Length == 0) continue;
            if (li[0] == '(') continue;
            Gcode gg = new Gcode(li);
            if ((gg.letter == 'G' && (gg.code == 0 || gg.code == 1)) || (gg.letter == 'M' && gg.code == 3)) f.txtcode.Items.Add(gg);
        }
        fg.Close();
        f.txtcode.ResumeLayout();
    }

    private void onAggiungi(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "File gCode|*.ngc";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            MainForm f = (MainForm)this.ActiveMdiChild;
            if (f == null)
            {
                f = new MainForm();
                f.MdiParent = this;
            }

            onAggiungi(ofd.FileName,f);
            f.Show();
        }
    }

    private void onSAVE(object sender, EventArgs e)
    {
        SaveFileDialog ofd = new SaveFileDialog();
        ofd.Filter = "File gCode|*.ngc";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            MainForm f = (MainForm)this.ActiveMdiChild;
            f.SaveGcode(ofd.FileName);
        }
    }


    private void onSetupSerialPort(object sender, EventArgs e)
    {

        string[] ports = SerialPort.GetPortNames();
        if (ports.Length == 0)
        {
            MessageBox.Show("Nessuna Porta Seriale", "GcodeViewer", MessageBoxButtons.OK);
            return;
        }

        ParamListDialog ComConfig = new ParamListDialog("ok", "Setup Serial");

        ComboBox cbComName = new ComboBox();
        ComboBox cbComVel = new ComboBox();
        ComboBox cbComPar = new ComboBox();

        cbComName.Items.AddRange(ports);
        cbComName.SelectedIndex = 0;
        cbComVel.Items.Add(4800);
        cbComVel.Items.Add(9600);
        cbComVel.Items.Add(19200);
        cbComVel.Items.Add(115200);
        cbComPar.Items.Add("None");
        cbComPar.Items.Add("Odd");
        cbComPar.Items.Add("Even");
        cbComPar.Items.Add("Mark");
        cbComPar.Items.Add("Space");
        cbComPar.SelectedIndex = 0;
        cbComVel.SelectedIndex = 2;

        ComConfig.pr.AddParam(new ParamListItemControl("name", cbComName, 25));
        ComConfig.pr.AddParam(new ParamListItemControl("baud", cbComVel, 25));
        ComConfig.pr.AddParam(new ParamListItemControl("parity", cbComPar, 25));

        if (ComConfig.ShowDialog() == DialogResult.OK)
        {
            sp.PortName = (string)cbComName.SelectedItem;
            sp.BaudRate = (int)cbComVel.SelectedItem;
            sp.Parity = (Parity)cbComPar.SelectedIndex;
            sp.Handshake = System.IO.Ports.Handshake.None;
            sp.WriteTimeout = 400;
            sp.ReadTimeout = 400;
            sp.StopBits = StopBits.One;
            sp.RtsEnable = false;
            sp.DtrEnable = false;
            sp.DataBits = 8;
            //sp.ParityReplace = 0x00;
        }
    }

    private void onSerialRecv(object sender,SerialDataReceivedEventArgs e)
    {
        if(e.EventType == SerialData.Eof)
        {
            string line = sp.ReadLine();
            tlog.Add("R"+line);

            if (line.StartsWith("ok"))
            {
                serialBusy = connbuf.Count > 0;
                if (connbuf.Count > 0)
                {
                    Gcode gc = connbuf.Dequeue();
                    tlog.Add("S:" + gc.ToString() + "\n");
                    sp.WriteLine(gc.ToString());
                }
            }
            else
            {
                connbuf.Clear();
                serialBusy = false;
            }

            if (line.StartsWith("<"))
            {
                MainForm f = (MainForm)ActiveMdiChild;
                //<Idle|MPos:0.000,0.000,0.000|FS:0.0,0> aggiorna la posizione
                string[] dd = line.Split('|');
                string[] pp = dd[1].Split(':');
                string[] vv = pp[1].Split(',');
                if ((0.1F).ToString().Contains(","))
                {
                    f.CursorX = float.Parse(vv[0].Replace('.', ',')); //non per america
                    f.CursorY = float.Parse(vv[1].Replace('.', ','));
                }
                else
                {
                    f.CursorX = float.Parse(vv[0]);
                    f.CursorY = float.Parse(vv[1]);
                }
                f.Redraw();
            }
        }
    }

    public void SendGcode(Gcode gc)
    {
        if(serialBusy)connbuf.Enqueue(gc);
        else
        {
            tlog.Add("S:" + gc.ToString() + "\n");
            if (sp.IsOpen) sp.WriteLine(gc.ToString());
            serialBusy = true;
        }

    }

    private void onstartComm(object sender, EventArgs e)
    {
        tlog.clear();
        tlog.Add("Start Comm");
        if (sp.IsOpen)
        {
            sp.Open();
            sp.WriteLine("");
            sp.WriteLine("");
        }
    }

    private void onstopComm(object sender, EventArgs e)
    {
        sp.Close();
    }

    private void onUpdateComm(object sender, EventArgs e)
    {
        tlog.Add("S:?");
        if (sp.IsOpen) sp.WriteLine("?");
    }

    private void onCreateGears(object sender, EventArgs e)
    {
        ParamListDialog gn = new ParamListDialog("Create", "Setup gears");
        
        TextBox txtr1 = new TextBox();
        TextBox txtr2 = new TextBox();
        TextBox txtrd = new TextBox();
        TextBox txtp = new TextBox();
        TextBox txtk1 = new TextBox();
        TextBox txts = new TextBox();
        TextBox txtdeep = new TextBox();

        txtr1.Text = "10";
        txtr2.Text = "20";
        txtk1.Text = "15";
        txtrd.Text = "1";
        txtp.Text = "3";
        txts.Text = "4";
        txtdeep.Text = "-1,0";

        gn.pr.AddParam(new ParamListItemControl("raggio 1", txtr1, 25));
        gn.pr.AddParam(new ParamListItemControl("raggio 2", txtr2, 25));
        gn.pr.AddParam(new ParamListItemControl("delta r", txtrd, 25));
        gn.pr.AddParam(new ParamListItemControl("r pignone", txtp, 25));
        gn.pr.AddParam(new ParamListItemControl("k", txtk1, 25));
        gn.pr.AddParam(new ParamListItemControl("step", txts, 25));
        gn.pr.AddParam(new ParamListItemControl("deep", txtdeep, 25));


        if (gn.ShowDialog() == DialogResult.OK)
        {
            double r1 = double.Parse(txtr1.Text);
            double r2 = double.Parse(txtr2.Text);
            double rd = double.Parse(txtrd.Text);
            double step1 = double.Parse(txts.Text)*2*Math.PI/360.0;
            double k1 = double.Parse(txtk1.Text);
            double rp = double.Parse(txtp.Text);
            double z = double.Parse(txtdeep.Text);
            double k2 = k1 / r1 * r2;
            double step2 = step1 / r2 * r1;
            int margin = 5;

            int y0 = (int)(r1 + rd);
            if(r2>r1) y0 = (int)(r2 + rd);
            y0 += margin;

            MainForm f = new MainForm();
            f.scaleX = 10;
            f.scaleY = 10;

            //g1
            f.txtcode.Items.Add(new Gcode(5));
            f.txtcode.Items.Add(new Gcode(margin + r1 * 2 + rd * 2, y0));
            f.txtcode.Items.Add(new Gcode(z));
            for (double a = 0; a < Math.PI * 2; a+= step1)
            {
                double x = (r1 + rd * Math.Cos(a * k1)) * Math.Cos(a);
                double y = (r1 + rd * Math.Cos(a * k1)) * Math.Sin(a);
                f.txtcode.Items.Add(new Gcode(margin + r1 + rd + x, y0 + y));
            }
            f.txtcode.Items.Add(new Gcode(margin + r1 * 2 + rd * 2, y0));
            f.txtcode.Items.Add(new Gcode(5));

            //pignone
            f.txtcode.Items.Add(new Gcode(margin + r1 + rd + rp, y0));
            f.txtcode.Items.Add(new Gcode(z));
            for (double a = 0; a < Math.PI / 2 * 3; a += Math.PI/5) {
                double x = rp * Math.Cos(a);
                double y = rp * Math.Sin(a);
                f.txtcode.Items.Add(new Gcode(margin + r1 + rd + x, y0 + y)); 
            }
            f.txtcode.Items.Add(new Gcode(margin + r1 + rd + rp, y0));
            f.txtcode.Items.Add(new Gcode(5));

            //pignone
            f.txtcode.Items.Add(new Gcode(margin * 2 + r1 * 2 + rd * 3 + rp + r2, y0));
            f.txtcode.Items.Add(new Gcode(z));
            for (double a = 0; a < Math.PI / 2 * 3; a += Math.PI / 5)
            {
                double x = rp * Math.Cos(a);
                double y = rp * Math.Sin(a);
                f.txtcode.Items.Add(new Gcode(margin * 2 + r1 * 2 + rd * 3 + r2+x, y0 + y));
            }
            f.txtcode.Items.Add(new Gcode(margin * 2 + r1 * 2 + rd * 3 + rp + r2, y0));
            f.txtcode.Items.Add(new Gcode(5));


            //g2
            f.txtcode.Items.Add(new Gcode(margin * 2 + r1 * 2 + rd * 4 + r2 * 2, y0));
            f.txtcode.Items.Add(new Gcode(z));
            for (double a = 0; a < Math.PI * 2; a += step2)
            {
                double x = (r2 + rd * Math.Cos(a * k2)) * Math.Cos(a);
                double y = (r2 + rd * Math.Cos(a * k2)) * Math.Sin(a);
                f.txtcode.Items.Add(new Gcode(margin * 2 + r1 * 2 + r2 + rd * 3 + x, y0 + y));
            }
            f.txtcode.Items.Add(new Gcode(margin * 2 + r1 * 2 + rd * 4 + r2 * 2, y0));
            f.txtcode.Items.Add(new Gcode(5));


            f.MdiParent = this;
            f.Show();
        }
    }

    private void InitializeComponent()
    {
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Text = "Gcode Viewer";

        MenuStrip mme = new MenuStrip();

        ToolStripMenuItem mi = new ToolStripMenuItem("File");
        mme.Items.Add(mi);
        mi.DropDownItems.Add("new", null, onNEW);
        mi.DropDownItems.Add("load", null, onLOAD);
        mi.DropDownItems.Add("append", null, onAggiungi);
        mi.DropDownItems.Add("Save", null, onSAVE);
        mi.DropDownItems.Add("import File SVG", null, onSVG_imp);
        mi.DropDownItems.Add("import File GBR", null, onGBR_imp);
        mi.DropDownItems.Add("export File SVG", null, onSVG_exp);
        mi.DropDownItems.Add("Create Gears", null, onCreateGears);

        ToolStripMenuItem mi2 = new ToolStripMenuItem("GBRL");
        mme.Items.Add(mi2);
        mi2.DropDownItems.Add("Serial Port Setup", null, onSetupSerialPort);
        mi2.DropDownItems.Add("Start", null, onstartComm);
        mi2.DropDownItems.Add("Stop", null, onstopComm);
        mi2.DropDownItems.Add("Update", null, onUpdateComm);

        tlog.Dock = DockStyle.Bottom;
        tlog.Height = 100;
        this.Controls.Add(tlog);

        sp.DataReceived += new SerialDataReceivedEventHandler(onSerialRecv);
        sp.NewLine = "\\r\\n";


        //per scavare un pista da un path 1 trova il punto medio della figura chiusa e sottrailo poi scala maggiore e minore diametro punta fresa
        // e poi riaggiungi il punto medio alle tre figure da un figura
        this.IsMdiContainer = true;
        this.MainMenuStrip = mme;
        this.Controls.Add(mme);
    }

    public MainFormMDI()
    {
        InitializeComponent();
    }
}
