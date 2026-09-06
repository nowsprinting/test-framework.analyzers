using System;
using System.Threading.Tasks;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2001
{
    public class TryCatchGood
    {
        [Test]
        public async Task ExceptionIsThrown()
        {
            try
            {
                await FooAsync();
            }
            catch (InvalidOperationException e)
            {
                Console.WriteLine(e.Message);
                return;
            }

            throw new Exception("Expected InvalidOperationException was not thrown");
        }

        private static async Task FooAsync()
        {
            await Task.Yield();
            throw new InvalidOperationException("boom");
        }
    }
}
