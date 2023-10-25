/**
 * Author: Miguel Rodriguez Ojeda
 * 
 * Purpose : This class covers all low level comunication to the C++ Kernel services 
 * 
 * Notes: The Kernel functionallity should be converted to Net Services a this
 * Class its deprecated and at some point disapear
 */

#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;

namespace mro {
  public class atl {
    public static void atlevent(StringBuilder webservice,
                                  StringBuilder hdr,
                                  mroJSON bas,
                                  mroJSON values,
                                  StringBuilder extra) {
      atlevent(webservice, hdr, bas.get_mro(), new CParameters(values), extra);
    }
    public static void atlevent(StringBuilder webservice,
                                  StringBuilder hdr,
                                  string bas,
                                  CParameters values,
                                  StringBuilder extra) {
      if (hdr != null && hdr.Length > 0) webservice.Append(hdr);
      if (bas != null && bas.Length > 0)
        webservice.Append(CParameters.gen_pair(defs.ZBASICS, bas));

      values.append_into(webservice);
      webservice.Append(extra);
    }
    public static void atlservice(StringBuilder webservice,
                                  string hdr,
                                  mroJSON bas,
                                  string fun,
                                  mroJSON values) {
      atlservice(webservice, hdr, bas.get_mro(), fun, new CParameters(values));
    }
    public static void atlservice(StringBuilder webservice,
                                  string hdr,
                                  string bas,
                                  string fun,
                                  CParameters values) {
      if (hdr != null && hdr.Length > 0) webservice.Append(hdr);
      if (bas != null && bas.Length > 0)
        webservice.Append(CParameters.gen_pair(defs.ZBASICS, bas));

      var f00 = new CParameters();
      f00.set(defs.ZTYPCOM, "com");
      f00.set(defs.ZFUNNAM, fun);
      f00.append(values);

      var fns = new CParameters();
      fns.set(defs.ZZNFUNS, "1");
      fns.set(defs.ZFUN00Z, f00);
      fns.append_into(webservice);
    }
    public static void atlservice(StringBuilder webservice,
                                  string hdr,
                                  mroJSON bas,
                                  string fun) {
      atlservice(webservice, hdr, bas.get_mro(), fun);
    }
    public static void atlservice(StringBuilder webservice,
                                  string hdr,
                                  string bas,
                                  string fun) {
      if (hdr != null && hdr.Length > 0) webservice.Append(hdr);
      if (bas != null && bas.Length > 0)
        webservice.Append(CParameters.gen_pair(defs.ZBASICS, bas));

      var f00 = new CParameters();
      f00.set(defs.ZTYPCOM, "com");
      f00.set(defs.ZFUNNAM, fun);

      var fns = new CParameters();
      fns.set(defs.ZZNFUNS, "1");
      fns.set(defs.ZFUN00Z, f00);
      fns.append_into(webservice);
    }
    public static void atlservice(StringBuilder webservice,
                                  string hdr,
                                  string bas,
                                  string fun,
                                  string key0, string val0) {
      if (hdr != null && hdr.Length > 0) webservice.Append(hdr);
      if (bas != null && bas.Length > 0)
        webservice.Append(CParameters.gen_pair(defs.ZBASICS, bas));

      var f00 = new CParameters();
      f00.set(defs.ZTYPCOM, "com");
      f00.set(defs.ZFUNNAM, fun);
      f00.set(key0, val0);

      var fns = new CParameters();
      fns.set(defs.ZZNFUNS, "1");
      fns.set(defs.ZFUN00Z, f00);
      fns.append_into(webservice);
    }
    public static void atlservice(StringBuilder webservice,
                                  string bas,
                                  string fun,
                                  string key0, string val0,
                                  string key1, string val1,
                                  string key2, string val2,
                                  string key3, string val3) {
      if (bas != null && bas.Length > 0)
        webservice.Append(CParameters.gen_pair(defs.ZBASICS, bas));

      var f00 = new CParameters();
      f00.set(defs.ZTYPCOM, "com");
      f00.set(defs.ZFUNNAM, fun);
      f00.set(key0, val0);
      f00.set(key1, val1);
      f00.set(key2, val2);
      f00.set(key3, val3);

      var fns = new CParameters();
      fns.set(defs.ZZNFUNS, "1");
      fns.set(defs.ZFUN00Z, f00);
      fns.append_into(webservice);
    }
    public static void atlservice(StringBuilder webservice,
                            mroJSON bas,
                            string fun,
                            string key0, string val0,
                            string key1, string val1,
                            string key2, string val2,
                            string key3, string val3,
                            string key4, string val4) {
      atlservice(webservice, bas.get_mro(), fun, key0, val0, key1, val1,
         key2, val2, key3, val3, key4, val4);
    }
    public static void atlservice(StringBuilder webservice,
                            string bas,
                            string fun,
                            string key0, string val0,
                            string key1, string val1,
                            string key2, string val2,
                            string key3, string val3,
                            string key4, string val4) {
      if (bas != null && bas.Length > 0)
        webservice.Append(CParameters.gen_pair(defs.ZBASICS, bas));

      var f00 = new CParameters();
      f00.set(defs.ZTYPCOM, "com");
      f00.set(defs.ZFUNNAM, fun);
      f00.set(key0, val0);
      f00.set(key1, val1);
      f00.set(key2, val2);
      f00.set(key3, val3);
      f00.set(key4, val4);

      var fns = new CParameters();
      fns.set(defs.ZZNFUNS, "1");
      fns.set(defs.ZFUN00Z, f00);
      fns.append_into(webservice);
    }
    public static void atlservice(StringBuilder webservice,
                                  mroJSON bas,
                                  string fun,
                                  string key0, string val0,
                                  string key1, string val1,
                                  string key2, string val2) {
      atlservice(webservice, bas.get_mro(), fun, key0, val0, key1, val1, key2, val2);
    }
    public static void atlservice(StringBuilder webservice,
                                  string bas,
                                  string fun,
                                  string key0, string val0,
                                  string key1, string val1,
                                  string key2, string val2) {
      if (bas != null && bas.Length > 0)
        webservice.Append(CParameters.gen_pair(defs.ZBASICS, bas));

      var f00 = new CParameters();
      f00.set(defs.ZTYPCOM, "com");
      f00.set(defs.ZFUNNAM, fun);
      f00.set(key0, val0);
      f00.set(key1, val1);
      f00.set(key2, val2);

      var fns = new CParameters();
      fns.set(defs.ZZNFUNS, "1");
      fns.set(defs.ZFUN00Z, f00);
      fns.append_into(webservice);
    }
  }
}
