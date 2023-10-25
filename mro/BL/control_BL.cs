/**
 * Core Business logic 
 * 
 * significant dates
 * creation: september 21 2009 
 * version 1: november 16 2009  (basic functionallity)
 * 
 */

#if NETCOREAPP
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
#endif

using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Threading;
using System.Data.SqlClient;
using mro.BO;
using mro.DAL;
using mro;
using IronPdf;

namespace mro.BL {
  public sealed class control_BL {
    public control_BL() {
    }

    public string get_conns(client clie, string target) {
      var res = string.Empty;
      if (clie.zsesins == -1) res = config.dbcode;
      else {
        var sql = string.Format("exec dbo.con_get_by_ses {0},{1},{2},{3},'{4}';",
                          clie.zsesins, clie.zsesmac, clie.zsescli, clie.zsesses,
                          target);
        res = mro.sql.sqlscalar(config.dbcode, sql);
        err.require(string.IsNullOrEmpty(res), cme.DATABASE_NOT_EXIST, target);
      }
      return res;
    }
    public void query_into_result(client clie,
                                     query qry,
                                     mroJSON result,
                                     string target) {
      using (var dal = control_DAL.instance(get_conns(clie, target))) {
        dal.query_into_result(clie, qry.sql, result);
      }
    }
    public string[] execute_query_one_row(client clie,
                                           query qry,
                                           int[] cols2ret,
                                           string target) {
      using (var dal = control_DAL.instance(get_conns(clie, target))) {
        return dal.execute_query_one_row(clie, qry.sql, cols2ret);
      }
    }
    public ListResponse execute_query_resp(client clie,
                                        ListResponse lr,
                                        bool direct,
                                        int listid,
                                        string qry,
                                        string target,
                                        int[] cols2ret = null,
                                        int nretcols = -1,
                                        bool retcols = false,
                                        bool retcoltype = false) {
      using (var dal = control_DAL.instance(get_conns(clie, target))) {
        return dal.execute_query_resp(clie, lr, direct, listid, qry,
           cols2ret, nretcols, retcols, retcoltype);
      }
    }
    /*private ListResponse get_matchcode(client clie,
                               int listid,
                               string source,
                               string sort,
                               string filter,
                               int[] cols2ret,
                               int nretcols,
                               string target) {
       var sql = source;
       if (!string.IsNullOrEmpty(filter))
          sql = mem.join3(sql, " where ", filter);
       if (!string.IsNullOrEmpty(sort))
          sql = mem.join3(sql, " order by ", sort);
       return execute_query_resp(clie, listid,
                                  sql, target, cols2ret, nretcols);
    }
    public ListResponse get_matchcode(int listid,
                               query qry,
                               string sort,
                               string filter,
                               int[] cols2ret,
                               int nretcols,
                               string target) {
       return get_matchcode(null, listid, qry.sql, sort, filter,
                               cols2ret, nretcols, target);
    }
    public ListResponse get_matchcode(client clie,
                               int listid,
                               query qry,
                               string sort,
                               string filter,
                               int[] cols2ret,
                               int nretcols,
                               string target) {
       return get_matchcode(clie, listid, qry.sql, sort, filter,
                               cols2ret, nretcols, target);
    }
    public ListResponse get_matchcode(client clie,
                               int listid,
                               table tbl,
                               string sort,
                               string filter,
                               int[] cols2ret,
                               int nretcols,
                               string target) {
       var sql = mem.join3("select * from ", tbl.id, " with (nolock) ");
       return get_matchcode(clie, listid, sql, sort, filter,
                      cols2ret, nretcols, target);
    }*/
    public void execute_query_no_resp(client clie, query qry, string target) {
      using (var dal = control_DAL.instance(get_conns(clie, target))) {
        dal.exec_query_no_resp(clie, qry.sql);
      }
    }
    public void execute_query_no_resp(client clie, string qry, string target) {
      using (var dal = control_DAL.instance(get_conns(clie, target))) {
        dal.exec_query_no_resp(clie, qry);
      }
    }
    public query_result execute_query(client clie,
                                        query qry,
                                        int[] cols2ret,
                                        bool multithreading,
                                        string target) {
      using (var dal = control_DAL.instance(get_conns(clie, target))) {
        return dal.execute_query(clie, qry.sql, cols2ret, multithreading);
      }
    }
    private void process_footer(query_result resftr,
                                  StreamWriter obj_file,
                                  StringBuilder titleline) {
      var footertitles = string.Empty;
      var footervalues = string.Empty;
      var ftr = resftr.data;
      var frow = 0;
      var fcols = resftr.cols;
      footertitles = "<br><hr><table border='0' cellspacing='0' cellpadding='0' width='100%%'><tr>\n";
      foreach (var r in ftr) {
        var col = 0;
        foreach (var d in r) {
          var colwidth = fcols[col].name.Length;
          if (frow == 0) {
            titleline.Length = 0;
            titleline.AppendFormat("<td width='{0}'><b><font size='2' face='Arial'>{1}</font></b></td>\n",
                colwidth, resftr.cols[col++].name);
            footertitles += titleline.ToString();
          }
          titleline.Length = 0;
          titleline.AppendFormat("<td width='{0}'><font size='1' face='Arial'>{1}</font></td>\n",
                   colwidth, d);
          footervalues += titleline.ToString();
        }
        footertitles += "</tr>";
        footervalues += "</tr>";
        ++frow; ;
      }
      footervalues += "</table>";
      if (footertitles.Length > 0 && footervalues.Length > 0) {
        obj_file.Write(footertitles);
        obj_file.Write(footervalues);
      }
    }
    public filedownload generate_report(client clie,
                               query qrydtl,
                               query qryhdr,
                               query qryftr,
                               query qryft2,
                               memhelper mhelp,
                               string server,
                               string homepath,
                               string folderdestiny,
                               string reportid,
                               string machine,
                               string description,
                               string range,
                               string dtlformat,
                               string hdrformat,
                               string ftrformat,
                               bool dtlbycreate,
                               bool dtlbyreplace,
                               mroJSON fwidths,
                               mroJSON fsums,
                               string user,
                               string target) {
      // get data from the queries if any
      query_result reshdr = qryhdr != null ? execute_query(clie, qryhdr, null, false, target) : null;
      query_result resdtl = qrydtl != null ? execute_query(clie, qrydtl, null, false, target) : null;
      query_result resftr = qryftr != null ? execute_query(clie, qryftr, null, false, target) : null;
      query_result resft2 = qryft2 != null ? execute_query(clie, qryft2, null, false, target) : null;

      // some fundamental validations
      err.require(resdtl.cols.Count > cnts.MAXCOLSPERQRY, cme.TOO_MUCH_COLUMNS, resdtl.cols.Count);

      // generate the final output file according with some unique variables
      var filename = utils.gen_filename(machine, reportid, doctype.HTML);
      var destiny = utils.gen_destiny(homepath, folderdestiny, filename);
      using (var obj_file = new StreamWriter(destiny)) {
        // write first part
        if (dtlbycreate) {
          obj_file.Write("<HTML DIR=LTR>\n");
          obj_file.Write("<HEAD>\n<META HTTP-EQUIV=\"Content-Type\" CONTENT=\"text/html; charset=Windows-1252\">\n");
          obj_file.Write(string.Concat("<TITLE>report-", reportid, "</TITLE>\n</HEAD>\n<BODY>\n"));
          obj_file.Write(string.Format(hdrformat, description, user, DateTime.Now, machine,
                          string.Concat("report : |", reportid, "| ", description), range));
        }
        try {
          var headertitles = string.Empty;
          var headervalues = string.Empty;
          var ayuda = mhelp.getsbl0();
          var fields = mhelp.getsbl1();
          var helper = mhelp.getsbl2();
          var titleline = mhelp.getsbl3();
          var key = string.Empty;
          var fieldsw = mhelp.getint0();
          var sums = mhelp.getint1();
          var replacef = string.Empty;
          var buffer = new char[1024];

          if (dtlbycreate) {
            // header process if any
            if (reshdr != null) {
              var hdr = reshdr.data;
              var hrow = 0;
              var hcols = reshdr.cols;
              headertitles = "<hr><table border='0' cellspacing='0' cellpadding='0' width='100%%'><tr>\n";
              foreach (var r in hdr) {
                var col = 0;
                foreach (var d in r) {
                  var colwidth = hcols[col].name.Length;
                  if (hrow == 0) {
                    titleline.Length = 0;
                    titleline.AppendFormat("<td width='{0}'><b><font size='2' face='Arial'>{1}</font></b></td>\n",
                                            colwidth, reshdr.cols[col++].name);
                    headertitles += titleline.ToString();
                  }
                  titleline.Length = 0;
                  titleline.AppendFormat("<td width='{0}'><font size='1' face='Arial'>{1}</font></td>\n",
                                                   colwidth, d);
                  headervalues += titleline.ToString();
                }
                headertitles += "</tr>";
                headervalues += "</tr>";
                ++hrow;
              }
              headervalues += "</table><br/>";
              obj_file.Write(headertitles);
              obj_file.Write(headervalues);
            }

            // prepare the house keeping and helpers if any
            helper.Append("<td valign='left' width='30'><div align='left'><font size='1' face='Courier New'>reg<br></font></div></td>");
            for (var i = 0; i < resdtl.cols.Count; ++i) {
              // get the fields width
              key = utils.cols[i];
              fieldsw[i] = fwidths.getint(key);
              if (fieldsw[i] == 0) fieldsw[i] = 50;
              helper.AppendFormat("<td valign='left' width='{0}'><div align='left'><font size='1' face='Courier New'>" +
                                "{1}<br></font></div></td>\n", fieldsw[i], resdtl.cols[i].name);
              // get if we are gonna acumulate the field
              sums[i] = fsums.ison(key) ? 0 : -1;
            }
            fields.Append("<hr><TABLE BORDER=0 CELLSPACING=0 CELLPADDING=0><TR HEIGHT=2 >");
            fields.Append(helper);
            fields.Append("</TR></TABLE><hr>\n");
          }

          // detail process if any
          var linesperpage = 47;
          var row = 0;
          var page = 0;
          var dtl = resdtl.data;
          foreach (var r in dtl) {
            if (dtlbycreate) {
              if (row == 0) { // print header (break line)
                              //if (reshdr != null && headertitles.Length > 0 && headervalues.Length > 0)
                              //{
                              //	obj_file.Write(headertitles);
                              //	obj_file.Write(headervalues);
                              //}
                obj_file.Write(fields);
              }

              helper.Length = 0;
              helper.Append("<td valign='left' width='30'><div align='left'><font size='1' face='Courier New'>");
              helper.AppendFormat("{0}<br></font></div></td>\n", row + 1);
            }

            var col = 0;
            if (dtlbyreplace) replacef = dtlformat;
            foreach (var d in r) {
              if (dtlbycreate) {
                // do the totals for columns if any
                var coltype = resdtl.cols[col].type;
                if (coltype == 0 && sums[col] != -1 && string.IsNullOrEmpty(d) == false)
                  sums[col] += int.Parse(d);
                //if ((coltype == typeof(Int32) || coltype == typeof(Int16)) &&
                //	sums[col] != -1 && string.IsNullOrEmpty(d) == false)
                //	    sums[col] += int.Parse(d);

                helper.AppendFormat("<td valign='left' width='{0}'><div align='left'><font size='1' face='Courier New'>" +
                       "{1}<br></font></div></td>\n", fieldsw[col], d);
              }

              if (dtlbyreplace) replacef = utils.ReplaceEx(buffer, replacef, resdtl.cols[col].name, d);

              col++;
            }

            ayuda.Length = 0;
            if (dtlbycreate) {
              ayuda.Append("<TABLE BORDER=0 CELLSPACING=0 CELLPADDING=0><TR HEIGHT=2 >");
              ayuda.Append(helper);
              ayuda.Append("</TR></TABLE>\n");
              if (false) //condition for subtotals
              {
                //ayuda.Append("<TABLE BORDER=0 CELLSPACING=0 CELLPADDING=0><TR HEIGHT=2 >");
                //ayuda.Append(helper);
                //ayuda.Append("</TR></TABLE>\n");
              }
            }
            if (dtlbyreplace) ayuda.Append(replacef);
            obj_file.Write(ayuda);

            ++row;
            if (row > linesperpage) {
              row = 0;
              ++page;
            }
          }

          if (dtlbycreate) {
            if (row == 0 && page == 0) obj_file.Write("<font size='1' face='Courier New'>empty report</font>");
            else {
              if (fsums.isempty() == false) { // we have to show totals
                helper.Length = 0;
                helper.Append("<br><td valign='left' width='30'>" +
                                "<div align='left'><font size='1' face='Courier New'>" +
                                "tots<br></font></div></td>");
                var ayu = string.Empty;
                for (var col = 0; col < resdtl.cols.Count; ++col) {
                  bool havesum = sums[col] != -1;
                  if (havesum) ayu = sums[col].ToString();
                  else ayu = string.Empty;

                  helper.AppendFormat("<td valign='left' width='{0}'>" +
                                  "<div align='left'><font size='1' face='Courier New'>" +
                                  "{1}<br></font></div></td>", fieldsw[col], ayu);
                }
                ayuda.Length = 0;
                ayuda.Append("<TABLE BORDER=0 CELLSPACING=0 CELLPADDING=0><TR HEIGHT=2 >");
                ayuda.Append(helper);
                ayuda.Append("</TR></TABLE>\n");
                obj_file.Write(ayuda.ToString());
              }
            }

            // process the footer if any
            if (resftr != null) process_footer(resftr, obj_file, titleline);
            if (resft2 != null) process_footer(resft2, obj_file, titleline);

            obj_file.Write("</BODY></HTML>");
          }
        } 
        finally {
          obj_file.Close();
        }
      }

      var download = new filedownload();
      download.server = server;
      download.file = filename;
      download.type = doctype.HTML;
      download.folder = defs.TMPFOLDER;
      download.tofile = filename;
      return download;
    }
    /*private void delete_sheet(string file,string sheet)
    {
        Excel.Application xlApp;
        Excel.Workbook xlWorkBook;
        Excel.Worksheet xlWorkSheet;
        xlApp = new Excel.ApplicationClass();
        xlWorkBook = xlApp.Workbooks.Open(file);

        //xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(1);
        //xlWorkSheet.Delete();
        xlApp.DisplayAlerts = false;
        for (int i = xlApp.ActiveWorkbook.Worksheets.Count; i > 0; i--)
        {
            Worksheet wkSheet = (Worksheet)xlApp.ActiveWorkbook.Worksheets[i];
            if (wkSheet.Name == sheet)
            {
                wkSheet.Delete();
                break;
            }
        }
        xlApp.DisplayAlerts = true;

        xlWorkBook.Save();
        xlWorkBook.Close(true);
        xlApp.Quit();
        releaseObject(xlWorkSheet);
        releaseObject(xlWorkBook);
        releaseObject(xlApp);
    }*/
    /*private void releaseObject(object obj)
  {
     try
     {
        System.Runtime.InteropServices.Marshal.ReleaseComObject(obj);
        obj = null;
     }
     catch (Exception ex) { obj = null; }
     finally { GC.Collect(); }
  }*/
    public void compose_workbook(mroJSON values,
                                OleDbCommand cmd,
                                string sheet,
                                StringBuilder query,
                                ref bool tblcreated) {
      var data = values.getobj(defs.ZDATA);
      var cols = data.getobj(defs.ZCOLS);
      var typs = data.getobj(defs.ZTYPES);
      int nrows = data.getint(defs.ZNROWS);
      int ncols = data.getint(defs.ZNCOLS);

      char letter = 'A';
      query.Append("CREATE TABLE Sheet1 (");
      for (int j = 0; j < ncols; ++j, ++letter) {
        // we add column number to avoid fields same name conflict
        var field = string.Concat(cols.get(letter.ToString()), j.ToString());
        // we eliminate spaces to avoid fields name format
        field = field.Replace(' ', '_');
        // we change numbers at begining to avoid fields name format
        if (char.IsDigit(field[0])) field = "_" + field;

        query.Append('[');
        query.Append(field);
        query.Append("] ");
        query.Append("nvarchar");
        query.Append(',');
      }
      if (ncols > 0) query.Length--; // eliminate the last comma
      query.Append(')');
      cmd.CommandText = query.ToString();
      cmd.ExecuteNonQuery();

      tblcreated = true;
      var tbl = string.Concat("INSERT INTO [", sheet, "] values(");
      for (int i = 0; i <= nrows; ++i) {
        if (i > 256000) break; // safety break
        query.Length = 0;
        query.Append(tbl);
        int col = 0;
        letter = 'A';

        for (int j = 0; j < ncols; ++j, ++letter) {
          string key = string.Format("{0}{1}", letter, i);
          string value = data.get(key);
          query.Append('\'');
          query.Append(value.Replace('\'', ' ')); // remove quotes in query
          query.Append("\',");
          ++col;
        }
        if (col > 0) query.Length--; // eliminate the last comma
        query.Append(')');

        cmd.CommandText = query.ToString();
        cmd.ExecuteNonQuery();
      }
    }
    public filedownload generate_workbook(client clie,
                                           query qry,
                                           memhelper mhelp,
                                           string server,
                                           string homepath,
                                           string foldersource,
                                           string folderdestiny,
                                           string sheet,
                                           string template,
                                           string machine,
                                           string target) {
      bool iscompose = qry == null;

      query_result res = null;
      Thread thread = null;
      if (!iscompose) {
        thread = new Thread(() => {
          res = execute_query(clie, qry, null, true, target);
        });
        thread.Start();
      }

      var ext = ".xls";
      if (template.IndexOf(".") != -1) ext = ""; //contains its own extension

      // check template
      var source = string.Format("{0}{1}\\{2}{3}", homepath, foldersource, template, ext);
      err.require(!File.Exists(source), cme.FILE_NOT_FOUND, source);

      // create new file 
      var filename = utils.gen_filename(machine, template, ext);
      var destiny = utils.gen_destiny(homepath, folderdestiny, filename);

      if (File.Exists(destiny)) File.Delete(destiny);
      File.Copy(source, destiny);

      FileInfo fileInfo = new FileInfo(destiny);
      if (fileInfo.IsReadOnly == true)
        fileInfo.IsReadOnly = false;

      string strConn = ext == "" ? // xlsx
              string.Concat("Provider=Microsoft.ACE.OLEDB.12.0;Data Source=", destiny,
                      ";Extended Properties=\"Excel 12.0 Xml;HDR=Yes;\"") :
              string.Concat("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=", destiny,
                      ";Extended Properties=\"Excel 8.0;HDR=Yes;\"");
      OleDbConnection oleConn = new OleDbConnection(strConn);

      var isgeneric = false;
      var tblcreated = false;
      var query = mhelp.getsbl0();
      var cmd = new OleDbCommand();
      int tblcols = -1;

      try {
        oleConn.Open();
        cmd.Connection = oleConn;

        if (iscompose) {
          compose_workbook(clie.values, cmd, sheet, query, ref tblcreated);
        }
        else {
          thread.Join();

          if (!iscompose && res.error != null)
            throw new Exception("", res.error); // could be trigger in thread

          isgeneric = string.CompareOrdinal(template, defs.WBKGENERIC) == 0 && res.cols.Count > 0;
          if (isgeneric) {
            int cols = 0;
            query.Append("CREATE TABLE Sheet1 (");
            var rescols = res.cols;
            foreach (var cl in rescols) {
              if (cl.name.Length != 0) query.Append(mem.join3('[', cl.name, ']'));
              else query.Append(cl.id);
              query.Append(' ');
              query.Append(cl.field_type);
              query.Append(',');
              ++cols;
            }
            if (cols > 0) query.Length--; // to eliminate the last comma
            query.Append(')');

            cmd.CommandText = query.ToString();
            cmd.ExecuteNonQuery();
          }

          tblcreated = true;
          var row = 1;
          var tbl = mem.join3("INSERT INTO [", sheet, "] values(");
          var colint = new bool[res.cols.Count];

          foreach (var r in res.data) {
            if (row > 256000) break; // safety break

            query.Length = 0;
            query.Append(tbl);
            int col = 0;

            foreach (var d in r) {
              if (col > 128) break; // safety break

              var coltype = res.cols[col].type;
              if (row == 1 && coltype == 0) colint[col] = true;
              //if (row == 1 && res.cols[col].type == typeof(Int32) ||
              //    res.cols[col].type == typeof(Int16)) colint[col] = true;

              if (colint[col]) {
                query.Append(d);
                query.Append(',');
              }
              else {
                query.Append('\'');
                query.Append(d);
                query.Append("\',");
              }
              ++col;
            }

            if (col > 0) query.Length--; // to eliminate the last comma
            query.Append(')');

            cmd.CommandText = query.ToString();
            cmd.ExecuteNonQuery();

            ++row;
          }
        }
      } 
      catch (Exception e) {
        if (tblcreated) {
          cmd.CommandText = mem.join3("select * FROM [", sheet, "]");
          IDataReader rrr = cmd.ExecuteReader();
          if (rrr != null) tblcols = rrr.FieldCount;

          char[] delimiters = new char[] { ')' };
          var s = query.ToString().Split(delimiters, 1);
          var inf = (!iscompose && res != null) ?
              "(qry:" + res.cols.Count.ToString() + "),(tbl:" + tblcols.ToString() + "):" :
              string.Empty;
          throw new Exception(string.Concat(s.Length > 0 ? s[0] : "", ":", inf, e.Message));
        }
        else throw;
      } 
      finally {
        oleConn.Close();
      }

      //if (isgeneric || iscompose) // this is eliminate the nasty first sheet
      //{
      //    delete_sheet(destiny,"sheet2");
      //}

      var download = new filedownload();
      download.server = server;
      download.file = filename;
      download.type = doctype.EXCEL;
      download.folder = defs.TMPFOLDER;
      download.tofile = filename;
      return download;
    }

    public StringBuilder compose_html_base(client clie, ListResponse dataset) {
      var data = clie.values.getobj(defs.ZDATA);
      var cols = data.getobj(defs.ZCOLS);
      var byset = dataset != null;
      int nrows = byset ? dataset.get_nrows() : data.getint(defs.ZNROWS);
      int ncols = byset ? dataset.get_ncols() : data.getint(defs.ZNCOLS);
      var set = byset ? dataset.result : null;
      var lstid = byset ? dataset.get_lstid() : -1;
      var listid = byset ? mem.join2('l', lstid.ToString()) : string.Empty;

      var line = new StringBuilder();

      line.Append("<HTML>\n");
      line.Append("<HEAD>\n");
      line.Append(mem.join3("<TITLE>", config.company, "</TITLE>\n"));
      line.Append("</HEAD>\n");
      line.Append("<BODY>\n");
      line.Append("<TABLE style=\"font-family:Consolas;font-size:12px;\">\n");

      line.Append("<tr>\n");
      char letter = 'A';
      for (int j = 0; j < ncols; ++j, ++letter) {
        // we add column number to avoid fields same name conflict
        line.Append("<td>");
        line.Append(mem.join2(cols.get(letter.ToString()), j.ToString()));
        line.Append("</td>\n");
      }
      line.Append("</tr>\n");

      StringBuilder key = new StringBuilder();
      for (int i = 0; i <= nrows; ++i) {
        if (i > 256000) break; // safety break

        line.Append("<tr>\n");
        int col = 0;
        letter = 'A';
        for (int j = 0; j < ncols; ++j, ++letter) {
          line.Append("<td>");
          key.Length = 0;
          if (byset) {
            key.Append(listid);
            key.Append(i.tostr());
            key.Append(letter);
            line.Append(set.get(key));
          }
          else {
            key.Append(letter);
            key.Append(i.tostr());
            line.Append(data.get(key));
          }
          line.Append("</td>\n");
          ++col;
        }
        line.Append("</tr>\n");
      }
      line.Append("</TABLE>\n");
      line.Append("</BODY>\n");
      line.Append("</HTML>\n");

      return line;
    }
    public filedownload compose_pdf(client clie,
                                    ListResponse dataset,
                                    string server,
                                    string homepath,
                                    string folderdestiny,
                                    string machine) {
      // generate the final output file according with some unique variables
      var filename = utils.gen_filename(machine, "", doctype.HTML);
      var destiny = utils.gen_destiny(homepath, folderdestiny, filename);

      using (var obj_file = new StreamWriter(destiny)) {
        try {
          obj_file.Write(compose_html_base(clie, dataset).ToString());
        } 
        finally {
          obj_file.Close();
        }
      }

      var o = filename;
      var n = filename.Replace(".html", ".pdf");
      System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
      psi.RedirectStandardOutput = true;
      psi.UseShellExecute = false;
      psi.CreateNoWindow = true;
      psi.FileName = string.Concat(config.home, "wkhtmltopdf.exe");
      psi.WorkingDirectory = config.home;
      psi.Arguments = folderdestiny + "\\" + o + "  " + folderdestiny + "\\" + n;
      var proc = System.Diagnostics.Process.Start(psi);
      filename = n;

      var download = new filedownload();
      download.server = server;
      download.file = filename;
      download.type = doctype.PDF;
      download.folder = defs.TMPFOLDER;
      download.tofile = filename;

      // important we need to wait until the process if finished, otherwise
      // the downloader will not find it, cause the process is asyncronous
      proc.WaitForExit();

      return download;
    }

    public filedownload generate_pdf(client clie,
                                     query qrydtl,
                                     string server,
                                     string homepath,
                                     string foldersource,
                                     string folderdestiny,
                                     string template,
                                     string machine,
                                     string target) {
      query_result resdtl = qrydtl != null ?
         execute_query(clie, qrydtl, null, false, target) : null;

      var ext = ".pdf";
      if (template.IndexOf(".") != -1) ext = ""; //contains its own extension

      // check template
      var source = string.Format("{0}{1}\\{2}{3}",
         homepath, foldersource, template, ext);
      err.require(!File.Exists(source), cme.FILE_NOT_FOUND, source);

      // create new file 
      var filename = utils.gen_filename(machine, template, ext);
      var destiny = utils.gen_destiny(homepath, folderdestiny, filename);

      IronPdf.PdfDocument pdf = new IronPdf.PdfDocument(source);

      var row = 0;
      var dtl = resdtl.data;
      foreach (var r in dtl) {
        var top = 0;
        var lft = 0;
        var val = "";
        var col = 0;
        foreach (var d in r) {
          if (col == 0) top = int.Parse(d);
          else if (col == 1) lft = int.Parse(d);
          else if (col == 2) val = d;
          col++;
        }

        var ForegroundStamp = new HtmlStamp() {
          Html = "<h2 style='color:black'>" + val,
          Top = top,
          Left = lft,
          Width = 50,
          Height = 50,
          Opacity = 50,
          //Rotation = -45, 
          ZIndex = HtmlStamp.StampLayer.OnTopOfExistingPDFContent
        };
        pdf.StampHTML(ForegroundStamp);

        row++;
      }
      pdf.SaveAs(destiny);

      var download = new filedownload();
      download.server = server;
      download.file = filename;
      download.type = doctype.PDF;
      download.folder = defs.TMPFOLDER;
      download.tofile = filename;
      return download;
    }
    #region email
    /*public void compose_email(  link lnk, client clie,
                string from,
                                  string to,
                                  string cc,
                                  string subject,
                                  string body,
                                  string attach
                              )
      {
          var em = new email(from, to, cc, subject, body, attach);
    using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb())))
    { dal.compose_email(em); }
      }*/
    #endregion

    public filedownload compose_html(client clie,
                                      ListResponse dataset,
                                      string server,
                                      string homepath,
                                      string folderdestiny,
                                      string machine) {
      // generate the final output file according with some unique variables
      var filename = utils.gen_filename(machine, "", doctype.HTML);
      var destiny = utils.gen_destiny(homepath, folderdestiny, filename);

      using (var obj_file = new StreamWriter(destiny)) {
        try {
          obj_file.Write(compose_html_base(clie, dataset).ToString());
        } 
        finally {
          obj_file.Close();
        }
      }

      var download = new filedownload();
      download.server = server;
      download.file = filename;
      download.type = doctype.HTML;
      download.folder = defs.TMPFOLDER;
      download.tofile = filename;
      return download;
    }

    public filedownload generate_label(client clie,
                                        query qry,
                                        string template,
                                        string filenamedestiny,
                                        string server,
                                        string homepath,
                                        string folderdestiny,
                                        string machine,
                                        string user,
                                        ref shell shl,
                                        string target) {
      query_result res = null;
      Thread thread = new Thread(() => {
        res = execute_query(clie, qry, null, true, target);
      });
      thread.Start();

      var filename = utils.gen_filename(machine, "txtfile", doctype.HTML);
      var destiny = utils.gen_destiny(homepath, folderdestiny, filename);

      using (var obj_file = new StreamWriter(destiny)) {
        thread.Join();

        if (res.error != null)
          throw new Exception("", res.error); // could be trigger in thread

        var ncols = res.cols.Count;

        if (ncols > cnts.MAXCOLSPERQRY)
          err.require(cme.TOO_MUCH_COLUMNS, string.Format(":{0}-{1}", ncols, 63));

        var mcols_types = new Type[64];
        var sums = new long[64];
        var replacef = template;
        var buffer = new char[1024 * 4];

        try {
          foreach (var r in res.data) {
            int col = 0;
            var resdata = res.data;
            foreach (var d in r) {
              replacef = utils.ReplaceEx(buffer, replacef, res.cols[col].name, d);
              ++col;
            }
          }
          obj_file.Write(replacef);
        } 
        finally {
          obj_file.Close();
        }
      }

      var download = new filedownload();
      download.server = server;
      download.file = filename;
      download.type = doctype.TEXT;
      download.folder = defs.TMPFOLDER;
      download.topath = filename;

      var s = new StringBuilder();
      var lpt1 = CParameters.gen_pair("printlpt1", defs.TMPFOLDER + "\\" + filename, s);
      clie.result.set(defs.ZLOCACT, lpt1);

      return download;
    }

    public filedownload generate_text_file(client clie,
                                           query qry,
                                           string filenamedestiny,
                                           string server,
                                           string homepath,
                                           string folderdestiny,
                                           string machine,
                                           string user,
                                           ref shell shl,
                                           string target)

    // Esto es para implementar la funcionalidad de reportes por codigo
    // incluir un parametro con el nombrfe de la funcion
    // para sdeterminar que si no esta vacio, corra por reflection esa funcion y no la del query
    // esa funcion debe tener la specificacion, uqery_result como resultado y como parametro un objeto CParameters
    {
      query_result res = null;
      Thread thread = new Thread(() => {
        res = execute_query(clie, qry, null, true, target);
      });
      thread.Start();

      var filename = utils.gen_filename(machine, "txtfile", doctype.HTML);
      var destiny = utils.gen_destiny(homepath, folderdestiny, filename);

      using (var obj_file = new StreamWriter(destiny)) {
        thread.Join();

        if (res.error != null)
          throw new Exception("", res.error); // could be trigger in thread

        var ncols = res.cols.Count;

        if (ncols > cnts.MAXCOLSPERQRY)
          err.require(cme.TOO_MUCH_COLUMNS, string.Format(":{0}-{1}", ncols, 63));

        //var mcols_types = new int[64];// Type[64];
        var sums = new long[64];

        try {
          //for (var i = 0; i < res.cols.Count; ++i)
          //{
          //var type = res.cols[i].type;
          //mcols_types[i] = type;
          //}

          var helper = new StringBuilder(string.Empty);
          var resdata = res.data;
          foreach (var r in resdata) {
            helper.Length = 0;

            var col = 0;
            foreach (var d in r) {
              //Type coltype = res.cols[col].type;
              helper.Append(d);
              helper.Append('\t');
              col++;
            }
            helper.Append("\r\n"); // break line

            obj_file.Write(helper);
          }
        } 
        finally {
          obj_file.Close();
        }
      }

      var download = new filedownload();
      download.server = server;
      download.file = filename;
      download.type = doctype.TEXT;
      download.folder = defs.TMPFOLDER;
      download.topath = filenamedestiny;

      shl.path = filenamedestiny;

      return download;
    }

    public company get_company(link lnk, client clie, int cmpyid) {
      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        return dal.get_company(cmpyid);
      }
    }
    public int cmpy_gen_new_id(link lnk, client clie) {
      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        return dal.cmpy_gen_new_id();
      }
    }
    public void insert_company(link lnk, client clie, company cpmy) {
      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        dal.insert_company(clie, cpmy);
      }
    }

    public user get_user(link lnk, client clie, string u) {
      err.require(string.IsNullOrEmpty(u), cme.INC_DATA_USER);
      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        return dal.get_user(u);
      }
    }
    public void insert_user(link lnk, client clie, user usr) {
      usr.validate();
      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        dal.insert_user(clie, usr);
      }
    }
    public void update_user(link lnk, client clie, user usr) {
      usr.validate();
      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        dal.update_user(clie, usr);
      }
    }
    public void delete_user(link lnk, client clie, string uid) {
      err.require(string.IsNullOrEmpty(uid), cme.INC_DATA_USER);
      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        dal.delete_user(clie, uid);
      }
    }
    /*public List<right> get_rights(link lnk, client clie, int cmpy, user u)
  {
    err.require(string.IsNullOrEmpty(u.id), cme.INC_DATA_USER);
    using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb())))
    { return dal.get_rights(cmpy, u); }
  }*/

    public document get_document(link lnk, client clie, string lib, string id, string type) {
      err.require(string.IsNullOrEmpty(lib), cme.INC_DATA_LIBRARY);
      err.require(string.IsNullOrEmpty(id), cme.INC_DATA_DOCUMENT);
      err.require(string.IsNullOrEmpty(type), cme.INC_DATA_TYPE);
      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        return dal.get_document(lib, id, type);
      }
    }
    public void update_document(link lnk, client clie, document d, devpackage p, user u) {
      d.validate();
      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        var devhdr = dal.get_pack_hdr(p);
        err.require(devhdr == null, cme.DEV_PACK_NOT_EXIST);
        err.require(string.CompareOrdinal(devhdr.status, devstatus.WIP) != 0,
                 cme.DEV_PACK_NOT_IN_WIP);
        var devdtl = dal.get_pack_dtl(p, d, u);
        err.require(devdtl == null, cme.DEV_PACK_DOC_NOT_BELONG);
        dal.update_document(d);
      }
    }
    public void insert_document(link lnk, client clie, document d, devpackage p, user u) {
      d.validate();
      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        var devhdr = dal.get_pack_hdr(p);
        err.require(devhdr == null, cme.DEV_PACK_NOT_EXIST);
        err.require(string.CompareOrdinal(devhdr.status, devstatus.WIP) != 0,
                 cme.DEV_PACK_NOT_IN_WIP);
        var devdtl = dal.get_pack_dtl(p, d, u);
        err.require(devdtl == null, cme.DEV_PACK_DOC_NOT_BELONG);
        dal.insert_document(d);
      }
    }

    public void ses_save_query(link lnk, int seq, string qry, string websrv) {
      var clie = lnk.clie;
      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        dal.ses_save_query(clie.zsesins, clie.zsesmac, clie.zsescli, clie.zsesses,
           seq, qry, websrv);
      }
    }
    public void ses_get_query(link lnk) {
      var values = lnk.values;
      int ins = values.getint("ins");
      int mac = values.getint("mac");
      int cli = values.getint("cli");
      int ses = values.getint("ses");
      var txt = string.Empty;
      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        txt = dal.ses_get_query(ins, mac, cli, ses);
      }
      lnk.result.set("text", txt);
    }

    /*public void notify_framework(link lnk, client clie, string model, string addr, string port, string service, string type,
                string comp, string func, CParameters parms, 
                bool donesrc)
  {
    try
    {
      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb())))
      {
        dal.notify_framework(model, addr, port, service, type, comp, func, parms, donesrc);
      }
    }
    catch(Exception e) { }
  }*/

    // Generates a random string with a given size.    
    public string RandomString(int size, bool lowerCase = false) {
      var builder = new StringBuilder(size);

      // Unicode/ASCII Letters are divided into two blocks
      // (Letters 65–90 / 97–122):
      // The first group containing the uppercase letters and
      // the second group containing the lowercase.  
      var rnd = new Random();

      // char is a single Unicode character  
      char offset = lowerCase ? 'a' : 'A';
      const int lettersOffset = 26; // A...Z or a..z: length=26  

      for (var i = 0; i < size; i++) {
        var @char = (char)rnd.Next(offset, offset + lettersOffset);
        builder.Append(@char);
      }

      return lowerCase ? builder.ToString().ToLower() : builder.ToString();
    }
    #region accounts
    public void create_company(link lnk) {
      var clie = lnk.clie;
      var values = lnk.values;

      // mandatories
      var cmpyn = values.get("cmpyname");
      var ident = values.get("id");
      var pass = values.get("password");
      // optionals
      var address1 = values.get("address1");
      var address2 = values.get("address2");
      var country = values.get("country");
      var city = values.get("city");
      var state = values.get("state");
      var zipcode = values.get("zipcode");

      // validations
      err.require(cmpyn.Length == 0, cme.INC_DATA_CMPY_NAME);
      err.require(ident.Length == 0, cme.INC_DATA_USER);
      err.require(pass.Length == 0, cme.INC_DATA_PASS);

      var usrid = string.Empty;
      var email = string.Empty;
      var utype = "O";
      var etype = 2;

      // we encrypt the password 
      //values.set("cpassini", pass);
      //encrypt_password(lnk);
      //var realpass = lnk.clie.newval.get("$encpassword$");

      int cmpyid = cmpy_gen_new_id(lnk, clie);
      var cmpyst = cmpyid.ToString();

      try {

        using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
          // for what ever id is given, get the real userid
          usrid = dal.user_get_credentials(ident);
          err.require(usrid.Length == 0, cme.USER_NOT_REGISTERED);

          // check is user registration
          var u = dal.user_check_pass(usrid, _encrypt_pass(pass));
          err.require(!u, cme.USER_NOT_EXIST);

          // extract email and name from real user
          var usr = dal.get_user(usrid);
          email = usr.email;
          err.require(email.Length == 0, cme.USER_NOT_EMAIL_REG);

          // create the company
          var cmp = new company(cmpyid);
          cmp.name = cmpyn;
          cmp.owner = 0;
          cmp.sector = 0;
          cmp.legalID = "";
          cmp.country = country;
          cmp.state = state;
          cmp.city = city;
          cmp.address1 = address1;
          cmp.address2 = address2;
          cmp.zipcode = zipcode;
          insert_company(lnk, clie, cmp);

          // create the basics enums
          dal.enums_create(clie, cmpyid, "1", "1", "", "");

          // create the basic master data
          dal.masterdata_create(clie, cmpyid, "1", "1");

          // a new entity for the owner (as employee)
          var entid = dal.entity_create(cmpyid, usr.description, etype);

          // set the default email as contact
          dal.entity_email_set(entid, email);

          // set the new entity as the owner for this company
          dal.cmpy_set_owner(clie, cmpyid, entid);

          // link user, enity and cmpy together
          dal.user_cmpy_insert(clie, cmpyid, usrid, utype, entid, 0, 0, 0);

          // create a company's library
          dal.lib_insert(clie, cmpyst, cmpyn);

          // create a company profile libs (header)
          dal.lib_grp_insert(clie, cmpyst, cmpyn);

          // create a company profile libs (detail)
          dal.lib_grp_dtl_insert(clie, cmpyst, "KERNEL");
          dal.lib_grp_dtl_insert(clie, cmpyst, "KERNELDB");
          dal.lib_grp_dtl_insert(clie, cmpyst, "INCLUDES");
          dal.lib_grp_dtl_insert(clie, cmpyst, "PRDDB");
          dal.lib_grp_dtl_insert(clie, cmpyst, "ORG");
          dal.lib_grp_dtl_insert(clie, cmpyst, "PRODUCT");
          dal.lib_grp_dtl_insert(clie, cmpyst, "SALES");
          dal.lib_grp_dtl_insert(clie, cmpyst, "CRM");
          dal.lib_grp_dtl_insert(clie, cmpyst, cmpyst);

          // link user and lib cmpy together
          dal.lib_grp_usr_insert(clie, cmpyid, cmpyst, usrid);

          // create basics rights for the user
          dal.rights_create(clie, cmpyid, usrid, utype);

          // add the ENTREP profile
          dal.rights_insert(clie, cmpyid, usrid, "ENTREP", "1", "", "", "", "", "", "", "");

          // process his trees menus
          dal.tree_user_process(lnk.clie, cmpyid, usrid, "ENTREP");
        }
      } 
      catch {
        using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
          // when any problem delete the garbage
          dal.cmpy_delete(clie, cmpyid);
        }
        throw;
      }

      // send the welcome email
      var body = "WELCOME<br/><br/>";
      body += "You are now part of a community that connects<br/>";
      body += "micro and small business to clients accros the world<br/><br/>";

      body += "As Entrepreneur, a world of opportunities are now opened!<br/><br/>";

      body += "These are your Company codes<br/>";
      body += "Company: " + cmpyid + "<br/>";
      body += "Email: " + email + "<br/>";
      body += "User: " + usrid + "<br/>";

      var fr = "Company Registration service";
      var to = email;
      var cc = "";
      var sb = "Registration";
      var bo = body;
      var at = "";

      // to the new user
      mail.compose_email(config.dbcode, fr, to, cc, sb, bo, at, 0, false);
      // to the guru
      mail.compose_email(config.dbcode, fr, "mikerodtjlp@gmail.com", cc, sb, bo, at, 0, false);
      // to the company
      mail.compose_email(config.dbcode, fr, "pacific@coromuel.mx", cc, sb, bo, at, 0, false);
    }
    public void create_account(link lnk) {
      var clie = lnk.clie;
      var values = lnk.values;

      var first = values.get("firstname");
      var lastn = values.get("lastname");
      var uname = first + " " + lastn;
      var addr1 = values.get("address1");
      var addr2 = values.get("address2");
      var city_ = values.get("city");
      var state = values.get("state");
      var zipcd = values.get("zipcode");

      var usrid = values.get("userid");
      var email = values.get("email");
      var rmail = values.get("repemail");
      var phone = values.get("phone");
      var utype = "N";

      // validations
      err.require(first.Length == 0, cme.INC_DATA_FIRST_NAME);
      err.require(lastn.Length == 0, cme.INC_DATA_LAST_NAME);
      err.require(email.Length == 0, cme.INC_DATA_EMAIL);
      err.require(phone.Length == 0, cme.INC_DATA_PHONE);
      err.require(string.CompareOrdinal(email, rmail) != 0, cme.EMAILS_ARE_DIFF);

      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        // validate new credentials
        if (usrid.Length != 0)
          err.require(dal.exist_user(usrid), cme.USER_ALREADY_EXIST);
        err.require(dal.exist_email(email), cme.EMAIL_ALREADY_EXIST);
        err.require(dal.exist_phone(phone), cme.PHONE_ALREADY_EXIST);
      }

      // generate password
      var pass = RandomString(10, true);
      //values.set("cpassini", pass);
      //encrypt_password(lnk);
      //var realpass = lnk.clie.newval.get("$encpassword$");

      try {
        var now = DateTime.Now;

        // create the user
        var obj = new user(usrid);
        obj.description = uname;
        obj.comments = "";
        obj.password = _encrypt_pass(pass);
        obj.date_start = now;
        obj.date_end = now.AddMonths(1);
        obj.type = utype;
        obj.time_start = now;
        obj.time_end = now;
        obj.groupid = "";
        obj.email = email;
        obj.winuser = "";
        obj.phone = phone;

        insert_user(lnk, clie, obj);
      } 
      catch {
        using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
          // when any problem delete the garbage
          dal.delete_user(clie, usrid);
        }
        throw;
      }

      // send the welcome email
      var body = "WELCOME<br/><br/>";
      body += "You are now part of a community that connects<br/>";
      body += "micro and small business to clients accros the world<br/><br/>";

      body += "As a User, a world of opportunities are now opened!<br/><br/>";

      body += "These are your credentials<br/>";
      body += "Email: " + email + "<br/>";
      body += "User: " + usrid + "<br/>";
      body += "Password: " + pass + "<br/>";

      var fr = "Account Registration service";
      var to = email;
      var cc = "";
      var sb = "Registration";
      var bo = body;
      var at = "";

      // to the new user
      mail.compose_email(config.dbcode, fr, to, cc, sb, bo, at, 0, false);
      // to the guru
      mail.compose_email(config.dbcode, fr, "mikerodtjlp@gmail.com", cc, sb, bo, at, 0, false);
      // to the company
      mail.compose_email(config.dbcode, fr, "pacific@coromuel.mx", cc, sb, bo, at, 0, false);
    }
    public void join_company(link lnk) {
      var clie = lnk.clie;
      var values = lnk.values;

      var cmpyid = values.getint("company", -1);
      var ident = values.get("id");
      var pass = values.get("password");

      var email = string.Empty;
      var utype = "C";
      var etype = 1;

      err.require(cmpyid == -1, cme.INC_DATA_CMPY);
      err.require(ident.Length == 0, cme.INC_DATA_USER);
      err.require(pass.Length == 0, cme.INC_DATA_PASS);

      // we encrypt the password 
      //values.set("cpassini", pass);
      //encrypt_password(lnk);
      //var realpass = lnk.clie.newval.get("$encpassword$");

      using (var dal = control_DAL.instance(get_conns(clie, lnk.get_appdb()))) {
        // validate the company
        err.require(dal.get_company(cmpyid) == null, cme.CMPY_NOT_EXIST);

        // for what ever id is given, get the real userid
        var usrid = dal.user_get_credentials(ident);
        err.require(usrid.Length == 0, cme.USER_NOT_REGISTERED);

        // check is user registration
        var u = dal.user_check_pass(usrid, _encrypt_pass(pass));
        err.require(!u, cme.USER_NOT_EXIST);

        // extract name from real user
        var usr = dal.get_user(usrid);
        email = usr.email;
        err.require(email.Length == 0, cme.USER_NOT_EMAIL_REG);

        //check if user already is joined
        bool joined = dal.user_cmpy_check(cmpyid, usrid);
        err.require(joined, cme.USER_ALREADY_JOINED);

        // a new entity for the customer (as customer)
        var entid = dal.entity_create(cmpyid, usr.description, etype);

        // set the default email as contact
        dal.entity_email_set(entid, email);

        // link user, entity and cmpy together
        dal.user_cmpy_insert(clie, cmpyid, usrid, utype, 0, entid, 0, 0);

        // link user and lib cmpy together
        dal.lib_grp_usr_insert(clie, cmpyid, cmpyid.ToString(), usrid);

        // create basics rights for the user
        dal.rights_create(clie, cmpyid, usrid, utype);

        // add the SALES ON LINE profile
        dal.rights_insert(clie, cmpyid, usrid, "SALONL", "1", "", "", "", "", "", "", "");

        // process his trees menus
        dal.tree_user_process(lnk.clie, cmpyid, usrid, "SALONL");
      }
    }
    #endregion

    #region documents

    public void upload_document_to_db(client clie,
                                        string rf,
                                        string name,
                                        string path,
                                        string target) {
      using (var dal = control_DAL.instance(get_conns(clie, target))) {

        byte[] file;
        var contentType = Path.GetExtension(path);
        using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read)) {
          using (var reader = new BinaryReader(stream)) {
            file = reader.ReadBytes((int)stream.Length);
          }
        }
        dal.upload_document_to_db(rf, name, contentType, file);
      }
    }
    public filedownload download_document_from_db(client clie,
                                                    string server,
                                                    string rf,
                                                    string name,
                                                    string homepath,
                                                    string folderdestiny,
                                                    string target) {
      var filename = name;
      var destiny = string.Format("{0}{1}\\{2}", homepath, folderdestiny, filename);
      using (var dal = control_DAL.instance(get_conns(clie, target))) {

        byte[] file = dal.download_document_from_db(rf, filename);
        using (var obj_file = new BinaryWriter(File.Open(destiny, FileMode.Create))) {
          try {
            obj_file.Write(file);
          } 
          finally {
            obj_file.Close();
          }
        }
      }

      var download = new filedownload();
      download.server = server;
      download.file = filename;
      //download.type   = doctype.HTML;
      download.folder = defs.TMPFOLDER;
      download.tofile = filename;
      return download;
    }

    #endregion

    #region implementation

    public void update_document(link lnk) {
      var values = lnk.values;

      var lib = string.Empty;
      err.require(values.get("clib", ref lib) == 0, cme.INC_DATA_LIBRARY);
      var doc = string.Empty;
      err.require(values.get("cname", ref doc) == 0, cme.INC_DATA_DOCUMENT);
      var data = new StringBuilder();
      err.require(values.get("cdata", data) == 0, cme.INC_DATA_CONTENT);
      var type = string.Empty;
      err.require(values.get("ctype", ref type) == 0, cme.INC_DATA_TYPE);
      var devpack = string.Empty;
      err.require(values.get("devpack", ref devpack) == 0, cme.INC_DATA_DEVPACK);
      var userid = string.Empty;
      err.require(lnk.basics.get(defs.ZUSERID, ref userid) == 0, cme.INC_DATA_USER);

      // trick for avoiding html injection problem
      int n = data.Length;
      int a = -1;
      for (int i = 0; i < n; ++i) {
        if (data[i] == '<') {
          for (int j = i; j < n; ++j) {
            if (data[j] == '@') { a = i; goto done; }
          }
        }
      }
    done:
      if (a != -1) data = data.Replace("<@:", "<", a, n - a);

      var obj = get_document(lnk, lnk.clie, lib, doc, type);
      err.require(obj == null || obj.data.Length == 0, cme.REG_NOT_EXIST);

      obj.data = data.ToString();
      obj.type = type;
      ++obj.version;
      obj.date_modify = new DateTime();
      update_document(lnk, lnk.clie, obj, new devpackage(devpack), new user(userid));
    }
    public void create_document(link lnk) {
      var values = lnk.values;

      var lib = string.Empty;
      err.require(values.get("clib", ref lib) == 0, cme.INC_DATA_LIBRARY);
      var doc = string.Empty;
      err.require(values.get("cname", ref doc) == 0, cme.INC_DATA_DOCUMENT);
      var data = new StringBuilder();
      err.require(values.get("cdata", data) == 0, cme.INC_DATA_CONTENT);
      var type = string.Empty;
      err.require(values.get("ctype", ref type) == 0, cme.INC_DATA_TYPE);
      var devpack = string.Empty;
      err.require(values.get("devpack", ref devpack) == 0, cme.INC_DATA_DEVPACK);
      var userid = string.Empty;
      err.require(lnk.basics.get(defs.ZUSERID, ref userid) == 0, cme.INC_DATA_USER);

      // trick for avoiding html injection problem
      int n = data.Length;
      int a = -1;
      for (int i = 0; i < n; ++i) {
        if (data[i] == '<') {
          for (int j = i; j < n; ++j) {
            if (data[j] == '@') { a = i; goto done; }
          }
        }
      }
    done:
      if (a != -1) data = data.Replace("<@:", "<", a, n - a);

      var obj = get_document(lnk, lnk.clie, lib, doc, type);
      err.require(obj != null && obj.data.Length > 0, cme.REG_ALREADY_EXIST);

      obj = new mro.BO.document(lib, doc);
      obj.data = data.ToString();
      obj.type = type;
      obj.version = 1;
      obj.date_create = new DateTime();
      obj.date_modify = new DateTime();
      insert_document(lnk, lnk.clie, obj, new devpackage(devpack), new user(userid));
    }

    public void deploy_create(link lnk) {
      var values = lnk.values;
      var data = values.getobj(defs.ZDATA);
      var pack = values.get("$package$");
      int nrows = data.getint(defs.ZNROWS);
      int ncols = data.getint(defs.ZNCOLS);

      err.require(pack.Length == 0, cme.INC_DATA_ID);

      var type = string.Empty;
      var userid = string.Empty;
      var action = string.Empty;
      var libsrc = string.Empty;
      var libtar = string.Empty;
      var item = string.Empty;

      var hdrlibsrc = string.Empty;
      var hdrlibtar = string.Empty;
      var deployid = -1;

      for (int i = 0; i < nrows; ++i) {
        for (int j = 0; j < ncols; ++j) {
          var col = 65 + j;
          var key = Convert.ToChar(col).ToString() + i.ToString();

          switch (j) {
            case 0: type = data.get(key); break;
            case 1: userid = data.get(key); break;
            case 2: action = data.get(key); break;
            case 3: libsrc = data.get(key); break;
            case 4: libtar = data.get(key); break;
            case 5: item = data.get(key); break;
          }
        }
        err.require(hdrlibsrc.Length > 0 && hdrlibsrc != libsrc, "multiple_src_libraries");
        err.require(hdrlibtar.Length > 0 && hdrlibtar != libtar, "multiple_tar_libraries");
        hdrlibsrc = libsrc;
        hdrlibtar = libtar;
      }

      // save header 
      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        deployid = dal.deploy_create_hdr(pack, hdrlibtar, hdrlibsrc);
      }

      for (int i = 0; i < nrows; ++i) {
        for (int j = 0; j < ncols; ++j) {
          var col = 65 + j;
          var key = Convert.ToChar(col).ToString() + i.ToString();

          switch (j) {
            case 0: type = data.get(key); break;
            case 1: userid = data.get(key); break;
            case 2: action = data.get(key); break;
            case 3: libsrc = data.get(key); break;
            case 4: libtar = data.get(key); break;
            case 5: item = data.get(key); break;
          }
        }

        // save detail
        using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
          dal.deploy_create_dtl(deployid, item);
        }
      }
    }

    public void exec_batch(link lnk) {
      var values = lnk.values;
      var basics = lnk.basics;

      var name = string.Empty;
      err.require(values.get("name", ref name) == 0, cme.INC_DATA_ID);
      var type = string.Empty;
      err.require(values.get("type", ref type) == 0, cme.INC_DATA_TYPE);
      var data = new StringBuilder();
      err.require(values.get("text", data) == 0, cme.INC_DATA_CONTENT);

      var tran = string.Empty; basics.get(defs.ZTRNCOD, ref tran);
      var user = string.Empty; basics.get(defs.ZUSERID, ref user);
      var mach = string.Empty; basics.get(defs.ZMACNAM, ref mach);

      // trick for avoiding html injection problem
      int n = data.Length;
      int a = -1;
      for (int i = 0; i < n; ++i) {
        if (data[i] == '<') {
          for (int j = i; j < n; ++j) {
            if (data[j] == '@') { a = i; goto done; }
          }
        }
      }
    done:
      if (a != -1) data = data.Replace("<@:", "<", a, n - a);

      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        dal.exec_batch(user, name, type, data.ToString());
      }
    }

    /*public void get_expanded_doc(string doc,
                                  string type,
                                  ref string data,
                                  config cfg,
                                  string pbasics) {
       var t = new StringBuilder();
       atl.atlservice(t, pbasics, funs.GET_FILE,
                      defs.ZTYPTRN, "trans",
                      defs.ZTYPRED, "force",
                      defs.ZFILE01, doc,
                      defs.PDOCTYP, type);

       var work = new StringBuilder();
       var res = new mroJSON();
       var server = cfg.locaddr;

       char[] buffer = new char[8192 + (8192 / 2)];
       byte[] bbytes = new byte[8192 + (8192 / 2)];

       mrosocket.atlantic(buffer, bbytes, work, server, cfg.gatprt, t, res);
       if (res.has(defs.ZSERROR)) err.require(res.get(defs.ZSERROR));

       // now we have the trans code
       data = string.Empty;
       res.get(defs.ZFILERS, ref data);
       err.require(data.Length == 0, cme.DOC_NOT_LOADED);
    }*/

    public void check_document(link lnk) {
      var lib = string.Empty;
      err.require(lnk.values.get("clib", ref lib) == 0, cme.INC_DATA_LIBRARY);
      var id = string.Empty;
      err.require(lnk.values.get("cname", ref id) == 0, cme.INC_DATA_DOCUMENT);
      var type = string.Empty;
      err.require(lnk.values.get("ctype", ref type) == 0, cme.INC_DATA_TYPE);
      document doc = null;
      err.require(string.IsNullOrEmpty(id), cme.INC_DATA_DOCUMENT);
      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        doc = dal.get_document(lib, id, type);
      }

      var data = doc.data;
      if (data.IndexOf("<script") == -1) lnk.result.set(defs.ZSWARNG, "script_tag_is_missing");
      if (data.IndexOf("module=") == -1) lnk.result.set(defs.ZSWARNG, "module_var_tag_is_missing");
      if (data.IndexOf("codebehind=") == -1) lnk.result.set(defs.ZSWARNG, "codebehind_var_tag_is_missing");
      if (data.IndexOf("lprms=") == -1) lnk.result.set(defs.ZSWARNG, "lprms_var_tag_is_missing");
    }
    /*public void check_document_expanded(link lnk) {
       var doc = string.Empty;
       var typ = string.Empty;
       err.require(lnk.values.get("cname", ref doc) == 0, cme.INC_DATA_DOCUMENT);
       err.require(lnk.values.get("ctype", ref typ) == 0, cme.INC_DATA_TYPE);

       var data = string.Empty;
       get_expanded_doc(doc, typ, ref data, link.cfg, lnk.basics.get_mro());

       var error = string.Empty;
       mro.html.CheckHtml(data, out error);
       err.require(error.Length > 0, cme.BAD_SYNTAX, error);
    }
    public void expand_document(link lnk) {
       var doc = string.Empty;
       var typ = string.Empty;
       err.require(lnk.values.get("cname", ref doc) == 0, cme.INC_DATA_DOCUMENT);
       err.require(lnk.values.get("ctype", ref typ) == 0, cme.INC_DATA_TYPE);

       var data = string.Empty;
       get_expanded_doc(doc, typ, ref data, link.cfg, lnk.basics.get_mro());

       lnk.result.set(defs.PFLDTXT, data);
    }*/
    public void desencrypt_password(link lnk) {
      lnk.clie.newval.set("$desencpassword$", _desencrypt_pass(lnk.values.get("cpassini")));
    }
    public string _desencrypt_pass(string oripass) {
      var len = oripass.Length;
      char[] dencpass = oripass.ToCharArray();
      for (var i = 0; i < len; ++i) {
        char lttr = oripass[i];
        if (lttr == '!') dencpass[i] = Convert.ToChar(65);
        else
           if (lttr == '*') dencpass[i] = Convert.ToChar(97);
        else dencpass[i] = Convert.ToChar(lttr + 1);
      }
      return new string(dencpass, 0, len);
    }
    public string _encrypt_pass(string oripass) {
      var len = oripass.Length;
      char[] encpass = oripass.ToCharArray();
      for (var i = 0; i < len; ++i) {
        char lttr = oripass[i];
        if (lttr == 65) encpass[i] = '!';
        else
           if (lttr == 97) encpass[i] = '*';
        else encpass[i] = Convert.ToChar(lttr - 1);
      }
      return new string(encpass, 0, len);
    }
    public void encrypt_password(link lnk) {
      lnk.clie.newval.set("$encpassword$", _encrypt_pass(lnk.values.get("cpassini")));
    }

    public void get_user(link lnk) {
      var userid = string.Empty;
      err.require(lnk.values.get(defs.USER, ref userid) == 0, cme.INC_DATA_USER);
      var obj = get_user(lnk, lnk.clie, userid);
      err.require(obj == null || obj.id.Length == 0, cme.REG_NOT_EXIST);

      obj.image = string.Concat(dhtml.http, lnk.data.proxysvr, ":",
               lnk.data.proxyprt, "/files/uphotos/", userid, ".jpg");

      //lnk.clie.values.set("cpassini", obj.password);
      //desencrypt_password(lnk);
      obj.password = _desencrypt_pass(obj.password); // lnk.clie.newval.get("$desencpassword$");

      obj.to_json(lnk.result);
    }
    public void change_user(link lnk) {
      var values = lnk.values;
      var basics = lnk.basics;

      var cmpy = basics.getint(defs.ZCOMPNY);
      var ousr = values.get("cuserini");
      var opas = values.get("coripas");
      var nusr = values.get("cnewusr");
      var rusr = values.get("crepusr");
      var npas = values.get("cnewpas");
      var rpas = values.get("creppas");

      err.require(ousr.Length == 0, cme.INC_DATA_USER);
      err.require(opas.Length == 0, cme.INC_DATA_ORIPASS);
      err.require(nusr.Length == 0, cme.INC_DATA_NEWUSER);
      err.require(rusr.Length == 0, cme.INC_DATA_REPUSER);
      err.require(npas.Length == 0, cme.INC_DATA_NEWPASS);
      err.require(rpas.Length == 0, cme.INC_DATA_REPPASS);

      err.require(string.CompareOrdinal(nusr, ousr) == 0, cme.PASS_NEW_ORI_SAME);
      err.require(string.CompareOrdinal(nusr, rusr) != 0, cme.PASS_NEW_REP_DIFF);
      err.require(string.CompareOrdinal(npas, opas) == 0, cme.PASS_NEW_ORI_SAME);
      err.require(string.CompareOrdinal(npas, rpas) != 0, cme.PASS_NEW_REP_DIFF);

      var ouser = new user(ousr);
      var nuser = new user(nusr);

      // we encrypt the password to be changed
      //values.set("cpassini", opas);
      //encrypt_password(lnk);
      //var original_pass = lnk.clie.newval.get("$encpassword$");
      var op = new password(_encrypt_pass(opas));

      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        // the old user must exist and new not
        var o = dal.get_user(ousr);
        if (o == null) err.require(cme.USER_NOT_EXIST);
        var n = dal.get_user(nusr);
        if (n == null) err.require(cme.USER_ALREADY_EXIST);

        // the password to be changed must exist
        var ep = dal.user_check_pass(ousr, opas);
        err.require(!ep, cme.PASS_NOT_EXIST);
      }
      validate_pass(lnk);

      // we encrypt the new pass password
      //values.set("cpassini", npas);
      //encrypt_password(lnk);
      //var new_pass = lnk.clie.newval.get("$encpassword$");
      var np = new password(_encrypt_pass(npas));

      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        dal.delete_user(lnk.clie, ouser.id);
        dal.insert_user(lnk.clie, nuser);
        dal.set_password(lnk.clie, nuser, op, np);
        dal.rights_process(lnk.clie, cmpy, nuser);
      }
    }
    public void create_modify_user(link lnk) {
      var values = lnk.values;

      var userid = string.Empty;
      err.require(values.get(defs.USER, ref userid) == 0, cme.INC_DATA_USER);

      var obj = get_user(lnk, lnk.clie, userid);

      bool toinsert = obj == null;
      if (toinsert) obj = new user(userid);

      obj.description = values.get("description");
      obj.comments = values.get("comments");
      obj.date_start = values.getdate("date_start");
      obj.date_end = values.getdate("date_end");
      obj.password = values.get("password");
      obj.type = values.get("type");
      obj.time_start = values.getdate("time_start");
      obj.time_end = values.getdate("time_end");
      obj.groupid = values.get("group");
      obj.email = values.get("email");
      obj.phone = values.get("phone");
      obj.winuser = values.get("winuser");

      //values.set("cpassini", obj.password);
      //encrypt_password(lnk);
      obj.password = _encrypt_pass(obj.password); //lnk.clie.newval.get("$encpassword$");

      if (toinsert) insert_user(lnk, lnk.clie, obj);
      else update_user(lnk, lnk.clie, obj);
    }

    public void validate_pass(link lnk) {
      var pass = string.Empty;
      int lenpass = lnk.values.get("cnewpas", ref pass);

      err.require(lenpass < 6, cme.PASS_LEAST_6_LONG);
      err.require(lenpass > 16, cme.PASS_NOMORE_16_LONG);

      var wrongcars = false;
      var havecars = false;
      var havenums = false;

      for (int i = 0; i < lenpass; ++i) {
        if (char.IsLetter(pass[i])) { havecars = true; continue; }
        if (char.IsNumber(pass[i])) { havenums = true; continue; }
        wrongcars = true;
      }

      err.require(wrongcars, cme.PASS_ONLY_CARS_NUMS);
      err.require(!havecars, cme.PASS_LEAST_CHAR_AZ);
      err.require(!havenums, cme.PASS_LEAST_NUM_09);
    }
    public void change_pass(link lnk) {
      var values = lnk.values;

      var enty = values.get("cuserini");
      var opas = values.get("coripas");
      var npas = values.get("cnewpas");
      var rpas = values.get("creppas");

      err.require(enty.Length == 0, cme.INC_DATA_USER);
      err.require(opas.Length == 0, cme.INC_DATA_ORIPASS);
      err.require(npas.Length == 0, cme.INC_DATA_NEWPASS);
      err.require(rpas.Length == 0, cme.INC_DATA_REPPASS);

      err.require(string.CompareOrdinal(npas, opas) == 0, cme.PASS_NEW_ORI_SAME);
      err.require(string.CompareOrdinal(npas, rpas) != 0, cme.PASS_NEW_REP_DIFF);

      // find user from any type of id: email, phone or direct id
      user u = null;
      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        var id = dal.user_get_credentials(enty);
        u = dal.get_user(id);
      }
      err.require(u == null || u.id.Length == 0, cme.USER_NOT_EXIST);

      // we encrypt the password to be changed
      //values.set("cpassini", opas);
      //encrypt_password(lnk);
      //var original_pass = lnk.clie.newval.get("$encpassword$");
      var op = new password(_encrypt_pass(opas));

      // the password to be changed must exist
      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        // check is user registration
        var ep = dal.user_check_pass(u.id, op.value);
        err.require(!ep, cme.PASS_NOT_EXIST);
      }
      validate_pass(lnk);

      // we encrypt the new pass password
      //values.set("cpassini", npas);
      //encrypt_password(lnk);
      //var new_pass = lnk.clie.newval.get("$encpassword$");
      var np = new password(_encrypt_pass(npas));

      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        dal.set_password(lnk.clie, u, op, np);
      }
    }
    public void recover_pass(link lnk) {
      var values = lnk.values;

      var userid = string.Empty;
      err.require(values.get(defs.USER, ref userid) == 0, cme.INC_DATA_USER);

      // find user from any type of id: email, phone or direct id
      user u = null;
      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        var id = dal.user_get_credentials(userid);
        u = dal.get_user(id);
      }
      err.require(u == null || u.id.Length == 0, cme.USER_NOT_EXIST);

      // find user's email
      var email = u.email.Trim();
      err.require(email.Length == 0, cme.MAIL_NOT_EXIST);

      // find passwords
      var body = "Credentials:<br/><br/>";
      body += "Email: " + email + "<br/>";
      body += "User ID: " + u.id + "<br/>";
      //values.set("cpassini", u.password);
      //desencrypt_password(lnk);
      body += "Password: " + _desencrypt_pass(u.password) /*lnk.clie.newval.get("$desencpassword$")*/ + "<br/>";

      var fr = "Security service";
      var to = email;
      var cc = "";
      var sb = "Recover password";
      var bo = body;
      var at = "";

      mail.compose_email(config.dbcode, fr, to, cc, sb, bo, at, 0, false);
    }

    public void get_passwords(link lnk) {
      var values = lnk.values;

      var userid = string.Empty;
      err.require(values.get(defs.USER, ref userid) == 0, cme.INC_DATA_USER);

      List<password> rights = null;
      using (var dal = control_DAL.instance(get_conns(lnk.clie, lnk.get_appdb()))) {
        rights = dal.get_raw_rights(new user(userid));
      }
      err.require(rights == null || rights.Count == 0, cme.USER_HAVE_NO_PASSWORD);

      int listid = values.getint(defs.ZLISTAF);
      var lr = new ListResponse(listid, 1, lnk.clie);
      var row = 0;

      foreach (var o in rights) {
        //values.set("cpassini", o.value);
        //desencrypt_password(lnk);
        lr.set_data(row, 'A', _desencrypt_pass(o.value)/*lnk.clie.newval.get("$desencpassword$")*/, '*', "1");
        ++row;
      }

      lr.set_rows(row);
      lr.pass_to_obj(lnk.result);
    }

    private static Dictionary<string, Dictionary<string, List<string>>> chat =
       new Dictionary<string, Dictionary<string, List<string>>>();
    public void check_chat(link lnk) {
      var me = lnk.basics.get(defs.ZUSERID);
      Dictionary<string, List<string>> text = null;
      if (chat.TryGetValue(me, out text)) {
        if (text.Count > 0) {
          lnk.result.set("zchthasz", "1");
          lnk.result.set("zchtfrmz", "text.Keys");
        }
      }
    }
    public void read_chat(link lnk) {
      var me = lnk.basics.get(defs.ZUSERID);
      var other = lnk.values.get("cusrini");

      other = other.TrimEnd();
      other = other.ToLower();

      err.require(me.Length == 0 || me.Length == 0, cme.INC_DATA_USER);

      var result = lnk.result;
      Dictionary<string, List<string>> text = null;
      if (chat.TryGetValue(me, out text)) {
        foreach (KeyValuePair<string, List<string>> par in text) {
          string persona = par.Key;
          foreach (var p in par.Value) {
            string t = mem.join3(persona, " say: ", p);
            result.set("uchatbox0", t);
            result.set("uchatbox0img", persona);
          }
        }
        chat.Remove(me);
      }

      /*    try
     {
       if(!::TryEnterCriticalSection(&cschat)) return;

       iterchat = chat.lower_bound(me);
       if(iterchat != endchat && !(chat.key_comp()(me, iterchat->first)))
       {
         map<CString, list<CString> >& chat2 = (*iterchat).second; // tomamos los mensajes para me

         int ini = 0;
         for(;;)
         {
           // desglozamos la lista para sacar cada persona
           CString persona;
           int fin = other.Find(';', ini);
           if(fin == -1) break; // ya no hay mas en la lista

           persona = other.Mid(ini, fin - ini);
           ini = fin + 1;

           persona.TrimLeft();
           persona.TrimRight();
           persona.Remove(';');
           if(persona.GetLength() ==  0) break;
           if(persona             == me) continue; // no se puede leer a uno mismo;

           // buscamos en los mensajes para me los de la otra persona de la lista
           map<CString, list<CString> >::iterator iter2 = chat2.lower_bound(persona);
           if(iter2 != chat2.end() && !(chat2.key_comp()(persona, iter2->first)))
           {
             if((*iter2).second.empty() == false)
             {
               list<CString>::iterator liter = (*iter2).second.begin();

               me.Format(_T("%s %s: %s"), persona, _T("say"), *liter);//get_desc(_T("say")), *liter);
               _params.set(_T("uchatbox0"), me, 9);
               _params.set(_T("uchatbox0img"), persona, 12);
               (*iter2).second.erase(liter);
               break;
             }
           }
         }
       }
       ::LeaveCriticalSection(&cschat);
     }
     catch(CException *e)	{ 	::LeaveCriticalSection(&cschat); throw; }
     catch(mroerr&)			{	::LeaveCriticalSection(&cschat); throw;	}
     catch(...)				{	::LeaveCriticalSection(&cschat); throw;	}*/
    }
    public void write_chat(link lnk) {
      var message = lnk.values.get("ctxtini");
      if (message.Length == 0) return;

      var me = lnk.basics.get(defs.ZUSERID);
      if (me.Length == 0) me = lnk.values.get(defs.ZUSERID);
      me = me.Trim();

      var other = lnk.values.get("cusrini");
      other = other.Trim();
      other = other.ToLower();

      err.require(me.Length == 0 || other.Length == 0, cme.INC_DATA_USER);

      List<string> data = null;
      Dictionary<string, List<string>> text = null;
      if (chat.TryGetValue(other, out text)) {
        if (text.TryGetValue(other, out data)) {
          data.Add(message);
        }
        else {
          data = new List<string>();
          data.Add(message);
          text.Add(me, data);
        }
      }
      else {
        data = new List<string>();
        data.Add(message);
        text = new Dictionary<string, List<string>>();
        text.Add(me, data);
        chat.Add(other, text);
      }

      lnk.result.set("utext", "");
      string t = mem.join3(me, " say: ", message);
      lnk.result.set("uchatbox0", t);
      lnk.result.set("uchatbox0img", me);

      /*	CString message;
     _params.get(_T("ctxtini"), message, 7);
     if(message.IsEmpty()) return;

     CString me;
     _basics.get(ZUSERID, me, ZUSERIDLEN);
     if(me.IsEmpty()) _params.get(ZUSERID, me, ZUSERIDLEN);
     me.TrimRight();
     me.MakeLower();

     CString other;
     _params.get(_T("cusrini"), other, 7);
     other.TrimRight();
     other.MakeLower();

     require(me.IsEmpty() || other.IsEmpty(), _T("inc_dat_user"));

     try
     {
       ::EnterCriticalSection(&cschat);

       int ini = 0;
       for(;;)
       {
         // desglozamos la lista para sacar cada persona
         CString persona;
         int fin = other.Find(_T(';'), ini);
         if(fin == -1) break; // ya no hay mas en la lista

         persona = other.Mid(ini, fin - ini);
         ini = fin + 1;

         persona.TrimLeft();
         persona.TrimRight();
         persona.Remove(_T(';'));
         if(persona.GetLength() ==  0) break;
         if(persona             == me) continue; // no se puede mandar a uno mismo;

         chat[persona][me].push_back(message);
       }
       endchat = chat.end();

       ::LeaveCriticalSection(&cschat);
     }
     catch(CException *e)	{ 	::LeaveCriticalSection(&cschat); throw; }
     catch(mroerr&)			{	::LeaveCriticalSection(&cschat); throw;	}
     catch(...)				{	::LeaveCriticalSection(&cschat); throw;	}

     _params.set(_T("utext"), _T(""), 5);

     CString msg;
     msg.Format(_T("%s %s: %s"), me, _T("say"), message);
     _params.set(_T("uchatbox0"), msg, 9);
     _params.set(_T("uchatbox0img"), me, 12);*/
    }
    public void clean_chat(link lnk) {
    }

    #endregion
    public void form_shortcut(link lnk) {
      var values = lnk.values;
      var basics = lnk.basics;
      var result = lnk.result;

      var tcod = values.get("name");
      var desc = values.get("desc");
      var type = values.getint("type", 0);
      var enty = values.get("enty");

      var parms = "";
      if (type == 0) parms += "c=" + basics.get(defs.ZCOMPNY) + "&e=" + enty;
      else parms += "sc=" + tcod;

      var page = "/web/mro.html?" + parms;

      result.set("link", desc);
      result.set("href", ".." + page);
      result.set("fulllink", string.Concat("http://", lnk.data.proxysvr, ":",
                         lnk.data.proxyprt, page));
    }
    public void get_user_photos(link lnk) {
      var values = lnk.values;
      var basics = lnk.basics;
      var result = lnk.result;

      var userid = values.get(defs.USER);
      err.require(userid.Length == 0, cme.INC_DATA_USER);

      var tempim = string.Concat("http://", lnk.data.proxysvr, ":",
                      lnk.data.proxyprt, "/temp/", userid, ".jpg");
      var usrimg = string.Concat("http://", lnk.data.proxysvr, ":",
                      lnk.data.proxyprt, "/files/uphotos/", userid, ".jpg");

      result.set("temp", tempim);
      result.set("image", usrimg);
    }
  }
}
