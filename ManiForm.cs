using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.IO;
using System.Xml;


public class MainForm : Form
{
    private Font ff = new Font("arial", 10);
    private Pen pp = new Pen(Brushes.Black);
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.PictureBox disegno;
    private System.Windows.Forms.SplitContainer sc;
    public System.Windows.Forms.ListBox txtcode;
    public float scaleX = 20F;
    public float scaleY = 20F;

    //aggiornato dai <
    public float CursorX = 0F;
    public float CursorY = 0F;
    public float SafeZZ = 1.0F;

    private int oldIndex=0;


    public class Gcode
    {
        public char letter = 'G';
        public int code;
        public double x;
        public double y;
        public double z;
        public double f;
        public double p;
        public double s;
        public bool vx;
        public bool vy;
        public bool vz;
        public bool vf;
        public bool vp;
        public bool vs;

        public Gcode()
        {
            x = 0;
            y = 0;
            z = 0;
            vx = false;
            vy = false;
            vz = false;
            code = -1;
        }

        public Gcode(string line)
        {
            while (line.Contains("  ")) line = line.Replace("  ", " ");
            string[] scode = line.Split(' ');
            letter = scode[0][0];
            code = Int32.Parse(scode[0].Substring(1));
            bool conv = ((1.5).ToString().Contains(","));
            for (int k = 1; k < scode.Length; k++)
            {
                if (conv) scode[k] = scode[k].Replace(".", ",");
                if (scode[k][0] == '(') return;
                switch (scode[k][0])
                {
                    case 's':
                    case 'S':
                        s = double.Parse(scode[k].Substring(1));
                        vs = true;
                        break;
                    case 'p':
                    case 'P':
                        p = double.Parse(scode[k].Substring(1));
                        vp = true;
                        break;
                    case 'f':
                    case 'F':
                        f = double.Parse(scode[k].Substring(1));
                        vf = true;
                        break;
                    case 'x':
                    case 'X':
                        x = double.Parse(scode[k].Substring(1));
                        vx = true;
                        break;
                    case 'y':
                    case 'Y':
                        y = double.Parse(scode[k].Substring(1));
                        vy = true;
                        break;
                    case 'z':
                    case 'Z':
                        z = double.Parse(scode[k].Substring(1));
                        vz = true;
                        break;
                }
            }
        }

        public Gcode(double xx, double yy)
        {
            x = xx;
            y = yy;
            z = 0;
            vx = true;
            vy = true;
            vz = false;
            code = 1;
            letter = 'G';
        }

        public Gcode(double zz)
        {
            x = 0;
            y = 0;
            z = zz;
            vx = false;
            vy = false;
            vz = true;
            code = 1;
            letter = 'G';
        }

        public bool Equals(double xx, double yy)
        {
            return ToString() == (new Gcode(xx, yy)).ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }
            Gcode o = (Gcode)obj;
            /*if(o.code!=code)return false;
            if(vx!=o.vx || vy!=o.vy || vz!=o.vz)return false;
            if(o.vx && vx)if(o.x!=x)return false;
            if(o.vy && vy)if(o.y!=y)return false;
            if(o.vz && vz)if(o.z!=z)return false;*/
            return ToString() == o.ToString();
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            string str = letter + code.ToString();
            if (vx) str += " X" + x.ToString("F2").Replace(",", ".");
            if (vy) str += " Y" + y.ToString("F2").Replace(",", ".");
            if (vz) str += " Z" + z.ToString("F2").Replace(",", ".");
            if (vf) str += " F" + f.ToString("F2").Replace(",", ".");
            if (vp) str += " P" + p.ToString("F2").Replace(",", ".");
            if (vs) str += " S" + s.ToString("F2").Replace(",", ".");
            return str;
        }
    }

    private void onFrmLoad(object sender, EventArgs e)
    {
        Redraw();
    }



    public async void ExportToSVG(String FileName)
    {
        StreamWriter sw = new StreamWriter(FileName);
        sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        sw.WriteLine("<svg style=\"fill:none;stroke-width:0.5;stroke-linecap:round;stroke-linejoin:round;stroke:rgb(0%,0%,0%);stroke-opacity:1;stroke-miterlimit:10;\" >");

        string path = string.Empty;
        bool scava = false;
        int nline = 0;
        foreach (Gcode gc in txtcode.Items)
        {
            if (gc.code != 1 && gc.code != 0) continue;
            if (gc.vz)
            {
                scava = (gc.z < 0.0);
            }
            if (gc.vx && gc.vy)
            {
                if (nline > 0) path += " ";
                if (scava) path += "L " + gc.x.ToString("#.00").Replace(',', '.') + "," + gc.y.ToString("#.00").Replace(',', '.');
                else path += "M " + gc.x.ToString("#.00").Replace(',', '.') + "," + gc.y.ToString("#.00").Replace(',', '.');
                nline++;
            }
        }

        sw.WriteLine("<path d=\"" + path + "\"/>");
        sw.WriteLine("</svg>");
        sw.Flush();
        sw.Close();
    }

    public void Coppia2_Beize(
            double x0, double y0,
            double x1, double y1,
            double x2, double y2,
            double x3, double y3,
            double step)
    {
        step = 1.0 / step;
        double x, y;
        for (double i = 0; i < 1.0; i += step)
        {
            x = Math.Pow(1.0 - i, 3) * x0 + 3.0 * i * Math.Pow(1.0 - i, 2) * x1 + 3.0 * (1.0 - i) * Math.Pow(i, 2) * x2 + Math.Pow(i, 3) * x3;
            y = Math.Pow(1.0 - i, 3) * y0 + 3.0 * i * Math.Pow(1.0 - i, 2) * y1 + 3.0 * (1.0 - i) * Math.Pow(i, 2) * y2 + Math.Pow(i, 3) * y3;
            if (!((Gcode)txtcode.Items[txtcode.Items.Count - 1]).Equals(x, y)) txtcode.Items.Add(new Gcode(x, y));
        }
        if (!((Gcode)txtcode.Items[txtcode.Items.Count - 1]).Equals(x3, y3)) txtcode.Items.Add(new Gcode(x3, y3));
    }

    public void Coppia2_Beize(
            double x0, double y0,
            double x1, double y1,
            double x2, double y2,
            double step)
    {
        step = 1.0 / step;
        double x, y;
        for (double i = 0; i < 1; i += step)
        {
            x = Math.Pow(1.0 - i, 2) * x0 + 2.0 * (1.0 - i) * i * x1 + Math.Pow(i, 2) * x2;
            y = Math.Pow(1.0 - i, 2) * y0 + 2.0 * (1.0 - i) * i * y1 + Math.Pow(i, 2) * y2;
            if (!((Gcode)txtcode.Items[txtcode.Items.Count - 1]).Equals(x, y)) txtcode.Items.Add(new Gcode(x, y));
        }
        if (!((Gcode)txtcode.Items[txtcode.Items.Count - 1]).Equals(x2, y2)) txtcode.Items.Add(new Gcode(x2, y2));
    }

    public void Coppia2_Linea(
            double x0, double y0,
            double x1, double y1,
            double dist)
    {
        double norma = Math.Sqrt((x1 - x0) * (x1 - x0) + (y1 - y0) * (y1 - y0));
        double np = norma / dist;
        np = 3;
        double step = 1.0 / np;
        double x, y;
        for (double i = 0; i < 1; i += step)
        {
            x = x0 * (1 - i) + x1 * (i);
            y = y0 * (1 - i) + y1 * (i);
            Gcode dd = (Gcode)txtcode.Items[txtcode.Items.Count - 1];
            if (!dd.Equals(x, y)) txtcode.Items.Add(new Gcode(x, y));
        }
        if (!((Gcode)txtcode.Items[txtcode.Items.Count - 1]).Equals(x1, y1)) txtcode.Items.Add(new Gcode(x1, y1));
    }

    public async void ImportSVG(String SvgFileName, double passo, double dep, double scale)
    {
        Text = SvgFileName;
        //data.Add(new Gcode(0,0));   
        txtcode.Items.Add(new Gcode("G90")); //COOCRDINATE ASSOLUTE
        txtcode.Items.Add(new Gcode("M03"));
        txtcode.Items.Add(new Gcode("G01 F200.00000"));

        XmlDocument xmldoc = new XmlDocument();
        xmldoc.Load(SvgFileName);
        XmlNodeList pl = xmldoc.DocumentElement.GetElementsByTagName("path");

        char curCom = 'z';
        List<string> vs = new List<string>();

        foreach (XmlElement path in pl)
        {
            string trans = path.GetAttribute("transform");
            string sp = path.GetAttribute("d").Trim();
            while (sp.Contains("  ")) sp = sp.Replace("  ", " ");
            sp = sp.Replace(",", " ");
            sp = sp.Replace(";", " ");
            sp = sp.Replace("\n", " ");
            vs.Clear();
            vs.AddRange(sp.Split(' '));

            if ((1.5).ToString().Contains(","))
            {
                for (int k = 0; k < vs.Count; k++)
                {
                    vs[k] = vs[k].Replace(".", ",");
                }
            }

            double x = 0;
            double y = 0;
            double xs = 0;
            double ys = 0;

            double a;
            double b;
            double c;
            double d;
            double e;
            double f;

            double aa = scale;
            double bb = 0.0;
            double cc = 0.0;
            double dd = scale;
            double ee = 0.0;
            double ff = 0.0;

            if (trans.StartsWith("matrix"))
            {
                int st = trans.IndexOf('(') + 1;
                int en = trans.IndexOf(')');
                string[] mat = trans.Substring(st, en - st).Split(',');
                aa = Double.Parse(mat[0]);
                bb = Double.Parse(mat[1]);
                cc = Double.Parse(mat[2]);
                dd = Double.Parse(mat[3]);
                ee = Double.Parse(mat[4]);
                ff = Double.Parse(mat[5]);
            }

            if (trans.StartsWith("scale"))
            {
                int st = trans.IndexOf('(') + 1;
                int en = trans.IndexOf(')');
                string[] mat = trans.Substring(st, en - st).Split(',');
                aa = Double.Parse(mat[0]);
                dd = Double.Parse(mat[1]);
            }

            if (trans.StartsWith("translate"))
            {
                int st = trans.IndexOf('(') + 1;
                int en = trans.IndexOf(')');
                string[] mat = trans.Substring(st, en - st).Split(',');
                ee = Double.Parse(mat[0]);
                ff = Double.Parse(mat[1]);
            }


            while (vs.Count > 0)
            {
                Console.WriteLine("process " + vs[0] + " " + vs.Count);
                if (vs[0].Length == 0) vs.RemoveAt(0);
                switch (vs[0][0])
                {
                    case 'M':
                    case 'm':
                        curCom = vs[0][0];
                        vs.RemoveAt(0);
                        a = Double.Parse(vs[0]);
                        vs.RemoveAt(0);
                        b = Double.Parse(vs[0]);
                        vs.RemoveAt(0);
                        a = a * aa + b * cc + ee;
                        b = a * bb + b * dd + ff;
                        if (curCom == 'm')
                        {
                            x += a;
                            y += b;
                            curCom = 'l';
                        }
                        if (curCom == 'M')
                        {
                            x = a;
                            y = b;
                            curCom = 'L';
                        }
                        txtcode.Items.Add(new Gcode(5));
                        txtcode.Items.Add(new Gcode(x, y));
                        txtcode.Items.Add(new Gcode(-1 * dep));
                        xs = x;
                        ys = y;
                        break;
                    case 'z':
                    case 'Z':
                        curCom = vs[0][0];
                        Coppia2_Linea(x, y, xs, ys, passo);
                        x = xs;
                        y = ys;
                        vs.RemoveAt(0);
                        break;
                    case 'l':
                    case 'L':
                    case 'v':
                    case 'V':
                    case 'h':
                    case 'H':
                    case 'q':
                    case 'Q':
                    case 'c':
                    case 'C':
                        curCom = vs[0][0];
                        vs.RemoveAt(0);
                        break;
                    case '-':
                    case '+':
                    case '.':
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                        if (curCom == 'l' || curCom == 'L')
                        {
                            a = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            b = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            a = a * aa + b * cc + ee;
                            b = a * bb + b * dd + ff;
                            if (curCom == 'l')
                            {
                                Coppia2_Linea(x, y, x + a, y + b, passo);
                                x += a;
                                y += b;
                            }
                            if (curCom == 'L')
                            {
                                Coppia2_Linea(x, y, a, b, passo);
                                x = a;
                                y = b;
                            }
                        }
                        if (curCom == 'q' || curCom == 'Q')
                        {
                            a = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            b = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            c = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            d = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            a = a * aa + b * cc + ee;
                            b = a * bb + b * dd + ff;
                            if (curCom == 'q')
                            {
                                Coppia2_Beize(x, y, a + x, b + y, c + x, d + y, passo);
                                x += c;
                                y += d;
                            }
                            if (curCom == 'Q')
                            {
                                Coppia2_Beize(x, y, a, b, c, d, passo);
                                x = c;
                                y = d;
                            }
                        }
                        if (curCom == 'c' || curCom == 'C')
                        {
                            a = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            b = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            c = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            d = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            e = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            f = Double.Parse(vs[0]);
                            vs.RemoveAt(0);
                            a = a * aa + b * cc + ee;
                            b = a * bb + b * dd + ff;
                            if (curCom == 'c')
                            {
                                Coppia2_Beize(x, y, a + x, b + y, c + x, d + y, e + x, f + y, passo);
                                x += e;
                                y += f;
                            }
                            if (curCom == 'C')
                            {
                                Coppia2_Beize(x, y, a, b, c, d, e, f, passo);
                                x = e;
                                y = f;
                            }
                        }
                        if (curCom == 'h' || curCom == 'H' || curCom == 'v' || curCom == 'V')
                        {
                            a = Double.Parse(vs[0]);
                            a = a * dd + ff;
                            vs.RemoveAt(0);
                            if (curCom == 'h')
                            {
                                Coppia2_Linea(x, y, x + a, y, passo);
                                x += a;
                            }
                            if (curCom == 'H')
                            {
                                Coppia2_Linea(x, y, a, y, passo);
                                x = a;
                            }
                            if (curCom == 'v')
                            {
                                Coppia2_Linea(x, y, x, y + a, passo);
                                y += a;
                            }
                            if (curCom == 'V')
                            {
                                Coppia2_Linea(x, y, x, a, passo);
                                y = a;
                            }
                        }
                        break;
                    default:
                        Console.WriteLine("error " + vs[0]);
                        vs.RemoveAt(0);
                        break;
                }

            }
        }

        txtcode.Items.Add(new Gcode(5));
        txtcode.Items.Add(new Gcode(0, 0));
        txtcode.Items.Add(new Gcode("M5"));
        txtcode.Items.Add(new Gcode("M9"));
        txtcode.Items.Add(new Gcode("M2"));
        Console.WriteLine("end import svg");
    }

    public async void LoadGC(string FileName)
    {
        Text = FileName;
        StreamReader fg = new StreamReader(FileName);
        string li;
        while ((li = fg.ReadLine()) != null)
        {
            if (li.Length == 0) continue;
            if (li[0] == '(') continue;
            txtcode.Items.Add(new Gcode(li));
        }
    }

    public void SaveGcode(string Filename)
    {
        StreamWriter sw = new StreamWriter(Filename);
        foreach (Gcode gc in txtcode.Items) sw.WriteLine(gc.ToString());
        sw.Flush();
        sw.Close();
        Text = Filename;
    }

    public void ImportDrill(string FileName, double dep, double scale)
    {
        StreamReader fg = new StreamReader(FileName);
        string li;
        bool conv = ((1.5).ToString().Contains(","));
        while ((li = fg.ReadLine()) != null)
        {
            if (conv) li = li.Replace(".", ",");
            if (li.Length == 0) continue;
            if (li[0] != 'X') continue;
            int py = li.IndexOf('Y');
            if (py == -1) continue;
            double x = Double.Parse(li.Substring(1, py - 1));
            double y = Math.Abs(Double.Parse(li.Substring(py + 1)));
            txtcode.Items.Add(new Gcode(5));
            txtcode.Items.Add(new Gcode(x * scale, y * scale));
            txtcode.Items.Add(new Gcode(-1 * dep));
        }
        txtcode.Items.Add(new Gcode(5));
    }


    public async void Redraw()
    {
        if (txtcode.Items.Count == 0) return;
        Pen blackPen = new Pen(Color.Red, 2);
        Pen blackGreen = new Pen(Color.Green, 2);
        Pen cPen = blackPen;

        double minx = Double.MaxValue;
        double miny = Double.MaxValue;
        double maxx = Double.MinValue;
        double maxy = Double.MinValue;

        foreach (Gcode gc in txtcode.Items)
        {
            if (gc.vx)
            {
                if (gc.x < minx)
                {
                    minx = gc.x;
                }
                if (gc.x > maxx)
                {
                    maxx = gc.x;
                }
            }
            if (gc.vy)
            {
                if (gc.y < miny)
                {
                    miny = gc.y;
                }
                if (gc.y > maxy)
                {
                    maxy = gc.y;
                }
            }
        }

        int margin = 3;
        Bitmap bmp = new Bitmap((int)((maxx - minx + margin * 2) * scaleX), (int)((maxy - miny + margin * 2) * scaleY));
        Graphics g = Graphics.FromImage(bmp);
        g.FillRectangle(Brushes.White, 0, 0, bmp.Width, bmp.Height);
        g.DrawString("Dimansion " + (maxx - minx) + "x" + (maxy - miny), this.Font, Brushes.Black, 0, 0);
        double xo = 0;
        double yo = 0;
        Brush br = Brushes.Black;
        int rr = 3;

        foreach (Gcode gc in txtcode.Items)
        {
            if (gc.Equals(txtcode.SelectedItem))
            {
                br = Brushes.Yellow;
                rr = 10;
            }
            else
            {
                br = Brushes.Black;
                rr = 3;
            }

            if (gc.vz)
            {
                if (gc.z < 0.1)
                {
                    g.DrawString(gc.z.ToString("F2"), this.Font, Brushes.Black, (int)(xo * scaleX), (int)(yo * scaleY));
                    cPen = blackPen;
                }
                else cPen = blackGreen;
            }
            if (gc.vx && gc.vy)
            {
                double x = margin + gc.x - minx;
                double y = margin + gc.y - miny;
                if (x > 0.0 && y > 0.0)
                {
                    g.DrawLine(cPen,
                    (int)(xo * scaleX),
                    (int)(yo * scaleY),
                    (int)(x * scaleX),
                    (int)(y * scaleY));
                    g.FillEllipse(br,
                    (int)(x * scaleX) - rr,
                    (int)(y * scaleY) - rr, 2*rr, 2*rr);
                    xo = x;
                    yo = y;
                }
            }
        }

        g.FillEllipse(Brushes.Red,
            (int)(CursorX * scaleX) - 10,
            (int)(CursorY * scaleY) - 10, 2 * 10, 2 * 10);

        g.Dispose();
        //disegno.BackgroundImage = bmp;
        disegno.Image = bmp;
    }

    public void onselect(object sender,EventArgs e)
    {
        Redraw();
    }

    public void selectnext()
    {
        oldIndex = txtcode.SelectedIndex;
        txtcode.SelectedIndex++;
    }

    public void onkey(object sender, KeyEventArgs e)
    {
        MainFormMDI o = (MainFormMDI)MdiParent;
        switch (e.KeyCode)
        {
            case Keys.Space:
                if (Math.Abs(oldIndex - txtcode.SelectedIndex) > 1)
                {
                    o.SendGcode(new Gcode(SafeZZ));
                    o.SendGcode((Gcode)txtcode.SelectedItem);
                    o.SendGcode(new Gcode(0.1F));
                }
                else
                {
                    o.SendGcode((Gcode)txtcode.SelectedItem);
                }
                selectnext();
                break;
            case Keys.U:
                o.SendGcode(new Gcode(SafeZZ));
                break;
            case Keys.D:
                o.SendGcode(new Gcode(0.1F));
                break;
            case Keys.R:
                o.SendGcode(new Gcode("M3"));
                break;
            case Keys.S:
                o.SendGcode(new Gcode("M5"));
                break;
        }
    }

    public void onConvertLaser(object sender, EventArgs e)
    {
        Redraw();
    }

    private void onZoomP(object sender, EventArgs e)
    {
        scaleX += 0.5F;
        scaleY += 0.5F;
        Redraw();
    }
    private void onZoomM(object sender, EventArgs e)
    {
        scaleX -= 0.5F;
        scaleY -= 0.5F;
        Redraw();
    }

    private void onZoomS(object sender, EventArgs e)
    {
        ParamListDialog config = new ParamListDialog("OK", "Gcode Viewer");
        TextBox txtMPasso = new TextBox();
        config.pr.AddParam(new ParamListItemControl("Scala", txtMPasso, 20));
        if (config.ShowDialog() == DialogResult.OK)
        {
            scaleX = float.Parse(txtMPasso.Text);
            scaleY = scaleX;
            Redraw();
        }  
    }

    private void onZoomDPI(object sender, EventArgs e)
    {
        scaleX = this.DeviceDpi;
        scaleY = this.DeviceDpi;
        Redraw();
    }

    private void InitializeComponent()
    {

        disegno = new PictureBox();
        //disegno.Dock = DockStyle.Fill;
        disegno.SizeMode = PictureBoxSizeMode.AutoSize;
        //disegno.HorizontalScroll.Enabled=true;
        //disegno.HorizontalScroll.Visible=true;
        //disegno.VerticalScroll.Enabled=true;
        //disegno.VerticalScroll.Visible=true;  
        //disegno.AutoScroll=true;
        //disegno.BackgroundImageLayout= ImageLayout.Center;

        sc = new SplitContainer();
        sc.Dock = DockStyle.Fill;
        txtcode = new ListBox();
        txtcode.Dock = DockStyle.Fill;
        sc.Panel2.Controls.Add(disegno);
        sc.Panel2.AutoScroll = true;
        sc.Panel1.Controls.Add(txtcode);

        MenuStrip mme = new MenuStrip();

        ToolStripMenuItem mi1 = new ToolStripMenuItem("Visualizza");
        mme.Items.Add(mi1);
        mi1.DropDownItems.Add("Zoom+", null, onZoomP);
        mi1.DropDownItems.Add("Zoom-", null, onZoomM);
        mi1.DropDownItems.Add("Set Zoom", null, onZoomS);
        mi1.DropDownItems.Add("Set Zoom DPI", null, onZoomDPI);

        ToolStripMenuItem mi2 = new ToolStripMenuItem("Edit");
        mi2.DropDownItems.Add("Convert to Laser", null, onConvertLaser);
        mme.Items.Add(mi2);


        this.MainMenuStrip = mme;
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Text = "Gcode Viewer";
        this.Controls.Add(sc);
        this.Controls.Add(mme);
    }

    public MainForm()
    {
        InitializeComponent();
        this.Load += new EventHandler(onFrmLoad);
        txtcode.SelectedIndexChanged += new EventHandler(onselect);
        txtcode.KeyDown += new KeyEventHandler(onkey);
    }
}
