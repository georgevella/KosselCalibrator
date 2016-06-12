namespace KosselCalibrator.Printer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;

    using KosselCalibrator.Connection;
    using KosselCalibrator.GCode;

    internal class DeltaPrinter : IPrinter
    {
        private GCodeParser _gcodeParser;

        public DeltaPrinter()
        {
            Info = new DeltaPrinterInformation();
            Settings = new DeltaPrinterSettings();
            Connection = new Connection(this);

            _gcodeParser = new GCodeParser();
        }

        public DeltaPrinterSettings Settings { get; }

        public IConnection Connection { get; }

        public DeltaPrinterInformation Info { get; }

        public void Dispose()
        {
            Connection.Dispose();
        }

        public void Connect(string port, BaudRate rate)
        {
            Connection.Port = port;
            Connection.BaudRate = rate;

            Connection.Open();
        }

        public void MoveTo(double x, double y, double z, double feedRate = 5000)
        {
            Connection.WriteLine($"G1 X{x:F2} Y{y:F2} Z{z:F2} F{feedRate:F2}");
            Connection.WaitForText("ok");
        }
        public void MoveTo(Vector vector, double feedRate = 5000)
        {
            Connection.WriteLine($"G1 X{vector.X:F2} Y{vector.Y:F2} Z{vector.Z:F2} F{feedRate:F2}");
            Connection.WaitForText("ok");
        }

        public void MoveToHomingPosition()
        {
            Connection.WriteLine("G28");
            Connection.WaitForText("ok");
        }

        public Vector GetCurrentPosition()
        {
            Connection.WriteLine("M114");
            var line = Connection.ReadLine();

            // X:-100.00 Y:-60.00 Z:-2.00 E:0.00 Count X: 25992 Y:16718 Z:16485
            var parts = line.Trim().Split(' ');
            var arguments = new Dictionary<char, double>();
            for (var i = 0; i < Math.Min(parts.Length, 4); i++)
            {
                var keyvaluepair = parts[i].Split(':');
                var value = double.Parse(keyvaluepair[1]);

                var argName = keyvaluepair[0][0];
                arguments[argName] = value;
            }

            return new Vector(arguments['X'], arguments['Y'], arguments['Z']);
        }

        public void GetPrinterInformation()
        {
            Connection.WriteLine("M503");
            string line;
            while ((line = Connection.ReadLine()) != null)
            {
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
                            Info.HomeOffset = new Vector(
                                command.Arguments['X'],
                                command.Arguments['Y'],
                                command.Arguments['Z']);

                            break;
                        // endstop adjustments
                        case 666:
                            Info.EndstopAdjustment = new Vector(
                                command.Arguments['X'],
                                command.Arguments['Y'],
                                command.Arguments['Z']);

                            break;
                        // delta settings
                        case 665:
                            Info.DeltaSettings = new DeltaSettings(
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

        public void Shutdown()
        {
            Connection.Close();
        }

        public void SetupMechanicalOffsets()
        {
            var points = new[]
            {
                new Vector(-100, -60, 0),
                new Vector(-100, 60, 0),
                new Vector(120, 0, 0),
            };

            for (var i = 0; i < points.Length; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    while (true)
                    {
                        var point = points[j];
                        // x-pole 
                        MoveTo(point.X, point.Y, 10);
                        for (float z = 9; z > -5f; z -= 0.1f)
                        {
                            var state = new Dictionary<string, bool>();
                            ReadEndpointReport(state);

                            if (state["z_min"]) break;

                            MoveTo(point.X, point.Y, z, 1000);
                            Thread.Sleep(50);
                        }

                        var position = GetCurrentPosition();
                        var bedDistance = position.Z - (Settings.ZProbeOffset);

                        var newX = Info.EndstopAdjustment.X + bedDistance;
                        if (Math.Abs(newX - Info.EndstopAdjustment.X) < 0.1)
                        {
                            Console.WriteLine("Verify x-axis calibration? [Y]");

                            switch (Console.ReadKey().Key)
                            {
                                case ConsoleKey.Enter:
                                case ConsoleKey.Y:
                                    MoveTo(point.X, point.Y, 0, 1000);
                                    break;
                            }

                            break;
                        }
                        Connection.WriteLine($"M666 X{newX:F2}");

                        Info.EndstopAdjustment = new Vector(newX, Info.EndstopAdjustment.Y, Info.EndstopAdjustment.Z);

                        Thread.Sleep(100);
                        MoveToHomingPosition();
                    }
                }
            }

        }

        public void SaveSettings()
        {
            Connection.WriteLine("M500");
            Connection.WaitForText("ok");
        }

        public void ReadEndpointReport(IDictionary<string, bool> endStopState)
        {
            Connection.WriteLine("M119");       // send endpoint report status command

            Connection.WaitForText("Reporting endstop status");

            using (var stream = new MemoryStream())
            using (var reader = new StreamReader(stream))
            {
                Connection.ReadUntil("ok", stream);
                stream.Position = 0;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(':');
                    endStopState[parts[0].Trim()] = !"open".Equals(parts[1].Trim());
                }
            }
        }
    }
}