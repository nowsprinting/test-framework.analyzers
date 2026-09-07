using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace UTF.Analyzers.Tests.TestData.UTF3006
{
    public class TestMethods
    {
        [Test]
        public async Task AddTwoPositiveNumbers() // VSTHRD200
        {
            await Task.Yield();
        }

        [UnityTest]
        public async Task LoadScene() // VSTHRD200
        {
            await Task.Yield();
        }

        [TestCase(1)]
        [TestCase(2)]
        public async Task IsPositive(int value) // VSTHRD200
        {
            await Task.Yield();
        }

        [TestCaseSource(nameof(Cases))]
        public async Task IsPositiveFromSource(int value) // VSTHRD200
        {
            await Task.Yield();
        }

        private static readonly int[] Cases = { 1, 2 };
    }
}
