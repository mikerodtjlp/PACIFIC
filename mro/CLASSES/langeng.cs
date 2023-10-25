#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mro {
  public class langeng {
    public static void translate( object cachelock,
                                  Dictionary<string, string> cache,
                                  string dbcode,
                                  mroJSON basics,
                                  ref string code) {
      if (code.Length > 0 && code.Length < 64 && code.IndexOf(' ') == -1) {
        try {
          var translated = string.Empty;
          langeng.get_desc(cachelock, cache, dbcode, code, ref translated, basics);
          if (translated != string.Empty) code = translated;
        } catch (Exception ex) { }
      }
    }
    public static void get_desc(  object cachelock,
                                  Dictionary<string, string> cache,
                                  string dbcode,
                                  string code,
                                  ref string desc,
                                  mroJSON basics) {
      desc = string.Empty;
      // codes that are too long are descriptions not codes so we know that 
      // they dont exist on the database so the best we can do is return the 
      // string to be inspected by the client (possibly a standard deviation)
      if (code.Length > 64) {
        desc = code.Substring(0, 63);
        return;
      }

      var lang = basics.get(defs.ZLANGUA);
      var key = mem.join2(lang, code);

      lock (cachelock) {
        if (cache.TryGetValue(key, out desc)) {
          if (string.IsNullOrEmpty(desc)) cache.Remove(key); // garbage
          else return;
        }
        // get it from the dictionary
        var sql = mem.join5("exec dbo.desc_get '", code, "','", lang, "';");
        desc = mro.sql.sqlscalar(dbcode, sql);
        cache.Add(key, desc);
      }
    }

    public static void apply_lang(object cachelock,
                                  Dictionary<string, string> cache,
                                  string dbcode,
                                  ref string dest,
                                  mroJSON basics) {
      StringBuilder final = new StringBuilder();
      var translated = string.Empty;
      int last = 0;

      for (; ;) {
        int left = dest.IndexOf("{@", last);
        if (left == -1) break;

        int right = dest.IndexOf("}", left + 2);
        if (right == -1) break;

        var code = dest.Substring(left + 2, (right - 1) - (left + 2));

        langeng.get_desc(cachelock, cache, dbcode, code, ref translated, basics);

        if (left > 0) final.Append(dest.Substring(last, left - 1));
        final.Append(translated);

        last = right + 1;
      }
      final.Append(dest.Substring(last));

      dest = final.ToString();
    }



      /*string key;

      int lst = 0;
      int ini = 0;
      int fin = 0;
      for (; ; ) {
        TCHAR * base = dest.GetBuffer();
        int baselen = dest.GetLength();

        // pattern begin
        TCHAR* p = _tcsstr(base + lst, patt2);
        if (p) ini = p - base;
        else break;
        lst = ini;

        // pattern end
        p = _tmemchr(p + 2, _T('}'), baselen - ini);
        if (p) fin = p - base;
        else break;

        // extract the value of the pattern that is the code
        int lenp = (fin - ini) + 1;
        if (lenp >= 128) {
          string err;
          if ((ini + 8) < dest.GetLength()) err.SetString(dest.GetBuffer() + ini, 8);
          requireex(true, _T("wrong_language_pattern"), err);
        }
        _tmemcpy(param, base + ini, lenp);
        param[lenp] = 0;

        // we form the key for the cache example: ENreg_not_exist
        int lenv = (fin - ini) - 2;
        int dl = _get_description(lang, ll, param + 2, lenv, rdesc, key);
        if (dl) dest.Replace(param, rdesc);
        else dest.Replace(param, "*"); // notfound);
      }*/

  }
}
