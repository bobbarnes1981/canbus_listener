using SdlDotNet.Core;
using SdlDotNet.Graphics;
using SdlDotNet.Graphics.Primitives;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

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
        private Dictionary<string, int[]> history = new Dictionary<string, int[]>();
        private int[] speedHistory = new int[1000];

        private Config config;

        public Controller(Config config, DataSource source)
        {
            this.config = config;
            this.source = source;

            for (int i = 0; i < historyLines.Length; i++)
            {
                historyLines[i] = (i % lineInterval) == 0;
            }

            foreach (Display d in config.Displays)
            {
                if ((d.Type == "byte" || d.Type == "word") && d.Graph)
                {
                    history.Add(d.Label, new int[1000]);
                }
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

            drawLines();

            elapsed += e.SecondsElapsed;

            Video.WindowCaption = $"CanBusDisplay {elapsed}";

            SdlDotNet.Graphics.Font f = new SdlDotNet.Graphics.Font("C:\\Windows\\Fonts\\ARIAL.TTF", 20);

            // TODO: only update values on specified interval?

            foreach (Display d in config.Displays)
            {
                switch(d.Type)
                {
                    case "text":
                        video.Blit(f.Render(d.Label, Color.FromName(d.Colour)), new Point(d.Location[0], d.Location[1]));
                        break;
                    case "byte":
                        byte valByte = source.Data[int.Parse(d.Value[0], NumberStyles.HexNumber)][int.Parse(d.Value[1], NumberStyles.HexNumber)];
                        if (d.Graph)
                        {
                            queue(valByte, history[d.Label]);
                            drawGraph(history[d.Label], Color.FromName(d.Colour), d.Min, d.Max);
                        }
                        video.Blit(f.Render($"{d.Label} {valByte}", Color.FromName(d.Colour)), new Point(d.Location[0], d.Location[1]));
                        break;
                    case "word":
                        byte hi = source.Data[int.Parse(d.Value[0], NumberStyles.HexNumber)][int.Parse(d.Value[1], NumberStyles.HexNumber)];
                        byte lo = source.Data[int.Parse(d.Value[2], NumberStyles.HexNumber)][int.Parse(d.Value[3], NumberStyles.HexNumber)];
                        int valWord = hi << 8 | lo;
                        if (d.Scale != 0)
                        {
                            valWord = (int)(valWord * d.Scale);
                        }
                        if (d.Offset != 0)
                        {
                            valWord += d.Offset;
                        }
                        if (d.Graph)
                        {
                            queue(valWord, history[d.Label]);
                            drawGraph(history[d.Label], Color.FromName(d.Colour), d.Min, d.Max);
                        }
                        video.Blit(f.Render($"{d.Label} {valWord}", Color.FromName(d.Colour)), new Point(d.Location[0], d.Location[1]));
                        break;
                }
            }

            video.Blit(f.Render($"ABS FL {source.ABSSpeedFL}", Color.Red), new Point(0, 140));
            video.Blit(f.Render($"ABS FR {source.ABSSpeedFR}", Color.Red), new Point(0, 160));
            video.Blit(f.Render($"ABS RL {source.ABSSpeedRL}", Color.Red), new Point(0, 180));
            video.Blit(f.Render($"ABS RR {source.ABSSpeedRR}", Color.Red), new Point(0, 200));

            video.Blit(f.Render($"MIL {source.MIL}", Color.Red), new Point(300, 20));

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

        private void drawGraph(int[] store, Color color, int min, int max)
        {
            int length = Math.Min(store.Length, width);
            for (int i = 0; i < length; i++)
            {
                video.Draw(new Point(width - i, height - scale(store[i], min, max, 0, 0xFF) - 1), color);
            }
        }

        private int scale(int value, int inMin, int inMax, int outMin, int outMax)
        {
            return (value - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
        }

        private void queue<T>(T value, T[] store)
        {
            for (int i = store.Length-1; i > 0; i--)
            {
                store[i] = store[i - 1];
            }
            store[0] = value;
        }
    }
}
