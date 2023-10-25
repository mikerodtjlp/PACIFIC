var BAR = { shwmenu: false, trload: false };
BAR._fns = (c, p0, p1, p2, p3, p4) => {
  let f = BAR[c], r = typeof f === 'function';
  if (r) f(p0, p1, p2, p3, p4);
  return r;
}
//BAR.ENTER = () => { MRO.proc_enter(); }
BAR.LOAD = (p0, p1, p2) => { MRO.lmod(p0, (p1 !== undefined) ? p1 : true, p2, false); }
BAR.LJSC = (p0, p1, p2) => { MRO.lmod(p0, (p1 !== undefined) ? p1 : true, p2, true); }
BAR.RELOAD = (p0, p1) => {
  let h = document.getElementsByTagName('head')[0];
  let s = h.getElementsByTagName('script');
  let j = s[p0 + '.js'];
  if (j) j.parentNode.removeChild(j);
  BAR.LOAD(p0, p1, true);
}
BAR.CALL = (code, p0, p1) => { MRO.ins_trans(code, null, false); }
BAR.BACK = () => {
  if (MRO.clie.isactive() && !MRO.are_in(DFS.ZTHOME))
    MRO.gui('gui_go_back');
}
BAR.OUT = () => {
  let m = MRO, d = DFS;
  if (!m.clie.isactive()) return;
  m.send(d.ZONUNLD);
  m.end_session();
  m.first_screen();
  m.show_photo();
  m.set_user(new pairs());
  m.update_status();
  BAR.EASYACC(false);
  $2('tit').value = '';
  localStorage.setItem('loggedin', '0');
  localStorage.setItem('loggedin', '0');
}
BAR.F3 = () => {
  let v = MRO.cmdh.pop();
  $2('command').value = v ? v : '';
}
BAR.HOME = () => {
  if (MRO.clie.isactive() && !MRO.are_in(DFS.ZTHOME)) {
    MRO.gui('gui_go_home');
    BAR.EASYACC(false);
  }
}
BAR.FORWARD = () => { if (MRO.clie.isactive()) MRO.gui('gui_go_frwd'); }
BAR.NEWAPP = (code, extrap) => {
  let m = MRO, c = CUR, s = m.ses, d = DFS, f = m.fromclie;
  f.clear();
  m.dlgtype = -1;
  if (m.clie.isactive()) {
    f.set(d.ZDOMAIN, s.get(d.ZDOMAIN));
    f.set(d.ZCOMPNY, s.get(d.ZCOMPNY));
    f.set(d.ZUSERID, s.get(d.ZUSERID));
    f.set(d.ZORIPAS, s.get(d.ZORIPAS));
    f.set(d.ZLANGUA, c.lng);
    f.set(d.ZLAYOUT, c.css);
    f.set(d.ZEMPLID, s.get(d.ZEMPLID));
    f.set(d.ZMACADR, s.get(d.ZMACADR));
    f.set(d.ZIPADDR, s.get(d.ZIPADDR));

    f.set('zjsmodz', m.devld ? 'dev' : '');
    if (code && code.length > 0) {
      f.set(d.ZRTRNCD, code);

      // we covernt session variables if any // nqsh
      let p = m.make(extrap);
      let swap = false;
      for (let k in p._) {
        let v = p.get(k);
        if (s.hasval(v)) p.set(k, s.get(v)), swap = true;
      }
      if (swap) extrap = p._; // wrong!!! need to no take the object

      f.set(d.ZRTRNPR, extrap);
      f.on('specific');
    }

    let p = new pairs();
    p.set('sfatheri', s.get(d.ZSESINS));
    p.set('sfatherm', s.get(d.ZSESMAC));
    p.set('sfatherc', s.get(d.ZSESCLI));
    p.set('sfathers', s.get(d.ZSESSES));
    f.set('datacli', p.buffer());
    m.dlgtype = 9;
  }
  window.open(window.document.URL, '_blank', '');
}
BAR.HIS = () => {
  let p = '""', c = $2('command');
  if (c.value !== '') p = '{"hint":"'.concat(c.value, '"}');
  c.value = '';
  let a = c.getBoundingClientRect();
  MRO.gen_menucontext(1, p, a.left, a.bottom);
}
BAR.TASKS = () => {
  err.require(!MRO.clie.isactive(), DFS.EOPOUSE);
  MRO.show_dialog(DFS.ZUTASKS, '', new pairs(), 0, null, 'onenter');
}
BAR.do_design = r => {
  trans.mpopitems = r.get(DFS.ZFILERS);
  MRO.context_menu(r.getint('x'), r.getint('y'));
}
BAR.DESIGN = p => {
  let t = trans, pr = new pairs();
  if (t.lastfocus) t.lastfocus.SetFocus();
  t.hidepop = false;
  pr.set(DFS.ZTRNCOD, $2('fbin').value);
  pr.set('dsctrns', $2('titleid').value.substr(1));
  let e = window.event, ext = new pairs();
  ext.set('x', e.clientX);
  ext.set('y', e.clientY);
  MRO.clie.proxy('btn_design', pr.buffer(), true, BAR.do_design, ext);
}
BAR.RAPP = () => { window.location.reload(true); }
BAR.NCSS = p => {
  let m = MRO;
  m.ses.set(DFS.ZLAYOUT, p.get('id'));
  m.handle_css(false);
}
BAR.NLANG = p => {
  let m = MRO;
  m.ses.set(DFS.ZLANGUA, p.get('id'));
  m.handle_lang();
}
BAR.isResizing = false, BAR.lastDownX = 230 - 15;
BAR.right = null, BAR.left = null, BAR.handle = null;
BAR.move_elem = e => {
  if (!BAR.isResizing) return;//we don't want to do anything if we aren't resizing.
  let b = BAR, a = e.clientX;
  b.left.style.width = (a - 15) + "px";
  b.handle.style.left = (a + 8) + "px";
  b.right.style.left = (a + 15) + "px";
  b.lastDownX = a;//last position
}
BAR.EASYACC = a => {
  if (a === undefined && (!MRO.clie.isactive() || MRO.are_in(DFS.ZTHOME))) return;
  let b = BAR, d = window.document;
  //if(m.ismc||!st||st.value==='0')return;
  b.shwmenu = !b.shwmenu;
  if (a !== undefined) b.shwmenu = a;
  $2('dtree').style.display = b.shwmenu ? 'block' : 'none';
  $2('dform').style.left = b.shwmenu ? ((b.lastDownX + 15) + 'px') : '5px';

  if (!b.shwmenu) {
    d.removeEventListener("mousemove", BAR.move_elem);
    return;
  }
  else d.addEventListener("mousemove", BAR.move_elem);
  if (!b.right) b.right = d.getElementById("dform"); // every trans has its unique dform div

  if (b.trload) return;
  b.trload = true;
  b.left = d.getElementById("dtree");
  b.handle = d.createElement('div');
  b.handle.id = 'drag';
  b.left.parentNode.insertBefore(b.handle, b.left.nextSibling);
  b.handle.onmousedown = function (e) { b.isResizing = true; };
  document.onmouseup = function (e) { b.isResizing = false; }
}
BAR.HELP = () => { MRO.sd('help', trans.name); }
BAR.MAIL = p0 => { MRO.send_mail(p0); }
BAR.SHOWURL = p0 => { window.open(p0.get('address')); }
BAR.SHOWURL2 = p0 => {
  let u = '';
  for (let i = 0; i < 16; ++i)
    u += p0.get('url' + i + '');
  window.open(u);
}
BAR.DOWNLOAD = p0 => { MRO.download(p0); }
BAR.UPLOAD = p0 => { MRO.upload(p0); }
BAR.SES2VAR = p0 => { MRO.ses2var(p0); }
BAR.DSPCMD = () => { MRO.sd('help', 'boxcmds'); }
BAR.CPYRGT = (p0, p1) => {
  MRO.check_resp(MRO.clie.proxy('cpyrgt', MRO.make2('from', p0, 'to', p1).buffer()));
}
BAR.CHGLIBGRP = p0 => { // specific lib
  MRO.check_resp(MRO.clie.proxy('chglibgrp', MRO.make1('libgrp', p0).buffer()));
}
BAR.CHGLIBCMPY = p0 => { // default compy lib
  MRO.check_resp(MRO.clie.proxy('chglibcmpy', MRO.make1('cmpy', p0).buffer()));
}
BAR.SESSETCMPY = p0 => { // memory
  MRO.check_resp(MRO.clie.proxy('sessetcmpy', MRO.make1('cmpy', p0).buffer()));
}
BAR.ADDLIBLE = p0 => {
  MRO.check_resp(MRO.clie.proxy('addlibentry', MRO.make1('name', p0).buffer()));
}
BAR.DLTLIBL = () => { MRO.clie.proxy('delliblist', '""'); }
BAR.EDTLIBL = () => {
  MRO.ins_trans(DFS.ZTUPLIB, '', false);
  MRO.send(DFS.ZONENTR);
}
BAR.ADDFAV = () => {
  let m = MRO, c = m.ses; d = DFS, code = trans.name;
  err.require(!m.clie.isactive(), d.EOPOUSE);
  m.clie.site('core', '', 'execute_query', '{"query":"embedded","sqltext":"'.concat(
    "exec CORE.dbo.fav_add '", c.get(d.ZCOMPNY), "','", c.get(d.ZUSERID), "','",
    code, "','", code, "','','',0;", '"}'), true);
}
BAR.VAR2CLIP = p => {
  let v = p.get('id');
  if (v === '') return;
  if (v.charAt(0) === '^') v = v.substr(1);
  let t = $2(v);
  if (t !== null) window.prompt("Ctrl+C,Enter", t.value);
}
BAR.RUN = (p0, p1, p2, p3, p4) => {
  p0 = p0 === undefined ? '' : p0;
  p1 = p1 === undefined ? '' : p1;
  p2 = p2 === undefined ? '' : p2;
  p3 = p3 === undefined ? '' : p3;
  p4 = p4 === undefined ? '' : p4;
  let prms = '{"type":"PGM","name":"' + p0 + '","p0":"' + p1 + '","p1":"' + p2 +
                                             '","p2":"' + p3 + '","p3":"' + p4 +'"}';
  MRO.check_resp(MRO.clie.proxy('RUN', prms));
}
MRO.enter_system = () => {
  let m = MRO, s = m.ses, d = DFS, r = trans.saferes;
  let l = localStorage;
  l.setItem('lstcmpy', $2('$company$').value);
  l.setItem('lstuser', $2('$_user$').value);
  l.setItem('lstlang', $2('$_lang$').value);
  l.setItem('lstlayo', $2('$layout').value);

  m.clie.start_session(r);
  m.check_resp(r);
  if (PRM.shortcut.length > 0) {
    m.ins_trans(PRM.shortcut, PRM.scprms, false);
    let sp = m.make(PRM.scprms);
    p = sp.get(d.ZDOEVNT);
    if (p.length > 0) m.send(p);
    PRM.shortcut = '';
    PRM.scprms = '';
  }
  else m.restore_state(r, null);
  m.show_photo();
  m.set_user(r);
  m.update_status();
  m.clie.gen_bas(m.basics);
  m.set_lastdata();
  m.load_cmpy();
  m.show_divs();

  l.setItem('loggedin', '1');
  l.setItem(d.ZSESINS, s.get(d.ZSESINS));
  l.setItem(d.ZSESMAC, s.get(d.ZSESMAC));
  l.setItem(d.ZSESCLI, s.get(d.ZSESCLI));
  l.setItem(d.ZSESSES, s.get(d.ZSESSES));
  l.setItem(d.ZINSTAN, s.get(d.ZINSTAN));
}