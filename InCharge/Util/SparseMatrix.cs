using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace InCharge.Util
{
    [Serializable]
    public class SparseMatrix<T> : IEnumerable<T>
    {
        private int rowMin = 0;
        private int rowMax = 0;
        private int colMin = 0;
        private int colMax = 0;

        private List<Tuple<int, int, T>> data = new List<Tuple<int, int, T>>();

        private Tuple<int, int, T> getTuple(int row, int col)
        {
            return data.Where(e => e.Item1 == row && e.Item2 == col).FirstOrDefault();
        }

        public T GetAt(int row, int col)
        {
            var e = getTuple(row, col);
            if (e == null)
            {
                return default(T);
            }
            else
            {
                return e.Item3;
            }
        }

        public void RemoveAt(int row, int col)
        {
            var e = getTuple(row, col);
            if (e != null) data.Remove(e);
            return;
        }

        private void updateDimensions(int row, int col)
        {
            if (row > this.rowMax) this.rowMax = row;
            if (row < this.rowMin) this.rowMin = row;
            if (col > this.colMax) this.colMax = col;
            if (col < this.colMin) this.colMin = col;
        }

        public void SetAt(int row, int col, T value)
        {
            var e = getTuple(row, col);
            if (e == null && EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                return;
            }
            if (e != null && EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                data.Remove(e);
                return;
            }
            if (e == null)
            {
                data.Add(new Tuple<int, int, T>(row, col, value));
                this.updateDimensions(row, col);
                return;
            }

            data.Remove(e);
            data.Add(new Tuple<int, int, T>(row, col, value));
            this.updateDimensions(row, col);
        }

        public IEnumerable<T> GetRowData(int row)
        {
            return data.Where(e => e.Item2 == row).Select(e => e.Item3);
        }

        public IEnumerable<T> GetColumnData(int col)
        {
            return data.Where(e => e.Item1 == col).Select(e => e.Item3);
        }

        public int GetColumnCount(int col)
        {
            return data.Where(e => e.Item2 == col).Count();
        }

        public int GetRowCount(int row)
        {
            return data.Where(e => e.Item1 == row).Count();
        }

        public T this[int row, int col]
        {
            get
            {
                return GetAt(row, col);
            }
            set
            {
                SetAt(row, col, value);
            }

        }

        public int RowMax { get { return this.rowMax; } }
        public int RowMin { get { return this.rowMin; } }
        public int ColMax { get { return this.colMax; } }
        public int ColMin { get { return this.colMin; } }

        /// <summary>
        /// Simple enumerator for the sparse matrix, does not guarantee any order!
        /// </summary>
        public class SparseMatrixEnumerator: IEnumerator<T>
        {
            private int currentIndex = -1;
            private T currentObject;
            private SparseMatrix<T> matrix;

            public SparseMatrixEnumerator(SparseMatrix<T> matrix)
            {
                this.matrix = matrix;
            }

            #region IEnumerator<T> Members

            public T Current
            {
                get { return this.currentObject; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose() {}

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                if (++this.currentIndex >= this.matrix.data.Count)
                {
                    return false;
                }
                else
                {
                    this.currentObject = this.matrix.data[this.currentIndex].Item3;
                }
                return true;
            }

            public void Reset()
            {
                this.currentIndex = -1;
            }

            #endregion
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SparseMatrixEnumerator(this);
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new SparseMatrixEnumerator(this);
        }

        #endregion
    }

    /// <summary>
    /// Provides a 3 dimensional sparse matrix implementation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SparseMatrix3D<T> : IEnumerable<T>
    {
        public int MaxX { get; set; }
        public int MinX { get; set; }
        public int MaxY { get; set; }
        public int MinY { get; set; }
        public int MaxZ { get; set; }
        public int MinZ { get; set; }

        private SortedSet<Tuple<int, int, int, T>> data = new SortedSet<Tuple<int, int, int, T>>();


        private Tuple<int, int, int, T> getTuple(int x, int y, int z)
        {
            return data.SingleOrDefault(e => e.Item1 == x && e.Item2 == y && e.Item3 == z);
        }

        public T GetAt(int x, int y, int z)
        {
            var e = this.getTuple(x, y, z);
            if (e == null)
            {
                return default(T);
            }
            else
            {
                return e.Item4;
            }
        }

        public void RemoveAt(int x, int y, int z)
        {
            var e = this.getTuple(x, y, z);
            if (e != null) data.Remove(e);
            return;
        }

        private void updateDimensions(int x, int y, int z)
        {
            if (x > this.MaxX) this.MaxX = x;
            if (x < this.MinX) this.MinX = x;
            if (y > this.MaxY) this.MaxY = y;
            if (y < this.MinY) this.MinY = y;
            if (z > this.MaxZ) this.MaxZ = z;
            if (z < this.MinZ) this.MinZ = z;
        }

        public void SetAt(int x, int y, int z, T value)
        {
            var e = getTuple(x, y, z);
            if (e == null && EqualityComparer<T>.Default.Equals(value, default(T)))
            {
                return;
            }            
            if (e == null)
            {
                data.Add(new Tuple<int, int, int, T>(x, y, z, value));
                this.updateDimensions(x, y, z);
                return;
            }

            data.Remove(e);
            data.Add(new Tuple<int, int, int, T>(x, y, z, value));
            this.updateDimensions(x, y, z);
        }

        public T this[int x, int y, int z]
        {
            get
            {
                return this.GetAt(x, y, z);
            }
            set
            {
                this.SetAt(x, y, z, value);
            }
        }

        /// <summary>
        /// Simple enumerator for the sparse matrix, does not guarantee any order!
        /// </summary>
        public class SparseMatrix3DEnumerator : IEnumerator<T>
        {
            private IEnumerator<Tuple<int, int, int, T>> enumerator;

            public SparseMatrix3DEnumerator(SparseMatrix3D<T> matrix)
            {
                this.enumerator = matrix.data.GetEnumerator();
            }

            #region IEnumerator<T> Members

            public T Current
            {
                get { return this.enumerator.Current.Item4; }
            }

            #endregion

            #region IDisposable Members

            public void Dispose() { }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get { return this.enumerator.Current.Item4; }
            }

            public bool MoveNext()
            {
                return this.enumerator.MoveNext();
            }

            public void Reset()
            {
                this.enumerator.Reset();
            }

            #endregion
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SparseMatrix3DEnumerator(this);
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new SparseMatrix3DEnumerator(this);
        }

        #endregion
    }
}
