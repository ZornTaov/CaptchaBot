//MIT License
//Copyright(c) [2019]
//[Xylex Sirrush Rayne]
//
//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:
//
//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.
using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using DreadBot;

namespace CaptchaBot
{
    public class Captcha
    {
        private string CaptchaCode { get; set; } = "";
        private Stream CaptchaImage { get; set; } = null;

        public Stream GetImage() { return CaptchaImage; }
        public string GetCode() { return CaptchaCode; }

        public Captcha(uint SizeX = 224, uint SizeY = 100, uint Distort = 14, uint CaptchaLength = 8, string FontFamily = "Comic Sans MS", uint FontSize = 30, FontStyle fontStyle = FontStyle.Strikeout)
        {
            Color BackgroundColor = Color.White;
            Color ForegroundColor = Color.Black;

            int xSize = Convert.ToInt32(SizeX);
            int ySize = Convert.ToInt32(SizeY);
            int distortion = Convert.ToInt32(Distort);
            int CharCount = Convert.ToInt32(CaptchaLength);

            Font fontProperties = new Font(FontFamily, FontSize, fontStyle);
            Random rand = new Random();
            CaptchaCode = new string(Enumerable.Repeat("ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz0123456789", CharCount).Select(s => s[rand.Next(s.Length)]).ToArray());
            int newX, newY;
            Stream stream = new MemoryStream();
            Bitmap captchaImage = new Bitmap(xSize, ySize, System.Drawing.Imaging.PixelFormat.Format64bppArgb);
            Bitmap cache = new Bitmap(xSize, ySize, System.Drawing.Imaging.PixelFormat.Format64bppArgb);
            Graphics graphicsTextHolder = Graphics.FromImage(captchaImage);
            graphicsTextHolder.Clear(BackgroundColor);
            graphicsTextHolder.DrawString(CaptchaCode, fontProperties, new SolidBrush(ForegroundColor), new PointF(8.4F, 20.4F));

            for (int y = 0; y < ySize; y++)
            {
                for (int x = 0; x < xSize; x++)
                {
                    newX = (int)(x + (distortion * Math.Sin(Math.PI * y / 64.0)));
                    newY = (int)(y + (distortion * Math.Cos(Math.PI * x / 64.0)));
                    if (newX < 0 || newX >= xSize) newX = 0;
                    if (newY < 0 || newY >= ySize) newY = 0;
                    cache.SetPixel(x, y, captchaImage.GetPixel(newX, newY));
                }
            }
            cache.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
            stream.Position = 0;
            CaptchaImage = stream;

        }
    }
}
