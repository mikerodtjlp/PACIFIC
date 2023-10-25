#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Web;

namespace mro {
  public sealed class mroJSON {

    /**
     * constrcutors 
     */
    public mroJSON() { }
    public mroJSON(string values) { set_value(values); }
    public mroJSON(string key, string val) { set(key, val); }
    public mroJSON(StringBuilder values) { set_value(values); }
    public mroJSON(mroJSON prms) { set_value(prms.get_json()); }
    public mroJSON(Hashtable values) { _rep = (Hashtable)values.Clone(); }
    public mroJSON(CParameters values) {
      set_value(mem.json(utils.mro2json(values, null, false)));
    }
    public mroJSON(string[] arr) {
      for (int i = 0; i < arr.Length; i += 2) set(arr[i], arr[i + 1]);
    }

    /**
     * deleting
     */
    public void clear() { _rep.Clear(); }
    public void del(string key) { _rep.Remove(key); }

    /**
     * set values
     */
    public void set_value(StringBuilder values) {
      set_value(values.ToString());
    }
    public void set_value(mroJSON prms) {
      set_value(prms.get_json());
    }
    public void set_value(CParameters values) {
      set_value(mem.json(utils.mro2json(values, null, false)));
    }
    public void set_value(Hashtable values) {          // CORE ONE
      _rep = (Hashtable)values.Clone();
    }
    public void set_value(string values) {
      _rep = (Hashtable)mro.JSON.JsonDecode(values); // CORE ONE
    }
    /** 
     * checkings
     */
    public bool notempty() { return _rep.Count > 0; }
    public bool isempty() { return _rep.Count == 0; }
    public bool isnull() { return _rep == null; }
    public bool ison(string key) {
      string v = (string)_rep[key];
      if (v != null)
        if (v.Length == 1 && v[0] == '1') return true;
      return false;
    }
    public int nkeys() { return _rep.Count; }
    public bool has(string key) { return _rep.ContainsKey(key); }
    public bool has(StringBuilder key) { return _rep.ContainsKey(key.ToString()); }
    public bool has_val(string key) {
      var v = _rep[key];
      if (v == null) return false;
      if (v is string) return ((string)v).Length > 0;
      if (v is Hashtable) return ((Hashtable)v).Count > 0;
      return false;
    }
    public bool has_val(StringBuilder key) { return has_val(key.ToString()); }
    public bool has_val_of(string key, string tocmp) {
      var v = _rep[key];
      if (v == null) return false;
      if (v is string) {
        var val = (string)v;
        return val.Length == tocmp.Length &&
                 string.CompareOrdinal(val, tocmp) == 0;
      }
      return false;
    }
    public static bool isObj(string value) {
      for (int i = 0, n = value.Length; i < n; ++i) {
        var c = value[i];
        if (c == '{') return true;
        if (c == '"') return false;
      }
      return false;
    }

    /**
     * switchs
     */
    public void on(string key) { set(key, "1"); }
    public void off(string key) { set(key, "0"); }

    /**
     * copies
     */
    /*public void copyto(string key, CParameters dest, string keydest) {
       var val = string.Empty;
       if (get(key, ref val) == 0) dest.del(keydest);
       else dest.set(keydest, val);
    }*/
    public void copyto(string key, mroJSON dest, string keydest) {
      var val = string.Empty;
      if (get(key, ref val) == 0) dest.del(keydest);
      else dest.set(keydest, val);
    }
    public void replace_from(mroJSON values) {
      append(values);
    }
    /*public void append(CParameters prms) {
       var values = new mroJSON(prms);
       if (values == null) return;
       foreach (DictionaryEntry p in values.get_dict()) {
          if (p.Value is string)
             set(p.Key.ToString(), p.Value.ToString());
          if (p.Value is Hashtable)
             set(p.Key.ToString(), (Hashtable)p.Value);
       }
    }*/
    public void append(mroJSON values) {
      if (values == null) return;
      foreach (DictionaryEntry p in values.get_dict()) {
        if (p.Value is string)
          set(p.Key.ToString(), p.Value.ToString());
        if (p.Value is Hashtable)
          set(p.Key.ToString(), (Hashtable)p.Value);
      }
    }
    public void clone_to(mroJSON dest) {
      if (dest == null) return;
      var tar = dest.get_dict();
      tar.Clear();
      foreach (DictionaryEntry p in _rep) {
        if (p.Value is string)
          tar.Add(p.Key.ToString(), p.Value.ToString());
        if (p.Value is Hashtable)
          tar.Add(p.Key.ToString(), ((Hashtable)p.Value).Clone());
      }
    }

    /**
     * getters
     */
    public string get(string key) {
      var v = _rep[key];
      if (v != null && v is string) return (string)v;
      return string.Empty; // for objects should use getobj
    }
    public string get(StringBuilder key) {
      var v = _rep[key.ToString()];
      if (v != null && v is string) return (string)v;
      return string.Empty; // for objects should use getobj
    }
    public int get(string key, ref string val) {
      var v = _rep[key];
      if (v != null && v is string) val = (string)v; // we are good
      else val = string.Empty;
      return val.Length;
    }
    public int get(string key, StringBuilder res) {
      res.Length = 0;
      var v = _rep[key];
      if (v != null && v is string) res.Append((string)v); // we are good
      return res.Length;
    }
    /*public int get(string key, char[] res) {
       int reslen = 0;
       var v = _rep[key];
       if (v != null && v is string) {
          var val = (string)v;
          reslen = val.Length;
          for (int i = 0; i < reslen; ++i) res[i] = val[i];
       }
       return reslen;
    }*/
    public int get(string key, mroJSON dest) {
      var v = (Hashtable)_rep[key];
      if (v != null) dest.set_value(v);
      else dest.clear();
      return dest.nkeys();
    }
    /*public int get(string key, CParameters dest) {
       var value = getobj(key);
       dest.clear();
       dest.append(value);
       return value.nkeys();
    }*/
    public DateTime getdate(string key) {
      var val = string.Empty;
      get(key, ref val);
      DateTime dval;
      if (DateTime.TryParse(val, out dval)) return dval;
      return DateTime.MinValue;
    }
    public int getint(string key, int def = 0) {
      var value = _rep[key];
      if (value != null) {
        if (value is string) {
          var v = (string)value;
          if (v.Length != 0) {
            int ival;
            if (int.TryParse(v, out ival)) return ival;
          }
        }
        if (JSON.IsNumeric(value)) return (int)Convert.ToDouble(value);
      }
      return def;
    }
    public bool getbool(string key) {
      string value = (string)_rep[key];
      if (value == null || value.Length == 0) return false;
      if (value.Length == 1) {
        if (value[0] == '1') return true;
        if (value[0] == '0') return false;
      }
      else return bool.Parse(value);
      return false;
    }
    public bool getbool(string key, bool def) {
      string value = (string)_rep[key];
      if (value == null || value.Length == 0) return def;
      if (value.Length == 1) {
        if (value[0] == '1') return true;
        if (value[0] == '0') return false;
      }
      else return bool.Parse(value);
      return def;
    }
    public mroJSON getobj(string key) {
      var dest = new mroJSON();
      var raw = _rep[key];
      if (raw == null) return dest;
      if (raw is string) return dest;
      var v = (Hashtable)_rep[key];
      if (v != null) dest.set_value(v);
      return dest;
    }
    /**
     * get json complete {"begin":"end",....."end":"end"}
     */
    public string get_json() {
      var json = mro.JSON.JsonEncode(_rep);
      var s = utils.ReplaceEx(null, json, "\", \"", "\",\"");
      s = utils.ReplaceEx(null, s, "}, \"", "},\"");
      return s;
    }
    /**
     * get json as a part "begin":"end",....."end":"end"
     */
    public string get_json_part() {
      var s = get_json();
      if (s.Length != 0) s = s.Substring(1, s.Length - 2);
      return s;
    }
    public string get_mro() {
      var json = mro.JSON.JsonEncode(_rep);
      var s = utils.ReplaceEx(null, json, "\", \"", "\",\"");
      s = utils.ReplaceEx(null, s, "}, \"", "},\"");

      var t0 = new StringBuilder(s);
      var t1 = new StringBuilder();
      utils.json2mro(t0, t1);
      return t1.ToString();
    }
    public int extractint(string key) {
      string value = get(key);
      if (value != null && value.Length != 0) {
        int ival;
        del(key);
        if (int.TryParse(value, out ival)) return ival;
      }
      return 0;
    }
    /*public void extract(string key, CParameters dest) {
       var value = getobj(key);
       dest.clear();
       dest.append(value);
       del(key);
    }*/
    public string extract(string key) {
      var value = get(key);
      del(key);
      return value;
    }
    public string extract(StringBuilder key) {
      var k = key.ToString();
      var value = get(k);
      del(k);
      return value;
    }
    public int extract(string key, mroJSON dest) {
      var value = getobj(key);
      dest.clear();
      dest.append(value);
      del(key);
      return dest.nkeys();
    }

    /*
     * setters
     */
    public void set(string key, DateTime value) {
      set(key, utils.to_std_date(value));
    }
    public void set(string key, StringBuilder value) {
      set(key, value.ToString());
    }
    public void set(string key, string value) {
      if (_rep.ContainsKey(key)) {
        if (value != null) _rep[key] = value;
        else _rep[key] = string.Empty;
      }
      else {
        if (value != null) _rep.Add(key, value);
        else _rep.Add(key, string.Empty);
      }
    }
    public void set(string key, int value) {
      string prev = get(key);
      if (prev != null) {
        int temp;
        if (int.TryParse(prev, out temp)) { // could be a string
          if (value != temp) _rep[key] = value.tostr();
        }
        else _rep[key] = value.tostr();
      }
      else _rep.Add(key, value.tostr());
    }
    public void set(string key, Hashtable value) {
      if (_rep.ContainsKey(key)) {
        if (value != null)
          _rep[key] = (Hashtable)value.Clone();
        else _rep[key] = null;
      }
      else {
        if (value != null)
          _rep.Add(key, (Hashtable)value.Clone());
        else _rep.Add(key, null);
      }
    }
    public void set(string key, mroJSON value) {
      if (value == null) return;
      set(key, value.get_dict());
    }
    /*public void set(string key, CParameters value) {
       if (value == null) return;
       set(key, new mroJSON(value));
    }*/
    /*public void clone_to(CParameters dest) {
       if (dest == null) return;
       var tar = dest.get_dict();
       tar.Clear();
       foreach (DictionaryEntry p in _rep) {
          if (p.Value is string)
             tar.Add(p.Key.ToString(), p.Value.ToString());
          if (p.Value is Hashtable) {
             var j = new mroJSON((Hashtable)p.Value);
             tar.Add(p.Key.ToString(), j.get_mro());
          }
       }
    }*/
    public Hashtable get_dict() { return _rep; }
    private Hashtable _rep = new Hashtable();
  }
  public sealed class CParameters {
    public CParameters() { }
    public CParameters(string values) { ins_values(values, 0, values.Length); }
    //public CParameters(StringBuilder values) { ins_values(values); }
    //public CParameters(char[] values) { ins_values(values, values.Length); }
    //public CParameters(CParameters prms) { append(prms); }
    public CParameters(mroJSON prms) { append(prms); }
    //public CParameters(string key, string value) { set(key, value); }
    //public CParameters(string key, CParameters value) { set(key, value); }
    //public CParameters(string[] arr) {
    //  for (int i = 0; i < arr.Length; i += 2) set(arr[i], arr[i + 1]);
    //}
    /*public void parse(StringBuilder json) { parse(json.ToString()); }*/
    /*public void parse(string json) {
       _rep.Clear();
       JObject o = JObject.Parse(json);
       foreach (var x in o) {
          var a = x.Value.ToString(Newtonsoft.Json.Formatting.None);
          if (a.Length >= 2 && a[0] == '"') a = a.Substring(1, a.Length - 2);
          _rep[x.Key] = a;
       }
       isjson = true;
    }*/
    public void set_value(string values) {
      _rep.Clear();
      ins_values(values, 0, values.Length);
    }
    /*public void set_value(mroJSON prms) {
      _rep.Clear();
      append(prms);
    }
    public void set_value(string values, int start, int count) {
      _rep.Clear();
      ins_values(values, start, count);
    }*/
    private unsafe void ins_values(string values, int start, int count) {
      isjson = false;
      int total = start + count;

      int lefts = 0;
      int seps = 0;
      int rights = 0;

      int s = 0;
      int e = 0;
      int u = 0;
      int d = 0;

      bool inkey = false;
      bool inval = false;
      bool found = false;
      fixed (char* p = values) {
        for (int i = start; i < total; ++i) {
          char car = *(p + i);
          if (car == LEFT) {
            if (++lefts == 1) {
              inkey = true;
              inval = false;
              s = i + 1;
              continue;
            }
          }
          else
              if (car == SEP) {
            if (++seps == 1) {
              inkey = false;
              inval = true;
              e = i;
              u = i + 1;
              continue;
            }
          }
          else if (car == RIGHT) {
            ++rights;
            found = rights == lefts;
            if (found) d = i;
          }

          if (inval && found) {
            if (s != 0 && u != 0)
              _rep[new string(p, s, e - s)] = new string(p, u, d - u);
            lefts = seps = rights = 0;
            found = inkey = inval = false;
            s = e = u = d = 0;
          }
        }
      }
    }

    public void set_value(StringBuilder values) {
      _rep.Clear();
      ins_values(values);
    }
    private unsafe void ins_values(StringBuilder values) {
      isjson = false;
      int total = values.Length;

      int lefts = 0;
      int seps = 0;
      int rights = 0;

      int s = 0;
      int e = 0;
      int u = 0;
      int d = 0;

      bool inkey = false;
      bool inval = false;
      bool found = false;
      for (int i = 0; i < total; ++i) {
        char car = values[i];
        if (car == LEFT) {
          if (++lefts == 1) {
            inkey = true;
            inval = false;
            s = i + 1;
            continue;
          }
        }
        else
            if (car == SEP) {
          if (++seps == 1) {
            inkey = false;
            inval = true;
            e = i;
            u = i + 1;
            continue;
          }
        }
        else if (car == RIGHT) {
          ++rights;
          found = rights == lefts;
          if (found) d = i;
        }

        if (inval && found) {
          if (s != 0 && u != 0)
            _rep[values.ToString(s, e - s)] = values.ToString(u, d - u);
          lefts = seps = rights = 0;
          found = inkey = inval = false;
          s = e = u = d = 0;
        }
      }
    }
    /*
    public void set_value(string key, string value) {
      _rep.Clear();
      set(key, value);
    }
    public void set_value(CParameters values) {
      _rep.Clear();
      append(values);
    }
    public void set_value(char[] values) {
      _rep.Clear();
      ins_values(values, values.Length);
    }*/
    public void set_value(char[] values, int l) {
      _rep.Clear();
      ins_values(values, l);
    }

    private unsafe void ins_values(char[] values, int total) {
      isjson = false;
      fixed (char* p = values) {
        int lefts = 0;
        int seps = 0;
        int rights = 0;

        int s = 0;
        int e = 0;
        int u = 0;
        int d = 0;

        bool inkey = false;
        bool inval = false;
        bool found = false;
        for (int i = 0; i < total; ++i) {
          char car = *(p + i);
          if (car == LEFT) {
            if (++lefts == 1) {
              inkey = true;
              inval = false;
              s = i + 1;
              continue;
            }
          }
          else
              if (car == SEP) {
            if (++seps == 1) {
              inkey = false;
              inval = true;
              e = i;
              u = i + 1;
              continue;
            }
          }
          else if (car == RIGHT) {
            ++rights;
            found = rights == lefts;
            if (found) d = i;
          }

          if (inval && found) {
            if (s != 0 && u != 0)
              _rep[new string(p, s, e - s)] = new string(p, u, d - u);
            lefts = seps = rights = 0;
            found = inkey = inval = false;
            s = e = u = d = 0;
          }
        }
      }
    }
    public void append(mroJSON prms) {
      if (prms == null || prms.nkeys() == 0) return; // nothing to append
      foreach (DictionaryEntry p in prms.get_dict()) {
        if (p.Value is string)
          set(p.Key.ToString(), p.Value.ToString());
        if (p.Value is Hashtable) {
          var j = new mroJSON((Hashtable)p.Value);
          set(p.Key.ToString(), j.get_mro());
        }
      }
    }
    public void append(CParameters prms) {
      if (prms == null || prms.nkeys() == 0) return; // nothing to append
      foreach (KeyValuePair<string, string> p in prms._rep)
        set(p.Key, p.Value);
    }
    /*public void append(string values) { ins_values(values, 0, values.Length); }
    public void append(StringBuilder values) { ins_values(values); }
    public void append(char[] values) { ins_values(values, values.Length); }

    public CParameters clonex() {
      var dest = new CParameters();
      var drep = dest._rep;
      foreach (KeyValuePair<string, string> entry in _rep)
        drep.Add(entry.Key, entry.Value);
      return dest;
    }
    public void clone_to(CParameters dest) {
      if (dest == null) return;
      var drep = dest._rep;
      drep.Clear();
      foreach (KeyValuePair<string, string> entry in _rep)
        drep.Add(entry.Key, entry.Value);
    }
    public void clone_to(mroJSON dest) {
      if (dest == null) return;
      var drep = dest.get_dict();
      drep.Clear();
      foreach (KeyValuePair<string, string> entry in _rep)
        drep.Add(entry.Key, entry.Value);
    }*/

    public string get_data() {
      if (nkeys() == 0) return string.Empty;
      _val.Length = 0;
      foreach (KeyValuePair<string, string> par in _rep) {
        _val.Append(LEFT);
        _val.Append(par.Key);
        _val.Append(SEP);
        _val.Append(par.Value);
        _val.Append(RIGHT);
      }
      return _val.ToString();
    }
    unsafe public string get_data_json_values() {
      if (nkeys() == 0) return string.Empty;
      _val.Length = 0;

      foreach (KeyValuePair<string, string> par in _rep) {
        var k = par.Key;
        var s = par.Value;
        _val.Append("\"");
        _val.Append(par.Key);
        _val.Append("\":\"");

        // xfile01 and xfile02 not processed
        if (k.Length == 7 && mem._tmemcmp(k, "xfile01") || mem._tmemcmp(k, "xfile02")) {
          /*k[0] == 'x' && k[1] == 'f' && k[2] == 'i' &&
          k[3] == 'l' && k[4] == 'e' && k[5] == '0' &&
          (k[6] == '1' || k[6] == '2')) {*/
          _val.Append(s);
          _val.Append("\",");
          continue;
        }

        char c = ' ';
        int ln = s.Length;
        for (int i = 0; i < ln; ++i) {
          c = s[i];
          if (char.IsWhiteSpace(c) == false) break;
        }
        if (c == '[') // embedded object
        {
          s = s.Trim();
          var p = new CParameters(s);
          s = p.get_data_json_values();
          --_val.Length; // remove " for simple object
          _val.Append('{');
          _val.Append(s);
          _val.Append("},");
        }
        else {
          int len = s.Length;
          var sl = false;
          var dq = false;
          var tb = false;
          var nl = false;
          var rl = false;
          //var right = false;
          //var left = false;
          fixed (char* p = s) {
            for (int i = 0; i < len; ++i) {
              char car = *(p + i);
              if (car == '\\') sl = true;
              else if (car == '\"') dq = true;
              else if (car == '\t') tb = true;
              else if (car == '\n') nl = true;
              else if (car == '\r') rl = true;
              //else if (car == '[') left = true;
              //else if (car == ']') right = true;
            }
          }
          if (sl) s = utils.ReplaceEx(null, s, "\\", "\\\\");
          if (dq) s = utils.ReplaceEx(null, s, "\"", "\\\"");
          if (tb) s = utils.ReplaceEx(null, s, "\t", "\\t");
          if (nl) s = utils.ReplaceEx(null, s, "\n", "\\n");
          if (rl) s = utils.ReplaceEx(null, s, "\r", "\\r");
          //if (left) s = utils.ReplaceEx(null, s, "[", "**{**");
          //if (right) s = utils.ReplaceEx(null, s, "]", "**}**");
          _val.Append(s);
          _val.Append("\",");
        }
      }
      if (_val[_val.Length - 1] == ',') --_val.Length;
      return _val.ToString();
    }
    public void append_into(StringBuilder result) {
      if (nkeys() == 0) return;
      if (result == null) return;
      foreach (KeyValuePair<string, string> par in _rep) {
        result.Append(LEFT);
        result.Append(par.Key);
        result.Append(SEP);
        result.Append(par.Value);
        result.Append(RIGHT);
      }
    }
    public void set(string key, string value) {
      string prev = null;
      if (_rep.TryGetValue(key, out prev)) {
        if (value != null) _rep[key] = value;
        else _rep[key] = string.Empty;
      }
      else {
        if (value != null) _rep.Add(key, value);
        else _rep.Add(key, string.Empty);
      }
    }
    /*public void set(string key, StringBuilder value) {
      string prev = null;
      if (_rep.TryGetValue(key, out prev)) {
        if (value != null) _rep[key] = value.ToString();
        else _rep[key] = string.Empty;
      }
      else {
        if (value != null) _rep.Add(key, value.ToString());
        else _rep.Add(key, string.Empty);
      }
    }*/
    public void set(string key, int value) {
      string prev = null;
      if (_rep.TryGetValue(key, out prev)) {
        int temp;
        if (int.TryParse(prev, out temp)) // could be a string
        {
          if (value != temp) _rep[key] = value.tostr();
        }
        else _rep[key] = value.tostr();
      }
      else _rep.Add(key, value.tostr());
    }
    public void set(string key, CParameters value) {
      if (value == null) return;
      set(key, value.get_data());
    }
    /*
    public void set(string key, mroJSON value) {
      if (value == null) return;
      set(key, value.get_mro());
    }
    public void set(string key, DateTime value) {
      string prev = null;
      if (_rep.TryGetValue(key, out prev)) {
        if (value != null) _rep[key] = utils.to_std_date(value);
        else _rep[key] = string.Empty;
      }
      else {
        if (value != null) _rep.Add(key, utils.to_std_date(value));
        else _rep.Add(key, string.Empty);
      }
    }

    // compares
    public bool are_eq(string k, string v) {
      return string.CompareOrdinal(get(k), v) == 0;
    }

    // getters
    public int get(StringBuilder key, ref string value) {
      if (_rep.TryGetValue(key.ToString(), out value) && value != null)
        return value.Length;
      value = string.Empty;
      return 0;
    }
    public int get(string key, CParameters res) {
      if (res == null) return 0;
      string value = string.Empty;
      int l = 0;
      if (_rep.TryGetValue(key, out value) && value != null) {
        l = value.Length;
        res.set_value(value);
      }
      else res.clear();
      return l;
    }
    public mroJSON getjson(string key) {
       var j = new mroJSON();
       get(key, j);
       return j;
    }
    public int get(string key, mroJSON res) {
      if (res == null) return 0;
      string value = string.Empty;
      int l = 0;
      if (_rep.TryGetValue(key, out value) && value != null) {
        l = value.Length;
        res.set_value(mem.json(new CParameters(value).get_data_json_values()));
      }
      else res.clear();
      return l;
    }
    public int get(string key, StringBuilder res) {
      if (res == null) return 0;
      string value = string.Empty;
      int l = 0;
      res.Length = 0;
      if (_rep.TryGetValue(key, out value) && value != null) {
        l = value.Length;
        res.Append(value);
      }
      return l;
    }
    public int get(string key, char[] res) {
      if (res == null) return 0;
      string value = string.Empty;
      int l = 0;
      if (_rep.TryGetValue(key, out value) && value != null) {
        l = value.Length;
        for (int i = 0; i < l; ++i) res[i] = value[i];
      }
      return l;
    }
    public string get(string key) {
      string value;
      if (_rep.TryGetValue(key, out value) && value != null) return value;
      return string.Empty;
    }*/
    public int get(string key, ref string value) {
      if (_rep.TryGetValue(key, out value) && value != null) return value.Length;
      value = string.Empty;
      return 0;
    }

    /*public void getappend(string key, CParameters parameters,
                                        CParameters pivot) {
      get(key, pivot);
      parameters.append(pivot);
    }

    public int getint(string key) {
      string value;
      if (_rep.TryGetValue(key, out value)) {
        int ival;
        if (int.TryParse(value, out ival)) return ival;
      }
      return 0;
    }
    public int getint(string key, int def) {
      string value;
      if (_rep.TryGetValue(key, out value)) {
        int ival;
        if (int.TryParse(value, out ival)) return ival;
      }
      return def;
    }
    public bool getbool(string key) {
      string value;
      if (_rep.TryGetValue(key, out value)) {
        if (value.Length == 1) {
          if (value[0] == '1') return true;
          if (value[0] == '0') return false;
        }
        return bool.Parse(value);
      }
      return false;
    }
    public bool getbool(string key, bool def) {
      string value;
      if (_rep.TryGetValue(key, out value)) {
        if (value.Length == 1) {
          if (value[0] == '1') return true;
          if (value[0] == '0') return false;
        }
        return bool.Parse(value);
      }
      return def;
    }
    public DateTime getdate(string key) {
      var val = string.Empty;
      get(key, ref val);
      DateTime dval;
      if (DateTime.TryParse(val, out dval)) return dval;
      return DateTime.MinValue;
    }

    public string get_pair(string key) {
      var value = get(key);
      var v = _val;

      int klen = key.Length;
      int vlen = value.Length;
      if (v.Capacity < klen + vlen + 3) v.Capacity = klen + vlen + 3;
      v.Length = klen + vlen + 3;
      int l = 0;
      v[l] = LEFT; ++l;
      if (klen == 7) {
        v[l++] = key[0]; v[l++] = key[1]; v[l++] = key[2];
        v[l++] = key[3]; v[l++] = key[4]; v[l++] = key[5]; v[l++] = key[6];
      }
      else for (int i = 0; i < klen; ++i, ++l) v[l] = key[i];
      v[l] = SEP; ++l;
      for (int i = 0; i < vlen; ++i, ++l) v[l] = value[i];
      v[l] = RIGHT; ++l;
      return v.ToString();
    }*/

    /*public void pass_pair(string key, ref string tar) {
       var value = get(key);
       var v = _val;

       int klen = key.Length;
       int vlen = value.Length;
       if (v.Capacity < klen + vlen + 3) v.Capacity = klen + vlen + 3;
       v.Length = klen + vlen + 3;
       int l = 0;
       v[l] = LEFT; ++l;
       if (klen == 7) {
          v[l++] = key[0]; v[l++] = key[1]; v[l++] = key[2];
          v[l++] = key[3]; v[l++] = key[4]; v[l++] = key[5]; v[l++] = key[6];
       }
       else for (int i = 0; i < klen; ++i, ++l) v[l] = key[i];
       v[l] = SEP; ++l;
       for (int i = 0; i < vlen; ++i, ++l) v[l] = value[i];
       v[l] = RIGHT; ++l;
       string.Concat(tar, v.ToString());
    }*/
    /*public void pass_pair(string key, StringBuilder tar) {
      tar.Append(LEFT);
      tar.Append(key);
      tar.Append(SEP);
      tar.Append(get(key));
      tar.Append(RIGHT);
    }*/

    public void clear() { _rep.Clear(); }
    //public void del(string key) { _rep.Remove(key); }

    /*public int extract(string key, mroJSON prms) {
      int len = 0;
      if ((len = get(key, prms)) != 0)
        del(key);
      return len;
    }
    public int extract(string key, CParameters prms) {
      int len = 0;
      if ((len = get(key, prms)) != 0)
        _rep.Remove(key);
      return len;
    }*/
    public int extract(string key, ref string prms) {
      int len = 0;
      if ((len = get(key, ref prms)) != 0)
        _rep.Remove(key);
      return len;
    }
    /*public int extractint(string key) {
      string value;
      if (_rep.TryGetValue(key, out value)) {
        _rep.Remove(key);
        int ival;
        if (int.TryParse(value, out ival)) return ival;
      }
      return 0;
    }

    public bool isempty() { return nkeys() == 0; }*/
    public bool has(string key) { return _rep.ContainsKey(key); }
    /*public void on(string key) { set(key, "1"); }
    public void off(string key) { set(key, "0"); }
    public bool istrue(string key) {
      string v;
      if (_rep.TryGetValue(key, out v)) {
        var l = v.Length;
        if (l == 1 && v[0] == '1') return true;
        if (l == 4 && v[0] == 't' && v[1] == 'r' && v[2] == 'u' && v[3] == 'e') return true;
      }
      return false;
    }
    public bool ison(string key) {
      string v;
      if (_rep.TryGetValue(key, out v))
        if (v.Length == 1 && v[0] == '1') return true;
      return false;
    }
    public bool has_val(string key) {
      string value;
      if (_rep.TryGetValue(key, out value)) {
        if (value == null) return false;
        return value.Length > 0;
      }
      return false;
    }
    public void copyto(string key, CParameters dest, string keydest) {
      var val = string.Empty;
      if (get(key, ref val) == 0) dest.del(keydest);
      else dest.set(keydest, val);
    }
    public void copyto(string key, mroJSON dest, string keydest) {
      var val = string.Empty;
      if (get(key, ref val) == 0) dest.del(keydest);
      else dest.set(keydest, val);
    }

    public void replace_from(string values) {
      if (string.IsNullOrEmpty(values)) return;
      replace_from(new CParameters(values));
    }
    public void replace_from(CParameters values) {
      if (values == null) return;
      foreach (KeyValuePair<string, string> par in values._rep)
        set(par.Key, par.Value);
    }
    public void replace_from(mroJSON values) {
      if (values == null) return;
      foreach (DictionaryEntry p in values.get_dict()) {
        if (p.Value is string)
          set(p.Key.ToString(), p.Value.ToString());
        if (p.Value is Hashtable) {
          var j = new mroJSON((Hashtable)p.Value);
          set(p.Key.ToString(), j.get_mro());
        }
      }
    }

    public static void gen_pair(string key, StringBuilder value, StringBuilder r) {
      int klen = key.Length;
      int vlen = value.Length;
      r.Length = klen + vlen + 3;
      int l = 0;
      r[l] = LEFT; ++l;
      if (klen == 7) {
        r[l++] = key[0]; r[l++] = key[1]; r[l++] = key[2];
        r[l++] = key[3]; r[l++] = key[4]; r[l++] = key[5]; r[l++] = key[6];
      }
      else for (int i = 0; i < klen; ++i, ++l) r[l] = key[i];
      r[l] = SEP; ++l;
      for (int i = 0; i < vlen; ++i, ++l) r[l] = value[i];
      r[l] = RIGHT; ++l;
    }*/
    public static string gen_pair(string key, string value) {
      var sb = new StringBuilder();
      return gen_pair(key, value, sb);
    }

    public static string gen_pair(string key, string value, StringBuilder r) {
      int klen = key.Length;
      int vlen = value.Length;
      r.Length = klen + vlen + 3;
      int l = 0;
      r[l] = LEFT; ++l;
      if (klen == 7) {
        r[l++] = key[0]; r[l++] = key[1]; r[l++] = key[2];
        r[l++] = key[3]; r[l++] = key[4]; r[l++] = key[5]; r[l++] = key[6];
      }
      else for (int i = 0; i < klen; ++i, ++l) r[l] = key[i];
      r[l] = SEP; ++l;
      for (int i = 0; i < vlen; ++i, ++l) r[l] = value[i];
      r[l] = RIGHT; ++l;
      return r.ToString();
    }
    //public static bool isObj(string value) {
    //  return value.Length > 0 && value[0] == LEFT;
    //}

    public int nkeys() { return _rep.Count; }
    //public Dictionary<string, string> get_dict() { return _rep; }

    private Dictionary<string, string> _rep = new Dictionary<string, string>();
    private StringBuilder _val = new StringBuilder();
    private bool isjson = false;

    public const char LEFT = '[';
    public const char SEP = ':';
    public const char RIGHT = ']';
  }

  /// <summary>
  /// This class encodes and decodes JSON strings.
  /// Spec. details, see http://www.json.org/
  /// 
  /// JSON uses Arrays and Objects. These correspond here to the datatypes ArrayList and Hashtable.
  /// All numbers are parsed to doubles.
  /// </summary>
  public sealed class JSON {
    public const int TOKEN_NONE = 0;
    public const int TOKEN_CURLY_OPEN = 1;
    public const int TOKEN_CURLY_CLOSE = 2;
    public const int TOKEN_SQUARED_OPEN = 3;
    public const int TOKEN_SQUARED_CLOSE = 4;
    public const int TOKEN_COLON = 5;
    public const int TOKEN_COMMA = 6;
    public const int TOKEN_STRING = 7;
    public const int TOKEN_NUMBER = 8;
    public const int TOKEN_TRUE = 9;
    public const int TOKEN_FALSE = 10;
    public const int TOKEN_NULL = 11;

    private const int BUILDER_CAPACITY = 2000;

    /// <summary>
    /// Parses the string json into a value
    /// </summary>
    /// <param name="json">A JSON string.</param>
    /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
    public static object JsonDecode(string json) {
      bool success = true;

      return JsonDecode(json, ref success);
    }

    /// <summary>
    /// Parses the string json into a value; and fills 'success' with the successfullness of the parse.
    /// </summary>
    /// <param name="json">A JSON string.</param>
    /// <param name="success">Successful parse?</param>
    /// <returns>An ArrayList, a Hashtable, a double, a string, null, true, or false</returns>
    public static object JsonDecode(string json, ref bool success) {
      success = true;
      if (json != null) {
        char[] charArray = json.ToCharArray();
        int index = 0;
        object value = ParseValue(charArray, ref index, ref success);
        return value;
      }
      else {
        return null;
      }
    }

    /// <summary>
    /// Converts a Hashtable / ArrayList object into a JSON string
    /// </summary>
    /// <param name="json">A Hashtable / ArrayList</param>
    /// <returns>A JSON encoded string, or null if object 'json' is not serializable</returns>
    public static string JsonEncode(object json) {
      StringBuilder builder = new StringBuilder(BUILDER_CAPACITY);
      bool success = SerializeValue(json, builder);
      return (success ? builder.ToString() : null);
    }

    protected static Hashtable ParseObject(char[] json, ref int index, ref bool success) {
      Hashtable table = new Hashtable();
      int token;

      // {
      NextToken(json, ref index);

      bool done = false;
      while (!done) {
        token = LookAhead(json, index);
        if (token == JSON.TOKEN_NONE) {
          success = false;
          return null;
        }
        else if (token == JSON.TOKEN_COMMA) {
          NextToken(json, ref index);
        }
        else if (token == JSON.TOKEN_CURLY_CLOSE) {
          NextToken(json, ref index);
          return table;
        }
        else {

          // name
          string name = ParseString(json, ref index, ref success);
          if (!success) {
            success = false;
            return null;
          }

          // :
          token = NextToken(json, ref index);
          if (token != JSON.TOKEN_COLON) {
            success = false;
            return null;
          }

          // value
          object value = ParseValue(json, ref index, ref success);
          if (!success) {
            success = false;
            return null;
          }

          table[name] = value;
        }
      }

      return table;
    }

    protected static ArrayList ParseArray(char[] json, ref int index, ref bool success) {
      ArrayList array = new ArrayList();

      // [
      NextToken(json, ref index);

      bool done = false;
      while (!done) {
        int token = LookAhead(json, index);
        if (token == JSON.TOKEN_NONE) {
          success = false;
          return null;
        }
        else if (token == JSON.TOKEN_COMMA) {
          NextToken(json, ref index);
        }
        else if (token == JSON.TOKEN_SQUARED_CLOSE) {
          NextToken(json, ref index);
          break;
        }
        else {
          object value = ParseValue(json, ref index, ref success);
          if (!success) {
            return null;
          }

          array.Add(value);
        }
      }

      return array;
    }

    protected static object ParseValue(char[] json, ref int index, ref bool success) {
      switch (LookAhead(json, index)) {
        case JSON.TOKEN_STRING:
          return ParseString(json, ref index, ref success);
        case JSON.TOKEN_NUMBER:
          return ParseNumber(json, ref index, ref success);
        case JSON.TOKEN_CURLY_OPEN:
          return ParseObject(json, ref index, ref success);
        case JSON.TOKEN_SQUARED_OPEN:
          return ParseArray(json, ref index, ref success);
        case JSON.TOKEN_TRUE:
          NextToken(json, ref index);
          return true;
        case JSON.TOKEN_FALSE:
          NextToken(json, ref index);
          return false;
        case JSON.TOKEN_NULL:
          NextToken(json, ref index);
          return null;
        case JSON.TOKEN_NONE:
          break;
      }

      success = false;
      return null;
    }

    protected static string ParseString(char[] json, ref int index, ref bool success) {
      StringBuilder s = new StringBuilder(BUILDER_CAPACITY);
      char c;

      EatWhitespace(json, ref index);

      // "
      c = json[index++];

      bool complete = false;
      while (!complete) {

        if (index == json.Length) {
          break;
        }

        c = json[index++];
        if (c == '"') {
          complete = true;
          break;
        }
        else if (c == '\\') {

          if (index == json.Length) {
            break;
          }
          c = json[index++];
          if (c == '"') {
            s.Append('"');
          }
          else if (c == '\\') {
            s.Append('\\');
          }
          else if (c == '/') {
            s.Append('/');
          }
          else if (c == 'b') {
            s.Append('\b');
          }
          else if (c == 'f') {
            s.Append('\f');
          }
          else if (c == 'n') {
            s.Append('\n');
          }
          else if (c == 'r') {
            s.Append('\r');
          }
          else if (c == 't') {
            s.Append('\t');
          }
          else if (c == 'u') {
            int remainingLength = json.Length - index;
            if (remainingLength >= 4) {
              // parse the 32 bit hex into an integer codepoint
              uint codePoint;
              if (!(success = UInt32.TryParse(new string(json, index, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out codePoint))) {
                return "";
              }
              // convert the integer codepoint to a unicode char and add to string
              s.Append(Char.ConvertFromUtf32((int)codePoint));
              // skip 4 chars
              index += 4;
            }
            else {
              break;
            }
          }

        }
        else {
          s.Append(c);
        }

      }

      if (!complete) {
        success = false;
        return null;
      }

      return s.ToString();
    }

    protected static double ParseNumber(char[] json, ref int index, ref bool success) {
      EatWhitespace(json, ref index);

      int lastIndex = GetLastIndexOfNumber(json, index);
      int charLength = (lastIndex - index) + 1;

      double number;
      success = Double.TryParse(new string(json, index, charLength), NumberStyles.Any, CultureInfo.InvariantCulture, out number);

      index = lastIndex + 1;
      return number;
    }

    protected static int GetLastIndexOfNumber(char[] json, int index) {
      int lastIndex;

      for (lastIndex = index; lastIndex < json.Length; lastIndex++) {
        if ("0123456789+-.eE".IndexOf(json[lastIndex]) == -1) {
          break;
        }
      }
      return lastIndex - 1;
    }

    protected static void EatWhitespace(char[] json, ref int index) {
      for (; index < json.Length; index++) {
        if (" \t\n\r".IndexOf(json[index]) == -1) {
          break;
        }
      }
    }

    protected static int LookAhead(char[] json, int index) {
      int saveIndex = index;
      return NextToken(json, ref saveIndex);
    }

    protected static int NextToken(char[] json, ref int index) {
      EatWhitespace(json, ref index);

      if (index == json.Length) {
        return JSON.TOKEN_NONE;
      }

      char c = json[index];
      index++;
      switch (c) {
        case '{':
          return JSON.TOKEN_CURLY_OPEN;
        case '}':
          return JSON.TOKEN_CURLY_CLOSE;
        case '[':
          return JSON.TOKEN_SQUARED_OPEN;
        case ']':
          return JSON.TOKEN_SQUARED_CLOSE;
        case ',':
          return JSON.TOKEN_COMMA;
        case '"':
          return JSON.TOKEN_STRING;
        case '0':
        case '1':
        case '2':
        case '3':
        case '4':
        case '5':
        case '6':
        case '7':
        case '8':
        case '9':
        case '-':
          return JSON.TOKEN_NUMBER;
        case ':':
          return JSON.TOKEN_COLON;
      }
      index--;

      int remainingLength = json.Length - index;

      // false
      if (remainingLength >= 5) {
        if (json[index] == 'f' &&
            json[index + 1] == 'a' &&
            json[index + 2] == 'l' &&
            json[index + 3] == 's' &&
            json[index + 4] == 'e') {
          index += 5;
          return JSON.TOKEN_FALSE;
        }
      }

      // true
      if (remainingLength >= 4) {
        if (json[index] == 't' &&
            json[index + 1] == 'r' &&
            json[index + 2] == 'u' &&
            json[index + 3] == 'e') {
          index += 4;
          return JSON.TOKEN_TRUE;
        }
      }

      // null
      if (remainingLength >= 4) {
        if (json[index] == 'n' &&
            json[index + 1] == 'u' &&
            json[index + 2] == 'l' &&
            json[index + 3] == 'l') {
          index += 4;
          return JSON.TOKEN_NULL;
        }
      }

      return JSON.TOKEN_NONE;
    }

    protected static bool SerializeValue(object value, StringBuilder builder) {
      bool success = true;

      if (value is string) {
        success = SerializeString((string)value, builder);
      }
      else if (value is Hashtable) {
        success = SerializeObject((Hashtable)value, builder);
      }
      else if (value is ArrayList) {
        success = SerializeArray((ArrayList)value, builder);
      }
      else if (IsNumeric(value)) {
        success = SerializeNumber(Convert.ToDouble(value), builder);
      }
      else if ((value is Boolean) && ((Boolean)value == true)) {
        builder.Append("true");
      }
      else if ((value is Boolean) && ((Boolean)value == false)) {
        builder.Append("false");
      }
      else if (value == null) {
        builder.Append("null");
      }
      else {
        success = false;
      }
      return success;
    }

    protected static bool SerializeObject(Hashtable anObject, StringBuilder builder) {
      builder.Append("{");

      IDictionaryEnumerator e = anObject.GetEnumerator();
      bool first = true;
      while (e.MoveNext()) {
        string key = e.Key.ToString();
        object value = e.Value;

        if (!first) {
          builder.Append(", ");
        }

        SerializeString(key, builder);
        builder.Append(":");
        if (!SerializeValue(value, builder)) {
          return false;
        }

        first = false;
      }

      builder.Append("}");
      return true;
    }

    protected static bool SerializeArray(ArrayList anArray, StringBuilder builder) {
      builder.Append("[");

      bool first = true;
      for (int i = 0; i < anArray.Count; i++) {
        object value = anArray[i];

        if (!first) {
          builder.Append(", ");
        }

        if (!SerializeValue(value, builder)) {
          return false;
        }

        first = false;
      }

      builder.Append("]");
      return true;
    }

    protected static bool SerializeString(string aString, StringBuilder builder) {
      builder.Append("\"");

      char[] charArray = aString.ToCharArray();
      for (int i = 0; i < charArray.Length; i++) {
        char c = charArray[i];
        if (c == '"') {
          builder.Append("\\\"");
        }
        else if (c == '\\') {
          builder.Append("\\\\");
        }
        else if (c == '\b') {
          builder.Append("\\b");
        }
        else if (c == '\f') {
          builder.Append("\\f");
        }
        else if (c == '\n') {
          builder.Append("\\n");
        }
        else if (c == '\r') {
          builder.Append("\\r");
        }
        else if (c == '\t') {
          builder.Append("\\t");
        }
        else {
          int codepoint = Convert.ToInt32(c);
          if ((codepoint >= 32) && (codepoint <= 126)) {
            builder.Append(c);
          }
          else {
            builder.Append("\\u" + Convert.ToString(codepoint, 16).PadLeft(4, '0'));
          }
        }
      }

      builder.Append("\"");
      return true;
    }

    protected static bool SerializeNumber(double number, StringBuilder builder) {
      builder.Append(Convert.ToString(number, CultureInfo.InvariantCulture));
      return true;
    }

    /// <summary>
    /// Determines if a given object is numeric in any way
    /// (can be integer, double, null, etc). 
    /// 
    /// Thanks to mtighe for pointing out Double.TryParse to me.
    /// </summary>
    public static bool IsNumeric(object o) {
      double result;

      return (o == null) ? false : Double.TryParse(o.ToString(), out result);
    }
  }
}