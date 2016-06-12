namespace KosselCalibrator.GCode
{
    using System.Collections.Generic;

    public class GCodeCommand
    {
        public GCodeCommand(GCodeType type, int code, IDictionary<char, double> arguments)
        {
            Type = type;
            Code = code;
            Arguments = arguments;
        }

        public GCodeType Type { get; }

        public int Code { get; }

        public IDictionary<char, double> Arguments { get; }
    }
}