using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class CurveTest
    {
        //[Test]
        //public void TypeConverter()
        //{
        //    var curve = new Curve();

        //    curve.Keys.Add(new CurveKey(0, 1));
        //    curve.Keys.Add(new CurveKey(1, 2));
        //    curve.Keys.Add(new CurveKey(3, 4));

        //    // Gets the attributes for the instance.

        //    var attributes = TypeDescriptor.GetAttributes(curve);

        //    // Assert.AreEqual(2,attributes.Count);

        //    for (var i = 0; i < attributes.Count; ++i)
        //    {
        //        Console.WriteLine("attribute #" + i + " = " + attributes[i]);
        //        Assert.AreEqual(false,attributes[i].IsDefaultAttribute());
        //    }
        //}

        [Test]
        public void Clone()
        {
            var curve = new Curve();
            curve.Keys.Add(new CurveKey(0, 1));
            curve.Keys.Add(new CurveKey(1, 2));
            curve.Keys.Add(new CurveKey(3, 4));

            var clone = curve.Clone();
            
            Assert.AreEqual(curve.PostLoop, clone.PreLoop);
            Assert.AreEqual(curve.PostLoop, clone.PostLoop);

            for (var i = 0; i < 3; ++i)
            {
                Assert.AreEqual(curve.Keys[i], clone.Keys[i]);
            }
        }

        [Test]
        public void Evaluate()
        {
            var curve = new Curve();
            curve.Keys.Add(new CurveKey(1, -1));
            curve.Keys.Add(new CurveKey(2, 2));
            curve.Keys.Add(new CurveKey(3, 4));

            var result = curve.Evaluate(1.25f);

            Assert.AreEqual(-0.53125f, result);
        }

        [Test]
        public void EvaluateNoKeys()
        {
            var curve = new Curve();

            var result = curve.Evaluate(1.25f);

            Assert.AreEqual(0f, result);
        }

        [Test]
        public void EvaluateOneKey()
        {
            var curve = new Curve();
            curve.Keys.Add(new CurveKey(1, -1));

            var result = curve.Evaluate(1.25f);

            Assert.AreEqual(-1f, result);
        }

        [Test]
        public void ComputeTangent()
        {
            var key1 = new CurveKey(-0.5f, 1.5f); 
            var key2 = new CurveKey(1.1f, 2.3f);
            var key3 = new CurveKey(2.25f, 4.4f);

            var curve1 = new Curve();
            curve1.Keys.Add(key1);
            curve1.Keys.Add(key2);
            curve1.Keys.Add(key3);

            curve1.ComputeTangent(0, CurveTangent.Smooth);
            curve1.ComputeTangent(1, CurveTangent.Smooth);
            curve1.ComputeTangent(2, CurveTangent.Smooth);

            Assert.AreEqual(0.0f, curve1.Keys[0].TangentIn);
            Assert.AreEqual(0.799999952f, curve1.Keys[0].TangentOut);

            Assert.AreEqual(1.68727279f, curve1.Keys[1].TangentIn);
            Assert.AreEqual(1.21272731f, curve1.Keys[1].TangentOut);

            Assert.AreEqual(2.10000014f, curve1.Keys[2].TangentIn);
            Assert.AreEqual(0.0f, curve1.Keys[2].TangentOut);

            var curve2 = new Curve();
            curve2.Keys.Add(key1);
            curve2.Keys.Add(key2);
            curve2.Keys.Add(key3);

            curve2.ComputeTangent(0, CurveTangent.Flat);
            curve2.ComputeTangent(1, CurveTangent.Flat);
            curve2.ComputeTangent(2, CurveTangent.Flat);

            Assert.AreEqual(0.0f, curve2.Keys[0].TangentIn);
            Assert.AreEqual(0.0f, curve2.Keys[0].TangentOut);

            Assert.AreEqual(0.0f, curve2.Keys[1].TangentIn);
            Assert.AreEqual(0.0f, curve2.Keys[1].TangentOut);

            Assert.AreEqual(0.0f, curve2.Keys[2].TangentIn);
            Assert.AreEqual(0.0f, curve2.Keys[2].TangentOut);

            var curve3 = new Curve();
            curve3.Keys.Add(key1);
            curve3.Keys.Add(key2);
            curve3.Keys.Add(key3);

            curve3.ComputeTangent(0, CurveTangent.Linear);
            curve3.ComputeTangent(1, CurveTangent.Linear);
            curve3.ComputeTangent(2, CurveTangent.Linear);

            Assert.AreEqual(0.0f, curve3.Keys[0].TangentIn);
            Assert.AreEqual(0.799999952f, curve3.Keys[0].TangentOut);

            Assert.AreEqual(0.799999952f, curve3.Keys[1].TangentIn);
            Assert.AreEqual(2.10000014f, curve3.Keys[1].TangentOut);

            Assert.AreEqual(2.10000014f, curve3.Keys[2].TangentIn);
            Assert.AreEqual(0.0f, curve3.Keys[2].TangentOut); 
        }

        [Test]
        public void ComputeTangents()
        {
            var key1 = new CurveKey(-0.5f, 1.5f);
            var key2 = new CurveKey(1.1f, 2.3f);
            var key3 = new CurveKey(2.25f, 4.4f);

            var curve1 = new Curve();
            curve1.Keys.Add(key1);
            curve1.Keys.Add(key2);
            curve1.Keys.Add(key3);

            curve1.ComputeTangents(CurveTangent.Smooth);

            Assert.AreEqual(0.0f, curve1.Keys[0].TangentIn);
            Assert.AreEqual(0.799999952f, curve1.Keys[0].TangentOut);

            Assert.AreEqual(1.68727279f, curve1.Keys[1].TangentIn);
            Assert.AreEqual(1.21272731f, curve1.Keys[1].TangentOut);

            Assert.AreEqual(2.10000014f, curve1.Keys[2].TangentIn);
            Assert.AreEqual(0.0f, curve1.Keys[2].TangentOut);

            var curve2 = new Curve();
            curve2.Keys.Add(key1);
            curve2.Keys.Add(key2);
            curve2.Keys.Add(key3);

            curve2.ComputeTangents(CurveTangent.Flat);

            Assert.AreEqual(0.0f, curve2.Keys[0].TangentIn);
            Assert.AreEqual(0.0f, curve2.Keys[0].TangentOut);

            Assert.AreEqual(0.0f, curve2.Keys[1].TangentIn);
            Assert.AreEqual(0.0f, curve2.Keys[1].TangentOut);

            Assert.AreEqual(0.0f, curve2.Keys[2].TangentIn);
            Assert.AreEqual(0.0f, curve2.Keys[2].TangentOut);

            var curve3 = new Curve();
            curve3.Keys.Add(key1);
            curve3.Keys.Add(key2);
            curve3.Keys.Add(key3);

            curve3.ComputeTangents(CurveTangent.Linear);

            Assert.AreEqual(0.0f, curve3.Keys[0].TangentIn);
            Assert.AreEqual(0.799999952f, curve3.Keys[0].TangentOut);

            Assert.AreEqual(0.799999952f, curve3.Keys[1].TangentIn);
            Assert.AreEqual(2.10000014f, curve3.Keys[1].TangentOut);

            Assert.AreEqual(2.10000014f, curve3.Keys[2].TangentIn);
            Assert.AreEqual(0.0f, curve3.Keys[2].TangentOut); 
        }

        [Test]
        public void IsConstant()
        {
            var curve1 = new Curve();
            var curve2 = new Curve();
            curve2.Keys.Add(new CurveKey(0, 0));
            var curve3 = new Curve();
            curve3.Keys.Add(new CurveKey(0, 0));
            curve3.Keys.Add(new CurveKey(0, 0));

            Assert.AreEqual(true, curve1.IsConstant);
            Assert.AreEqual(true, curve2.IsConstant);
            Assert.AreEqual(false, curve3.IsConstant);
        }
    }
}
