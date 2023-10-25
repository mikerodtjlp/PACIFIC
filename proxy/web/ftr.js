var FTR={};
FTR._fns = (c, p0, p1) => {
  let f = FTR[c], r = typeof f === 'function';
  if (r) f(p0, p1);
  return r;
}
FTR.SSESINF = () => {
  let m = MRO, n = '\\n', d = DFS, s = m.ses, r = trans.saferes;
  let hasr = r !== null;
  let _=function(a,f){return a.concat(': ',s.get(f),n);}
  let c=[
    'session : ', s.get(d.ZSESINS), ':', s.get(d.ZSESMAC), ':',
                  s.get(d.ZSESCLI), ':', s.get(d.ZSESSES), n, 
    _('user    ',d.ZUSERID),_('winuser ',d.ZWINUSR),_('domain  ',d.ZWINDOM),n,
    _('ip      ',d.ZIPADDR),_('macaddr ',d.ZMACADR),_('macname ',d.ZMACNAM),n,
    _('domain  ', d.ZDOMAIN),
    _('company ', d.ZCOMPNY),
    'trans   : ',trans.name,n,n,
    'res time: ', hasr ? (r.getfloat(d.ZPRXTIM)+r.getfloat(d.ZSITTIM)).toString() : '',n,
    'pxy time: ', hasr ? r.get(d.ZPRXTIM) : '', n,
    'sit time: ', hasr ? r.get(d.ZSITTIM) : '', n, n,
    'res pool: ',parseInt(m.nrunfuns).toString(),n,
    'layout  : ',CUR.css,n,'language: ',CUR.lng,n,'compile : ',document.lastModified
  ].join('');
  FTR.popup(c);
}
FTR.SSTACK = () => { FTR.popup(MRO.stack.replace(/(\r\n|\n|\r)/gm, '\\n')); }
FTR.SERRINF = () => {
  let m = MRO, a = m.rpl_all(m.make(trans.saferes.get(DFS.ZERRORI)).buffer(),'}','\\n');
  a = m.rpl_all(a,'"','');
  a = m.rpl_all(a,'{','');
  //a = m.rpl_all(a, '}', '');
  a = m.rpl_all(a, ',error', '\\n\\nerror');
  FTR.popup(a);
}
FTR.SWRNGS = () => { FTR.popup(trans.warnings); }
FTR.popup = a => { MRO.show_dialog(DFS.ZTRNDLG, '{"utext":"' + a + '"}', new pairs(), 0, null); };
FTR.SHORTCUT = () => { MRO.show_dialog('S006', '{"$name$":"' + trans.name + '"}', new pairs(), 0, null, 'onenter'); };
FTR.CLNSTA = () => { MRO.status('', '', DFS.NORMAL); }
FTR.SHWCMP = () => {
  err.require(!MRO.clie.isactive(), DFS.EOPOUSE);
  MRO.show_dialog('S024', '', new pairs(), 2, null, 'onenter');
}
FTR.CHGCMP = p => {
  let m = MRO, newcmpy = p.get('id');
  m.ses.set(DFS.ZCOMPNY, newcmpy);
  m.reset_basics();
  BAR.CHGLIBCMPY(newcmpy);
  BAR.SESSETCMPY(newcmpy);
  m.load_cmpy();
  m.update_status();
  m.close_match();
  m.first_tree = true;
  if (m.are_in(DFS.ZTHOME))
    m.proc_tree(trans.tredat);
}