using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using System.Runtime.Serialization;

namespace C_Sharp_WinForm
{
    public partial class Form1 : Form
    {
        Bitmap pImage;
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
        }

        private Color GetBW(Color clr)
        {
            if (clr.G <=127)
                return Color.FromArgb(0,0,0);
            else
                return Color.FromArgb(255,255,255);
        }

        bool OutOfRange(int x, int y)
        {
            if (x <= 5 || x >= 16)
                return true;
            if (y >= 12 && x >= 15)
                return true;
            if (y >= 10 && x >= 15)
                return true;
            if (y > 16 || y < 5)
                return true;
            return false;
        }


        double getratio(Bitmap Image, Bitmap Word,int x)
        {
            int success = 0;
            for (int i = x; i < x + 10; i++)
            {
                for (int j = 0; j < Word.Height; j++)
                {
                    if (Image.GetPixel(i, j) == Word.GetPixel(i - x, j))
                        success++;
                }
            }
            return (double)success / (double)(Word.Width * Word.Height);
        }

        double verify(Bitmap Image, Bitmap Word, int x)
        {
            double ret = 0;
            for (int i = x - 4; i < x + 5; i++)
                ret = Math.Max(ret,getratio(Image,Word,i));
            return ret;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (WebClient client = new WebClient())
            {
                byte[] img = client.DownloadData("http://new.tyvj.cn/UserControl/imageValidate.aspx");
                Bitmap RemoteImage = new Bitmap(new MemoryStream(img));
                Bitmap vcode = new Bitmap(RemoteImage.Size.Width*10,RemoteImage.Height*10);
                Bitmap vcodes = new Bitmap(RemoteImage.Size.Width, RemoteImage.Size.Height);
                pImage = new Bitmap(10,RemoteImage.Height);
                for (int x = RemoteImage.Size.Width - 1; x >= 0;x-- )
                {
                    for (int y = RemoteImage.Size.Height - 1; y >= 0; y--)
                    {
                        int[] dx = new int[] { 0, 1, -1, 0, -1, -1, 1, 1 };
                        int[] dy = new int[] { 1, 0, 0, -1, 1, -1, 1, -1 };
                        Color now = GetBW(RemoteImage.GetPixel(x, y));
                        int count = 0;
                        for (int i = 0; i < 4; i++)
                        {
                            int nowx = x + dx[i];
                            int nowy = y + dy[i];
                            if (nowx >= 0 && nowx < RemoteImage.Size.Width && nowy >= 0 && nowy < RemoteImage.Size.Height)
                            {
                                Color tmp = GetBW(RemoteImage.GetPixel(nowx,nowy));
                                if (tmp == now)
                                    count++;
                            }
                        }
                        if (count < 2)
                            now = Color.FromArgb(255, 255, 255);
                        /*
                        if (OutOfRange(x, y))
                            now = Color.FromArgb(255, 255, 255);
                        else
                        {
                            pImage.SetPixel(x - 6, y, now);
                        }
                         */
                        vcodes.SetPixel(x, y, now);
                        for (int xx = x * 10 + 1; xx < (x + 1) * 10; xx++)
                            for (int yy = y * 10 + 1; yy < (y + 1) * 10; yy++)
                                vcode.SetPixel(xx, yy, now);
                    }
                }
                for (int x = vcode.Size.Width-1; x >= 0; x--)
                    for (int y = vcode.Size.Height-1; y >= 0; y--)
                        if (x % 10 == 0 || y % 10 == 0)
                            vcode.SetPixel(x,y,Color.FromArgb(255, 0, 0));
                pictureBox1.Image = vcode;
                pictureBox2.Image = RemoteImage;
                char[] res = new char[5];
                for (int x = 0; x < 5; x ++)
                {
                    double ratio = 0;
                    foreach (string file in Directory.GetFiles("..\\..\\vcode\\"))
                    {
                        if (".bmp" == Path.GetExtension(file))
                        {
                            Bitmap tmp = new Bitmap(file);
                            if (verify(vcodes, tmp, x*10+6) > ratio)
                            {
                                ratio = verify(vcodes, tmp, x*10+6);
                                res[x] = Path.GetFileName(file)[0];
                            }
                        }
                    }
                }
                textBox1.Text = new string(res);
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                Bitmap b = (Bitmap)pictureBox1.Image;
                Color c = b.GetPixel(e.X, e.Y);
                label1.Text = c.ToString();
            }
            catch
            {
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length != 1)
                MessageBox.Show("Error!");
            else
            {
                pImage.Save("..\\..\\vcode\\" + textBox1.Text+".bmp");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
