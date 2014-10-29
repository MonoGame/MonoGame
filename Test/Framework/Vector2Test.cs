using Microsoft.Xna.Framework;
using NUnit.Framework;

namespace MonoGame.Tests.Framework
{
    class Vector2Test
    {
        [Test]
        public void CatmullRom()
        {
            var expectedResult = new Vector2(5.1944f, 6.1944f);
            var v1 = new Vector2(1, 2); var v2 = new Vector2(3, 4); var v3 = new Vector2(5, 6); var v4 = new Vector2(7, 8); var value = 1.0972f;

            Vector2 result;
            Vector2.CatmullRom(ref v1, ref v2, ref v3, ref v4, value, out result);
        
            Assert.AreEqual(expectedResult,Vector2.CatmullRom(v1,v2,v3,v4,value));
            Assert.AreEqual(expectedResult,result);
        }

        [Test]
        public void Multiply()
        {
            var vector = new Vector2(1, 2);

            // Test 0.0 scale.
            Assert.AreEqual(Vector2.Zero, 0 * vector);
            Assert.AreEqual(Vector2.Zero, vector * 0);
            Assert.AreEqual(Vector2.Zero, Vector2.Multiply(vector, 0));
            Assert.AreEqual(Vector2.Multiply(vector, 0), vector * 0.0f);

            // Test 1.0 scale.
            Assert.AreEqual(vector, 1 * vector);
            Assert.AreEqual(vector, vector * 1);
            Assert.AreEqual(vector, Vector2.Multiply(vector, 1));
            Assert.AreEqual(Vector2.Multiply(vector, 1), vector * 1.0f);

            var scaledVec = vector * 2;

            // Test 2.0 scale.
            Assert.AreEqual(scaledVec, 2 * vector);
            Assert.AreEqual(scaledVec, vector * 2);
            Assert.AreEqual(scaledVec, Vector2.Multiply(vector, 2));
            Assert.AreEqual(vector * 2.0f, scaledVec);
            Assert.AreEqual(2 * vector, Vector2.Multiply(vector, 2));

            scaledVec = vector * 0.999f;

            // Test 0.999 scale.
            Assert.AreEqual(scaledVec, 0.999f * vector);
            Assert.AreEqual(scaledVec, vector * 0.999f);
            Assert.AreEqual(scaledVec, Vector2.Multiply(vector, 0.999f));
            Assert.AreEqual(vector * 0.999f, scaledVec);
            Assert.AreEqual(0.999f * vector, Vector2.Multiply(vector, 0.999f));

            var vector2 = new Vector2(2, 2); 

            // Test two vectors multiplication.
            Assert.AreEqual(new Vector2(vector.X * vector2.X,vector.Y*vector2.Y), vector * vector2);
            Assert.AreEqual(vector2 * vector, new Vector2(vector.X * vector2.X, vector.Y * vector2.Y));
            Assert.AreEqual(vector * vector2, Vector2.Multiply(vector, vector2));
            Assert.AreEqual(Vector2.Multiply(vector, vector2), vector * vector2);

            Vector2 refVec;

            // Overloads comparsion 
            var vector3 =  Vector2.Multiply(vector, vector2);
            Vector2.Multiply(ref vector, ref vector2, out refVec);
            Assert.AreEqual(vector3, refVec);

            vector3 = Vector2.Multiply(vector, 2);
            Vector2.Multiply(ref vector, ref vector2, out refVec);
            Assert.AreEqual(vector3, refVec);
        }

        [Test]
        public void Hermite()
        {
            var t1 = new Vector2(1.40625f, 1.40625f);
            var t2 = new Vector2(2.662375f, 2.26537514f);

            var v1 = new Vector2(1, 1); var v2 = new Vector2(2, 2); var v3 = new Vector2(3, 3); var v4 = new Vector2(4, 4);
            var v5 = new Vector2(4, 3); var v6 = new Vector2(2, 1); var v7 = new Vector2(1, 2); var v8 = new Vector2(3, 4);

            Assert.AreEqual(t1,Vector2.Hermite(v1, v2, v3, v4, 0.25f));
            Assert.AreEqual(t2,Vector2.Hermite(v5, v6, v7, v8, 0.45f));

            Vector2 result1;
            Vector2 result2;

            Vector2.Hermite(ref v1, ref v2, ref v3, ref v4, 0.25f, out result1);
            Vector2.Hermite(ref v5, ref v6, ref v7, ref v8, 0.45f, out result2);

            Assert.AreEqual(t1,result1);
            Assert.AreEqual(t2,result2);
        }

        [Test]
        public void Transform()
        {
            // STANDART OVERLOADS TEST

            var expectedResult1 = new Vector2(24, 28);
            var expectedResult2 = new Vector2(-0.0168301091f, 2.30964f);

            var v1 = new Vector2(1, 2);
            var m1 = new Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);

            var v2 = new Vector2(1.1f, 2.45f);
            var q2 = new Quaternion(0.11f, 0.22f, 0.33f, 0.55f);

            var q3 = new Quaternion(1, 2, 3, 4);

            Assert.AreEqual(expectedResult1, Vector2.Transform(v1, m1));
            Assert.AreEqual(expectedResult2, Vector2.Transform(v2, q2));

            // OUTPUT OVERLOADS TEST

            Vector2 result1;
            Vector2 result2;

            Vector2.Transform(ref v1, ref m1, out result1);
            Vector2.Transform(ref v2, ref q2, out result2);

            Assert.AreEqual(expectedResult1, result1);
            Assert.AreEqual(expectedResult2, result2);

            // TRANSFORM ON LIST (MATRIX)
            {
                var sourceList1 = new Vector2[10];
                var desinationList1 = new Vector2[10];

                for (int i = 0; i < 10; i++)
                {
                    sourceList1[i] = (new Vector2(1 + i, 1 + i));
                }

                Vector2.Transform(sourceList1, 0, ref m1, desinationList1, 0, 10);

                for (int i = 0; i < 10; i++)
                {
                    Assert.AreEqual(desinationList1[i], new Vector2(19 + (6*i), 22 + (8*i)));
                }
            }
            // TRANSFORM ON LIST (MATRIX)(DESTINATION & SOURCE)
            {
                var sourceList2 = new Vector2[10];
                var desinationList2 = new Vector2[10];

                for (int i = 0; i < 10; i++)
                {
                    sourceList2[i] = (new Vector2(1 + i, 1 + i));
                }

                Vector2.Transform(sourceList2, 2, ref m1, desinationList2, 1, 3);

                Assert.AreEqual(Vector2.Zero, desinationList2[0]);

                Assert.AreEqual(new Vector2(31, 38), desinationList2[1]);
                Assert.AreEqual(new Vector2(37, 46), desinationList2[2]);
                Assert.AreEqual(new Vector2(43, 54), desinationList2[3]);

                for (int i = 4; i < 10; i++)
                {
                    Assert.AreEqual(Vector2.Zero, desinationList2[i]);
                }
            }
            // TRANSFORM ON LIST (MATRIX)(SIMPLE)
            {
                var sourceList3 = new Vector2[10];
                var desinationList3 = new Vector2[10];

                for (int i = 0; i < 10; i++)
                {
                    sourceList3[i] = (new Vector2(1 + i, 1 + i));
                }

                Vector2.Transform(sourceList3, ref m1, desinationList3);

                for (int i = 0; i < 10; i++)
                {
                    Assert.AreEqual(desinationList3[i], new Vector2(19 + (6*i), 22 + (8*i)));
                }
            }
            // TRANSFORM ON LIST (QUATERNION)
            {
                var sourceList4 = new Vector2[10];
                var desinationList4 = new Vector2[10];

                for (int i = 0; i < 10; i++)
                {
                    sourceList4[i] = (new Vector2(1 + i, 1 + i));
                }

                Vector2.Transform(sourceList4, 0, ref q3, desinationList4, 0, 10);

                for (int i = 0; i < 10; i++)
                {
                    Assert.AreEqual(new Vector2(-45 + (-45*i), 9 + (9*i)), desinationList4[i]);
                }
            }
            // TRANSFORM ON LIST (QUATERNION)(DESTINATION & SOURCE)
            {
                var sourceList5 = new Vector2[10];
                var desinationList5 = new Vector2[10];

                for (int i = 0; i < 10; i++)
                {
                    sourceList5[i] = (new Vector2(1 + i, 1 + i));
                }

                Vector2.Transform(sourceList5, 2, ref q3, desinationList5, 1, 3);

                Assert.AreEqual(Vector2.Zero, desinationList5[0]);

                Assert.AreEqual(new Vector2(-135,27), desinationList5[1]);
                Assert.AreEqual(new Vector2(-180,36), desinationList5[2]);
                Assert.AreEqual(new Vector2(-225,45), desinationList5[3]);

                for (int i = 4; i < 10; i++)
                {
                    Assert.AreEqual(Vector2.Zero, desinationList5[i]);
                }
            }
            // TRANSFORM ON LIST (QUATERNION)(SIMPLE)
            {
                var sourceList6 = new Vector2[10];
                var desinationList6 = new Vector2[10];

                for (int i = 0; i < 10; i++)
                {
                    sourceList6[i] = (new Vector2(1 + i, 1 + i));
                }

                Vector2.Transform(sourceList6, ref q3, desinationList6);

                for (int i = 0; i < 10; i++)
                {
                    Assert.AreEqual(new Vector2(-45 + (-45*i), 9 + (9*i)), desinationList6[i]);
                }
            }
        }

        [Test]
        public void TransformNormal()
        {
            var normal = new Vector2(1.5f, 2.5f);
            var matrix = new Matrix(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);

            var expectedResult1 = new Vector2(14, 18);
            var expectedResult2 = expectedResult1;

            Assert.AreEqual(expectedResult1, Vector2.TransformNormal(normal, matrix));

            Vector2 result;          
            Vector2.TransformNormal(ref normal, ref matrix, out result);

            Assert.AreEqual(expectedResult2, result);

            // TRANSFORM ON LIST
            {
                var sourceArray1 = new Vector2[10];
                var destinationArray1 = new Vector2[10];

                for (int i = 0; i < 10; i++)
                {
                    sourceArray1[i] = new Vector2(i, i);
                }

                Vector2.TransformNormal(sourceArray1, 0, ref matrix, destinationArray1, 0, 10);

                for (int i = 0; i < 10; i++)
                {
                    Assert.AreEqual(new Vector2(0 + (6*i), 0 + (8*i)), destinationArray1[i]);
                }
            }
            // TRANSFORM ON LIST(SOURCE OFFSET)
            {
                var sourceArray2 = new Vector2[10];
                var destinationArray2 = new Vector2[10];

                for (int i = 0; i < 10; i++)
                {
                    sourceArray2[i] = new Vector2(i, i);
                }

                Vector2.TransformNormal(sourceArray2, 5, ref matrix, destinationArray2, 0, 5);

                for (int i = 0; i < 5; i++)
                {
                    Assert.AreEqual(new Vector2(30 + (6 * i), 40 + (8 * i)), destinationArray2[i]);
                }

                for (int i = 5; i < 10; i++)
                {
                    Assert.AreEqual(Vector2.Zero, destinationArray2[i]);
                }
            }
            // TRANSFORM ON LIST(DESTINATION OFFSET)
            {
                var sourceArray3 = new Vector2[10];
                var destinationArray3 = new Vector2[10];

                for (int i = 0; i < 10; ++i)
                {
                    sourceArray3[i] = new Vector2(i, i);
                }

                Vector2.TransformNormal(sourceArray3, 0, ref matrix, destinationArray3, 5, 5);

                for (int i = 0; i < 6; ++i)
                {
                    Assert.AreEqual(Vector2.Zero, destinationArray3[i]);
                }

                Assert.AreEqual(new Vector2(6, 8), destinationArray3[6]);
                Assert.AreEqual(new Vector2(12, 16), destinationArray3[7]);
                Assert.AreEqual(new Vector2(18, 24), destinationArray3[8]);
                Assert.AreEqual(new Vector2(24, 32), destinationArray3[9]);
            }
            // TRANSFORM ON LIST(DESTINATION & SOURCE)
            {
                var sourceArray4 = new Vector2[10];
                var destinationArray4 = new Vector2[10];

                for (int i = 0; i < 10; ++i)
                {
                    sourceArray4[i] = new Vector2(i, i);
                }

                Vector2.TransformNormal(sourceArray4, 2, ref matrix, destinationArray4, 3, 6);

                for (int i = 0; i < 3; ++i)
                {
                    Assert.AreEqual(Vector2.Zero, destinationArray4[i]);
                } 

                Assert.AreEqual(new Vector2(12, 16), destinationArray4[3]);
                Assert.AreEqual(new Vector2(18, 24), destinationArray4[4]);
                Assert.AreEqual(new Vector2(24, 32), destinationArray4[5]);
                Assert.AreEqual(new Vector2(30, 40), destinationArray4[6]);
                Assert.AreEqual(new Vector2(36, 48), destinationArray4[7]);
                Assert.AreEqual(new Vector2(42, 56), destinationArray4[8]); 

                Assert.AreEqual(Vector2.Zero, destinationArray4[9]);
            }
            // TRANSFORM ON LIST(SIMPLE)
            {
                var sourceArray5 = new Vector2[10];
                var destinationArray5 = new Vector2[10];

                for (int i = 0; i < 10; ++i)
                {
                    sourceArray5[i] = new Vector2(i, i);
                }

                Vector2.TransformNormal(sourceArray5, ref matrix, destinationArray5);

                for (int i = 0; i < 10; ++i)
                {
                    Assert.AreEqual(new Vector2(0 + (6 * i), 0 + (8 * i)), destinationArray5[i]);
                }
            }
        }

        [Test]
        public void ToPoint()
        {
            Assert.AreEqual(new Point(0, 0), new Vector2(0.1f, 0.1f).ToPoint());
            Assert.AreEqual(new Point(0, 0), new Vector2(0.5f, 0.5f).ToPoint());
            Assert.AreEqual(new Point(0, 0), new Vector2(0.55f, 0.55f).ToPoint());
            Assert.AreEqual(new Point(0, 0), new Vector2(1.0f - 0.1f, 1.0f - 0.1f).ToPoint());
            Assert.AreEqual(new Point(1, 1), new Vector2(1.0f - float.Epsilon, 1.0f - float.Epsilon).ToPoint());
            Assert.AreEqual(new Point(1, 1), new Vector2(1.0f, 1.0f).ToPoint());
            Assert.AreEqual(new Point(19, 27), new Vector2(19.033f, 27.1f).ToPoint());
        }
    }
}
