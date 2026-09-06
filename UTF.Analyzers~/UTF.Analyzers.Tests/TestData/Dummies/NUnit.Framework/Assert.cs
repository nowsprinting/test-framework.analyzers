// Dummy of NUnit.Framework.Assert. Declaration-only; see test-data-conventions.md.
// Bodies never forward to another Assert member: the dummy is analyzed by the analyzer under test,
// so a CatchAsync that calls ThrowsAsync (as the real one does) would report UTF2001 in every test.
using System;

namespace NUnit.Framework
{
    public abstract class Assert
    {
        public static Exception ThrowsAsync(Type expectedExceptionType, AsyncTestDelegate code)
        {
            throw new NotImplementedException();
        }

        public static TActual ThrowsAsync<TActual>(AsyncTestDelegate code) where TActual : Exception
        {
            throw new NotImplementedException();
        }

        public static Exception CatchAsync(AsyncTestDelegate code)
        {
            throw new NotImplementedException();
        }

        public static TActual CatchAsync<TActual>(AsyncTestDelegate code) where TActual : Exception
        {
            throw new NotImplementedException();
        }

        public static void DoesNotThrowAsync(AsyncTestDelegate code)
        {
            throw new NotImplementedException();
        }

        public static TActual Throws<TActual>(TestDelegate code) where TActual : Exception
        {
            throw new NotImplementedException();
        }
    }
}
