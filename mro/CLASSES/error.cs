/**
 * The purpose of this implementation is that of manage all apsect related to 
 * handle, process or manage errors, in two aspects 1) controlling exceptions
 * and controlling the language translation some 
 * 
 * significant dates
 * creation: september 21 2009 
 * version 1: november 16 2009  (basic functionallity)
 * version 2: june 7 2010       (clear distinction between proxy and core)
 * version 3: march 14 2011     (use of caches and include support for log, rights, 
 *                                       and most basic core features)
 * version 4: december 5 2011   (increasse stability and high capacity processing)
 * incorporata JSON : august 10 2012    (process mro2json and json2mro converters)
 */
#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Text;
using mro;
using System.Collections.Generic;

namespace mro {
  public sealed class mroerr : Exception {
    public mroerr(string error, string extra) {
      description = error;
      extrainfo = extra;
    }
    public string description { get; set; }
    public string extrainfo { get; set; }
  }

  public sealed class err {
    public static void require(string e) {
      throw new mroerr(e, string.Empty);
    }
    public static void require(bool p, string e) {
      if (p) throw new mroerr(e, string.Empty);
    }
    public static void require(string e, string extra) {
      throw new mroerr(e, extra);
    }
    public static void require(bool p, string e, string extra) {
      if (p) throw new mroerr(e, extra);
    }
    public static void require(string e, int extra) {
      throw new mroerr(e, extra.ToString());
    }
    public static void require(bool p, string e, int extra) {
      if (p) throw new mroerr(e, extra.ToString());
    }

    /*public static void check(CParameters res) {
      if (res.has(defs.ZSERROR)) err.require(res.get(defs.ZSERROR));
    }*/
    public static void check(mroJSON res) {
      if (res.has(defs.ZSERROR)) err.require(res.get(defs.ZSERROR));
    }

    public static void get_error_desc(object errlock,
                                        Dictionary<string, string> errcache,
                                        string dbcode,
                                        client clie) {
      var desc = string.Empty;
      var extra = string.Empty;
      var translated = string.Empty;
      int descln = clie.result.get(defs.ZSERROR, ref desc);

      /// if we have some desc thats looks like code we try to decode it
      if (descln > 0 && descln < 64 && desc.IndexOf(' ') == -1) {
        try {
          langeng.get_desc(errlock, errcache, dbcode, desc, ref translated, clie.basics);
          if (translated == string.Empty) translated = desc;
        } 
        catch (Exception ex) {
          translated = ex.Message;
        }
      }
      else translated = desc;

      // something different means something was translated
      if (translated.Length != desc.Length ||
          string.CompareOrdinal(translated, desc) != 0) {
        var errinf = new mroJSON();
        clie.result.get(defs.ZERRORI, errinf);
        if (errinf.notempty()) {
          //		errinf.get(defs.ZHERROR, ref extra);
          //		if (extra.Length > 0)
          //			translated = string.Concat(translated, ":", extra);
          errinf.set(defs.ZSERROR, translated);
          clie.result.set(defs.ZERRORI, errinf);
        }
        clie.result.set(defs.ZSERROR, translated);
      }
    }
    public static void manage_exception(client clie,
                                        Exception e,
                                        JSONpool poole) {
      var desc = string.Empty;
      var code = string.Empty;
      var extra = string.Empty;
      var inner = string.Empty;
      var t = new StringBuilder();

      var inf = poole.getclean(0);
      var loc = poole.getclean(1);
      var infi = 0;
      var loci = 0;
      t.Length = 0;
      var formedfromdb = false;

      for (Exception ex = e; ;) {
        bool getout = ex.InnerException == null;

        var mex = ex as mroerr;
        if (mex != null) {
          code = desc = mex.description;
          extra = mex.extrainfo;
        }
        t.Length = 0;
        t.Append("errori"); t.Append(infi++);
        var exmsg = ex.Message.Trim();

        if (utils.isjson(exmsg)) {
          var errp = poole.getclean(2);
          formedfromdb = true;
          inf.set_value(exmsg);
          inf.get("errori0", errp);
          errp.get(defs.ZSERROR, ref desc);
          code = desc = mro.corefuncs.clean_MROFORMAT(desc);
          errp.get(defs.ZHERROR, ref extra);
          extra = mro.corefuncs.clean_MROFORMAT(extra);
          getout = true;
        }
        else inf.set(t.ToString(), exmsg);

        t.Length = 0;
        t.Append("errorl");
        t.Append(loci++);
        var exstack = ex.StackTrace != null ? ex.StackTrace : string.Empty;
        loc.set(t.ToString(), exstack);

        inner = exmsg;

        if (getout) break;
        ex = ex.InnerException;
      }

      if (infi > 0 && !formedfromdb) inf.set(defs.ZNERRIN, infi);
      if (loci > 0) loc.set(defs.ZNERRLO, loci);

      if (string.IsNullOrEmpty(desc)) {
        if (inner.Length > 0) desc = inner;
        else desc = cme.INTERNAL_ERROR;
      }

      // /-----------------------------
      var terr = poole.getclean(3);
      terr.set(defs.ZSERROR, desc);
      terr.set(defs.ZCERROR, code);
      terr.set(defs.ZNERROR, '1');
      terr.set(defs.ZHERROR, extra);
      terr.set(defs.ZERRORM, inf);
      terr.set(defs.ZERRORS, loc);

      var mainerr = poole.getclean(4);
      mainerr.set(defs.ZERRORI, terr);

      // all erro information goes into ZERRORI, but for fast error show 2
      mainerr.set(defs.ZSERROR, desc);
      mainerr.set(defs.ZHERROR, extra);

      clie.result.append(mainerr);
    }
  }
}
