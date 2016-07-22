using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
namespace Oscillofun
{
    [StructLayout(LayoutKind.Sequential)]
    struct VertexUV
    {
        public static readonly int SizeInBytes = BlittableValueType.StrideOf(new VertexUV());

        /// <summary>
        /// The position in model-space of the vertex
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// The color of the vertex. Will be blended over textures/stuff
        /// </summary>
        public Vector3 Color;
        /// <summary>
        /// The UV coordinate for textures for this vertex
        /// </summary>
        public Vector2 UV;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public static readonly int SizeInBytes = BlittableValueType.StrideOf(new Vertex());

        /// <summary>
        /// The position in model-space of the vertex
        /// </summary>
        public Vector2 Position;
        /// <summary>
        /// The color of the vertex. Will be blended over textures/stuff
        /// </summary>
        public Vector3 Color;

        public void SetColor(float r, float g, float b)
        {
            Color.X = Math.Max(r, 0.001f);
            Color.Y = Math.Max(g, 0.001f);
            Color.Z = Math.Max(b, 0.001f);
        }

        public void FadeColor(float amt)
        {
            SetColor(Color.X * amt, Color.Y * amt, Color.Z * amt);
        }
    }

    abstract class Drawable
    {
        public BeginMode DrawMode = BeginMode.Triangles;
        public BufferUsageHint UsageHint = BufferUsageHint.StaticDraw;

        public Vector2 Position;
        public Vector2 Scale;
        float Angle;

        public int VAO = -1;

        public abstract void UpdateBuffer();
        public abstract void Draw();
    }

    class TexturedMesh : Drawable 
    {
        struct Vertex
        {
        }

        public override void UpdateBuffer()
        {
            throw new NotImplementedException();
        }

        public override void Draw()
        {
            throw new NotImplementedException();
        }
    }

    class LineMesh : Drawable 
    {
        //List of points to draw onto the screen
        public Vertex[] Vertices;

        private const int ATRIB_POS = 0;
        private const int ATRIB_COL = 1;
        private const int ATRIB_UV = 2;
        private int[] attribBuffers = new int[1];
        private int Vertex_Attribute_Buffer;

        public LineMesh( int vertexCount )
        {
            DrawMode = BeginMode.LineStrip;
            Vertices = new Vertex[vertexCount];

            this.UsageHint = BufferUsageHint.StreamDraw;
        }

        public override void UpdateBuffer()
        {
            if (VAO < 0)
            {
                //Create a new Vertex Array Object
                GL.GenVertexArrays(1, out VAO);
                GL.BindVertexArray(VAO);

                //Create the buffers for the vertex attributes
                GL.GenBuffers(1, out Vertex_Attribute_Buffer);
            }

            //Upload the data to the gpu
            GL.BindBuffer(BufferTarget.ArrayBuffer, Vertex_Attribute_Buffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertices.Length * Vertex.SizeInBytes), Vertices, this.UsageHint);

            //Define the position attribute
            GL.EnableVertexAttribArray(ATRIB_POS);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 0);

            //Define the color attribute
            GL.EnableVertexAttribArray(ATRIB_COL);
            GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes, 8);

            //Reset the acive VAO
            GL.BindVertexArray(0);

        }

        public override void Draw()
        {
            GL.Enable(EnableCap.Blend);
            GL.Color4(0.25f, 1.0f, 0.25f, 0.4f);
            //Bind the VAO
            GL.BindVertexArray(VAO);

            //Draw
            //GL.DrawElements(this.DrawMode, Points.Length, DrawElementsType.UnsignedInt, IntPtr.Zero);
            GL.DrawArrays(this.DrawMode, 0, Vertices.Length);

            //Reset VAO
            GL.BindVertexArray(0);
            GL.Disable(EnableCap.Blend);
        }
    }
}
