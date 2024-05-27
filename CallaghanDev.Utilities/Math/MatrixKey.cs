using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallaghanDev.Utilities.MathTools
{
    public struct MatrixKey : IEquatable<MatrixKey>
    {
        public int Row;
        public int Column;

        public MatrixKey(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public override bool Equals(object obj)
        {
            return obj is MatrixKey other && Equals(other);
        }

        public bool Equals(MatrixKey other)
        {
            return Row == other.Row && Column == other.Column;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Row, Column);
        }
    }
}
