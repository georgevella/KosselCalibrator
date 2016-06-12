namespace KosselCalibrator.Connection
{
    using System;
    using System.IO;

    public interface IConnection : IDisposable
    {
        string Port { get; set; }

        BaudRate BaudRate { get; set; }

        void Open();

        void WriteLine(string line);

        void Close();

        void WaitForText(string text);

        void ReadUntil(string line, Stream stream);

        void ReadUntil(string line, TextWriter writer);

        string ReadLine();
    }
}