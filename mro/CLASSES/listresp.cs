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
  public sealed class ListResponse {
    bool fast = true;
    response Response = null;
    StringBuilder t0 = new StringBuilder();
    StringBuilder t1 = new StringBuilder();
    StringBuilder t2 = new StringBuilder();
    StringBuilder t3 = new StringBuilder();
    public char[] listid = new char[2];
    public mroJSON result = null;
    private int listno = 0;
    private int nrows = 0;
    private int ncols = 0;

    public int get_lstid() { return listno; }
    public int get_nrows() { return nrows; }
    public int get_ncols() { return ncols; }

    public ListResponse() {
    }
    public ListResponse(int listno, int ncols, client clie, bool direct = true) {
      init(listno, ncols, clie, direct);
    }
    // temporal trick
    public void init(int listno, int ncols, client clie, bool direct = true) {
      fast = direct; // clie != null && clie.resp != null;
      if (fast) this.Response = clie.resp;
      else result = new mroJSON();

      this.listno = listno;

      t1.Length = 0;
      t1.Append("zla");
      t1.Append(listno);
      string lid = t1.ToString();

      t1.Length = 0;
      t1.Append('z');
      t1.Append(listno);
      t1.Append("nclsz");
      string cls = t1.ToString();

      if (fast) {
        response.send(Response, lid, listno.tostr());
        response.send(Response, cls, ncols.tostr());
      }
      else {
        result.set(lid, listno);
        result.set(cls, ncols);
      }
      listid[0] = 'l';
      listid[1] = (char)(48 + listno);

      this.ncols = ncols;
    }

    #region one
    public void set_data(int row, string col, string obj) {
      t1.Length = 0;
      t1.Append(listid, 0, 2);
      t1.Append(row);
      t1.Append(col);
      if (fast) response.send(Response, t1, obj);
      else result.set(t1.ToString(), obj);
    }
    public void set_data(int row, char col, string obj) {
      t1.Length = 0;
      t1.Append(listid, 0, 2);
      t1.Append(row);
      t1.Append(col);
      if (fast) response.send(Response, t1, obj);
      else result.set(t1.ToString(), obj);
    }
    public void set_data(int row, string col, int obj) {
      t1.Length = 0;
      t1.Append(listid, 0, 2);
      t1.Append(row);
      t1.Append(col);
      if (fast) response.send(Response, t1, obj.ToString());
      else result.set(t1.ToString(), obj.ToString());
    }
    #endregion
    #region two
    public void set_data(int row, string col0, string obj0, string col1, string obj1) {
      t0.Length = 0;
      t0.Append(listid, 0, 2);
      t0.Append(row);
      int l2 = t0.Length;

      t1.Length = 0; t1.Append(t0);
      t2.Length = 0; t2.Append(t0);

      t1.Append(col0);
      t2.Append(col1);
      if (fast) response.send(Response, t1, obj0, t2, obj1);
      else {
        result.set(t1.ToString(), obj0);
        result.set(t2.ToString(), obj1);
      }
    }
    public void set_data(int row, char col0, string obj0, char col1, string obj1) {
      t0.Length = 0;
      t0.Append(listid, 0, 2);
      t0.Append(row);
      int l2 = t0.Length;

      t1.Length = 0; t1.Append(t0);
      t2.Length = 0; t2.Append(t0);

      t1.Append(col0);
      t2.Append(col1);
      if (fast) response.send(Response, t1, obj0, t2, obj1);
      else {
        result.set(t1.ToString(), obj0);
        result.set(t2.ToString(), obj1);
      }
    }
    #endregion
    #region three
    public void set_data(int row, string col0, string obj0, string col1, string obj1,
                                    string col2, string obj2) {
      t0.Length = 0;
      t0.Append(listid, 0, 2);
      t0.Append(row);
      int l2 = t0.Length;

      t1.Length = 0; t1.Append(t0);
      t2.Length = 0; t2.Append(t0);
      t3.Length = 0; t3.Append(t0);

      t1.Append(col0);
      t2.Append(col1);
      t3.Append(col2);
      if (fast) response.send(Response, t1, obj0, t2, obj1, t3, obj2);
      else {
        result.set(t1.ToString(), obj0);
        result.set(t2.ToString(), obj1);
        result.set(t3.ToString(), obj2);
      }
    }
    public void set_data(int row, char col0, string obj0, char col1, string obj1,
                                    char col2, string obj2) {
      t0.Length = 0;
      t0.Append(listid, 0, 2);
      t0.Append(row);
      int l2 = t0.Length;

      t1.Length = 0; t1.Append(t0);
      t2.Length = 0; t2.Append(t0);
      t3.Length = 0; t3.Append(t0);

      t1.Append(col0);
      t2.Append(col1);
      t3.Append(col2);
      if (fast) response.send(Response, t1, obj0, t2, obj1, t3, obj2);
      else {
        result.set(t1.ToString(), obj0);
        result.set(t2.ToString(), obj1);
        result.set(t3.ToString(), obj2);
      }
    }
    #endregion
    #region four
    public void set_data(int row, string col0, string obj0, string col1, string obj1,
                                    string col2, string obj2, string col3, string obj3) {
      t0.Length = 0;
      t0.Append(listid, 0, 2);
      t0.Append(row);
      int l2 = t0.Length;

      t1.Length = 0; t1.Append(t0);
      t2.Length = 0; t2.Append(t0);
      t3.Length = 0; t3.Append(t0);

      t1.Append(col0);
      t2.Append(col1);
      t3.Append(col2);
      if (fast) response.send(Response, t1, obj0, t2, obj1, t3, obj2);
      else { result.set(t1.ToString(), obj0); result.set(t2.ToString(), obj1); result.set(t3.ToString(), obj2); }

      t1.Length = l2; t1.Append(col3);
      if (fast) response.send(Response, t1, obj3);
      else {
        result.set(t1.ToString(), obj3);
      }
    }
    public void set_data(int row, char col0, string obj0, char col1, string obj1,
                                    char col2, string obj2, char col3, string obj3) {
      t0.Length = 0;
      t0.Append(listid, 0, 2);
      t0.Append(row);
      int l2 = t0.Length;

      t1.Length = 0; t1.Append(t0);
      t2.Length = 0; t2.Append(t0);
      t3.Length = 0; t3.Append(t0);

      t1.Append(col0);
      t2.Append(col1);
      t3.Append(col2);
      if (fast) response.send(Response, t1, obj0, t2, obj1, t3, obj2);
      else { result.set(t1.ToString(), obj0); result.set(t2.ToString(), obj1); result.set(t3.ToString(), obj2); }

      t1.Length = l2; t1.Append(col3);
      if (fast) response.send(Response, t1, obj3);
      else {
        result.set(t1.ToString(), obj3);
      }
    }
    #endregion
    #region five
    public void set_data(int row, string col0, string obj0, string col1, string obj1,
                                    string col2, string obj2, string col3, string obj3,
                                    string col4, string obj4) {
      t0.Length = 0;
      t0.Append(listid, 0, 2);
      t0.Append(row);
      int l2 = t0.Length;

      t1.Length = 0; t1.Append(t0);
      t2.Length = 0; t2.Append(t0);
      t3.Length = 0; t3.Append(t0);

      t1.Append(col0);
      t2.Append(col1);
      t3.Append(col2);
      if (fast) response.send(Response, t1, obj0, t2, obj1, t3, obj2);
      else {
        result.set(t1.ToString(), obj0);
        result.set(t2.ToString(), obj1);
        result.set(t3.ToString(), obj2);
      }

      t1.Length = l2; t1.Append(col3);
      t2.Length = l2; t2.Append(col4);
      if (fast) response.send(Response, t1, obj3, t2, obj4);
      else {
        result.set(t1.ToString(), obj3);
        result.set(t2.ToString(), obj4);
      }
    }
    public void set_data(int row, char col0, string obj0, char col1, string obj1,
                                    char col2, string obj2, char col3, string obj3,
                                    char col4, string obj4) {
      t0.Length = 0;
      t0.Append(listid, 0, 2);
      t0.Append(row);
      int l2 = t0.Length;

      t1.Length = 0; t1.Append(t0);
      t2.Length = 0; t2.Append(t0);
      t3.Length = 0; t3.Append(t0);

      t1.Append(col0);
      t2.Append(col1);
      t3.Append(col2);
      if (fast) response.send(Response, t1, obj0, t2, obj1, t3, obj2);
      else {
        result.set(t1.ToString(), obj0);
        result.set(t2.ToString(), obj1);
        result.set(t3.ToString(), obj2);
      }

      t1.Length = l2; t1.Append(col3);
      t2.Length = l2; t2.Append(col4);
      if (fast) response.send(Response, t1, obj3, t2, obj4);
      else {
        result.set(t1.ToString(), obj3);
        result.set(t2.ToString(), obj4);
      }
    }
    #endregion
    #region six
    public void set_data(int row, string col0, string obj0, string col1, string obj1,
                                    string col2, string obj2, string col3, string obj3,
                                    string col4, string obj4, string col5, string obj5) {
      t0.Length = 0;
      t0.Append(listid, 0, 2);
      t0.Append(row);
      int l2 = t0.Length;

      t1.Length = 0; t1.Append(t0);
      t2.Length = 0; t2.Append(t0);
      t3.Length = 0; t3.Append(t0);

      t1.Append(col0);
      t2.Append(col1);
      t3.Append(col2);
      if (fast) response.send(Response, t1, obj0, t2, obj1, t3, obj2);
      else {
        result.set(t1.ToString(), obj0);
        result.set(t2.ToString(), obj1);
        result.set(t3.ToString(), obj2);
      }

      t1.Length = l2; t1.Append(col3);
      t2.Length = l2; t2.Append(col4);
      t3.Length = l2; t3.Append(col5);
      if (fast) response.send(Response, t1, obj3, t2, obj4, t3, obj5);
      else {
        result.set(t1.ToString(), obj3);
        result.set(t2.ToString(), obj4);
        result.set(t3.ToString(), obj5);
      }
    }
    public void set_data(int row, char col0, string obj0, char col1, string obj1,
                                    char col2, string obj2, char col3, string obj3,
                                    char col4, string obj4, char col5, string obj5) {
      t0.Length = 0;
      t0.Append(listid, 0, 2);
      t0.Append(row);
      int l2 = t0.Length;

      t1.Length = 0; t1.Append(t0);
      t2.Length = 0; t2.Append(t0);
      t3.Length = 0; t3.Append(t0);

      t1.Append(col0);
      t2.Append(col1);
      t3.Append(col2);
      if (fast) response.send(Response, t1, obj0, t2, obj1, t3, obj2);
      else {
        result.set(t1.ToString(), obj0);
        result.set(t2.ToString(), obj1);
        result.set(t3.ToString(), obj2);
      }

      t1.Length = l2; t1.Append(col3);
      t2.Length = l2; t2.Append(col4);
      t3.Length = l2; t3.Append(col5);
      if (fast) response.send(Response, t1, obj3, t2, obj4, t3, obj5);
      else {
        result.set(t1.ToString(), obj3);
        result.set(t2.ToString(), obj4);
        result.set(t3.ToString(), obj5);
      }
    }
    #endregion
    public void set_row_img(int row) {
      t1.Length = 0;
      t1.Append(listid, 0, 2);
      t1.Append(row);
      t1.Append('*');
      if (fast) response.send(Response, t1, "0");
      else result.set(t1.ToString(), "0");
    }
    public void set_rows(int nrows) {
      t1.Length = 0;
      t1.Append('z');
      t1.Append(listid, 0, 2);
      t1.Append("rows");
      if (fast) response.send(Response, t1, nrows.ToString());
      else result.set(t1.ToString(), nrows);
      this.nrows = nrows;
    }
    public void return_regs() {
      t1.Length = 0;
      t1.Append("ztotslst");
      t1.Append(listno);
      string key = t1.ToString();

      t1.Length = 0;
      t1.Append("regs:");
      t1.Append(nrows);
      if (fast) response.send(Response, key, t1.ToString());
      else result.set(key, t1.ToString());
    }
    public void return_colname_info(int col, string name, int namelen) {
      t0.Length = 0;
      t0.Append('z');
      t0.Append(listno);
      t0.Append("cl");
      t0.Append(col);

      t1.Length = 0;
      t1.Append(t0);
      t1.Append('z');
      string k0 = t1.ToString();

      t2.Length = 0;
      t2.Append(t0);
      t2.Append('l');
      string k1 = t2.ToString();

      if (fast) {
        response.send(Response, k0, name);
        response.send(Response, k1, namelen.ToString());
      }
      else {
        result.set(k0, name);
        result.set(k1, namelen.ToString());
      }
    }
    public void return_coltype_info(int col, int type) {
      t0.Length = 0;
      t0.Append('z');
      t0.Append(listno);
      t0.Append("cl");
      t0.Append(col);

      t1.Length = 0;
      t1.Append(t0);
      t1.Append('t');
      string k = t1.ToString();

      if (fast) response.send(Response, k, type.ToString());
      else result.set(k, type.ToString());
    }
    public void pass_to_obj(mroJSON obj) {
      if (!fast) obj.append(result);
    }
  }
}