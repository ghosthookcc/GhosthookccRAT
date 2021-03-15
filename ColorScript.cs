using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Drawing.Imaging;

namespace color
{
    public class ColorScript
    {
        public static void ClearLine()
        {
            
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        public void banner_print()
        {
            void checkIfValid(List<char> prevline)
            {
                string newline = "";
                foreach (char letter in prevline)
                {
                    newline += letter;
                }

                newline = newline.Replace("################################################################", null);

                //Console.WriteLine(newline.Length);

                if (newline.Length == 64)
                {
                    colored_print(newline, "Green", "Black", false, false);
                }
                else
                {
                    ClearLine();
                }
            }

            Image picture = Image.FromFile("logo7.png");
            Console.SetBufferSize((picture.Width * 0x2), (picture.Height * 0x2));
            Console.WindowWidth = 90;
            Console.WindowHeight = 40;

            FrameDimension dimension = new FrameDimension(picture.FrameDimensionsList[0x0]);
            int FrameCount = picture.GetFrameCount(dimension);
            int Left = Console.WindowLeft, Top = Console.WindowTop;

            char[] chars = { '#', '#', '@', '%', '=', '+', '*', ':', '-', '.', ' ' };

            picture.SelectActiveFrame(dimension, 0x0);
            for (int i = 0x0; i < picture.Height; i++)
            {
                List<char> prevline = new List<char>();
                for (int x = 0x0; x < picture.Width; x++)
                {
                    Color colorBuffer = ((Bitmap)picture).GetPixel(x, i);
                    int Gray = (colorBuffer.R + colorBuffer.G + colorBuffer.B) / 0x3;
                    int Index = (Gray * (chars.Length - 0x1)) / 0xFF;

                    //colored_print(chars[Index], "Green", "Black", false, false);
                    prevline.Add(chars[Index]);
                }

                checkIfValid(prevline);
                Console.Write("\n");
                Thread.Sleep(50);
            }
        }

        private const string BLACK = "@";
        private const string CHARCOAL = "#";
        private const string DARKGRAY = "8";
        private const string MEDIUMGRAY = "&";
        private const string MEDIUM = "o";
        private const string GRAY = ":";
        private const string SLATEGRAY = "*";
        private const string LIGHTGRAY = ".";
        private const string WHITE = " ";

        private static string getGrayShade(int redValue)
        {
            string asciival = " ";

            if (redValue >= 230)
            {
                asciival = WHITE;
            }
            else if (redValue >= 200)
            {
                asciival = LIGHTGRAY;
            }
            else if (redValue >= 180)
            {
                asciival = SLATEGRAY;
            }
            else if (redValue >= 160)
            {
                asciival = GRAY;
            }
            else if (redValue >= 130)
            {
                asciival = MEDIUM;
            }
            else if (redValue >= 100)
            {
                asciival = MEDIUMGRAY;
            }
            else if (redValue >= 70)
            {
                asciival = DARKGRAY;
            }
            else if (redValue >= 50)
            {
                asciival = CHARCOAL;
            }
            else
            {
                asciival = BLACK;
            }

            return asciival;
        }

        public string GrayscaleImageToASCII(Image img)
        {
            StringBuilder html = new StringBuilder();
            Bitmap bmp = null;

            try
            {
                // Create a bitmap from the image

                bmp = new Bitmap(img);

                // The text will be enclosed in a paragraph tag with the class

                // ascii_art so that we can apply CSS styles to it.

                html.Append("&lt;br/&rt;");

                // Loop through each pixel in the bitmap

                for (int y = 0; y < bmp.Height; y++)
                {
                    for (int x = 0; x < bmp.Width; x++)
                    {
                        // Get the color of the current pixel

                        Color col = bmp.GetPixel(x, y);

                        // To convert to grayscale, the easiest method is to add

                        // the R+G+B colors and divide by three to get the gray

                        // scaled color.

                        col = Color.FromArgb((col.R + col.G + col.B) / 3,
                            (col.R + col.G + col.B) / 3,
                            (col.R + col.G + col.B) / 3);

                        // Get the R(ed) value from the grayscale color,

                        // parse to an int. Will be between 0-255.

                        int rValue = int.Parse(col.R.ToString());

                        // Append the "color" using various darknesses of ASCII

                        // character.

                        html.Append(getGrayShade(rValue));

                        // If we're at the width, insert a line break

                        if (x == bmp.Width - 1)
                            html.Append("&lt;br/&rt");
                    }
                }

                // Close the paragraph tag, and return the html string.

                html.Append("&lt;/p&rt;");

                return html.ToString();
            }
            catch (Exception exc)
            {
                return exc.ToString();
            }
            finally
            {
                bmp.Dispose();
            }
        }

        public void colored_print(dynamic input, string TextColor, string BackgroundColor, bool endchar, bool centertext)
        {
            ConsoleColor text;
            ConsoleColor background;

            ConsoleColor.TryParse(TextColor, out text);
            ConsoleColor.TryParse(BackgroundColor, out background);

            Console.ForegroundColor = text;
            Console.BackgroundColor = background;

            if (endchar == true)
            {
                if (centertext == true)
                {
                    Console.WriteLine(String.Format("{0," + Console.WindowWidth / 2 + "}", input));
                }
                Console.WriteLine(input);
            }
            else
            {
                if (centertext == true)
                {
                    Console.Write(String.Format("{0," + Console.WindowWidth / 2 + "}", input));
                }
                Console.Write(input);
            }

            Reset();
        }

        public void changeForeground(string ForegroundColor)
        {
            ConsoleColor foreground;

            ConsoleColor.TryParse(ForegroundColor, out foreground);

            Console.ForegroundColor = foreground;
        }

        public void changeBackground(string BackgroundColor)
        {
            ConsoleColor background;

            ConsoleColor.TryParse(BackgroundColor, out background);

            Console.BackgroundColor = background;
        }

        public void Reset()
        {
            Console.ResetColor();
        }
    }
}

