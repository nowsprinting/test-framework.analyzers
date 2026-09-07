using System.Collections;
using System.IO;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3003
{
    public class FieldDisposedInUnityTearDown // CA1001
    {
        private StreamWriter _writer;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return null;
            _writer = new StreamWriter("log.txt");
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            _writer.Dispose();
            yield return null;
        }
    }
}
