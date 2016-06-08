using System;
using System.IO.Ports;
using KosselCalibrator.GCode;
using KosselCalibrator.Printer;

namespace KosselCalibrator.Connection
{
    public class Connection : IConnection
    {
        private readonly IPrinter _printer;
        private readonly SerialPort _serialPort;
        private readonly GCodeParser _gcodeParser;

        public Connection(IPrinter printer)
        {
            _printer = printer;
            _serialPort = new SerialPort();
            _serialPort.ErrorReceived += (sender, eventArgs) => Console.WriteLine($"Serial Error: {eventArgs.EventType}");
            _gcodeParser = new GCodeParser();
        }

        public string Port
        {
            get { return _serialPort.PortName; }
            set { _serialPort.PortName = value; }
        }

        public BaudRate BaudRate
        {
            get { return (BaudRate)_serialPort.BaudRate; }
            set { _serialPort.BaudRate = (int)value; }
        }

        public void Open()
        {
            _serialPort.Open();

            WaitForText(_serialPort, "start");

            // parse M503 output
            try
            {
                while (_serialPort.BytesToRead > 0)
                {
                    var line = _serialPort.ReadLine();
                    if (line.StartsWith("echo:"))
                    {
                        line = line.Substring("echo:".Length).Trim();
                        if (line.Length <= 0) continue;

                        var command = _gcodeParser.Parse(line);

                        switch (command.Code)
                        {
                            case 206:
                                var homeOffsetVector = new Vector(command.Arguments['X'], command.Arguments['Y'], command.Arguments['Z']);

                                _printer.DeltaPrinterInformation.HomeOffset = homeOffsetVector;

                                break;
                        }
                    }
                }
            }
            catch (TimeoutException)
            {

            }



        }

        private static void WaitForText(SerialPort serialPort, string text)
        {
            while (serialPort.IsOpen)
            {
                try
                {
                    while (serialPort.BytesToRead > 0)
                    {
                        var ch = serialPort.ReadLine();
                        Console.WriteLine($"> {ch}");

                        if (ch.Equals(text)) return;
                    }
                }
                catch (TimeoutException)
                {

                }
            }
        }
    }
}