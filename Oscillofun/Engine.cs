using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Oscillofun
{
    public struct GLVersion
    {
        static public int Major;
        static public int Minor;

        public static string SimpleString()
        {
            return string.Format("{0}.{1}", GLVersion.Major, GLVersion.Minor);
        }
    }

    class Engine : GameWindow
    {
        public Engine()
            : base(800, 800, new GraphicsMode(32, 24, 0, 4), typeof(Engine).Assembly.GetName().Name, GameWindowFlags.Default)
        {
            var settings = new Settings();
            settings.Width = this.Width;
            settings.Height = this.Height;
            settings.Samples = this.Context.GraphicsMode.Samples;
            settings.Fullscreen = this.WindowState == OpenTK.WindowState.Fullscreen;

            initializeEngine(settings);
        }

        public Engine(Settings engineSettings)
            : base(engineSettings.Width, engineSettings.Height, new GraphicsMode(32, 24, 0, engineSettings.Samples), typeof(Engine).Assembly.GetName().Name, !engineSettings.Fullscreen || engineSettings.NoBorder ? GameWindowFlags.Default : GameWindowFlags.Fullscreen)
        {
            initializeEngine(engineSettings);
        }

        public Engine(Settings engineSettings, string title)
            : base(engineSettings.Width, engineSettings.Height, new GraphicsMode(32, 24, 0, engineSettings.Samples), title, !engineSettings.Fullscreen || engineSettings.NoBorder ? GameWindowFlags.Default : GameWindowFlags.Fullscreen)
        {
            initializeEngine(engineSettings);
        }

        private void initializeEngine(Settings engineSettings)
        {
            Trace.WriteLine("==================================");
            Trace.WriteLine(string.Format("OLEG ENGINE - LITE - 2D \n{0}", typeof(Engine).Assembly.GetName().Version.ToString()));
            Trace.WriteLine("==================================\n");

            //Store current engine settings
            Utilities.EngineSettings = engineSettings;
            Utilities.EngineInstance = this;

            //Toggle VSync
            this.VSync = engineSettings.VSync;

            //Noborder if applicable
            if (engineSettings.NoBorder)
            {
                this.WindowBorder = OpenTK.WindowBorder.Hidden;

                if (engineSettings.Fullscreen)
                    this.WindowState = OpenTK.WindowState.Fullscreen;
            }

            //Hook into some engine callbacks
            //this.OnRenderSceneOpaque += new Action<FrameEventArgs>(RenderSceneOpaque);
            //this.OnRenderSceneTranslucent += new Action<FrameEventArgs>(RenderSceneTranslucent);

            //Make a furious attempt to change the window's icon
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.AppDomain.CurrentDomain.FriendlyName); }
            catch (Exception e) { Trace.WriteLine("Failed to load icon! " + e.Message); }

            //Hide the console if we want
            if (!engineSettings.ShowConsole)
                ConsoleManager.ShowWindow(ConsoleManager.GetConsoleWindow(), ConsoleManager.SW_HIDE);

        }

        /// <summary>
        /// Check to make sure our current settings are supported, and bump them down if they aren't
        /// </summary>
        private void FeatureCheck()
        {
            Trace.WriteLine("->Beginning feature check");

            //Before OpenGL 2.0, shaders were not part of core so don't bother here
            if (GLVersion.Major < 2)
            {
                Trace.WriteLine("GL Version less than 2.0. Good luck");
                Utilities.EngineSettings.Shaders = Utilities.EngineSettings.GeoShaders = false;         
            }


            Trace.WriteLine("SHADERS: " + Utilities.EngineSettings.Shaders);
            Trace.WriteLine("GEOMETRY SHADERS: " + Utilities.EngineSettings.GeoShaders);
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            #region Hardware info
            Trace.WriteLine("->Retrieving hardware information");
            Trace.WriteLine("Vendor: " + GL.GetString(StringName.Vendor));
            Trace.WriteLine("Renderer: " + GL.GetString(StringName.Renderer));
            Trace.WriteLine("GLSL Version: " + GL.GetString(StringName.ShadingLanguageVersion));
            string versionOpenGL = GL.GetString(StringName.Version);
            GLVersion.Major = (int)Char.GetNumericValue(versionOpenGL[0]);
            GLVersion.Minor = (int)Char.GetNumericValue(versionOpenGL[2]);
            Trace.WriteLine("OpenGL version: " + versionOpenGL);
            Trace.WriteLine("");

            //Check our settings to make sure they're legit
            FeatureCheck();
            #endregion

            #region OpenGL Default Values
            GL.ClearColor(0, 0, 0, 0);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            GL.AlphaFunc(AlphaFunction.Greater, 0.1f);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthClamp);

            GL.LineWidth(1.5f);
            #endregion

        }
        
        /// <summary>
        /// Called when the window is resized
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            RegenerateProjectionMatrix();

            //Resize the view area to match the new bounds
            GL.Viewport(0, 0, ClientSize.Width, ClientSize.Height);

            //Change the general projection matrix so that units==pixels
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.MultMatrix(ref Utilities.ProjectionMatrix);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
        }

        /// <summary>
        /// Called when it is time to render the next frame. Add your rendering code here.
        /// </summary>
        /// <param name="e">Contains timing information.</param>
        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            //Clear the background
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        private void RegenerateProjectionMatrix()
        {
            Utilities.ProjectionMatrix = Matrix4.CreateOrthographicOffCenter(0, ClientSize.Width, ClientSize.Height, 0, -1, 1);
        }
    }
}
