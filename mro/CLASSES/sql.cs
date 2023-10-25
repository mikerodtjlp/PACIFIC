#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif
using System;
using System.Text;
using System.Data.SqlClient;

namespace mro {
  public sealed class sql {
    public static string sqlscalar(string dbcode, string sql) {
      using (var conn = new SqlConnection(dbcode)) {
        var cmd = new SqlCommand(sql.ToString(), conn);
        conn.Open();
        var r = cmd.ExecuteScalar();
        return r == null ? string.Empty : r.ToString();
      }
    }
    public static void sqlnores(string dbcode, string q, bool rethrow) {
      try {
        using (var conn = new SqlConnection(dbcode)) {
          conn.Open();
          using (var cmd = new SqlCommand(q, conn)) {
            cmd.ExecuteNonQuery();
          }
        }
      } 
      catch (Exception e) {
        if (rethrow) throw new Exception(q, e);
      }
    }
    public static string sp5(StringBuilder a, string s0,
                               int i0, int i1, int i2, int i3) {
      a.Length = 0;
      a.Append(s0);
      a.Append(' ');
      a.Append(i0);
      a.Append(',');
      a.Append(i1);
      a.Append(',');
      a.Append(i2);
      a.Append(',');
      a.Append(i3);
      a.Append(';');
      return a.ToString();
    }
    public static string sp6(StringBuilder a, string s0,
                               int i0, int i1, int i2, int i3,
                               string s1) {
      a.Length = 0;
      a.Append(s0);
      a.Append(' ');
      a.Append(i0);
      a.Append(',');
      a.Append(i1);
      a.Append(',');
      a.Append(i2);
      a.Append(',');
      a.Append(i3);
      a.Append(", '");
      a.Append(s1);
      a.Append("';");
      return a.ToString();
    }
    /**
   * wrap a nacked query with SQL exception handling in order to catch the 
   * SQL error and give a chance to translate the error through a rise keyword
   */

    ///////////////////////////////
    /// MAYBE THIS FUNCTION is too deep, maybe we should move whe the original query i get from values
    //////////////////////////////
    public static string track_query(client clie, string qry) {
      // check if we have already inserted the tracking info
      if (qry.Length > 1 && qry[0] == '-' && qry[1] == '-')
        return qry;
      var hdr = clie != null ? mem.join9(clie.macname, " ",
                                          clie.user, " ",
                                          clie.trans, " ",
                                          client.node, " ",
                                          clie.cmpy) : string.Empty;
      return mem.join5("-- ", hdr, "\r\nbegin try \r\n", qry,
      " \r\nend try \r\nbegin catch \r\ndeclare @e varchar(256) \r\nset @e=ERROR_MESSAGE() \r\nRAISERROR (@e,16,1) \r\nend catch \r\n");
    }
  }
}
