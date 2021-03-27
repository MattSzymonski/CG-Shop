using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace cgshop
{
    public class FilterSettings
    {

        // --- Function filters: Graph ---

        // First and last points can be moved only vertically
        public static List<GraphPoint> identityFunctionPoints = new List<GraphPoint>() { new GraphPoint(0, 0),
                                                                                          new GraphPoint(255, 255) };

        public static List<GraphPoint> inversionFunctionPoints = new List<GraphPoint>() { new GraphPoint(0, 255),
                                                                                          new GraphPoint(255, 0) };

        public static List<GraphPoint> brightnessCorrectionFunctionPoints = new List<GraphPoint>() { new GraphPoint(0, 50),
                                                                                                     new GraphPoint(205, 255),
                                                                                                     new GraphPoint(255, 255) };

        public static List<GraphPoint> contrastEnhancementFunctionPoints = new List<GraphPoint>() { new GraphPoint(0, 0),
                                                                                                    new GraphPoint(50, 0),
                                                                                                    new GraphPoint(205, 255),
                                                                                                    new GraphPoint(255, 255) };


        // --- Function filters: Formula ---
        public unsafe delegate void FunctionFormula_Formula(byte* pBuffer, WriteableBitmap bitmap, params object[] otherParams);
        public static FunctionFormula_Formula functionFormula_Formula;

        public static double gammaCoefficient = 3.0;
        public static unsafe void CalculateGamma(byte* pBuffer, WriteableBitmap bitmap, params object[] otherParams)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int stride = bitmap.BackBufferStride;
            int bytesPerPixel = (bitmap.Format.BitsPerPixel + 7) / 8;

            if (otherParams.Length != 1)
                throw new Exception("Wrong additional parameters passed to Calculate Gamma function");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    double gammaCoefficient = (double)otherParams[0];

                    for (int i = 0; i < 3; i++) // For each color channel
                    {
                        pBuffer[4 * x + (y * bitmap.BackBufferStride) + i] = (byte)Utils.Clamp((int)(255 * Math.Pow((double)pBuffer[4 * x + (y * bitmap.BackBufferStride) + i] / 255, 1 / gammaCoefficient)), 0, 255);
                    }
                }
            }
        }

        public static unsafe void CalculateGrayscale(byte* pBuffer, WriteableBitmap bitmap, params object[] otherParams)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    byte gray = (byte)(pBuffer[4 * x + (y * bitmap.BackBufferStride) + 0] * .21 + pBuffer[4 * x + (y * bitmap.BackBufferStride) + 1] * .71 + pBuffer[4 * x + (y * bitmap.BackBufferStride) + 2] * .071);

                    for (int i = 0; i < 3; i++) // For each color channel
                    {
                        pBuffer[4 * x + (y * bitmap.BackBufferStride) + i] = gray;
                    }
                }
            }
        }

        public static int octreeColorQuantization_maxColors = 16;
        public static unsafe void CalculateOctreeColorQuantization(byte* pBuffer, WriteableBitmap bitmap, params object[] otherParams)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int maxColors = (int)otherParams[0];

            OctreeColorQuantizer octreeColorQuantizer = new OctreeColorQuantizer(maxColors);
            
            for (int y = 0; y < height; y++) // Prepare quantization
            {
                for (int x = 0; x < width; x++)
                {
                    octreeColorQuantizer.AddPixelColor(pBuffer[4 * x + (y * bitmap.BackBufferStride) + 0], pBuffer[4 * x + (y * bitmap.BackBufferStride) + 1], pBuffer[4 * x + (y * bitmap.BackBufferStride) + 2]);
                }
            }

            for (int y = 0; y < height; y++) // Write new pixel values from generated palette
            {
                for (int x = 0; x < width; x++)
                {
                    (int b, int g, int r) newColor = octreeColorQuantizer.GetNearestPaletteColor(pBuffer[4 * x + (y * bitmap.BackBufferStride) + 0], pBuffer[4 * x + (y * bitmap.BackBufferStride) + 1], pBuffer[4 * x + (y * bitmap.BackBufferStride) + 2]);

                    pBuffer[4 * x + (y * bitmap.BackBufferStride) + 0] = (byte)newColor.b;
                    pBuffer[4 * x + (y * bitmap.BackBufferStride) + 1] = (byte)newColor.g;
                    pBuffer[4 * x + (y * bitmap.BackBufferStride) + 2] = (byte)newColor.r;
                }
            }
        }

        public static int[] averageDithering_k = { 4, 4, 4 };
        class LevelInterval
        {
            public int intervalStart = 0;
            public int intervalEnd = 0;
            public int intervalLength = 0;

            public int pixelCount = 0;
            public int pixelSum = 0;
            public int threshold = 0;

            public LevelInterval(int intervalLength, int index)
            {
                this.intervalStart = intervalLength * index;
                this.intervalEnd = intervalLength * (index + 1);
                this.intervalLength = intervalLength;
            }

            public void AddPixelValueToInterval(int value)
            {
                pixelSum += value;
                pixelCount++;
            }

            public void CalculateAverageThreshold()
            {
                if (pixelCount != 0)
                    threshold = pixelSum / pixelCount;
                else
                    threshold = intervalStart + ((int)intervalLength / 2);
            }

        }      
        public static unsafe void CalculateAverageDithering(byte* pBuffer, WriteableBitmap bitmap, params object[] otherParams)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            List<List<LevelInterval>> levelIntervals = new List<List<LevelInterval>>();

            // Set up intervals for each channel
            for (int c = 0; c < 3; c++)
            {
                int K = (int)((int[])otherParams[0])[c];
                List<LevelInterval> channelLevelIntervals = new List<LevelInterval>();
                for (int i = 0; i < K - 1; i++)
                {
                    channelLevelIntervals.Add(new LevelInterval(255 / (K - 1), i));
                }
                levelIntervals.Add(channelLevelIntervals);
            }

            // Sum channel values for each interval to calculate threshold
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int i = 0; i < 3; i++) // For each color channel
                    {
                        int channelValue = pBuffer[4 * x + (y * bitmap.BackBufferStride) + i];
                        int intervalIndex = (int)Math.Floor((double)(channelValue / (levelIntervals[i][0].intervalLength + 1)));
                        levelIntervals[i][intervalIndex].AddPixelValueToInterval(channelValue);
                    }
                }
            }

            // After last pixel, calculate threshold
            foreach (var levelInterval in levelIntervals)
            {
                foreach (var channelLevelInterval in levelInterval)
                {
                    channelLevelInterval.CalculateAverageThreshold();
                }
            }

            // Set new values
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int i = 0; i < 3; i++) // For each color channel
                    {
                        int channelValue = pBuffer[4 * x + (y * bitmap.BackBufferStride) + i];
                        int intervalIndex = (int)Math.Floor((double)(channelValue / (levelIntervals[i][0].intervalLength + 1)));
                        LevelInterval interval = levelIntervals[i][intervalIndex];

                        if (channelValue < interval.threshold)
                            pBuffer[4 * x + (y * bitmap.BackBufferStride) + i] = (byte)interval.intervalStart;
                        else
                            pBuffer[4 * x + (y * bitmap.BackBufferStride) + i] = (byte)interval.intervalEnd;
                    }
                }
            }
        }




        // --- Convolution filters ---

        // Row count, Column count
        public static double blurDivisor = 9;
        public static double[,] blurKernel = new double[3, 3] { { 1, 1, 1 },
                                                                { 1, 1, 1 },
                                                                { 1, 1, 1 } };

        public static double gaussianBlurDivisor = 80;
        public static double[,] gaussianBlurKernel = new double[5, 5] { { 0, 1, 2, 1, 0 },
                                                                        { 1, 4, 8, 4, 1 },
                                                                        { 2, 8, 16, 8, 2 },
                                                                        { 1, 4, 8, 4, 1 },
                                                                        { 0, 1, 2, 1, 0 } };

        public static double sharpenDivisor = 1;
        public static double[,] sharpenKernel = new double[3, 3] { { -1, -1, -1 },
                                                                   { -1,  9, -1 },
                                                                   { -1, -1, -1 } };

        public static double edgeDetectionDivisor = 1;
        public static double[,] edgeDetectionKernel = new double[3, 3] { { -1, -1, -1 },
                                                                         { -1,  8, -1 },
                                                                         { -1, -1, -1 } };

        public static double embossDivisor = 1;
        public static double[,] embossKernel = new double[3, 3] { { 1,  1,  0 },
                                                                  { 1,  1, -1 },
                                                                  { 0, -1, -1 } };
    }
}
