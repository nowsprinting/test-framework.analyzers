using System.Collections;
using System.IO;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3003
{
    public abstract class TearDownBase
    {
        [UnityTearDown]
        public abstract IEnumerator TearDown();
    }

    public class FieldDisposedInOverriddenUnityTearDown : TearDownBase // CA1001
    {
        private StreamWriter _writer;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return null;
            _writer = new StreamWriter("log.txt");
        }

        public override IEnumerator TearDown()
        {
            _writer.Dispose();
            yield return null;
        }
    }
}
