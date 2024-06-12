using CallaghanDev.Utilities.Extensions;
using CallaghanDev.XML.Excel;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text;
using ILGPU.Runtime;
using CallaghanDev.Utilities.GPU;

namespace CallaghanDev.Utilities.MathTools
{
    public class Matrix<T> : IMatrix<T>, IEnumerable<KeyValuePair<MatrixKey, T>>, IDisposable
    {
        ParallelOptions options;
        ConcurrentDictionary<MatrixKey, T> Data;

        private int _MaxDegreeOfParallelism;
        public int MaxDegreeOfParallelism
        {
            get 
            { 
                return _MaxDegreeOfParallelism; 
            }
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
            Data = new ConcurrentDictionary<MatrixKey, T>();
            MaxDegreeOfParallelism = Environment.ProcessorCount;
        }

        public T this[int Row, int Column]
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
    
        public T[] Column(int Index)
        {
            return  Data.AsParallel().Where(kvp => kvp.Key.Column == Index)
               .Select(kvp => kvp.Value).ToArray();
        }
        public T[] Row(int Index)
        {
            return Data.AsParallel().Where(kvp => kvp.Key.Row == Index)
               .Select(kvp => kvp.Value).ToArray();
        }

        #region Private Helpers
        private T GetElement(int row, int col)
        {
            return Data.TryGetValue(new MatrixKey(row, col), out T value) ? value : default(T);
        }

        private void SetElement(int row, int column, T value)
        {
            MatrixKey key = new MatrixKey(row, column);

            Data.AddOrUpdate(key, value, (existingKey, existingValue) => value);

            // Update the row and column counts if necessary
            // Assuming _RowCount and _ColumnCount are thread-safe (e.g., using locks or Interlocked)
            if (column >= _ColumnCount)
            {
                _ColumnCount = column + 1;
            }
            if (row >= _RowCount)
            {
                _RowCount = row + 1;
            }
        }

        public (int Rows, int Columns) GetSize()
        {
            if (Data.Count == 0)
            {
                return (0, 0);
            }
            // Size is max index + 1 as matrix indices are 0-based
            return (RowCount(), ColumnCount());
        }
        #endregion

        #region Matrix Size and Structure

        private int _RowCount = 0;
        private int _ColumnCount = 0;
        public int RowCount()
        {
            return _RowCount;
        }
        public int ColumnCount()
        {
            return _ColumnCount;
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

            for (int r = 0; r < this.RowCount(); r++)
            {
                for (int c = 0; c < this.ColumnCount(); c++)
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

        #region Matrix Operations CPU
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
                for (int j = 0; j < colsB; j++)
                {
                    dynamic sum = default(T);
                    for (int k = 0; k < colsA; k++)
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
                for (int j = 0; j < colsThis; j++)
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
                for (int j = 0; j < colsThis; j++)
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
                for (int j = 0; j < cols; j++)
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
                for (int i = 0; i < rows; i++)
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
                for (int j = 0; j < cols; j++)
                {
                    dynamic sum = default(T);
                    for (int k = 0; k < cols; k++)
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
            int dim = RowCount();

            if (dim == 0)
                throw new ArgumentException("diag_vector must be 0x(N-1) or N-1x0");

            Matrix<T> M = new Matrix<T>();

            for (int i = 0; i < dim; i++)
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

            int n = this.ColumnCount();

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
                for (int j = 0; j < n; j++)
                {
                    dynamic sign = (int)System.Math.Pow(-1, (i + j + 1));
                    dynamic minorDeterminant = this.Minor((int)i, j).Determinant();
                    buf[(int)j, (int)i] = sign * minorDeterminant;
                }
            });
            return buf.MultiplyScalar((dynamic)1 / det);
        }
        public Matrix<T> DiagVector()
        {
            if (!this.IsSquare())
                throw new InvalidOperationException("Cannot get diagonal of non-square matrix.");

            Matrix<T> v = new Matrix<T>();

            int RowCount = this.RowCount();

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
            for (int i = 0; i < this.RowCount(); i++)
            {
                temp += this[i, i];
            }
            return temp;
        }
        public Matrix<T> Conjugate()
        {
            Matrix<T> M = new Matrix<T>();

            for (int i = 1; i <= RowCount(); i++)
            {
                for (int j = 1; j <= ColumnCount(); j++)
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
        private T CalculateDeterminant(Matrix<T> matrix, int size)
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

        #region Matrix Operations GPU

        private GPUAcceleratedArrayTensor.DotProduct GPUAccelleratedDotProduct;

        /// <summary>
        /// This gives a IlGPU accelerator option for dot product of a matrix.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="accelerator"></param>
        /// <returns></returns>
        public Matrix<float> DotProduct(Matrix<float> other, Accelerator accelerator)
        {
            if (GPUAccelleratedDotProduct == null)
            {
                GPUAccelleratedDotProduct = new GPUAcceleratedArrayTensor.DotProduct(accelerator);
            }

            float[,] thisArray = this.ToFloatArray();
            float[,] OtherArray = other.ToArray();

            float[,] values = GPUAccelleratedDotProduct.Calculate(thisArray, OtherArray);

            return new Matrix<float>() { Data = ToConcurrentDictionary<float>(values) };
        }
        public Matrix<double> DotProduct(Matrix<double> other, Accelerator accelerator)
        {
            if (GPUAccelleratedDotProduct == null)
            {
                GPUAccelleratedDotProduct = new GPUAcceleratedArrayTensor.DotProduct(accelerator);
            }

            double[,] thisArray = this.ToDoubleArray();
            double[,] OtherArray = other.ToArray();

            double[,] values = GPUAccelleratedDotProduct.Calculate(thisArray, OtherArray);

            return new Matrix<double>() { Data = ToConcurrentDictionary<double>(values) };
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
        private Matrix<T> Minor(int excludingRow, int excludingCol)
        {
            int size = RowCount();

            Matrix<T> subMatrix = new Matrix<T>();

            ParallelExtensions.For(0, size, options, (i) =>
            {
                if (i == excludingRow)
                    return; // Equivalent to 'continue' in a regular for-loop.

                int subi = i > excludingRow ? i - 1 : i; // Adjust subi index if rows above excludingRow were skipped.
                int subj = 0;

                for (int j = 0; j < size; j++)
                {
                    if (j == excludingCol)
                        continue;

                    subMatrix.SetElement(subi, subj, this.GetElement(i, j));
                    subj++;
                }
            });

            return subMatrix;
        }
        public Matrix<T> WithoutFirstAndLastColumn()
        {
            int Columns = ColumnCount()-1;
            int Rows = RowCount();
            Matrix<T> subMatrix = new Matrix<T>();

            int subR = 0; // Adjust subi index if rows above excludingRow were skipped.
            int subC;

            ParallelExtensions.For(1, Columns, options, (c) =>
            {
                subC = 0;
                subR++;
                for (int r = 0; r < Rows; r++)
                {
                    subMatrix.SetElement(subR, subC, this.GetElement(r, c));
                    subC++;
                }
            });

            return subMatrix;
        }
        public Matrix<T> Identity(int n)
        {
            Matrix<T> result = new Matrix<T>(); // Assuming Matrix<T> has a constructor that initializes the matrix size
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    result[i, j] = (i == j) ? (dynamic)1 : (dynamic)0; // Using dynamic to assume T can be an integer or any numeric type
                }
            }
            return result;
        }
        public Matrix<T> Clone()
        {
            var clone = new Matrix<T>();

            List<KeyValuePair<MatrixKey, T>> elements = new List<KeyValuePair<MatrixKey, T>>(Data);

            ParallelExtensions.For(0, (int)this.Data.Count, options, i =>
            {
                KeyValuePair<MatrixKey, T> Element;

                lock (elements)
                {
                    Element = elements[(int)i];
                }
                clone.SetElement(Element.Key.Row, Element.Key.Column, Element.Value);
            });

            return clone;
        }
        private void DeleteRow(int rowIndex)
        {
            if (rowIndex >= _RowCount)
            {
                throw new Exception("Row doesnt exist and cannot be deleted");
            }
            ConcurrentDictionary<MatrixKey, T> Temp_matrixData = new ConcurrentDictionary<MatrixKey, T>();
            MatrixKey matrixkey;
            MatrixKey matrixkey1;
            foreach (var key in Data.Keys)
            {
                if (key.Row > rowIndex)
                {
                     matrixkey1 = new MatrixKey(key.Row-1, key.Column);
                     matrixkey = new MatrixKey(key.Row, key.Column);

                    Temp_matrixData.TryAdd(matrixkey1, Data[matrixkey]);
                }
                if (key.Row < rowIndex)
                {
                     matrixkey = new MatrixKey(key.Row, key.Column);

                    Temp_matrixData.TryAdd(matrixkey, Data[matrixkey]);
                }
            }
            this.Data = Temp_matrixData;
            _RowCount = _RowCount -1;
        }
        private void DeleteColumn(int columnIndex)
        {
            if (columnIndex >= _ColumnCount)
            {
                throw new Exception("Column doesnt exist and cannot be deleted");
            }
            ConcurrentDictionary<MatrixKey, T> Temp_matrixData = new ConcurrentDictionary<MatrixKey, T>();
            MatrixKey matrixkey;
            MatrixKey matrixkey1;

            foreach (var key in Data.Keys)
            {
                matrixkey1 = new MatrixKey(key.Row, key.Column-1);
                matrixkey = new MatrixKey(key.Row, key.Column);
                if (key.Column > columnIndex)
                {
                    Temp_matrixData.TryAdd(matrixkey1, Data[matrixkey]);
                }
                if (key.Column < columnIndex)
                {
                    Temp_matrixData.TryAdd(matrixkey, Data[matrixkey]);
                }
            }

            this.Data = Temp_matrixData;
            _ColumnCount = _ColumnCount - 1;
        }
        private void InsertEmptyRow(int rowIndex)
        {
            ConcurrentDictionary<MatrixKey, T> Temp_matrixData = new ConcurrentDictionary<MatrixKey, T>();
            MatrixKey matrixkey;
            MatrixKey matrixkey1;
            foreach (var key in Data.Keys)
            {
                matrixkey = new MatrixKey(key.Row, key.Column);
                if (key.Row == rowIndex)
                {
                    Temp_matrixData.TryAdd(matrixkey, default(T));
                }
                if (key.Row > rowIndex)
                {
                    matrixkey1 = new MatrixKey(key.Row + 1, key.Column);
                    Temp_matrixData.TryAdd(matrixkey1, Data[matrixkey]);
                }
                if (key.Row < rowIndex)
                {
                    Temp_matrixData.TryAdd(matrixkey, Data[matrixkey]);
                }
            }
            this.Data = Temp_matrixData;
            _RowCount = _RowCount + 1;
        }
        private void InsertEmptyColumn(int columnIndex)
        {
            ConcurrentDictionary<MatrixKey, T> Temp_matrixData = new ConcurrentDictionary<MatrixKey, T>();
            MatrixKey matrixkey;
            MatrixKey matrixkey1;
            foreach (var key in Data.Keys)
            {
                matrixkey = new MatrixKey(key.Row, key.Column);
                if (key.Column == columnIndex)
                {
                    Temp_matrixData.TryAdd(matrixkey, default(T));
                }
                if (key.Column > columnIndex)
                {
                    matrixkey1 = new MatrixKey(key.Row + 1, key.Column);
                    Temp_matrixData.TryAdd(matrixkey1, Data[matrixkey]);
                }
                if (key.Column < columnIndex)
                {
                    Temp_matrixData.TryAdd(matrixkey, Data[matrixkey]);
                }
            }
            this.Data = Temp_matrixData;

            _ColumnCount = _ColumnCount + 1;
        }
        public static Matrix<int> RandomIntMatrix(int RowCount, int ColumnCount)
        {
            ParallelOptions _options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            Matrix<int> M = new Matrix<int>();

            ParallelExtensions.For(0, RowCount, _options, r =>
            {
                Random rnd = new Random();
                for (int c = 0; c < ColumnCount; c++)
                {
                    M[r, c] = rnd.Next();
                }
            });
         
            return M;
        }
        public static Matrix<int> RandomBoxIntMatrix(int RowCount, int ColumnCount)
        {
            ParallelOptions _options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            Matrix<int> M = new Matrix<int>();

            ParallelExtensions.For(0, RowCount, _options, r =>
            {
                Random rnd = new Random();
                for (int c = 0; c < ColumnCount; c++)
                {
                    M[r, c] = rnd.Next();
                }
            });

            return M;
        }
        public static Matrix<T> InitializeBoxMatrix(int RowCount, int ColumnCount)
        {
            ParallelOptions _options = new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            };
            Matrix<T> M = new Matrix<T>();

            ParallelExtensions.For(0, RowCount, _options, r =>
            {
                for (int c = 0; c < ColumnCount; c++)
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
            var tempMatrixData = JsonConvert.DeserializeObject<ConcurrentDictionary<MatrixKey, T>>(json, settings);

            Matrix<T> Temp_matrix = new Matrix<T>();
            try
            {
                foreach (var key in tempMatrixData.Keys)
                {

                    // Attempt to convert the value to the type T
                    T value = ChangeType<T>(tempMatrixData[key]);

                    Temp_matrix[key.Row, key.Column] = value;
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
                            int rowNumber = (int)(cell.Address.RowNumber - 1); // Convert back to 0-based index
                            int columnNumber = (int)(cell.Address.ColumnNumber - 1); // Convert back to 0-based index

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
            int maxItemR = RowCount();
            int maxItemC = ColumnCount();

            if (maxItemC > 16384 || maxItemR > 1048576)
            {
                throw new System.Exception("Matrix is too large to export to excel");
            }

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add(MatrixName);

                foreach (var cell in Data)
                {
                    if (Data.ContainsKey(new MatrixKey(cell.Key.Row, cell.Key.Column)))
                    {
                        worksheet.Cell((int)cell.Key.Row + 1, (int)cell.Key.Column + 1).Value = cell.Value != null ? cell.Value.ToString() : default(T)?.ToString() ?? string.Empty;
                    }
                    else
                    {
                        worksheet.Cell((int)cell.Key.Row + 1, (int)cell.Key.Column + 1).Value = 0;
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

        #region utility methods and Linq
        public override string ToString()
        {
            if (Data == null || !Data.Any())
                return "The matrix is empty.";

            var stringBuilder = new StringBuilder();
            var maxRow = Data.Keys.Max(k => k.Row);
            var maxCol = Data.Keys.Max(k => k.Column);
            int cellWidth = Data.Values.Max(v => v.ToString().Length) + 2; // Find maximum value width for alignment

            // Top border
            stringBuilder.AppendLine("┌" + string.Join("", Enumerable.Repeat($"─{new string('─', cellWidth)}┬", (int)maxCol)) + $"─{new string('─', cellWidth)}┐");

            for (int row = 0; row <= maxRow; row++)
            {
                stringBuilder.Append("│ "); // Left border for each row

                for (int col = 0; col <= maxCol; col++)
                {
                    MatrixKey matrixKey = new MatrixKey(row, col);
                    if (Data.TryGetValue(matrixKey, out T value))
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

        public T[,] ToArray()
        {
            if (Data == null || Data.Count == 0)
            {
                return new T[0, 0];
            }

            int maxRow = RowCount();
            int maxColumn = ColumnCount();

            // Initialize the array with the determined maximum dimensions
            T[,] array = new T[maxRow, maxColumn];


          /*  Parallel.ForEach(Data, kvp =>
            {
                array[kvp.Key.Row, kvp.Key.Column] = kvp.Value;

            });*/

            foreach (var kvp in Data)
            {

                array[kvp.Key.Row, kvp.Key.Column] = kvp.Value;
            }

            return array;
        }
        public T[][] ConvertToColumnArray()
        {
            if (Data.IsEmpty)
            {
                return new T[0][]; // Return an empty jagged array if the dictionary is empty.
            }

            // Determine the size of the jagged array
            int maxRow = RowCount();
            int maxColumn = ColumnCount();

            // Initialize the jagged array
            T[][] dataArray = new T[maxColumn][];
            for (int i = 0; i < maxColumn; i++)
            {
                dataArray[i] = new T[maxRow]; // Correctly initialize each row with 'maxColumn' columns
            }

            // Populate the jagged array from the dictionary
            Parallel.ForEach(Data, keyValuePair =>
            {
                int row = keyValuePair.Key.Row;
                int column = keyValuePair.Key.Column;
                dataArray[column][row] = keyValuePair.Value; // Correctly access row first, then column
            });
            return dataArray;
        }
        public Matrix<TResult> Select<TResult>(Func<T, TResult> selector)
        {
            var resultMatrix = new Matrix<TResult>();

            Parallel.ForEach(Data, item =>
            {
                resultMatrix[item.Key.Row, item.Key.Column] = selector(item.Value);
            });

            return resultMatrix;
        }
        public ConcurrentDictionary<MatrixKey, T> ToConcurrentDictionary<T>(T[,] array)
        {
            var dictionary = new ConcurrentDictionary<MatrixKey, T>();

            int rows = array.GetLength(0);
            int columns = array.GetLength(1);

            Parallel.For(0, rows, row =>
            {
                for (int column = 0; column < columns; column++)
                {
                    T value = array[row, column];
                    if (!EqualityComparer<T>.Default.Equals(value, default(T))) // Optional: only add if not default
                    {
                        dictionary.TryAdd(new MatrixKey(row, column), value);
                    }
                }
            });

            return dictionary;
        }
        public IEnumerator<KeyValuePair<MatrixKey, T>> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public float[,] ToFloatArray()
        {
            // Get the array from Matrix<T>
            T[,] genericArray = this.ToArray();

            // Create a float array with the same dimensions
            float[,] floatArray = new float[genericArray.GetLength(0), genericArray.GetLength(1)];

            // Convert each element
            for (int i = 0; i < genericArray.GetLength(0); i++)
            {
                for (int j = 0; j < genericArray.GetLength(1); j++)
                {
                    floatArray[i, j] = (float)(object)genericArray[i, j];
                }
            }

            return floatArray;
        }
        public double[,] ToDoubleArray()
        {
            // Get the array from Matrix<T>
            T[,] genericArray = this.ToArray();

            // Create a float array with the same dimensions
            double[,] doubleArray = new double[genericArray.GetLength(0), genericArray.GetLength(1)];

            // Convert each element
            for (int i = 0; i < genericArray.GetLength(0); i++)
            {
                for (int j = 0; j < genericArray.GetLength(1); j++)
                {
                    doubleArray[i, j] = (double)(object)genericArray[i, j];
                }
            }

            return doubleArray;
        }
        public decimal[,] ToDecimalArray()
        {
            // Get the array from Matrix<T>
            T[,] genericArray = this.ToArray();

            // Create a float array with the same dimensions
            decimal[,] doubleArray = new decimal[genericArray.GetLength(0), genericArray.GetLength(1)];

            // Convert each element
            for (int i = 0; i < genericArray.GetLength(0); i++)
            {
                for (int j = 0; j < genericArray.GetLength(1); j++)
                {
                    doubleArray[i, j] = (decimal)(object)genericArray[i, j];
                }
            }

            return doubleArray;
        }
        public int[,] ToIntArray()
        {
            // Get the array from Matrix<T>
            T[,] genericArray = this.ToArray();

            // Create a float array with the same dimensions
            int[,] doubleArray = new int[genericArray.GetLength(0), genericArray.GetLength(1)];

            // Convert each element
            for (int i = 0; i < genericArray.GetLength(0); i++)
            {
                for (int j = 0; j < genericArray.GetLength(1); j++)
                {
                    doubleArray[i, j] = (int)(object)genericArray[i, j];
                }
            }

            return doubleArray;
        }

        public Matrix<T> FromArray(T[,] array)
        {
            ConcurrentDictionary<MatrixKey, T> result = new ConcurrentDictionary<MatrixKey, T>();

            int rows = array.GetLength(0);
            int columns = array.GetLength(1);
            MatrixKey matrixKey;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    matrixKey = new MatrixKey(i,j);

                    result.TryAdd(matrixKey, array[i, j]);
                }
            }

            return new Matrix<T> { Data = result };
        }

        #endregion

        #region IDisposable

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (Data != null)
                    {
                        Data.Clear();
                        Data = null;
                    }

                    if (options != null)
                    {
                        options = null;
                    }
                }

                // Dispose unmanaged resources
                // (none yet for this class)

                disposed = true;
            }
        }

        ~Matrix()
        {
            Dispose(false);
        }

        #endregion
    }


}