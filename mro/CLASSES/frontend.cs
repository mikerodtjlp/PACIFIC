#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Globalization;

namespace mro {
  public class frontend {
    public static void btn_design(client clie, string tcod, string desc) {
      clie.result.set(defs.ZFILERS, "<div id=\"rep_Menu\"class=\"menu\"onmouseover=\"menuMouseover(event)\"style=\"visibility:visible;\">" +
      "<a class=\"menuItem\"onclick=\"proc_context_menu('matchcode','S004','{}','{}','{}','&quot;onenter&quot;');\">lang</a>" +
      "<a class=\"menuItem\"onclick=\"proc_context_menu('matchcode','S005','{}','{}','{}','&quot;onenter&quot;');\">layout</a>" +
      "<a class=\"menuItem\"onclick=\"proc_context_menu('matchcode','S006','{&quot;$name$&quot;:&quot;" + tcod + "&quot;}','{}','{}','&quot;onenter&quot;');\">shortcut</a></div>");
    }

    public static void gui_get_favorites(client clie, string dbcode) {
      var hint = clie.values.get("hint");
      var basics = clie.basics;
      var result = clie.result;
      using (var dal = mro.DAL.control_DAL.instance(dbcode)) {
        var q = new mro.BO.query(hint.Length == 0 ?
                                    string.Concat("exec dbo.favorites_get ",
                                    basics.get(defs.ZCOMPNY), ",'",
                                    basics.get(defs.ZUSERID), "','",
                                    basics.get(defs.ZLANGUA), "';") :
                                    mem.join5("exec dbo.trans_get_likes '",
                                    hint, "','",
                                    basics.get(defs.ZLANGUA), "';"));
        var res = dal.execute_query(clie, q.sql, null, false);
        int n = 0;
        var resdata = res.data;
        var row = clie.poolj.getclean(0);
        foreach (var r in resdata) {
          row.set(defs.ZHISTRN, r[1]);
          row.set(defs.ZHISDSC, r[2]);
          row.set(defs.ZHISTYP, r[3]);
          result.set(utils.zhis[n], row);
          ++n;
        }
        result.set(defs.ZHISSIZ, n);
      }
    }
  }
}
