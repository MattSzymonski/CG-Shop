using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cgshop
{
    public class FilterSettings
    {

        // --- Function filters: Graph ---

        //public static List<GraphPoint> inversionFunctionPoints = new List<GraphPoint>() { new GraphPoint(0, 255, true), 
        //                                                                                  new GraphPoint(255, 0, true), };

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
        public delegate int FunctionFormula_Formula(int value, int channelNumer, params object[] otherParams);
        public static FunctionFormula_Formula functionFormula_Formula;

        public static double gammaCoefficient = 3.0;
        public static int CalculateGamma(int value, int channelNumer, params object[] otherParams)
        {
            if (channelNumer < 0 || channelNumer > 3)
                throw new Exception("Wrong channel settings passed to Calculate Gamma function");

            if (otherParams.Length != 1)
                throw new Exception("Wrong additional parameters passed to Calculate Gamma function");

            double result = value;
            if (channelNumer != 3)
            {
                double gammaCoefficient = (double)otherParams[0];
                result = Utils.Clamp((int)(255 * Math.Pow((double)value / 255, 1 / gammaCoefficient)), 0, 255);
            }
            return (int)result;
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
