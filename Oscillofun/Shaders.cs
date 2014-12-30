using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

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
            public ActiveUniformType DataType;
            public object Value;

            public static Uniform Default = new Uniform()
            {
                Name = "INVALID UNIFORM",
                Location = -1,
            };
        }

        public int ProgramID = -1;
        public string Name;

        //Dictionary<string, int> UniformLocations = new Dictionary<string, int>();
        Dictionary<string, Uniform> Uniforms = new Dictionary<string, Uniform>();

        public Shader(string name, string vertexShaderSource, string fragmentShaderSource)
        {
            Name = name;

            //Compile the shaders
            int vShader = CompileShader(vertexShaderSource, ShaderType.VertexShader);
            int fShader = CompileShader(fragmentShaderSource, ShaderType.FragmentShader);

            //Link
            GL.LinkProgram(ProgramID);

            //The shader objects are no longer necessary, they're executing in the program object now
            GL.DetachShader(ProgramID, vShader);
            GL.DetachShader(ProgramID, fShader);
            GL.DeleteShader(vShader);
            GL.DeleteShader(fShader);

            //Cache some useful locations for uniforms
            CacheUniformLocations();
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
                Console.WriteLine("SHADER \"{0}\" FAILED TO COMPILE!{1}{2}", Name, Environment.NewLine, compileLog);
                return -1;
            }
            else if (compileLog.Length > 0)
                Console.WriteLine("SHADER \"{0}\" COMPILED but with some warnings{1}{2}", Name, Environment.NewLine, compileLog);

            //Attach the shader to our active program
            GL.AttachShader(ProgramID, shader);

            return shader;
        }

        public void CacheUniformLocations()
        {
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

                //Create our uniform struct and store it
                Uniform uniform = new Uniform()
                {
                    DataType = type,
                    Location = i,
                    Name = uniformName,
                    Value = null,
                };

                //Store it's values
                Uniforms.Add(uniformName, uniform);
                
            }
        }

        public virtual void Bind()
        {
            GL.UseProgram(ProgramID);
        }

        private Uniform GetUniform(string name, bool autoBind)
        {
            if (!Uniforms.ContainsKey(name))
                return Uniform.Default;

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
