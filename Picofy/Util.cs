using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Picofy
{
    public static class Util
    {
        public static Color AverageColorOfImage(BitmapImage bmp)
        {
            int stride = bmp.PixelWidth*4;
            int size = bmp.PixelHeight*stride;
            byte[] pixels = new byte[size];
            bmp.CopyPixels(pixels, stride, 0);

            int totalR = 0;
            int totalG = 0;
            int totalB = 0;

            int minDiversion = 15; // drop pixels that do not differ by at least minDiversion between color values (white, gray or black)
            int dropped = 0; // keep track of dropped pixels

            for (int y = 0; y < bmp.PixelHeight; y++)
            {
                for (int x = 0; x < bmp.PixelWidth; x++)
                {
                    int index = y * stride + 4 * x;
                    byte red = pixels[index];
                    byte green = pixels[index + 1];
                    byte blue = pixels[index + 2];
                    //byte alpha = pixels[index + 3];

                    if ((Math.Abs(red - green) > minDiversion || Math.Abs(red - blue) > minDiversion || Math.Abs(green - blue) > minDiversion)
                        && (red + green + blue) < 600 && (red + green + blue) > 100)
                    {
                        totalR += red;
                        totalG += green;
                        totalB += blue;
                    }
                    else
                    {
                        dropped++;
                    }
                }
            }

            int count = size - dropped;

            return Color.Multiply(Color.FromRgb((byte)(totalR / count), (byte)(totalG / count), (byte)(totalB / count)), 6);
        }
    }
}
