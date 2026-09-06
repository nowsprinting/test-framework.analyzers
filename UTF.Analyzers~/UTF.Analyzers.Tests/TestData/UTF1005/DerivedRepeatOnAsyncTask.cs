using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF1005
{
    public class DerivedRepeatOnAsyncTask
    {
        [Test]
        [DerivedRepeatOnAsyncTaskRepeat(3)] // UTF1005
        public async Task MyAsyncTest()
        {
            await Task.Yield();
        }
    }

    public class DerivedRepeatOnAsyncTaskRepeatAttribute : RepeatAttribute
    {
        public DerivedRepeatOnAsyncTaskRepeatAttribute(int count) : base(count)
        {
        }
    }
}
