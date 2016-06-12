namespace KosselCalibrator.GCode
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class GCodeParser
    {
        private readonly Regex _gCodeRegex = new Regex(@"^(?:(?:M|G)(\d{1,3}))", RegexOptions.Compiled);

        public GCodeCommand Parse(string line)
        {
            if (line == null)
            {
                throw new ArgumentNullException(nameof(line));
            }
            if (string.IsNullOrEmpty(line))
            {
                throw new ArgumentException("Argument is null or empty", nameof(line));
            }

            line = line.Trim();

            // not a gcode / mcode command
            if (!line.StartsWith("M") && !line.StartsWith("G"))
            {
                return null;
            }

            if (!_gCodeRegex.IsMatch(line))
            {
                return null;
            }

            // split command in parts
            var parts = line.Split(' ');

            // parse identity part
            var gcodeType = parts[0][0] == 'M' ? GCodeType.MCode : GCodeType.GCode;

            var rawCommand = parts[0].Substring(1);
            var command = int.Parse(rawCommand);

            // parse arguments
            var arguments = new Dictionary<char, double>();
            for (var i = 1; i < parts.Length; i++)
            {
                var rawValue = parts[i].Substring(1);
                var value = double.Parse(rawValue);

                var argName = parts[i][0];
                arguments[argName] = value;
            }

            return new GCodeCommand(gcodeType, command, arguments);
        }
    }
}