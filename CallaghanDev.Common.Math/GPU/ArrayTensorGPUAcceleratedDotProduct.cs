using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.OpenCL;

namespace CallaghanDev.Utilities.Math.GPU
{
    public class GPUAcceleratedArrayTensor
    {
        public class DotProduct
        {
            private Accelerator _accelerator;

            private Action<Index1D, ArrayView1D<float, Stride1D.Dense>, ArrayView1D<float, Stride1D.Dense>, ArrayView1D<float, Stride1D.Dense>> Float_Vectorkernel;
            private Action<Index1D, ArrayView1D<double, Stride1D.Dense>, ArrayView1D<double, Stride1D.Dense>, ArrayView1D<double, Stride1D.Dense>> Double_Vectorkernel;

            private Action<KernelConfig, ArrayView2D<float, Stride2D.DenseX>, ArrayView2D<float, Stride2D.DenseX>, ArrayView2D<float, Stride2D.DenseX>> Float_MatrixKernel;
            private Action<KernelConfig, ArrayView2D<double, Stride2D.DenseX>, ArrayView2D<double, Stride2D.DenseX>, ArrayView2D<double, Stride2D.DenseX>> double_MatrixKernel;

            private Action<Index1D, ArrayView1D<float, Stride1D.Dense>, ArrayView2D<float, Stride2D.DenseX>, ArrayView1D<float, Stride1D.Dense>> Float_MatrixVectorKernel;
            private Action<Index1D, ArrayView1D<double, Stride1D.Dense>, ArrayView2D<double, Stride2D.DenseX>, ArrayView1D<double, Stride1D.Dense>> double_MatrixVectorKernel;

            public DotProduct(Accelerator accelerator)
            {
                _accelerator = accelerator;
                InitKernel();
            }

            private void InitKernel()
            {
                #region Vector Kernels Init
                Float_Vectorkernel = _accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView1D<float, Stride1D.Dense>,
                    ArrayView1D<float, Stride1D.Dense>,
                    ArrayView1D<float, Stride1D.Dense>>(
                    VectorMultiplyAcceleratedKernel);

                /*Decimal_Vectorkernel = _accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView1D<decimal, Stride1D.Dense>,
                    ArrayView1D<decimal, Stride1D.Dense>,
                    ArrayView1D<decimal, Stride1D.Dense>>(
                    VectorMultiplyAcceleratedKernel);*/
                Double_Vectorkernel = _accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView1D<double, Stride1D.Dense>,
                    ArrayView1D<double, Stride1D.Dense>,
                    ArrayView1D<double, Stride1D.Dense>>(
                    VectorMultiplyAcceleratedKernel);
                #endregion

                #region Matrix Kernels Init

                Float_MatrixKernel = _accelerator.LoadStreamKernel<
                ArrayView2D<float, Stride2D.DenseX>,
                ArrayView2D<float, Stride2D.DenseX>,
                ArrayView2D<float, Stride2D.DenseX>>(
                MatrixMultiplyTiledKernel);

                double_MatrixKernel = _accelerator.LoadStreamKernel<
                  ArrayView2D<double, Stride2D.DenseX>,
                  ArrayView2D<double, Stride2D.DenseX>,
                  ArrayView2D<double, Stride2D.DenseX>>(
                  MatrixMultiplyTiledKernel);
                /*
                decimal_MatrixKernel = _accelerator.LoadStreamKernel<
                  ArrayView2D<decimal, Stride2D.DenseX>,
                  ArrayView2D<decimal, Stride2D.DenseX>,
                  ArrayView2D<decimal, Stride2D.DenseX>>(
                  MatrixMultiplyTiledKernel);*/
                #endregion

                #region Matrix Vector Kernel Init
                Float_MatrixVectorKernel = _accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView1D<float, Stride1D.Dense>,
                    ArrayView2D<float, Stride2D.DenseX>,
                    ArrayView1D<float, Stride1D.Dense>>(
                    VectorMatrixMultiplyAcceleratedKernel);

                double_MatrixVectorKernel = _accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView1D<double, Stride1D.Dense>,
                    ArrayView2D<double, Stride2D.DenseX>,
                    ArrayView1D<double, Stride1D.Dense>>(
                    VectorMatrixMultiplyAcceleratedKernel);
                /*
                decimal_MatrixVectorKernel = _accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView1D<decimal, Stride1D.Dense>,
                    ArrayView2D<decimal, Stride2D.DenseX>,
                    ArrayView1D<decimal, Stride1D.Dense>>(
                    VectorMatrixMultiplyAcceleratedKernel);*/
                #endregion
            }

            #region V*V Float[]*float[] DotProduct

            /// <summary>
            /// Multiplies(Dotproduct) two dense vectors and returns the resultant value.
            /// </summary>
            /// <param name="accelerator">The Accelerator to run the multiplication on</param>
            /// <param name="a">A dense MxK matrix</param>
            /// <param name="b">A dense KxN matrix</param>
            /// <returns>A dense MxN matrix</returns>
            public float Calculate(float[] a, float[] b)
            {
                var m = a.GetLength(0);
                var n = b.GetLength(0);

                if (n != m)
                    throw new ArgumentException($"Cannot multiply {m} by {n} vectors", nameof(b));

                using var resultBuffer = _accelerator.Allocate1D<float>(1);
                using var aBuffer = _accelerator.Allocate1D<float>(new Index1D(m));
                using var bBuffer = _accelerator.Allocate1D<float>(new Index1D(n));

                aBuffer.CopyFromCPU(a);
                bBuffer.CopyFromCPU(b);

                Float_Vectorkernel(resultBuffer.Extent.ToIntIndex(), aBuffer.View, bBuffer.View, resultBuffer.View);

                // Reads data from the GPU buffer into a new CPU array.
                // Implicitly calls accelerator.DefaultStream.Synchronize() to ensure
                // that the kernel and memory copy are completed first.
                return resultBuffer.GetAsArray1D()[0];
            }

            /// <summary>
            /// The Vector multiplication kernel that runs on the accelerated device.
            /// </summary>
            /// <param name="index">Current matrix index</param>
            /// <param name="aView">An input matrix of size MxK</param>
            /// <param name="bView">An input matrix of size KxN</param>
            /// <param name="cView">An output matrix of size MxN</param>
            static void VectorMultiplyAcceleratedKernel(
               Index1D index,
               ArrayView1D<float, Stride1D.Dense> aView,
               ArrayView1D<float, Stride1D.Dense> bView,
               ArrayView1D<float, Stride1D.Dense> cView)
            {

                var sum = 0.0f;

                for (var i = 0; i < aView.IntExtent; i++)
                {
                    sum += aView[new Index1D(i)] * bView[new Index1D(i)];
                }

                cView[0] = sum;
            }

            /// <summary>
            /// Multiplies(Dotproduct) two dense vectors and returns the resultant value.
            /// </summary>
            /// <param name="accelerator">The Accelerator to run the multiplication on</param>
            /// <param name="a">A dense MxK matrix</param>
            /// <param name="b">A dense KxN matrix</param>
            /// <returns>A dense MxN matrix</returns>
            public double Calculate(double[] a, double[] b)
            {
                var m = a.GetLength(0);
                var n = b.GetLength(0);

                if (n != m)
                    throw new ArgumentException($"Cannot multiply {m} by {n} vectors", nameof(b));

                using var resultBuffer = _accelerator.Allocate1D<double>(1);
                using var aBuffer = _accelerator.Allocate1D<double>(new Index1D(m));
                using var bBuffer = _accelerator.Allocate1D<double>(new Index1D(n));

                aBuffer.CopyFromCPU(a);
                bBuffer.CopyFromCPU(b);

                Double_Vectorkernel(resultBuffer.Extent.ToIntIndex(), aBuffer.View, bBuffer.View, resultBuffer.View);

                // Reads data from the GPU buffer into a new CPU array.
                // Implicitly calls accelerator.DefaultStream.Synchronize() to ensure
                // that the kernel and memory copy are completed first.
                return resultBuffer.GetAsArray1D()[0];
            }

            /// <summary>
            /// The Vector multiplication kernel that runs on the accelerated device.
            /// </summary>
            /// <param name="index">Current matrix index</param>
            /// <param name="aView">An input matrix of size MxK</param>
            /// <param name="bView">An input matrix of size KxN</param>
            /// <param name="cView">An output matrix of size MxN</param>
            static void VectorMultiplyAcceleratedKernel(
               Index1D index,
               ArrayView1D<double, Stride1D.Dense> aView,
               ArrayView1D<double, Stride1D.Dense> bView,
               ArrayView1D<double, Stride1D.Dense> cView)
            {
                double sum = 0;

                for (var i = 0; i < aView.IntExtent; i++)
                {
                    sum += aView[new Index1D(i)] * bView[new Index1D(i)];
                }

                cView[0] = sum;
            }

            #endregion

            #region M*M Float[,]*float[,] DotProduct

            /// <summary>
            /// Size of the tile (NxN).
            /// </summary>
            const int TILE_SIZE = 2;

            /// <summary>
            /// Multiplies two dense matrices and returns the resultant matrix (using tiling).
            /// </summary>
            /// <param name="accelerator">The Accelerator to run the multiplication on</param>
            /// <param name="a">A dense MxK matrix</param>
            /// <param name="b">A dense KxN matrix</param>
            /// <returns>A dense MxN matrix</returns>
            public float[,] Calculate(float[,] a, float[,] b)
            {
                var m = a.GetLength(0);
                var ka = a.GetLength(1);
                var kb = b.GetLength(0);
                var n = b.GetLength(1);

                if (ka != kb)
                    throw new ArgumentException($"Cannot multiply {m}x{ka} matrix by {n}x{kb} matrix", nameof(b));


                var groupSize = new Index2D(TILE_SIZE, TILE_SIZE);
                var numGroups = new Index2D((m + TILE_SIZE - 1) / TILE_SIZE, (n + TILE_SIZE - 1) / TILE_SIZE);

                using var aBuffer = _accelerator.Allocate2DDenseX<float>(new Index2D(m, ka));
                using var bBuffer = _accelerator.Allocate2DDenseX<float>(new Index2D(ka, n));
                using var cBuffer = _accelerator.Allocate2DDenseX<float>(new Index2D(m, n));
                aBuffer.CopyFromCPU(a);
                bBuffer.CopyFromCPU(b);

                Float_MatrixKernel((numGroups, groupSize), aBuffer, bBuffer, cBuffer);

                // Reads data from the GPU buffer into a new CPU array.
                // Implicitly calls accelerator.DefaultStream.Synchronize() to ensure
                // that the kernel and memory copy are completed first.
                return cBuffer.GetAsArray2D();
            }

            /// <summary>
            /// The tiled matrix multiplication kernel that runs on the accelerated device.
            /// </summary>
            /// <param name="aView">An input matrix of size MxK</param>
            /// <param name="bView">An input matrix of size KxN</param>
            /// <param name="cView">An output matrix of size MxN</param>
            static void MatrixMultiplyTiledKernel(ArrayView2D<float, Stride2D.DenseX> aView, ArrayView2D<float, Stride2D.DenseX> bView, ArrayView2D<float, Stride2D.DenseX> cView)
            {
                var global = Grid.GlobalIndex.XY;
                var x = Group.IdxX;
                var y = Group.IdxY;

                var aTile = SharedMemory.Allocate2D<float, Stride2D.DenseX>(new Index2D(TILE_SIZE, TILE_SIZE), new Stride2D.DenseX(TILE_SIZE));
                var bTile = SharedMemory.Allocate2D<float, Stride2D.DenseX>(new Index2D(TILE_SIZE, TILE_SIZE), new Stride2D.DenseX(TILE_SIZE));
                var sum = 0.0f;

                for (var i = 0; i < aView.IntExtent.X; i += TILE_SIZE)
                {
                    if (global.X < aView.IntExtent.X && y + i < aView.IntExtent.Y)
                        aTile[x, y] = aView[global.X, y + i];
                    else
                        aTile[x, y] = 0;

                    if (x + i < bView.IntExtent.X && global.Y < bView.IntExtent.Y)
                        bTile[x, y] = bView[x + i, global.Y];
                    else
                        bTile[x, y] = 0;
                    Group.Barrier();

                    for (var k = 0; k < TILE_SIZE; k++)
                        sum += aTile[new Index2D(x, k)] * bTile[new Index2D(k, y)];
                    Group.Barrier();
                }

                if (global.X < cView.IntExtent.X && global.Y < cView.IntExtent.Y)
                    cView[global] = sum;
            }


            /// <summary>
            /// Multiplies two dense matrices and returns the resultant matrix (using tiling).
            /// </summary>
            /// <param name="accelerator">The Accelerator to run the multiplication on</param>
            /// <param name="a">A dense MxK matrix</param>
            /// <param name="b">A dense KxN matrix</param>
            /// <returns>A dense MxN matrix</returns>
            public double[,] Calculate(double[,] a, double[,] b)
            {
                var m = a.GetLength(0);
                var ka = a.GetLength(1);
                var kb = b.GetLength(0);
                var n = b.GetLength(1);

                if (ka != kb)
                    throw new ArgumentException($"Cannot multiply {m}x{ka} matrix by {n}x{kb} matrix", nameof(b));


                var groupSize = new Index2D(TILE_SIZE, TILE_SIZE);
                var numGroups = new Index2D((m + TILE_SIZE - 1) / TILE_SIZE, (n + TILE_SIZE - 1) / TILE_SIZE);

                using var aBuffer = _accelerator.Allocate2DDenseX<double>(new Index2D(m, ka));
                using var bBuffer = _accelerator.Allocate2DDenseX<double>(new Index2D(ka, n));
                using var cBuffer = _accelerator.Allocate2DDenseX<double>(new Index2D(m, n));
                aBuffer.CopyFromCPU(a);
                bBuffer.CopyFromCPU(b);

                double_MatrixKernel((numGroups, groupSize), aBuffer, bBuffer, cBuffer);

                // Reads data from the GPU buffer into a new CPU array.
                // Implicitly calls accelerator.DefaultStream.Synchronize() to ensure
                // that the kernel and memory copy are completed first.
                return cBuffer.GetAsArray2D();
            }

            /// <summary>
            /// The tiled matrix multiplication kernel that runs on the accelerated device.
            /// </summary>
            /// <param name="aView">An input matrix of size MxK</param>
            /// <param name="bView">An input matrix of size KxN</param>
            /// <param name="cView">An output matrix of size MxN</param>
            static void MatrixMultiplyTiledKernel(ArrayView2D<double, Stride2D.DenseX> aView, ArrayView2D<double, Stride2D.DenseX> bView, ArrayView2D<double, Stride2D.DenseX> cView)
            {
                var global = Grid.GlobalIndex.XY;
                var x = Group.IdxX;
                var y = Group.IdxY;

                var aTile = SharedMemory.Allocate2D<double, Stride2D.DenseX>(new Index2D(TILE_SIZE, TILE_SIZE), new Stride2D.DenseX(TILE_SIZE));
                var bTile = SharedMemory.Allocate2D<double, Stride2D.DenseX>(new Index2D(TILE_SIZE, TILE_SIZE), new Stride2D.DenseX(TILE_SIZE));
                var sum = 0.0D;

                for (var i = 0; i < aView.IntExtent.X; i += TILE_SIZE)
                {
                    if (global.X < aView.IntExtent.X && y + i < aView.IntExtent.Y)
                        aTile[x, y] = aView[global.X, y + i];
                    else
                        aTile[x, y] = 0;

                    if (x + i < bView.IntExtent.X && global.Y < bView.IntExtent.Y)
                        bTile[x, y] = bView[x + i, global.Y];
                    else
                        bTile[x, y] = 0;
                    Group.Barrier();

                    for (var k = 0; k < TILE_SIZE; k++)
                        sum += aTile[new Index2D(x, k)] * bTile[new Index2D(k, y)];
                    Group.Barrier();
                }

                if (global.X < cView.IntExtent.X && global.Y < cView.IntExtent.Y)
                    cView[global] = sum;
            }

            #endregion

            #region M*V Float[,]*float[] DotProduct
            public float[] Calculate(float[,] matrix, float[] vector)
            {
                var vectorLength = vector.Length;
                var matrixRows = matrix.GetLength(0);
                var matrixCols = matrix.GetLength(1);

                if (vectorLength != matrixRows)
                    throw new ArgumentException($"Vector length {vectorLength} does not match matrix rows {matrixRows}");

                using var resultBuffer = _accelerator.Allocate1D<float>(matrixCols);
                using var vectorBuffer = _accelerator.Allocate1D<float>(vectorLength);
                using var matrixBuffer = _accelerator.Allocate2DDenseX<float>(new Index2D(matrixRows, matrixCols));

                vectorBuffer.CopyFromCPU(vector);
                matrixBuffer.CopyFromCPU(matrix);

                Float_MatrixVectorKernel(resultBuffer.Extent.ToIntIndex(), vectorBuffer.View, matrixBuffer.View, resultBuffer.View);

                return resultBuffer.GetAsArray1D();
            }

            static void VectorMatrixMultiplyAcceleratedKernel(
               Index1D index,
               ArrayView1D<float, Stride1D.Dense> vectorView,
               ArrayView2D<float, Stride2D.DenseX> matrixView,
               ArrayView1D<float, Stride1D.Dense> resultView)
            {
                var sum = 0.0f;
                for (var i = 0; i < vectorView.IntExtent; i++)
                {
                    sum += vectorView[i] * matrixView[i, index];
                }
                resultView[index] = sum;
            }



            public double[] Calculate(double[,] matrix, double[] vector)
            {
                var vectorLength = vector.Length;
                var matrixRows = matrix.GetLength(0);
                var matrixCols = matrix.GetLength(1);

                if (vectorLength != matrixRows)
                    throw new ArgumentException($"Vector length {vectorLength} does not match matrix rows {matrixRows}");



                using var resultBuffer = _accelerator.Allocate1D<double>(matrixCols);
                using var vectorBuffer = _accelerator.Allocate1D<double>(vectorLength);
                using var matrixBuffer = _accelerator.Allocate2DDenseX<double>(new Index2D(matrixRows, matrixCols));

                vectorBuffer.CopyFromCPU(vector);
                matrixBuffer.CopyFromCPU(matrix);

                double_MatrixVectorKernel(resultBuffer.Extent.ToIntIndex(), vectorBuffer.View, matrixBuffer.View, resultBuffer.View);

                return resultBuffer.GetAsArray1D();
            }


            static void VectorMatrixMultiplyAcceleratedKernel(
               Index1D index,
               ArrayView1D<double, Stride1D.Dense> vectorView,
               ArrayView2D<double, Stride2D.DenseX> matrixView,
               ArrayView1D<double, Stride1D.Dense> resultView)
            {
                double sum = 0.0d;
                for (var i = 0; i < vectorView.IntExtent; i++)
                {
                    sum += vectorView[i] * matrixView[i, index];
                }
                resultView[index] = sum;
            }
            #endregion

            public void Dispose()
            {
                _accelerator.Dispose();
            }

        }

        public class NeuralNetworkMisc : IDisposable
        {
            private Accelerator _accelerator;
            private Action<Index1D, ArrayView1D<double, Stride1D.Dense>, ArrayView1D<double, Stride1D.Dense>, double, double> double_WeightChangesKernel;

            public NeuralNetworkMisc(Accelerator accelerator)
            {
                _accelerator = accelerator;
                InitKernel();
            }

            private void InitKernel()
            {
                // Load the kernel with correct parameter types
               /* double_WeightChangesKernel = _accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView1D<double, Stride1D.Dense>,
                    ArrayView1D<double, Stride1D.Dense>,
                    double,
                    double>(ComputeWeightChangesKernel);*/
            }


            void LaunchBackpropagationKernel(
    Accelerator accelerator,
    ArrayView2D<double, Stride2D.DenseX> weightsMatrixView,
    ArrayView1D<double, Stride1D.Dense> deltasView,
    ArrayView1D<double, Stride1D.Dense> activationsView,
    ArrayView1D<double, Stride1D.Dense> activationDerivativeView,
    ArrayView1D<double, Stride1D.Dense> biasesView,
    double learningRate,
    double clippingLowerLimit,
    double clippingUpperLimit)
            {
                // Load the kernel with all necessary parameters
                var backpropagationKernel = accelerator.LoadAutoGroupedStreamKernel<
                    Index1D,
                    ArrayView2D<double, Stride2D.DenseX>,
                    ArrayView1D<double, Stride1D.Dense>,
                    ArrayView1D<double, Stride1D.Dense>,
                    ArrayView1D<double, Stride1D.Dense>,
                    ArrayView1D<double, Stride1D.Dense>,
                    double,
                    double,
                    double>(BackpropagationKernel);

                // Launch the kernel across all neurons
                int numberOfNeurons = weightsMatrixView.IntExtent.X;
                backpropagationKernel(numberOfNeurons, weightsMatrixView, deltasView, activationsView, activationDerivativeView, biasesView, learningRate, clippingLowerLimit, clippingUpperLimit);
            }
            static void BackpropagationKernel(
                Index1D neuronIndex,
                ArrayView2D<double, Stride2D.DenseX> weightsMatrix,
                ArrayView1D<double, Stride1D.Dense> deltasView,
                ArrayView1D<double, Stride1D.Dense> activationsView,
                ArrayView1D<double, Stride1D.Dense> activationDerivativeView,
                ArrayView1D<double, Stride1D.Dense> biasesView,
                double learningRate,
                double clippingLowerLimit,
                double clippingUpperLimit)
            {
                // Sum up weighted deltas
                double sumOfWeightedDeltas = 0.0;
                for (int i = 0; i < weightsMatrix.IntExtent.Y; ++i)
                {
                    sumOfWeightedDeltas += weightsMatrix[neuronIndex, i] * deltasView[i];
                }

                // Apply gradient clipping
                if (sumOfWeightedDeltas < clippingLowerLimit)
                {
                    sumOfWeightedDeltas = clippingLowerLimit;
                }
                else if (sumOfWeightedDeltas > clippingUpperLimit)
                {
                    sumOfWeightedDeltas = clippingUpperLimit;
                }

                // Derive activation function gradient
                double activationDerivative = activationDerivativeView[neuronIndex];

                // Update delta
                double delta = sumOfWeightedDeltas * activationDerivative;

                // Compute weight changes and update weights
                for (int k = 0; k < weightsMatrix.IntExtent.Y; ++k)
                {
                    double weightChange = learningRate * delta * activationsView[k];
                    weightsMatrix[neuronIndex, k] -= weightChange;
                }

                // Update bias
                biasesView[neuronIndex] -= learningRate * delta;
            }

            public void Dispose()
            {
                _accelerator.Dispose();
            }
        }

    }

}
