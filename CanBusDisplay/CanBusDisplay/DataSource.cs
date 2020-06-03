using System;
using System.Collections.Generic;
using System.Globalization;
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

        private Thread workerThread;

        public Dictionary<int, byte[]> Data = new Dictionary<int, byte[]>
        {
            { 0x201, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { 0x420, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { 0x4B0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
        };

        #region 0x420
        public MILState MIL
        {
            get
            {
                return (MILState)(Data[0x420][4] >> 4);
            }
        }
        #endregion

        #region 0x4B0
        public int ABSSpeedFL
        {
            get
            {
                return toSpeed(Data[0x4B0][0] << 8 | Data[0x4B0][1]);
            }
        }
        public int ABSSpeedFR
        {
            get
            {
                return toSpeed(Data[0x4B0][2] << 8 | Data[0x4B0][3]);
            }
        }
        public int ABSSpeedRL
        {
            get
            {
                return toSpeed(Data[0x4B0][4] << 8 | Data[0x4B0][5]);
            }
        }
        public int ABSSpeedRR
        {
            get
            {
                return toSpeed(Data[0x4B0][6] << 8 | Data[0x4B0][7]);
            }
        }
        #endregion

        private int toSpeed(int rawSpeed)
        {
            return (int)((rawSpeed * 0.0066) - 66);
        }

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

                    if (line.StartsWith("201") || line.StartsWith("420") || line.StartsWith("4B0"))
                    {
                        parseFrame(line);
                    }
                }
            }
        }

        private void parseFrame(string line)
        {
            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            int id = int.Parse(parts[0], NumberStyles.HexNumber);
            byte length = byte.Parse(parts[1], NumberStyles.HexNumber);

            for (int i = 0; i < length; i++)
            {
                Data[id][i] = byte.Parse(parts[i + 2], NumberStyles.HexNumber);
            }
        }
    }
}
