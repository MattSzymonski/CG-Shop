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
    public class RGBToYCbCrConverter // RGB to Y'CbCr conversion
    {
        public unsafe (BitmapImage Y, BitmapImage Cb, BitmapImage Cr) Apply(BitmapImage original)
        {
            BitmapImage[] result = new BitmapImage[3];
          

            for (int i = 0; i < 3; i++)
            {
                var bitmap = new WriteableBitmap(original);

                int width = bitmap.PixelWidth;
                int height = bitmap.PixelHeight;

                bitmap.Lock();

                byte* pBuffer = (byte*)bitmap.BackBuffer; // Pointer to actual image data in buffer (BGRA32 format (1 byte for each channel))

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {  
                        int B = pBuffer[4 * x + (y * bitmap.BackBufferStride) + 0];
                        int G = pBuffer[4 * x + (y * bitmap.BackBufferStride) + 1];
                        int R = pBuffer[4 * x + (y * bitmap.BackBufferStride) + 2];

                        switch(i)
                        {
                            case 0:
                                {
                                    byte Y = (byte)(16.0 + 1.0 / 256.0 * (65.738 * (double)R + 129.057 * (double)G + 25.064 * (double)B));
                                    for (int c = 0; c < 3; c++) // For each color channel
                                    {
                                        pBuffer[4 * x + (y * bitmap.BackBufferStride) + c] = Y;
                                    }
                                }
                                break;
                            case 1:
                                {
                                    byte Cb = (byte)(128.0 + 1.0 / 256.0 * (-37.945 * (double)R - 74.494 * (double)G + 112.439 * (double)B));
                                    int[] colorA = new int[] { 0, 255, 127 };
                                    int[] colorB = new int[] { 255, 0, 127 };

                                    for (int c = 0; c < 3; c++) // For each color channel
                                    {
                                        byte colorResult = (byte)( (1.0 - (double)Cb) * colorA[c] + (double)Cb * colorB[c]);
                                        pBuffer[4 * x + (y * bitmap.BackBufferStride) + c] = colorResult;
                                    }
                                }
                                break;
                            case 2:
                                {
                                    byte Cr = (byte)(128.0 + 1.0 / 256.0 * (112.439 * (double)R - 94.154 * (double)G - 18.285 * (double)B));
                                    int[] colorA = new int[] { 127, 255, 0 };
                                    int[] colorB = new int[] { 127, 0, 255 };

                                    for (int c = 0; c < 3; c++) // For each color channel
                                    {
                                        byte colorResult = (byte)((1.0 - (double)Cr) * colorA[c] + (double)Cr * colorB[c]);
                                        pBuffer[4 * x + (y * bitmap.BackBufferStride) + c] = colorResult;
                                    }
                                }
                                break;
                            default:
                                throw new Exception("Other channels not implmented");
                        }
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

                result[i] = bitmapImage;
            }

            return (result[0],result[1],result[2]);
        }

    }
}
