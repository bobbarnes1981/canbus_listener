using SdlDotNet.Core;
using SdlDotNet.Graphics;
using System;
using System.Drawing;

namespace CanBusDisplay
{
    class Controller
    {
        private const int width = 640;
        private const int height = 480;
        private const int colourDepth = 32;

        private Surface video;

        private float elapsed;

        private DataSource source;

        public Controller(DataSource source)
        {
            this.source = source;
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

            video.Blit(f.Render("testing", Color.White), new Point(width / 2, height / 2));

            video.Blit(f.Render($"RPM {source.RPM}", Color.Red), new Point(width / 2, (height / 2)+20));

            video.Update();
        }
    }
}
