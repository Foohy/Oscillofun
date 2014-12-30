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
        Material GUIMaterial;
        Material LissajousMaterial;

        public static float BufferHistoryLength = 0.05f;//0.08533f //The length of the data, in seconds, to allocate for our sample size
        public static float rayDistanceFadeMultiplier = 900000f; //The length of time it takes for a photon to no longer light against a cathode ray tube, in some magic unit

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


            Audio.Init();

            //Grab the output frequency of our device
            int bufferSize = (int)(WasapiDevice.CurrentDeviceInfo.mixfreq * BufferHistoryLength);
            Audio.SetSampleSize(bufferSize*2);

            Lissajous = new LineMesh(bufferSize); //8192*4//TODO: Change the length based on output frequency
            Lissajous.UpdateBuffer();

            //Create some materials
            LissajousMaterial = new Material("LissajousMat", new Shader("PrettyLine", Oscillofun.Shaders.shader_prettylines.VertSource, Oscillofun.Shaders.shader_prettylines.FragSource));

            //Levels.LevelManager.InitalizeLevel(new Levels.FrustumCullTest());
        }

        /// <summary>
        /// Called when it is time to setup the next frame. Add you game logic here.
        /// </summary>
        /// <param name="e">Contains timing information for framerate independent logic.</param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            float lastLeftAmt = 0;
            float lastRightAmt = 0;

            //Update the positions of the points
            for (int i = 0; i < Audio.SampleData.Length; i++)
            {
                //Lissajous.Points[i].X = -1 + 2f * i / Lissajous.Points.Length;
                //Lissajous.Points[i].Y = (float)Math.Sin(Utilities.CurTime + Math.PI * 2 * i / (float)Lissajous.Points.Length);

                //Maintain aspect ratio
                float Scale = Math.Min(ClientSize.Width, ClientSize.Height);


                //Channels are dithered left and right throughout the array
                //EG. index 0 is left, 1 is right, 2 is left, etc.
                //Only execute ever _other_ loop
                if (i % 2 == 1) //  LEFT
                {
                    float leftAmt = Audio.SampleData[i - 1];
                    float rightAmt = -Audio.SampleData[i];

                    if (i > 7 && i < 30000)
                    {
                        //leftAmt = (ClientSize.Width / 2f - Mouse.X) / Scale;
                        //rightAmt = (ClientSize.Height / 2f - Mouse.Y) / Scale;
                        //leftAmt = i*0.0001f - 1.5f;
                        //rightAmt = (float)Math.Tan(i * -0.01f + Utilities.CurTime )*0.05f;
                    }

                    Lissajous.Vertices[i / 2].Position.X = leftAmt * Scale * 0.5f + ClientSize.Width * 0.5f;
                    Lissajous.Vertices[i / 2].Position.Y = rightAmt * Scale * 0.5f + ClientSize.Height * 0.5f;
            
                    if (i >= 2)
                    {
                        //float dist = Lissajous.Vertices[i / 2].Position.DistanceFast(ref Lissajous.Vertices[i / 2 - 1].Position);
                        float dist = DistFast(leftAmt, rightAmt, lastLeftAmt, lastRightAmt);

                        //Larger distance moved since the last point would make it more faded
                        //Ray gun moves faster so less photons are emitting against the screen
                        //float falloff = Math.Max(0.4f-dist * 1.176f, 0);
                        float falloff = Math.Max(0.8f - dist * rayDistanceFadeMultiplier / (float)WasapiDevice.CurrentDeviceInfo.mixfreq, 0);

                        //Additionally, the farther in history the photon is, the more faded it gets as its energy fades from the screen
                        falloff *= ((i / 2) / (float)Lissajous.Vertices.Length);

                        Lissajous.Vertices[i / 2].SetColor(0.10f, 1, 0.10f);
                        Lissajous.Vertices[i / 2].FadeColor( falloff );
                        //Lissajous.Vertices[i / 2].Color *= 0; //Set the next color to 0 just in case we don't get there
                    }


                    //Lissajous.Vertices[i / 2].SetColor(0.10f, 1, 0.10f);
                    //Lissajous.Vertices[i / 2].Color.X = 0.10f;// *i / (float)Audio.SampleData.Length;
                    //Lissajous.Vertices[i / 2].Color.Y = 1;// i / (float)Audio.SampleData.Length;
                    //Lissajous.Vertices[i / 2].Color.Z = 0.10f;// *i / (float)Audio.SampleData.Length;
                    
                    //Store these so we can compare them with our next vertex
                    lastLeftAmt = leftAmt;
                    lastRightAmt = rightAmt;
                }
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
            LissajousMaterial.Bind();
            //OpenTK.Graphics.OpenGL.GL.UseProgram(0);
            Lissajous.Draw();

            //Swap buffers and show mommy what we made
            SwapBuffers();
        }

        private float DistFast(float x1, float y1, float x2, float y2)
        {
            return (float)Math.Sqrt(Math.Pow(x1 - x2,2) + Math.Pow(y1 - y2, 2));
        }
    }
}
