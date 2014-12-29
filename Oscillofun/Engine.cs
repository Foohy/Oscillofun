using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            settings.WindowMode = this.WindowState;

            initializeEngine(settings);
        }

        public Engine(Settings engineSettings)
            : base(engineSettings.Width, engineSettings.Height, new GraphicsMode(32, 24, 0, engineSettings.Samples), typeof(Engine).Assembly.GetName().Name, engineSettings.WindowMode == WindowState.Fullscreen && engineSettings.NoBorder ? GameWindowFlags.Fullscreen : GameWindowFlags.Default)
        {
            initializeEngine(engineSettings);
        }

        public Engine(Settings engineSettings, string title)
            : base(engineSettings.Width, engineSettings.Height, new GraphicsMode(32, 24, 0, engineSettings.Samples), title, engineSettings.WindowMode == WindowState.Fullscreen && engineSettings.NoBorder ? GameWindowFlags.Fullscreen : GameWindowFlags.Default)
        {
            initializeEngine(engineSettings);
        }

        private void initializeEngine(Settings engineSettings)
        {
            Console.WriteLine("==================================");
            Console.WriteLine("OLEG ENGINE - LITE - 2D \n{0}", typeof(Engine).Assembly.GetName().Version.ToString());
            Console.WriteLine("==================================\n");

            //Store current engine settings
            Utilities.EngineSettings = engineSettings;

            //Toggle VSync
            this.VSync = engineSettings.VSync;

            //Noborder if applicable
            if (engineSettings.NoBorder)
                this.WindowBorder = OpenTK.WindowBorder.Hidden;

            //If we don't want noborder and we're set to fullscreen, change our windowmode to whatever
            if (!engineSettings.NoBorder && engineSettings.WindowMode == OpenTK.WindowState.Fullscreen)
                this.WindowState = engineSettings.WindowMode;
            //BUT if we're set to noborder, require the windowstate is 'normal'
            else if (engineSettings.NoBorder)
                this.WindowState = OpenTK.WindowState.Normal;

            //Hook into some engine callbacks
            //this.OnRenderSceneOpaque += new Action<FrameEventArgs>(RenderSceneOpaque);
            //this.OnRenderSceneTranslucent += new Action<FrameEventArgs>(RenderSceneTranslucent);

            //Make a furious attempt to change the window's icon
            try { this.Icon = System.Drawing.Icon.ExtractAssociatedIcon(System.AppDomain.CurrentDomain.FriendlyName); }
            catch (Exception e) { Console.WriteLine("Failed to load icon! {0}", e.Message); }

            //Hide the console if we want
            if (!engineSettings.ShowConsole)
                ConsoleManager.ShowWindow(ConsoleManager.GetConsoleWindow(), ConsoleManager.SW_HIDE);
        }

        /// <summary>Load resources here.</summary>
        /// <param name="e">Not used.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            #region Hardware info
            Console.WriteLine("==================================");

            Console.WriteLine("Vendor: {0}", GL.GetString(StringName.Vendor));
            Console.WriteLine("Renderer: {0}", GL.GetString(StringName.Renderer));
            Console.WriteLine("GLSL Version: {0}", GL.GetString(StringName.ShadingLanguageVersion));
            string versionOpenGL = GL.GetString(StringName.Version);
            GLVersion.Major = (int)Char.GetNumericValue(versionOpenGL[0]);
            GLVersion.Minor = (int)Char.GetNumericValue(versionOpenGL[2]);
            Console.WriteLine("OpenGL version: {0}", versionOpenGL);

            Console.WriteLine("==================================");
            #endregion

            #region OpenGL Default Values
            GL.ClearColor(0, 0, 0, 0);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
            GL.AlphaFunc(AlphaFunction.Greater, 0.1f);
            GL.Enable(EnableCap.Texture2D);
            GL.Enable(EnableCap.DepthClamp);

            GL.LineWidth(ClientSize.Width / 500f);
            #endregion

        }
        
        /// <summary>
        /// Called when the window is resized
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            //Resize the view area to match the new bounds
            GL.Viewport(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            GL.LineWidth(ClientSize.Width / 500f);
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
    }
}
