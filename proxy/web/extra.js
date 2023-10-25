MRO.sd = (fld, name, lng) => {
  window.open('/' + fld + '/' + MRO.ses.get(DFS.ZLANGUA) + '/' + name + '.htm');
}
MRO.show_usrinf = () => {
  MRO.ldm(typeof PRF, 'prf');
  PRF.show_usrinf();
}
MRO.hide_usrinf = () => {
  MRO.ldm(typeof PRF, 'prf');
  PRF.hide_usrinf();
}
MRO.show_dialog = (c, s, r, d, p, a) => {
  MRO.ldm(typeof DLG, 'dlg');
  DLG.show_dialog(c, s, r, d, p, a);
}
MRO.ses2var = (p) => {
  let m = MRO, s = m.ses;
  for (let i = 0, n = p.get('nvars'); i < n; ++i)
    m.set_val_of(p.get('var' + i), s.get(p.get('ses' + i)));
}
MRO.upload = p => { MRO.upload_file(p.get('folder'), p.get('file')); }
MRO.download = r => {
  let l = window.location;
  let p = new pairs();
  p.set(DFS.ZDWNFSV, l.hostname + ':' + l.port);
  p.set(DFS.ZDWNFPA, r.get('folder'));
  p.set(DFS.ZDWNFFL, r.get('file'));
  MRO.proc_download(p);
}
MRO.set_val_of = (vr, t) => {
  let v = $2(vr);
  if (v) v.value = t;
}
MRO.get_val_of = (el) => {
  let a = $2(el);
  return !a ? '' : a.value;
}
MRO.gen_values = retrow => new pairs('{'+MRO.get_values(retrow)+'}');
showtab = tb => {
  for (let i = 0; i < 16; ++i) {
    let t = $2('tab' + i);
    if (t) MRO.display(t.style, 0);
    let l = $2('li_tab' + i);
    if (l) l.setAttribute('class', '');
  }
  let tab = $2('tab' + tb);
  MRO.display(tab.style,1);
  $2('li_tab' + tb).setAttribute('class', 'current');

  // force recalculate a list if any //fbw
  let lst = tab.getElementsByTagName("TABLE");
  if (lst.length > 0) MRO.adjustdivtbl(lst[0].id.substr(4,1));
}
var process_paste = (evt) => {
  let m = MRO;
  if (m.isincapt) return;
  if (!m.islsel) return;

  let lid = m.curr_list;
  let fc = window.document.getElementById('formcap' + lid);
  if (!fc) return;
  let els = fc.childNodes;
  let caplen = els.length;
  let captype = linfo[lid].captype;
  m._before_alt_ins = -1;

  let clipText = MAC.brws === '2' ? window.clipboardData.getData('Text') :
    evt.clipboardData.getData('text/plain');
  let rows = clipText.split(String.fromCharCode(13));//separate rows
  if (rows.length === 0) return;
  m.show_cursor(true);
  for (let i = 0; i < rows.length; ++i) {
    let cols = rows[i].split(String.fromCharCode(9));//separate cols
    if (cols.length === 0) continue;
    let hasdata = false;
    let lastfield = 0;
    for (let c = 0; c < cols.length; ++c) {
      let val = cols[c];
      if (i > 0 && c === 0) {
        val = val.slice(1);
        if (val.length === 0) continue;
      }
      if (c === 0) {
        m.btn_list_bottom(lid);
        on_ldclick(lid, m.curr_row, 'A');//simulate click
      }
      // pass to capture edits/inputs
      for (let j = lastfield; j < caplen; ++j) {
        let ele = els[j];
        let cnm = ele.className;
        if (cnm === undefined) continue;
        if (cnm.charAt(0) !== 'm') continue;
        if (cnm.localeCompare('mroinput') === 0 ||
          cnm.localeCompare('mrodesc') === 0) {
          ele.value = val;
          hasdata = true;
          lastfield = j + 1;
          break;
        }
        if (cnm.localeCompare('mrocheck') === 0) {
          ele.checked = parseInt(val);
          hasdata = true;
          lastfield = j + 1;
          break;
        }
      }
    }
    if (hasdata) {
      let real_row = m.curr_real !== '*' ? m.curr_real : linfo[lid].nrows + 1;
      m.update_row(false, $2('formcap' + m.curr_list), m.curr_list, m.curr_row, real_row);
      m.send('oninsert' + lid, undefined, true);
      m.close_cap();
    }
  }
  m.show_cursor(false);
};
window.document.addEventListener('paste', process_paste, false);
MRO.set_focus = res => {
  let id = res.get(DFS.ZSETCUR);
  if (id.length === 0) return;
  if (id.localeCompare('zfrsfld') === 0) {
    if (MRO.ffocus) MRO.ffocus.focus();
    return;
  }
  let el = $2(id);
  if (!el) return;
  if (res.ison('zclnfld')) el.value = '';
  el.focus();
}
MRO.lackuse = () => {
  alert('session time out\n\n please log again\n\n' +
      'error: ' + DFS.ESESLKU + '\ndetail: session was reseted for inactivity\n');
  onclose();
}
MRO.handle_notify = r => {
  if (r.has(DFS.ZSERROR)) {
    if (MRO.check_error(r, DFS.ESESLKU)) MRO.lackuse();
  } else MRO.clklf = r.get('timeleft');
}
MRO.notify_use = m => {
  if (!m.clie.isactive()) return;
  m.clie.proxy('check_session', '""', true, m.handle_notify);
}
MRO.show_time = () => {
  let m = MRO, d = new Date();
  d.setMilliseconds(d.getMilliseconds() + m.clkof);
  let s = d.getSeconds();
  if (s % 15 === 0) m.notify_use(m);
  setTimeout(() => { m.show_time() }, 1000);
}
MRO.set_lastdata = () => {
  // we need to put code here to save data into localstorage
  let m = MRO, c = CUR, d = DFS, p = new pairs(), s = m.ses;
  if (!s.hasval(d.ZUSERID) || !m.ismain) return;
  /*p.set("cuserini", s.get(d.ZUSERID));
  p.set("clanguage", c.lng);
  p.set("clayout", c.css);
  m.clie.proxy('set_lastdata', p.buffer(), true);*/
}
// aync true cauze https://www.chromestatus.com/feature/4664843055398912
MRO.end_session = () => {
  let m = MRO, s = m.ses, d = DFS;
  if (m.clie.isactive()) m.clie.kernel('end_session', '', true);
  s.del(d.ZINSTAN);
  s.del(d.ZSESINS);
  s.del(d.ZSESMAC);
  s.del(d.ZSESCLI);
  s.del(d.ZSESSES);
  s.del(d.ZUSERID);
  s.del(d.ZDOMAIN);
  s.del(d.ZCOMPNY);
  m.clklf = '';
}
MRO._before_alt_ins = -1;
MRO.CAP = { INS: 0, UPD: 1, DEL: 2 };
MRO.close_cap = () => {
  let m = MRO, t = trans;
  m.isincapt = false;
  let frm = CNS.frms;
  let n = t.listmax + 1;
  for (let i = 1; i < n; ++i) {
    let fcap = $2(frm[i]);
    if (fcap) {
      let sty = fcap.style;
      sty.visibility = 'hidden';
      sty.position = 'absolute';
      sty.left = -10;
      sty.top = -10;
    }
  }
  m.block_outer_fields(true);
  m.close_sel();
}
MRO.upload_file = (fold, f) => {
  let r = '', fuf = $2('file_upload_form');
  let x = new XMLHttpRequest();
  x.open('POST', MRO.clie.url + '?function=upload&folder=' + fold + '&newname=' + f, false);
  x.onreadystatechange = function () {
    if (x.readyState === 4 && x.status === 200) r = x.responseText;
  };
  x.send(new FormData(fuf));
  return r;
}
MRO.downloadURL = url => {
  let d = window.document;
  let hiddenIFrameID = 'hiddenDownloader', ifr = d.getElementById(hiddenIFrameID);
  if (!ifr) {
    ifr = d.createElement('iframe');
    ifr.id = hiddenIFrameID;
    MRO.display(ifr.style,0);
    d.body.appendChild(ifr);
  }
  ifr.src = url;
};
dirdownload = (s, l) => {
  let a = document.createElement('a');
  a.setAttribute('href', s);
  a.setAttribute('download', l);
  MRO.display(a.style, 0);
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
}
download_files = res => {
  let addr = res.get(DFS.ZDWNDIR);
  if (!addr || addr === '') {
    addr = 'http://' + res.get(DFS.ZDWNFSV) + '/' +
    res.get(DFS.ZDWNFPA) + '/' + res.get(DFS.ZDWNFFL);
  }
  let t = res.get(DFS.ZDWNTYP);
  if (t === '') {
    if (addr.indexOf('.txt') !== -1) t = 'txt';
    else if (addr.indexOf('.xls') !== -1) t = 'excel';
    else if (addr.indexOf('.htm') !== -1) t = 'hmtl';
    else if (addr.indexOf('.pdf') !== -1) t = 'pdf';
  }
  switch (t) {
    case 'excel': MRO.downloadURL(addr); break;
    case 'html': window.open(addr); break;
    case 'txt': dirdownload(addr, res.get(DFS.ZDWNFFL)); break;
    case 'pdf': window.open(addr); break;
    default: window.open(addr);
  }
}
MRO.postmessage = a => { if (a === 32850) setTimeout(MRO.enter_system(), 0); }
MRO.proc_download = res => { download_files(res); }
function dispatch(fname) {
  try {
    let btnfound = false;
    let who = event ? event.srcElement : '';
    let newfocus = null;
    let d = window.document;
    if (who && who.id !== '') {
      let frms = CNS.frms;
      for (let i = 0, ilen = frms.length; i < ilen; ++i) {
        let x = d.getElementById(frms[i]);
        if (x) {
          let jlen = x.length;
          let els = x.elements;
          for (let j = jlen - 1; j > 0; --j) {
            let el = els[j];
            let id = el.id;
            let clnm = el.className;
            if (clnm === undefined) continue;
            if (clnm.charAt(0) !== 'm') continue;
            if (who.id === id) {
              btnfound = true;
              //if(MRO.isincapt&&!MRO.is_capture_control(id)){MRO.inuse=false;return;}
              continue;
            }
            if (btnfound && clnm.localeCompare('mroinput') === 0 && id !== '') {
              newfocus = el;
              break;
            }
          }
        }
        if (btnfound && newfocus) break;
      }
    }
    if (!newfocus && MRO.ffocus) newfocus = MRO.ffocus;
    if (newfocus) newfocus.focus();//when got function,find where put focus(matchcode)if any
    handle_ws(fname, null);
  }
  catch (e) {
    err.rescue(e)
  }
}
actr = (c, p) => {
  try {
    proc_context_menu(MRO.mro2json(p).get('type'), c, '""', '""', '""', '""');
  }
  catch (e) {
    MRO.handle_esc();
    err.rescue(e);
  }
}
proc_context_menu = (t, p0, p1, p2, p3, p4) => {
  let m = MRO;
  if (t === 'matchcode') m.do_goto('context_menu', '{"matchcode":"'.concat(
    p0, '","send":', p1, ',"receive":', p2, ',"params":', p3, ',"action":', p4, '}'));
  else if (t === 'msg') m.postmessage(p0);
  else if (t === 'fun') handle_ws(p0);
  else if (t === 'goto') m.ins_trans(p0, '', false);
  else if (t === 'gotopos') m.gui('gui_go_pos', '{"znewpos":' + p0 + '}');
  else m.ins_trans(p0, '', false);//?
}
MRO.gen_menucontext = (t, p, x, y) => {
  let ext = new pairs('{"type":' + t + '}');
  ext.set('x', x);
  ext.set('y', y);
  if (t === 1) MRO.clie.proxy('gui_get_favorites', p, true, MRO.do_menucontext, ext);
  else if (t === 0) MRO.clie.kernel('gui_get_history', p, true, MRO.do_menucontext, ext);
}
MRO.do_menucontext = his => {
  let m = MRO, d = DFS, hislabel = 'history', c = CNS, li = c.imgs;;
  try {
    m.check_resp(his);
    let type = his.getint('type');
    trans.hidepop = false;
    let s = '<div id="ctx_menu"class="menu"onmouseover="menuMouseover(event)"style="visibility:visible;display:block;">';

    if (type === 0 && trans.mtrnitems !== '') {
      let a = trans.mtrnitems;
      a = m.rpl_all(a, '<item value="', '<a class="menuItem" onclick="actr(\'');
      a = m.rpl_all(a, '" type="', '\', \'[type:');
      a = m.rpl_all(a, '" name="', ']\');">');
      a = m.rpl_all(a, '"></item>', '</a>');
      a = m.rpl_all(a, '<basics>', '');
      a = m.rpl_all(a, '</basics>', '');
      a = m.rpl_all(a, '<sort>', '');
      a = m.rpl_all(a, '</sort>', '');
      s += a;
    }

    let imt = {
      T: '<img src="'.concat(li, 'saptrans.png"/>'),
      R: '<img src="'.concat(li, 'report.png"/>'),
      W: '<img src="'.concat(li, 'excel.png"/>')
    };

    let n = his.getint(d.ZHISSIZ);
    let zhis = c.zhis;
    if (type === 0) s += '<a class="menuItemPop"onmouseover="mimo(event,"men_ctx");">'.concat(hislabel, '</a>');
    for (let i = 0; i < n; ++i) {
      let en = m.make(his.get(zhis[i]));
      let trn = en.get(d.ZHISTRN);
      let dsc = en.get(d.ZHISDSC);
      let typ = en.get(d.ZHISTYP);
      let img = imt[typ];
      let pr0 = type === 1 ? trn : '' + i;
      let pr1 = type === 1 ? '' : '[type:gotopos]';
      s += '<a class="menuItem"onclick="actr(\''.concat(pr0, '\',\'', pr1, '\');">', img, ' ', trn, ' ', dsc, '</a>');
    }
    trans.mpopitems = s.concat('</div>');

    let x = his.getint('x');
    let y = his.getint('y');

    if (type === 0) {
      trans.hidepop = true;
      MRO.context_menu(x,y);
    }
    else if (type === 1) {
      let c = $2('command');
      let a = c.getBoundingClientRect();
      MRO.context_menu(x,y);
    }
  }
  catch (e) {
    err.rescue(e)
  }
}
MRO.handle_keybd = k => {
  let d = window.document;
  let keyboardEvent = d.createEvent("KeyboardEvent");
  let initMethod = typeof keyboardEvent.initKeyboardEvent !== 'undefined' ? "initKeyboardEvent" : "initKeyEvent";
  keyboardEvent[initMethod]("keydown", true, true, window, false, false, false, false, 40, k);
  d.dispatchEvent(keyboardEvent);
}
//free functions functions activated outside like: menu,buttons,etc..
onclose = () => {
  if (MRO.ismain) execmd('out');
  close();
}
//guionnewapp = () => { window.open(window.document.URL); }
show_webpage = wp => { window.open(wp, '_blank'); }
//list functions
MRO.select_list_item = (wo, id, row) => {
  let cols2vars = wo.MRO.cols2vars;
  let m = MRO, d = window.document;
  let wod = wo.document;
  let cols = m.listncols(id);
  let cars = CARS;
  let gen = m.litem;
  for (let i = 0; i < cols; ++i) {
    let rowid = d.getElementById(gen(id, row, cars[i]));
    if (!rowid) break;
    let colid = 'col' + (i + 1);
    let varia = cols2vars.get(colid);
    if (varia !== '') {
      let dst = wod.getElementById(varia);
      if (dst) {
        dst.value = rowid.value;
        dst.focus();
      }
    }
    //if (cmd !== '') cmd = m.rpl_all(cmd, colid, rowid.value);
  }
  //if (cmd !== '') {
  //  wo.command.value = cmd;
  //  wo.bok_.onclick();
  //}
  close();
}
on_ldclick = (id, row, col) => {
  let m = MRO, d = document, wo = m.iswaitforsel();

  // chek onclick on transaction
  let fun = 'ondblclick' + id;
  if (trans.json.has(fun)) {
    m.send(fun); /*return false;*/
  } //not quite sure return or not

  // check for doble click selection and from new app?
  if (wo !== null) {
    m.select_list_item(wo, id, row);
    return false;
  }

  // normal editing capture
  m.close_match();
  if (m.isincapt) return false;
  // delete records cant be captured
  let rid = $2(m.litem(id, row, 'A'));
  if (rid && rid.value === '~') return false;
  rid = $2(m.litem(id, row, 'itm'));
  if (rid && rid.value !== '') m.capture_reg(rid, id, row);
}
on_lclick = (id, row) => {
  let m = MRO, t = trans;
  m.close_match();
  if (m.isincapt) return false;
  let it = $2(m.litem(id, row, 'itm'));
  if (!it || it.value === '') return false;

  let insel = m.isshftsel || m.isctrlsel;
  if (!insel) m.restrow(); // in selection not restore

  let ret = m.markrow(id, row, null, 'rowselected');

  if (!insel)
    t.lastrow = { lid: id, row: [{ id: parseInt(row), cols: ret }] }; // all the time one row
  else if (!t.lastrow)
    t.lastrow = { lid: id, row: [{ id: parseInt(row), cols: ret }] };
  else if (!t.lastrow.row.find(e => e.id === parseInt(row)))
    t.lastrow.row.push({ id: parseInt(row), cols: ret });

  m.curr_list = id;
  m.curr_row = row;
  m.curr_real = it.value;
  m.show_linfo(id, row);
  m.islsel = true;

  let fun = 'onrowselect' + id;
  m.do_local_stuff(id, row, fun);
  if (t.json.has(fun)) m.send(fun);
}
MRO.iswaitforsel = () => {
  if (!MRO.ismc) return null;
  let a = parent.window.opener;
  return (a && a.MRO.dlgtype === 2) ? a : null;
}
MRO.restrow = () => {
  let r = trans.lastrow;
  if (!r) return;
  let nrows = r.row.length;
  for (let i = 0; i < nrows; ++i) {
    let row = r.row[i];
    MRO.markrow(r.lid, row.id, row.cols, null);
  }
}
MRO.markrow = (id, row, cols, cssclass) => {
  let m = MRO, d = window.document;
  let lir = m.litem(id, row, '');
  let ret = [];
  let one = cols === null;

  let r = d.getElementById(lir.concat('itm')); //current (item number column)
  let rs = r.style;
  m.display(rs, 0);
  if (one) ret.push({ cssclass: r.className }); //before change
  r.className = one ? cssclass : cols[0].cssclass;
  m.display(rs, 2);

  let cars = CARS, nc = m.listncols(id);
  for (let i = 0, j = 1, n = nc; i < n; ++i,++j) {
    r = d.getElementById(lir.concat(cars[i]));
    rs = r.style;
    m.display(rs, 0);
    if (one) ret.push({ cssclass: r.className }); //before change
    r.className = one ? cssclass : cols[j].cssclass;
    m.display(rs, 2);
  }
  return ret;
}
MRO.pass_col2vars = (p, lid, row) => {
  let m = MRO, c = CNS, d = window.document, n = p.getint('vars');
  let gen = m.litem, cols = c.cols, vars = c.vars;
  for (let i = 0; i < n; i++) {
    // 64 is a bug because on client is 65,thats because,we are not reading
    // from the table but on the memory on the deskptop thats the difference
    let col = String.fromCharCode(64 + p.getint(cols[i]));
    let r = d.getElementById(gen(lid, row, col));
    if (!r) return;
    let data = r.value;
    if (data !== '') m.set_val_of(p.get(vars[i]), data);
  }
}
MRO.findPos = obj => {
  let x = 0, y = 0;
  if (obj.offsetParent) {
    do {
      x += obj.offsetLeft;
      y += obj.offsetTop;
    } while (obj = obj.offsetParent);
  }
  return [x, y];
}
MRO.set_cap_pos = (fc, id, row) => {
  let rowid = $2(MRO.litem(id, row, 'itm'));
  if (!rowid) return;
  let isexp = MAC.brws === '2';

  let doc = window.document;
  err.require(!fc, 'formcap_is_missing');
  let form = doc.getElementById('dform');
  let els = fc.childNodes;

  let top = rowid.getBoundingClientRect().top;
  let divtop = form.getBoundingClientRect().top;
  top -= divtop;
  top += form.scrollTop;
  top += rowid.offsetHeight - (isexp ? 7 : 7);

  let u = $2('dtree');
  let lft = (u && u.style.display === 'block') ? (parseInt(u.style.width, 10) + 25) : 0;
  lft -= isexp ? 5 : 10;

  let ncols = MRO.listncols(id);
  let rect = null, rect_left = null;
  let tab = 0;
  let _capctrlsmax = els.length;
  let currctrl = 0;
  let lasteditlen = 0;
  let a = 'l' + id + row;
  for (let i = 0, cars = CARS; ;) {
    if (i < ncols) {
      // inspect list id to know where (left) put edit field
      rect = doc.getElementById(a.concat(cars[i]));
      if (!rect) continue;
        let rectpos = MRO.findPos(rect);
        if (!rectpos) continue;
        rect_left = rectpos[0] - lft;
        ++i;
    }
    if (i === ncols && currctrl === _capctrlsmax) break;
    for (; currctrl < _capctrlsmax; ++currctrl) {
      let ele = els[currctrl];
      let clsn = ele.className;
      if (clsn === undefined) continue;
      let clsl = clsn.length;
      if (clsl === 0 || clsn.charAt(0) !== 'm') continue;
      let sty = ele.style;
      sty.position = 'absolute';
      sty.top = top + 'px';
      tab = 0;

      if ((clsl === 11 && clsn === 'mrobtnmatch') ||
          (clsl === 9 && clsn === 'mrobtnimg')) {
        sty.left = lasteditlen + 'px';
        tab = !ele.width ? 9 : ele.width;
        lasteditlen += tab;
      }
      else {
        sty.left = rect_left + 'px';
        tab = ele.offsetWidth;
        lasteditlen = rect_left + tab;
        currctrl++;
        break;
      }

      /*if ((clsl === 8 && clsn === 'mroinput') ||
        (clsl === 7 && (clsn === 'mrodesc' || clsn === 'mrodate'))) {
        sty.left = rect_left + 'px';
        tab = ele.offsetWidth;
        lasteditlen = rect_left + tab;
        currctrl++;
        break;
      } else
        if ((clsl === 11 && clsn === 'mrobtnmatch') ||
            (clsl === 9 && clsn === 'mrobtnimg')) {
        sty.left = lasteditlen + 'px';
        tab = !ele.width ? 9 : ele.width;
        lasteditlen += tab;
      } else
      if (clsl === 8 && clsn === 'mrocheck') {
        sty.left = rect_left + 'px';
        tab = ele.offsetWidth;
        lasteditlen = rect_left + tab;
        currctrl++;
        break;
      }*/
    }
  }
  fc.style.visibility = 'visible';
}
// save true:read memory 2 fields, false:write fields 2 memory
MRO.update_row = (save, fc, lid, row, real_row) => {
  if (!fc) return;
  let m = MRO, d = window.document;
  let adds = trans.l_adds[lid], bound = trans.l_adbs[lid], nbns = adds.length;
  let els = fc.childNodes;
  let caplen = els.length;
  let captype = linfo[lid].captype;

  if (!save && captype === m.CAP.INS) { }
  else if (m._before_alt_ins !== -1) real_row = m._before_alt_ins;
  m._before_alt_ins = -1;

  let ncols = m.listncols(lid);
  let lr = m.litem(lid, row, '');
  let lastfield = 0;
  let cars = CARS;
  for (let j = 0; j < ncols; ++j) {
    let c = cars[j];
    let item = d.getElementById(lr.concat(c));
    if (!item) continue;
    for (let i = lastfield; i < caplen; i++) {
      let ele = els[i];
      let cnm = ele.className;
      if (cnm === undefined) continue;
      if (cnm.charAt(0) !== 'm') continue;
      let clnl = cnm.length;
      if ((clnl === 8 && cnm === 'mroinput') ||
        (clnl === 7 && cnm === 'mrodesc') ||
        (clnl === 7 && cnm === 'mrodate')) {
        if (save) {
          // pass from list ids to fields ids
          let v = m.get_list_data(lid, real_row - 1, c);
          if (ele.type === 'date')
            ele.value = MRO.rpl_all(v.substr(0, 10), '/', '-');
          else ele.value = v;

          if (captype === m.CAP.INS) { // process bounds if any
            for (let k = 0; k < nbns; ++k) {
              let bnd = bound[k];
              if (bnd.length > 0) {
                if (adds[k].localeCompare(ele.id) === 0) {
                  // check vars on current form
                  let a = $2(bnd);
                  if (a) {
                    ele.value = a.value;
                    break;
                  }
                  // check on session vars
                  if (m.ses.hasval(bnd)) {
                    ele.value = m.ses.get(bnd);
                    break;
                  }
                }
              }
            }
          }
        }
        else {
          item.value = ele.value;
          m.set_list_data(lid, real_row - 1, c, item.value);
        }
        lastfield = i + 1;
        break;
      }
      if (cnm.localeCompare('mrocheck') === 0) {
        if (save) ele.checked = parseInt(m.get_list_data(lid, real_row - 1, c));
        else {
          item.value = ele.checked ? '1' : '0';
          m.set_list_data(lid, real_row - 1, c, item.value);
        }
        lastfield = i + 1;
        break;
      }
      if (cnm.localeCompare('mrobtnmatch') === 0) {
        if (save) { } else { }
      }
    }
  }
  if (!save && captype === m.CAP.INS)
    m.set_list_img(lid, real_row - 1, '1');//???
}
MRO.block_outer_fields = restore => {
  // need to find a way to not change the fields that actually are readonly or are indeed dissabled
  let rls = trans.l_ronlies;
  let x = $2(CNS.frms[0]); // only main form
  for (let i = 0, n = x.length; i < n; i++) {
    let el = x.elements[i];
    let id = el.id;
    if (!id || id.length === 0) continue;
    let cn = el.className;
    if (restore) {
      if (cn.localeCompare('mrobtnmatch') === 0) {
        el.disabled = false;
        continue;
      }
      if (el.parentElement.tagName.localeCompare('TD') === 0) continue;
      if (rls.hasOwnProperty(id)) el.readOnly = false;
    }
    else {
      if (cn.localeCompare('mrobtnmatch') === 0) {
        el.disabled = true;
        continue;
      }
      if (el.parentElement.tagName.localeCompare('TD') === 0) continue;
      el.readOnly = true;
    }
  }
}
MRO.apply_cap_restrictions = (fc, lid, row) => {
  if (!fc) return;
  let first = null, wnd = null, t = trans;
  let lf = linfo[lid];
  let els = fc.childNodes;
  let caplen = els.length;
  for (let i = 0; i < caplen; ++i) {
    let ele = els[i];
    let cnm = ele.className;
    if (cnm === undefined) continue;
    if (cnm.charAt(0) !== 'm') continue;
    let sly = ele.style; 

    // by default we block all fields
    ele.readOnly = true;
    ele.disabled = true;
    sly.backgroundColor = '#a4b7c5';

    let eleid = ele.id;
    if (eleid === '') continue;
    if (lf.captype === MRO.CAP.UPD) {
      let editlen = t.l_edts[lid].length;
      let edits = t.l_edts[lid];
      for (let j = 0; j < editlen; ++j) {
        let item_a = edits[j];
        if (item_a === '') continue;
        if (item_a === eleid) {
          ele.readOnly = false;
          ele.disabled = false;
          if (!first) first = ele;
          sly.removeProperty('background-color');
        }
      }
    }
    if (lf.captype === MRO.CAP.INS) {
      let addlen = t.l_adds[lid].length;
      let adds = t.l_adds[lid];
      for (let j = 0; j < addlen; ++j) {
        let item_a = adds[j];
        if (item_a === '') continue;
        if (item_a === eleid) {
          ele.readOnly = false;
          ele.disabled = false;
          if (!first) first = ele;
          sly.removeProperty('background-color');
        }
      }
    }
  }
  if (first) first.focus();
}
MRO.do_list = capt => {
  let m = MRO, ev = '';
  if (!m.isincapt) return;
  document.activeElement.blur(); // force lose focus trigger code if any

  if (capt === m.CAP.INS) ev = 'oninsert';
  else if (capt === m.CAP.UPD) ev = 'onedit';
  else if (capt === m.CAP.DEL) ev = 'ondelete';
  else return;

  let lid = m.curr_list;
  ev += lid;
  m.send(ev);
  let real_row = m.curr_real !== '*' ? m.curr_real : linfo[lid].nrows + 1;
  let curr_row = m.curr_row;
  m.update_row(false, $2('formcap' + m.curr_list), m.curr_list, m.curr_row, real_row);
  m.close_cap();

  if (capt === m.CAP.DEL) {
    let ncols = MRO.listncols(lid), c = CARS;
    for (let i = 0; i < ncols; i++)
      m.set_list_data(lid, real_row - 1, c[i], '~');
    m.list_update(lid, real_row);
  }
  on_lclick(lid, curr_row);
}
MRO.capture_reg = (rowid, lid, row) => {
  let m = MRO, lf = linfo[lid];
  lf.captype = rowid.value.indexOf('*') === -1 ? m.CAP.UPD : m.CAP.INS;
  if (lf.captype === m.CAP.INS && lf.has_ins === false) return;
  if (lf.captype === m.CAP.UPD && m.has_fun('onedit'.concat(lid)) === false) return;

  try {
    let real_row = m.curr_real !== '*' ? m.curr_real : linfo[lid].nrows + 1;
    let fc = $2('formcap' + lid);
    m.set_cap_pos(fc, lid, row);
    m.update_row(true, fc, lid, row, real_row);
    m.apply_cap_restrictions(fc, lid);
    m.block_outer_fields(false);
    m.isincapt = true;
  }
  catch (e) {
    err.rescue(e)
  }
}
MRO.post_cap = (res, evnt) => {
  let m = MRO, ln = evnt.length;
  if (ln === 0) return;
  let f = evnt.slice(0, ln - 1);

  let lid = -1;
  if (ln === 8 + 1 && f.localeCompare('oninsert') === 0) lid = evnt.slice(ln - 1, ln);
  else
    if (ln === 6 + 1 && f.localeCompare('onedit') === 0) lid = evnt.slice(ln - 1, ln);
  if (lid === -1) return;

  let lf = linfo[lid];
  let row = m.curr_real !== '*' ? m.curr_real : lf.nrows + 1;
  let img = -1;
  if (res.hasval('cimage')) img = res.getint('cimage');
  if (img === -1) img = 1;
  if (img !== -1) m.set_list_img(lid, row - 1, img);

  let newpage = false;
  let hrow = ((row/*-1*/) % linfo[lid].rwspag);//-1;//last value row
  let rid = $2(m.litem(lid, hrow, 'itm'))
  let hasnew = rid && rid.value === '*';//already has new? maybe list was regenerated
  if (!hasnew && lf.captype === m.CAP.INS) {//add new row(*)
    let nr = (lf.nrows % lf.rwspag) + 1;
    if (nr === lf.rwspag) {
      lf.lstpag += 1;
      nr = 0;
      newpage = true;
    }//nextpage
    lf.nrows++;
    if (!newpage) LST._insert_row(lid, nr); // when new page no need add (*) row
    trans.l_res.set(CNS.tots[lid], lf.nrows);
    m.show_linfo(lid, 0);
    //on_lclick(lid,m.curr_row);
  }
  //if (lf.captype === m.CAP.INS || lf.captype === m.CAP.UPD) {//maybe need update column(s)
    /*let n = res.getint('zupcols');//old method
    if (n > 0) {
      let col = '', value = '';
      for (let i = 0; i < n; ++i) {
        col = i < 10 ? 'zupcol0' + i : 'zupcol' + i;
        let colid = res.getint(col);
        col = i < 10 ? 'zupcolval0' + i : 'zupcolval' + i;
        value = res.get(col);
        m.set_list_data(lid, row - 1, colid, value);
      }
    }*/
    /*for (let i = 0, cars = CARS, n = m.listncols(lid); i < n; ++i) {//new method check cell to change
      let k = 'lcell'.concat(cars[i]);
      if (res.has(k)) m.set_list_data(lid, row - 1, cars[i], res.get(k));
    }*/
  //}
  if (!hasnew) m.list_update(lid, row);
  on_lclick(lid, m.curr_row);
}
MRO.do_local_stuff = (lid, row, fun) => {
  let d = MRO.make(trans.json.get('dolocal'));
  if (d.isempty()) return;
  let a = MRO.make(d.get(fun));
  if (a.notempty()) {
    let p = MRO.make(a.get('lcell2var'));
    if (p.notempty()) MRO.pass_col2vars(p, lid, row);
  }
}
MRO.mouseoldy = -1;
MRO.mousedown = e => {
  if (trans.lastrow) MRO.mouseoldy = e.pageY;
}
MRO.mouseup = e => {
  if (!trans.lastrow) return; // not selected
  if (MRO.mouseoldy === -1) return; // not down first
  if (e.pageY < (MRO.mouseoldy - 16)) move_page_down();
  else if (e.pageY > (MRO.mouseoldy + 16)) move_page_up();
  MRO.mouseoldy = -1;
}
MRO.mro2json = p => {//FIX BUG-data comes old format
  let m = MRO, f = m.rpl_all;
  p = f(p, '][', '","');
  p = f(p, '[', '"');
  p = f(p, ':', '":"');
  p = f(p, ']', '"');
  return new pairs('{' + p + '}');
}
MRO.new_list = (res, lid, nrows, ncols) => {
  let m = MRO, t = document.getElementById(CNS.zlst[lid]);
  if (!t) return;

  while (t.hasChildNodes()) {
    t.removeChild(t.firstChild);
  }

  let c = CARS, z = CNS.zlci[lid], srows = '';
  let h = '<colgroup id="colg'.concat(lid, '"span="', ncols, '">');
  let ccols = '', collns = [];
  for (let j = 0; j < ncols; ++j) {
    let zi = z + j;
    let dsc = res.get(zi.concat('z'));
    let len = res.get(zi.concat('l'));
    let typ = res.get(zi.concat('t'));
    h += '<col id="'.concat(c[j], '"dsc="', dsc, '"width="', len, '"></col>');
    ccols += '<td><input class="rowhdr"readonly="true"tabindex="-1"id="l'.concat(lid, 'c', c[j],
      '"value="', dsc, '"maxlength="', len, '"size="', len, '"></input></td>');
    collns.push(len);
  }
  h += '</colgroup>'.concat(
    '<input type="hidden"id="rowspertable', lid, '"value="', nrows, '"></input>',
    '<tr class="rowheader">',
    '<td><input class="rowhdr"readonly="true"tabindex="-1"value="item"maxlength="9"size="9"></input></td>',
    ccols, '</tr>');
  let rows = '';
  for (let i = 0; i < nrows; ++i) {
    let clss = i % 2 === 0 ? 'rowodd' : 'roweven';
    let lidi = lid + '\',\'' + i;
    let li = lid.toString() + i.toString();
    rows += '<tr class="'.concat(clss, '"onclick="on_lclick(\'', lidi, '\');">',
      '<td ondblclick="on_ldclick(\'', lidi, '\',\'itm\');">',
      '<img src=""class="mroimgrow"id="l', li, 'img"style="visibility:hidden;"/>',
      '<input class="rowitem"readonly="true"tabindex="-1"id="l', li,
      'itm"maxlength="6"size="6"></input></td>');
    for (let j = 0; j < ncols; ++j) {
      rows += '<td ondblclick="on_ldclick(\''.concat(lidi, '\',\'', c[j], '\');">',
        '<input class="rowitem"readonly="true"tabindex="-1"id="l', li, c[j],
        '"maxlength="', collns[j], '"size="', collns[j], '"></input></td>');
    }
    rows += '</tr>';
  }
  let tmp = document.createElement('div');
  tmp.innerHTML = '<table><tbody>'.concat(h, rows, '</tbody></table>');
  t.appendChild(tmp.cloneNode(true));
}
MRO.creupld = () => {
  let d = window.document;
  let f = d.createElement("form");
  f.setAttribute('id', 'file_upload_form');
  f.setAttribute('enctype', "multipart/form-data");
  d.getElementsByTagName('body')[0].appendChild(f);
  let i = d.createElement('input');
  i.type = 'file';
  i.id = '_file';
  i.name = 'filename';
  i.style.visibility = 'hidden';
  f.appendChild(i);
  return f;
}
MRO.del_ctrls = () => {
  //MRO.clr_linfo();
  if (MRO.ismc) return;
  for (let i = 0; i < trans.treemax; ++i) {
    let t = $2('tree' + i);
    if (t) t.innerHTML = '';
  }
}
MRO.close_sel = () => {
  if (!MRO.islsel) return;
  MRO.restrow();
  MRO.islsel = false;
  MRO.curr_list = null;
  MRO.curr_row = null;
  trans.lastrow = null;
}
MRO.handle_esc = () => {
  let m = MRO, t = trans;
  if (parent.window.opener && parent.window.opener.MRO.dlgtype === 2) close();
  m.close_sel();
  if (m.isincapt) m.close_cap();
  else m.close_match();
  m.hide_pop_menu();
  m.hide_menu();
  if (!m.ismc) m.display($2('prof').style,0);
}
MRO.close_match = () => {
  if (MRO.dlgtype !== 8 && MRO._match)
    MRO._match.close();
}
String.prototype.replaceAt = function (i, c) {
  return this.substr(0, i) + c + this.substr(i + c.length);
}
MRO.hdate_ = id => {
  let i = window.document.getElementById(id);
  if (!i) return;
  let e = window.event;
  let k = e.keyCode || e.which;
  if (k === 8) return;
  let v = i.value;
  let vl = v.length;
  if (vl >= 4) i.value = v.replaceAt(4, '/');
  if (vl >= 7) i.value = v.replaceAt(7, '/');
}
MRO.hmoney_ = id => {
/*  let i = window.document.getElementById(id);
  if (!i) return;
  let e = window.event;
  let k = e.keyCode || e.which;
  //if (k === 8) return;
  let v = i.value;
  //let vl = v.length;

  v = MRO.rpl_all(v, ',', '');
  //v = MRO.rpl_all(v, '.', '');
  let vl = v.length;

  //if (vl >= 11) v = v.substr(0, vl - 3) + ',' + v.substr(vl - 3);
  //else
  if (vl >= 7) v = v.substr(0, vl - 6) + ',' + v.substr(vl - 6, 3) + ',' + v.substr(vl - 3);
  else
  if (vl >= 4) v = v.substr(0, vl - 3) + ',' + v.substr(vl - 3);
  //if (vl >= 11) v = v[0] + ',' + v.substr(1, 3) + ',' + v.substr(4, 3) + '.' + v.substr(7);

  i.value = v;*/
}
MRO.context_menu = (x, y) => {
  let t = trans, m = $2('menupop'), s = m.style;
  s.left = x + 'px';
  s.top = y + 'px'; 
  MRO.display(s, 2);
  if (t.mpopitems && t.mpopitems !== '') m = MRO.repHtml(m, t.mpopitems);
  else m.innerHTML = DFS.NONMENU;
}
MRO.OnContextMenu = e => {
  e = e ? e : window.event;
  var t = e.target, go = false;
  if (t && t.id === 'dform') go = true;
  if (!go) { MRO.hide_pop_menu(); return false; }
  MRO.gen_menucontext(0, '', e.clientX, e.clientY);
  return false;
}
MRO.on_click = (e) => {
  if (trans.hidepop) MRO.hide_pop_menu();
  trans.hidepop = true;
  return true;
}
MRO.do_matchcode = (m, p) => {
  let send = '', receive = '', act = '', parms = new pairs();
  let dlgtype = 2, name = p.get('matchcode');

  if (p.has('send')) send = p.get('send');
  if (p.has('receive')) receive = p.get('receive');
  if (p.has('action')) act = p.get('action');
  if (p.has('params')) parms.attach(p.get('params'));
  if (p.has('nodlgtype')) dlgtype = 0;
  if (p.ison('newapp')) {
    execmd('newapp', name);
    return true;
  }

  m.show_dialog(name, send, receive, dlgtype, parms, act);
  return true;
}
MRO.do_gototrans = (m, p) => {
  let prms = m.make(p.get('params'));
  if (p.has('gototrans')) prms.set('where', p.get('gototrans'));
  if (p.has('where')) prms.set('where', p.get('where'));
  prms.set('function', p.get('function'));
  let retrow = prms.buffer().indexOf('lcell');
  let values = m.gen_values(retrow !== -1);
  prms.merge(prms, values);

  let t = '{"where":"'.concat(prms.get('where'), '","function":"',
          prms.get('function'), '","params":');
  prms.del('where');
  prms.del('function');
  if (prms.isempty()) t += '""';
  else t += prms.buffer();
  m.gui('gui_goto_trans', t.concat('}'));
  return true;
}
MRO.do_case = (ev, m, p) => {
  let prms = m.make(p.get('case'));
  let f = '', v = prms.get('var');
  if (v.length !== 0) f = m.get_val_of(v);
  else {
    v = prms.get('ses');
    if (v.length !== 0) f = m.ses.get(v);
  }
  v = prms.get(f);
  if (v.length === 0) v = prms.get('default');
  if (v.length > 0) {
    let isrecursion = ev.length === v.length && ev.localeCompare(v) === 0;
    err.require(isrecursion, '*recursion*');
    handle_ws(v, null);
  }
  return true;
}
MRO.do_goto = (ev, r) => {
  let m = MRO, p = m.make(r);
  if (p.has('matchcode')) return m.do_matchcode(m, p);
  if (p.has('gototrans') || p.has('where')) return m.do_gototrans(m, p);
  if (p.has('case')) return m.do_case(ev, m, p);
  return false;
}
MRO.do_disp = (tname, ev, res, guiev) => {
  let p = null;
  if (res) {
    let tcode = res.get(DFS.ZTRNCOD);
    err.require(tcode === tname && guiev === ev, '*recursion*');
    p = MRO.make(res.get('parameters')).tojson();
  }
  MRO.send(ev, p);
}
MRO.key_ctrl = (m, e, k) => {
  if (k === 67 || k === 70 || k === 86) return;//copy,find,paste
  if (k === 81) $2('command').focus();//focus cmdline
  if (k === 48) execmd('cmdbox', '1');//cmdbox
  if (k === 68) {
    if (sessionStorage.getItem('usrtype') === 'A')
      execmd('load', 'dev', false), DEV.DEVCON();
  }//dev
  else m.send('onctrl-'.concat(String.fromCharCode(k)).toLowerCase());
  e.preventDefault();
}
MRO.set_hmtl = p => {
  try {
    let m = MRO, s = MRO.ses, x = sessionStorage;
    m.check_resp(p);
    let html = p.get(DFS.ZFILERS);
    let a = $2('prof');
    let v = m.gen_values(false);
    for (let k in v._) html = m.rpl_all(html, '#' + k + '#', v.get(k));
    for (let k in s._) html = m.rpl_all(html, '#' + k + '#', s.get(k));
    for (let i = 0, n = x.length; i < n; ++i) {
      let k = x.key(i);
      html = m.rpl_all(html, '#' + k + '#', x.getItem(k));
    }
    a = m.repHtml(a, html);
    m.display(a.style,1);
  }
  catch (e) {
    err.rescue(e);
  }
}
MRO.load_html = (code, extra) => {
  try {
    let p = MRO.make_fileprms('trans', code, 'HTM', '');
    MRO.clie.proxy('get_file', p.buffer(), true, MRO.set_hmtl, extra);
  }
  catch (e) { err.rescue(e); };
}
window.document.oncontextmenu = MRO.OnContextMenu;
window.document.onclick = MRO.on_click;
(function () {
  if (MRO.ismain) MRO.show_time();
})();
