using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Oscillofun.Shaders
{
    class shader_prettylines_geo
    {
        public static string GeoSource1 =
@"#version 150
#extension GL_EXT_gpu_shader4 : enable
#extension GL_EXT_geometry_shader4 : enable

uniform float _thickness = 30f;
uniform float _miterLimit = 0.75;
uniform float _winScale;

void main()
{
  // get the four vertices passed to the shader:
  vec2 p0 = screen_space( gl_PositionIn[0] );	// start of previous segment
  vec2 p1 = screen_space( gl_PositionIn[1] );	// end of previous segment, start of current segment
  vec2 p2 = screen_space( gl_PositionIn[2] );	// end of current segment, start of next segment
  vec2 p3 = screen_space( gl_PositionIn[3] );	// end of next segment

  // determine the direction of each of the 3 segments (previous, current, next)
  vec2 v0 = normalize(p1-p0);
  vec2 v1 = normalize(p2-p1);
  vec2 v2 = normalize(p3-p2);

  // determine the normal of each of the 3 segments (previous, current, next)
  vec2 n0 = vec2(-v0.y, v0.x);
  vec2 n1 = vec2(-v1.y, v1.x);
  vec2 n2 = vec2(-v2.y, v2.x);

  // determine miter lines by averaging the normals of the 2 segments
  vec2 miter_a = normalize(n0 + n1);	// miter at start of current segment
  vec2 miter_b = normalize(n1 + n2);	// miter at end of current segment

  // determine the length of the miter by projecting it onto normal and then inverse it
  float length_a = _thickness / dot(miter_a, n1);
  float length_b = _thickness / dot(miter_b, n1);
}
";

        public static string GeoSource =
@"#version 120
//layout(lines) in;
//layout(line_strip, max_vertices=4) out;
//#extension GL_EXT_gpu_shader4 : enable
#extension GL_EXT_geometry_shader4 : enable
 
void main()
{
    for(int i = 0; i < gl_VerticesIn; i++)
    {
        // copy attributes
        gl_Position = gl_PositionIn[i];

        // done with the vertex
        EmitVertex();
    }
    EndPrimitive();
}
";
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
