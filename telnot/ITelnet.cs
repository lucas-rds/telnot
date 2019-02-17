using System;
using System.Threading.Tasks;

namespace telnot
{
    interface ITelnet : IDisposable
    {
        void Write(string data);

        string Read(Func<string, bool> condition);

        string ReadAvaiable();

        Task StartReading(Action<string> callback = null);

    }
}
