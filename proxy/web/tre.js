var TRE = {
tree: { state: [] }, // testing
    tree0: { state: [] }, tree1: { state: [] }, to: 0
};
TRE._fns = (c, p0, p1) => {
  let f = TRE[c], r = typeof f === 'function';
  if (r) f(p0, p1);
  return r;
}
TRE.goto = (code, act, prms) => {
  MRO.do_gototrans(MRO, MRO.make(
    '{"gototrans":"' + code +
    '","function":"' + act +
    '","params":' + prms + '}'))
  if (window.event) event.cancelBubble = true;
  //else e.stopPropagation();
  return false;
}
TRE.refresh = (a, p) => {
  if (p && p !== '') MRO.show_result(new pairs(p));
  MRO.send(a);
  if (window.event) event.cancelBubble = true;
  return false;
}
$3 = (c,f) => {
  let _={},elms=document.getElementById(c).getElementsByTagName("*");
  for (let i = 0, n = elms.length; i < n; ++i)
    if (elms[i].id === f) { _ = elms[i]; break; }
  return _;
}
TRE.get_state = a => TRE[a].state;
TRE.get_tree = a => { return MRO.are_in(DFS.ZTHOME) ? $3('dform', a) : $2(a); }
TRE.toggle = () => {
  let li = window.event.currentTarget;
  let ul = li.getElementsByTagName("ul")[0];
  if(ul){
    let img = li.getElementsByTagName("img");
    let i=img[0],f=img[1],a=CNS.imgs;
    let open = ul.style.display === "block";
    MRO.display(ul.style, open ? 0:1);
    if(f)f.src=a.concat(open?'sapfolderclose.png':'sapfolderopen.png');
    i.src=a.concat(open?'sapright.png':'sapdown.png');
    ul.open=!open;
  }
  if(window.event)event.cancelBubble=true;
  else e.stopPropagation();
  return false;
}
TRE.TREEIA = p => {
  let id = p.get('id');
  let mt = TRE.get_state(id), ul = TRE.get_tree(id).getElementsByTagName("ul");
  if(mt.length<1)return;//first no process
  for(let i=0,n=ul.length;i<n;++i)
  if(mt[i]){
    let u=ul[i];
    u.open=true;
    u.style.display="block";
  }
  $2('dform').scrollTop = TRE.to;
}
TRE.TREEIS = p => {
  let id = p.get('id');
  let u = TRE.get_tree(id).getElementsByTagName("ul");
  let n = u.length, mt = TRE.get_state(id);
  if (mt.length !== n) mt.length = n;
  for (let i=0; i<n; ++i) mt[i] = u[i].open;
  TRE.to = $2('dform').scrollTop;
}
TRE.TREEIE = (p,l) => {TRE.proc_tr(true,'block',p,l);}
TRE.TREEIC = (p,l) => {TRE.proc_tr(false,'none',p,l);}
TRE.proc_tr = (o, s, p, l) => {
  let ims=CNS.imgs;
  let spo=ims.concat('sapfolderopen.png');
  let sad=ims.concat('sapdown.png');
  let spc=ims.concat('sapfolderclose.png');
  let sar=ims.concat('sapright.png');
  let a = TRE.get_tree(p.get('id')).getElementsByTagName("ul");
  let n = a.length;
  let brk = l === undefined || l === null ? n : l;
  for(let i=0;i<n;++i){
    let u=a[i];
    u.open=o;
    u.style.display=s;
    let z=u.previousSibling; // img
    let b=z.previousSibling; // text
    let c=b.previousSibling; // folder
    let d=c.previousSibling; // open/close
    if(o){c.src=spo;d.src=sad;}
    c.src = spc;
    d.src = sar;
    if (i === brk) break;
  }
}
TRE.LODTRE = p => {
  err.require(!p, 'id_required');
  MRO.load_tree(p);
}
TRE.RTRE = p => {
  if (!p) {
    for (let i=0; i<trans.treemax; ++i)
      MRO.load_tree(MRO.make(trans.tredat._[i]), true);
    return;
  }
  //else MRO.load_tree(p, true);
}