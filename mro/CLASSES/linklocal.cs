#if NETCOREAPP
using Microsoft.AspNetCore.Http;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Data.SqlClient;
using mro.BO;
using System.Runtime.InteropServices;

namespace mro {
  public partial class link {
    /**
     * this function tries to be fast by get riding of the overhead of preparing 
     * the query, it should be used only in places when performance is demanding 
     * and the info from the query is known from the client and included on 
     * the query string or be implicit on the query and it does not return data
     */
    private void query_direct() {
      var qrydtl = extract_main_query();
      appbl.execute_query_no_resp(clie, qrydtl, get_appdb());
    }
    /**
     * this function does not return data eliminating unnecessary overhead
     * candidates for this function are UPDATES, INSERTS, DELETES and
     * store procedures that are certain that does not return info
     **/
    private void run_query() {
      var qrydtl = prepare_query();
      appbl.execute_query_no_resp(clie, qrydtl, get_appdb());
    }
    /**
     * this function does return data and it is the most common function and it
     * is used a lot as a default execution, but it is most suitable for SELECT 
     * data and specially for lists, on doubts about what function call use this
     **/
    private void execute_query() {
      var qrydtl = prepare_query();
      var target = get_appdb();
      var link = poolj.get(0);
      if (values.get(defs.PLINKVS, link) == 0) {
        exec_list_resp(link, qrydtl, target, micr.lstres, true);
      }
      else {
        var qr = appbl.execute_query_one_row(clie, qrydtl, null, target);
        if (qr.Length > 0) {
          var nlinks = link.getint(defs.PNLINKS);
          if (nlinks >= utils.cols.Length)
            err.require(cme.E2MVARS, nlinks); // prevent finger bugs
          for (var i = 0; i < nlinks; ++i) {
            if (i >= qr.Length) continue;
            var k = link.get(utils.cols[i]); // get variable name
            if (k.Length > 0) {
              var v = qr[i]; // get db value
              response.send(resp, k, v);
              clie.newval.set(k, v);
            }
          }
        }
      }
    }
    /**
     * function no callable for the clients, its the base for the queries
     * that generate lists of data as result, being direct whole respond
     * sent back to the client or indirect/dataset keeping result in cache
     */
    private ListResponse exec_list_resp(mroJSON link,
                                        query qrydtl,
                                        string target,
                                        ListResponse lstresr,
                                        bool direct) {
      var listid = values.getint(defs.ZLISTAF);
      var retcols = values.getbool(defs.RETCOLS);
      var retcoltype = values.getbool(defs.RETCOLTYPE);
      var lr = appbl.execute_query_resp(clie,
                                         lstresr,
                                         direct,
                                         listid,
                                         qrydtl.sql, target, null,
                                         -1, retcols, retcoltype);
      var rowsreturn = lr.get_nrows();
      if (values.has(defs.RETINFO)) {
        values.get(defs.RETINFO, link);
        var rettot = mem.join3(link.get("text"), ": ", rowsreturn.tostr());
        response.send(resp, link.get("link"), rettot);
      }
      return lr;
    }
    /**
     * this function return one particular page from the list cache, normally
     * the client call execute_dataset to generate the whole list and then 
     * call this function as pgedown, pageup, etc.. to move the list fata
     */
    private void exec_dataset_page() {
      var listid = values.getint(defs.ZLISTAF);
      var dataset = link.dataset[_getsessunit()][listid];
      var res = dataset.result;
      var ncols = dataset.get_ncols();

      var rowsperpage = values.getint("rowsperpage");
      var page = values.getint("targetpage");
      var top = (page - 1) * rowsperpage;
      var btm = ((page) * rowsperpage);

      var lid = pools.getclean(0);
      lid.Append('l');
      lid.Digits(listid);

      var cell = pools.get(1);
      var row = pools.get(2);
      for (int i = top; i < btm; ++i) {
        row.set(lid);
        row.Digits(i);
        for (int j = 0; j < ncols; ++j) {
          cell.set(row);
          cell.Append((char)(65 + j));
          response.send(resp, cell, res.get(cell)); // datum
        }
        cell.set(row);
        cell.Append('*');
        response.send(resp, cell, res.get(cell));  // image
      }
    }
    /**
     * this function is like the execute_query but it leaves the result
     * onto memory/cache an return one page only instead of the whole
     * result at once, it;s recommened for long list result data
     */
    private void execute_dataset() {
      var qrydtl = prepare_query();
      var target = get_appdb();
      var linkvs = poolj.get(0);
      if (values.get(defs.PLINKVS, linkvs) == 0) {
        var listid = values.getint(defs.ZLISTAF);

        var ses = _getsessunit();
        var dataset = link.dataset;
        if (!dataset.ContainsKey(ses))
          dataset.Add(ses, new Dictionary<int, ListResponse>());
        var lstres = dataset[ses];
        if (!lstres.ContainsKey(listid))
          lstres.Add(listid, new ListResponse());

        var lr = exec_list_resp(linkvs, qrydtl, target, lstres[listid], false);

        var res = lr.result;
        var ncols = lr.get_ncols();
        var cell = pools.get(0);
        var row = pools.get(1);
        for (int i = 0; i < ncols; ++i) { // columns
          row.set('z');
          row.Digits(listid);
          row.Append("cl");
          row.Digits(i);

          cell.set(row);
          cell.Append('z');
          response.send(resp, cell, res.get(cell));
          cell.set(row);
          cell.Append('l');
          response.send(resp, cell, res.get(cell));
        }

        // data
        var tarpage = values.getint("targetpage", -1);
        values.set("targetpage", tarpage == -1 ? 1 : tarpage);
        exec_dataset_page();

        // total rows
        cell.set("zl");
        cell.Append(listid);
        cell.Append("rows");
        response.send(resp, cell, res.get(cell));
        // list affected
        cell.set("zla");
        cell.Digits(listid);
        response.send(resp, cell, res.get(cell));
        // number of cols
        cell.set('z');
        cell.Digits(listid);
        cell.Append("nclsz");
        response.send(resp, cell, res.get(cell));
        // totals descriptions
        cell.set("ztotslst");
        cell.Digits(listid);
        response.send(resp, cell, res.get(cell));

        result.on(mem.join2("bydataset", listid));
      }
    }
    private void query_into_result() {
      var qrydtl = prepare_query();
      appbl.query_into_result(clie, qrydtl, result, get_appdb());
    }
    /**
     * Report HTML generation
     */
    private void generate_report() {
      var lib = values.get(defs.LIBRARY);

      // use default templates or not
      var defhdr = values.getbool(defs.USEDEFHDR, true);
      var defftr = values.getbool(defs.USEDEFFTR, false);

      // find out the type for the detail generation
      var dtlctr = values.getbool(defs.DTLBYCRT, true);
      var dtlrpl = values.getbool(defs.DTLBYRPL, false);

      // report header template (company, user, machine, other stuff)
      query hdrformat = null;
      var header = values.get(defs.PHEADER);
      if (header.Length == 0 && defhdr) header = "repfmts/genbtrnghdr_net.txt";
      if (header.Length > 0) hdrformat = get_query_cache(lib, header);

      // report detail template (company, user, machine, other stuff)
      query dtlformat = null;
      var detail = values.get(defs.PDETAIL);
      if (detail.Length > 0) dtlformat = get_query_cache(lib, detail);

      // report footer template (company, user, machine, other stuff)
      query ftrformat = null;
      var footer = values.get(defs.PFOOTER);
      if (footer.Length == 0 && defftr) footer = "repfmts/genbtrngftr_net.txt";
      if (footer.Length > 0) ftrformat = get_query_cache(lib, footer);

      // query header 
      query qryhdr = null;
      var queryhdr = values.get(defs.QUERYHDR);
      if (queryhdr.Length > 0) qryhdr = get_query_cache(lib, queryhdr);

      // query detail
      var qrydtl = extract_main_query();
      if (qrydtl == null) err.require(cme.QUERY_EMPTY, values.get(defs.PQRYCOD));

      // query footer 
      query qryftr = null;
      var queryftr = values.get(defs.QUERYFTR);
      if (queryftr.Length > 0) qryftr = get_query_cache(lib, queryftr);

      // query footer 2
      query qryft2 = null;
      var queryft2 = values.get(defs.QUERYFT2);
      if (queryft2.Length > 0) qryft2 = get_query_cache(lib, queryft2);

      // some helper data description (the main description comes on the header)
      var desc = values.get(defs.REPDESC);
      var range = values.get(defs.RNGDESC);

      // specify the a custom column width
      var fwidths = poolj.get(0);
      values.get(defs.FIELDSW, fwidths);
      // sepecify the a custom column totals
      var fsums = poolj.get(1);
      values.get(defs.SUMFLDS, fsums);

      // replace the querys variables with the real values
      var nvars = values.getint(defs.PNREPVS);
      if (nvars > 0) {
        var var2chg = string.Empty;
        var value = string.Empty;
        if (nvars >= utils.repvars.Length) // prevent finger bugs 
          err.require(cme.E2MVARS, nvars);
        for (var i = 0; i < nvars; ++i) {
          var2chg = utils.repvars[i];
          values.get(var2chg, ref value);
          if (qryhdr != null)
            qryhdr.sql = utils.ReplaceEx(clie.buffer, qryhdr.sql, var2chg, value);
          if (qrydtl != null)
            qrydtl.sql = utils.ReplaceEx(clie.buffer, qrydtl.sql, var2chg, value);
          if (qryftr != null)
            qryftr.sql = utils.ReplaceEx(clie.buffer, qryftr.sql, var2chg, value);
          if (qryft2 != null)
            qryft2.sql = utils.ReplaceEx(clie.buffer, qryft2.sql, var2chg, value);
          range = utils.ReplaceEx(clie.buffer, range, var2chg, value);
        }
      }
      // data for the creation for the unique report file to be downloaded
      var trans = basics.get(defs.ZTRNCOD);
      var addr = basics.get(defs.ZMACNAM);
      var user = basics.get(defs.ZUSERID);
      var download = appbl.generate_report(clie,
                                             qrydtl,
                                             qryhdr,
                                             qryftr,
                                             qryft2,
                                             clie.mhelp,
                                             client.node,
                                             config.home,
                                             defs.TMPFOLDER,
                                             trans,
                                             addr,
                                             desc,
                                             range,
                                             dtlformat != null ? dtlformat.sql : string.Empty,
                                             hdrformat != null ? hdrformat.sql : string.Empty,
                                             ftrformat != null ? ftrformat.sql : string.Empty,
                                             dtlctr,
                                             dtlrpl,
                                             fwidths,
                                             fsums,
                                             user,
                                             get_appdb());
      download.pass_into(result);
    }
    private void compose_html() {
      var address = basics.get(defs.ZMACNAM);
      var noreturn = values.getbool(defs.NORTREP);
      var folder = values.get(defs.ZFOLDER);
      if (folder.Length == 0) folder = defs.TMPFOLDER;
      var listid = values.getint(defs.ZLISTAF);
      var useset = values.getbool("usedataset");
      var dataset = useset ? link.dataset[_getsessunit()][listid] : null;
      var download = appbl.compose_html(clie,
                                        dataset,
                                        client.node,
                                        config.home,
                                        folder,
                                        address);
      if (!noreturn) download.pass_into(result);
    }
    private void compose_pdf() {
      var address = basics.get(defs.ZMACNAM);
      var noreturn = values.getbool(defs.NORTREP);
      var folder = values.get(defs.ZFOLDER);
      if (folder.Length == 0) folder = defs.TMPFOLDER;
      var listid = values.getint(defs.ZLISTAF);
      var useset = values.getbool("usedataset");
      var dataset = useset ? link.dataset[_getsessunit()][listid] : null;
      var download = appbl.compose_pdf(clie,
                                        dataset,
                                        client.node,
                                        config.home,
                                        folder,
                                        address);
      if (!noreturn) download.pass_into(result);

      //var o = download.file;
      //var n = download.file.Replace(".html",".pdf");

      //System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
      //psi.RedirectStandardOutput = true;
      //psi.UseShellExecute = false;
      //psi.CreateNoWindow = true;
      //psi.FileName = string.Concat(config.home, "wkhtmltopdf.exe");
      //psi.WorkingDirectory = config.home;
      //psi.Arguments = folder + "\\" + o + "  " + folder + "\\" + n;
      //System.Diagnostics.Process.Start(psi);

      //download.file = n;

      //if (!noreturn) download.pass_into(result);
    }
    /**
     * PDF Report Generator
     */
    private void generate_pdf() {
      var qrydtl = prepare_query();
      var template = values.get(defs.TEMPLATE);
      var address = basics.get(defs.ZMACNAM);
      var noreturn = values.getbool(defs.NORTREP);
      var folder = values.get(defs.ZFOLDER);
      if (folder.Length == 0) folder = defs.TMPFOLDER;
      var download = appbl.generate_pdf(clie, qrydtl,
                                          client.node,
                                          config.home,
                                          defs.PDFFOLDER,
                                          folder,
                                          template,
                                          address,
                                          get_appdb());
      if (!noreturn) download.pass_into(result);
    }
    private void compose_workbook() {
      var address = basics.get(defs.ZMACNAM);
      var noreturn = values.getbool(defs.NORTREP);
      var folder = values.get(defs.ZFOLDER);
      if (folder.Length == 0) folder = defs.TMPFOLDER;
      var download = appbl.generate_workbook(clie,
                                             null,
                                             clie.mhelp,
                                             client.node,
                                             config.home,
                                             defs.WKBFOLDER,
                                             folder,
                                             "Sheet1$",
                                             defs.WBKGENERIC,
                                             address,
                                             "");
      if (!noreturn) download.pass_into(result);
    }
    private void compose_email() {
      var typ = values.getint("type", -1);
      if (typ == -1) typ = 0;
      switch (typ) {
        case 0: compose_workbook(); break;
        case 1: compose_pdf(); break;
        default: compose_workbook(); break;
      }

      var source = string.Empty;
      err.require(result.get(defs.ZDWNFSV, ref source) == 0, cme.WKB_NOT_GENERATED);
      var folder = result.get(defs.ZDWNFPA);
      var file = result.get(defs.ZDWNFFL);
      var item = string.IsNullOrEmpty(folder) ? file : mem.join3(folder, "/", file);
      var fromurl = mem.join4(dhtml.http, source, "/", item);
      var fromdir = mem.join3(config.home_temp, "\\", file);

      // remove link to not be download it
      result.del(defs.ZDOWNLD);
      result.del(defs.ZNDOWNS);
      result.del(defs.ZDWNFSV);
      result.del(defs.ZDWNFPA);
      result.del(defs.ZDWNFFL);

      var fr = config.company + " " + defs.MALFROM;
      var to = values.get("to");
      var cc = values.get("cc");
      var sb = values.get("subject");
      var bo = values.get("body");
      var at = fromdir;// fromurl;

      mail.compose_email(config.dbcode, fr, to, cc, sb, bo, at, 0, false);
    }
    /**
     * Generic Excel Report Generation, whatever query its inserted on the sheet
     */
    private void generic_workbook() {
      var qrydtl = prepare_query();
      var address = basics.get(defs.ZMACNAM);
      var noreturn = values.getbool(defs.NORTREP);
      var folder = values.get(defs.ZFOLDER);
      if (folder.Length == 0) folder = defs.TMPFOLDER;
      var download = appbl.generate_workbook(clie,
                                             qrydtl,
                                             clie.mhelp,
                                             client.node,
                                             config.home,
                                             defs.WKBFOLDER,
                                             folder,
                                             "Sheet1$",
                                             defs.WBKGENERIC,
                                             address,
                                             get_appdb());
      if (!noreturn) download.pass_into(result);
    }
    /**
     * Template Excel Report Generation, whatever query its inserted on 
     * specific EXCEL template
     */
    private void generate_workbook() {
      var qrydtl = prepare_query();
      var template = values.get(defs.TEMPLATE);
      var address = basics.get(defs.ZMACNAM);
      var noreturn = values.getbool(defs.NORTREP);
      var folder = values.get(defs.ZFOLDER);
      if (folder.Length == 0) folder = defs.TMPFOLDER;
      var download = appbl.generate_workbook(clie,
                                             qrydtl,
                                             clie.mhelp,
                                             client.node,
                                             config.home,
                                             defs.WKBFOLDER,
                                             folder,
                                             "DataCollectionSystem$",
                                             template,
                                             address,
                                             get_appdb());
      if (!noreturn) download.pass_into(result);
    }
    private void generate_label() {
      var qrydtl = prepare_query();

      query dtlformat = null;
      var library = values.get(defs.LIBRARY);
      var detail = values.get(defs.PDETAIL);
      if (detail.Length > 0) dtlformat = get_query_cache(library, detail);

      var filename = values.get(defs.FILENAME);
      var address = basics.get(defs.ZMACNAM);
      var user = basics.get(defs.ZUSERID);
      var shl = new shell();
      var download = appbl.generate_label(clie,
                                             qrydtl,
                                             dtlformat != null ? dtlformat.sql : string.Empty,
                                             filename,
                                             client.node,
                                             config.home,
                                             defs.TMPFOLDER,
                                             address,
                                             user,
                                             ref shl,
                                             get_appdb());
      download.pass_into(result);
      shl.pass_into(result);
    }
    /**
     * Text FILE Generation, whatever query its inserted on the txt FILE
     */
    private void generate_text_file() {
      var qrydtl = prepare_query();
      var filename = values.get(defs.FILENAME);
      var address = basics.get(defs.ZMACNAM);
      var user = basics.get(defs.ZUSERID);
      var shl = new shell();
      var download = appbl.generate_text_file(clie,
                                                qrydtl,
                                                filename,
                                                client.node,
                                                config.home,
                                                defs.TMPFOLDER, address,
                                                user,
                                                ref shl,
                                                get_appdb());
      download.pass_into(result);
      shl.pass_into(result);
    }
    private void execute_webservice() {
      var qrydtl = prepare_query();

      var meth = values.get("method");
      meth = meth.ToUpper();
      if (meth.Length == 0) meth = method.GET;

      var postdata = string.Empty;
      if (meth == method.POST) {
        postdata = values.get("post");
      }

      var raddr = qrydtl.sql;
      var buffer = new char[8192 + (8192 / 2)];
      var a = mrosocket.webservice(buffer, meth, raddr, postdata);
      //a = utils.ReplaceEx(null, a, "\\\"", "\"");
      result.append(new mroJSON(a));
    }
    /*
     * function although used for trigger any web addresses for any purpose, 
     * one big use its for consume reports likw report services, the system 
     * collects the values and it runs the address with the parameters
     */
    private void execute_address() {
      var qrydtl = prepare_query();
      var download = new filedownload();

      var raddr = qrydtl.sql;
      var a = -1;
      a = raddr.IndexOf('\r'); if (a != -1) raddr = raddr.Remove(a, 1);
      a = raddr.IndexOf('\n'); if (a != -1) raddr = raddr.Remove(a, 1);
      a = raddr.IndexOf('\t'); if (a != -1) raddr = raddr.Remove(a, 1);

      /*int n = cfg.netsites.getint(defs.NSITES);
      var addresses = poolp.get(0);
      var site = string.Empty;
      for (int i = 0; i < n; ++i)
      {
          cfg.netsites.get(utils.sites[i], addresses);
          addresses.get(defs.NAME, ref site);
          site = string.Concat("$", site, "$");
          if (raddr.IndexOf(site) != -1)
          {
              var addr = string.Empty;
              addresses.get(defs.PADDRSS, ref addr);
              raddr = utils.ReplaceEx(clie.buffer, raddr, site, addr);
              break;
          }
      }*/

      download.direct = raddr;
      download.pass_into(result);
    }
    private void move_file() {
      var fi = values.get("file");
      var fr = values.get("from");
      var to = values.get("to");
      var proxypath = config.home.Replace("\\netsites\\core", "");
      var path = proxypath + "\\" + fr + "\\" + fi;
      var path2 = proxypath + "\\" + to + "\\" + fi;

      err.require(!File.Exists(path), cme.SRC_NOT_EXIST, path);
      if (File.Exists(path2)) File.Delete(path2);
      File.Move(path, path2);
      err.require(File.Exists(path), cme.SRC_STILL_EXIST, path);
      err.require(!File.Exists(path2), cme.DST_NOT_EXIST, path2);
    }
    private void upload_file_to_db() {
      var rf = values.get("ref");
      var path = values.get("uploadfilename");
      var name = Path.GetFileName(path);
      appbl.upload_document_to_db(clie, rf, name, path, get_appdb());
    }
    private void download_file_from_db() {
      var rf = values.get("ref");
      var name = values.get("file");
      err.require(name == string.Empty, cme.INC_DATA_FILE);

      var download = appbl.download_document_from_db(clie,
                                                      client.node,
                                                      rf, name,
                                                      config.home,
                                                      defs.TMPFOLDER,
                                                      get_appdb());
      download.pass_into(result);
    }
    private void excel_to_db() {
      var data = poolj.get(0);
      values.get(defs.ZEXEDAT, data);

      var total = data.getint(defs.ZEXETOT);
      var cols = data.getint(defs.ZEXECLS);
      var columns = values.getint(defs.COLSNEEDED);
      var checkempty = values.getbool(defs.CHKISEMPTY);
      var nvars = values.getint(defs.PNREPVS);

      if (cols != columns)
        err.require(cme.COLS_WRONG_NUMBER,
                    mem.join3(cols.tostr(), ':', columns.tostr()));
      if (checkempty && total <= 0) err.require(cme.FILE_EMPTY);

      var command = string.Empty;
      var h = string.Empty;
      var cell = pools.get(1);

      var cmdqry = extract_main_query();
      var qrydtl = new query();

      var end = false;
      var endvar = nvars - 1;
      var t = pools.getclean(2);
      for (int i = 1; i <= total; ++i) {
        char letter = (char)(65 + cols - 1);
        command = cmdqry.sql;
        for (var j = endvar; j >= 0; --j/*, --letter*/) {
          if (j < columns) { // create the cell backwards !!important!!
            cell.Length = 0;
            cell.Append(letter--);
            cell.Append(i);
            data.get(cell.ToString(), ref h);
          }
          else h = values.get(utils.repvars[j]);

          // check for end mark if any, on last col and last row
          if (j == endvar && string.CompareOrdinal(h, "*end*") == 0) {
            command = string.Empty; // we clean the template query line
            end = true;
            break;
          }
          command = utils.ReplaceEx(clie.buffer, command, utils.repvars[j], h);
        }

        t.Append(command);
        if ((i % 32) == 0) {
          qrydtl.sql = t.ToString();
          appbl.execute_query_no_resp(clie, qrydtl, get_appdb());
          t.Length = 0;
        }

        if (end) break; // no more if end mark was supply
      }

      if (t.Length > 0) { // any remain
        qrydtl.sql = t.ToString();
        appbl.execute_query_no_resp(clie, qrydtl, get_appdb());
      }
    }
    private void excel_2_db() {
      var data = poolj.get(2);
      values.get("cells", data);

      var columns = data.getint(defs.COLSNEEDED);
      var checkempty = data.getbool(defs.CHKISEMPTY);
      var nvars = data.getint("ncells");

      var exedata = poolj.get(2);
      values.get(defs.ZEXEDAT, exedata);
      var total = exedata.getint(defs.ZEXETOT);
      var cols = exedata.getint(defs.ZEXECLS);

      if (cols != columns)
        err.require(cme.COLS_WRONG_NUMBER,
                    mem.join3(cols.tostr(), ':', columns.tostr()));
      if (checkempty && total <= 0) err.require(cme.FILE_EMPTY);

      var command = string.Empty;
      var h = string.Empty;
      var cell = pools.get(1);

      var cmdqry = prepare_query();
      var qrydtl = new query();

      var t = pools.getclean(2);
      for (int i = 1; i <= total; ++i) {
        char letter = (char)(65 + cols - 1);
        command = cmdqry.sql;
        for (var j = nvars - 1; j >= 0; --j, --letter) {
          cell.Length = 0;
          cell.Append(letter);
          cell.Append(i);
          exedata.get(cell.ToString(), ref h);
          command = utils.ReplaceEx(clie.buffer, command, utils.cellvars[j], h);
        }
        t.Append(command);
        if ((i % 32) == 0) {
          qrydtl.sql = t.ToString();
          appbl.execute_query_no_resp(clie, qrydtl, get_appdb());
          t.Length = 0;
        }
      }
      if (t.Length > 0) { // any remain
        qrydtl.sql = t.ToString();
        appbl.execute_query_no_resp(clie, qrydtl, get_appdb());
      }
    }
    private void text_to_db() {
      var data = poolj.get(0);
      values.get(defs.ZTXTDAT, data);

      var total = data.getint(defs.ZTXTTOT);
      var columns = data.getint(defs.COLSNEEDED);
      var checkempty = data.getbool(defs.CHKISEMPTY);

      if (checkempty && total <= 0) err.require(cme.FILE_EMPTY);

      var linetxt = string.Empty;
      var lineid = "txtln0";
      data.get("txtln0", ref linetxt);

      var cols = linetxt.Split(',').Length - 1;
      if (cols != columns) err.require(cme.COLS_WRONG_NUMBER);

      var command = string.Empty;
      var helper = clie.mhelp.getsbl0();
      var cell = string.Empty;

      var sqlcmd = string.Empty;
      values.get(defs.SQLCMD, ref sqlcmd);
      var qrydtl = new query();

      for (int i = 0; i < total; ++i) {
        command = sqlcmd;

        lineid = mem.join2(defs.TXTLINE, i.ToString());
        data.get(lineid, ref linetxt);
        int n = linetxt.Length;
        int p = 0;
        int q = p;
        int fields = 1;
        int j = 0;
        for (; p < n; ++p) {
          if (linetxt[p] == ',') {
            int len0 = p - q;
            helper.Length = 0;
            helper.Append(linetxt, q, len0);
            q = p + 1;
            command = utils.ReplaceEx(clie.buffer, command,
                      utils.cols[j], helper.ToString().Trim());
            ++j;
            ++fields;
          }
        }
        int len = p - q;
        helper.Length = 0;
        helper.Append(linetxt, q, len);
        q = p + 1;
        command = utils.ReplaceEx(clie.buffer, command,
                  utils.cols[j], helper.ToString().Trim());
        ++j;

        if (fields != cols) err.require(cme.ERROR_IN_LINE, i);
      }
    }
    private void list_to_db() {
      var data = poolj.get(0);
      values.get(defs.ZLSTDAT, data);

      var total = data.getint(defs.ZLSTTOT);
      var cols = data.getint(defs.ZLSTCLS);
      var columns = values.getint(defs.COLSNEEDED);
      var checkempty = values.getbool(defs.CHKISEMPTY);
      var nvars = values.getint(defs.PNREPVS);

      if (cols != columns)
        err.require(cme.COLS_WRONG_NUMBER,
                    mem.join3(cols.tostr(), ':', columns.tostr()));
      if (checkempty && total <= 0) err.require(cme.FILE_EMPTY);

      var command = string.Empty;
      var h = string.Empty;
      var cell = pools.get(1);

      var cmdqry = extract_main_query();
      var qrydtl = new query();

      var t = pools.getclean(2);
      for (int i = 1; i <= total; ++i) {
        char letter = (char)(65 + cols - 1);
        command = cmdqry.sql;
        for (var j = nvars - 1; j >= 0; --j, --letter) {
          cell.Length = 0;
          cell.Append(letter);
          cell.Append(i);
          data.get(cell.ToString(), ref h);
          command = utils.ReplaceEx(clie.buffer, command, utils.repvars[j], h);
        }
        t.Append(command);
        if ((i % 32) == 0) {
          qrydtl.sql = t.ToString();
          appbl.execute_query_no_resp(clie, qrydtl, get_appdb());
          t.Length = 0;
        }
      }
      if (t.Length > 0) { // any remain
        qrydtl.sql = t.ToString();
        appbl.execute_query_no_resp(clie, qrydtl, get_appdb());
      }
    }
    // pending to be deleted because it moved to proxy
    private void download_file() {
      var folder = values.get(defs.ZFOLDER);
      var file = values.get(defs.ZFILE);
      var type = values.get(defs.ZTYPE);
      err.require(folder.Length == 0, cme.FOLDER_EMPTY);
      err.require(file.Length == 0, cme.FILE_EMPTY);
      var download = new filedownload(
          mem.join3(config.locaddr, ':', config.locport),
          folder, file, type);
      download.pass_into(result);
    }
    private string _getsessunit() {
      return mem.join6(clie.trans, clie.zsesins, clie.zsesmac,
                                    clie.zsescli, clie.zsesses, clie.unit);
    }
  }
}
