using System.Collections.Generic;
using FluentAssertions;
using KosselCalibrator.GCode;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KosselCalibrator.Tests.GCode
{
    [TestClass]
    public class GCodeParserTests
    {
        [TestMethod]
        public void Parse()
        {
            var parser = new GCodeParser();

            var command = parser.Parse("  M92 X80.00 Y80.00 Z80.00 E836.00");

            command.Code.Should().Be(92);
            command.Arguments.Should().HaveCount(4).
                And.Subject.Should().Contain(new[]
                {
                    new KeyValuePair<char, double>('X', 80.0),
                    new KeyValuePair<char, double>('Y', 80.0),
                    new KeyValuePair<char, double>('Z', 80.0),
                    new KeyValuePair<char, double>('E', 836.0),
                });
        }
    }
}
