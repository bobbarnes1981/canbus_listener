using System.Collections.Generic;

namespace CanBusDisplay
{
    interface IDataSource
    {
        void Start();
        void Stop();
        Dictionary<int, byte[]> Data { get; }
    }
}
