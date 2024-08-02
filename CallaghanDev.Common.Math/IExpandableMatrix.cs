using CallaghanDev.Utilities.MathTools;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Common.Math
{
    public interface IMatrix<T>
    {
        public int MaxDegreeOfParallelism { get; set; }

        public T this[int Row, int Column] { get; set; }

        #region Matrix Size and Structure
        public int RowCount();
        public int ColumnCount();
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

        #region utility methods and Linq
        public T[,] ToArray();
        public Matrix<TResult> Select<TResult>(Func<T, TResult> selector);
        public ConcurrentDictionary<MatrixKey, T> ToConcurrentDictionary<T>(T[,] jaggedArray);
        #endregion

    }
}
