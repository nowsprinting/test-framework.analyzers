// Dummy of Unity.PerformanceTesting.FramesMeasurement. Declaration-only; see test-data-conventions.md.

using System;
using System.Collections;

namespace Unity.PerformanceTesting
{
    public class FramesMeasurement
    {
        public ScopedFrameTimeMeasurement Scope(string name = "Time")
        {
            throw new NotImplementedException();
        }

        public IEnumerator Run()
        {
            throw new NotImplementedException();
        }

        public struct ScopedFrameTimeMeasurement : IDisposable
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}
