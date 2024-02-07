using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.MathTools
{
    public class ExpandableMatrix<T>
    {
        private Dictionary<(int, int), T> matrix;

        public ExpandableMatrix()
        {
            matrix = new Dictionary<(int, int), T>();
        }

        public T GetElement(int row, int col)
        {
            return matrix.TryGetValue((row, col), out T value) ? value : default(T);
        }

        public void SetElement(int row, int col, T value)
        {
            matrix[(row, col)] = value;
        }

        public (int Rows, int Columns) GetSize()
        {
            if (matrix.Count == 0)
            {
                return (0, 0);
            }

            int maxRow = matrix.Keys.Max(k => k.Item1);
            int maxCol = matrix.Keys.Max(k => k.Item2);

            // Size is max index + 1 as matrix indices are 0-based
            return (maxRow + 1, maxCol + 1);
        }
        public ExpandableMatrix<T> Multiply(ExpandableMatrix<T> other)
        {
            var (rowsA, colsA) = this.GetSize();
            var (rowsB, colsB) = other.GetSize();

            if (colsA != rowsB)
            {
                throw new InvalidOperationException("Matrix dimensions are not compatible for multiplication.");
            }

            ExpandableMatrix<T> result = new ExpandableMatrix<T>();

            for (int i = 0; i < rowsA; i++)
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
            }

            return result;
        }
        public ExpandableMatrix<T> Add(ExpandableMatrix<T> other)
        {
            var (rowsThis, colsThis) = this.GetSize();
            var (rowsOther, colsOther) = other.GetSize();

            if (rowsThis != rowsOther || colsThis != colsOther)
            {
                throw new InvalidOperationException("Matrices dimensions do not match for addition.");
            }

            ExpandableMatrix<T> result = new ExpandableMatrix<T>();

            for (int i = 0; i < rowsThis; i++)
            {
                for (int j = 0; j < colsThis; j++)
                {
                    dynamic valueThis = this.GetElement(i, j);
                    dynamic valueOther = other.GetElement(i, j);
                    result.SetElement(i, j, valueThis + valueOther);
                }
            }

            return result;
        }
        public ExpandableMatrix<T> Subtract(ExpandableMatrix<T> other)
        {
            var (rowsThis, colsThis) = this.GetSize();
            var (rowsOther, colsOther) = other.GetSize();

            if (rowsThis != rowsOther || colsThis != colsOther)
            {
                throw new InvalidOperationException("Matrices dimensions do not match for subtraction.");
            }

            ExpandableMatrix<T> result = new ExpandableMatrix<T>();

            for (int i = 0; i < rowsThis; i++)
            {
                for (int j = 0; j < colsThis; j++)
                {
                    dynamic valueThis = this.GetElement(i, j);
                    dynamic valueOther = other.GetElement(i, j);
                    result.SetElement(i, j, valueThis - valueOther);
                }
            }

            return result;
        }
        public ExpandableMatrix<T> MultiplyScalar(T scalar)
        {
            var (rows, cols) = this.GetSize();
            ExpandableMatrix<T> result = new ExpandableMatrix<T>();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    dynamic value = this.GetElement(i, j);
                    result.SetElement(i, j, value * scalar);
                }
            }

            return result;
        }
        public ExpandableMatrix<T> Transpose()
        {
            var (rows, cols) = this.GetSize();
            ExpandableMatrix<T> transposedMatrix = new ExpandableMatrix<T>();

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    T value = this.GetElement(i, j);
                    transposedMatrix.SetElement(j, i, value);
                }
            }

            return transposedMatrix;
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

        private T CalculateDeterminant(ExpandableMatrix<T> matrix, int size)
        {
            if (size == 1)
            {
                return matrix.GetElement(0, 0);
            }

            dynamic determinant = default(T);

            for (int i = 0; i < size; i++)
            {
                ExpandableMatrix<T> subMatrix = CreateSubMatrix(matrix, size, 0, i);
                dynamic element = matrix.GetElement(0, i);
                determinant += element * CalculateDeterminant(subMatrix, size - 1) * ((i % 2 == 1) ? -1 : 1);
            }

            return determinant;
        }

        private ExpandableMatrix<T> CreateSubMatrix(ExpandableMatrix<T> originalMatrix, int size, int excludingRow, int excludingCol)
        {
            ExpandableMatrix<T> subMatrix = new ExpandableMatrix<T>();
            int subi = 0;

            for (int i = 0; i < size; i++)
            {
                if (i == excludingRow)
                    continue;

                int subj = 0;
                for (int j = 0; j < size; j++)
                {
                    if (j == excludingCol)
                        continue;

                    subMatrix.SetElement(subi, subj, originalMatrix.GetElement(i, j));
                    subj++;
                }
                subi++;
            }

            return subMatrix;
        }

        // Method for calculating inverse (only for square matrices)
        public ExpandableMatrix<T> Inverse()
        {
            throw new NotImplementedException();
        }

        // Method for calculating trace (only for square matrices)
        public T Trace()
        {
            throw new NotImplementedException();
        }

        // Method for calculating rank
        public int Rank()
        {
            throw new NotImplementedException();
        }

        // Methods for eigenvalues and eigenvectors are typically complex and require specialized libraries

        // Method for LU Decomposition
        public (ExpandableMatrix<T> L, ExpandableMatrix<T> U) LUDecomposition()
        {
            throw new NotImplementedException();
        }

        // Method for QR Decomposition
        public (ExpandableMatrix<T> Q, ExpandableMatrix<T> R) QRDecomposition()
        {
            throw new NotImplementedException();
        }

        // Method for Singular Value Decomposition
        public (ExpandableMatrix<T> U, ExpandableMatrix<T> S, ExpandableMatrix<T> V) SingularValueDecomposition()
        {
            throw new NotImplementedException();
        }

        // Method for Gaussian Elimination (Row Reduction)
        public ExpandableMatrix<T> GaussianElimination()
        {
            throw new NotImplementedException();
        }

        // Method for raising a matrix to a power
        public ExpandableMatrix<T> Power(int exponent)
        {
            throw new NotImplementedException();
        }
    }


}
