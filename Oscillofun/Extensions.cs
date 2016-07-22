using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
namespace Oscillofun
{
    public static class Extensions
    {
        public static float DistanceFast(this Vector2 a, ref Vector2 b)
        {
            return (a - b).LengthFast;
        }
    }
}
