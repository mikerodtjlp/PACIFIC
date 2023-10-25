using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Net;
using mro.BO;
using mro.DAL;
using mro;

namespace mro.BL
{
    public class report_BL
    {
        public report_BL(CParameters conns)
        {
            this.conns = conns;
            conns.get(defs.ZDFAULT, ref dbcode);
        }
        public CParameters conns = new CParameters();
        public readonly string dbcode = string.Empty;

        public string GetIP()
        {
            string strHostName = string.Empty;
            strHostName = System.Net.Dns.GetHostName();
            IPHostEntry ipEntry = System.Net.Dns.GetHostEntry(strHostName);
            IPAddress[] addr = ipEntry.AddressList;
            return addr[addr.Length-1].ToString();
        }
        //public string[] execute_query_one_row(query qry, int[] cols2ret, string target = "")
        //{
        //    string database = dbcode;
        //    if (target.Length > 0) conns.get(target, ref database);
        //    using (var dal = control_DAL.instance(database))
        //    {
        //        return dal.execute_query_one_row(qry.sql, cols2ret);
        //    }
        //}

    }
}
