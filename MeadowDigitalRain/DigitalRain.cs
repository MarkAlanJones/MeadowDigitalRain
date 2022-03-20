using Meadow.Foundation;
using Meadow.Foundation.Displays.TftSpi;
using Meadow.Foundation.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

/*
       Digital Rain Animation based on https://github.com/0015/TP_Arduino_DigitalRain_Anim/blob/main/src/DigitalRainAnim.cpp
       Created by Eric Nam, November 08, 2021.
       Released into the public domain.
       Converted to Meadow c# by Mark Jones Nov 2021
*/

namespace MeadowDigitalRain
{
    public class DigitalRain
    {
        private MicroGraphics graphics { get; set; }
        private TftSpiBase display { get; set; }

        readonly Stopwatch sw = new Stopwatch();
        readonly Random rand = new Random();

        // DigitalRain properties
        readonly int line_len_min;        //minimum length of characters 
        readonly int line_len_max;        //maximum length of characters 
        readonly int line_speed_min;      //minimum vertical move speed
        readonly int line_speed_max;      //maximum vertical move speed
        readonly int timeFrame;           //time frame for drawing
        readonly bool isAlphabetOnly;     //boolean for showing Alphabet only
        bool isPlaying = false;           //boolean for play or pause

        const int KEY_RESET_TIME = 60 * 1000;  //1 Min reset time

        ScaleFactor fontSize;       //default font size X1
        int lineWidth;              //default line width
        int letterHeight;           //default letter height

        int numOfline;              //number of calculated row

        long lastDrawTime;          //checking last drawing time
        long lastUpdatedKeyTime;    //checking last generating key time

        Color headCharColor;        //having a text color  
        Color textColor;            //having a text color  
        Color bgColor;              //having a bg color  
        string keyString;           //storing generated key

        List<int> line_length;      //dynamic array for each line of vertical length
        List<int> line_pos;         //dynamic array for each line Y position
        List<int> line_speed;       //dynamic array for each line speed

        //initialze with defaults
        public DigitalRain(TftSpiBase display, bool biggerText, bool alphabetOnly) :
            this(display, line_len_min: 3, line_len_max: 20,
                 line_speed_min: 3, line_speed_max: 15,
                 timeFrame: 100,
                 biggerText: biggerText, alphabetOnly: alphabetOnly)
        { }
     
        //initialze with params
        public DigitalRain(TftSpiBase display,
                           int line_len_min, int line_len_max,
                           int line_speed_min, int line_speed_max,
                           int timeFrame,
                           bool biggerText, bool alphabetOnly) 
        {
            Console.WriteLine($"Digital Rain Init {line_len_min}-{line_len_max}");
            this.line_len_min = line_len_min;
            this.line_len_max = line_len_max;
            this.line_speed_min = line_speed_min;
            this.line_speed_max = line_speed_max;
            this.timeFrame = timeFrame;
            this.isAlphabetOnly = alphabetOnly;

            // extended graphics library
            graphics = new MicroGraphics(display)
            {
                Rotation = RotationType._270Degrees,
                // set font here
                CurrentFont = new Font12x16()
            };

            SetBigText(biggerText);
            PrepareAnim(display);
            graphics.Clear(bgColor, true);
        }

        //set Text Bigger
        private void SetBigText(bool isOn)
        {
            fontSize = isOn ? ScaleFactor.X2 : ScaleFactor.X1;
            lineWidth = graphics.CurrentFont.Width * (int)fontSize;
            letterHeight = graphics.CurrentFont.Height * (int)fontSize;
        }

        //checking how many lines it can draw from the width of the screen.
        //the size of the array is determined by the number of lines.
        private void PrepareAnim(TftSpiBase display)
        {
            Console.WriteLine($"Digital Rain Preparing...");
            this.display = display;
            sw.Start();

            // set colours here
            headCharColor = Color.White;
            textColor = Color.Green;
            bgColor = Color.Black;

            lastDrawTime = sw.ElapsedMilliseconds - timeFrame;

            numOfline = display.Width / lineWidth;
            Console.WriteLine($"{numOfline} lines");

            line_length = new List<int>();
            line_pos = new List<int>();
            line_speed = new List<int>();

            for (int i = 0; i < numOfline; i++)
            {
                line_length.Add(rand.Next(line_len_min, line_len_max + 1));
                line_pos.Add(SetYPos(line_length[i]) - letterHeight);
                line_speed.Add(rand.Next(line_speed_min, line_speed_max + 1));
                //Console.WriteLine($"   line {i}  {line_length[i]} {line_pos[i]} {line_speed[i]}");
            }

            isPlaying = true;
            lastUpdatedKeyTime = sw.ElapsedMilliseconds - timeFrame;
        }

        //a function where animation continues to run.
        public void DigitalRainAnim_loop()
        {
            long currentTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"Digital Rain {currentTime:0,0}ms {(currentTime-lastDrawTime)/1000.0:0.##}s per loop - Playing? {isPlaying}");

            if ((currentTime - lastUpdatedKeyTime) > KEY_RESET_TIME)
            {
                ResetKey();
            }

            if ((currentTime - lastDrawTime) < timeFrame)
            {
                return;
            }
            else if (isPlaying)
            {
                for (int i = 0; i < numOfline; i++)
                {
                    LineAnimation(i);
                }
            }
            lastDrawTime = currentTime;
        }

        //a function to stop animation.
        public void DigitalRainAnim_pause()
        {
            Console.WriteLine($"Digital Rain Paused");
            isPlaying = false;
        }

        //a function to resume animation.
        public void DigitalRainAnim_resume()
        {
            Console.WriteLine($"Digital Rain Resumed");
            isPlaying = true;
        }

        //the function is to generate a random key with a length
        public string GetKey(int key_length)
        {
            ResetKey();
            int maxKeyLength = key_length > 0 ? Math.Min(key_length, numOfline) : numOfline;

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < maxKeyLength; i++)
            {
                sb.Append(GetAbcASCIIChar());
            }

            keyString = sb.ToString();
            return keyString;
        }

        //------------------------------------------------------------------------------------------------------

        //updating each line with a new length, Y position, and speed.
        private void LineUpdate(int lineNum)
        {
            Console.WriteLine($"Digital Rain Updating Line {lineNum}");
            line_length[lineNum] = rand.Next(line_len_min, line_len_max + 1);
            line_pos[lineNum] = SetYPos(line_length[lineNum]);
            line_speed[lineNum] = rand.Next(line_speed_min, line_speed_max + 1);
        }

        //while moving vertically, the color value changes and the character changes as well.
        private void LineAnimation(int lineNum)
        {
            //Console.WriteLine($"Digital Rain Line {lineNum}");
            int startX = lineNum * lineWidth;
            int currentY = -letterHeight;
            graphics.DrawRectangle(startX, 0, lineWidth, display.Height, bgColor, true);

            bool isKeyMode = keyString.Length > 0;

            List<int> lumins = Enumerable.Range(0, line_length[lineNum]).Select(x => x * 254 / line_length[lineNum] + 10).ToList<int>();
            for (int i = 0; i < line_length[lineNum]; i++)
            {
                double lum = lumins[i] / 255.0;
                Color lumColor = Luminance(textColor, lum);
                //Console.WriteLine($"  Digital Rain Lum {i}={lum} @{startX},{line_pos[lineNum] + currentY} {lumColor}");

                graphics.DrawRectangle(startX, line_pos[lineNum] + currentY, lineWidth, letterHeight, bgColor);
                graphics.PenColor = isKeyMode ? textColor : lumColor;
                graphics.DrawText(startX, line_pos[lineNum] + currentY,
                                  isAlphabetOnly ? GetAbcASCIIChar() : GetASCIIChar(),
                                  fontSize);

                currentY = i * letterHeight;
            }

            graphics.PenColor = headCharColor;
            if (keyString.Length > lineNum)
            {
                // from key
                graphics.DrawText(startX, line_pos[lineNum] + currentY,
                                  keyString[lineNum].ToString(),
                                  fontSize);
            }
            else
            {
                // random
                graphics.DrawText(startX, line_pos[lineNum] + currentY,
                                  isAlphabetOnly ? GetAbcASCIIChar() : GetASCIIChar(),
                                  fontSize);
            }

            line_pos[lineNum] += line_speed[lineNum];
            graphics.Show();

            if (line_pos[lineNum] >= display.Height)
            {
                LineUpdate(lineNum);
            }
        }

        //a function that gets randomly from ASCII code 33 to 126.
        private string GetASCIIChar()
        {
            return ((char)(rand.Next(33, 127))).ToString();
        }

        //a function that gets only alphabets from ASCII code.
        private string GetAbcASCIIChar()
        {
            return ((char)(rand.Next(0, 2) == 0 ? rand.Next(65, 91) : rand.Next(97, 123))).ToString();
        }

        //move the position to start from out of the screen.
        private int SetYPos(int lineLen)
        {
            return lineLen * -20;
        }

        //the function is to remove the generated key
        private void ResetKey()
        {
            keyString = string.Empty;
            lastUpdatedKeyTime = sw.ElapsedMilliseconds;
            Console.WriteLine($"Digital Rain Key reset {lastUpdatedKeyTime}");
        }

        private Color Luminance(Color color, double luminance)
        {
            //return color.WithBrightness(luminance);
            return new Color(hue: color.Hue, brightness: luminance, saturation: color.Saturation, alpha: 255);
        }
    }

}
