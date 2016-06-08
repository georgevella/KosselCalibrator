using KosselCalibrator.Connection;

namespace KosselCalibrator.Printer
{
    internal class DeltaPrinter : IPrinter
    {
        public DeltaPrinterInformation DeltaPrinterInformation { get; }

        public IConnection Connection { get; }

        public DeltaPrinter()
        {
            DeltaPrinterInformation = new DeltaPrinterInformation();
            Connection = new Connection.Connection(this);
        }
    }
}