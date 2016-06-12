namespace KosselCalibrator.Printer
{
    using System;

    public interface IPrinter : IDisposable
    {
        DeltaPrinterInformation Info { get; }
    }
}