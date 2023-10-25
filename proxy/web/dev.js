var DEV={savedw:false};
DEV._fns = (c, p0, p1) => {
  let f = DEV[c], r = typeof f === 'function';
  if (r) f(p0, p1);
  return r;
}
DEV.set_value = v => {
  editor.setValue(v);
  editor.resize(true);
  editor.scrollToLine(0, true, true, function () {});
  editor.gotoLine(0, 10, true);
}
DEV.edisetrng = r => {editor.selection.setSelectionRange(r);}
DEV.FNDSTR = p0 => {
  let s = p0.get('str');
  let r = editor.find(s);
  if(r) DEV.edisetrng(r);
}
DEV.EVAL = p => { MRO.eval_js(p); }
DEV.FNDNXT = () => {let r=editor.findNext();if(r)DEV.edisetrng(r);}
DEV.FNDPRV = () => {let r=editor.findPrevious();if(r)DEV.edisetrng(r);}
DEV.RQRY = pms => {DEV.rqry(pms);}
DEV.SURL = () => { DEV.dw(MRO.clie.url + '&' + trans.sdata);}
DEV.JLNT = () => {window.open('https://jsonlint.com/');}
DEV.DIFF = () => {window.open('https://www.diffchecker.com/diff');}
DEV.SELOG = () => {DEV.show_log(0);}
DEV.EDIT = p => { DEV.edit_(false,p); }
DEV.EDITN = p => { DEV.edit_(true,p); }
DEV.GTED = p => { DEV.goto_edit(p); }
DEV.DEBUG = () => {MRO.debug = !MRO.debug;}
DEV.SVER = () => {DEV.saveer=!DEV.saveer;}
DEV.SVDW = () => {DEV.savedw=!DEV.savedw;}
DEV.LOADM = () => { MRO.load_menu(trans.menuicode, false); }
DEV.RLOADM = () => { MRO.load_menu(trans.menuicode, true); }
DEV.dw = s => { window.open().document.write(s);}
DEV.RQRY = pms => { // reset on pacific site
  let m = MRO, p = m.make('');
  if (pms !== '') p.set('document',pms);
  m.clie.site(trans.defmod, '', 'reset_document_cache', p.buffer(), true);
}
DEV.RDOC = (p0, p1) => { // reset on atlantic server
  let m = MRO, a = p0.split('/');
  err.requireex(a.length < 1 || a.length > 2, 'incorrect_input', p0);
  let lib = a.length === 2 ? a[0]:'';
  let prg = a.length === 2 ? a[1] : a[0];

  let p = m.make_fileprms('trans', prg, p1, 'force');
  p.set(DFS.LIBRARY, lib);
  m.clie.proxy('get_file', p.buffer(), true);
}
DEV.COMPILE = () => {DEV.RFUNS();DEV.RPAGE();}
DEV.SUSR = () => {
  let m=MRO,d=DFS,c=['{"utext":"'],un=m.ses.get(d.ZUSERID);
  if(un==='')return;
  let r = m.clie.kernel('get_user_logons','"'.concat(d.ZUSERID,'":"',un,'"'));
  for(let i=0,nr=r.getint('zl0rows');i<nr;++i)
    c.push('Machine:'.concat(r.get('l0'+i+'A'),', Since:',r.get('l0'+i+'B'),'\\n'));
  c.push('"}');
  m.show_dialog(d.ZTRNDLG,c.join(''),new pairs(),0,null);
}
DEV.RFUNS = () => {MRO.send(DFS.ZONRELD);}
DEV.RPAGE = () => {
  try {
    let p = MRO.make_fileprms('trans', trans.name, 'TRN', 'force');
    MRO.clie.proxy('get_file', p.buffer(), true, DEV.proc_rpage);
  }
  catch (e) { err.rescue(e) };
}
DEV.proc_rpage = p => {
  try {
    MRO.check_resp(p);
    let html = p.get(DFS.ZFILERS);
    let v = MRO.gen_values(false);
    MRO.move2trans(trans.library, trans.name, v.buffer(), false, html); // no force already done
  }
  catch (e) { err.rescue(e) };
}
DEV.RCSS = () => {
  let m=MRO;
  CUR.css='';
  m.handle_css(true);
  let v = m.gen_values(false);
  m.move2trans(trans.library, trans.name, v.buffer(), true, '');
  m.load_pos_css('cssEXT', true);
  m.load_pos_css('cssEXM', true);
}
DEV.SCSS = () => {DEV.strn(MRO.ses.get(DFS.ZFILERS),1);}
DEV.STRN = () => {DEV.strn($2('dform').innerHTML,0);}
DEV.SJSC = () => {DEV.strn(JSON.stringify(trans.json._,null,'\t'),1);}
DEV.SJSR = () => {DEV.strn(trans.json_raw,1);}
DEV.SJSE = () => {DEV.strn(MRO._err,1);}
DEV.SJST = () => {DEV.strn(JSON.stringify(trans.jctrls._,null,'\t'),1);}
DEV.SRES = () => {DEV.strn(trans.saferes.buffer(),1);}
DEV.SSES = () => {DEV.strn(MRO.ses.buffer(),1);}
DEV.SBAS = () => {DEV.strn(MRO.clie.gen_basics(''),1);}
DEV.SMEN = () => {DEV.strn($2('menu').innerHTML,0);}
DEV.SMOD = () => {let a=MRO.lmods,b='';for(let i in a)b+=a[i]+'\n';DEV.strn(b,1);}
DEV.SCMD = p0 => {
  err.requireex(p0 === undefined || p0 === '', 'missing','library');
  let n = window[p0];
  err.requireex(n === undefined || n === null, 'obj_not_exist', p0);
  if (n !== undefined) {
    let r = new RegExp("^[A-Z0-9]*$");
    let a = Object.getOwnPropertyNames(n), b = '',c='';
    for (let i in a) {
      c = a[i];
      if (r.test(c)) b += c + '\n';
    }
    DEV.strn(b, 1);
  }
}
DEV.strn = (a, t) => {
  let s='';
  if(t===0)s="<html>".concat(MRO.rpl_all(a,'>','>\n'),"</html>");
  if(t===1){s=MRO.rpl_all(a,'{','{\n');s=MRO.rpl_all(s,'}','}\n');s=MRO.rpl_all(s,'",','",\n');}
  s=s.replace(/</g, "&lt;").replace(/>/g, "&gt;");
  s="<pre>".concat(s,"</pre>");
  let w=window.open('','Pacific','height=800,width=800,scrollbars=1,resizable=1');
  w.document.write(s);
  w.document.close();
  if(window.focus)w.focus();
}
DEV.LODEDT = () => {
  let d=window.document;
  let n=d.getElementById('acescript');
  if(n!==null)n.parentElement.removeChild(n);
  let t='var editor=ace.edit("$text$");'.concat(
    'editor.setTheme("ace/theme/twilight");',
    'editor.getSession().setMode("ace/mode/javascript");',
    'editor.session.setOptions({tabSize:2,useSoftTabs:true});');
  let s=d.createElement('script');
  s.id='acescript';
  s.appendChild(d.createTextNode(t));
  d.getElementsByTagName('head')[0].appendChild(s);

  MRO.lmod('txt', true);
}
DEV.edit_ = (n, p) => {
  let a = MRO.make(p);
  if (!a.has('$file$')) { // empty comes editn alone
    a.set('$lib$', trans.library);
    a.set('$file$', trans.name); 
    a.set('$type$', 'TRN');
  }
  DEV.launch_edit(a, n);
}
DEV.launch_edit = (data, n) => {
  let m=MRO,d=DFS;
  if(!n){
    m.ins_trans(d.ZTRNDEV,data.buffer(),false);
    m.send(d.ZONENTR);
    return;
  }
  data.set(d.ZDOEVNT,d.ZONENTR);
  execmd('newapp',d.ZTRNDEV,data.buffer());
}
DEV.goto_edit = n => {
  let l = $2('$lib$');
  let e = $2('$text$');
  let k = $2('$package$');
  if(!e||!editor)return;
  let p = '{"$lib$":"' + l.value + '","$file$":"' + editor.getCopyText() +
    '","$package$":"'+ k.value + '"}';
  if(n==='1')execmd("editn",p);
  else execmd("edit",p);
}
DEV.SJSF = () => {
  let b = '';
  for(let i=0,s=document.getElementsByTagName('script');i<s.length;++i)
      b+=s[i].id+'<br/>';
  DEV.dw(b);
}
DEV.SQRY = () => {
  let a = $2('onsu');
  err.requireex(!a.checked, 'support', 'not_activated');
  err.requireex(trans.saferes === null, 'need','result');
  a='',b=trans.saferes;
  let n=b.get('nqryret');
  if(n>0){
    a+='nqryret:'+n+'\n';
    for(let i=0;i<n;++i){
      let k='qryret'+i;
      a+=k.concat(':',b.get(k),'\n');
    }
    DEV.strn(a,1);
  }
}
DEV.proc_fndlib = r => {
  let m = MRO;
  try {
    m.check_resp(r);
    let v = r.get('l00A');
    if (v === '') m.status(r.get('trans') + ' not found', '', DFS.ERROR);
    else m.status('found at:' + v, '', DFS.OK);
  }
  catch (e) { err.rescue(e) };
}
DEV.FNDLIB = t => {
  if (!t || t.length === 0) return;
  let ext = MRO.make1('trans', t);
  let m = MRO, r = m.clie.proxy(
    'execute_query', '{"query":"embedded","sqltext":"'.concat(
      "exec DEV.dbo.findlib_tcode '", t, "';", '"}'), true, DEV.proc_fndlib, ext);
}
DEV.proc_fndpkg = r => {
  let m = MRO;
  try {
    m.check_resp(r);
    let v = r.get('l00A');
    if (v === '') m.status(r.get('trans') + ' not found', '', DFS.ERROR);
    else m.status('found at:' + v, '', DFS.OK);
  }
  catch (e) { err.rescue(e) };
}
DEV.FNDPKG = t => {
  if (!t || t.length === 0) return;
  let ext = MRO.make1('trans', t);
  let m=MRO,r=m.clie.proxy(
    'execute_query','{"query":"embedded","sqltext":"'.concat(
      "exec DEV.dbo.findpack_tcode '", t, "';", '"}'), true, DEV.proc_fndpkg, ext );
}
DEV.show_log = t => {
  let m = MRO, a = t ===0 ? m.errlog : m.dnllog;
  let n = a.length, c = ['{"utext":"'];
  for (let i = 0; i < n; ++i) c.push(a[i].concat('\\n'));
  c.push('"}');
  m.show_dialog(DFS.ZTRNDLG, c.join(''), null, 0, null);
}
DEV.CLNTRN = p => {
  let a = MRO.make(p);
  a.set('$libsrc$', trans.library);
  a.set('$trans$', trans.name);
  MRO.ins_trans('ST00', a.buffer(), false);
  MRO.send(DFS.ZONENTR);
}
DEV.DEVCON = () => {
  err.require(!MRO.clie.isactive(), DFS.EOPOUSE);
  MRO.show_dialog('SE04', '', new pairs(), 2);
};
(function(){
  MRO.devld=true;
  let a=$2('devjs');
  if(a!==null)return;
  let d=window.document;
  let l=d.createElement('img');
  l.id='devjs';
  l.src = CNS.imgs + 'settings.png';
  l.style.cursor = 'pointer';
  l.onclick = () => { DEV.DEVCON(); };
  $2('tbox').appendChild(l);
  // load ace editor libraries
  if (typeof ace === 'undefined') {
    let a = '/web/src-min/'
    MRO.load(a,'ace.js', false);
    MRO.load(a,'theme-twilight.js', false);
    MRO.load(a,'mode-javascript.js', false);
  }
})();
