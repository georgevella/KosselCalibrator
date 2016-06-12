namespace KosselCalibrator
{
    using System;

    using KosselCalibrator.Connection;
    using KosselCalibrator.Printer;

    internal class Program
    {
        private static void Main(string[] args)
        {
            using (var printer = new DeltaPrinter())
            {
                printer.Settings.ZProbeOffset = 1.2;

                printer.Connect("COM3", BaudRate.Rate250000);
                printer.MoveToHomingPosition();
                //printer.MoveTo(0, 0, 10);

                printer.SetupMechanicalOffsets();
            }

            Console.ReadKey();
        }
    }
}