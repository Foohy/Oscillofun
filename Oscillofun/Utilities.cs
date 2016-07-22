using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
namespace Oscillofun
{
    class Utilities
    {
        public static Settings EngineSettings;
        public static float CurTime;
        public static float DeltaTime;

        public static Matrix4 ProjectionMatrix;
        public static Engine EngineInstance;

        public static float Lerp(float start, float end, float percent)
        {
            return start + percent * (end - start);
        }

        public static float Map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }

    }
}
