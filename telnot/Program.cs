using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace telnot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //Coisas do windows: https://www.jerriepelser.com/blog/using-ansi-color-codes-in-net-console-apps/
            ConsoleConfiguration.EnableAnsiVT100();

            var backendAddress = new IPEndPoint(IPAddress.Loopback, 23);
            var frontendAddress = new IPEndPoint(IPAddress.Loopback, 9090);

            await new Telnot(backendAddress, frontendAddress).StartTunneling();
        }
    }


}
