#nullable disable

using System;
using System.Collections.Generic;

namespace CallaghanDev.XML.Excel.Caching
{
    internal sealed class XLColorRepository : XLRepositoryBase<XLColorKey, XLColor>
    {
        #region Constructors

        public XLColorRepository(Func<XLColorKey, XLColor> createNew)
            : base(createNew)
        {
        }

        public XLColorRepository(Func<XLColorKey, XLColor> createNew, IEqualityComparer<XLColorKey> comparer)
            : base(createNew, comparer)
        {
        }

        #endregion Constructors
    }
}
