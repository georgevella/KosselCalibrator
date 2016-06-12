namespace KosselCalibrator.Printer
{
    public class DeltaSettings
    {
        public DeltaSettings(double diagonalRodLength, double radius, double segmentsPerSecond, double diagonalRodTrimTowerA, double diagonalRodTrimTowerB, double diagonalRodTrimTowerC)
        {
            DiagonalRodLength = diagonalRodLength;
            Radius = radius;
            SegmentsPerSecond = segmentsPerSecond;
            DiagonalRodTrimTower = new Vector(diagonalRodTrimTowerA, diagonalRodTrimTowerB, diagonalRodTrimTowerC);
        }

        public DeltaSettings()
        {

        }

        public double DiagonalRodLength { get; set; }

        public double Radius { get; set; }

        public double SegmentsPerSecond { get; set; }

        public Vector DiagonalRodTrimTower { get; set; }
    }
}