using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF3003
{
    public class FieldDisposedInTearDown // CA1001
    {
        private StreamWriter _writer;

        [SetUp]
        public void SetUp()
        {
            _writer = new StreamWriter("log.txt");
        }

        [TearDown]
        public void TearDown()
        {
            _writer.Dispose();
        }
    }
}
