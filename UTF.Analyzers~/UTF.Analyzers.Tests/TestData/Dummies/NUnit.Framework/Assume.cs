// Dummy of NUnit.Framework.Assume. Declaration-only; see test-data-conventions.md.

using System;
using NUnit.Framework.Constraints;

namespace NUnit.Framework
{
    public abstract class Assume
    {
        public static void That<TActual>(ActualValueDelegate<TActual> del, IResolveConstraint expr)
        {
            throw new NotImplementedException();
        }

        public static void That<TActual>(ActualValueDelegate<TActual> del, IResolveConstraint expr, string message,
            params object[] args)
        {
            throw new NotImplementedException();
        }

        public static void That(TestDelegate code, IResolveConstraint constraint)
        {
            throw new NotImplementedException();
        }

        public static void That<TActual>(TActual actual, IResolveConstraint expression)
        {
            throw new NotImplementedException();
        }
    }
}
