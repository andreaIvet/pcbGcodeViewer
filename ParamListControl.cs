using System;
using System.Windows;
using System.Drawing;
using System.Windows.Forms;

public class ParamListItemControl : Panel
{
    public void _Layout(object sender, LayoutEventArgs e)
    {

        Control lbl = Controls[1];
        Control txt = Controls[0];
        int w = ClientRectangle.Width / 3 -2;
        int h = ClientRectangle.Height -2;
        lbl.Height = h;
        txt.Height = h;
        lbl.Width = w;
        txt.Width = w * 2;
        lbl.Left = 0;
        txt.Left = w;
        lbl.Top = 0;
        txt.Top = 0;
    }

    public ParamListItemControl(string desc, Control cc,int h) : base()
    {
        Height =h;
        Label lbl = new Label();
        lbl.Text = desc;
        lbl.TextAlign = ContentAlignment.TopRight;
        Controls.Add(cc);
        Controls.Add(lbl);
        this.Layout += new LayoutEventHandler(_Layout);
    }
}
public class ParamListControl : Control
{
    public int StartY = 10;
    public ParamListControl()
    {
        this.Layout += new LayoutEventHandler(_Layout);
    }
    public void AddParam(ParamListItemControl i)
    {
        Controls.Add(i);
    }

    public int h = 25;

    public void _Layout(object sender, LayoutEventArgs e)
    {

        int x = StartY;
        foreach (Control c in Controls)
        {
            c.Width = ClientRectangle.Width;
            c.Top = x;
            x += c.Height;
        }
    }
}

public class ParamListDialog : Form
{
    public ParamListControl pr = new ParamListControl();
    private Button btok = new Button();

    private void Bte(object sender, EventArgs e)
    {
        DialogResult = DialogResult.OK;
        Close();
    }

    public ParamListDialog(string buttoncap, string titolo)
    {
        Text = titolo;
        btok.Text = buttoncap;
        btok.Dock = DockStyle.Bottom;
        btok.Click += new EventHandler(Bte);
        pr.Dock = DockStyle.Fill;
        Controls.Add(btok);
        Controls.Add(pr);
    }
}

