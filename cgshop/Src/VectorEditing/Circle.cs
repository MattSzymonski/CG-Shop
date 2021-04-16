﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using cgshop.point;

namespace cgshop
{
    public class Circle : Shape
    {
        Point midPoint;
        Point circumferencePoint;
        int thickness;
        Color color;

        public Circle(Point midPoint, Point circumferencePoint, int thickness, Color color)
        {
            this.midPoint = midPoint;
            this.circumferencePoint = circumferencePoint;
            this.thickness = thickness;
            this.color = color;
        }

        public unsafe BitmapImage Draw(BitmapImage canvas)
        {
            var bitmap = new WriteableBitmap(canvas);

            bitmap.Lock();

            byte* pBuffer = (byte*)bitmap.BackBuffer; // Pointer to actual image data in buffer (BGRA32 format (1 byte for each channel))

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;

            { // Midpoint Circle Algorithm
                int r = (int)Math.Sqrt(Math.Pow((midPoint.X - circumferencePoint.X), 2) + Math.Pow((midPoint.Y - circumferencePoint.Y), 2));

                int d = 1 - r; // decider
                int y = 0;
                int x = r;

                // Drawing first 4 points on circumference
                Utils.SetPixel(pBuffer, bitmap, midPoint.X, midPoint.Y + r, color);
                Utils.SetPixel(pBuffer, bitmap, midPoint.X, midPoint.Y - r, color);
                Utils.SetPixel(pBuffer, bitmap, midPoint.X + r, midPoint.Y, color);
                Utils.SetPixel(pBuffer, bitmap, midPoint.X - r, midPoint.Y, color);
                
                while (x > y) // We just need to check for one octant only because the others are simple reflections
                {
                    y++;
                    if (d <= 0) // Choose pixel inside the circle, plot (x, y+1)
                    {
                        d = d + 2 * y + 1;
                    }
                    else // Choose pixel outside the circle, plot (x-1, y+1)
                    {
                        x--;
                        d = d + 2 * y - 2 * x + 1;
                    }

                    if (x < y) // If all points have been printed
                        break;

                    // Drawing point and its reflection in the other octants;
                    Utils.SetPixel(pBuffer, bitmap, x + midPoint.X, y + midPoint.Y, color); // 0
                    Utils.SetPixel(pBuffer, bitmap, x + midPoint.X, -y + midPoint.Y, color); // 7
                    Utils.SetPixel(pBuffer, bitmap, -x + midPoint.X, y + midPoint.Y, color); // 3
                    Utils.SetPixel(pBuffer, bitmap, -x + midPoint.X, -y + midPoint.Y, color); // 4

                    Utils.SetPixel(pBuffer, bitmap, y + midPoint.X, x + midPoint.Y, color); // 1
                    Utils.SetPixel(pBuffer, bitmap, y + midPoint.X, -x + midPoint.Y, color); // 2
                    Utils.SetPixel(pBuffer, bitmap, -y + midPoint.X, x + midPoint.Y, color); // 6
                    Utils.SetPixel(pBuffer, bitmap, -y + midPoint.X, -x + midPoint.Y, color); // 5
                }
            }



            bitmap.Unlock();

            // Convert WritableBitmap to BitmapImage
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(stream);
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }
            return bitmapImage;
        }
    }

}
