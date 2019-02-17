using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace telnot
{
    class TelnetClient : ITelnetClient
    {
        private readonly TcpClient _tcpClient;
        private readonly string _lineEnd;
        private const string LOGIN = "ogin:";
        private const string PASSWORD = "assword:";
        private const string XFF = "\0xFF";

        public TelnetClient(IPAddress host, int port = 23, string lineEnd = "\r\n", int connectionTimeout = 1)
        {
            _tcpClient = new TcpClient();
            var result = _tcpClient.BeginConnect(host, port, null, null);
            if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(connectionTimeout)))
                throw new Exception("Failed to connect.");

            _lineEnd = lineEnd;
        }

        public Task StartReading(Action<string> callback = null)
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    var data = ReadAvaiable();
                    if (data.Length > 0) callback?.Invoke(ANSI.RemoveCursorPosition(data));
                }
            });
        }

        public string Login(string username, string password)
        {
            string read = ReadIncluding(LOGIN);
            if (!read.Contains(LOGIN))
                throw new Exception("Failed to connect : no login prompt");
            WriteLine(username);

            read += ReadIncluding(PASSWORD);
            if (!read.Contains(PASSWORD))
                throw new Exception("Failed to connect : no password prompt");
            WriteLine(password);

            read += ReadIncluding(_lineEnd);

            return read;
        }

        public Task<string> LoginAsync(string username, string password) =>
            Task.Factory.StartNew(() => Login(username, password));

        public void Write(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data.Replace(XFF, XFF + XFF));
            _tcpClient.GetStream().Write(buffer, 0, buffer.Length);
        }

        public void WriteLine(string cmd) => Write(cmd + _lineEnd);

        public string Read(Func<string, bool> condition)
        {
            if (!_tcpClient.Connected) throw new Exception("Not connected.");
            if (!_tcpClient.GetStream().CanRead) throw new Exception("Cant read Network Stream.");
            while (_tcpClient.Available < 0) Thread.Sleep(10);

            string text = "";
            do { text += TelnetNegociator.Negociate(_tcpClient.GetStream()); }
            while (condition(text));

            return text;
        }

        private string ReadIncluding(string textCheck) =>
            Read(text => _tcpClient.Available > 0 || !text.Contains(textCheck));

        private string ReadLine() => Read(text => !text.Contains(_lineEnd));

        public string ReadAvaiable() => Read(_ => _tcpClient.Available > 0);

        public void Dispose()
        {
            _tcpClient.Dispose();
        }

        ~TelnetClient()
        {
            Dispose();
        }

    }

}

