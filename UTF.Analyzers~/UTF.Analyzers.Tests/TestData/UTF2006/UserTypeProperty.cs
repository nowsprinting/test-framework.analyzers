using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;

namespace UTF.Analyzers.Tests.TestData.UTF2006
{
    public class UserTypeProperty
    {
        private class ScoreBoard
        {
            public int Score { get; set; }
        }

        [Test]
        public void Test()
        {
            Assert.That(new ScoreBoard(), Has.Property("Score").EqualTo(0)); // UTF2006
        }
    }
}
