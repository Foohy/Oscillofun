using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Oscillofun
{
    class Shader
    {
        struct Uniform
        {
            public string Name;
            public int Location;
            //public ActiveUniformType DataType;
            public object Value;

            public static Uniform Default = new Uniform()
            {
                Name = "INVALID UNIFORM",
                Location = -1,
            };
        }

        public int ProgramID = -1;
        public string Name;

        private bool LocationsCached;

        //Dictionary<string, int> UniformLocations = new Dictionary<string, int>();
        Dictionary<string, Uniform> Uniforms = new Dictionary<string, Uniform>();

        //Hold a list of shader objects we compiled so we can remove them later
        private List<int> compiledShaderObjects = new List<int>();

        public Shader(string name, string vertexShaderSource, string fragmentShaderSource, string geoShaderSource = "")
        {
            Name = name;

            //Don't do any shader work if shaders aren't enabled
            if (!Utilities.EngineSettings.Shaders) return;

            //Compile the shaders
            CompileShader(vertexShaderSource, ShaderType.VertexShader);
            CompileShader(fragmentShaderSource, ShaderType.FragmentShader);

            //Optionally a geometry shader
            if (!string.IsNullOrEmpty(geoShaderSource))
            {
                CompileShader(geoShaderSource, ShaderType.GeometryShaderExt);
                Console.WriteLine(GL.GetError());
                //TODO: Abstract this so it's not slapped in here like this EW
                GL.ProgramParameter(ProgramID, AssemblyProgramParameterArb.GeometryInputType, (int)All.LineStrip);
                GL.ProgramParameter(ProgramID, AssemblyProgramParameterArb.GeometryOutputType, (int)All.LineStrip);
                GL.ProgramParameter(ProgramID, AssemblyProgramParameterArb.GeometryVerticesOut, 4);
            }
            //Link
            GL.LinkProgram(ProgramID);

            //The shader objects are no longer necessary, they're executing in the program object now
            ClearShaderObjects();

            //Cache some useful locations for uniforms
            CacheUniformLocations();
        }

        /// <summary>
        /// Detach and remove any existing shader objects that were just compiled into a program
        /// </summary>
        private void ClearShaderObjects()
        {
            for (int i = compiledShaderObjects.Count-1; i >= 0; i--)
            {
                //Delete the shader from the gpu
                GL.DetachShader(ProgramID, compiledShaderObjects[i]);
                GL.DeleteShader(compiledShaderObjects[i]);

                //Delete it here, it's dead, Jim.
                compiledShaderObjects.Remove(i);
            }
        }

        private int CompileShader(string source, ShaderType type)
        {
            //Create a new shader program if we don't have one yet
            if (ProgramID == -1)
            {
                ProgramID = GL.CreateProgram();

                //Bind some attribute locations that match our vertex data
                GL.BindAttribLocation(ProgramID, 0, "_Position");
                GL.BindAttribLocation(ProgramID, 1, "_Color");
                GL.BindAttribLocation(ProgramID, 2, "_UV");
            }

            //Create our shader instance
            int shader = GL.CreateShader(type);
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            //Get compilation status about the shader
            int compileStatus;
            string compileLog;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out compileStatus);
            GL.GetShaderInfoLog(shader, out compileLog);

            //Error/warning output
            if (compileStatus == 0)
            {
                Trace.WriteLine(string.Format("{0} SHADER \"{1}\" FAILED TO COMPILE!{2}{3}", type, Name, Environment.NewLine, compileLog));
                return -1;
            }
            else if (compileLog.Length > 0)
                Trace.WriteLine(string.Format("{0} SHADER \"{1}\" COMPILED but with some warnings{2}{3}", type, Name, Environment.NewLine, compileLog));

            //Attach the shader to our active program
            GL.AttachShader(ProgramID, shader);

            //Add it to our list of shader objects
            compiledShaderObjects.Add(shader);

            return shader;
        }

        public void CacheUniformLocations()
        {
            //Test if our hardware is gonna let us cache locations beforehand
            if (!allowsUniformNameRetrieval())
            {
                Trace.WriteLine("Warning, unable to cache uniform locations.\"glGetActiveUniformName\" or \"glGetActiveUniform\" are not available.");
                return;
            }

            //Retrieve the number of uniforms
            int uniCount = 0;
            GL.GetProgram(ProgramID, ProgramParameter.ActiveUniforms, out uniCount);

            //Loop through each of the count and store some good ole fashion info about it
            for (int i = 0; i < uniCount; i++)
            {
                //Get the length of the uniform name
                string uniformName = GL.GetActiveUniformName(ProgramID, i);

                //Get other info
                int size, length;
                ActiveUniformType type;
                StringBuilder name = new StringBuilder();
                GL.GetActiveUniform(ProgramID, i, uniformName.Length+1, out size, out length, out type, name);

                //Cache this single uniform
                cacheSingleUniform(uniformName, i);
                /*
                //Create our uniform struct and store it
                Uniform uniform = new Uniform()
                {
                    //DataType = type,
                    Location = i,
                    Name = uniformName,
                    Value = null,
                };

                //Store it's values
                Uniforms.Add(uniformName, uniform);  
                 * */
            }

            LocationsCached = true;
        }

        private bool cacheSingleUniform(string name, int loc = -1)
        {
            //If they passed us an invalid location, try to retrieve it ourselves
            loc = loc == -1 ? GL.GetUniformLocation(ProgramID, name) : loc;
            if (loc == -1) return false; //Uniform does not exist, can't cache it

            //Create our uniform struct and store it
            Uniform uniform = new Uniform()
            {
                //DataType = type,
                Location = loc,
                Name = name,
                Value = null,
            };

            //Store it's values
            Uniforms.Add(name, uniform);

            return true;
        }

        private bool allowsUniformNameRetrieval()
        {
            IntPtr uniNameAddr = (Utilities.EngineInstance.Context as IGraphicsContextInternal).GetAddress("glGetActiveUniformName");
            IntPtr uniAddr = (Utilities.EngineInstance.Context as IGraphicsContextInternal).GetAddress("glGetActiveUniform");

            return uniNameAddr != IntPtr.Zero && uniAddr != IntPtr.Zero;
        }

        public virtual void Bind()
        {
            //Don't bind if we don't wanna
            if (ProgramID == -1 || !Utilities.EngineSettings.Shaders) return;

            GL.UseProgram(ProgramID);
        }

        private Uniform GetUniform(string name, bool autoBind)
        {
            //If there's no valid program, there's no valid uniform!!
            if (ProgramID == -1)
                return Uniform.Default;

            if (!Uniforms.ContainsKey(name))
            {
                //If we weren't able to cache all the uniforms beforehand, we'll have to do it as they're accessed
                if (LocationsCached || !cacheSingleUniform(name))
                    return Uniform.Default;
            }

            Uniform uniform = Uniforms[name];

            if (autoBind)
                GL.UseProgram(ProgramID);

            return uniform;
        }

        #region Set Uniforms

        #region Uniform1
        public bool SetDouble(string name, double val, bool bindProgram = true)
        {
            Uniform uniform = GetUniform(name, bindProgram);
            if (uniform.Location == -1) return false;

            GL.Uniform1(uniform.Location, val);

            return true;
        }
        public bool SetFloat(string name, float val, bool bindProgram = true)
        {
            Uniform uniform = GetUniform(name, bindProgram);
            if (uniform.Location == -1) return false;

            GL.Uniform1(uniform.Location, val);

            return true;
        }

        public bool SetInteger(string name, int val, bool bindProgram = true)
        {
            Uniform uniform = GetUniform(name, bindProgram);
            if (uniform.Location == -1) return false;

            GL.Uniform1(uniform.Location, val);

            return true;
        }
        #endregion
        #region Uniform2
        public bool SetVector2(string name, Vector2 val, bool bindProgram = true)
        {
            Uniform uniform = GetUniform(name, bindProgram);
            if (uniform.Location == -1) return false;

            GL.Uniform2(uniform.Location, val);

            return true;
        }
        public bool SetFloat2d(string name, double val1, double val2, bool bindProgram = true)
        {
            Uniform uniform = GetUniform(name, bindProgram);
            if (uniform.Location == -1) return false;

            GL.Uniform2(uniform.Location, val1, val2);

            return true;
        }

        public bool SetFloat2f(string name, float val1, float val2, bool bindProgram = true)
        {
            Uniform uniform = GetUniform(name, bindProgram);
            if (uniform.Location == -1) return false;

            GL.Uniform2(uniform.Location, val1, val2);

            return true;
        }
        #endregion
        #region Uniform3
        public bool SetVector3(string name, Vector3 val, bool bindProgram = true)
        {
            Uniform uniform = GetUniform(name, bindProgram);
            if (uniform.Location == -1) return false;

            GL.Uniform3(uniform.Location, val);

            return true;
        }
        public bool SetFloat3d(string name, double val1, double val2, double val3, bool bindProgram = true)
        {
            Uniform uniform = GetUniform(name, bindProgram);
            if (uniform.Location == -1) return false;

            GL.Uniform3(uniform.Location, val1, val2, val3);

            return true;
        }

        public bool SetFloat3f(string name, float val1, float val2, float val3, bool bindProgram = true)
        {
            Uniform uniform = GetUniform(name, bindProgram);
            if (uniform.Location == -1) return false;

            GL.Uniform3(uniform.Location, val1, val2, val3);

            return true;
        }
        #endregion
        #region UniformMatrix4
        public bool SetMatrix4(string name, Matrix4 val, bool bindProgram = true)
        {
            Uniform uniform = GetUniform(name, bindProgram);
            if (uniform.Location == -1) return false;

            GL.UniformMatrix4(uniform.Location, false, ref val);

            return true;
        }
        #endregion

        #endregion
    }
}
