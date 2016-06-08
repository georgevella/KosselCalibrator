using System.Collections.Generic;

namespace KosselCalibrator.GCode
{
    public class GCodeCommand
    {
        public GCodeType Type { get; }

        public int Code { get; }

        public IDictionary<char, double> Arguments { get; }

        public GCodeCommand(GCodeType type, int code, IDictionary<char, double> arguments)
        {
            Type = type;
            Code = code;
            Arguments = arguments;
        }
    }
}