namespace KosselCalibrator.Connection
{
    using System;
    using System.IO;
    using System.IO.Ports;
    using System.Text;

    using KosselCalibrator.GCode;
    using KosselCalibrator.Printer;

    public class Connection : IConnection
    {
        private readonly GCodeParser _gcodeParser;

        private readonly IPrinter _printer;

        private readonly SerialPort _serialPort;

        public Connection(IPrinter printer)
        {
            _printer = printer;
            _serialPort = new SerialPort();
            _serialPort.ErrorReceived +=
                (sender, eventArgs) => Console.WriteLine($"Serial Error: {eventArgs.EventType}");
            _serialPort.DtrEnable = true;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _gcodeParser = new GCodeParser();
        }

        public string Port
        {
            get
            {
                return _serialPort.PortName;
            }
            set
            {
                _serialPort.PortName = value;
            }
        }

        public BaudRate BaudRate
        {
            get
            {
                return (BaudRate)_serialPort.BaudRate;
            }
            set
            {
                _serialPort.BaudRate = (int)value;
            }
        }

        public void Open()
        {
            _serialPort.Open();

            WaitForText("start");

            // parse M503 output
            try
            {
                while (_serialPort.BytesToRead > 0)
                {
                    var line = DoReadLine();

                    if (line.StartsWith("echo:"))
                    {
                        line = line.Substring("echo:".Length).Trim();
                        if (line.Length <= 0)
                        {
                            continue;
                        }

                        var command = _gcodeParser.Parse(line);
                        if (command == null)
                        {
                            continue;
                        }

                        switch (command.Code)
                        {
                            // home offset
                            case 206:
                                _printer.Info.HomeOffset = new Vector(
                                    command.Arguments['X'],
                                    command.Arguments['Y'],
                                    command.Arguments['Z']);

                                break;
                            // endstop adjustments
                            case 666:
                                _printer.Info.EndstopAdjustment = new Vector(
                                    command.Arguments['X'],
                                    command.Arguments['Y'],
                                    command.Arguments['Z']);

                                break;
                                // delta settings
                            case 665:
                                _printer.Info.DeltaSettings = new DeltaSettings(
                                    command.Arguments['L'],
                                    command.Arguments['R'],
                                    command.Arguments['S'],
                                    command.Arguments['A'],
                                    command.Arguments['B'],
                                    command.Arguments['C']);

                                break;
                        }
                    }
                }
            }
            catch (TimeoutException)
            {
                Console.WriteLine(">> timeout exception");
            }
        }

        public void WriteLine(string line)
        {
            Console.WriteLine($"< {line}");
            _serialPort.WriteLine(line);
        }

        public void Close()
        {
            _serialPort.Close();
        }

        public void Dispose()
        {
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort.Dispose();
        }

        public void WaitForText(string text)
        {
            while (_serialPort.IsOpen)
            {
                try
                {
                    while (_serialPort.BytesToRead > 0)
                    {
                        var ch = DoReadLine();

                        if (ch.Equals(text))
                        {
                            return;
                        }
                    }
                }
                catch (TimeoutException)
                {
                }
            }
        }

        public void ReadUntil(string line, Stream stream)
        {
            using (var writer = new StreamWriter(stream, Encoding.ASCII, 2048, true))
            {
                ReadUntil(line, writer);
            }
        }

        public void ReadUntil(string line, TextWriter writer)
        {
            while (_serialPort.IsOpen)
            {
                try
                {
                    while (_serialPort.BytesToRead > 0)
                    {
                        var ch = DoReadLine();
                        if (ch.Equals(line))
                        {
                            return;
                        }

                        writer.WriteLine(ch);
                    }
                }
                catch (TimeoutException)
                {
                }
            }
        }

        public string ReadLine()
        {
            try
            {
                return DoReadLine(true);
            }
            catch (TimeoutException)
            {
                return null;
            }
        }

        private string DoReadLine(bool showOutput = true)
        {
            var ch = _serialPort.ReadLine();
            if (showOutput)
            {
                var fg = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine($"> {ch}");

                Console.ForegroundColor = fg;
            }
            return ch;
        }
    }

    internal static class SerialPortExtensions
    {
    }
}