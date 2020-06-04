using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

namespace CanBusDisplay
{
    class FakeDataSource : IDataSource
    {
        private TimeSpan offset;
        private int location;

        private List<Frame> frames;

        private bool running = true;

        private Thread workerThread;

        public FakeDataSource(string path)
        {
            frames = new List<Frame>();
            using (var reader = new StreamReader(File.OpenRead(path)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] parts = line.Split(new string[] { " -> " }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts[1] == "started" || parts[1] == "no messages")
                    {
                        continue;
                    }

                    Frame f = new Frame();
                    f.TimeStamp = TimeSpan.Parse(parts[0]);

                    parts = parts[1].Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    f.ID = int.Parse(parts[0], NumberStyles.HexNumber);
                    f.Length = byte.Parse(parts[1], NumberStyles.HexNumber);
                    f.Data = new byte[f.Length];
                    for (int i = 0; i < f.Length; i++)
                    {
                        f.Data[i] = byte.Parse(parts[i + 2], NumberStyles.HexNumber);
                    }

                    frames.Add(f);
                }
            }
            workerThread = new Thread(worker);
        }

        public Dictionary<int, byte[]> Data { get; } = new Dictionary<int, byte[]>
        {
            { 0x201, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { 0x200, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { 0x210, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { 0x230, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { 0x420, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { 0x428, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { 0x430, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { 0x4B0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
        };

        public void Start()
        {
            offset = DateTime.Now.TimeOfDay - frames[0].TimeStamp;
            location = 0;
            workerThread.Start();
        }

        public void Stop()
        {
            workerThread.Abort();
        }

        private void worker()
        {
            while (running)
            {
                if (frames[location].TimeStamp <= DateTime.Now.TimeOfDay - offset)
                {
                    Data[frames[location].ID] = frames[location].Data;
                    location++;
                }
                if (location >= frames.Count)
                {
                    offset = DateTime.Now.TimeOfDay - frames[0].TimeStamp;
                    location = 0;
                }
            }
        }
    }
}
