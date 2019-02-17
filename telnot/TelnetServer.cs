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
    class TelnetServer : ITelnetServer
    {
        private readonly string _lineEnd;
        private readonly IPAddress _host;
        private readonly int _port;
        private Socket _socket;
        private NetworkStream _stream;

        private const string XFF = "\0xFF";

        public TelnetServer(IPAddress host, int port = 23, string lineEnd = "\r\n")
        {
            _host = host;
            _port = port;
            _lineEnd = lineEnd;
        }

        public async Task ListenConnections()
        {
            var entry = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            entry.Bind(new IPEndPoint(_host, _port));
            entry.Listen(1);

            _socket = await entry.AcceptAsync();
            _stream = new NetworkStream(_socket);
        }

        public Task StartReading(Action<string> callback = null)
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    var data = ReadAvaiable();
                    if (data.Length > 0)
                    {
                        callback?.Invoke(ANSI.RemoveCursorPosition(data));
                    }
                }
            });
        }

        public void Write(string data)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(data.Replace(XFF, XFF + XFF));
            _stream.Write(buffer, 0, buffer.Length);
        }

        public string Read(Func<string, bool> condition)
        {
            string text = "";
            do { text += TelnetNegociator.Negociate(_stream); }
            while (condition(text));

            return text;
        }

        public string ReadAvaiable() =>
            Read(_ => _stream != null && _stream.CanRead && _stream.DataAvailable);

        public void Dispose()
        {
            _stream.Dispose();
            _socket.Close();
        }

    }

}

