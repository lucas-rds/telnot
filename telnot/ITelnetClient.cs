using System;
using System.Threading.Tasks;

namespace telnot
{
    interface ITelnetClient : ITelnet
    {
        string Login(string username, string password);
        Task<string> LoginAsync(string username, string password);
    }
}
