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
    class Graphics
    {
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
        public Vector2[] Points;

        private const int ATRIB_POS = 0;
        private int[] attribBuffers = new int[1];

        public LineMesh( int vertexCount )
        {
            DrawMode = BeginMode.LineStrip;
            Points = new Vector2[vertexCount];
        }

        public override void UpdateBuffer()
        {
            if (VAO < 0)
            {
                //Create a new Vertex Array Object
                GL.GenVertexArrays(1, out VAO);
                GL.BindVertexArray(VAO);

                //Create the buffers for the vertex attributes
                GL.GenBuffers(1, attribBuffers);
            }
            
            //Upload the data to the gpu
            GL.BindBuffer(BufferTarget.ArrayBuffer, attribBuffers[ATRIB_POS]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Points.Length * Vector2.SizeInBytes), Points, this.UsageHint);

            //Define the position attribute
            GL.EnableVertexAttribArray(ATRIB_POS);
            GL.VertexAttribPointer(0, 2, VertexAttribPointerType.Float, false, Vector2.SizeInBytes, 0);

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
            GL.DrawArrays(this.DrawMode, 0, Points.Length);

            //Reset VAO
            GL.BindVertexArray(0);
            GL.Disable(EnableCap.Blend);
        }
    }
}
