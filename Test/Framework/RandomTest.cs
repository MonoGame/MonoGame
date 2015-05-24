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

        [Test]
        public void LowerBound()
        {
            // Due to the mathematical definition of the permutation step, any internal
            // state below 2^29 will output 32 zero bits, representing the lower edge case:
            // All Next variants should return the lower bounds of their outputs

            // Due to the way the LCG's increment is calculated from the sequence id, seeding
            // the generator to zero and using a sufficiently low id will force this condition.

            MathHelper.Random rng = new MathHelper.Random(0);

            rng.Seed = 0;
            Assert.AreEqual(0, rng.Next());

            rng.Seed = 0;
            Assert.AreEqual(16, rng.NextByte(16, 32));

            rng.Seed = 0;
            Assert.AreEqual(-128, rng.NextShort(-128, 128));

            rng.Seed = 0;
            Assert.AreEqual(-5, rng.Next(-5, int.MaxValue));

            rng.Seed = 0;
            Assert.AreEqual(0.0, rng.NextFloat(), 0.0);

            // NextLong() and NextDouble() require more than 32 bits of entropy, and thus generate
            // two numbers internally, so Wolfram Alpha was used to solve this equation:
            //     0 = (6364136223846793005 x + 1) mod 2^64
            // <=> x = 18446744073709551616 n + 4568919932995229531,   n element Z
            // Any seed x (4568919932995229531, for n = 0) therefore produces an internal state 0.

            rng.Seed = 4568919932995229531;
            rng.Next();
            Assert.AreEqual(0, rng.State);

            rng.Seed = 4568919932995229531;
            Assert.AreEqual(0, rng.NextLong());

            rng.Seed = 4568919932995229531;
            Assert.AreEqual(-500000, rng.NextLong(-500000, 0));

            rng.Seed = 4568919932995229531;
            Assert.AreEqual(0.0, rng.NextDouble(), 0.0);
        }
    }
}
