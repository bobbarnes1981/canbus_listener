using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace CanBusDisplay
{
    class DataSource
    {
        private string name;
        private int baud;

        private bool running = true;

        public int RPM { get; private set; }

        private Thread workerThread;

        public DataSource(string name, int baud)
        {
            this.name = name;
            this.baud = baud;
            workerThread = new Thread(worker);
        }

        public void Start()
        {
            workerThread.Start();
        }

        public void Stop()
        {
            workerThread.Abort();
        }

        private void worker()
        {
            using (SerialPort port = new SerialPort(name, baud))
            {
                try
                {
                    port.Open();
                }
                catch (IOException e)
                {
                    Console.WriteLine($"failed to open port {name}");
                    return;
                }

                while (running)
                {

                    string line = port.ReadLine();

                    if (line.StartsWith("201"))
                    {
                        Console.WriteLine(line);

                        string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        int hi = int.Parse(parts[2], System.Globalization.NumberStyles.HexNumber);
                        int lo = int.Parse(parts[3], System.Globalization.NumberStyles.HexNumber);

                        RPM = (hi << 8) | lo;
                    }
                }
            }
        }
    }
}
