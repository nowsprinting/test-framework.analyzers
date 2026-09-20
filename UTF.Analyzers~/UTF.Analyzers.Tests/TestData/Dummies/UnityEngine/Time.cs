// Dummy of UnityEngine.Time. Declaration-only; see test-data-conventions.md.

using System;

namespace UnityEngine
{
    public class Time
    {
        public static float time => throw new NotImplementedException();

        public static float deltaTime => throw new NotImplementedException();

        public static int frameCount => throw new NotImplementedException();
    }
}
