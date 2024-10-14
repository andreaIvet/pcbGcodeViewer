using System;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Xml;


public class MainForm : Form
{
    private Font ff = new Font("arial",10);
    private Pen pp =new Pen(Brushes.Black); 
    private System.ComponentModel.IContainer components = null;
    private System.Windows.Forms.Panel disegno;
    private System.Windows.Forms.SplitContainer sc;
    public System.Windows.Forms.RichTextBox txtcode;
    

    private struct Gcode
    {
        public int code;
        public double x;
        public double y;
        public double z;
        public bool vx;
        public bool vy;
        public bool vz;

        public Gcode(){
            x=0;
            y=0;
            z=0;
            vx=false;
            vy=false;
            vz=false;
            code=-1;
        }

        public Gcode(double xx,double yy){
            x=xx;
            y=yy;
            z=0;
            vx=true;
            vy=true;
            vz=false;
            code=1;
        }
        
        public Gcode(double zz){
            x=0;
            y=0;
            z=zz;
            vx=false;
            vy=false;
            vz=true;
            code=1;
        }

    }

    private void onFrmLoad(object sender, EventArgs e)
    {            
        Redraw();           
    }

    private void onsize(object sender, EventArgs e)
    {            
        Redraw();           
    }

    private void onTXT(object sender, EventArgs e)
    {  
        if(Visible){                      
            Redraw();
        }
    }
    
    private string PrintGcode(Gcode c){
        string str = "G0"+c.code;
        if(c.vx)str +=" X"+c.x.ToString("F2").Replace(",",".");
        if(c.vy)str +=" Y"+c.y.ToString("F2").Replace(",",".");
        if(c.vz)str +=" Z"+c.z.ToString("F2").Replace(",",".");
        return str;
    }
    
    private Gcode decodeGcode(string line){
        
        Gcode p=new Gcode();

        while(line.Contains("  "))line = line.Replace("  "," ");
        string[] code = line.Split(" ");  
        
        if(code[0]=="G1" || code[0]=="G0" || code[0]=="G00")code[0]="G01";
        if(code[0]!="G01" && code[0]!="G00")return p;
        if(code[0][2]=='0')p.code = 0;
        if(code[0][2]=='1')p.code = 1;
        bool conv=((1.5).ToString().Contains(","));
        for(int k=1;k<code.Length;k++){
            if(conv)code[k] = code[k].Replace(".", ",");
            switch(code[k][0]){
                case 'x':
                case 'X':
                    p.x = double.Parse(code[k].Substring(1));
                    p.vx=true;
                    break;
                case 'y':
                case 'Y':
                    p.y = double.Parse(code[k].Substring(1));
                    p.vy=true;
                    break;
                case 'z':
                case 'Z':
                    p.z = double.Parse(code[k].Substring(1));
                    p.vz = true;
                    break;
            }
        }
         
        return p;
    }

    public async void ExportToSVG(String FileName)
    {
       StringReader sr = new StringReader(txtcode.Text);
       StreamWriter sw =new StreamWriter(FileName);
       sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
       sw.WriteLine("<svg style=\"fill:none;stroke-width:0.5;stroke-linecap:round;stroke-linejoin:round;stroke:rgb(0%,0%,0%);stroke-opacity:1;stroke-miterlimit:10;\" >");

       string line;
       string path =string.Empty;
       bool scava=false;
       int nline=0;
       while((line=sr.ReadLine())!=null){
            Gcode gc = decodeGcode(line);
            if(gc.code!=1)continue;
            if(gc.vz){
                scava = (gc.z<0.0);
			} 
            if(gc.vx && gc.vy){
                if(nline>0)path +=" ";
                if(scava)path += "L "+gc.x.ToString("#.00").Replace(',','.')+","+gc.y.ToString("#.00").Replace(',','.');
                else path +="M "+gc.x.ToString("#.00").Replace(',','.')+","+gc.y.ToString("#.00").Replace(',','.');
                nline++;
            }
       }

        sw.WriteLine("<path d=\""+path+"\"/>");
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
        step = 1.0 / 3;
        for (double i = 0; i < 1.0; i += step)
        {
            Gcode gc =new Gcode();
            gc.code =1;
            gc.vx=true;
            gc.vy=true;
            gc.x = Math.Pow(1.0 - i, 3) * x0 + 3.0 * i * Math.Pow(1.0 - i, 2) * x1 + 3.0 * (1.0 - i) * Math.Pow(i, 2) * x2 + Math.Pow(i, 3) * x3;
            gc.y = Math.Pow(1.0 - i, 3) * y0 + 3.0 * i * Math.Pow(1.0 - i, 2) * y1 + 3.0 * (1.0 - i) * Math.Pow(i, 2) * y2 + Math.Pow(i, 3) * y3;
            txtcode.AppendText(PrintGcode(gc)+ "\r\n");
        }
        txtcode.AppendText(PrintGcode(new Gcode(x3,y3))+ "\r\n");
    }

    public void Coppia2_Beize(
            double x0, double y0,
            double x1, double y1,
            double x2, double y2,
            double step)
    {
        step = 1.0 / 3;
        for (double i = 0; i < 1; i += step)
        {
            Gcode gc =new Gcode();
            gc.code =1;
            gc.vx=true;
            gc.vy=true;
            gc.x = Math.Pow(1.0 - i, 2) * x0 + 2.0 * (1.0 - i) * i * x1 + Math.Pow(i, 2) * x2;
            gc.y = Math.Pow(1.0 - i, 2) * y0 + 2.0 * (1.0 - i) * i * y1 + Math.Pow(i, 2) * y2;
            txtcode.AppendText(PrintGcode(gc)+ "\r\n");
        }
        txtcode.AppendText(PrintGcode(new Gcode(x2,y2))+ "\r\n");
    }

    public void Coppia2_Linea(
            double x0, double y0,
            double x1, double y1,
            double dist)
    {
        double norma = Math.Sqrt((x1-x0)*(x1-x0)+(y1-y0)*(y1-y0));
        double np = norma/dist;
        double step = 1.0 / np;
        for (double i = 0; i < 1; i += step)
        {
            Gcode gc =new Gcode();
            gc.code =1;
            gc.vx=true;
            gc.vy=true;
            gc.x = x0 * (1-i) + x1 * (i);
            gc.y = y0 * (1-i) + y1 * (i);
            txtcode.AppendText(PrintGcode(gc)+ "\r\n");
        }
        txtcode.AppendText(PrintGcode(new Gcode(x1,y1))+ "\r\n");
    }

    public async void ImportSVG(String SvgFileName,double passo)
    {
		txtcode.AppendText(PrintGcode(new Gcode(0,0))+"\r\n");   
		txtcode.AppendText("G90"+"\r\n"); //COOCRDINATE ASSOLUTE
		txtcode.AppendText("M03"+"\r\n");
		txtcode.AppendText("G01 F200.00000"+"\r\n");
            
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

            double aa = 1.0;
            double bb = 0.0;
            double cc = 0.0;
            double dd = 1.0;
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
				Console.WriteLine("process " + vs[0]+" "+vs.Count);
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
                        txtcode.AppendText(PrintGcode(new Gcode(5))+"\r\n");
                        txtcode.AppendText(PrintGcode(new Gcode(x,y))+"\r\n");
                        txtcode.AppendText(PrintGcode(new Gcode(-0.15))+"\r\n");
                        xs = x;
                        ys = y;
                        break;
                    case 'z':
                    case 'Z':
                        curCom = vs[0][0];
                        Coppia2_Linea(x, y, xs, ys,  passo);
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
                                Coppia2_Linea(x, y, x + a, y + b,  passo);
                                x += a;
                                y += b;
                            }
                            if (curCom == 'L')
                            {
                                Coppia2_Linea(x, y, a, b,  passo);
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
                                Coppia2_Beize(x, y, a + x, b + y, c + x, d + y,  passo);
                                x += c;
                                y += d;
                            }
                            if (curCom == 'Q')
                            {
                                Coppia2_Beize(x, y, a, b, c, d,  passo);
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
                                Coppia2_Beize(x, y, a + x, b + y, c + x, d + y, e + x, f + y,  passo);
                                x += e;
                                y += f;
                            }
                            if (curCom == 'C')
                            {
                                Coppia2_Beize(x, y, a, b, c, d, e, f,  passo);
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
                                Coppia2_Linea(x, y, x + a, y,  passo);
                                x += a;
                            }
                            if (curCom == 'H')
                            {
                                Coppia2_Linea(x, y, a, y,  passo);
                                x = a;
                            }
                            if (curCom == 'v')
                            {
                                Coppia2_Linea(x, y, x, y + a,  passo);
                                y += a;
                            }
                            if (curCom == 'V')
                            {
                                Coppia2_Linea(x, y, x, a,  passo);
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
        
        txtcode.AppendText(PrintGcode(new Gcode(5))+"\r\n");
        txtcode.AppendText(PrintGcode(new Gcode(0,0))+"\r\n");
		txtcode.AppendText("M5"+"\r\n");
		txtcode.AppendText("M9"+"\r\n");
		txtcode.AppendText("M2"+"\r\n");
		Console.WriteLine("end import svg");
        Redraw();
    }

    public async void LoadGC(string FileName)
    {
        StreamReader fg = new StreamReader(FileName);
        string li;
        while((li=fg.ReadLine())!=null){ 
            li=li.Replace("G1", "G01");
            txtcode.AppendText(li+ "\r\n");	               
        }
        Redraw();
    }

    private async void Redraw()
    {
       StringReader sr =new StringReader(txtcode.Text);
       float LL =disegno.ClientSize.Width;
       float HH =disegno.ClientSize.Height;
       Bitmap bmp =new Bitmap((int)LL,(int)HH); 
	   Pen blackPen = new Pen(Color.Red, 2);
	   Pen blackGreen = new Pen(Color.Green, 2);
	   Pen cPen=blackPen;
       Graphics g =Graphics.FromImage(bmp);
       g.FillRectangle(Brushes.White,0,0,LL,HH);
       List<Gcode> data =new List<Gcode>();
       string line;

       double minx=Double.MaxValue;
       double miny=Double.MaxValue;
       double maxx=0;
       double maxy=0;
       float scale=1F;
       
       while((line=sr.ReadLine())!=null)
       {
            Gcode gc = decodeGcode(line);
            if(gc.code!=1)continue;
            if(gc.vx){
                if(gc.x<minx){
                    minx = gc.x;
                }
                if(gc.x>maxx){
                    maxx = gc.x;
                }
            }
            if(gc.vy){
                if(gc.y<miny){
                    miny = gc.y;
                }
                if(gc.y>maxy){
                    maxy = gc.y;
                }
            }
            data.Add(gc);
       }

       if(LL<HH)scale=(float)(LL/Math.Abs(maxx-minx));
       else scale=(float)(HH/Math.Abs(maxy-miny));
       
       double xo=minx;
       double yo=miny;

       foreach(Gcode gc in data){ 
            if(gc.vz){
				if(gc.z<0)cPen=blackPen;
				else cPen=blackGreen;
			}            
            if(gc.vx && gc.vy){            
                double x=gc.x-minx;
                double y=gc.y-miny;
                if(x>0.0 && y>0.0){
                    g.DrawLine(cPen,
                    (int)(xo*scale)+20,
                    (int)(yo*scale)+20,
                    (int)(x*scale)+20,
                    (int)(y*scale)+20);
                    g.FillEllipse(Brushes.Black,
                    (int)(x*scale)-3+20,
                    (int)(y*scale)-3+20,6,6);												
                    xo=x;
                    yo=y;
                }
            }
       }
       g.Dispose();
       disegno.BackgroundImage = bmp;
    }
    
    private void InitializeComponent()
    {
        
        disegno =new Panel();  
        disegno.Dock = DockStyle.Fill;
        disegno.HorizontalScroll.Enabled=false;
        disegno.HorizontalScroll.Visible=false;
        disegno.VerticalScroll.Enabled=false;
        disegno.VerticalScroll.Visible=false;  
        disegno.AutoScroll=false;
        disegno.BackgroundImageLayout= ImageLayout.Center;

        sc =new SplitContainer();
        sc.Dock = DockStyle.Fill;        
        txtcode =new RichTextBox();
        txtcode.Dock = DockStyle.Fill;
        sc.Panel2.Controls.Add(disegno);
        sc.Panel1.Controls.Add(txtcode);

        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Text = "Gcode Viewer";
        this.Controls.Add(sc);
    }
    
    public MainForm()
    {
        InitializeComponent();
        this.Load+=new EventHandler(onFrmLoad);
        this.Resize+=new EventHandler(onsize);
        txtcode.TextChanged+=new EventHandler(onTXT);
    }
}
