var ERR = {};
ERR.proc_color = (b) => {
  let m = MRO, d = DFS, c = b ? b.style : null, s = m.lststy;
  if (c) {
    m.lstinp = b, s[0] = c.background, s[1] = c.color;
    c.background = d.STABKCOLS[d.ERROR], c.color = d.STACOLORS[d.ERROR];
  }
}
ERR.proc_onerror=(r,ev)=>{
  let m=MRO,d=DFS,a=r.get('zincfld');
  if (a !== '') ERR.proc_color($2(a));
  if(trans.json.has(ev))ERR.clean_onerror(ev);
  m.check_resp(r);
}
ERR.clean_onerror=(ev)=>{
    let j=MRO.make(trans.json.get(ev));
    if(!j.has('onerror'))return;
    let onerror=MRO.make(j.get('onerror'));
    let lid=onerror.getint('clrlst',-1);
    if(lid>=0&&lid<4&&trans.listmax>0)MRO._fill_list(lid,new pairs());
}
ERR.get_desc = t => {
  let r = MRO.clie.site('core', '', 'execute_query', '{"query":"embedded","sqltext":"'.concat(
    "exec CORE.dbo.desc_get '", t, "','", CUR.lng, "';", '"}'));
  let q = r.get('l00A');
  return q !== '' ? q : t;
}
