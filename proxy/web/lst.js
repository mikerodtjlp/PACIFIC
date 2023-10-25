var LST = {};
LST._fns = (c, p0, p1) => {
  let f = LST[c], r = typeof f === 'function';
  if (r) f(p0, p1);
  return r;
}
DFS.ZDATA = 'data';
DFS.ZCOLS = 'cols';
DFS.ZTYPES = 'types';
DFS.ZNROWS = 'nrows';
DFS.ZNCOLS = 'ncols';
LST.LSELITEM = () => { execmd('SIMDBCLK'); }
LST.LESCITEM = () => { MRO.handle_esc(); }
LST.LEDIITEM = () => { err.require(!MAC.mobi, 'only_mobile'); execmd('SIMDBCLK'); }
LST.LDELITEM = () => { err.require(!MAC.mobi, 'only_mobile'); MRO.del_list_row(); }
LST.SFNDLI = pms => { LST.show_find_item(pms); }
LST.CHSCOLSLI = lid => {
  if (lid === -1) return;
  let c = '{"$lid$":"' + lid + '","$trans$":"' + trans.name + '"}';
  MRO.show_dialog('SL00', c, new pairs(), 0, null, 'onenter');
}
LST.CHSFLDSLI = p => {
  let m = MRO, prms = m.mro2json(p);
  let lid = prms.getint('listid');
  let ncols = m.listncols(lid);
  if (lid === -1) return;
  if (ncols > 18) ncols = 18;

  let send = new pairs();
  send.set('$id$', lid.toString());
  prms.set('ncols', ncols);
  for (let i = 0; i < ncols; ++i)
    prms.set('col' + i, i + '');

  m.show_dialog(DFS.ZTCHFLD, send.buffer(), new pairs(), 0, null);
  LST.show_field_names(prms, 0);
}
LST.show_find_item = pr => {
  let m = MRO, p = m.mro2json(pr);
  if (p.getint('listid', -1) !== -1) LST.show_find_list_item(p);
  if (p.getint('textid', -1) !== -1) LST.show_find_text_item(p);
}
LST.sortli = (lid, col, typ) => {
  let m = MRO, cars = CARS;
  let result = trans.l_res;
  let helper = CNS.zl[lid];
  let c = cars[col];
  let ncols = m.listncols(lid);
  let nrows = result.getint(CNS.tots[lid]);
  let up2dw = typ === 0; // up to down or down to up
  for (let i = 0; i < nrows - 1; i++) {
    for (let j = 0; j < nrows - i - 1; j++) {
      let lft = helper + j;
      let rgt = helper + (j + 1);
      let kel = result.get(lft.concat(c));
      let ker = result.get(rgt.concat(c));
      let cmp = kel.localeCompare(ker);
      if (up2dw ? cmp === 1 : cmp === -1) {
        for (let k = 0; k < ncols; ++k) {
          let cark = cars[k];
          let left = lft.concat(cark);
          let righ = rgt.concat(cark);
          let temp = result.get(left);
          result.set(left, result.get(righ));
          result.set(righ, temp);
        }
      }
    }
  }
  m.btn_list_top(lid);
}
LST.SETCOLS = p => {
  if (!trans.l_res) return; // no list no refresh
  let lid = p.getint('lid');
  MRO.get_css_list(lid, true);
}
LST.SORTUPLI = lid => {
  let col = CNS.zlcs[lid];
  LST.sortli(lid, col === -1 ? 0 : col, 0);
}
LST.SORTDOWNLI = lid => {
  let col = CNS.zlcs[lid];
  LST.sortli(lid, col === -1 ? 0 : col, 1);
}
LST.show_find_list_item = prms => {
  let m = MRO, lid = prms.getint('listid');
  if (lid === -1) return;

  let send = new pairs();
  send.set('$type$', 'list');
  send.set('$id$', lid.toString());
  // what I'm trying to do here,a trick? if so describe it,it is confused
  let res = m.make(send.buffer());
  let src = m.make(send.buffer());
  let vals = m.gen_values(false);
  res.merge(src, vals);

  m.show_dialog(DFS.ZTRNFLI, res.buffer(), new pairs(), 0, null);
  LST.show_field_names(prms, 0);
}
LST.show_field_names = (parms, tries) => {
  let m = MRO, a = m._match;
  if (!a) return;
  let lid = parms.getint('listid');
  let ncols = m.listncols(lid);
  let cars = CARS;

  let first = true, cols = CNS.cols;
  for (let i = 0; i < ncols && i < 18; ++i) {
    let col = i + '';
    if (tries === 160) return;
    if (!a.MRO || a.MRO === undefined ||
      !a.MRO.set_val_of || a.MRO.set_val_of === undefined) {
      setTimeout(function () { LST.show_field_names(parms, ++tries); }, 64);
      return;
    }

    let c = $2('l'.concat(lid, 'c', cars[i]));
    if (!c) continue;
    let label = '$lab'.concat(col, '$');
    let input = '$value'.concat(col, '$');
    a.MRO.set_val_of(label, c.value);
    let f = a.window.document.getElementById(label);
    if (f)// && (f.className === 'mroinput' || f.className === 'mrocheck'))
      m.display(f.style, 2);
    f = a.window.document.getElementById(input);
    if (f) {// && (f.className === 'mroinput' || f.className === 'mrocheck')){
      f.size = c.size;
      f.maxlength = c.maxlength;
      m.display(f.style, 2);
      if (first) { f.focus(); first = false; }
    }
  }
}
LST.find_list2_item = parms => {
  let m = MRO, found = -1, tofind = [10];
  let lid = parms.getint('lid');
  let ncols = m.listncols(lid);
  if (lid === -1) lid = 0;

  for (let i = 0; i < ncols && i < 9; ++i)
    tofind[i] = parms.get('value' + i);

  let lf = linfo[lid];
  let helper = CNS.zl[lid];
  let v, it, n = lf.nrows;
  let cars = CARS;
  let res = trans.l_res;
  loop1:

  for (let i = 0; i < ncols; ++i) {
    it = tofind[i];
    if (!it || it.length === 0) continue;
    for (let j = 0; j < n; ++j) {
      let data_row = helper + j;
      v = res.get(data_row.concat(cars[i]));
      if (it.length <= v.length) {
        let nv = v.substr(0, it.length);
        if (nv.localeCompare(it) === 0) {
          found = j;
          break loop1;
        }
      }
    }
  }
  end:

  m.close_match();
  if (found !== -1) {
    lf.curpag = Math.floor(found / lf.rwspag) + 1;
    m._fill_list(lid, trans.l_res);
    on_lclick(lid, found % lf.rwspag);
    m.status('item_found', 'line:' + (found + 1), DFS.OK);
  }
  else m.status('item_not_found', '', DFS.WARNING);
}
LST.FNDLI = p => {
  if (p.are_eq('type', 'list')) LST.find_list2_item(p);
  if (p.are_eq('type', 'text')) LST.find_text_item(p);
}
LST.l2data = type => { // selection range
  let m = MRO, wd = window.document, t = trans, f = DFS;
  if (!m.islsel) return null;
  let lid = trans.lastrow.lid;
  let data = new pairs();
  let ncols = m.listncols(lid);
  let nrows = t.lastrow.row.length;
  let cars = CARS;
  for (let i = 0; i < nrows; ++i) {
    let si = i.toString()
    let row = t.lastrow.row[i].id;
    for (let j = 0; j < ncols; ++j) {
      let cell = cars[j];
      data.set(cell.concat(si), wd.getElementById(m.litem(lid, row, cell)).value);
    }
  }
  data.set('lstid', lid);
  data.set(f.ZNROWS, nrows);
  data.set(f.ZNCOLS, ncols);
  let res = null;
  if (type === '0') res = m.make('{"'.concat(DFS.ZDATA, '":', data.buffer(), '}'));
  else if (type === '1') res = data;
  return res;
}
LST.list_c = lid => { // selection whole list
  if (!trans.l_res) return null;
  let m = MRO, d = window.document, f = DFS;
  let iid = parseInt(lid);
  let res = trans.l_res;
  let data = new pairs();
  let cols = new pairs();
  let typs = new pairs();
  let z = CNS.zlci[lid];
  //let ncols=m.listncols(iid);
  let nrows = res.getint(CNS.tots[iid]);
  if (nrows === 0) return null;
  let cars = CARS;
  let colg = $2(CNS.zclg[iid]);
  let ncols = linfo[iid]._d_ = !colg ? 0 : colg.span;
  let zl = CNS.zl[iid];
  for (let i = 0; i < nrows; ++i) {
    let si = i.toString()
    let drow = zl + si;
    for (let j = 0; j < ncols; ++j) {
      let cj = cars[j];
      if (i === 0) {
        cols.set(cj, d.getElementById(zl.concat('c', cj)).value);
        typs.set(cj, res.get((z + j + '').concat('t')));
      }
      data.set(cj.concat(si), res.get(drow.concat(cj)));
    }
  }
  data.set(f.ZCOLS, cols._);
  data.set(f.ZTYPES, typs._);
  data.set(f.ZNROWS, nrows);
  data.set(f.ZNCOLS, ncols);
  return data;
}
LST.list_2 = (lid, fun, async) => {
  let data = LST.list_c(lid);
  if (data) {
    let prms = '{"';
    if (linfo[lid].dataset) prms += DFS.ZLSTAFF.concat('":"', lid, '","usedataset":"1","');
    prms += DFS.ZDATA.concat('":', data.buffer(), '}');
    MRO.clie.site('core', '', fun, prms, async);
  }
  else MRO.status('empty_list', '', DFS.WARNING); return;
}
LST.L2CLIP = lid => {
  if (!trans.l_res) return;
  let iid = parseInt(lid);
  let m = MRO, d = window.document;
  let res = trans.l_res;
  let ncols = m.listncols(iid);
  let nrows = res.getint(CNS.tots[iid]);
  if (nrows === 0) return;
  let colg = $2(CNS.zclg[iid]);
  ncols = linfo[iid]._d_ = !colg ? 0 : colg.span;
  let t = '', zl = CNS.zl[iid], c = CARS;
  for (let j = 0; j < ncols; ++j)
    t += d.getElementById(zl.concat('c', c[j])).value + '\t';
  t += '\r\n';
  for (let i = 0; i < nrows; ++i) {
    let drow = zl + i;
    for (let j = 0; j < ncols; ++j)
      t += res.get(drow.concat(c[j])) + '\t';
    t += '\r\n';
  }
  window.prompt("Ctrl+C,Enter", t);
}
LST.L2HTML = l => { LST.list_2(l, 'compose_html', true); }
LST.L2EXCEL = l => { LST.list_2(l, 'compose_workbook', true); }
LST.L2PDF = l => { LST.list_2(l, 'compose_pdf', true); }
LST.L2EMAIL = l => { MRO.show_dialog(DFS.ZSEMAIL, new pairs(), new pairs(), 0, null); }
LST.MAILSVR = r => {
  let m = MRO;
  let lid = r.extract('lid');
  let d = LST.list_c(lid);
  if (d === null || d === undefined) return;
  let rr = '{"data":'.concat(d.buffer(), ',', r.tojson(), '}');
  m.clie.site('core', '', 'compose_email', rr, true);
  m.close_match();
}
LST.PKEYLST = k => { // find first item by press key
  if (!MRO.islsel || MRO.isincapt) return;
  let lid = MRO.curr_list;
  let col = CNS.zlcs[lid];
  if (col === -1) return;
  execmd('fndli', MRO.make('{"lid":"'.concat(lid, '","type":"list","value', col, '":"', k, '"}')));
}
LST.SIMDBCLK = () => { // simulate dbclick
  if (MRO.islsel && !MRO.isincapt)
    on_ldclick(MRO.curr_list, MRO.curr_row, '');
}
LST.FILTERLI = p => {
  let m = MRO, prms = m.mro2json(p);
  let lid = prms.getint('listid');
  if (lid === -1) return;

  let send = new pairs();
  send.set('$type$', 'list');
  send.set('$id$', lid.toString());
  // what I'm trying to do here,a trick? if so describe it,it is confused
  let res = m.make(send.buffer());
  let src = m.make(send.buffer());
  let vals = m.gen_values(false);
  res.merge(src, vals);

  m.show_dialog(DFS.ZTRNFIL, res.buffer(), new pairs(), 0, null);
  LST.show_field_names(prms, 0);
}
LST.FLTRLI = parms => {
  let m = MRO, tofind = [18];
  let lid = parms.getint('lid');
  let ncols = m.listncols(lid);
  if (lid === -1) lid = 0;

  for (let i = 0; i < ncols && i < 18; ++i)
    tofind[i] = parms.get('value' + i);

  let lf = linfo[lid];
  let helper = CNS.zl[lid];
  let v, it, n = lf.nrows;
  let cars = CARS;
  let result = trans.l_res;
  let newr = 0;

  for (let i = 0; i < ncols; ++i) {
    it = tofind[i];
    if (it.length === 0) continue;
    for (let j = 0; j < n; ++j) {
      let data_row = helper + j;
      v = result.get(data_row.concat(cars[i]));

      if (it.localeCompare(v.substr(0, it.length)) === 0) { //should be include
        let data_new = helper + newr;
        for (let k = 0; k < ncols; ++k) {
          let item = result.get(data_row.concat(cars[k]));
          result.set(data_new.concat(cars[k]), item);
        }
        ++newr;
      }
    }
  }

  result.set(CNS.tots[lid], newr + '');

  m.btn_list_top(lid);
}
LST.SHWLIFLDS = parms => {
  let m = MRO, lid = parms.getint('lid');
  if (lid === -1) lid = 0;
  let helper = CNS.zl[lid];
  let ncols = m.listncols(lid);
  let rowsxpage = linfo[lid].rwspag;

  for (let i = 0; i < rowsxpage; ++i) {
    for (let col = 0; col < ncols && col < 18; ++col) {
      let colid = parms.get('value' + col);
      if (colid === '0') { // hide it
        // input
        let id = helper.concat(i + '', String.fromCharCode(65 + parseInt(col)));
        let el = $2(id);
        let st = el.style;
        st.width = st.paddingLeft = st.paddingRight = '0px';
        // td
        el = el.parentNode;
        st = el.style;
        st.width = st.paddingLeft = st.paddingRight = '0px';
        if (i !== 0) continue;
        // column
        id = helper.concat('c', String.fromCharCode(65 + parseInt(col)));
        el = $2(id);
        st = el.style;
        st.width = st.paddingLeft = st.paddingRight = '0px';
      }
    }
  }

  m.close_match();
}
LST._insert_row = (id, row) => {
  let rowid = $2(MRO.litem(id, row, 'img'));
  if (!rowid) return;
  rowid.style.visibility = 'visible';
  rowid.src = trans.l_imgs[parseInt(id)][0];
  rowid = $2(MRO.litem(id, row, 'itm'));
  if (!rowid) return;
  rowid.style.visibility = 'visible';
  rowid.value = '*';
}
MRO.set_list_img = (l, r, img) => { MRO.set_list_data(l, r, '*', img); }
MRO.set_list_data = (l, r, c, d) => { trans.l_res.set('l' + l + r + c, d); }
MRO.get_list_data = (l, r, c) => { return trans.l_res.get('l' + l + r + c); }
MRO.list_update = (lid, row) => {
  let hrow = ((row - 1) % linfo[lid].rwspag);//-1;
  let rowid = $2(MRO.litem(lid, hrow, 'img'));
  if (rowid) {
    let img = MRO.get_list_data(lid, row - 1, '*');
    rowid.src = trans.l_imgs[parseInt(lid)][parseInt(img)];
    rowid = $2(MRO.litem(lid, hrow, 'itm'));
    if (rowid) rowid.value = row;
    let ncols = MRO.listncols(lid);//linfo[lid].ncols;
    let cars = CARS;
    let gen = MRO.litem;
    for (let i = 0; i < ncols; ++i) {
      rowid = $2(gen(lid, hrow, cars[i]));
      if (rowid) rowid.value = MRO.get_list_data(lid, row - 1, cars[i]);
    }
  }
}
MRO.ins_list_row = () => {
  let lid = 0, t = trans;        // assume the 0 list (most common)
  if (!t.lastrow) {              // not row selected
    if (t.listmax !== 1) return; // more than one can't be deduced
  } else lid = t.lastrow.lid;    // get the selected one

  let m = MRO;
  if (!linfo[lid].has_ins) return;
  m._before_alt_ins = m.curr_real;
  m.btn_list_bottom(lid);

  let rid = $2(m.litem(lid, m.curr_row, 'itm'));
  if (rid && rid.value !== '') m.capture_reg(rid, lid, m.curr_row);
}
MRO.del_list_row = () => {
  if (!trans.lastrow) return;
  let m = MRO, lid = trans.lastrow.lid;
  let lf = linfo[lid];
  if (m.has_fun('ondelete'.concat(lid)) === false) return;
  if (lf.has_ins && m.curr_real === '*') return;
  //try{
  let row = m.curr_real;
  let data = m.get_list_data(lid, row - 1, String.fromCharCode(65 + 1));
  if (data === '~') return;
  lf.captype = m.CAP.DEL;
  m.isincapt = true;
  //let delrow = m.curr_row;
  m.update_row(true, $2('formcap' + lid), lid, m.curr_row, row);
  m.do_list(m.CAP.DEL);
  // the i+1 is because we must start from 1 not 0,0 is the column of the reg count
  /*let ncols = MRO.listncols(lid);
  let c = CARS;
  for (let i = 0; i < ncols; i++) m.set_list_data(lid, row - 1, c[i], '~');
  m.list_update(lid, row);
  on_lclick(lid, delrow);*/
  //}catch (e) { err.rescue(e) }
}
colonclick = (lid, col) => { colondblclick(lid, col); }
colondblclick = (lid, col) => {
  let helper = CNS.zl[lid] + 'c';
  let colid = helper + String.fromCharCode(65 + parseInt(col));
  let pcol = $2(colid);
  if (pcol) {
    let ps = pcol.style;
    // restore previous if any
    if (CNS.zlcs[lid] !== -1) {
      colid = helper + String.fromCharCode(65 + CNS.zlcs[lid]);
      ocol = $2(colid);
      if (ocol) {
        let z = ocol.style;
        z.backgroundColor = ps.backgroundColor;
        z.background = ps.background;
        z.color = ps.color;
      }
    }
    ps.backgroundColor = '#80A0C0';
    ps.background = "linear-gradient(to bottom, #E2E2E2,#80A0C0)";
    ps.color = "midnightblue";
    CNS.zlcs[lid] = parseInt(col);
  }
}
MRO.get_page_data = (lid, page) => {
  let p = MRO.make('');
  p.set(DFS.ZLSTAFF, lid);
  p.set('targetpage', '' + page);
  p.set('rowsperpage', $2('rowspertable'.concat(lid)).value)
  let r = MRO.clie.site('core', '', 'exec_dataset_page', p.buffer(), false);
  trans.l_res.append(r);
}
MRO.btn_list_top = lid => {
  if (MRO.isincapt) return;
  let lf = linfo[lid];
  if (lf.lstpag === -1) return;
  lf.curpag = 1;
  MRO.close_cap();
  if (lf.dataset) MRO.get_page_data(lid, lf.curpag);
  MRO._fill_list(lid, trans.l_res);
  on_lclick(lid, 0);
}
MRO.btn_list_up = lid => {
  if (MRO.isincapt) return;
  let lf = linfo[lid];
  if (lf.lstpag === -1) return;
  let cp = lf.curpag;
  if (cp > 1) {
    cp--;
    lf.curpag = cp;
    MRO.close_cap();
    if (lf.dataset) MRO.get_page_data(lid, lf.curpag);
    MRO._fill_list(lid, trans.l_res);
    on_lclick(lid, lf.rwspag - 1);
  }
}
MRO.btn_list_down = lid => {
  if (MRO.isincapt) return;
  let lf = linfo[lid];
  if (lf.lstpag === -1) return;
  let lastpage = lf.lstpag;
  let cp = lf.curpag;
  if (cp < lastpage) {
    cp++;
    lf.curpag = cp;
    MRO.close_cap();
    if (lf.dataset) MRO.get_page_data(lid, lf.curpag);
    MRO._fill_list(lid, trans.l_res);
    on_lclick(lid, 0);
  }
}
MRO.btn_list_bottom = lid => {
  if (MRO.isincapt) return;
  let lf = linfo[lid];
  if (lf.lstpag === -1) return;
  lf.curpag = lf.lstpag;
  MRO.close_cap();
  if (lf.dataset) MRO.get_page_data(lid, lf.curpag);
  MRO._fill_list(lid, trans.l_res);

  let linf = linfo[lid];
  let rowsxtable = linf.rwspag;
  let nrows = linf.nrows;
  let page = linf.curpag;
  let offset = (page - 1) * rowsxtable;
  let row = (nrows - offset);
  row -= lf.has_ins ? 0 : 1;
  on_lclick(lid, row);
}
move_page_up = () => { MRO.btn_list_up(trans.lastrow.lid); }
move_page_down = () => { MRO.btn_list_down(trans.lastrow.lid); }
move_row_up = () => {
  let lid = parseInt(MRO.curr_list);
  let row = parseInt(MRO.curr_row);
  if (row === 0) MRO.btn_list_up(lid);
  else on_lclick(lid, row - 1);
}
move_row_down = () => {
  let lid = parseInt(MRO.curr_list);
  let row = parseInt(MRO.curr_row);
  let lf = linfo[lid];
  if ((row + 1) === lf.rwspag) {
    let bound = lf.nrows;
    bound += lf.has_ins ? 1 : 0;
    if (parseInt(MRO.curr_real) < bound) MRO.btn_list_down(lid);
  }
  else {
    let r = ((lf.curpag - 1) * lf.rwspag) + row;
    let t = lf.nrows;
    if (r < t - 1) on_lclick(lid, row + 1);
  }
}
LST.L2HELP = l => {
  if (!MRO.clie.isactive() || MRO.ismc) return;
  let a = $2('prof'), b = a.style;
  if (b.display === 'none') {
    let t = $2('theform').getBoundingClientRect();
    let top = t.top - 96;//(t.height - 400) / 2;
    let lft = (t.width - 550) / 2;
    b.top = top + 'px';
    b.left = lft + 'px';
    b.width = '550px';
    b.height = '480px';
    b.overflow = 'scroll';
    b.background = 'linear-gradient(to bottom, #E2E2E2,#80A0C0)';
    b.border = '4px solid #7593b1';
    MRO.load_html('LSTHLP');
  }
  else MRO.display(b, 0);
}