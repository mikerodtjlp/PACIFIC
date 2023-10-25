#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Net.Mail;
//using System.Net.Mime;
using System.Reflection;
using System.IO;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Data;
using System.Threading;
using System.Web;
using System.Globalization;

namespace mro {
  public sealed class strbpool {
    public strbpool(int max = 3) {
      this.top = max;
      pool = new StringBuilder[top];
      for (int i = 0; i < top; ++i)
        pool[i] = new StringBuilder();
    }
    /*public StringBuilder pop() {
       if (i < top) return pool[i++];
       err.require("max_pool", i);
       return null;
    }
    public void back() { --i; }*/
    public StringBuilder get(int j) {
      if (j < top) return pool[j];
      err.require("max_pool", j);
      return null;
    }
    public StringBuilder getclean(int j) {
      var it = get(j);
      it.Length = 0;
      return it;
    }
    private StringBuilder[] pool = null;
    private int top = 0;
    private int i = 0;
  }
  public sealed class JSONpool {
    public JSONpool(int max = 6) {
      this.top = max;
      pool = new mroJSON[top];
      for (int i = 0; i < top; ++i)
        pool[i] = new mroJSON();
    }
    /*public mroJSON pop() {
       if (i < top) return pool[i++];
       err.require("max_pool", i);
       return null;
    }
    public void back() { --i; }*/
    public mroJSON get(int j) {
      if (j < top) return pool[j];
      err.require("max_pool", j);
      return null;
    }
    public mroJSON getclean(int j) {
      var it = get(j);
      if (it != null) it.clear();
      return it;
    }
    private mroJSON[] pool = null;
    private int top = 0;
    private int i = 0;
  }
  public class memhelper {
    private string[] str0 = null;
    private string[] str1 = null;
    private string[] str2 = null;
    private string[] str3 = null;
    private bool[] bool0 = null;
    private bool[] bool1 = null;
    private bool[] bool2 = null;
    private int[] int0 = null;
    private int[] int1 = null;
    private char[] char0 = null;
    //private ListResponse lr0 = null;
    private StringBuilder sbl0 = null;
    private StringBuilder sbl1 = null;
    private StringBuilder sbl2 = null;
    private StringBuilder sbl3 = null;

    public string[] getstr0() { if (str0 == null) str0 = new string[128]; return str0; }
    public string[] getstr1() { if (str1 == null) str1 = new string[128]; return str1; }
    public string[] getstr2() { if (str2 == null) str2 = new string[128]; return str2; }
    public string[] getstr3() { if (str3 == null) str3 = new string[128]; return str3; }
    public bool[] getbool0() { if (bool0 == null) bool0 = new bool[128]; return bool0; }
    public bool[] getbool1() { if (bool1 == null) bool1 = new bool[128]; return bool1; }
    public bool[] getbool2() { if (bool2 == null) bool2 = new bool[128]; return bool2; }
    public int[] getint0() { if (int0 == null) int0 = new int[128]; return int0; }
    public int[] getint1() { if (int1 == null) int1 = new int[128]; return int1; }
    public char[] getchr0() { if (char0 == null) char0 = new char[128]; return char0; }
    //public ListResponse () { if (lr0 == null) lr0 = new ListResponse(); return lr0; }
    public StringBuilder getsbl0() {
      if (sbl0 == null) sbl0 = new StringBuilder();
      sbl0.Length = 0;
      return sbl0;
    }
    public StringBuilder getsbl1() {
      if (sbl1 == null) sbl1 = new StringBuilder();
      sbl1.Length = 0;
      return sbl1;
    }
    public StringBuilder getsbl2() {
      if (sbl2 == null) sbl2 = new StringBuilder();
      sbl2.Length = 0;
      return sbl2;
    }
    public StringBuilder getsbl3() {
      if (sbl3 == null) sbl3 = new StringBuilder();
      sbl3.Length = 0;
      return sbl3;
    }
  }

  #region proxy
  public sealed class cpu {
    public enum states { FREE, PROXY, BUSY, EXEC };
  }
  public sealed class funcdata {
    public funcdata(mroJSON info, bool reload) {
      this.info = new mroJSON(info);
      this.reload = reload;
    }
    public mroJSON info = null;
    public bool reload = false;
    public uint reldfns = 0;
    public TimeSpan time = new TimeSpan();
    public uint accesses = 1;
  }
  #endregion

  public sealed class utils {
    /*public static void handle_script(mroJSON result) {
       var s = result.get(defs.ZFILERS);
       result.set(defs.ZFILERS, utils.fix_script(s, 0));
    }*/
    /*public static string fix_script(string s, int pos) {
       s = utils.ReplaceEx(null, s, "\n", "");
       //s = utils.ReplaceEx(null, s, "\r", "");
       s = utils.ReplaceEx(null, s, "\"", "\\\"");
       s = utils.ReplaceEx(null, s, "</", "<\\/");
       s = utils.ReplaceEx(null, s, "/>", " />");
       //s = utils.ReplaceEx(null, s, "\"", "&quot;");
       return s;
    }*/
    public static string gen_filename(string machine, string id, string ext) {
      return string.Format("{0}_{1}{2}.{3}",
         machine, id, Environment.TickCount & Int32.MaxValue, ext);
    }
    public static string gen_destiny(string home, string folder, string filename) {
      return string.Format("{0}{1}\\{2}", home, folder, filename);
    }
    public static void handle_html(mroJSON result) {
      var s = result.get(defs.ZFILERS);
      int pos = s.IndexOf(dhtml.html);
      if (pos != -1) result.set(defs.ZFILERS, utils.fix_html(s, pos));
    }
    public static string handle_html(string s) {
      int pos = s.IndexOf(dhtml.html);
      if (pos != -1) s = utils.fix_html(s, pos);
      return s;
    }

    public static string fix_html(string s, int pos) {
      s = utils.ReplaceEx(null, s, "\n", "");
      s = utils.ReplaceEx(null, s, "\r", "");
      for (int i = 0; i < 32; ++i) {
        int ini = s.IndexOf("\"sqltext\":", pos + 6); // pacific mode
        if (ini == -1) ini = s.IndexOf("\"command\":", pos + 6); // atlantinc mode
        if (ini == -1) break;
        int fin = s.IndexOf('"', ini + 10); // begin dquote
        if (fin == -1) break;
        fin = s.IndexOf('"', fin + 1); // end dquote
        if (fin == -1) break;
        int n = s.Length;
        int end = -1;
        for (int j = fin + 1; j < n; ++j) {
          var c = s[j];
          if (c == ',') { end = j + 1; break; }
          if (c == '}') {
            end = j - 1;
            for (int k = ini - 1; k > 0; --k) {
              var l = s[k];
              if (l == ',') { ini = k; break; }
              if (l == '"' || l == '{') break;
            }
            break;
          }
        }
        if (end == -1) break;
        int todel = (end - ini);// +1;
        s = s.Remove(ini, todel);
      }
      //// "X":["...","..."] - > "X":~"...","..."~
      //s = utils.ReplaceEx(null, s, "\":[", "\":~");
      //s = utils.ReplaceEx(null, s, "\"]", "\"~");     // "]
      //s = utils.ReplaceEx(null, s, "\"}]", "\"}~");   // "}]
      return s;
    }
    public static string[] fnums = { "zfun00z", "zfun01z", "zfun02z", "zfun03z",
                              "zfun04z", "zfun05z", "zfun06z", "zfun07z",
                              "zfun08z", "zfun09z", "zfun10z", "zfun11z",
                              "zfun12z", "zfun13z", "zfun14z", "zfun15z"};
    public static char[] eclc = {   'A', 'B', 'C', 'D', 'E', 'F',
                              'G', 'H', 'I', 'J', 'K', 'L',
                              'M', 'N', 'O', 'P', 'Q', 'R',
                              'S', 'T', 'U', 'V', 'W', 'X',
                              'Y', 'Z'
                             };
    public static string[] ecls = { "A", "B", "C", "D", "E", "F",
                              "G", "H", "I", "J", "K", "L",
                              "M", "N", "O", "P", "Q", "R",
                              "S", "T", "U", "V", "W", "X",
                              "Y", "Z", "AA", "AB", "AC", "AD",
                              "AE", "AF",
                              "AG", "AH", "AI", "AJ", "AK", "AL",
                              "AM", "AN", "AO", "AP", "AQ", "AR",
                              "AS", "AT", "AU", "AV", "AW", "AX",
                              "AY", "AZ"
                             };
    public static string[] sites = { "site0", "site1", "site2", "site3", "site4", "site5",
                              "site6", "site7", "site8", "site9", "site10", "site11",
                              "site12", "site13", "site14", "site15", "site16", "site17",
                              "site18", "site19", "site20", "site21", "site22", "site23",
                              "site24", "site25", "site26", "site27", "site28", "site29",
                              "site30", "site31"
                             };
    public static string[] funs = { "fun0", "fun1", "fun2", "fun3", "fun4", "fun5",
                              "fun6", "fun7", "fun8", "fun9", "fun10", "fun11",
                              "fun12", "fun13", "fun14", "fun15", "fun16", "fun17",
                              "fun18", "fun19", "fun20", "fun21", "fun22", "fun23",
                              "fun24", "fun25", "fun26", "fun27", "fun28", "fun29",
                              "fun30", "fun31"
                             };
    public static string[] prms = { "prms0", "prms1", "prms2", "prms3", "prms4", "prms5",
                              "prms6", "prms7", "prms8", "prms9", "prms10", "prms11",
                              "prms12", "prms13", "prms14", "prms15", "prms16", "prms17",
                              "prms18", "prms19", "prms20", "prms21", "prms22", "prms23",
                              "prms24", "prms25", "prms26", "prms27", "prms28", "prms29",
                              "prms30", "prms31"
                             };
    /*public static string[] cfgs = { "config0", "config1", "config2", "config3", "config4", "config5",
                            "config6", "config7", "config8", "config9", "config10", "config11",
                            "config12", "config13", "config14", "config15", "config16", "config17",
                            "config18", "config19", "config20", "config21", "config22", "config23",
                            "config24", "config25", "config26", "config27", "config28", "config29",
                            "config30", "config31"
                           };*/
    public static string[] repvars = {
                              "repvar0", "repvar1", "repvar2", "repvar3", "repvar4", "repvar5",
                              "repvar6", "repvar7", "repvar8", "repvar9", "repvar10", "repvar11",
                              "repvar12", "repvar13", "repvar14", "repvar15", "repvar16", "repvar17",
                              "repvar18", "repvar19", "repvar20", "repvar21", "repvar22", "repvar23",
                              "repvar24", "repvar25", "repvar26", "repvar27", "repvar28", "repvar29",
                              "repvar30", "repvar31",
                              "repvar32", "repvar33", "repvar34", "repvar35", "repvar36", "repvar37",
                              "repvar38", "repvar39", "repvar40", "repvar41", "repvar42", "repvar43",
                              "repvar44", "repvar45", "repvar46", "repvar47", "repvar48", "repvar49",
                              "repvar50", "repvar51", "repvar52", "repvar53", "repvar54", "repvar55",
                              "repvar56", "repvar57", "repvar58", "repvar59", "repvar60", "repvar61",
                              "repvar62", "repvar63"
                              };
    public static string[] repvars_ext = {
                              "'repvar0'", "'repvar1'", "'repvar2'", "'repvar3'", "'repvar4'", "'repvar5'",
                              "'repvar6'", "'repvar7'", "'repvar8'", "'repvar9'", "'repvar10'", "'repvar11'",
                              "'repvar12'", "'repvar13'", "'repvar14'", "'repvar15'", "'repvar16'", "'repvar17'",
                              "'repvar18'", "'repvar19'", "'repvar20'", "'repvar21'", "'repvar22'", "'repvar23'",
                              "'repvar24'", "'repvar25'", "'repvar26'", "'repvar27'", "'repvar28'", "'repvar29'",
                              "'repvar30'", "'repvar31'",
                              "'repvar32'", "'repvar33'", "'repvar34'", "'repvar35'", "'repvar36'", "'repvar37'",
                              "'repvar38'", "'repvar39'", "'repvar40'", "'repvar41'", "'repvar42'", "'repvar43'",
                              "'repvar44'", "'repvar45'", "'repvar46'", "'repvar47'", "'repvar48'", "'repvar49'",
                              "'repvar50'", "'repvar51'", "'repvar52'", "'repvar53'", "'repvar54'", "'repvar55'",
                              "'repvar56'", "'repvar57'", "'repvar58'", "'repvar59'", "'repvar60'", "'repvar61'",
                              "'repvar62'", "'repvar63'"
                              };
    public static string[] cellvars = {
                              "cell0", "cell1", "cell2", "cell3", "cell4", "cell5",
                              "cell6", "cell7", "cell8", "cell9", "cell10", "cell11",
                              "cell12", "cell13", "cell14", "cell15", "cell16", "cell17",
                              "cell18", "cell19", "cell20", "cell21", "cell22", "cell23",
                              "cell24", "cell25", "cell26", "cell27", "cell28", "cell29",
                              "cell30", "cell31"
                              };
    public static string[] cols =   {
                              "col0", "col1", "col2", "col3", "col4", "col5",
                              "col6", "col7", "col8", "col9", "col10", "col11",
                              "col12", "col13", "col14", "col15", "col16", "col17",
                              "col18", "col19", "col20", "col21", "col22", "col23",
                              "col24", "col25", "col26", "col27", "col28", "col29",
                              "col30", "col31","col32","col33","col34","col35","col36",
                              "col37", "col38","col39","col40","col41","col42","col43",
                              "col44", "col45","col46","col47","col48","col49","col50",
                              "col51","col52","col53","col54","col55","col56","col57",
                              "col58","col59","col60","col61","col62","col63", "col64",
                              "col65", "col66", "col67", "col68", "col69", "col70",
                              "col71", "col72", "col73", "col74", "col75", "col76",
                              "col77", "col78", "col79", "col80", "col81", "col82",
                              "col83", "col84", "col85", "col86", "col87", "col88",
                              "col89", "col90", "col91", "col92", "col93", "col94",
                              "col95", "col96"
                              };
    /*public static string[] fields =   {
                            "field0", "field1", "field2", "field3", "field4", "field5",
                            "field6", "field7", "field8", "field9", "field10", "field11",
                            "field12", "field13", "field14", "field15", "field16", "field17",
                            "field18", "field19", "field20", "field21", "field22", "field23",
                            "field24", "field25", "field26", "field27", "field28", "field29",
                            "field30", "field31"
                            };*/
    public static string[] zhis = {
                              "zhis00z", "zhis01z", "zhis02z", "zhis03z", "zhis04z", "zhis05z",
                              "zhis06z", "zhis07z", "zhis08z", "zhis09z", "zhis10z", "zhis11z",
                              "zhis12z", "zhis13z", "zhis14z", "zhis15z", "zhis16z", "zhis17z",
                              "zhis18z", "zhis19z", "zhis20z", "zhis21z", "zhis22z", "zhis23z",
                              "zhis24z", "zhis25z", "zhis26z", "zhis27z", "zhis28z", "zhis29z",
                              "zhis30z", "zhis31z"
                              };

    public static string to_std_date(DateTime obj) {
      return string.Format("{0:yyyy/MM/dd HH:mm:ss}", obj);
    }
    public static string to_std_date(object obj) {
      if (obj == DBNull.Value) return string.Empty;
      return string.Format("{0:yyyy/MM/dd HH:mm:ss}", (DateTime)obj);
    }
    public static string date_part(DateTime obj) {
      return string.Format("{0:yyyy/MM/dd}", obj);
    }
    public static string date_part(object obj) {
      if (obj == DBNull.Value) return string.Empty;
      return string.Format("{0:yyyy/MM/dd}", (DateTime)obj);
    }
    public static string hour_part(DateTime time) {
      //	return string.Format("{0:HH:mm:ss}", obj);
      char[] d = new char[8];
      d[0] = (char)(time.Hour / 10 + '0');
      d[1] = (char)(time.Hour % 10 + '0');
      d[2] = ':';
      d[3] = (char)(time.Minute / 10 + '0');
      d[4] = (char)(time.Minute % 10 + '0');
      d[5] = ':';
      d[6] = (char)(time.Second / 10 + '0');
      d[7] = (char)(time.Second % 10 + '0');
      return new string(d);
    }
    public static string hour_part(object obj) {
      if (obj == DBNull.Value) return string.Empty;
      return string.Format("{0:HH:mm:ss}", (DateTime)obj);
    }
    public static bool IsFileLocked(FileInfo file) {
      FileStream stream = null;
      try {
        stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
      } 
      catch (IOException) {
        return true;
      } 
      finally {
        if (stream != null) stream.Close();
      }
      return false;
    }
    public static bool isjson(string tocheck) {
      var s = tocheck.Trim();
      var l = s.Length;
      if (l == 2 && s[0] == '{' && s[1] == '}') return true;
      if (l > 2 && s[0] == '{' && s[1] == '\"') return true;
      return false;
    }
    public static bool isparm(string tocheck) {
      var s = tocheck.Trim();
      return s.Length > 0 && s[0] == '[';
    }
    public static void json2mro(StringBuilder src, StringBuilder data) {
      int n = src.Length;
      data.Length = 0;
      int ins = 1;
      int brks = 0;
      for (int i = 0; i < n; ++i) {
        char l = src[i];
        if (l == '{') {
          if ((i + 1 < n) && src[i + 1] == '"') { // start key
            brks++;
            ++ins; data.Append('['); ++i; continue;
          }
        }
        if (l == '"') {
          if ((i + 1 < n) && src[i + 1] == ':') { // end key
            if ((i + 2 < n) && src[i + 2] == '"') {
              data.Append(':'); i += 2; continue;
            }
            if ((i + 2 < n) && src[i + 2] == '{') {
              brks++;
              data.Append(':'); ++i;
              if ((i + 2 < n) && src[i + 2] == '}') {
                brks--;
                data.Append(']'); i += 2;
                if ((i + 1 < n) && src[i + 1] == ',')
                  ++i;
              }
              continue;
            }
            if ((i + 2 < n) && char.IsDigit(src[i + 2])) {
              data.Append(':');
              data.Append(src[i + 2]); i += 2;
              if (char.IsDigit(src[i + 1])) {
                data.Append(src[i + 1]); ++i;
                if (char.IsDigit(src[i + 1])) {
                  data.Append(src[i + 1]); ++i;
                  if (char.IsDigit(src[i + 1])) {
                    data.Append(src[i + 1]); ++i;
                    if (char.IsDigit(src[i + 1])) {
                      data.Append(src[i + 1]); ++i;
                    }
                  }
                }
              }
              if ((i + 1 < n) && src[i + 1] == ',') {
                data.Append(']'); ++i;
              }
              if ((i + 1 < n) && src[i + 1] == '}') {
                data.Append(']'); ++i; brks--;
              }
              if ((i + 1 < n) && src[i + 1] == '}') {
                data.Append(']'); ++i; brks--;
              }
              if ((i + 1 < n) && src[i + 1] == '}') {
                data.Append(']'); ++i; brks--;
              }
              if ((i + 1 < n) && src[i + 1] == '}') {
                data.Append(']'); ++i; brks--;
              }
              if ((i + 1 < n) && src[i + 1] == ',') { // end compose object and there comes another
                data.Append(']'); ++i; continue; // simply we advance that coma
              }
              //continue;
            }
          }
          if (i + 1 < n) {
            if (src[i + 1] == '}') { // end compose object
              --ins; data.Append(']'); --ins;
              if (ins > 0) data.Append(']');
              ++i;

              if (i + 1 < n && src[i + 1] == ',') { // end compose object and there comes another
                ++i; continue; // simply we advance that coma
              }
              if (i + 1 < (n - 1) && src[i + 1] == '}') { // end compose object and there comes another
                --ins; data.Append(']'); ++i; // simply we advance that coma
              }
              if (i + 1 < n && src[i + 1] == ',') { // end compose object and there comes another
                ++i; continue; // simply we advance that coma
              }
              continue;
            }
            if (src[i + 1] == ',') { // end simple object
              --ins; data.Append(']'); ++i; continue;
            }
            if (src[i - 1] == ',') { // start simple object
              ++ins; data.Append('['); continue;
            }
          }
        }
        if (l == '}') {
          brks--;
          --ins;
          if (char.IsDigit(src[i - 1])) {
            data.Append(']'); continue;
          }
        }
        if (ins == 0) continue;
        if (brks == 0) continue;
        data.Append(l);
      }
    }
    public static string mro2json(CParameters p,
                                  StringBuilder pd,
                                  bool checkhtml) {
      var s = p.get_data_json_values(); // FIX values only
      if (s.Length == 0) return s;

      //super parche
      if (checkhtml && (s.IndexOf(defs.ZFILERS) != -1 ||
                        s.IndexOf(defs.ZFILE02) != -1)) {
        s = utils.ReplaceEx(null, s, ":~", ":[");
        s = utils.ReplaceEx(null, s, "\"~", "\"]");
        s = utils.ReplaceEx(null, s, "\"}~", "\"}]");
      }

      return s;
    }
    public static void separte_address(string full,
                               ref string addr,
                               ref int port) {
      int i = full.IndexOf(':');
      if (i == -1) { addr = string.Empty; port = 0; return; }
      addr = full.Substring(0, i);
      port = utils.IntParseFast(full.Substring(i + 1));
    }

    /**
   *  here is a nasty problem that arise when work with the 127.0.0.1
   *  address so we must detected in order to take actions on it
   */
    public static bool is_127001(string ip) {
      return ip.Length == 9 &&
            string.CompareOrdinal(ip, mro.mrosocket.LOCADDR) == 0;
    }
    public static string getlocalip() {
      string localIP = "?";
      /*IPHostEntry host;
   host = Dns.GetHostEntry(Dns.GetHostName());
   var addrlist = host.AddressList;
   foreach (IPAddress ip in addrlist)
   {
      if (ip.AddressFamily.ToString() == "InterNetwork")
      {
         localIP = ip.ToString();
         break;
      }
   }*/
      using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
        socket.Connect("8.8.8.8", 65530);
        IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
        localIP = endPoint.Address.ToString();
      }
      return localIP;
    }
    public static char[] work(int nblanks) {
      char[] r = new char[(nblanks * 5) + 1];
      int rl = 0;
      for (int i = 0; i < nblanks; ++i) {
        r[rl++] = '&'; r[rl++] = 'n';
        r[rl++] = 'b'; r[rl++] = 's';
        r[rl++] = 'p';
      }
      return r;
    }
    // at this moment works only for positive numbers
    public static int IntParseFast(string value) {
      int result = 0;
      for (int i = 0; i < value.Length; i++) {
        result = 10 * result + (value[i] - 48);
      }
      return result;
    }
    public static string DeSerializeString(string s, StringBuilder bl) {
      /*s = utils.ReplaceEx(null, s, "\\\"", "\"");
        s = utils.ReplaceEx(null, s, "\\\\", "\\");
        s = utils.ReplaceEx(null, s, "\\b", "\b");
        s = utils.ReplaceEx(null, s, "\\f", "\f");
        s = utils.ReplaceEx(null, s, "\\t", "\t");
        s = utils.ReplaceEx(null, s, "\\n", "\n");
        s = utils.ReplaceEx(null, s, "\\r", "\r");
         return s;*/

      bl.Length = 0;
      char[] charArray = s.ToCharArray();
      for (int i = 0; i < charArray.Length; i++) {
        char c = charArray[i];
        bl.Append(c);
        int j = bl.Length;

        if (j > 1) {
          char a0 = bl[j - 2];
          char a1 = bl[j - 1];

          if (a0 == '\\') {
            if (a1 == '\"') {
              bl.Length -= 2;
              bl.Append('\"');
            }
            else if (a1 == '\\') {
              bl.Length -= 2;
              bl.Append('\\');
            }
            else if (a1 == 'b') {
              bl.Length -= 2;
              bl.Append('\b');
            }
            else if (a1 == 'f') {
              bl.Length -= 2;
              bl.Append('\f');
            }
            else if (a1 == 'n') {
              bl.Length -= 2;
              bl.Append('\n');
            }
            else if (a1 == 'r') {
              bl.Length -= 2;
              bl.Append('\r');
            }
            else if (a1 == 't') {
              bl.Length -= 2;
              bl.Append('\t');
            }
            else if (c == 'u') { // unicode
              bl.Length -= 2;
              uint codePoint;
              if (UInt32.TryParse(new string(charArray, i + 1, 4),
                                      NumberStyles.HexNumber,
                                      CultureInfo.InvariantCulture,
                                      out codePoint)) {
                bl.Append(Char.ConvertFromUtf32((int)codePoint));
                i += 4;
              }
            }
          }
        }
      }

      return bl.ToString();
    }
    public static string ReplaceEx(char[] buffer,
                                     string original,
                                     string pattern,
                                     string replacement) {
      int count, position0, position1;
      count = position0 = position1 = 0;
      string upperString = original;// original.ToUpper();
      string upperPattern = pattern;// pattern.ToUpper();
      int inc = (original.Length / pattern.Length) *
              (replacement.Length - pattern.Length);
      int size = original.Length + Math.Max(0, inc);
      char[] chars = buffer == null || size > buffer.Length ?
                  new char[original.Length + Math.Max(0, inc)] : buffer;
      while ((position1 = upperString.IndexOf(upperPattern, position0)) != -1) {
        for (int i = position0; i < position1; ++i)
          chars[count++] = original[i];
        for (int i = 0; i < replacement.Length; ++i)
          chars[count++] = replacement[i];
        position0 = position1 + pattern.Length;
      }
      if (position0 == 0) return original;
      for (int i = position0; i < original.Length; ++i)
        chars[count++] = original[i];
      return new string(chars, 0, count);
    }
    /*public static bool SetUseUnsafeHeaderParsing(bool b)
  {
    Assembly a = Assembly.GetAssembly(typeof(System.Net.Configuration.SettingsSection));
    if (a == null) return false;

    Type t = a.GetType("System.Net.Configuration.SettingsSectionInternal");
    if (t == null) return false;

    object o = t.InvokeMember("Section",
      BindingFlags.Static | BindingFlags.GetProperty | BindingFlags.NonPublic, null, null, new object[] { });
    if (o == null) return false;
    FieldInfo f = t.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
    if (f == null) return false;

    f.SetValue(o, b);
    return true;
  }*/
  }

  public sealed class sdata {
    public string proxysvr;
    public string proxyprt;
    public string proxypat; // pending....
    public string mac;
    public string header;
    public string basics;
    public string parms;
    public string func;
    public string site;
    public string page;

    public string function;
    public string folder;
    public string newname;

    public bool isempty = true;
  }

  public sealed class corefuncs {
    /*[HttpPost("UploadFiles")]
    public async Task<IActionResult> Post(List<IFormFile> files) {
       long size = files.Sum(f => f.Length);

       // full path to file in temp location
       var filePath = Path.GetTempFileName();

       foreach (var formFile in files) {
          if (formFile.Length > 0) {
             using (var stream = new FileStream(filePath, FileMode.Create)) {
                await formFile.CopyToAsync(stream);
             }
          }
       }

       // process uploaded files
       // Don't rely on or trust the FileName property without validation.

       return Ok(new { count = files.Count, size, filePath });
    }*/

    /**
     * if we lose result, we create new one for next calling
     */
    public static void check_client(client clie, mroJSON values, mroJSON result) {
      if (result == null || result.isnull())
        result = clie.result = new mroJSON();
      if (values == null || values.isnull())
        values = clie.values = new mroJSON();

    }
    public static string save_it( HttpContext Context,
                                  string folder,
                                  string newfilename) {
      var res = string.Empty;
#if NETCOREAPP
#else
      HttpFileCollection fc = Context.Request.Files;
       try {
          // Get the HttpFileCollection
          for (var i = 0; i < fc.Count; ++i) {
             HttpPostedFile hpf = fc[i];
             if (hpf.ContentLength > 0) {
                try {
                   var fn = newfilename.Length > 0 ? newfilename :
                     System.IO.Path.GetFileName(hpf.FileName);
                   var a = Context.Server.MapPath("");
                   res = a + "\\" + folder.Replace("\\", "\\\\") + "\\" + fn;
                   hpf.SaveAs(res);
                }
                catch (Exception e) { }
             }
          }
       }
       catch (Exception e) { }
#endif
      return res;
    }

    public static void reset_rights_cache(object olock,
                                           Dictionary<string, mroJSON> cache,
                                           string id) {
      var l = new List<string>();
      lock (olock) {
        if (id.Length == 0) cache.Clear();
        else {
          foreach (var f in cache) {
            if (f.Key.Length <= id.Length) continue; // to short to be a candidate
            int i = f.Key.IndexOf(id, 0, id.Length);
            l.Add(f.Key);
          }
          foreach (var i in l) cache.Remove(i);
        }
      }
    }

    public static void reset_funs_cache(object olock,
                                           Dictionary<string, MethodInfo> cache,
                                           string id) {
      var l = new List<string>();
      lock (olock) {
        if (id.Length == 0) cache.Clear();
        else {
          foreach (var f in cache) {
            if (f.Key.Length <= id.Length) continue; // to short to be a candidate
            int i = f.Key.IndexOf(id, 0, id.Length);
            l.Add(f.Key);
          }
          foreach (var i in l) cache.Remove(i);
        }
      }
    }

    /** 
   * not always is possible to get the rights, for example when we are 
     * entering, we dont know who we are, when we are in the main screen 
     * a check rights for entering to a new transaction(we are not in that 
     * transaction), other example would be satellites systems that do they 
     * work a little diferent
   */
    public static void look_rights(client clie,
                                     string dbcode,
                                     mroJSON basics,
                                     object olock,
                                     Dictionary<string, mroJSON> cache,
                                     mroJSON outrights) {
      var cmpy = basics.get(defs.ZCOMPNY);
      if (cmpy.Length == 0) return;
      var user = basics.get(defs.ZUSERID);
      if (user.Length == 0) return;
      var trans = basics.get(defs.ZTRNCOD);
      if (trans.Length == 0) return;
      var code = mem.join2(user, trans);

      mroJSON rights = null;

      lock (olock) {
        if (cache.TryGetValue(code, out rights)) {
          if (rights == null || rights.isempty()) cache.Remove(code);
          else {
            rights.clone_to(outrights); return;
          }
        }
        var q = string.Concat("exec dbo.rights_get_process ",
           clie.zsesins, ",", clie.zsesmac, ",",
           clie.zsescli, ",", clie.zsesses, ",",
           cmpy, ",'", user, "','", trans, "';");
        using (var conn = new SqlConnection(dbcode)) {
          var cmd = new SqlCommand(q, conn);
          conn.Open();
          var r = cmd.ExecuteScalar();
          var res = r == null ? string.Empty : r.ToString();
          rights = new mroJSON(res);
        }
        cache.Add(code, rights);
      }

      if (rights == null) outrights.clear();
      else rights.clone_to(outrights);
    }

    /**
   * the constant process is simple, get rid of the "^" mark of every 
     * value entry
   */
    static public void process_constants(mroJSON tprms,
                                           mroJSON prms) {
      mroJSON changes = null;
      var sprms = string.Empty;
      var parms = prms.get_dict();
      foreach (DictionaryEntry pair in parms) {
        var pKey = pair.Key.ToString();
        var pVal = pair.Value is string ? pair.Value.ToString() :
                         new mroJSON((Hashtable)pair.Value).get_json();

        int pattern = -1;
        sprms = pVal;
        int left = sprms.Length;
        bool found = false;
        while ((pattern = sprms.IndexOf('^', pattern + 1)) != -1) {
          found = true;
          sprms = sprms.Remove(pattern, 1);
        }
        if (found) {
          if (changes == null) {
            changes = tprms;
            changes.clear();
          }
          if (mroJSON.isObj(sprms)) changes.set(pKey, new mroJSON(sprms));
          else changes.set(pKey, sprms);
        }
      }
      if (changes != null) prms.replace_from(changes);
    }
    static public void update_addresses(char[] buffer,
                                           mroJSON addresses,
                                           ref string whole) {
      var addrs = addresses.get_dict();
      foreach (DictionaryEntry p in addrs) {
        whole = utils.ReplaceEx(buffer, whole,
                                p.Key.ToString(), p.Value.ToString());
      }
    }
    /*static public int repl_values(client clie,         // wrk
                                  mroJSON tprms,       // wrk
                                  mroJSON prms,        // out
                                  CParameters values,  // inp
                                  mroJSON notempty) {  // inp
      var v = new mroJSON(values);
      var c = repl_values(clie, tprms, prms, v, notempty);
      return c;
    }*/
    /*static public int repl_values(client clie,         // wrk
                                  mroJSON tprms,       // wrk
                                  CParameters prms,    // out
                                  mroJSON values,      // inp
                                  mroJSON notempty) {  // inp
      var p = new mroJSON(prms);
      var c = repl_values(clie, tprms, p, values, notempty);
      prms.set_value(p);
      return c;
    }*/
    /** 
     * the replace values replace the values's name from the function with 
     * the real values within the values object
     */
    static public int repl_values(client clie,         // out
                                  mroJSON tprms,       // wrk
                                  mroJSON prms,        // out
                                  mroJSON values,      // inp
                                  mroJSON notempty) {  // inp
      var nconstvals = 0;
      if (prms.notempty()) {
        mroJSON changes = null;
        var vals = values.get_dict();
        var coms = prms.get_dict();
        foreach (DictionaryEntry pcom in coms) {
          var comKey = pcom.Key.ToString();
          var sprms = pcom.Value is string ? pcom.Value.ToString() :
                           new mroJSON((Hashtable)pcom.Value).get_json();
          var once = false;
          var sprmslen = sprms.Length;
          foreach (DictionaryEntry entry in vals) {
            int pattern = -1;

            var pKey = entry.Key.ToString();
            var pVal = entry.Value is string ? entry.Value.ToString() :
                             new mroJSON((Hashtable)entry.Value).get_json();

            while ((pattern + 1 < sprmslen) &&
                  (pattern = sprms.IndexOf(pKey, pattern + 1)) != -1) {
              if (pattern > 0 && sprms[pattern - 1] == '^') ++nconstvals;
              else {
                if (notempty != null) {//check if should not be empty
                  var inc_dat = notempty.get(pKey);
                  if (inc_dat.Length != 0 && pVal.Length == 0) {
                    clie.result.set(defs.ZINCFLD, pKey);
                    err.require(inc_dat);
                  }
                }
                sprms = sprms.Remove(pattern, pKey.Length);
                sprms = sprms.Insert(pattern, pVal);
                sprmslen = sprms.Length;
                pattern += pVal.Length;

                if (changes == null) {
                  changes = tprms;
                  changes.clear();
                }
                if (utils.isjson(sprms))
                  changes.set(comKey, new mroJSON(sprms));
                else changes.set(comKey, sprms);
                once = mem._tmemcmp(pKey, defs.PFLDTXT);
              }
            }
            if (once) break; // ugly break to change the value only once
          }
        }
        if (changes != null) prms.replace_from(changes);
      }
      return nconstvals;
    }

    /**
     * Note: the functions expects that the first row are the FIELDS names
     */
    private static void excel_to_values(client clie,
                                          mroJSON values,
                                          mroJSON fun_params,
                                          string file) {
      err.require(file.Length == 0, cme.FILE_UPLOADED_EMPTY);
      err.require(!File.Exists(file), cme.FILE_NOT_UPLOADED, file);
      values.clear();

      //string strConn = string.Concat("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=",
      //                file, ";Extended Properties=\"Excel 8.0;HDR=Yes;\"");

      var xls = true; // old format
      if (file.IndexOf(".xlsx") != -1) xls = false; //new format
      string strConn = !xls ? // xlsx
          mem.join3("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=", file,
              ";Extended Properties=\"Excel 12.0 Xml;HDR=Yes;\"") :
          mem.join3("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=", file,
              ";Extended Properties=\"Excel 8.0;HDR=Yes;\"");

      OleDbConnection oleConn = new OleDbConnection(strConn);

      try {
        oleConn.Open();
        var query = new StringBuilder();
        query.Length = 0;
        query.Append("select * from [Sheet1$]");
        var cmd = new OleDbCommand();
        cmd.Connection = oleConn;
        cmd.CommandText = query.ToString();
        var ada = new OleDbDataAdapter(cmd);
        var tbl = new DataSet();
        ada.Fill(tbl);

        var dt = tbl.Tables[0];
        var nrows = dt.Rows.Count;
        var ncols = dt.Columns.Count;
        DataRow dr = null;
        var v = "";
        var c = "";
        var rw = dt.Rows;
        for (int i = 0; i < nrows; ++i) {
          dr = rw[i];
          char col = 'A';
          for (int j = 0; j < ncols; ++j, ++col) {
            v = dr[j].ToString();
            c = col.ToString() + (i + 1).ToString();
            //////////
            //TEMP FIX
            v = v.Replace("'", "");
            //v=v.Replace(":","");
            v = v.Replace("[", "");
            v = v.Replace("]", "");
            v = v.Replace("&", "");
            //////////
            values.set(c, v);
          }
        }
        values.set(defs.ZEXETOT, nrows);
        values.set(defs.ZEXECLS, ncols);
        fun_params.set(defs.ZEXEDAT, values);

        tbl.Dispose();
        ada.Dispose();
        cmd.Dispose();
        oleConn.Close();
        oleConn.Dispose();
      } 
      finally {
        oleConn.Close();
      }
    }
    public static void proc_params(client clie) {
      // extract the function parameters
      var tmpp = clie.poolj.get(0);
      var nconsts = 0;
      var fun_params = clie.poolj.get(2);
      var websvr = clie.webservice;
      websvr.extract(defs.ZPARAMS, fun_params);
      var notempty = clie.poolj.get(3);
      var donoem = fun_params.get(defs.PNOEMPT, notempty) > 0;

      // some results come from basics like the part added for the server
      var usebasics = fun_params.ison(defs.ZUSEBAS);

      // process the values that we have to return 
      if (clie.hasretprms = websvr.has(defs.RETPRMS)) {
        var ret_prms = clie.poolj.get(1); //clie.ret_prms;
        websvr.extract(defs.RETPRMS, ret_prms);
        if (usebasics) {
          nconsts = corefuncs.repl_values(clie,
                                           tmpp,
                                           ret_prms,
                                           clie.basics,
                                           null);
          if (nconsts > 0) corefuncs.process_constants(tmpp, ret_prms);
          // we dont need to compact the fun_params as on the C++ server
        }
        // we must send the RETPRMS because is the site who returns it and
        // unlike C++ who keep it on a variable that is used to return it
        websvr.set(defs.RETPRMS, ret_prms);
      }

      nconsts = 0;
      if (clie.workonvalues) {
        // replacing the values
        if (fun_params.isempty() == false) {
          if (usebasics)
            nconsts = corefuncs.repl_values(clie,
                                             tmpp,
                                             fun_params,
                                             clie.basics,
                                             null);
          nconsts = corefuncs.repl_values(clie,
                                           tmpp,
                                           fun_params,
                                           clie.values,
                                           donoem ? notempty : null);
        }
        // we must send the values because on the pacific version there is a lot of
        // code that read the values and not the values from the params unlike C++ code
        else websvr.append(clie.values);

        //FIX
        var vals = clie.values;
        if (vals.has(defs.ZLSTDAT))
          fun_params.set(defs.ZLSTDAT, new mroJSON(vals.get(defs.ZLSTDAT)));
        if (vals.has(defs.ZEXEDAT))
          fun_params.set(defs.ZEXEDAT, new mroJSON(vals.get(defs.ZEXEDAT)));
        if (vals.has("exc2vals"))
          excel_to_values(clie, tmpp, fun_params, vals.get("fileuploaded"));
        if (vals.has("usefilename"))
          fun_params.set("uploadfilename", vals.get("fileuploaded"));
        if (vals.has(defs.ZTXTDAT))
          fun_params.set(defs.ZTXTDAT, new mroJSON(vals.get(defs.ZTXTDAT)));
        if (nconsts > 0) corefuncs.process_constants(tmpp, fun_params);
        // we dont need to compact the fun_params as on the C++ server
      }
      // we must send the fun_params because is the site who returns it and
      // unlike C++ who keep it on a variable that is used to return it
      websvr.append(fun_params);
    }
    public static string EscapeStringValue(string value) {
      const char BACK_SLASH = '\\';
      const char SLASH = '/';
      const char DBL_QUOTE = '"';

      var output = new StringBuilder(value.Length);
      foreach (char c in value) {
        switch (c) {
          case SLASH: output.AppendFormat("{0}{1}", BACK_SLASH, SLASH); break;
          case BACK_SLASH: output.AppendFormat("{0}{0}", BACK_SLASH); break;
          case DBL_QUOTE: output.AppendFormat("{0}{1}", BACK_SLASH, DBL_QUOTE); break;
          default: output.Append(c); break;
        }
      }

      return output.ToString();
    }
    public static string clean_qry(string q) {
      return EscapeStringValue(q);

    }
    public static string clean_MROFORMAT(string q) {
      q = EscapeStringValue(q);
      return q.Replace('[', '|').Replace(']', '|').Replace('"', '\'').Replace('\\', '/');
    }

    public static string getmac(HttpContext context) {
#if NETCOREAPP
         var ip = context.Connection.RemoteIpAddress.ToString();
         var local = context.Connection.LocalIpAddress.ToString();
#else
      var ip = context.Request.UserHostAddress;
#endif
      // FIX the localhost name problem
      if (ip.Length == 3 && ip[0] == ':' && ip[1] == ':' && ip[2] == '1')
        ip = mro.mrosocket.LOCADDR;
      return ip;
    }

    /**
     * we get POST/Form and GET/Query although more webservices are POST 
     * but when rapid testing user uses browser url bar and it comes as GET
     */
    public static sdata extract_data(HttpRequest Request,
                                     bool proxy) {

      var data = new sdata();
#if NETCOREAPP
         var isPOST = Request.Method == method.POST;
#else
      var isPOST = Request.HttpMethod == method.POST;
#endif

      // we pass the values of the url into the data objet, this is because
      // the data/values can come from get or from post or from both, so in
      // order to keep it simple and safe we process both into one object:data
      if (isPOST) {
        var prms = Request.Form;
        if (!proxy) {
          data.proxysvr = prms["proxysvr"];
          data.proxyprt = prms["proxyprt"];
        }
        data.mac = prms["mac"];
        data.header = prms["hdr"];
        data.basics = prms["bas"];
        data.parms = HttpUtility.UrlDecode(prms["prm"]);
        data.func = prms["fun"];
        if (proxy) {
          data.site = prms["server"];
          data.page = prms["page"];
          data.function = prms["function"];
          data.folder = prms["folder"];
          data.newname = prms["newname"];
        }
        data.isempty = prms.Count == 0;
      }
      else {
#if NETCOREAPP
            var prms = Request.Query;
#else
        var prms = Request.QueryString;
#endif
        if (!proxy) {
          data.proxysvr = prms["proxysvr"];
          data.proxyprt = prms["proxyprt"];
        }
        data.mac = prms["mac"];
        data.header = prms["hdr"];
        data.basics = prms["bas"];
        data.parms = HttpUtility.UrlDecode(prms["prm"]);
        data.func = prms["fun"];
        if (proxy) {
          data.site = prms["server"];
          data.page = prms["page"];
          data.function = prms["function"];
          data.folder = prms["folder"];
          data.newname = prms["newname"];
        }
        data.isempty = prms.Count == 0;
      }

      if (!proxy) {
        if (data.proxysvr == null) data.proxysvr = string.Empty;
        if (data.proxyprt == null) data.proxyprt = string.Empty;
      }
      if (data.mac == null) data.mac = string.Empty;
      if (data.header == null) data.header = string.Empty;
      if (data.basics == null) data.basics = string.Empty;
      if (data.parms == null) data.parms = string.Empty;
      if (data.func == null) data.func = string.Empty;
      if (proxy) {
        if (data.site == null) data.site = string.Empty;
        if (data.page == null) data.page = string.Empty;
        if (data.function == null) data.function = string.Empty;
        if (data.folder == null) data.folder = string.Empty;
        if (data.newname == null) data.newname = string.Empty;
      }

      return data;
    }
    public static void ses_save_query(link lnk,
                                        BL.control_BL bl,
                                        int seq,
                                        string qry,
                                        string websrv) {
      // we make a copy cause if let the clie.log and cli.basics alone maybe in 
      // a new shotthey change while we have a reference to them and thats not good
      ThreadPool.QueueUserWorkItem(new WaitCallback((Object stateInfo) => {
        try {
          bl.ses_save_query(lnk, seq, qry, websrv);
        } 
        catch (Exception e) { }
      }));
    }

    public static void savelog(string target,
                               mroJSON basics,
                               mroJSON log) {
      if (log.isempty()) return;
      int nlogs = 0;
      bool alone = false;
      if (log.has(defs.ZZNLOGS)) nlogs = log.getint(defs.ZZNLOGS);
      if (nlogs == 0) {
        alone = true;
        nlogs = 1;
      }

      var txt = string.Empty;
      var key = string.Empty;
      var typ = string.Empty;
      var sav = true;

      var cmpy = string.Empty; basics.get(defs.ZCOMPNY, ref cmpy);
      var tran = string.Empty; basics.get(defs.ZTRNCOD, ref tran);
      var user = string.Empty; basics.get(defs.ZUSERID, ref user);
      var mach = string.Empty; basics.get(defs.ZMACNAM, ref mach);

      var k = string.Empty;
      var qry = new StringBuilder();

      for (int i = 0; i < nlogs; ++i) {
        if (i == 0 && alone) {
          log.get(defs.ZTXTLOG, ref txt);
          log.get(defs.ZKEYLOG, ref key);
          log.get(defs.ZTYPLOG, ref typ);
          if (log.has_val(defs.ZSAVLOG))
            sav = log.getbool(defs.ZSAVLOG);
        }
        else {
          k = defs.ZTXTLOG; k += i; log.get(k, ref txt);
          k = defs.ZKEYLOG; k += i; log.get(k, ref key);
          k = defs.ZTYPLOG; k += i; log.get(k, ref typ);
          k = defs.ZSAVLOG; k += i;
          if (log.has_val(k))
            sav = log.getbool(k);
        }

        if (sav)
          qry.AppendFormat("exec dbo.insert_log " +
                         "{0},'{1}','{2}','{3}','XXX','{4}','{5}','{6}'; ",
                         cmpy, user, mach, tran, txt, key, typ);
      }
      if (qry.Length > 0) {
        using (var conn = new SqlConnection(target)) {
          var cmd = new SqlCommand(qry.ToString(), conn);
          conn.Open();
          cmd.ExecuteNonQuery();
        }
      }
    }
    public static void save_log(string target,
                                  mroJSON log,
                                  mroJSON bas) {
      // we make a copy cause if let the clie.log and cli.basics alone maybe in 
      // a new shotthey change while we have a reference to them and thats not good
      ThreadPool.QueueUserWorkItem(new WaitCallback((Object stateInfo) => {
        try {
          corefuncs.savelog(target, new mroJSON(bas), new mroJSON(log));
        } 
        catch (Exception e) { }
      }));
    }
  }

  static class Extensions {
    public static void set(this StringBuilder sb, char value) {
      sb.Length = 0;
      sb.Append(value);
    }
    public static void set(this StringBuilder sb, string value) {
      sb.Length = 0;
      sb.Append(value);
    }
    public static void set(this StringBuilder sb, StringBuilder value) {
      sb.Length = 0;
      sb.Append(value);
    }
    public static int IndexOf(this StringBuilder sb, string value, int startIndex, bool ignoreCase) {
      int index;
      int length = value.Length;
      int maxSearchLength = (sb.Length - length) + 1;

      if (ignoreCase) {
        for (int i = startIndex; i < maxSearchLength; ++i) {
          if (Char.ToLower(sb[i]) == Char.ToLower(value[0])) {
            index = 1;
            while ((index < length) && (Char.ToLower(sb[i + index]) == Char.ToLower(value[index])))
              ++index;

            if (index == length)
              return i;
          }
        }

        return -1;
      }

      for (int i = startIndex; i < maxSearchLength; ++i) {
        if (sb[i] == value[0]) {
          index = 1;
          while ((index < length) && (sb[i + index] == value[index]))
            ++index;

          if (index == length)
            return i;
        }
      }

      return -1;
    }
    public static void Digits(this StringBuilder builder, int number) {
      if (number >= 100000000) {
        // Use system ToString.
        builder.Append(number.ToString());
        return;
      }
      if (number < 0) {
        // Negative.
        builder.Append(number.ToString());
        return;
      }
      int copy;
      int digit;
      if (number >= 10000000) {
        // 8.
        copy = number % 100000000;
        digit = copy / 10000000;
        builder.Append((char)(digit + 48));
      }
      if (number >= 1000000) {
        // 7.
        copy = number % 10000000;
        digit = copy / 1000000;
        builder.Append((char)(digit + 48));
      }
      if (number >= 100000) {
        // 6.
        copy = number % 1000000;
        digit = copy / 100000;
        builder.Append((char)(digit + 48));
      }
      if (number >= 10000) {
        // 5.
        copy = number % 100000;
        digit = copy / 10000;
        builder.Append((char)(digit + 48));
      }
      if (number >= 1000) {
        // 4.
        copy = number % 10000;
        digit = copy / 1000;
        builder.Append((char)(digit + 48));
      }
      if (number >= 100) {
        // 3.
        copy = number % 1000;
        digit = copy / 100;
        builder.Append((char)(digit + 48));
      }
      if (number >= 10) {
        // 2.
        copy = number % 100;
        digit = copy / 10;
        builder.Append((char)(digit + 48));
      }
      if (number >= 0) {
        // 1.
        copy = number % 10;
        digit = copy / 1;
        builder.Append((char)(digit + 48));
      }
    }
    static string[] _cache = {
   "0","1","2","3","4","5","6","7","8","9",
   "10","11","12","13","14","15","16","17","18","19",
   "20","21","22","23","24","25","26","27","28","29",
   "30","31","32","33","34","35","36","37","38","39",
   "40","41","42","43","44","45","46","47","48","49",
   "50","51","52","53","54","55","56","57","58","59",
   "60","61","62","63","64","65","66","67","68","69",
   "70","71","72","73","74","75","76","77","78","79",
   "80","81","82","83","84","85","86","87","88","89",
   "90","91","92","93","94","95","96","97","98","99",
   "100","101","102","103","104","105","106","107","108","109",
   "110","111","112","113","114","115","116","117","118","119",
   "120","121","122","123","124","125","126","127","128","129",
   "130","131","132","133","134","135","136","137","138","139",
   "140","141","142","143","144","145","146","147","148","149",
   "150","151","152","153","154","155","156","157","158","159",
   "160","161","162","163","164","165","166","167","168","169",
   "170","171","172","173","174","175","176","177","178","179",
   "180","181","182","183","184","185","186","187","188","189",
   "190","191","192","193","194","195","196","197","198","199"
    };

    const int _top = 199;
    public static string tostr(this int value) {
      if (value >= 0 && value <= _top) return _cache[value];
      return value.ToString();
    }
  }
}
