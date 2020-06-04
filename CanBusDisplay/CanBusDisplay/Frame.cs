using System;

namespace CanBusDisplay
{
    class Frame
    {
        public TimeSpan TimeStamp;

        public int ID;
        public byte Length;
        public byte[] Data;
    }
}
