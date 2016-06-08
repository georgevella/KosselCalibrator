namespace KosselCalibrator.Printer
{
    public class DeltaPrinterInformation
    {
        public Vector HomeOffset { get; internal set; }

        public Vector EndstopAdjustment { get; internal set; }

        public DeltaSettings DeltaSettings { get; }

        public DeltaPrinterInformation()
        {
            HomeOffset = new Vector();

            EndstopAdjustment = new Vector();

            DeltaSettings = new DeltaSettings();
        }
    }
}