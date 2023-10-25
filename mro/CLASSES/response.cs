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
  public sealed class response {
    public HttpResponse resp = null;
    public StringBuilder flushed = new StringBuilder();
    public bool retresult = true;
    public int saveresult = 0;
    public int nblocks = 0;
    public void init(HttpResponse resp) {
      this.resp = resp;
      flushed.Length = 0;
      retresult = true;
      saveresult = 0;
      nblocks = 0;
    }
    public void start() { response.Write(resp, '{'); }
    public void end() { response.Write(resp, '}'); }

#if NETCOREAPP
      public static void Write(HttpResponse Response, string data) {
         Response.WriteAsync(data);
      }
      public static void Write(HttpResponse Response, char data) {
         Response.WriteAsync(data.ToString());
      }
      unsafe public static void Write(HttpResponse Response, char[] data, int s, int l) {
         fixed (char* dat = data) {
            Response.WriteAsync(new string(dat, s, l));
         }
      }
      public static void Write(HttpResponse Response, StringBuilder data) {
         Response.WriteAsync(data.ToString());
      }
#else
    public static void Write(HttpResponse Response, string data) {
      Response.Write(data);
    }
    public static void Write(HttpResponse Response, char data) {
      Response.Write(data);
    }
    public static void Write(HttpResponse Response, char[] data, int s, int l) {
      Response.Write(data, s, l);
    }
    public static void Write(HttpResponse Response, StringBuilder data) {
      Response.Write(data);
    }
#endif
    public void Write(char data) {
      if (retresult) {
        if (saveresult == 1 || saveresult == 2)
          flushed.Append(data);
        response.Write(resp, data);
        ++nblocks;
      }
    }
    public void Write(string data) {
      if (retresult) {
        if (saveresult == 1 || saveresult == 2)
          flushed.Append(data);
        response.Write(resp, data);
        ++nblocks;
      }
    }
    public void Write(char[] data, int s, int l) {
      if (retresult) {
        if (saveresult == 1 || saveresult == 2)
          flushed.Append(data, s, l);
        response.Write(resp, data, s, l);
        ++nblocks;
      }
    }
    public void Write(char l, string k, char s, string v, char r) {
      if (retresult) {
        if (saveresult == 1 || saveresult == 2) {
          var f = flushed;
          f.Append(l);
          f.Append(k);
          f.Append(s);
          f.Append(v);
          f.Append(r);
        }
        response.Write(resp, l);
        response.Write(resp, k);
        response.Write(resp, s);
        response.Write(resp, v);
        response.Write(resp, r);
        ++nblocks;
      }
    }
    public void Write(StringBuilder l, string k, string s, char v) {
      if (retresult) {
        if (saveresult == 1 || saveresult == 2) {
          var f = flushed;
          f.Append(l);
          f.Append(k);
          f.Append(s);
          f.Append(v);
        }
        response.Write(resp, l);
        response.Write(resp, k);
        response.Write(resp, s);
        response.Write(resp, v);
        ++nblocks;
      }
    }
    public void Write(string l, string k, string s, char v) {
      if (retresult) {
        if (saveresult == 1 || saveresult == 2) {
          var f = flushed;
          f.Append(l);
          f.Append(k);
          f.Append(s);
          f.Append(v);
        }
        response.Write(resp, l);
        response.Write(resp, k);
        response.Write(resp, s);
        response.Write(resp, v);
        ++nblocks;
      }
    }
    public void Write(string l, string k, string s, string v) {
      if (retresult) {
        if (saveresult == 1 || saveresult == 2) {
          var f = flushed;
          f.Append(l);
          f.Append(k);
          f.Append(s);
          f.Append(v);
        }
        response.Write(resp, l);
        response.Write(resp, k);
        response.Write(resp, s);
        response.Write(resp, v);
        ++nblocks;
      }
    }
    public void Write(string l, StringBuilder k, string s, string v) {
      if (retresult) {
        if (saveresult == 1 || saveresult == 2) {
          var f = flushed;
          f.Append(l);
          f.Append(k);
          f.Append(s);
          f.Append(v);
        }
        response.Write(resp, l);
        response.Write(resp, k);
        response.Write(resp, s);
        response.Write(resp, v);
        ++nblocks;
      }
    }
    public void Write(StringBuilder l, string k, string s, string v) {
      if (retresult) {
        if (saveresult == 1 || saveresult == 2) {
          var f = flushed;
          f.Append(l);
          f.Append(k);
          f.Append(s);
          f.Append(v);
        }
        response.Write(resp, l);
        response.Write(resp, k);
        response.Write(resp, s);
        response.Write(resp, v);
        ++nblocks;
      }
    }
    public static void send(response resp, string jsonpair) {
      if (resp.nblocks > 0) resp.Write(',');
      if (jsonpair.IndexOf('\\') == -1)
        resp.Write(jsonpair);
      else
        resp.Write(jsonpair.Replace("\\", "\\\\").Replace("\"", "\\\""));
    }
    public static void send(response resp, string key, StringBuilder val) {
      if (resp.nblocks > 0) resp.Write(@",""");
      else resp.Write('"');
      var value = val.ToString();
      resp.Write(key, @""":""", value.Replace("\\", "\\\\").Replace("\"", "\\\""), '"');
    }
    public static void send(response resp, StringBuilder key, string value) {
      if (resp.nblocks > 0) resp.Write(@",""");
      else resp.Write('"');
      resp.Write(key, @""":""", value.Replace("\\", "\\\\").Replace("\"", "\\\""), '"');
    }
    public static void send(response resp, string key, string value) {
      if (resp.nblocks > 0) resp.Write(@",""");
      else resp.Write('"');
      resp.Write(key, @""":""", value.Replace("\\", "\\\\").Replace("\"", "\\\""), '"');
    }
    public static void send(response resp,
                               StringBuilder key0, string value0,
                               StringBuilder key1, string value1) {
      if (resp.nblocks > 0) resp.Write(@",""");
      else resp.Write('"');
      resp.Write(key0, @""":""", value0, '"');
      resp.Write(@",""");
      resp.Write(key1, @""":""", value1, '"');
    }
    public static void send(response resp,
                               string key0, string value0,
                               string key1, string value1) {
      if (resp.nblocks > 0) resp.Write(@",""");
      else resp.Write('"');
      resp.Write(key0, @""":""", value0, '"');
      resp.Write(@",""");
      resp.Write(key1, @""":""", value1, '"');
    }
    public static void send(response resp,
                               StringBuilder key0, string value0,
                               StringBuilder key1, string value1,
                               StringBuilder key2, string value2) {
      if (resp.nblocks > 0) resp.Write(',');
      resp.Write("\"", key0, "\":\"", value0);
      resp.Write("\",\"");
      resp.Write(key1, "\":\"", value1, "\",\"");
      resp.Write(key2, "\":\"", value2, '"');
    }
    /*public static void send(response resp,
                               string key0, string value0,
                               string key1, string value1,
                               string key2, string value2) {
       if (resp.nblocks > 0) resp.Write(',');
       resp.Write("\"", key0, "\":\"", value0);
       resp.Write("\",\"");
       resp.Write(key1, "\":\"", value1, "\",\"");
       resp.Write(key2, "\":\"", value2, '"');
    }*/
    public static void send(response resp, mroJSON prms) {
      if (prms.isempty()) return;
      var s = prms.get_json_part();
      if (s.IndexOf(defs.ZFILERS) != -1 || s.IndexOf(defs.ZFILE02) != -1)
        s = utils.DeSerializeString(s, new StringBuilder());
      if (resp.nblocks > 0) resp.Write(',');
      resp.Write(s);
    }
  }
}
