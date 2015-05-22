using Microsoft.Xna.Framework;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace MonoGame.Tests.Framework
{
    class RandomTest
    {
        [Test]
        public void GenerationStep()
        {
            MathHelper.Random rng = new MathHelper.Random();
            rng.Seed = 0;

            ulong[] states = new ulong[]
            {
                0x0000000000000000,
                0x14057B7EF767814F,
                0x1A08EE1184BA6D32,
                0x9AF678222E728119,
                0x66B61AE97F2099B4,
                0x62354CDA6226D1F3
            };

            foreach (ulong expected in states)
            {
                Assert.AreEqual(expected, rng.State);
                rng.Next();
            }
        }

        [Test]
        public void PermutationStep()
        {
            MathHelper.Random rng = new MathHelper.Random();
            rng.Seed = 0;

            int[] values = new int[]
            {
                0x0AF65DC5,
                0x11DCF34E,
                0x5ECF493E,
                0x2D86DCE9,
                0x0D5354AD
            };

            foreach (int expected in values)
            {
                Assert.AreEqual(expected, rng.Next());
            }
        }
    }
}
