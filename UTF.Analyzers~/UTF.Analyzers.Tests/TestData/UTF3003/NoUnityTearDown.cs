using System.Collections;
using System.IO;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3003
{
    public class NoUnityTearDown // CA1001
    {
        private StreamWriter _writer;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return null;
            _writer = new StreamWriter("log.txt");
        }

        public void Write()
        {
            _writer.Write("test");
        }
    }
}
