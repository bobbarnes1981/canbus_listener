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

        public Dictionary<int, byte[]> data = new Dictionary<int, byte[]>
        {
            { 0x201, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { 0x420, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
            { 0x4B0, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } },
        };

        #region 0x201
        public int RPM
        {
            get
            {
                return data[0x201][0] << 8 | data[0x201][1];
            }
        }

        public int TorqueDelta
        {
            get
            {
                return data[0x201][2] << 8 | data[0x201][3];
            }
        }

        public int Speed
        {
            get
            {
                return data[0x201][4] << 8 | data[0x201][5];
            }
        }

        public byte AcceleratorPedal
        {
            get
            {
                return data[0x201][6];
            }
        }
        #endregion

        #region 0x420
        public byte Coolant
        {
            get
            {
                return data[0x420][0];
            }
        }
        public MILState MIL
        {
            get
            {
                return (MILState)(data[0x420][4] >> 4);
            }
        }

        #endregion

        #region 0x4B0
        public int ABSSpeedFL
        {
            get
            {
                return data[0x4B0][0] << 8 | data[0x4B0][1];
            }
        }
        public int ABSSpeedFR
        {
            get
            {
                return data[0x4B0][2] << 8 | data[0x4B0][3];
            }
        }
        public int ABSSpeedRL
        {
            get
            {
                return data[0x4B0][4] << 8 | data[0x4B0][5];
            }
        }
        public int ABSSpeedRR
        {
            get
            {
                return data[0x4B0][6] << 8 | data[0x4B0][7];
            }
        }
        #endregion

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
                data[id][i] = byte.Parse(parts[i + 2], NumberStyles.HexNumber);
            }
        }
    }
}
