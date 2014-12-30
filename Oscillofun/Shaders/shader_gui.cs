using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oscillofun.Shaders
{
    class shader_gui
    {
        public static string VertSource =
@"
#version 150

in vec2 _Position;
uniform matrix4 _pmatrix; //Projection matrix
uniform float _time;

void main()
{
    gl_position = _pmatrix * _Position;
}
";

        public static string FragSource =
@"
#version 150

uniform float _time;

out vec4 ex_FragColor;
void main()
{
    ex_FragColor = vec4( 1.0, 1.0, 0.0, 1.0 );
}
";
    }
}
