// Dummy of NUnit.Framework.Interfaces.ICommandWrapper. Declaration-only; see test-data-conventions.md.
using NUnit.Framework.Internal.Commands;

namespace NUnit.Framework.Interfaces
{
    public interface ICommandWrapper
    {
        TestCommand Wrap(TestCommand command);
    }
}
