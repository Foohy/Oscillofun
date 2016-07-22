using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oscillofun.Shaders
{
    class shader_prettylines
    {
        public static string VertSource =
@"#version 150

in vec2 _Position;
in vec3 _Color;
uniform mat4 _pmatrix; //Projection matrix
uniform float _time;

flat out vec3 ex_Color;
void main()
{
    gl_Position = _pmatrix * vec4(_Position.x, _Position.y, 1.0, 1.0);
    ex_Color = _Color;
}
";

        public static string FragSource =
@"#version 150

flat in vec3 ex_Color;
uniform float _time;

out vec4 ex_FragColor;
void main()
{
    ex_FragColor = vec4(ex_Color, 1.0 );
}
";
    }
}
