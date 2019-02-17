using System;
using System.Threading.Tasks;

namespace telnot
{
    interface ITelnetServer : ITelnet
    {
        Task ListenConnections();

    }
}
