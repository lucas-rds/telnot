using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace telnot
{
    class Telnot
    {
        private readonly IPEndPoint _backendEndpoint;
        private readonly IPEndPoint _frontendEndpoint;
        private ITelnetServer _frontend;
        private ITelnetClient _backend;

        private const string BACKSPACE = "\u007f";

        public Telnot(IPEndPoint backendEndpoint, IPEndPoint frontendEndpoint)
        {
            _backendEndpoint = backendEndpoint;
            _frontendEndpoint = frontendEndpoint;
        }

        public async Task StartTunneling()
        {
            using (_frontend = new TelnetServer(_frontendEndpoint.Address, _frontendEndpoint.Port))
            using (_backend = new TelnetClient(_backendEndpoint.Address, _backendEndpoint.Port))
            {
                await _frontend.ListenConnections();

                string backendLogin = await _backend.LoginAsync("lucas", "");
                var data = ANSI.RemoveCursorPosition(_backend.ReadAvaiable());
                Console.Write(backendLogin);
                Console.Write(data);
                _frontend.Write(backendLogin);
                _frontend.Write(data);

                await Task.WhenAll(LoopProcessFrontEnd(), LoopProcessBackEnd());
            }
        }

        private Task LoopProcessBackEnd()
        {
            return _backend.StartReading(text =>
            {
                Console.WriteLine($"backend] {text}");
                _frontend.Write(text);
            });
        }

        private Task LoopProcessFrontEnd()
        {
            var stringBuilder = new StringBuilder();
            return _frontend.StartReading(text =>
            {
                if (text.Contains(BACKSPACE) && stringBuilder.Length > 0)
                    stringBuilder.Remove(stringBuilder.Length - 1, 1);
                else
                    stringBuilder.Append(text);

                Console.WriteLine($"frontend] {text}");
                //Console.WriteLine($"frontend input] {input}");

                if (text.Contains("\n"))
                {
                    _backend.Write(stringBuilder.ToString());
                    stringBuilder.Clear();
                }
            });
        }
    }
}
