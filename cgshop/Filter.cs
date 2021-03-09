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
    public class FilterEntry
    {
        public string Name { get; set; }
        public IFilter Filter { get; }

        public FilterEntry(string name, IFilter filter)
        {
            this.Name = name;
            this.Filter = filter;
        }
    }



    public interface IFilter
    {
        unsafe BitmapImage Apply(BitmapImage original);
    }

    [Serializable]
    public class FunctionFilter : IFilter
    {
        public IFunction Function { get; set; }

        public FunctionFilter(IFunction function)
        {
            this.Function = function;
        }

        public unsafe BitmapImage Apply(BitmapImage original)
        {
            return Function.Apply(original);
        }
    }

    [Serializable]
    public class ConvolutionFilter : IFilter
    {
        public double[,] Kernel { get; }
        public int KernelWidth { get; } // Column count
        public int KernelHeight { get; } // Row count
        public double Divisor { get; }

        public ConvolutionFilter(double[,] kernel, double divisor)
        {
            this.Kernel = kernel;
            this.KernelWidth = kernel.GetLength(1);
            this.KernelHeight = kernel.GetLength(0);
            this.Divisor = divisor;
        }

        public unsafe BitmapImage Apply(BitmapImage original)
        {
            if (Divisor == 0)
                throw new ArgumentException("Convolution filter divisor cannot be equal to 0");

            var bitmap = new WriteableBitmap(original);

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;


            bitmap.Lock();

            byte[] newBuffer = new byte[width * height * bytesPerPixel];

            unsafe
            {
                byte* pBuffer = (byte*)bitmap.BackBuffer.ToPointer(); // Pointer to actual image data in buffer (BGRA32 format (1 byte for each channel))

                int kernelFirstRow = -(KernelHeight - 1) / 2;
                int kernelLastRow = (KernelHeight - 1) / 2;
                int kernelFirstColumn = -(KernelWidth - 1) / 2;
                int kernelLastColumn = (KernelWidth - 1) / 2;

                for (int y = 0; y < height; y++) // Row
                {
                    for (int x = 0; x < width; x++) // Column
                    {
                        double[] accumulatedValue = new double[4] { 0, 0, 0, 0 };

                        for (int i = 0; i < 4; i++) // Channels
                        {
                            if (i != 3) // Color channels
                            {

                                for (int yK = kernelFirstRow; yK <= kernelLastRow; yK++) // Row
                                {
                                    for (int xK = kernelFirstColumn; xK <= kernelLastColumn; xK++) // Column
                                    {
                                        int neighbourPixelX = x + xK;
                                        int neighbourPixelY = y + yK;

                                        int neighbourPixelIndex = (4 * neighbourPixelX + (neighbourPixelY * bitmap.BackBufferStride));

                                        var kernelValue = (int)Kernel[yK - kernelFirstRow, xK - kernelFirstColumn];
                                        if (neighbourPixelX < 0 || neighbourPixelX > width - 1 || neighbourPixelY < 0 || neighbourPixelY > height - 1) // Image edge case
                                        {
                                            accumulatedValue[i] += (int)pBuffer[4 * x + (y * bitmap.BackBufferStride) + i]; // Edge behaviour
                                        }
                                        else
                                        {
                                            accumulatedValue[i] += kernelValue * pBuffer[neighbourPixelIndex + i]; // Add value of current channel of current pixel
                                        }
                                    }
                                }
                            }
                            else // Alpha channel
                            {
                                int index = 4 * x + (y * bitmap.BackBufferStride) + i;
                                accumulatedValue[i] = pBuffer[index];
                            }
                        }

                        for (int i = 0; i < 4; i++) // Channels
                        {
                            int index = 4 * x + (y * bitmap.BackBufferStride) + i;

                            if (i != 3) // Color
                            {
                                newBuffer[index] = Convert.ToByte(Utils.Clamp((int)(accumulatedValue[i] / Divisor), 0, 255));
                            }
                            else  // Alpha
                            {
                                newBuffer[index] = Convert.ToByte(Utils.Clamp((int)accumulatedValue[i], 0, 255));
                            }
                        }
                    }
                }
            }

            bitmap.Unlock();

            // Create new bitmap from create buffer
            var newBitmap = BitmapImage.Create((int)width, (int)height, original.DpiX, original.DpiY, bitmap.Format, null, newBuffer, (int)bytesPerPixel * width);

            // Convert WritableBitmap to BitmapImage
            BitmapImage bitmapImage = new BitmapImage();
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(newBitmap));
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





    public interface IFunction
    {
        unsafe BitmapImage Apply(BitmapImage original);
    }

    [Serializable]
    public class FunctionGraph : IFunction
    {
        public Graph Graph { get; }

        public FunctionGraph(Graph functionGraph)
        {
            this.Graph = functionGraph;
        }

        public unsafe BitmapImage Apply(BitmapImage original)
        {
            var bitmap = new WriteableBitmap(original);

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;

            bitmap.Lock();

            unsafe
            {
                byte* pBuffer = (byte*)bitmap.BackBuffer; // Pointer to actual image data in buffer (BGRA32 format (1 byte for each channel))

                // Precalculate slope factors of functions between points
                List<(double, double)> functionFactors = new List<(double, double)>();
                for (int i = 0; i < Graph.points.Count - 1; i++)
                {
                    double slopeFactor = (Graph.points[i + 1].Value.Y - Graph.points[i].Value.Y) / (Graph.points[i + 1].Value.X - Graph.points[i].Value.X);
                    double constant = Graph.points[i + 1].Value.Y - slopeFactor * Graph.points[i + 1].Value.X;
                    functionFactors.Add((slopeFactor, constant));
                }

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Find new value in graph function
                        for (int i = 0; i < 3; i++) // For each channel
                        {
                            for (int p = 0; p < Graph.points.Count - 1; p++)
                            {
                                int pixelChannelValue = (int)pBuffer[4 * x + (y * bitmap.BackBufferStride) + i];

                                Point pointOnLeftValue = Graph.points[p].Value;
                                Point pointOnRightValue = Graph.points[p + 1].Value;

                                if (pixelChannelValue >= pointOnRightValue.X) // Incorrect interval (go to next interval on right)
                                {
                                    if (pixelChannelValue == pointOnRightValue.X) // Edge
                                    {
                                        pBuffer[4 * x + (y * bitmap.BackBufferStride) + i] = (byte)pointOnRightValue.Y;
                                        break;
                                    }
                                }
                                else // Correct interval
                                {
                                    if (pixelChannelValue == pointOnLeftValue.X) // Edge
                                    {
                                        pBuffer[4 * x + (y * bitmap.BackBufferStride) + i] = (byte)pointOnLeftValue.Y;
                                        break;
                                    }

                                    // Calculate new value for pixel
                                    pBuffer[4 * x + (y * bitmap.BackBufferStride) + i] = (byte)((functionFactors[p].Item1 * pixelChannelValue) + functionFactors[p].Item2);
                                    break;
                                }
                            }
                        }

                        //// Index is byte offset
                        //var bluePixelByte = pBuffer[4 * x + (y * bitmap.BackBufferStride)] = 0;
                        //var greenPixelByte = pBuffer[4 * x + (y * bitmap.BackBufferStride) + 1] = 255;
                        //var redPixelByte = pBuffer[4 * x + (y * bitmap.BackBufferStride) + 2] = 0;
                        //var alphaPixelByte = pBuffer[4 * x + (y * bitmap.BackBufferStride) + 3] = 255;
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

            return bitmapImage;
        }
    }

    [Serializable]
    public class FunctionFormula : IFunction
    {
        public FilterSettings.FunctionFormula_Formula Formula { get; }
        object[] otherFunctionParams;

        public FunctionFormula(FilterSettings.FunctionFormula_Formula functionFormula, params object[] otherFunctionParams)
        {
            this.Formula = functionFormula;
            this.otherFunctionParams = otherFunctionParams;
        }

        public unsafe BitmapImage Apply(BitmapImage original)
        {
            var bitmap = new WriteableBitmap(original);

            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;

            bitmap.Lock();

            unsafe
            {
                byte* pBuffer = (byte*)bitmap.BackBuffer; // Pointer to actual image data in buffer (BGRA32 format (1 byte for each channel))

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int[] pixel = new int[4];

                        for (int i = 0; i < 4; i++)
                            pixel[i] = pBuffer[4 * x + (y * bitmap.BackBufferStride) + i];

                        int[] result = Formula(pixel, otherFunctionParams);

                        for (int i = 0; i < 4; i++)
                            pBuffer[4 * x + (y * bitmap.BackBufferStride) + i] = (byte)result[i];
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

            return bitmapImage;
        }
    }

}
