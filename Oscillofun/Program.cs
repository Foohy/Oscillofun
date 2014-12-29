using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace Oscillofun
{
    class Program : Engine
    {
        public const string SettingsFile = "settings.cfg";
        LineMesh Lissajous;
        WasapiDevice wasapiDevice;

        [STAThread]
        static void Main(string[] args)
        {
            using (Program game = new Program(new Settings(SettingsFile)))
            {
                game.Run(60.0);
            }
        }

        public Program()
            : base()
        {
        }

        public Program(Settings settings)
            : base(settings)
        {
        }

        public Program(Settings settings, string title)
            : base(settings, title)
        {
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            Lissajous = new LineMesh(8192);
            Lissajous.UpdateBuffer();

            Audio.Init();

            Audio.SetSampleSize(Lissajous.Points.Length);

            //Levels.LevelManager.InitalizeLevel(new Levels.FrustumCullTest());
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            //Update the positions of the points
            for (int i = 0; i < Audio.SampleData.Length; i++)
            {
                //Lissajous.Points[i].X = -1 + 2f * i / Lissajous.Points.Length;
                //Lissajous.Points[i].Y = (float)Math.Sin(Utilities.CurTime + Math.PI * 2 * i / (float)Lissajous.Points.Length);

                //Channels are dithered left and right throughout the array
                //EG. index 0 is left, 1 is right, 2 is left, etc.

                if (i%2==0) //  LEFT
                    Lissajous.Points[i/2].X = Audio.SampleData[i];
                else //RIGHT 
                    Lissajous.Points[i/2].Y = Audio.SampleData[i];
            }
            Lissajous.UpdateBuffer();

            this.Title = string.Format("{0:0.00} ms ({1:0.00} fps)", Utilities.DeltaTime*1000, 1 / Utilities.DeltaTime );
            //Levels.LevelManager.Think(e);
        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            //Update timing information
            Utilities.DeltaTime = (float)e.Time;
            Utilities.CurTime += Utilities.DeltaTime;

            //Draw this thing
            Lissajous.Draw();

            //Swap buffers and show mommy what we made
            SwapBuffers();
        }
    }
}
