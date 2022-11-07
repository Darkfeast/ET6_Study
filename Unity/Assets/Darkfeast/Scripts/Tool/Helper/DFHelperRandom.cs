using System;

public partial class DFHelper
{
    public static class ToolRandom
    {
        private static Random r;

        static ToolRandom()
        {
            r = new Random();
        }

        public static double RandomFloat(int max)
        {
            return r.NextDouble() * max;
        }

        public static double RandomFloat01()
        {
            return RandomFloat(1);
        }

        // public static double RandomFloatRange(int min, int max)
        // {
        //     return RandomFloat(max - min) + min;
        // }
        public static double RandomFloat(int min, int max)
        {
            return RandomFloat(max - min) + min;
        }

        public static int RandomInt(int max)
        {
            return r.Next(max);
        }

        public static int RandomInt01()
        {
            return RandomInt(2);
        }

        // public static int RandomIntRange(int min, int max)
        // {
        //     return RandomInt(max - min) + min;
        // }
        
        public static int RandomInt(int min, int max)
        {
            return RandomInt(max - min) + min;
        }
    }
}