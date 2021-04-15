using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace cgshop
{

    public class Drawer
    {
        public unsafe void SetPixel(byte* pBuffer, WriteableBitmap bitmap, int x, int y, Color color)
        {
            for (int i = 0; i < 3; i++) // For each color channel
            {
                pBuffer[4 * x + (y * bitmap.BackBufferStride) + i] = color[i];
            }
        }

        public unsafe BitmapImage Point(BitmapImage original, Point point, string operation)
        {
            var bitmap = new WriteableBitmap(original);

            bitmap.Lock();

            byte* pBuffer = (byte*)bitmap.BackBuffer; // Pointer to actual image data in buffer (BGRA32 format (1 byte for each channel))

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;

            SetPixel(pBuffer, bitmap, (int)point.X, (int)point.Y, new Color(255, 255, 255, 255));

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
