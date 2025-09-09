using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using static MainForm;
using System.Drawing;
using System.Diagnostics;
using System.Threading;

public class MainFormMDI : Form
{
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
            svgconfig.pr.AddParam(new ParamListItemControl("minimo passo", txtMPasso, 20));
            svgconfig.pr.AddParam(new ParamListItemControl("profondita ", txtProf, 20));
            svgconfig.pr.AddParam(new ParamListItemControl("scala ", txtscale, 20));
            if (svgconfig.ShowDialog() == DialogResult.OK)
            {
                MainForm f = new MainForm();
                f.MdiParent = this;
                List<Gcode> data = new List<Gcode>();
                f.ImportSVG(ofd.FileName, double.Parse(txtMPasso.Text), double.Parse(txtProf.Text), double.Parse(txtscale.Text), data);
                f.filltext(data);
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
                f.Print(new Gcode("M5"));
                f.Print(new Gcode("M2"));
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
        List<Gcode> data = new List<Gcode>();
        f.ParseGcode(data);

        StreamReader fg = new StreamReader(FileName);
        string li;

        while ((li = fg.ReadLine()) != null)
        {
            if (li.Length == 0) continue;
            if (li[0] == '(') continue;
            Gcode gg = new Gcode(li);
            if ((gg.letter == 'G' && (gg.code == 0 || gg.code == 1)) || (gg.letter == 'M' && gg.code == 3)) data.Add(new Gcode(li));
        }

        fg.Close();
        f.filltext(data);
        f.Redraw();
        f.Show();
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

    private void onZoomP(object sender, EventArgs e)
    {
        MainForm f = (MainForm)this.ActiveMdiChild;
        if (f != null)
        {
            f.scaleX += 0.5F;
            f.scaleY += 0.5F;
            f.Redraw();
        }
    }
    private void onZoomM(object sender, EventArgs e)
    {
        MainForm f = (MainForm)this.ActiveMdiChild;
        if (f != null)
        {
            f.scaleX -= 0.5F;
            f.scaleY -= 0.5F;
            f.Redraw();
        }
    }

    private void onZoomS(object sender, EventArgs e)
    {
        MainForm f = (MainForm)this.ActiveMdiChild;
        if (f != null)
        {
            ParamListDialog config = new ParamListDialog("OK", "Gcode Viewer");
            TextBox txtMPasso = new TextBox();
            config.pr.AddParam(new ParamListItemControl("Scala", txtMPasso, 20));
            if (config.ShowDialog() == DialogResult.OK)
            {
                f.scaleX = float.Parse(txtMPasso.Text);
                f.scaleY = f.scaleX;
                f.Redraw();
            }
        }
    }

    private void onZoomDPI(object sender, EventArgs e)
    {
        MainForm f = (MainForm)this.ActiveMdiChild;
        if (f != null)
        {
            f.scaleX = this.DeviceDpi;
            f.scaleY = this.DeviceDpi;
            f.Redraw();
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
        mi.DropDownItems.Add("Nuovo", null, onNEW);
        mi.DropDownItems.Add("Carica", null, onLOAD);
        mi.DropDownItems.Add("Accoda", null, onAggiungi);
        mi.DropDownItems.Add("Salva", null, onSAVE);
        mi.DropDownItems.Add("importa File SVG", null, onSVG_imp);
        mi.DropDownItems.Add("importa File GBR", null, onGBR_imp);
        mi.DropDownItems.Add("esporta File SVG", null, onSVG_exp);

        ToolStripMenuItem mi1 = new ToolStripMenuItem("Visualizza");
        mme.Items.Add(mi1);
        mi1.DropDownItems.Add("Zoom+", null, onZoomP);
        mi1.DropDownItems.Add("Zoom-", null, onZoomM);
        mi1.DropDownItems.Add("Set Zoom", null, onZoomS);
        mi1.DropDownItems.Add("Set Zoom DPI", null, onZoomDPI);

        ToolStripMenuItem mi2 = new ToolStripMenuItem("Edita");
        mme.Items.Add(mi2);
        mi2.DropDownItems.Add("Crea Recinto Interno e Esterno ", null, onZoomP);
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
