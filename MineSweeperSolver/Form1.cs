using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Timers;
using System.Threading;

namespace MineSweeperSolver
{
    public partial class Form1 : Form
    {
        System.Timers.Timer t = new System.Timers.Timer
        {
            Interval = 200
        };
        public Form1()
        {
            InitializeComponent();
            panel1.AutoScroll = true;
           
            t.Elapsed += new ElapsedEventHandler(TimerDone);
            
        }

        private void ScreenshotBtn_Click(object sender, EventArgs e)
        {
            Bitmap screenShot = null;

            
            screenShot = CaptureImage(Convert.ToInt32(XBox.Text), Convert.ToInt32(YBox.Text), Convert.ToInt32(WidthBox.Text), Convert.ToInt32(HeightBox.Text), 1);
            if (InvokeRequired)
            {
                // after we've done all the processing, 
                this.Invoke(new MethodInvoker(delegate
                {
                    pictureBox1.Image = screenShot;
                }));
            }           
        }

        Bitmap CaptureImage(int x, int y, int width, int height, int scale)
        {
            
            Bitmap captureBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            Graphics captureGraphics = Graphics.FromImage(captureBitmap);
            captureGraphics.CopyFromScreen(x, y, 0, 0, captureBitmap.Size);
            Debug.WriteLine("Screenshot taken!");
           
            var resized = new Bitmap(captureBitmap, new Size(captureBitmap.Width / scale, captureBitmap.Height / scale));
            //resized = new Bitmap(resized, new Size(resized.Width * 38, resized.Height * 38));
            var SmallRect = new Rectangle(0, 0, width / scale, height / scale);
            var NormRect = new Rectangle(0, 0, width, height);
            
            using (var graphics = Graphics.FromImage(resized))
            {
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                graphics.SmoothingMode = SmoothingMode.None;

                graphics.DrawImage(resized, SmallRect, 0, 0, resized.Width, resized.Height, GraphicsUnit.Pixel);
            }

           /* var Upsize = new Bitmap(resized, new Size(resized.Width * scale, resized.Height * scale));
            using (var graphics = Graphics.FromImage(Upsize))
            {
                graphics.PixelOffsetMode = PixelOffsetMode.Half;

                graphics.InterpolationMode = InterpolationMode.NearestNeighbor;

                graphics.SmoothingMode = SmoothingMode.None;

                graphics.DrawImage(Upsize, 0, 0, captureBitmap.Width, captureBitmap.Height);
            }*/


            return resized;
        }

        private void SaveImageBtn_Click(object sender, EventArgs e)
        {
            Bitmap screenShot = null;
            checkBox1.Checked = false;
            t.Enabled = false;
            screenShot = CaptureImage(Convert.ToInt32(XBox.Text), Convert.ToInt32(YBox.Text), Convert.ToInt32(WidthBox.Text), Convert.ToInt32(HeightBox.Text), Convert.ToInt32(ScaleBox.Text));
            pictureBox1.Image = screenShot;
            MineSolver solver = new MineSolver();
            solver.CheckImage(screenShot);
            
             SaveFileDialog saveFileDialog = new SaveFileDialog
             {
                 Filter = "Bitmap (*.bmp)|.bmp"
             };

             if (saveFileDialog.ShowDialog() == DialogResult.OK)
             {
                 screenShot.Save(saveFileDialog.FileName);
             }
        }

        void TimerDone(object sender, ElapsedEventArgs e)
        {
            ScreenshotBtn_Click(sender, e);
        }

        private void SolveBtn_Click(object sender, EventArgs e)
        {
            checkBox1.Checked = false;
            t.Enabled = false;
            Bitmap screenShot = null;
            screenShot = CaptureImage(Convert.ToInt32(XBox.Text), Convert.ToInt32(YBox.Text), Convert.ToInt32(WidthBox.Text), Convert.ToInt32(HeightBox.Text), Convert.ToInt32(ScaleBox.Text));
            //pictureBox1.Image = screenShot;
            MineSolver solver = new MineSolver();
            solver.Solve(solver.CheckImage(screenShot), Convert.ToInt32(XBox.Text), Convert.ToInt32(YBox.Text));
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                t.Enabled = true;
            }
            else
            {
                t.Enabled = false;
            }
        }

        private void OverlayBtn_Click(object sender, EventArgs e)
        {
            Bitmap screenShot = CaptureImage(Convert.ToInt32(XBox.Text), Convert.ToInt32(YBox.Text), Convert.ToInt32(WidthBox.Text), Convert.ToInt32(HeightBox.Text), Convert.ToInt32(ScaleBox.Text));
            MineSolver solver = new MineSolver();
            var Field = solver.CheckImage(screenShot);
            screenShot = CaptureImage(Convert.ToInt32(XBox.Text), Convert.ToInt32(YBox.Text), Convert.ToInt32(WidthBox.Text), Convert.ToInt32(HeightBox.Text), 1);

            Rectangle[] rectangles = new Rectangle[481];
            for (int y = 0; y < 16; y++)
            {
                for(int x = 0; x < 30; x++)
                {
                    Graphics g;
                    g = Graphics.FromImage(screenShot);
                    Pen pen = new Pen(Brushes.Black);
                    pen.Width = 5;
                    Font font = new Font("Times New Roman", 12.0f);
                    switch (Field[x, y]){
                        case 0: pen.Color = Color.FromArgb(80, 255, 255, 255); break;       //Unknown:  White
                        case 1: pen.Color = Color.FromArgb(80, 112, 25, 33);   break;       //Flagged:  Dark Red
                        case 2: pen.Color = Color.FromArgb(80, 128, 128, 128); break;       //0:        Gray   
                        case 3: pen.Color = Color.FromArgb(80, 0, 0, 255);     break;       //1:        Blue       
                        case 4: pen.Color = Color.FromArgb(80, 0, 255, 0);     break;       //2:        Green
                        case 5: pen.Color = Color.FromArgb(80, 255, 0, 0);     break;       //3:        Red
                        case 6: pen.Color = Color.FromArgb(80, 13, 13, 112);   break;       //4:        Darkblue
                        case 7: pen.Color = Color.FromArgb(80, 112, 50, 50);   break;       //5:        Random
                        default: pen.Color = Color.FromArgb(80, 0, 0, 0);      break;
                    }             
                    
                    g.FillRectangle(pen.Brush, 44 * x, 44 * y, 43,  43);
                   
                    pen.Color = Color.Black;
                    g.DrawString(Field[x, y].ToString(), font, pen.Brush, 44 * x, 44 * y);
                    pen.Dispose();
                    
                    //pictureBox1.Image = screenShot;
                    
                }
            }
            pictureBox1.Image = screenShot;
        }

        private void TestBtn_Click(object sender, EventArgs e)
        {
            MineSolver ms = new MineSolver();
            ms.RightClickField(1, 1, 0, Convert.ToInt32(XBox.Text), Convert.ToInt32(YBox.Text));
        }
    }
}
