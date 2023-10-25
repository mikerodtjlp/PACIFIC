/**
 *  description : handle the access for the core components of the system
 *  author      : miguel rodriguez ojeda
 *  log         : march 3 creation
 */

#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Collections.Generic;
using System.Text;
using mro;
using mro.db;
using mro.BO;
using mro.BL;
using System.Data.SqlClient;
using System.Data;

namespace mro.DAL {
  public sealed class control_DAL : DataWorker, IDisposable {
    #region database setup

    private IDbConnection conn = null;

    public static control_DAL instance(string name = "main") {
      return new control_DAL(name);
    }
    public control_DAL(string name) : base(name) {
      conn = database.CreateOpenConnection();
    }
    void IDisposable.Dispose() {
      conn.Close();
      conn.Dispose();
    }
    #endregion

    #region query execution

    public static Exception handle_Exception(Exception e) {
      var errmsg = e.Message.Trim();
      if (utils.isparm(errmsg)) {
        e = new Exception(mem.json(utils.mro2json(
                                   new CParameters(e.Message),
                                   new StringBuilder(), false)
                          ));
      }
      return e;
    }

    public void exec_query_no_resp(client clie, string qry) {
      var q = sql.track_query(clie, qry);
      using (var cmd = database.CreateCommand(q, conn)) {
        try {
          cmd.ExecuteNonQuery();
        } 
        catch (Exception e) {
          e = handle_Exception(e);
          throw new Exception(q, e);
        }
      }
    }
    public query_result execute_query(client clie,
                                        string qry,
                                        int[] cols2ret,
                                        bool multithreading) {
      var q = sql.track_query(clie, qry);
      using (var cmd = database.CreateCommand(q, conn)) {
        IDataReader r = null;
        try {
          r = cmd.ExecuteReader();
        } 
        catch (Exception e) {
          e = handle_Exception(e);
          if (multithreading) return new query_result(e);
          else throw new Exception(q, e);
        }

        if (r == null) return new query_result(null, null);

        var cols = new List<table_col>();
        char col = 'A';
        char col2 = 'A';
        bool dcol = false;
        var customcols = cols2ret != null && cols2ret.Length > 0;
        var ncols = customcols ? cols2ret.Length : r.FieldCount;
        var strs = new bool[ncols];
        var dats = new bool[ncols];

        if (customcols) {
          for (var i = 0; i < cols2ret.Length; ++i) {
            cols.Add(new table_col(string.Concat(col.ToString(),
                                    dcol ? col2.ToString() : string.Empty),
                                    r.GetName(cols2ret[i]),
                                    r.GetDataTypeName(cols2ret[i])));
            if (!dcol) ++col; else ++col2;
            if (col == 'Z' + 1) {
              dcol = true;
              col = 'A';
            }
          }
        }
        else {
          for (var i = 0; i < r.FieldCount; ++i) {
            cols.Add(new table_col(string.Concat(col.ToString(),
                                    dcol ? col2.ToString() : string.Empty),
                                    r.GetName(i),
                                    r.GetDataTypeName(i)));
            if (!dcol) ++col; else ++col2;
            if (col == 'Z' + 1) {
              dcol = true;
              col = 'A';
            }
          }
        }

        var data = new List<List<string>>();
        var types_collected = false;
        string datum = string.Empty;
        for (; r.Read();) {
          var row = new List<string>();
          if (customcols) {
            for (var c = 0; c < cols2ret.Length; ++c) {
              int cc = cols2ret[c];
              if (!types_collected) {
                Type t = r[cc].GetType();
                cols[c].type = (t == typeof(Int32) || t == typeof(Int16)) ? 0 : 1;
                string dt = r.GetDataTypeName(cc);
                strs[c] = dt.IndexOf("char") != -1;
                dats[c] = dt.IndexOf("datetime") != -1;
              }
              if (strs[c]) datum = r.IsDBNull(cc) ? string.Empty : r.GetString(cc);
              else if (dats[c]) datum = utils.to_std_date(r[cc]);
              else datum = r[cc].ToString();
              row.Add(datum);
            }
            types_collected = true;
          }
          else {
            for (var c = 0; c < r.FieldCount; ++c) {
              if (!types_collected) {
                Type t = r[c].GetType();
                cols[c].type = (t == typeof(Int32) || t == typeof(Int16)) ? 0 : 1;
                cols[c].name = r.GetName(c);
                cols[c].field_type = r.GetDataTypeName(c);
                string dt = r.GetDataTypeName(c);
                strs[c] = dt.IndexOf("char") != -1;
                dats[c] = dt.IndexOf("datetime") != -1;
              }
              if (strs[c]) datum = r.IsDBNull(c) ? string.Empty : r.GetString(c);
              else if (dats[c]) datum = utils.to_std_date(r[c]);
              else datum = r[c].ToString();
              row.Add(datum);
            }
            types_collected = true;
          }
          data.Add(row);
        }
        return new query_result(data, cols);
      }
    }

    public void query_into_result(client clie,
                                  string qry,
                                  mroJSON result) {
      var q = sql.track_query(clie, qry);
      using (var cmd = database.CreateCommand(q, conn)) {
        IDataReader r = null;
        try {
          r = cmd.ExecuteReader();
        } 
        catch (Exception e) {
          e = handle_Exception(e);
          throw new Exception(q, e);
        }
        if (r == null) return;

        var ncols = r.FieldCount;
        var datum = "";
        for (; r.Read();) {
          for (var c = 0; c < r.FieldCount; ++c) {
            string dt = r.GetDataTypeName(c);
            if (dt.IndexOf("char") != -1) datum = r.IsDBNull(c) ?
                                                string.Empty : r.GetString(c);
            else if (dt.IndexOf("datetime") != -1) datum = utils.to_std_date(r[c]);
            else datum = r[c].ToString();
            result.set(r.GetName(c), datum);
          }
          break;
        }
      }
    }

    public string[] execute_query_one_row(client clie,
                                           string qry,
                                           int[] cols2ret) {
      var q = sql.track_query(clie, qry);
      using (var cmd = database.CreateCommand(q, conn)) {
        IDataReader r = null;
        try {
          r = cmd.ExecuteReader();
        } 
        catch (Exception e) {
          e = handle_Exception(e);
          throw new Exception(q, e);
        }
        if (r == null) new query_result(null, null);

        var customcols = cols2ret != null && cols2ret.Length > 0;
        var ncols = customcols ? cols2ret.Length : r.FieldCount;
        var datum = new string[ncols];
        for (; r.Read();) {
          if (customcols) {
            for (var c = 0; c < cols2ret.Length; ++c) {
              int cc = cols2ret[c];
              string dt = r.GetDataTypeName(cc);
              if (dt.IndexOf("char") != -1) datum[c] = r.IsDBNull(cc) ?
                                                  string.Empty : r.GetString(cc);
              else if (dt.IndexOf("datetime") != -1) datum[c] = utils.to_std_date(r[cc]);
              else datum[c] = r[cc].ToString();
            }
          }
          else {
            for (var c = 0; c < r.FieldCount; ++c) {
              string dt = r.GetDataTypeName(c);
              if (dt.IndexOf("char") != -1) datum[c] = r.IsDBNull(c) ?
                                                  string.Empty : r.GetString(c);
              else if (dt.IndexOf("datetime") != -1) datum[c] = utils.to_std_date(r[c]);
              else datum[c] = r[c].ToString();
            }
          }
          break;
        }
        return datum;
      }
    }

    /*public string execute_scalar(client clie, string qry)
    {
        var q = validate.form_query(clie, qry);
        using (var cmd = database.CreateCommand(q, conn))
        {
            var r = cmd.ExecuteScalar();
            return r == null ? string.Empty : r.ToString();
        }
    }*/
    public ListResponse execute_query_resp(client clie,
                                              ListResponse lr,
                                              bool direct,
                                              int listid,
                                              string qry,
                                              int[] cols2ret,
                                              int nretcols,
                                              bool retcols,
                                              bool retcoltype) {
      var q = sql.track_query(clie, qry);
      using (var cmd = database.CreateCommand(q, conn)) {
        IDataReader rs = null;
        try {
          rs = cmd.ExecuteReader();
        } 
        catch (Exception e) {
          e = handle_Exception(e);
          throw new Exception(q, e);
        }
        if (rs == null) {
          lr.init(listid, 0, clie, direct);
          return lr;
        }

        // detect if we take column data from client or default table
        var customcols = cols2ret != null && cols2ret.Length > 0;
        var ncols = customcols ? nretcols : rs.FieldCount;

        var mhelp = clie.mhelp;
        var ltrs = mhelp.getchr0();
        var datum = mhelp.getstr1();
        var cols = mhelp.getstr2();
        var strs = mhelp.getbool0();
        var dats = mhelp.getbool1();
        var ints = mhelp.getbool2();
        var cnms = mhelp.getstr3();
        var vlns = mhelp.getint0();

        lr.init(listid, ncols, clie, direct);

        var row = 0;
        var hasimage = false;
        var ncolsup24 = ncols > 24;
        var ncolsdown24 = ncols <= 24;

        if (retcols)
          for (int c = 0; c < ncols; ++c)
            vlns[c] = 0;

        if (ncols > 0 && ncols < 64) {   // just security
          for (; rs.Read();) {
            // row 0 process columns
            if (row == 0) {
              char col = 'A';
              char col2 = 'A';
              bool dcol = false;
              if (customcols) {
                for (var c = 0; c < ncols; ++c) {
                  int cc = cols2ret[c];

                  var dt = rs.GetDataTypeName(cc);
                  if (!dcol) cols[c] = col.ToString();
                  else cols[c] = mem.join2(col, col2);
                  strs[c] = dt.IndexOf("char") != -1;
                  dats[c] = dt.IndexOf("datetime") != -1;
                  ints[c] = dt.IndexOf("int") != -1;
                  cnms[c] = rs.GetName(cc);
                  if (!dcol) ++col; else ++col2;
                  if (col == 'Z' + 1) {
                    dcol = true;
                    col = 'A';
                  }
                }
                var n = cnms[ncols - 1];
                hasimage = n.Length > 0 && n[0] == '*';
              }
              else {
                for (var c = 0; c < ncols; ++c) {
                  var dt = rs.GetDataTypeName(c);
                  if (!dcol) cols[c] = col.ToString();
                  else cols[c] = mem.join2(col, col2);
                  strs[c] = dt.IndexOf("char") != -1;
                  dats[c] = dt.IndexOf("datetime") != -1;
                  ints[c] = dt.IndexOf("int") != -1;
                  cnms[c] = rs.GetName(c);
                  if (!dcol) ++col; else ++col2;
                  if (col == 'Z' + 1) {
                    dcol = true;
                    col = 'A';
                  }
                }
                var n = cnms[ncols - 1];
                hasimage = n.Length > 0 && n[0] == '*';
              }
            }

            // process the data
            string d = null;
            for (var c = 0; c < ncols; ++c) {
              var i = customcols ? cols2ret[c] : c;

              if (strs[c]) d = datum[c] = rs.IsDBNull(i) ? string.Empty : rs.GetString(i);
              else if (dats[c]) d = datum[c] = utils.to_std_date(rs[i]);
              else d = datum[c] = rs[i].ToString();

              if (vlns[c] < d.Length) vlns[c] = d.Length;

              if (ncolsup24) {
                if (hasimage) {
                  if (i == ncols - 1) lr.set_data(row, '*', d);
                  else lr.set_data(row, cols[c], d);
                }
                else lr.set_data(row, cols[c], d);
              }
            }

            // create the response
            if (ncolsdown24) {
              if (row == 0) {
                for (int i = 0; i < ncols; ltrs[i] = utils.eclc[i], ++i) ;
                if (hasimage) ltrs[ncols - 1] = '*';
              }

              switch (ncols) {
                case 1: lr.set_data(row, ltrs[0], datum[0]); break;
                case 2: lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1]); break;
                case 3: lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2]); break;
                case 4: lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3]); break;
                case 5: lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4]); break;
                case 6: lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]); break;

                case 7:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6]); break;
                case 8:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7]); break;
                case 9:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8]); break;
                case 10:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9]); break;
                case 11:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10]); break;
                case 12:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]); break;

                case 13:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12]); break;
                case 14:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12], ltrs[13], datum[13]); break;
                case 15:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12], ltrs[13], datum[13], ltrs[14], datum[14]); break;
                case 16:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12], ltrs[13], datum[13], ltrs[14], datum[14], ltrs[15], datum[15]); break;
                case 17:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12], ltrs[13], datum[13], ltrs[14], datum[14], ltrs[15], datum[15], ltrs[16], datum[16]); break;
                case 18:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12], ltrs[13], datum[13], ltrs[14], datum[14], ltrs[15], datum[15], ltrs[16], datum[16], ltrs[17], datum[17]); break;

                case 19:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12], ltrs[13], datum[13], ltrs[14], datum[14], ltrs[15], datum[15], ltrs[16], datum[16], ltrs[17], datum[17]);
                  lr.set_data(row, ltrs[18], datum[18]); break;
                case 20:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12], ltrs[13], datum[13], ltrs[14], datum[14], ltrs[15], datum[15], ltrs[16], datum[16], ltrs[17], datum[17]);
                  lr.set_data(row, ltrs[18], datum[18], ltrs[19], datum[19]); break;
                case 21:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12], ltrs[13], datum[13], ltrs[14], datum[14], ltrs[15], datum[15], ltrs[16], datum[16], ltrs[17], datum[17]);
                  lr.set_data(row, ltrs[18], datum[18], ltrs[19], datum[19], ltrs[20], datum[20]); break;
                case 22:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12], ltrs[13], datum[13], ltrs[14], datum[14], ltrs[15], datum[15], ltrs[16], datum[16], ltrs[17], datum[17]);
                  lr.set_data(row, ltrs[18], datum[18], ltrs[19], datum[19], ltrs[20], datum[20], ltrs[21], datum[21]); break;
                case 23:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12], ltrs[13], datum[13], ltrs[14], datum[14], ltrs[15], datum[15], ltrs[16], datum[16], ltrs[17], datum[17]);
                  lr.set_data(row, ltrs[18], datum[18], ltrs[19], datum[19], ltrs[20], datum[20], ltrs[21], datum[21], ltrs[22], datum[22]); break;
                case 24:
                  lr.set_data(row, ltrs[0], datum[0], ltrs[1], datum[1], ltrs[2], datum[2], ltrs[3], datum[3], ltrs[4], datum[4], ltrs[5], datum[5]);
                  lr.set_data(row, ltrs[6], datum[6], ltrs[7], datum[7], ltrs[8], datum[8], ltrs[9], datum[9], ltrs[10], datum[10], ltrs[11], datum[11]);
                  lr.set_data(row, ltrs[12], datum[12], ltrs[13], datum[13], ltrs[14], datum[14], ltrs[15], datum[15], ltrs[16], datum[16], ltrs[17], datum[17]);
                  lr.set_data(row, ltrs[18], datum[18], ltrs[19], datum[19], ltrs[20], datum[20], ltrs[21], datum[21], ltrs[22], datum[22], ltrs[23], datum[23]); break;
              }
            }

            if (!hasimage) lr.set_row_img(row); // we put one as default
            ++row;

            if (row > 256000)
              err.require(cme.QUERY_TOO_LONG, row); // another security
          }
        }

        // process the totals
        lr.set_rows(row);

        if (retcols)
          for (int c = 0; c < ncols; ++c)
            lr.return_colname_info(c, cnms[c], vlns[c]);

        if (retcoltype)
          for (int c = 0; c < ncols; ++c) {
            int type = 0;                  // unknown
            if (strs[c]) type = 1;         // string
            else if (dats[c]) type = 2;    // date
            else if (ints[c]) type = 3;    // int
            lr.return_coltype_info(c, type);
          }

        return lr;
      }
    }
    #endregion

    #region accounts

    public company get_company(int id) {
      var qry = string.Concat("exec dbo.cmpy_get ", id.ToString(), ";");
      company obj = null;
      using (var cmd = database.CreateCommand(qry, conn)) {
        using (var r = cmd.ExecuteReader()) {
          for (; r.Read();) {
            obj = new company(id);
            obj.name = r.IsDBNull(1) ? string.Empty : r.GetString(1);
            obj.owner = r.IsDBNull(2) ? 0 : r.GetInt32(2);
            obj.sector = r.IsDBNull(3) ? 0 : r.GetInt32(3);
            obj.legalID = r.IsDBNull(4) ? string.Empty : r.GetString(4);
            obj.country = r.IsDBNull(5) ? string.Empty : r.GetString(5);
            obj.state = r.IsDBNull(6) ? string.Empty : r.GetString(6);
            obj.city = r.IsDBNull(7) ? string.Empty : r.GetString(7);
            obj.address1 = r.IsDBNull(8) ? string.Empty : r.GetString(8);
            obj.address2 = r.IsDBNull(9) ? string.Empty : r.GetString(9);
            obj.zipcode = r.IsDBNull(10) ? string.Empty : r.GetString(10);
          }
        }
      }
      return obj;
    }
    public int cmpy_gen_new_id() {
      var qry = "exec dbo.cmpy_gen_new_id;";
      using (var cmd = database.CreateCommand(qry, conn)) {
        var r = cmd.ExecuteScalar();
        return r == null ? 0 : Convert.ToInt32(r);
      }
    }
    public void insert_company(client clie, company cmpy) {
      var qry = string.Concat(
          "exec dbo.cmpy_insert ", cmpy.id.ToString(),
          ",'", cmpy.name,
          "',", cmpy.owner,
          ",", cmpy.sector,
          ",'", cmpy.legalID,
          "','", cmpy.country,
          "','", cmpy.state,
          "','", cmpy.city,
          "','", cmpy.address1,
          "','", cmpy.address2,
          "','", cmpy.zipcode,
          "';");
      exec_query_no_resp(clie, qry);
    }
    public void enums_create(client clie, int cmpyid,
        string prdtyp, string idntyp, string gentrn, string finmov) {
      var qry = string.Concat("exec dbo.enums_create ",
          cmpyid, ",'", prdtyp, "','", idntyp, "','", gentrn, "','", finmov, "';");
      exec_query_no_resp(clie, qry);
    }
    public void masterdata_create(client clie, int cmpyid,
        string costcntr, string profcntr) {
      var qry = string.Concat("exec dbo.masterdata_create ",
          cmpyid, ",'", costcntr, "','", profcntr, "';");
      exec_query_no_resp(clie, qry);
    }
    public bool user_cmpy_check(int cmpy, string user) {
      var qry = string.Concat("exec dbo.user_cmpy_check ", cmpy, ",'", user, "';");
      using (var cmd = database.CreateCommand(qry, conn)) {
        var r = cmd.ExecuteScalar();
        return r == null ? false : Convert.ToBoolean(r);
      }
    }
    public void cmpy_set_owner(client clie, int cmpyid, int owner) {
      exec_query_no_resp(clie, string.Concat("exec dbo.cmpy_set_owner ", cmpyid, ",", owner, ";"));
    }
    public void user_cmpy_insert(client clie, int cmpyid, string userid, string type,
        int empl, int cust, int vend, int agen) {
      var qry = string.Concat("exec dbo.user_cmpy_insert ",
          cmpyid, ",'", userid, "','", type, "',", empl, ",", cust, ",", vend, ",", agen, ";");
      exec_query_no_resp(clie, qry);
    }
    public bool user_check_pass(string user, string pass) {
      var qry = mem.join5("exec dbo.user_check_pass '", user, "','", pass, "';");
      using (var cmd = database.CreateCommand(qry, conn)) {
        var r = cmd.ExecuteScalar();
        return r == null ? false : Convert.ToBoolean(r);
      }
    }
    public string user_get_credentials(string user) {
      var qry = mem.join3("exec dbo.user_get_credentials '", user, "';");
      using (var cmd = database.CreateCommand(qry, conn)) {
        var r = cmd.ExecuteScalar();
        return r == null ? string.Empty : r.ToString();
      }
    }
    public bool exist_user(string user) {
      var qry = mem.join3("exec dbo.user_exists '", user, "';");
      using (var cmd = database.CreateCommand(qry, conn)) {
        var r = cmd.ExecuteScalar();
        return r == null ? false : Convert.ToBoolean(r);
      }
    }
    public bool exist_email(string email) {
      var qry = mem.join3("exec dbo.user_email_exists '", email, "';");
      using (var cmd = database.CreateCommand(qry, conn)) {
        var r = cmd.ExecuteScalar();
        return r == null ? false : Convert.ToBoolean(r);
      }
    }
    public bool exist_phone(string phone) {
      var qry = mem.join3("exec dbo.user_phone_exists '", phone, "';");
      using (var cmd = database.CreateCommand(qry, conn)) {
        var r = cmd.ExecuteScalar();
        return r == null ? false : Convert.ToBoolean(r);
      }
    }
    public int entity_email_set(int entity, string email) {
      var qry = string.Concat("exec PERSON.dbo.email_set ", entity, ",'", email, "';");
      using (var cmd = database.CreateCommand(qry, conn)) {
        var r = cmd.ExecuteScalar();
        return r == null ? 0 : Convert.ToInt32(r);
      }
    }
    public int entity_create(int cmpyid, string uname, int type) {
      var qry = string.Concat("exec dbo.entity_create ", cmpyid, ",'", uname, "',", type, ";");
      using (var cmd = database.CreateCommand(qry, conn)) {
        var r = cmd.ExecuteScalar();
        return r == null ? 0 : Convert.ToInt32(r);
      }
    }
    public user get_user(string u) {
      var qry = mem.join3("exec dbo.user_get '", u, "';");
      user obj = null;
      using (var cmd = database.CreateCommand(qry, conn)) {
        using (var r = cmd.ExecuteReader()) {
          for (; r.Read();) {
            obj = new user(u);
            obj.description = r.IsDBNull(1) ? string.Empty : r.GetString(1);
            obj.comments = r.IsDBNull(2) ? string.Empty : r.GetString(2);
            obj.date_start = (DateTime)r[3];
            obj.date_end = (DateTime)r[4];
            obj.password = r.IsDBNull(5) ? string.Empty : r.GetString(5);
            obj.type = r.IsDBNull(6) ? string.Empty : r.GetString(6);
            obj.time_start = (DateTime)r[7];
            obj.time_end = (DateTime)r[8];
            obj.groupid = r.IsDBNull(9) ? string.Empty : r.GetString(9);
            obj.email = r.IsDBNull(10) ? string.Empty : r.GetString(10);
            obj.winuser = r.IsDBNull(11) ? string.Empty : r.GetString(11);
            obj.phone = r.IsDBNull(12) ? string.Empty : r.GetString(12);
          }
        }
      }
      return obj;
    }
    public void insert_user(client clie, user u) {
      var qry = string.Concat(
          "exec dbo.user_insert '", u.id,
          "','", u.description,
          "','", u.comments,
          "','", utils.date_part(u.date_start),
          "','", utils.date_part(u.date_end),
          "','", u.password,
          "','", u.type,
          "','", utils.hour_part(u.time_start),
          "','", utils.hour_part(u.time_end),
          "','", u.groupid,
          "','", u.email,
          "','", u.winuser,
          "','", u.phone,
          "';");
      exec_query_no_resp(clie, qry);
    }
    public void update_user(client clie, user u) {
      var qry = string.Concat(
          "exec dbo.user_update '", u.id,
          "','", u.description,
          "','", u.comments,
          "','", utils.date_part(u.date_start),
          "','", utils.date_part(u.date_end),
          "','", u.password,
          "','", u.type,
          "','", utils.hour_part(u.time_start),
          "','", utils.hour_part(u.time_end),
          "','", u.groupid,
          "','", u.email,
          "','", u.winuser,
          "','", u.phone,
          "';");
      exec_query_no_resp(clie, qry);
    }
    public void delete_user(client clie, string uid) {
      exec_query_no_resp(clie, mem.join3("exec dbo.user_delete '", uid, "';"));
    }
    public List<password> get_raw_rights(user u) {
      var qry = mem.join3("exec dbo.user_get_passwords '", u.id, "';");
      var obj = new List<password>();
      using (var cmd = database.CreateCommand(qry, conn)) {
        using (var r = cmd.ExecuteReader()) {
          for (; r.Read();) {
            obj.Add(new password(r.GetString(0)));
          }
        }
      }
      return obj;
    }
    /*public List<right> get_rights(int cmpy, user u)
    {
        var qry = string.Concat("exec dbo.rights_get ",cmpy,",'", u.id,"';");
        var obj = new List<right>();
        using (var cmd = database.CreateCommand(qry, conn))
        {
            using (var r = cmd.ExecuteReader())
            {
                for (; r.Read(); )
                {
                    obj.Add(new right(  r.GetString(0), 
                                        r.GetString(1),
                                        r.GetString(2)));
                }
            }
        }
        return obj;
    }*/
    /*public right get_right(int cmpyid, user u, string t)
    {
        var qry = string.Concat("exec dbo.right_get ",cmpyid,",'", u.id, "','", t, "';");
        right obj = null;
        using (var cmd = database.CreateCommand(qry, conn))
        {
            using (var r = cmd.ExecuteReader())
            {
                for (; r.Read(); )
                {
                    obj = new right(r.GetString(0), 
                                    r.GetString(1),
                                    r.GetString(2));
                }
            }
        }
        return obj;
    }*/
    public void tree_user_process(client clie, int cmpyid, string userid, string profile) {
      var qry = string.Concat("exec dbo.tree_user_process ",
          cmpyid, ",'", userid, "','", profile, "';");
      exec_query_no_resp(clie, qry);
    }
    public void rights_create(client clie, int cmpyid, string userid, string type) {
      var qry = string.Concat("exec dbo.rights_create ", cmpyid, ",'", userid, "','", type, "';");
      exec_query_no_resp(clie, qry);
    }
    public void rights_insert(client clie, int cmpyid, string userid, string trans,
        string grp, string acc, string cns, string ins, string upd, string del,
        string cms, string vwl) {
      var qry = string.Concat("exec dbo.rights_insert ",
          cmpyid, ",'", userid, "','", trans, "','",
          grp, "','", acc, "','", cns, "','", ins, "','", upd, "','",
          del, "','", cms, "','", vwl, "';");
      exec_query_no_resp(clie, qry);
    }
    public void lib_insert(client clie, string lib, string name) {
      var qry = string.Concat("exec dbo.lib_insert '", lib, "','", name, "';");
      exec_query_no_resp(clie, qry);
    }
    public void lib_grp_insert(client clie, string grp, string name) {
      var qry = string.Concat("exec dbo.lib_grp_insert '", grp, "','", name, "';");
      exec_query_no_resp(clie, qry);
    }
    public void lib_grp_dtl_insert(client clie, string grp, string lib) {
      var qry = string.Concat("exec dbo.lib_grp_dtl_insert '", grp, "','", lib, "';");
      exec_query_no_resp(clie, qry);
    }
    public void lib_grp_usr_insert(client clie, int cmpyid, string grp, string userid) {
      var qry = string.Concat("exec dbo.lib_grp_usr_insert ",
          cmpyid, ",'", grp, "','", userid, "';");
      exec_query_no_resp(clie, qry);
    }
    public void set_password(client clie, user u, password oldpass, password newpass) {
      var qry = mem.join7("exec dbo.password_set '", u.id, "','",
                          oldpass.value, "','", newpass.value, "';");
      exec_query_no_resp(clie, qry);
    }
    public void rights_process(client clie, int cmpy, user u) {
      rights_process(clie, cmpy, u.id);
    }
    public void rights_process(client clie, int cmpy, string userid) {
      exec_query_no_resp(clie, mem.join5("exec dbo.rights_process ", cmpy, ",'", userid, "';"));
    }
    public void cmpy_delete(client clie, int cmpyid) {
      exec_query_no_resp(clie, mem.join3("exec dbo.cmpy_delete ", cmpyid.ToString(), ";"));
    }
    #endregion

    #region development
    public devpackage get_pack_hdr(devpackage p) {
      var qry = mem.join3("exec dbo.develop_get_header '", p.id, "';");
      devpackage obj = null;
      using (var cmd = database.CreateCommand(qry, conn)) {
        using (var r = cmd.ExecuteReader()) {
          for (; r.Read();) {
            obj = new devpackage(p.id, r.GetString(1));
            obj.comments = r.GetString(2);
            obj.status = r.GetString(3);
            obj.type = r.GetString(4);
            obj.reason = r.GetString(5);
            obj.priority = r.GetString(6);
            obj.owner = r.GetString(7);
            obj.originator = r.GetString(8);
            obj.keyuser = r.GetString(9);
            obj.date_submitted = (DateTime)validate.getDefaultIfDBNull(r[10], TypeCode.DateTime);
            obj.date_required = (DateTime)validate.getDefaultIfDBNull(r[11], TypeCode.DateTime);
            obj.date_tentative = (DateTime)validate.getDefaultIfDBNull(r[12], TypeCode.DateTime);
            obj.date_start = (DateTime)validate.getDefaultIfDBNull(r[13], TypeCode.DateTime);
            obj.date_finish = (DateTime)validate.getDefaultIfDBNull(r[14], TypeCode.DateTime);
          }
        }
      }
      return obj;
    }
    public devpackage_detail get_pack_dtl(devpackage p, document d, user u) {
      var qry = mem.join7("exec dbo.develop_get_detail '",
                              p.id, "','", d.id, "','", u.id, "';");
      devpackage_detail obj = null;
      using (var cmd = database.CreateCommand(qry, conn)) {
        using (var r = cmd.ExecuteReader()) {
          for (; r.Read();) {
            obj = new devpackage_detail(p.id, r.GetString(1),
                                                r.GetString(2),
                                                r.GetString(3),
                                                r.GetString(4),
                                                r.IsDBNull(5) ? string.Empty : r.GetString(5),
                                                r.IsDBNull(6) ? string.Empty : r.GetString(6));
          }
        }
      }
      return obj;
    }
    #endregion

    #region documents
    /** 
     * note: the document store procedures are executed by the database machinery 
     * instead of a simple query execution, that is cause the document could possible 
     * contains info like SQL queries, json code, html, etc... that make hard to 
     * push a string parameter into SP without problems of some value that can 
     * close the string example: , ' select 'ID' as id ' and the libraries handle 
     * pretty well these cases
     */
    /*public document_header get_doc_header(string lib, string id)
    {
        using (IDbCommand cmd = database.CreateStoredProcCommand("document_header_get", conn))
        {
            cmd.Parameters.Add(database.CreateParameter("@lib", lib));
            cmd.Parameters.Add(database.CreateParameter("@name", id));
            document_header obj = null;
            using (var r = cmd.ExecuteReader())
            {
                for (; r.Read(); )
                {
                    obj = new document_header(r.GetString(0),
                                       r.GetString(1),
                                       r.GetInt32(2));
                    obj.date_create = (DateTime)validate.getDefaultIfDBNull(r[3], TypeCode.DateTime);
                    obj.date_modify = (DateTime)validate.getDefaultIfDBNull(r[4], TypeCode.DateTime);
                    obj.system = r.GetString(5);
                    obj.module = r.GetString(6);
                    break;
                }
            }
            return obj;
        }
    }*/
    /*public void compose_email(email em)
    {
        var qry = string.Concat("exec dbo.email_ins ",
            "'", em.from,"',",
            "'", em.to, "',",
            "'", em.subject, "',",
            "'", em.body, "',",
            "'", em.attach, "'",
            ";");
        using (var cmd = database.CreateCommand(qry, conn))
        {
            cmd.ExecuteNonQuery();
        }
    }*/
    public email email_get_last() {
      using (var cmd = database.CreateStoredProcCommand("email_get_last", conn)) {
        email obj = null;
        using (var r = cmd.ExecuteReader()) {
          for (; r.Read();) {
            obj = new email(r.GetInt32(0),
                                r.GetString(1),
                                r.GetString(2),
                                "",
                                r.GetString(3),
                                r.GetString(4),
                                r.GetString(5));
            obj.status = r.GetInt32(6);
            obj.creation_date = (DateTime)validate.getDefaultIfDBNull(r[7], TypeCode.DateTime);
            obj.send_date = (DateTime)validate.getDefaultIfDBNull(r[8], TypeCode.DateTime);
            break;
          }
        }
        return obj;
      }
    }
    public email email_get(int status, int dir) {
      using (var cmd = database.CreateStoredProcCommand("email_get", conn)) {
        cmd.Parameters.Add(database.CreateParameter("@sta", status));
        cmd.Parameters.Add(database.CreateParameter("@dir", dir));
        email obj = null;
        using (var r = cmd.ExecuteReader()) {
          for (; r.Read();) {
            obj = new email(r.GetInt32(0),
                                r.GetString(1),
                                r.GetString(2),
                                "",
                                r.GetString(3),
                                r.GetString(4),
                                r.GetString(5));
            obj.status = r.GetInt32(6);
            obj.creation_date = (DateTime)validate.getDefaultIfDBNull(r[7], TypeCode.DateTime);
            obj.send_date = (DateTime)validate.getDefaultIfDBNull(r[8], TypeCode.DateTime);
            break;
          }
        }
        return obj;
      }
    }
    public void email_update(long id, int status) {
      var qry = string.Concat("exec dbo.email_upd ", id, ",", status, ";");
      using (var cmd = database.CreateCommand(qry, conn)) {
        cmd.ExecuteNonQuery();
      }
    }
    public document get_document(string lib, string id, string type) {
      using (var cmd = database.CreateStoredProcCommand("core.dbo.document_get", conn)) {
        cmd.Parameters.Add(database.CreateParameter("@lib", lib));
        cmd.Parameters.Add(database.CreateParameter("@name", id));
        cmd.Parameters.Add(database.CreateParameter("@type", type));
        document obj = null;
        using (var r = cmd.ExecuteReader()) {
          for (; r.Read();) {
            obj = new document(lib,
                               r.GetString(0),
                               r.GetString(1),
                               r.GetString(2),
                               r.GetInt32(3));
            obj.date_create = (DateTime)validate.getDefaultIfDBNull(r[4], TypeCode.DateTime);
            obj.date_modify = (DateTime)validate.getDefaultIfDBNull(r[5], TypeCode.DateTime);
            break;
          }
        }
        return obj;
      }
    }
    public void insert_document(document d) {
      using (var cmd = database.CreateStoredProcCommand("core.dbo.document_insert", conn)) {
        var p = cmd.Parameters;
        p.Add(database.CreateParameter("@lib", d.lib));
        p.Add(database.CreateParameter("@name", d.id));
        p.Add(database.CreateParameter("@data", d.data));
        p.Add(database.CreateParameter("@type", d.type));
        p.Add(database.CreateParameter("@version", d.version));
        validate.evaluateParameters(p, false);
        cmd.ExecuteNonQuery();
      }
    }
    public void update_document(document d) {
      using (var cmd = database.CreateStoredProcCommand("core.dbo.document_update", conn)) {
        var p = cmd.Parameters;
        p.Add(database.CreateParameter("@lib", d.lib));
        p.Add(database.CreateParameter("@name", d.id));
        p.Add(database.CreateParameter("@data", d.data));
        p.Add(database.CreateParameter("@type", d.type));
        p.Add(database.CreateParameter("@version", d.version));
        p.Add(database.CreateParameter("@date_modify", DateTime.Now));
        validate.evaluateParameters(p, false);
        cmd.ExecuteNonQuery();
      }
    }
    public void ses_save_query(int ins, int mac, int cli, int ses, int seq,
        string qry, string websrv) {
      using (var cmd = database.CreateStoredProcCommand("core.dbo.ses_save_query", conn)) {
        var p = cmd.Parameters;
        p.Add(database.CreateParameter("@ins", ins));
        p.Add(database.CreateParameter("@mac", mac));
        p.Add(database.CreateParameter("@cli", cli));
        p.Add(database.CreateParameter("@ses", ses));
        p.Add(database.CreateParameter("@seq", seq));
        p.Add(database.CreateParameter("@qry", qry));
        p.Add(database.CreateParameter("@web", websrv));
        validate.evaluateParameters(p, false);
        cmd.ExecuteNonQuery();
      }
    }
    public string ses_get_query(int ins, int mac, int cli, int ses) {
      using (var cmd = database.CreateStoredProcCommand("core.dbo.ses_get_query", conn)) {
        var p = cmd.Parameters;
        p.Add(database.CreateParameter("@ins", ins));
        p.Add(database.CreateParameter("@mac", mac));
        p.Add(database.CreateParameter("@cli", cli));
        p.Add(database.CreateParameter("@ses", ses));
        var r = cmd.ExecuteScalar();
        return r == null ? string.Empty : r.ToString();
      }
    }

    public int deploy_create_hdr(string pack, string libsrc, string libtar) {
      var qry = mem.join7("exec dbo.deploy_create_hdr '",
          pack, "','", libsrc, "','", libtar, "'; ");
      using (var cmd = database.CreateCommand(qry, conn)) {
        var r = cmd.ExecuteScalar();
        return r != null ? Convert.ToInt32(r) : -1;
      }
    }
    public void deploy_create_dtl(int deployid, string item) {
      var qry = mem.join5("exec dbo.deploy_create_dtl ",
          deployid.ToString(), ",'", item, "'; ");
      using (var cmd = database.CreateCommand(qry, conn)) {
        cmd.ExecuteNonQuery();
      }
    }

    public void exec_batch(string user, string name, string type, string data) {
      using (var cmd = database.CreateStoredProcCommand("core.dbo.job_insert", conn)) {
        var p = cmd.Parameters;
        p.Add(database.CreateParameter("@user", user));
        p.Add(database.CreateParameter("@name", name));
        p.Add(database.CreateParameter("@type", type));
        p.Add(database.CreateParameter("@data", data));
        validate.evaluateParameters(p, false);
        cmd.ExecuteNonQuery();
      }
    }

    public void upload_document_to_db(string rf, string name, string type, byte[] bytes) {
      string query = "insert into documents values (@ref, @Name, @Type, @Data)";
      using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)conn)) {
        cmd.Parameters.AddWithValue("@ref", rf);
        cmd.Parameters.AddWithValue("@Name", name);
        cmd.Parameters.AddWithValue("@Type", type);
        cmd.Parameters.AddWithValue("@Data", bytes);
        cmd.ExecuteNonQuery();
      }
    }

    public byte[] download_document_from_db(string rf, string name) {
      byte[] bytes;
      string fileName, contentType;
      string query = "select name,type,data from documents where ref=@ref and name=@Id";

      using (SqlCommand cmd = new SqlCommand(query, (SqlConnection)conn)) {
        //cmd.CommandText = "select Name, Data, ContentType from tblFiles where Id=@Id";
        cmd.Parameters.AddWithValue("@ref", rf);
        cmd.Parameters.AddWithValue("@Id", name);
        using (SqlDataReader sdr = cmd.ExecuteReader()) {
          sdr.Read();
          fileName = sdr["name"].ToString();
          contentType = sdr["type"].ToString();
          bytes = (byte[])sdr["data"];
        }
      }
      return bytes;
    }

    #endregion
  }
}
