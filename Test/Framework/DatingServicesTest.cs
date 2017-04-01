using System;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    public class DatingServicesTest
    {
        [Test]
        public void TestConnection()
        {
            Contract.Requires(DatingService.Response != "Shutdown");
        }
    }
}
