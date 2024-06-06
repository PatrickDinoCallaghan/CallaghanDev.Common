using System.Text;

namespace CallaghanDev.Utilities.MathTools
{
    public static class ArrayTensorExtensions
    {
        public static string ToVisualString<T>(this T[] array)
        {
            if (array == null || array.Length == 0) return "[]";

            StringBuilder sb = new StringBuilder();
            sb.Append("[ ");
            foreach (var item in array)
            {
                sb.Append(item.ToString() + ", ");
            }
            sb.Remove(sb.Length - 2, 2); // Remove the last comma and space
            sb.Append(" ]");
            return sb.ToString();
        }
        public static bool AreEquall(this float[] array1, float[] array2)
        {
            if (array1 == null || array2 == null) return array1 == array2;
            if (array1.Length != array2.Length) return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (!array1[i].Equals(array2[i]))
                    return false;
            }

            return true;
        }
        public static bool CompareFloatArrays(this float[] array1, float[] array2)
        {
            if (array1 == null || array2 == null) return array1 == array2;
            if (array1.Length != array2.Length) return false;

            var sortedArray1 = array1.OrderBy(x => x).ToArray();
            var sortedArray2 = array2.OrderBy(x => x).ToArray();

            for (int i = 0; i < sortedArray1.Length; i++)
            {
                if (!sortedArray1[i].Equals(sortedArray2[i]))
                    return false;
            }

            return true;
        }
        public static string MatrixToString(this float[,] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            int rows = array.GetLength(0);
            int columns = array.GetLength(1);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    // Append each float to the StringBuilder, formatted as desired
                    sb.Append(array[i, j].ToString("0.##")); // Format for 2 decimal places, adjust as needed
                    if (j < columns - 1)
                        sb.Append(", "); // Separate elements within the same row
                }
                if (i < rows - 1)
                    sb.AppendLine(); // Start a new line for each row except the last one
            }

            return sb.ToString();
        }
        public static string VectorToString(this float[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            StringBuilder sb = new StringBuilder();

            sb.Append("["); // Start of the vector representation

            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(array[i].ToString("0.##")); // Convert float to string with 2 decimal places
                if (i < array.Length - 1)
                    sb.Append(", "); // Separate elements with a comma
            }

            sb.Append("]"); // End of the vector representation

            return sb.ToString();
        }


        // Extension method for calculating the dot product of two arrays
        public static float DotProduct(this float[] array1, float[] array2)
        {
            if (array1.Length != array2.Length)
                throw new ArgumentException("Arrays must be of the same length.");

            return array1.Zip(array2, (a, b) => a * b).Sum();
        }
        public static double DotProduct(this double[] array1, double[] array2)
        {
            if (array1.Length != array2.Length)
                throw new ArgumentException("Arrays must be of the same length.");

            return array1.Zip(array2, (a, b) => a * b).Sum();
        }
        public static decimal DotProduct(this decimal[] array1, decimal[] array2)
        {
            if (array1.Length != array2.Length)
                throw new ArgumentException("Arrays must be of the same length.");

            return array1.Zip(array2, (a, b) => a * b).Sum();
        }
        public static T[][] ToJaggedMatrix<T>(this T[,] arrayMatrix)
        {
            int rows = arrayMatrix.GetLength(0);
            int cols = arrayMatrix.GetLength(1);

            T[][] jaggedArray = new T[rows][];

            //In this case, each thread writes to a different part of the jaggedArray, so there are no thread safety issues regarding shared data modification.
            Parallel.For(0, rows, i =>
            {
                jaggedArray[i] = new T[cols];
                for (int j = 0; j < cols; j++)
                {
                    jaggedArray[i][j] = arrayMatrix[i, j];
                }
            });

            return jaggedArray;
        }


        public static double[][] Transpose(this double[][] inputArray)
        {
            if (inputArray == null || inputArray.Length == 0)
                throw new ArgumentException("Input array cannot be null or empty.");

            // Determine the length of the longest sub-array to handle non-uniform jagged arrays
            int maxLength = inputArray.Max(subArray => subArray.Length);

            // Initialize the transposed array with maxLength rows and inputArray.Length columns
            double[][] transposedArray = new double[maxLength][];
            for (int i = 0; i < maxLength; i++)
            {
                transposedArray[i] = new double[inputArray.Length];
            }

            // Transpose the array
            for (int i = 0; i < inputArray.Length; i++)
            {
                for (int j = 0; j < inputArray[i].Length; j++)
                {
                    transposedArray[j][i] = inputArray[i][j];
                }
            }

            return transposedArray;
        }
    }


    /*
    public static class ArrayTensorExtensions
    {
        public static string ToVisualString<T>(this T[] array)
        {
            if (array == null || array.Length == 0) return "[]";

            StringBuilder sb = new StringBuilder();
            sb.Append("[ ");
            foreach (var item in array)
            {
                sb.Append(item.ToString() + ", ");
            }
            sb.Remove(sb.Length - 2, 2); // Remove the last comma and space
            sb.Append(" ]");
            return sb.ToString();
        }
        public static bool AreEquall(this float[] array1, float[] array2)
        {
            if (array1 == null || array2 == null) return array1 == array2;
            if (array1.Length != array2.Length) return false;

            for (int i = 0; i < array1.Length; i++)
            {
                if (!array1[i].Equals(array2[i]))
                    return false;
            }

            return true;
        }
        public static bool CompareFloatArrays(this float[] array1, float[] array2)
        {
            if (array1 == null || array2 == null) return array1 == array2;
            if (array1.Length != array2.Length) return false;

            var sortedArray1 = array1.OrderBy(x => x).ToArray();
            var sortedArray2 = array2.OrderBy(x => x).ToArray();

            for (int i = 0; i < sortedArray1.Length; i++)
            {
                if (!sortedArray1[i].Equals(sortedArray2[i]))
                    return false;
            }

            return true;
        }
        public static string MatrixToString(this float[,] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            int rows = array.GetLength(0);
            int columns = array.GetLength(1);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    // Append each float to the StringBuilder, formatted as desired
                    sb.Append(array[i, j].ToString("0.##")); // Format for 2 decimal places, adjust as needed
                    if (j < columns - 1)
                        sb.Append(", "); // Separate elements within the same row
                }
                if (i < rows - 1)
                    sb.AppendLine(); // Start a new line for each row except the last one
            }

            return sb.ToString();
        }
        public static string VectorToString(this float[] array)
        {
            if (array == null)
                throw new ArgumentNullException(nameof(array));

            StringBuilder sb = new StringBuilder();

            sb.Append("["); // Start of the vector representation

            for (int i = 0; i < array.Length; i++)
            {
                sb.Append(array[i].ToString("0.##")); // Convert float to string with 2 decimal places
                if (i < array.Length - 1)
                    sb.Append(", "); // Separate elements with a comma
            }

            sb.Append("]"); // End of the vector representation

            return sb.ToString();
        }

        #region V*V Float[]*float[] DotProduct
        // Extension method for calculating the dot product of two arrays
        public static float DotProduct(this float[] array1, float[] array2)
        {
            if (array1.Length != array2.Length)
                throw new ArgumentException("Arrays must be of the same length.");

            return array1.Zip(array2, (a, b) => a * b).Sum();
        }

        public static double DotProduct(this double[] array1, double[] array2)
        {
            if (array1.Length != array2.Length)
                throw new ArgumentException("Arrays must be of the same length.");

            return array1.Zip(array2, (a, b) => a * b).Sum();
        }
        public static decimal DotProduct(this decimal[] array1, decimal[] array2)
        {
            if (array1.Length != array2.Length)
                throw new ArgumentException("Arrays must be of the same length.");

            return array1.Zip(array2, (a, b) => a * b).Sum();
        }
        /// <summary>
        /// Multiplies(Dotproduct) two dense vectors and returns the resultant value.
        /// </summary>
        /// <param name="accelerator">The Accelerator to run the multiplication on</param>
        /// <param name="a">A dense MxK matrix</param>
        /// <param name="b">A dense KxN matrix</param>
        /// <returns>A dense MxN matrix</returns>
        public static float DotProduct(this float[] a, float[] b, Accelerator accelerator)
        {
            var m = a.GetLength(0);
            var n = b.GetLength(0);

            if (n != m)
                throw new ArgumentException($"Cannot multiply {m} by {n} vectors", nameof(b));

            var kernel = accelerator.LoadAutoGroupedStreamKernel<
                Index1D,
                ArrayView1D<float, Stride1D.Dense>,
                ArrayView1D<float, Stride1D.Dense>,
                ArrayView1D<float, Stride1D.Dense>>(
                VectorMultiplyAcceleratedKernel);

            using var resultBuffer = accelerator.Allocate1D<float>(1);
            using var aBuffer = accelerator.Allocate1D<float>(new Index1D(m));
            using var bBuffer = accelerator.Allocate1D<float>(new Index1D(n));

            aBuffer.CopyFromCPU(a);
            bBuffer.CopyFromCPU(b);

            kernel(resultBuffer.Extent.ToIntIndex(), aBuffer.View, bBuffer.View, resultBuffer.View);

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
        public static decimal DotProduct(this decimal[] a, decimal[] b, Accelerator accelerator)
        {
            var m = a.GetLength(0);
            var n = b.GetLength(0);

            if (n != m)
                throw new ArgumentException($"Cannot multiply {m} by {n} vectors", nameof(b));

            var kernel = accelerator.LoadAutoGroupedStreamKernel<
                Index1D,
                ArrayView1D<decimal, Stride1D.Dense>,
                ArrayView1D<decimal, Stride1D.Dense>,
                ArrayView1D<decimal, Stride1D.Dense>>(
                VectorMultiplyAcceleratedKernel);

            using var resultBuffer = accelerator.Allocate1D<decimal>(1);
            using var aBuffer = accelerator.Allocate1D<decimal>(new Index1D(m));
            using var bBuffer = accelerator.Allocate1D<decimal>(new Index1D(n));

            aBuffer.CopyFromCPU(a);
            bBuffer.CopyFromCPU(b);

            kernel(resultBuffer.Extent.ToIntIndex(), aBuffer.View, bBuffer.View, resultBuffer.View);

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
            ArrayView1D<decimal, Stride1D.Dense> aView,
            ArrayView1D<decimal, Stride1D.Dense> bView,
            ArrayView1D<decimal, Stride1D.Dense> cView)
        {

            var sum = 0.0m;

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
        public static double DotProduct(this double[] a, double[] b, Accelerator accelerator)
        {
            var m = a.GetLength(0);
            var n = b.GetLength(0);

            if (n != m)
                throw new ArgumentException($"Cannot multiply {m} by {n} vectors", nameof(b));

            var kernel = accelerator.LoadAutoGroupedStreamKernel<
                Index1D,
                ArrayView1D<double, Stride1D.Dense>,
                ArrayView1D<double, Stride1D.Dense>,
                ArrayView1D<double, Stride1D.Dense>>(
                VectorMultiplyAcceleratedKernel);

            using var resultBuffer = accelerator.Allocate1D<double>(1);
            using var aBuffer = accelerator.Allocate1D<double>(new Index1D(m));
            using var bBuffer = accelerator.Allocate1D<double>(new Index1D(n));

            aBuffer.CopyFromCPU(a);
            bBuffer.CopyFromCPU(b);

            kernel(resultBuffer.Extent.ToIntIndex(), aBuffer.View, bBuffer.View, resultBuffer.View);

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
        public static float[,] DotProduct(this float[,] a, float[,] b, Accelerator accelerator)
        {
            var m = a.GetLength(0);
            var ka = a.GetLength(1);
            var kb = b.GetLength(0);
            var n = b.GetLength(1);

            if (ka != kb)
                throw new ArgumentException($"Cannot multiply {m}x{ka} matrix by {n}x{kb} matrix", nameof(b));

            var kernel = accelerator.LoadStreamKernel<
                ArrayView2D<float, Stride2D.DenseX>,
                ArrayView2D<float, Stride2D.DenseX>,
                ArrayView2D<float, Stride2D.DenseX>>(
                MatrixMultiplyTiledKernel);

            var groupSize = new Index2D(TILE_SIZE, TILE_SIZE);
            var numGroups = new Index2D((m + TILE_SIZE - 1) / TILE_SIZE, (n + TILE_SIZE - 1) / TILE_SIZE);

            using var aBuffer = accelerator.Allocate2DDenseX<float>(new Index2D(m, ka));
            using var bBuffer = accelerator.Allocate2DDenseX<float>(new Index2D(ka, n));
            using var cBuffer = accelerator.Allocate2DDenseX<float>(new Index2D(m, n));
            aBuffer.CopyFromCPU(a);
            bBuffer.CopyFromCPU(b);

            kernel((numGroups, groupSize), aBuffer, bBuffer, cBuffer);

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
        public static decimal[,] DotProduct(this decimal[,] a, decimal[,] b, Accelerator accelerator)
        {
            var m = a.GetLength(0);
            var ka = a.GetLength(1);
            var kb = b.GetLength(0);
            var n = b.GetLength(1);

            if (ka != kb)
                throw new ArgumentException($"Cannot multiply {m}x{ka} matrix by {n}x{kb} matrix", nameof(b));

            var kernel = accelerator.LoadStreamKernel<
                ArrayView2D<decimal, Stride2D.DenseX>,
                ArrayView2D<decimal, Stride2D.DenseX>,
                ArrayView2D<decimal, Stride2D.DenseX>>(
                MatrixMultiplyTiledKernel);
            var groupSize = new Index2D(TILE_SIZE, TILE_SIZE);
            var numGroups = new Index2D((m + TILE_SIZE - 1) / TILE_SIZE, (n + TILE_SIZE - 1) / TILE_SIZE);

            using var aBuffer = accelerator.Allocate2DDenseX<decimal>(new Index2D(m, ka));
            using var bBuffer = accelerator.Allocate2DDenseX<decimal>(new Index2D(ka, n));
            using var cBuffer = accelerator.Allocate2DDenseX<decimal>(new Index2D(m, n));
            aBuffer.CopyFromCPU(a);
            bBuffer.CopyFromCPU(b);

            kernel((numGroups, groupSize), aBuffer, bBuffer, cBuffer);

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
        static void MatrixMultiplyTiledKernel(ArrayView2D<decimal, Stride2D.DenseX> aView, ArrayView2D<decimal, Stride2D.DenseX> bView, ArrayView2D<decimal, Stride2D.DenseX> cView)
        {
            var global = Grid.GlobalIndex.XY;
            var x = Group.IdxX;
            var y = Group.IdxY;

            var aTile = SharedMemory.Allocate2D<decimal, Stride2D.DenseX>(new Index2D(TILE_SIZE, TILE_SIZE), new Stride2D.DenseX(TILE_SIZE));
            var bTile = SharedMemory.Allocate2D<decimal, Stride2D.DenseX>(new Index2D(TILE_SIZE, TILE_SIZE), new Stride2D.DenseX(TILE_SIZE));
            var sum = 0.0M;

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
        public static float[] DotProduct(this float[,] matrix, float[] vector, Accelerator accelerator)
        {
            var vectorLength = vector.Length;
            var matrixRows = matrix.GetLength(0);
            var matrixCols = matrix.GetLength(1);

            if (vectorLength != matrixRows)
                throw new ArgumentException($"Vector length {vectorLength} does not match matrix rows {matrixRows}");

            var kernel = accelerator.LoadAutoGroupedStreamKernel<
                Index1D,
                ArrayView1D<float, Stride1D.Dense>,
                ArrayView2D<float, Stride2D.DenseX>,
                ArrayView1D<float, Stride1D.Dense>>(
                VectorMatrixMultiplyAcceleratedKernel);

            using var resultBuffer = accelerator.Allocate1D<float>(matrixCols);
            using var vectorBuffer = accelerator.Allocate1D<float>(vectorLength);
            using var matrixBuffer = accelerator.Allocate2DDenseX<float>(new Index2D(matrixRows, matrixCols));

            vectorBuffer.CopyFromCPU(vector);
            matrixBuffer.CopyFromCPU(matrix);

            kernel(resultBuffer.Extent.ToIntIndex(), vectorBuffer.View, matrixBuffer.View, resultBuffer.View);

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



        public static double[] DotProduct(this double[,] matrix, double[] vector, Accelerator accelerator)
        {
            var vectorLength = vector.Length;
            var matrixRows = matrix.GetLength(0);
            var matrixCols = matrix.GetLength(1);

            if (vectorLength != matrixRows)
                throw new ArgumentException($"Vector length {vectorLength} does not match matrix rows {matrixRows}");

            var kernel = accelerator.LoadAutoGroupedStreamKernel<
                Index1D,
                ArrayView1D<double, Stride1D.Dense>,
                ArrayView2D<double, Stride2D.DenseX>,
                ArrayView1D<double, Stride1D.Dense>>(
                VectorMatrixMultiplyAcceleratedKernel);

            using var resultBuffer = accelerator.Allocate1D<double>(matrixCols);
            using var vectorBuffer = accelerator.Allocate1D<double>(vectorLength);
            using var matrixBuffer = accelerator.Allocate2DDenseX<double>(new Index2D(matrixRows, matrixCols));

            vectorBuffer.CopyFromCPU(vector);
            matrixBuffer.CopyFromCPU(matrix);

            kernel(resultBuffer.Extent.ToIntIndex(), vectorBuffer.View, matrixBuffer.View, resultBuffer.View);

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
    }
    */


}