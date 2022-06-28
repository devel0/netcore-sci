namespace SearchAThing
{

    public static partial class SciToolkit
    {

        /// <summary>
        /// tolerance used when comparing normalized vectors.
        /// considering range is [0..1] the valued should not smaller than double precision (15-16 digits) ie.1e-15
        /// </summary>
        public const double NormalizedLengthTolerance = 1e-4;

        /// <summary>
        /// tolerance over 2PI (rad)
        /// </summary>
        public const double TwoPIRadTol = 1e-10;

    }

}