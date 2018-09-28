using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MineSweeperSolver
{
    class MineSolver
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        //Mouse actions
        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        readonly Color[] Blocks = {  Color.FromArgb(89, 153, 205),       //Unknown
                            Color.FromArgb(184,150,66),         //Flagged
                            Color.FromArgb(169, 171, 172),      //0
                            Color.FromArgb(161, 169, 171),      //1
                            Color.FromArgb(159, 163, 158),      //2
                            Color.FromArgb(167, 159, 164),      //3
                            Color.FromArgb(155, 160, 167),      //4
                            Color.FromArgb(165,159,160)         //5
                };

        public int[,] CheckImage(Bitmap bmp)
        {
            int x,y = 0;

            int[] totals = { 0, 0, 0, 0, 0, 0, 0, 0, 0};
            int[,] Field= new int[30,16];

            for (y = 0; y < 16; y++)
            {
                for (x = 0; x < 30; x++)
                {
                    var c = bmp.GetPixel(x, y);
                    int color = FindColor(c);
                    Field[x, y] = color;
                    totals[color]++;
                }
            }

            for (int i = 0; i < 9; i++)
            {
                Debug.WriteLine("Array index " + i + ". We found " + totals[i].ToString());
            }

            return Field;
        }

        int FindColor(Color c)
        {
            int mindifference = 30;
            int output = 7;
            for (int i = 0; i < 8; i++)
            {
                int difference = 0;
                difference += Math.Abs(c.R - Blocks[i].R);
                difference += Math.Abs(c.G - Blocks[i].G);
                difference += Math.Abs(c.B - Blocks[i].B);

                if (difference < mindifference) {
                    output =i;
                    mindifference = difference;
                }                       
            }
            return output;
        }

        public void Solve(int[,] field, int offsetx, int offsety)
        {
            int x, y = 0;

            for (y = 0; y < 16; y++)
            {
                for (x = 0; x < 30; x++)
                {
                    field = SolvingAlgorithm(field, x, y, offsetx, offsety);
                }
            }
        }
        /// <summary>
        /// Given any field and a location of a location it needs to check
        /// this outputs all 8 locations
        /// it will return a -1 if a adjecent location is not present
        /// for example at the borders
        /// </summary>
        /// <param name="field"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        int[] CheckSurroundings(int[,] field, int x, int y)
        {
            //starting up and then go clockwise
            int[] surroundings = new int[8];
            
            for (int i = 0; i < 8; i++)
            {
                try
                {
                    switch (i)
                    {
                        case 0: surroundings[0] = field[x, y - 1]; break;
                        case 1: surroundings[1] = field[x + 1, y - 1]; break;
                        case 2: surroundings[3] = field[x + 1, y + 1]; break;
                        case 3: surroundings[3] = field[x + 1, y + 1]; break;
                        case 4: surroundings[4] = field[x, y + 1]; break;
                        case 5: surroundings[5] = field[x - 1, y + 1]; break;
                        case 6: surroundings[6] = field[x - 1, y]; break;
                        case 7: surroundings[7] = field[x - 1, y - 1]; break;
                    }
                }
                catch(Exception)
                {
                    surroundings[i] = -1;
                    Debug.WriteLine("Found Exception");
                }
            }/*
            if (y > 0)
            {
                surroundings[0] = field[x, y - 1];

                if (x < 29)
                    surroundings[1] = field[x + 1, y - 1];

                if (x > 0) 
                surroundings[7] = field[x - 1, y - 1];
            }
            else
            {
                surroundings[0] = surroundings[1] = surroundings[7] = -1;
            }

            if (y < 15)
            {
                if (x < 29)
                    surroundings[3] = field[x + 1, y + 1];

                surroundings[4] = field[x, y + 1];

                if (x > 0)
                    surroundings[5] = field[x - 1, y + 1];
            }
            else
            {
                surroundings[3] = surroundings[4] = surroundings[5] = -1;
            }
            if (x < 29)
                surroundings[2] = field[x + 1, y];
            else
                surroundings[2] = -1;

            if (x > 0)
                surroundings[6] = field[x - 1, y];
            else
                surroundings[6] = -1;*/

            return surroundings;
        }

        
        /// <summary>
        /// Right clicks the right field.
        /// It only needs the current location + the field index + where the field is located on the screen
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="index"></param>
        /// <param name="offsetx"></param>
        /// <param name="offsety"></param>

        public void RightClickField(int x, int y, int index, int offsetx, int offsety)
        {

            Point p = SurroundingLocation(x, y, index);
            
            Debug.WriteLine("Clicking. X = " + x.ToString() + "Y = " + y.ToString());
            p.X = (p.X * 44) + 15 + offsetx;
            p.Y = (p.Y * 44) + 15 + offsety;
            Cursor.Position = p;
            uint X = (uint)Cursor.Position.X;
            uint Y = (uint)Cursor.Position.Y;
            mouse_event(MOUSEEVENTF_RIGHTDOWN, X, Y, 0, 0);
            mouse_event(MOUSEEVENTF_RIGHTUP, X, Y, 0, 0);
        }

        int[,] SolvingAlgorithm(int[,] field, int x, int y, int offsetx, int offsety)
        {
            int value = field[x, y];
            int EmptyFields = 0;
            if (value > 2)
            {
                value = value - 2;
                var surroundings = CheckSurroundings(field, x, y);
                for (int i = 0; i < 8; i++)
                {
                    if (surroundings[i] == 1)
                        value -= 1;
                    if (value < 0)
                        Debug.WriteLine("Something went wrong");
                    if (surroundings[i] == 0)
                        EmptyFields++;
                }

                if (value > 0)
                {
                    if (EmptyFields == value)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            if (surroundings[i] == 0)
                            {
                                RightClickField(x, y, i, offsetx, offsety);
                                //System.Threading.Thread.Sleep(1000);
                                Point point = SurroundingLocation(x, y, i);
                                field[x, y] = 1;
                            }
                        }                        
                    }                    
                }
            }
            return field;
        }

        Point SurroundingLocation(int x, int y, int index)
        {
            Point point = new Point();
            switch (index)
            {
                case 0: y--; break;
                case 1: x++; y--; break;
                case 2: x++; break;
                case 3: x++; y++; break;
                case 4: y++; break;
                case 5: x--; y++; break;
                case 6: x--; break;
                case 7: x--; y--; break;
            }

            point.X = x;
            point.Y = y;
            return point;
        }
    }    
}
