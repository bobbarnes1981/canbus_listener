using SdlDotNet.Core;
using SdlDotNet.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace LogGrapher
{
    enum Size
    {
        None,
        bit8,
        bit16
    }

    class Program
    {
        private static Surface video;

        private static List<Frame> frames;

        private static List<int> ids;

        private static TimeSpan start = TimeSpan.Parse("12:52:03.000");

        private static int width = 1280;
        private static int height = 480;

        private static Color[] colours = new Color[]
        {
            Color.Red,
            Color.Orange,
            Color.Yellow,
            Color.Green,
            Color.Blue,
            Color.Purple,
            Color.Fuchsia,
            Color.White
        };

        private static int FRAME = 0x40;

        private static Dictionary<int, Size[]> definition = new Dictionary<int, Size[]>
        {
            {0x000, new Size[] { Size.bit8, Size.bit8, Size.bit8, Size.bit8, Size.bit8, Size.bit8, Size.bit8, Size.bit8 } },
            {0x201, new Size[] { Size.bit16, Size.None, Size.bit16, Size.None, Size.bit16, Size.None, Size.bit8, Size.bit8 } },
        };

        static void Main(string[] args)
        {
            ids = new List<int>();
            frames = new List<Frame>();
            using (var reader = new StreamReader(File.OpenRead("output.txt")))
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
                    if (ids.Contains(f.ID) == false)
                    {
                        ids.Add(f.ID);
                        Console.WriteLine($"0x{f.ID:X3}");
                    }
                }
            }


            video = Video.SetVideoMode(width, height, 32, false, false, false, true);

            Events.Quit += Events_Quit;
            Events.Tick += Events_Tick;

            Events.Run();
        }

        private static void Events_Tick(object sender, TickEventArgs e)
        {
            video.Fill(Color.Black);

            int x = 0;

            foreach (var frame in frames)
            {
                if(x <= width && frame.ID == FRAME && frame.TimeStamp > start)
                {
                    var def = definition[0x000];
                    if (definition.ContainsKey(frame.ID))
                    {
                        def = definition[frame.ID];
                    }
                    for (int i = 0; i < frame.Data.Length; i++)
                    {
                        switch (def[i])
                        {
                            case Size.None:
                                break;
                            case Size.bit8:
                                var m8 = 255.0;
                                var scale8 = height / m8;
                                int val8 = frame.Data[i];
                                val8 = (int)(val8 * scale8);
                                video.Draw(new Point(x, height - val8), colours[i]);
                                break;
                            case Size.bit16:
                                var m16 = 65535.0;
                                var scale16 = height / m16;
                                int val16 = frame.Data[i] << 8 | frame.Data[i + 1];
                                val16 = (int)(val16 * scale16);
                                video.Draw(new Point(x, height - val16), colours[i]);
                                break;
                            default:
                                break;
                        }
                    }
                    x++;
                }
            }


            video.Update();
        }

        private static void Events_Quit(object sender, QuitEventArgs e)
        {
            Events.QuitApplication();
        }
    }
}
