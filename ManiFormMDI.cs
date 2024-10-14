using System;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

public class MainFormMDI : Form
{
    private void onSVG_imp(object sender, EventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Filter = "Immagine Vettoriale|*.svg";
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            ParamListDialog svgconfig =new ParamListDialog("OK","Gcode Viewer");
            TextBox txtMPasso = new TextBox();
            svgconfig.pr.AddParam(new ParamListItemControl("mimo passo",txtMPasso,20));
            if(svgconfig.ShowDialog()== DialogResult.OK){
                MainForm f = new MainForm();
                f.MdiParent = this;
                f.ImportSVG(ofd.FileName,double.Parse(txtMPasso.Text));
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
            if(f!=null){
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
        mi.DropDownItems.Add("importa File SVG", null, onSVG_imp);
        mi.DropDownItems.Add("esporta File SVG", null, onSVG_exp);

        this.IsMdiContainer = true;
        this.MainMenuStrip = mme;
        this.Controls.Add(mme);
    }

    public MainFormMDI()
    {
        InitializeComponent();
    }
}
