using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF1001;

public class UnityTestWithValueSource
{
    private static readonly int[] s_values = { 1, 2 };

    [UnityTest]
    public IEnumerator MyCoroutineTest([ValueSource(nameof(s_values))] int value)
    {
        yield return null;
    }
}
