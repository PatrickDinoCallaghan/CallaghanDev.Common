using CallaghanDev.Utilities.Extensions;
using CallaghanDev.XML.Excel;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Algorithms;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;
using Group = ILGPU.Group;

namespace CallaghanDev.Utilities.MathTools
{

    public static class MatrixExtensions
    {
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
        public static float[,] DotProduct(this float[,] a,float[,] b, Accelerator accelerator)
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
        private static void MatrixMultiplyTiledKernel(ArrayView2D<float, Stride2D.DenseX> aView, ArrayView2D<float, Stride2D.DenseX> bView, ArrayView2D<float, Stride2D.DenseX> cView)
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
    }
    // TODO: Create thread safe collection data structures that allow indexing of ulong index length Namely: ulongList<T> and ulongConcurrentDictionary<(ulong, ulong), T>
    public class Matrix<T> : IMatrix<T>
    {
        ParallelOptions options;
        ConcurrentDictionary<(ulong, ulong), T> Data;

        private int _MaxDegreeOfParallelism;
        public int MaxDegreeOfParallelism
        {
            get { return _MaxDegreeOfParallelism; }
            set
            {
                _MaxDegreeOfParallelism = value;

                options = new ParallelOptions
                {
                    MaxDegreeOfParallelism = this.MaxDegreeOfParallelism
                };
            }
        }

        public Matrix()
        {
            Data = new ConcurrentDictionary<(ulong, ulong), T>();
            MaxDegreeOfParallelism = Environment.ProcessorCount;
        }

        public T this[ulong Row, ulong Column]
        {
            get
            {
                return GetElement(Row, Column);
            }
            set
            {
                SetElement(Row, Column, value);
            }
        }

        public T[] Column(ulong Index)
        {
            return Data.Where(kvp => kvp.Key.Item2 == Index).Select(kvp => kvp.Value).Where(ele => ele != null).ToArray();
        }
        public T[] Row(ulong Index)
        {
            return Data.Where(kvp => kvp.Key.Item1 == Index).Select(kvp => kvp.Value).Where(ele => ele != null).ToArray();
        }

        #region Private Helpers
        private T GetElement(ulong row, ulong col)
        {
            return Data.TryGetValue((row, col), out T value) ? value : default(T);
        }
        private void SetElement(ulong row, ulong col, T value)
        {
            if (Data.ContainsKey((row, col)))
            {
                Data[(row, col)] = value;
            }
            else
            {
                Data.TryAdd((row, col), value);
            }
        }
        public (ulong Rows, ulong Columns) GetSize()
        {
            if (Data.Count == 0)
            {
                return (0, 0);
            }

            ulong maxRow = Data.Keys.Max(k => k.Item1);
            ulong maxCol = Data.Keys.Max(k => k.Item2);

            // Size is max index + 1 as matrix indices are 0-based
            return (maxRow + 1, maxCol + 1);
        }
        #endregion

        #region Matrix Size and Structure
        public ulong RowCount()
        {

            if (Data.Count == 0)
            {
                return 0;
            }
            ulong maxRow = Data.Keys.Max(k => k.Item1);

            // Size is max index + 1 as matrix indices are 0-based
            return maxRow + 1;
        }
        public ulong ColumnCount()
        {

            if (Data.Count == 0)
            {
                return 0;
            }

            ulong maxCol = Data.Keys.Max(k => k.Item2);

            // Size is max index + 1 as matrix indices are 0-based
            return maxCol + 1;
        }
        public bool IsSquare()
        {
            return (this.ColumnCount() == this.RowCount());
        }
        public bool IsOrthogonal()
        {
            return (this.IsSquare() && this * this.Transpose() == Identity(this.RowCount()));
        }
        public bool IsDiagonal()
        {
            Matrix<T> Zeros = new Matrix<T>();

            for (ulong r = 0; r < this.RowCount(); r++)
            {
                for (ulong c = 0; c < this.ColumnCount(); c++)
                {
                    Zeros[r, c] = (dynamic)0;
                }
            }

            Matrix<T> DiagVector = this.DiagVector();
            Matrix<T> DiagOfMatrix = Diag(DiagVector);

            return (this.Clone() - DiagOfMatrix == Zeros);
        }
        public bool IsUnitary()
        {
            if (!this.IsSquare())
                return false;

            return (this.Transpose() * this == Identity(RowCount()));
        }
        #endregion

        #region Matrix Operations

        #region DotProduct
        public Matrix<T> DotProduct(Matrix<T> other)
        {
            var (rowsA, colsA) = this.GetSize();
            var (rowsB, colsB) = other.GetSize();

            if (colsA != rowsB)
            {
                throw new InvalidOperationException("Matrix dimensions are not compatible for multiplication.");
            }

            Matrix<T> result = new Matrix<T>();

            ParallelExtensions.For(0, rowsA, options, i =>
            {
                for (uint j = 0; j < colsB; j++)
                {
                    dynamic sum = default(T);
                    for (uint k = 0; k < colsA; k++)
                    {
                        dynamic valA = this.GetElement(i, k);
                        dynamic valB = other.GetElement(k, j);
                        sum += valA * valB;
                    }
                    result.SetElement(i, j, sum);
                }
            });

            return result;
        }
        public Matrix<float> DotProduct(Matrix<float> other, Accelerator accelerator)
        {
            if (typeof(T) != typeof(float))
            {
                throw new InvalidOperationException("ILGPU accelerated DotProduct is only supported for float type.");
            }

            var thisMatrix = To2DFloatArray();

            float[,] otherMatrix = other.To2DFloatArray();

            float[,] values = thisMatrix.DotProduct(otherMatrix, accelerator);

            return new Matrix<float>() { Data = ToConcurrentDictionary(values) };
        }
        private float[,] To2DFloatArray()
        {
            if (Data == null || Data.Count == 0)
            {
                return new float[0, 0];
            }

            ulong maxRow = Data.Keys.Max(k => k.Item1) + 1;
            ulong maxColumn = Data.Keys.Max(k => k.Item2) + 1;

            float[,] array = new float[maxRow, maxColumn];


            ParallelExtensions.For(0, maxRow, options, row =>
            {
                for (ulong column = 0; column < maxColumn; column++)
                {
                    if (Data.TryGetValue((row, column), out T value))
                    {
                        array[row, column] = (float)Convert.ChangeType(value, typeof(float)); ;
                    }
                    else
                    {
                        array[row, column] = 0;
                    }
                }
            });


            return array;
        }
        private ConcurrentDictionary<(ulong, ulong), T> ToConcurrentDictionary<T>(T[,] array)
        {
            var dictionary = new ConcurrentDictionary<(ulong, ulong), T>();

            ulong rows = (ulong)array.GetLength(0);
            ulong columns = (ulong)array.GetLength(1);

            ParallelExtensions.For(0, rows, options, row =>
            {
                for (ulong column = 0; column < columns; column++)
                {
                    T value = array[row, column];
                    if (!EqualityComparer<T>.Default.Equals(value, default(T))) // Optional: only add if not default
                    {
                        dictionary.TryAdd((row, column), value);
                    }
                }
            });

            return dictionary;
        }

        #endregion

        public Matrix<T> Add(Matrix<T> other)
        {
            var (rowsThis, colsThis) = this.GetSize();
            var (rowsOther, colsOther) = other.GetSize();

            if (rowsThis != rowsOther || colsThis != colsOther)
            {
                throw new InvalidOperationException("Matrices dimensions do not match for addition.");
            }

            Matrix<T> result = new Matrix<T>();

            ParallelExtensions.For(0, rowsThis, options, i =>
            {
                for (uint j = 0; j < colsThis; j++)
                {
                    dynamic valueThis = this.GetElement(i, j);
                    dynamic valueOther = other.GetElement(i, j);
                    result.SetElement(i, j, valueThis + valueOther);
                }
            });

            return result;
        }
        public Matrix<T> Subtract(Matrix<T> other)
        {
            var (rowsThis, colsThis) = this.GetSize();
            var (rowsOther, colsOther) = other.GetSize();

            if (rowsThis != rowsOther || colsThis != colsOther)
            {
                throw new InvalidOperationException("Matrices dimensions do not match for subtraction.");
            }

            Matrix<T> result = new Matrix<T>();

            ParallelExtensions.For(0, rowsThis, options, i =>
            {
                for (uint j = 0; j < colsThis; j++)
                {
                    dynamic valueThis = this.GetElement(i, j);
                    dynamic valueOther = other.GetElement(i, j);
                    result.SetElement(i, j, valueThis - valueOther);
                }
            });

            return result;
        }
        public Matrix<T> MultiplyScalar(T scalar)
        {
            var (rows, cols) = this.GetSize();
            Matrix<T> result = new Matrix<T>();

            ParallelExtensions.For(0, rows, options, i =>
            {
                for (uint j = 0; j < cols; j++)
                {
                    dynamic value = this.GetElement(i, j);
                    result.SetElement(i, j, value * scalar);
                }
            });

            return result;
        }
        public Matrix<T> Transpose()
        {
            var (rows, cols) = this.GetSize();
            Matrix<T> transposedMatrix = new Matrix<T>();

            ParallelExtensions.For(0, cols, options, j =>
            {
                for (uint i = 0; i < rows; i++)
                {
                    T value = this.GetElement(i, j);
                    transposedMatrix.SetElement(j, i, value);
                }
            });

            return transposedMatrix;
        }
        public Matrix<T> AddScalar(T scalar)
        {
            var (rows, cols) = this.GetSize();

            Matrix<T> result = new Matrix<T>();

            ParallelExtensions.For(0, rows, options, i =>
            {
                for (uint j = 0; j < cols; j++)
                {
                    dynamic sum = default(T);
                    for (uint k = 0; k < cols; k++)
                    {
                        dynamic valA = this.GetElement(i, k);
                        sum += valA + scalar;
                    }
                    result.SetElement(i, j, sum);
                }
            });
            return result;
        }
        public Matrix<T> Diag(Matrix<T> diag_vector)
        {
            ulong dim = RowCount();

            if (dim == 0)
                throw new ArgumentException("diag_vector must be 0x(N-1) or N-1x0");

            Matrix<T> M = new Matrix<T>();

            for (ulong i = 0; i < dim; i++)
            {
                M[i, i] = diag_vector[i, 0];
            }

            return M;

        }
        public Matrix<T> Inverse()
        {
            if (!this.IsSquare())
                throw new InvalidOperationException("Cannot invert non-square matrix.");

            dynamic det = this.Determinant();

            if (det == 0)
            {
                throw new InvalidOperationException("Cannot invert matrix with determinant of zero.");
            }

            ulong n = this.ColumnCount();

            if (n == 1)
            {
                Matrix<T> singleElementMatrix = new Matrix<T>();
                singleElementMatrix[0, 0] = (1 / det);

                return singleElementMatrix;
            }

            if (this.IsDiagonal())
            {
                Matrix<T> d = this.DiagVector();

                ParallelExtensions.For(0, n, options, i =>
                {
                    d[i, 0] = (dynamic)1 / d[i, 0];
                });
                return Diag(d);
            }

            Matrix<T> buf = new Matrix<T>();

            ParallelExtensions.For(0, n, options, i =>
            {
                for (ulong j = 0; j < n; j++)
                {
                    dynamic sign = (int)Math.Pow(-1, (i + j + 1));
                    dynamic minorDeterminant = this.Minor((ulong)i, j).Determinant();
                    buf[(ulong)j, (ulong)i] = sign * minorDeterminant;
                }
            });
            return buf.MultiplyScalar((dynamic)1 / det);
        }
        public Matrix<T> DiagVector()
        {
            if (!this.IsSquare())
                throw new InvalidOperationException("Cannot get diagonal of non-square matrix.");

            Matrix<T> v = new Matrix<T>();

            ulong RowCount = this.RowCount();

            ParallelExtensions.For(0, RowCount, options, i =>
            {
                v[i, 0] = this[i, i];
            });
            return v;
        }
        public T Trace()
        {
            if (!this.IsSquare())
                throw new InvalidOperationException("Cannot calc trace of non-square matrix.");

            dynamic temp = 0;
            for (ulong i = 0; i < this.RowCount(); i++)
            {
                temp += this[i, i];
            }
            return temp;
        }
        public Matrix<T> Conjugate()
        {
            Matrix<T> M = new Matrix<T>();

            for (ulong i = 1; i <= RowCount(); i++)
            {
                for (ulong j = 1; j <= ColumnCount(); j++)
                {
                    M[i, j] = this[i, j];
                }
            }

            return M;
        }
        public T Determinant()
        {
            var (rows, cols) = this.GetSize();

            if (rows != cols)
            {
                throw new InvalidOperationException("Determinant can only be calculated for square matrices.");
            }


            if (rows == 2) // 2x2 Matrix
            {
                dynamic a = this.GetElement(0, 0);
                dynamic b = this.GetElement(0, 1);
                dynamic c = this.GetElement(1, 0);
                dynamic d = this.GetElement(1, 1);

                return a * d - b * c;
            }
            else if (rows == 3) // 3x3 Matrix
            {
                dynamic a = this.GetElement(0, 0);
                dynamic b = this.GetElement(0, 1);
                dynamic c = this.GetElement(0, 2);
                dynamic d = this.GetElement(1, 0);
                dynamic e = this.GetElement(1, 1);
                dynamic f = this.GetElement(1, 2);
                dynamic g = this.GetElement(2, 0);
                dynamic h = this.GetElement(2, 1);
                dynamic i = this.GetElement(2, 2);

                return a * (e * i - f * h) - b * (d * i - f * g) + c * (d * h - e * g);
            }
            else if (rows == 4) // 4x4 Matrix
            {
                dynamic a = this.GetElement(0, 0);
                dynamic b = this.GetElement(0, 1);
                dynamic c = this.GetElement(0, 2);
                dynamic d = this.GetElement(0, 3);
                dynamic e = this.GetElement(1, 0);
                dynamic f = this.GetElement(1, 1);
                dynamic g = this.GetElement(1, 2);
                dynamic h = this.GetElement(1, 3);
                dynamic i = this.GetElement(2, 0);
                dynamic j = this.GetElement(2, 1);
                dynamic k = this.GetElement(2, 2);
                dynamic l = this.GetElement(2, 3);
                dynamic m = this.GetElement(3, 0);
                dynamic n = this.GetElement(3, 1);
                dynamic o = this.GetElement(3, 2);
                dynamic p = this.GetElement(3, 3);

                return a * (f * (k * p - l * o) - g * (j * p - l * n) + h * (j * o - k * n))
                     - b * (e * (k * p - l * o) - g * (i * p - l * m) + h * (i * o - k * m))
                     + c * (e * (j * p - l * n) - f * (i * p - l * m) + h * (i * n - j * m))
                     - d * (e * (j * o - k * n) - f * (i * o - k * m) + g * (i * n - j * m));
            }
            else
            {
                // Computationally expensive so left last
                return CalculateDeterminant(this, rows);
            }
        }
        private T CalculateDeterminant(Matrix<T> matrix, ulong size)
        {
            if (size == 1)
            {
                return matrix.GetElement(0, 0);
            }

            dynamic determinant = default(T);
            object lockObj = new object();

            ParallelExtensions.For(0, size, options, i =>
            {
                Matrix<T> subMatrix = Minor(0, i);
                dynamic element = matrix.GetElement(0, i);
                dynamic cofactor = element * CalculateDeterminant(subMatrix, size - 1) * ((i % 2 == 1) ? -1 : 1);

                lock (lockObj)
                {
                    determinant += cofactor;
                }
            });

            return determinant;
        }
        public Matrix<T> Power(int exponent)
        {
            var (rows, cols) = this.GetSize();

            Matrix<T> result = this.Clone();

            for (int iexp = 1; iexp < exponent; iexp++)
            {
                result = result.DotProduct(this);
            }
            return result;
        }
        #endregion

        #region Operator Overloads
        public static Matrix<T> operator *(Matrix<T> a, Matrix<T> b)
        {
            return a.DotProduct(b);
        }
        public static Matrix<T> operator +(Matrix<T> a, Matrix<T> b)
        {
            return a.Add(b);
        }
        public static Matrix<T> operator -(Matrix<T> a, Matrix<T> b)
        {
            return a.Subtract(b);
        }
        public static Matrix<T> operator *(Matrix<T> a, T b)
        {
            return a.MultiplyScalar(b);
        }
        public static Matrix<T> operator +(Matrix<T> a, T b)
        {
            return a.AddScalar(b);
        }
        #endregion

        #region Utility Methods
        private Matrix<T> Minor(ulong excludingRow, ulong excludingCol)
        {
            ulong size = RowCount();

            Matrix<T> subMatrix = new Matrix<T>();

            ParallelExtensions.For(0, size, options, (i) =>
            {
                if (i == excludingRow)
                    return; // Equivalent to 'continue' in a regular for-loop.

                ulong subi = i > excludingRow ? i - 1 : i; // Adjust subi index if rows above excludingRow were skipped.
                ulong subj = 0;

                for (ulong j = 0; j < size; j++)
                {
                    if (j == excludingCol)
                        continue;

                    subMatrix.SetElement(subi, subj, this.GetElement(i, j));
                    subj++;
                }
            });

            return subMatrix;
        }
        public Matrix<T> Identity(ulong n)
        {
            Matrix<T> result = new Matrix<T>(); // Assuming Matrix<T> has a constructor that initializes the matrix size
            for (ulong i = 0; i < n; i++)
            {
                for (ulong j = 0; j < n; j++)
                {
                    result[i, j] = (i == j) ? (dynamic)1 : (dynamic)0; // Using dynamic to assume T can be an integer or any numeric type
                }
            }
            return result;
        }
        public Matrix<T> Clone()
        {
            var clone = new Matrix<T>();

            List<KeyValuePair<(ulong, ulong), T>> elements = new List<KeyValuePair<(ulong, ulong), T>>(Data);

            ParallelExtensions.For(0, (ulong)this.Data.Count, options, i =>
            {
                KeyValuePair<(ulong, ulong), T> Element;

                lock (elements)
                {
                    Element = elements[(int)i];
                }
                clone.SetElement(Element.Key.Item1, Element.Key.Item2, Element.Value);
            });

            return clone;
        }
        private void DeleteRow(ulong rowIndex)
        {
            ConcurrentDictionary<(ulong, ulong), T> Temp_matrixData = new ConcurrentDictionary<(ulong, ulong), T>();

            foreach (var key in Data.Keys)
            {
                if (key.Item1 > rowIndex)
                {
                    Temp_matrixData.TryAdd((key.Item1 - 1, key.Item2), Data[(key.Item1, key.Item2)]);
                }
                if (key.Item1 < rowIndex)
                {
                    Temp_matrixData.TryAdd((key.Item1, key.Item2), Data[(key.Item1, key.Item2)]);
                }
            }
            this.Data = Temp_matrixData;
        }
        private void DeleteColumn(ulong columnIndex)
        {
            ConcurrentDictionary<(ulong, ulong), T> Temp_matrixData = new ConcurrentDictionary<(ulong, ulong), T>();

            foreach (var key in Data.Keys)
            {
                if (key.Item2 > columnIndex)
                {
                    Temp_matrixData.TryAdd((key.Item1, key.Item2 - 1), Data[(key.Item1, key.Item2)]);
                }
                if (key.Item2 < columnIndex)
                {
                    Temp_matrixData.TryAdd((key.Item1, key.Item2), Data[(key.Item1, key.Item2)]);
                }
            }
            this.Data = Temp_matrixData;
        }
        private void InsertEmptyRow(ulong rowIndex)
        {
            ConcurrentDictionary<(ulong, ulong), T> Temp_matrixData = new ConcurrentDictionary<(ulong, ulong), T>();

            foreach (var key in Data.Keys)
            {
                if (key.Item1 == rowIndex)
                {
                    Temp_matrixData.TryAdd((key.Item1, key.Item2), default(T));
                }
                if (key.Item1 > rowIndex)
                {
                    Temp_matrixData.TryAdd((key.Item1 + 1, key.Item2), Data[(key.Item1, key.Item2)]);
                }
                if (key.Item1 < rowIndex)
                {
                    Temp_matrixData.TryAdd((key.Item1, key.Item2), Data[(key.Item1, key.Item2)]);
                }
            }
            this.Data = Temp_matrixData;
        }
        private void InsertEmptyColumn(ulong columnIndex)
        {
            ConcurrentDictionary<(ulong, ulong), T> Temp_matrixData = new ConcurrentDictionary<(ulong, ulong), T>();

            foreach (var key in Data.Keys)
            {
                if (key.Item2 == columnIndex)
                {
                    Temp_matrixData.TryAdd((key.Item1, key.Item2), default(T));
                }
                if (key.Item2 > columnIndex)
                {
                    Temp_matrixData.TryAdd((key.Item1, key.Item2 + 1), Data[(key.Item1, key.Item2)]);
                }
                if (key.Item2 < columnIndex)
                {
                    Temp_matrixData.TryAdd((key.Item1, key.Item2), Data[(key.Item1, key.Item2)]);
                }
            }
            this.Data = Temp_matrixData;
        }
        public static Matrix<int> RandomIntMatrix(ulong RowCount, ulong ColumnCount)
        {
            ParallelOptions _options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            Matrix<int> M = new Matrix<int>();

            ParallelExtensions.For(0, RowCount, _options, r =>
            {
                Random rnd = new Random();
                for (ulong c = 0; c < ColumnCount; c++)
                {
                    M[r, c] = rnd.Next();
                }
            });

            return M;
        }
        public static Matrix<int> RandomBoxIntMatrix(ulong RowCount, ulong ColumnCount)
        {
            ParallelOptions _options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            Matrix<int> M = new Matrix<int>();

            ParallelExtensions.For(0, RowCount, _options, r =>
            {
                Random rnd = new Random();
                for (ulong c = 0; c < ColumnCount; c++)
                {
                    M[r, c] = rnd.Next();
                }
            });

            return M;
        }

        public static Matrix<T> InitializeBoxMatrix(ulong RowCount, ulong ColumnCount)
        {
            ParallelOptions _options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            Matrix<T> M = new Matrix<T>();

            ParallelExtensions.For(0, RowCount, _options, r =>
            {
                for (ulong c = 0; c < ColumnCount; c++)
                {
                    M[r, c] = default(T);
                }
            });

            return M;
        }
        #endregion

        #region Serialization
        public void ExportToFile(string filePath)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string json = JsonConvert.SerializeObject(Data, settings);
            File.WriteAllText(filePath, json);
        }
        public static Matrix<T> LoadFromFile(string filePath)
        {
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                Formatting = Newtonsoft.Json.Formatting.Indented
            };

            string json = File.ReadAllText(filePath);

            // Initially deserialize to dictionary with string keys to manually handle tuple conversion
            var tempMatrixData = JsonConvert.DeserializeObject<Dictionary<string, double>>(json, settings);

            Matrix<T> Temp_matrix = new Matrix<T>();
            try
            {
                foreach (var key in tempMatrixData.Keys)
                {
                    var parts = key.Trim('(', ')').Split(',');
                    // Attempt to convert the value to the type T
                    T value = ChangeType<T>(tempMatrixData[key]);

                    Temp_matrix[ulong.Parse(parts[0]), ulong.Parse(parts[1])] = value;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return Temp_matrix;
        }
        private static T ChangeType<T>(object value)
        {
            // Check if the value is already of type T
            if (value is T variable)
                return variable;

            // Handle the case where T is nullable
            Type conversionType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);

            // Convert the value to type T
            return (T)Convert.ChangeType(value, conversionType, CultureInfo.InvariantCulture);
        }
        public static Matrix<T> LoadFromExcel(string filePath, string sheetName)
        {
            Matrix<T> ExcelMatrix = new Matrix<T>();
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var worksheet = workbook.Worksheet(sheetName);

                    var rows = worksheet.RangeUsed().RowsUsed();
                    foreach (var row in rows)
                    {
                        var cells = row.CellsUsed();
                        foreach (var cell in cells)
                        {
                            ulong rowNumber = (ulong)(cell.Address.RowNumber - 1); // Convert back to 0-based index
                            ulong columnNumber = (ulong)(cell.Address.ColumnNumber - 1); // Convert back to 0-based index

                            ExcelMatrix[rowNumber, columnNumber] = ChangeType<T>((string)cell.Value);

                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

            return ExcelMatrix;
        }
        public void ExportToExcel(string filePath, string MatrixName)
        {
            ulong maxItemR = Data.Keys.Max(key => key.Item1);
            ulong maxItemC = Data.Keys.Max(key => key.Item2);

            if (maxItemC > 16384 || maxItemR > 1048576)
            {
                throw new System.Exception("Matrix is too large to export to excel");
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(MatrixName);

                foreach (var cell in Data)
                {
                    if (Data.ContainsKey((cell.Key.Item1, cell.Key.Item2)))
                    {
                        worksheet.Cell((int)cell.Key.Item1 + 1, (int)cell.Key.Item2 + 1).Value = cell.Value != null ? cell.Value.ToString() : default(T)?.ToString() ?? string.Empty;
                    }
                    else
                    {
                        worksheet.Cell((int)cell.Key.Item1 + 1, (int)cell.Key.Item2 + 1).Value = 0;
                    }
                }

                if (string.IsNullOrWhiteSpace(Path.GetExtension(filePath)))
                {
                    filePath += ".xlsx";
                }

                workbook.SaveAs(filePath);
            }
        }
        #endregion

        public override string ToString()
        {
            if (Data == null || !Data.Any())
                return "The matrix is empty.";

            var stringBuilder = new StringBuilder();
            var maxRow = Data.Keys.Max(k => k.Item1);
            var maxCol = Data.Keys.Max(k => k.Item2);
            int cellWidth = Data.Values.Max(v => v.ToString().Length) + 2; // Find maximum value width for alignment

            // Top border
            stringBuilder.AppendLine("┌" + string.Join("", Enumerable.Repeat($"─{new string('─', cellWidth)}┬", (int)maxCol)) + $"─{new string('─', cellWidth)}┐");

            for (ulong row = 0; row <= maxRow; row++)
            {
                stringBuilder.Append("│ "); // Left border for each row

                for (ulong col = 0; col <= maxCol; col++)
                {
                    if (Data.TryGetValue((row, col), out T value))
                        stringBuilder.Append($"{value}".PadRight(cellWidth)); // Align for better readability
                    else
                        stringBuilder.Append(new string(' ', cellWidth)); // Placeholder for missing values

                    stringBuilder.Append("│ "); // Separator or right border
                }

                stringBuilder.AppendLine(); // Newline for the next row

                // Separator between rows or bottom border
                if (row < maxRow)
                    stringBuilder.AppendLine("├" + string.Join("", Enumerable.Repeat($"─{new string('─', cellWidth)}┼", (int)maxCol)) + $"─{new string('─', cellWidth)}┤");
                else
                    stringBuilder.AppendLine("└" + string.Join("", Enumerable.Repeat($"─{new string('─', cellWidth)}┴", (int)maxCol)) + $"─{new string('─', cellWidth)}┘");
            }

            return stringBuilder.ToString();
        }

    }

    public interface IMatrix<T>
    {
        public int MaxDegreeOfParallelism { get; set; }

        public T this[ulong Row, ulong Column] { get; set; }

        #region Matrix Size and Structure
        public ulong RowCount();
        public ulong ColumnCount();
        public bool IsSquare();
        public bool IsOrthogonal();
        public bool IsDiagonal();
        public bool IsUnitary();
        #endregion

        #region Matrix Operations
        public Matrix<T> DotProduct(Matrix<T> other);
        public Matrix<T> Add(Matrix<T> other);
        public Matrix<T> Subtract(Matrix<T> other);
        public Matrix<T> MultiplyScalar(T scalar);
        public Matrix<T> Transpose();
        public Matrix<T> AddScalar(T scalar);
        public Matrix<T> Diag(Matrix<T> diag_vector);
        public Matrix<T> Inverse();
        public Matrix<T> DiagVector();
        public T Trace();
        public Matrix<T> Conjugate();
        public T Determinant();
        public Matrix<T> Power(int exponent);
        #endregion

    }
}