using System;
using System.Collections.Generic;

using Vec3 = Microsoft.Xna.Framework.Vector3;
using Vec4 = Microsoft.Xna.Framework.Vector4;

namespace TextureSquish
{
    class ClusterFit : ColourFit
    {
        private const int MAXITERATIONS = 8;
        
        private static readonly Vec4 HALF_HALF2 = new Vec4(0.5f, 0.5f, 0.5f, 0.25f);
        private static readonly Vec4 HALF = new Vec4(0.5f);
        private static readonly Vec4 GRID = new Vec4(31.0f, 63.0f, 31.0f, 0.0f);
        private static readonly Vec4 GRIDRCP = new Vec4(1) / GRID;
        private static readonly Vec4 ONETHIRD_ONETHIRD2 = new Vec4(1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 3.0f, 1.0f / 9.0f);
        private static readonly Vec4 TWOTHIRDS_TWOTHIRDS2 = new Vec4(2.0f / 3.0f, 2.0f / 3.0f, 2.0f / 3.0f, 4.0f / 9.0f);
        private static readonly Vec4 TWONINETHS = new Vec4(2.0f / 9.0f);

        public ClusterFit(ColourSet colours, CompressionMode flags) : base(colours, flags)
        {
            // set the iteration count
            m_iterationCount = (m_flags & CompressionMode.ColourIterativeClusterFit) != 0 ? MAXITERATIONS : 1;

            // initialise the best error
            m_besterror = new Vec4(float.MaxValue);

            // initialise the metric
            bool perceptual = ((m_flags & CompressionMode.ColourMetricPerceptual) != 0);
            
            m_metric = perceptual ? new Vec4(0.2126f, 0.7152f, 0.0722f, 0.0f) : Vec4.One;

            // cache some values
            var count = m_colours.Count;
            var values = m_colours.Points;

            // get the covariance matrix
            Sym3x3 covariance = Sym3x3.ComputeWeightedCovariance(count, values, m_colours.Weights);

            // compute the principle component
            m_principle = Sym3x3.ComputePrincipleComponent(covariance);
        }

        private bool ConstructOrdering(Vec3 axis, int iteration)
        {
            // cache some values
            var count = m_colours.Count;
            var values = m_colours.Points;

            // build the list of dot products
            var dps = new float[16];            

            var orderIndex =16*iteration;           

            for (int i = 0; i < count; ++i)
            {
                dps[i] = Vec3.Dot(values[i], axis);
                m_order[orderIndex+i] = (Byte)i;
            }

            // stable sort using them
            for (int i = 0; i < count; ++i)
            {
                for (int j = i; j > 0 && dps[j] < dps[j - 1]; --j)
                {
                    dps.SwapElements(j, j - 1);
                    m_order.SwapElements(orderIndex + j, orderIndex + j - 1);
                }
            }

            // check this ordering is unique
            for (int it = 0; it < iteration; ++it)
            {
                var prevIdx = 16 * it;
                bool same = true;
                for (int i = 0; i < count; ++i)
                {
                    if (m_order[orderIndex + i] != m_order[prevIdx+i])
                    {
                        same = false;
                        break;
                    }
                }
                if (same)
                    return false;
            }

            // copy the ordering and weight all the points
            var unweighted = m_colours.Points;
            var weights = m_colours.Weights;
            m_xsum_wsum = Vec4.Zero;

            for (int i = 0; i < count; ++i)
            {
                int j = m_order[orderIndex + i];
                var p = new Vec4(unweighted[j], 1);
                var w = new Vec4(weights[j]);
                var x = p * w;
                m_points_weights[i] = x;
                m_xsum_wsum += x;
            }

            return true;
        }

        protected override void Compress3(BlockWindow block)
        {
            // declare variables
            var count = m_colours.Count;
            
            // prepare an ordering using the principle axis
            ConstructOrdering(m_principle, 0);

            // check all possible clusters and iterate on the total order
            Vec4 beststart = Vec4.Zero;
            Vec4 bestend = Vec4.Zero;
            Vec4 besterror = m_besterror;
            var bestindices = new Byte[16];
            int bestiteration = 0;
            int besti = 0, bestj = 0;

            // loop over iterations (we avoid the case that all points in first or last cluster)
            for (int iterationIndex = 0; ;)
            {
                // first cluster [0,i) is at the start
                var part0 = Vec4.Zero;
                for (int i = 0; i < count; ++i)
                {
                    // second cluster [i,j) is half along
                    Vec4 part1 = (i == 0) ? m_points_weights[0] : Vec4.Zero;
                    int jmin = (i == 0) ? 1 : i;
                    for (int j = jmin; ;)
                    {
                        // last cluster [j,count) is at the end
                        Vec4 part2 = m_xsum_wsum - part1 - part0;

                        // compute least squares terms directly
                        Vec4 alphax_sum = HALF_HALF2.MultiplyAdd(part1, part0);
                        Vec4 betax_sum  = HALF_HALF2.MultiplyAdd(part1, part2);

                        Vec4 alpha2_sum = alphax_sum.SplatW();
                        Vec4 beta2_sum  = betax_sum.SplatW();

                        Vec4 alphabeta_sum = (part1 * HALF_HALF2).SplatW();

                        // compute the least-squares optimal points
                        Vec4 factor = alphabeta_sum.NegativeMultiplySubtract(alphabeta_sum, alpha2_sum * beta2_sum).Reciprocal();
                        Vec4 a      =     betax_sum.NegativeMultiplySubtract(alphabeta_sum, alphax_sum * beta2_sum) * factor;
                        Vec4 b      =    alphax_sum.NegativeMultiplySubtract(alphabeta_sum, betax_sum * alpha2_sum) * factor;

                        // clamp to the grid
                        a = a.Clamp(Vec4.Zero,Vec4.One);
                        b = b.Clamp(Vec4.Zero,Vec4.One);
                        a = GRID.MultiplyAdd(a, HALF).Truncate() * GRIDRCP;
                        b = GRID.MultiplyAdd(b, HALF).Truncate() * GRIDRCP;

                        // compute the error (we skip the constant xxsum)
                        Vec4 e1 = (a * a * alpha2_sum) + (b * b * beta2_sum);
                        Vec4 e2 = a.NegativeMultiplySubtract(alphax_sum, a * b * alphabeta_sum);
                        Vec4 e3 = b.NegativeMultiplySubtract(betax_sum, e2);
                        Vec4 e4 = e3 * 2 + e1;

                        // apply the metric to the error term
                        Vec4 e5 = e4 * m_metric;
                        Vec4 error = e5.SplatX() + e5.SplatY() + e5.SplatZ();

                        // keep the solution if it wins
                        if (error.CompareAnyLessThan(besterror))
                        {
                            beststart = a;
                            bestend = b;
                            besti = i;
                            bestj = j;
                            besterror = error;
                            bestiteration = iterationIndex;
                        }

                        // advance
                        if (j == count) break;
                        part1 += m_points_weights[j];
                        ++j;
                    }

                    // advance
                    part0 += m_points_weights[i];
                }

                // stop if we didn't improve in this iteration
                if (bestiteration != iterationIndex) break;

                // advance if possible
                ++iterationIndex;
                if (iterationIndex == m_iterationCount) break;

                // stop if a new iteration is an ordering that has already been tried
                Vec3 axis = (bestend - beststart).GetVec3();
                if (!ConstructOrdering(axis, iterationIndex)) break;
            }

            // save the block if necessary
            if (besterror.CompareAnyLessThan(m_besterror))
            {
                // remap the indices
                var orderIndex = 16 * bestiteration;

                var unordered = new Byte[16];
                for (int m = 0;     m < besti; ++m) unordered[m_order[orderIndex + m]] = 0;
                for (int m = besti; m < bestj; ++m) unordered[m_order[orderIndex + m]] = 2;
                for (int m = bestj; m < count; ++m) unordered[m_order[orderIndex + m]] = 1;

                m_colours.RemapIndices(unordered, bestindices);

                // save the block
                block.WriteColourBlock3(beststart.GetVec3(), bestend.GetVec3(), bestindices);

                // save the error
                m_besterror = besterror;
            }
        }

        protected override void Compress4(BlockWindow block)
        {
            // declare variables
            var count = m_colours.Count;
            
            // prepare an ordering using the principle axis
            ConstructOrdering(m_principle, 0);

            // check all possible clusters and iterate on the total order
            Vec4 beststart = Vec4.Zero;
            Vec4 bestend = Vec4.Zero;
            Vec4 besterror = m_besterror;
            var bestindices = new Byte[16];
            int bestiteration = 0;
            int besti = 0, bestj = 0, bestk = 0;

            // loop over iterations (we avoid the case that all points in first or last cluster)
            for (int iterationIndex = 0; ;)
            {
                // first cluster [0,i) is at the start
                Vec4 part0 = Vec4.Zero;
                for (int i = 0; i < count; ++i)
                {
                    // second cluster [i,j) is one third along
                    Vec4 part1 = Vec4.Zero;
                    for (int j = i; ;)
                    {
                        // third cluster [j,k) is two thirds along
                        Vec4 part2 = (j == 0) ? m_points_weights[0] : Vec4.Zero;
                        int kmin = (j == 0) ? 1 : j;
                        for (int k = kmin; ;)
                        {
                            // last cluster [k,count) is at the end
                            Vec4 part3 = m_xsum_wsum - part2 - part1 - part0;

                            // compute least squares terms directly
                            Vec4 alphax_sum = ONETHIRD_ONETHIRD2.MultiplyAdd(part2, TWOTHIRDS_TWOTHIRDS2.MultiplyAdd(part1, part0));
                            Vec4 betax_sum  = ONETHIRD_ONETHIRD2.MultiplyAdd(part1, TWOTHIRDS_TWOTHIRDS2.MultiplyAdd(part2, part3));

                            Vec4 alpha2_sum = alphax_sum.SplatW();
                            Vec4 beta2_sum = betax_sum.SplatW();

                            Vec4 alphabeta_sum = TWONINETHS * (part1 + part2).SplatW();

                            // compute the least-squares optimal points
                            Vec4 factor = alphabeta_sum.NegativeMultiplySubtract(alphabeta_sum, alpha2_sum * beta2_sum).Reciprocal();
                            Vec4 a      =     betax_sum.NegativeMultiplySubtract(alphabeta_sum, alphax_sum * beta2_sum) * factor;
                            Vec4 b      =    alphax_sum.NegativeMultiplySubtract(alphabeta_sum, betax_sum * alpha2_sum) * factor;

                            // clamp to the grid
                            a = a.Clamp(Vec4.Zero,Vec4.One);
                            b = b.Clamp(Vec4.Zero, Vec4.One);
                            a = GRID.MultiplyAdd(a, HALF).Truncate() * GRIDRCP;
                            b = GRID.MultiplyAdd(b, HALF).Truncate() * GRIDRCP;

                            // compute the error (we skip the constant xxsum)
                            Vec4 e1 = (a * a * alpha2_sum) + (b * b * beta2_sum);
                            Vec4 e2 = a.NegativeMultiplySubtract(alphax_sum, a * b * alphabeta_sum);
                            Vec4 e3 = b.NegativeMultiplySubtract(betax_sum, e2);
                            Vec4 e4 = e3 * 2 + e1;

                            // apply the metric to the error term
                            Vec4 e5 = e4 * m_metric;
                            Vec4 error = e5.SplatX() + e5.SplatY() + e5.SplatZ();

                            // keep the solution if it wins
                            if (error.CompareAnyLessThan(besterror))
                            {
                                beststart = a;
                                bestend = b;
                                besterror = error;
                                besti = i;
                                bestj = j;
                                bestk = k;
                                bestiteration = iterationIndex;
                            }

                            // advance
                            if (k == count) break;
                            part2 += m_points_weights[k];
                            ++k;
                        }

                        // advance
                        if (j == count) break;
                        part1 += m_points_weights[j];
                        ++j;
                    }

                    // advance
                    part0 += m_points_weights[i];
                }

                // stop if we didn't improve in this iteration
                if (bestiteration != iterationIndex) break;

                // advance if possible
                ++iterationIndex;
                if (iterationIndex == m_iterationCount) break;

                // stop if a new iteration is an ordering that has already been tried
                Vec3 axis = (bestend - beststart).GetVec3();
                if (!ConstructOrdering(axis, iterationIndex)) break;
            }

            // save the block if necessary
            if (besterror.CompareAnyLessThan(m_besterror))
            {
                // remap the indices
                var orderIndex = 16 * bestiteration;

                var unordered = new Byte[16];
                for (int m = 0;     m < besti; ++m) unordered[m_order[orderIndex + m]] = 0;
                for (int m = besti; m < bestj; ++m) unordered[m_order[orderIndex + m]] = 2;
                for (int m = bestj; m < bestk; ++m) unordered[m_order[orderIndex + m]] = 3;
                for (int m = bestk; m < count; ++m) unordered[m_order[orderIndex + m]] = 1;

                m_colours.RemapIndices(unordered, bestindices);

                // save the block
                block.WriteColourBlock4(beststart.GetVec3(), bestend.GetVec3(), bestindices);

                // save the error
                m_besterror = besterror;
            }
        }        

        private int m_iterationCount;
        private Vec3 m_principle;
        private readonly Byte[] m_order = new Byte[16 * MAXITERATIONS];
        private readonly Vec4[] m_points_weights = new Vec4[16];
        private Vec4 m_xsum_wsum;
        private Vec4 m_metric;
        private Vec4 m_besterror;
    }
}


