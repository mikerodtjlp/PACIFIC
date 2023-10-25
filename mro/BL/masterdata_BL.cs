using System;
using System.Collections.Generic;
using System.Text;
using mro.BO;
using mro.DAL;
using mro;

namespace mro.BL
{
    public class masterdata_BL
    {
        public masterdata_BL(CParameters conns)
        {
            conns.get(defs.ZDFAULT, ref dbcode);
        }
        public readonly string dbcode = string.Empty;

    }
}
