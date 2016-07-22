using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oscillofun
{
    class Material
    {
        string Name;
        Shader DrawShader;
        System.Drawing.Color Color;

        public Material(string name, Shader shader)
        {
            Name = name;
            DrawShader = shader;
        }

        public void Bind()
        {
            //Bind the shader we're going to use
            DrawShader.Bind();

            //Update the uniform for the world matrix
            DrawShader.SetFloat("_time", Utilities.CurTime, false);
            DrawShader.SetMatrix4("_pmatrix", Utilities.ProjectionMatrix, false);
        }

    }
}
