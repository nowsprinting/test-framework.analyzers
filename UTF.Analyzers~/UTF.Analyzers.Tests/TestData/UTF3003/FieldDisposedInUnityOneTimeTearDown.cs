using System.Collections;
using System.IO;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3003
{
    public class FieldDisposedInUnityOneTimeTearDown // CA1001
    {
        private StreamWriter _writer;

        [UnityOneTimeSetUp]
        public IEnumerator OneTimeSetUp()
        {
            yield return null;
            _writer = new StreamWriter("log.txt");
        }

        [UnityOneTimeTearDown]
        public IEnumerator OneTimeTearDown()
        {
            _writer.Dispose();
            yield return null;
        }
    }
}
