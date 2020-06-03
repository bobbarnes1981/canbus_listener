using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;
using System;
using System.Drawing;

namespace CanBusDisplay
{
    class Controller
    {
        private const int width = 640;
        private const int height = 480;
        private const int colourDepth = 32;
        private const int lineInterval = 50;

        private Surface video;

        private float elapsed;

        private DataSource source;

        private bool[] historyLines = new bool[1000];
        private int[] rpmHistory = new int[1000];
        private int[] torqueHistory = new int[1000];
        private int[] speedHistory = new int[1000];
        private int[] acelHistory = new int[1000];
        private int[] coolantHistory = new int[1000];

        public Controller(DataSource source)
        {
            this.source = source;

            for (int i = 0; i < historyLines.Length; i++)
            {
                historyLines[i] = (i % lineInterval) == 0;
            }
        }

        public void Start()
        {
            source.Start();

            video = Video.SetVideoMode(width, height, colourDepth, false, false, false, true);

            Events.Quit += new EventHandler<QuitEventArgs>(quit);
            Events.Tick += new EventHandler<TickEventArgs>(tick);

            Events.Run();
        }

        private void quit(object sender, QuitEventArgs e)
        {
            source.Stop();
            Events.QuitApplication();
        }

        private void tick(object sender, TickEventArgs e)
        {
            video.Fill(Color.Black);

            elapsed += e.SecondsElapsed;

            Video.WindowCaption = $"CanBusDisplay {elapsed}";

            SdlDotNet.Graphics.Font f = new SdlDotNet.Graphics.Font("C:\\Windows\\Fonts\\ARIAL.TTF", 20);

            video.Blit(f.Render("Ford Fiesta ST150", Color.White), new Point(0, 0));

            // TODO: only do this every x-interval?
            moveLines();
            queue(source.RPM, rpmHistory);
            queue(source.TorqueDelta, torqueHistory);
            queue(source.Speed, speedHistory);
            queue(source.AcceleratorPedal, acelHistory);
            queue(source.Coolant, coolantHistory);

            // TODO: scale values
            video.Blit(f.Render($"RPM {source.RPM}", Color.Red), new Point(0, 20));
            video.Blit(f.Render($"Torque Delta {source.TorqueDelta}", Color.Red), new Point(0, 40));
            video.Blit(f.Render($"Speed {source.Speed}", Color.Red), new Point(0, 60));
            video.Blit(f.Render($"Accelerator {source.AcceleratorPedal}", Color.Red), new Point(0, 80));
            video.Blit(f.Render($"Coolant {source.Coolant}", Color.Red), new Point(0, 100));

            video.Blit(f.Render($"ABS FL {source.ABSSpeedFL}", Color.Red), new Point(0, 120));
            video.Blit(f.Render($"ABS FR {source.ABSSpeedFR}", Color.Red), new Point(0, 140));
            video.Blit(f.Render($"ABS RL {source.ABSSpeedRL}", Color.Red), new Point(0, 160));
            video.Blit(f.Render($"ABS RR {source.ABSSpeedRR}", Color.Red), new Point(0, 180));

            video.Blit(f.Render($"MIL {source.MIL}", Color.Red), new Point(300, 20));

            drawLines();
            // TODO: scale graph y axis
            drawGraph(rpmHistory, Color.Blue, 10);
            drawGraph(torqueHistory, Color.Yellow, 20);
            drawGraph(speedHistory, Color.Red, 30);
            drawGraph(acelHistory, Color.Green, 40);
            drawGraph(coolantHistory, Color.AliceBlue, 50);

            video.Update();
        }

        private void drawLines()
        {
            for (int i = 0; i < width; i++)
            {
                if (historyLines[i])
                {
                    video.Draw(new Line(new Point(width - i, height), new Point(width - i, height-0xFF)), Color.Gray);
                }
            }
        }

        private void drawGraph(int[] store, Color color, int offset)
        {
            int length = Math.Min(store.Length, width);
            for (int i = 0; i < length; i++)
            {
                video.Draw(new Point(width - i, height - store[i] - offset), color);
            }
        }

        private void moveLines()
        {
            //bool prev = historyLines[historyLines.Length - 1];
            //for (int i = historyLines.Length-1; i > 0; i--)
            //{
            //    historyLines[i] = historyLines[i - 1];
            //}
            //historyLines[0] = prev;
        }

        private void queue(int value, int[] store)
        {
            for (int i = store.Length-1; i > 0; i--)
            {
                store[i] = store[i - 1];
            }
            store[0] = value;
        }
    }
}
