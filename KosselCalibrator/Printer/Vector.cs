namespace KosselCalibrator.Printer
{
    public class Vector
    {
        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector()
        {

        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }
    }
}