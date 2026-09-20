using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace UTF.Analyzers.Tests.TestData.UTF4001
{

    public class ClockReadInLoopBody
    {
        private bool _flag;
        private float _elapsed;

        [Test]
        public async Task Test()
        {
            while (!_flag)   // UTF4001
            {
                _elapsed += Time.deltaTime;
                await Task.Yield();
            }
        }
    }
}
