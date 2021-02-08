using System;
using System.Collections.Generic;
using static System.FormattableString;
using static System.Math;

namespace SearchAThing
{

    /// <summary>
    /// Lookup Table genertor
    /// </summary>
    public class LUT
    {

        public int LUTSize { get; private set; }

        public Func<double, double> Fx { get; private set; }

        public double XFrom { get; private set; }
        public double XTo { get; private set; }
        public double XStep { get; private set; }

        public double YFrom { get; private set; }
        public double YTo { get; private set; }
        public double YStep { get; private set; }

        List<double> lut = null;
        public IReadOnlyList<double> YXTable => lut;
        MathNet.Numerics.Interpolation.IInterpolation invInterp = null;

        public double ComputeX(double y)
        {
            if (y.EqualsTol(Abs(YStep / 2), YFrom)) return XFrom;
            if (y.EqualsTol(Abs(YStep / 2), YTo)) return XTo;
            return invInterp.Interpolate(y);
        }

        public LUT(Func<double, double> fx, double xFrom, double xTo, int lutSize)
        {
            Fx = fx;
            LUTSize = lutSize;
            XFrom = xFrom;
            XTo = xTo;
            XStep = (XTo - XFrom) / (LUTSize - 1);

            YFrom = fx(XFrom);
            YTo = fx(XTo);
            YStep = (YTo - YFrom) / (LUTSize - 1);

            build(LUTSize);
        }

        void build(int internalLUTSize)
        {
            var xset = new List<double>();
            var yset = new List<double>();

            var x = XFrom;

            for (int k = 0; k < internalLUTSize; ++k, x += XStep)
            {
                xset.Add(x);
                yset.Add(Fx(x));
            }

            invInterp = SciToolkit.LinearSplineInterpolate(yset, xset);
        }

    };

}