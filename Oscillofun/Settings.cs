using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using Newtonsoft.Json;

namespace Oscillofun
{
    class Settings
    {
        //General settings
        public VSyncMode VSync = VSyncMode.On;
        public WindowState WindowMode = WindowState.Normal;
        public bool NoBorder = false;
        public int Width = 1024;
        public int Height = 768;

        //Rendering settings
        public int Samples = 2;

        //Audio settings
        //public float GlobalVolume = 1.0f;

        //Debug settings
        public bool ShowFPS = false;
        public bool ShowConsole = false;

        public Settings()
        {
        }

        public Settings(string filename)
        {
            if (!File.Exists(filename))
            {
                Save(filename); //Create the default settings file
            }
            else
            {
                try
                {
                    Settings s = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(filename));
                    System.Reflection.FieldInfo[] fields = s.GetType().GetFields();

                    foreach (var field in fields)
                    {
                        this.GetType().GetField(field.Name).SetValue(this, field.GetValue(s));
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to load {0}!\n\t {1}", filename, e.Message);
                }
            }
        }


        //Try saving settings to a file
        public void Save(string filename)
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filename, json);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to save {0}!\n\t {1}", filename, e.Message);
            }

        }
    }
}
