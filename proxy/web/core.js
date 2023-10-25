var MRO = {
  createjs: function (jsnm, jstx) {
    let d = window.document;
    let h = d.getElementsByTagName("head")[0];
    let s = d.createElement('script');
    s.id = jsnm;
    s.type = "text/javascript";
    s.text = jstx;
    h.appendChild(s);
  },
  load: function (fld, entry, asy) {
    let stg = localStorage;
    let ver = stg.getItem(entry + '.ver.new');
    let dif = true;
    if (ver && ver !== '') { 
      dif = stg.getItem(entry + '.ver') !== ver;
      if (!dif) { 
        let res = stg.getItem(entry);
        if (res === null || res === '') dif = true;
        else MRO.createjs(entry, res);
      }
    }
    else { MRO.loadX(fld, entry, asy); return; }

    if (dif) { 
      stg.setItem(entry + '.ver', ver);
      if (asy) { // asynchrouns
        let req = new Request(fld + entry, {
          method: 'get',
          mode: 'cors',
          redirect: 'follow',
          headers: { "Content-type": "application/x-www-form-urlencoded; charset=UTF-8" }
        });
        fetch(req).then(response => response.text()).then(res => {
          stg.setItem(entry, res);
          MRO.createjs(entry, res);
        });
      }
      else { // synchronous
        let req = new XMLHttpRequest();
        req.open('GET', fld + entry, false);
        req.send('');
        stg.setItem(entry, req.responseText);
        MRO.createjs(entry, req.responseText);
      }
    }
  },
  loadX: function (p, f, a) {
    let d = window.document;
    let h = d.getElementsByTagName("head")[0];
    let s = d.createElement('script');
    s.id = f;
    if (a) {
      s.setAttribute("type", "text/javascript");
      s.setAttribute("src", p + f);
      s.defer = true;
    } else {
      let x = new XMLHttpRequest();
      x.open('GET', p + f, false);
      x.send('');
      s.type = "text/javascript";
      s.text = x.responseText;
    } h.appendChild(s);
  }
};
var LOC = {};
function escapeRegExp(string) { return string.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1"); }
function $2(i) { return window.document.getElementById(i); }
function $4(a, b) { return a.indexOf(b) !== -1; }
function pairs(v) { if (v === undefined) this._ = JSON.parse('{}'); else this.set_value(v); }
pairs.prototype = {
  get: function (k) { return this._[k] ? this._[k] : ''; },
  getint: function (k, d) { return this._[k] ? parseInt(this._[k]) : (typeof d !== 'undefined' ? d : 0); },
  getfloat: function (k, d) { return this._[k] ? parseFloat(this._[k]) : (typeof d !== 'undefined' ? d : 0); },
  getbool: function (k) { return this._[k] ? this._[k] === '1' : false; },
  set: function (k, v) { this._[k] = v ? v : ''; },
  clear: function () { this._ = JSON.parse('{}'); },
  del: function (k) { if (this._[k] !== null) delete this._[k]; },
  herr: function (e, a) {
    MRO._err = a;
    alert('JSON Error');
    if (e !== null) console.log(e);
    console.log(a);
  },
  set_value: function (v) {
    if (v) {
      try { this._ = JSON.parse(v); }
      catch (e) { this.herr(e, v); }
    }
    else this._ = JSON.parse('{}');
  },
  set_simple: function (v) {
    try { this._ = JSON.parse(v);}
    catch (e) {this.herr(e, v); }
  },
  get_data: function () { return this._ ? this._ : JSON.parse('{}'); },
  attach: function (v) {
    if (!v) JSON.parse('{}');
    if (typeof (v) === 'string') this.set_value(v);
    else {
      let b = v.constructor();
      for (let a in v) if (v.hasOwnProperty(a)) b[a] = v[a];
      this._ = b;
    }
  },
  has: function (k) { return ('undefined' !== typeof this._[k]); },
  hasval: function (k) {
    let v = this._[k]; if (!v) return false;
    if (typeof (v) === 'string') return v.length > 0;
    return true;
  },
  extract: function (k) { let r = this.get(k); this.del(k); return r; },
  isempty: function () { return Object.keys(this._).length === 0; },
  notempty: function () { return Object.keys(this._).length !== 0; },
  on: function (k) { this._[k] = '1'; },
  ison: function (k) { return this._[k] === '1'; },
  are_eq: function (k, v) { return this.get(k).localeCompare(v) === 0; },
  append: function (v) { if (v._) for (let i in v._) this._[i] = v.get(i); },
  nkeys: function () { return Object.keys(this._).length; },
  getjson: function () { return JSON.stringify(this._); },
  tojson: function () {
    let s = JSON.stringify(this._);
    let l = s.length;
    return l === 2 ? '' : s.slice(1, l - 1);
  },
  buffer: function () { return JSON.stringify(this._); },
  rpl_from: function (v) { if (!v) return; for (let i in v._) this.set(i, v.get(i)); },
  merge: function (s, o) {
    if (!s || !o) return;
    for (let k in s._) {
      let v = s.get(k);
      this.set(k, o.has(v) ? o.get(v) : v);
    }
  }
}
const DFS = {
  ZTRNPAS: 'S000', ZTHOME: 'S001', ZSEMAIL: 'S007', ZTRNFLI: 'S010', ZTUPLIB: 'S011', ZTRNFIL: 'S014',
  ZTRNDLG: 'S015', ZTCHFLD: 'S017', ZTJOBBT: 'S018', ZTRNDEV: 'SE03', ZTRNSQL: 'SE14', ZUTASKS: 'STS0',
  ZSLSHLP: 'SLH0',
  ZDATTIM: 'datetim', ZUSERID: 'zzzuser', ZWINUSR: 'zwinusr', ZWINDOM: 'zwindom', ZEMPLID: 'zemplid',
  ZLANGUA: 'p_langu', ZLAYOUT: 'zlayout', ZSESINS: 'zsesins', ZSESMAC: 'zsesmac', ZSESCLI: 'zsescli',
  ZSESSES: 'zsesses', ZINSTAN: 'instanc', ZIPADDR: 'ipaddre', ZMACADR: 'macaddr', ZMACNAM: 'macname',
  LIBRARY: 'library', ZDOMAIN: 'aserver', ZCOMPNY: 'zcompny', ZAPIKEY: 'ZAPIKEY', ZGATSVR: 'gat_svr', 
  ZWEBSVR: 'web_svr', ZIISPRT: 'iisport', ZGUIAGN: 'ZGUIAGN', ZBRWTYP: 'ZBRWTYP', ZCLIVER: 'version',
  ZPRXTIM: 'prxtime', ZSITTIM: 'sittime', ZMODULE: 'wmodule', RETPRMS: 'retprms', ZKEYPRE: 'ZKEYPRE',
  ZLSTAFF: 'listaff', ZGOTODO: 'zgotodo', ZPSTMSG: 'ZPSTMSG', ZPSTMSI: 'ZPSTMSI', ZLOCACT: 'zlocact',
  ZZNFUNS: 'zznfuns', ZORIPAS: 'oripass', ZRTRNCD: 'rmodule', ZFILERS: 'xfile01', PDOCTYP: 'doctype',
  ZFILE01: 'file_01', ZFOLDER: 'folder', ZUPDCLS: 'updclis', ZRESEND: 'zresend', ZDSPTCH: 'ZDSPTCH',
  ZDSPPAG: 'ZDSPPAG', ZORIVAL: 'zorival', ZPROXYP: 'proxy.ashx', ZLSTRES: 'lastres', ZFILE: 'file',
  ZUSRPRM: 'zusrprm', ZHISPOS: 'zhispos', ZEVENTN: 'zeventn', ZCURFLD: 'zcurfld', ZTRNCOD: 'modcode',
  ZSETCUR: 'zsetcur', ZDLGTYP: 'zdlgtyp', /*ZSHWDLG: 'ZSHWDLG',*/ ZONUNLD: 'onunload', ZONENTR: 'onenter',
  ZDOEVNT: 'doevent', ZONLOAD: 'onload', ZONRELD: 'onreload', ZSESTIM: 'zsestim', ZTYPRGT: 'tyright',
  ZRTRNPR: 'rmodprm', ZTYPTRN: 'typtran', ZTYPRED: 'typread', ZSWARNG: 'warning', ZSTATUS: 'zstatus',
  ZERRORI: 'zerrori', ZERRORM: 'zierror', ZERRORS: 'zlerror', ZSERROR: 'error01', ZCERROR: 'errorc1',
  ZNERROR: 'errorn1', ZHERROR: 'errorh1', ZNERRIN: 'errorin', ZNERRLO: 'errorln', ZIERROR: 'errori1',
  ZLERROR: 'errorl1', ZDOWNLD: 'ZISDWLD', ZNDOWNS: 'ZNDWLDS', ZDWNFSV: 'ZDWLFS0', ZDWNTYP: 'ZDWLTY0',
  ZDWNFPA: 'ZDWLFP0', ZDWNFFL: 'ZDWLFF0', ZDWNTFL: 'ZDWLTF0', ZDWNTPA: 'ZDWLTP0', ZDWNDIR: 'ZDWLDI0',
  ZDWNWIT: 'ZDWLWI0', ZHISSIZ: 'zhissiz', ZHISTRN: 'zhistrn', ZHISDSC: 'zhisdsc', ZHISTYP: 'zhistyp',
  ESESLKU: 'session_reset_lack_use', EOPOUSE: 'oper_out_of_session',
  NONMENU: '<div class="menuBar"style="width:100%;"><a class="menuButton"onclick="execmd(\'help\');">Help</a></div>',
  NORMAL: 0, OK: 1, ERROR: 2, PROCESS: 3, WARNING: 4,
  STABKCOLS: ['#C8C8B6', '#7BAB9A', '#CE6060', '#80A0D0', '#E0E080'],
  STACOLORS: ['#FFFFFF', '#FFFFFF', '#FFFFFF', '#FFFFFF', '#323232']
}
MRO.custcss = $2('customcss') !== null;
MRO.distyp = ['none', 'block', 'inline'];
MRO.make = obj => { let o = new pairs(); o.attach(obj); return o; }
MRO.make1 = (k, v) => { let o = new pairs(); o.set(k, v); return o; }
MRO.make2 = (k0, v0, k1, v1) => { let o = MRO.make1(k0, v0); o.set(k1, v1); return o; }
MRO.isArray = what => Object.prototype.toString.call(what) === '[object Array]';
MRO.del_str = (s, i, f) => { let r = s.slice(0, i); return r += s.slice(f); }
MRO.$undef = a => a === undefined;
MRO.rpl_all = (s, f, r) => s.replace(new RegExp(escapeRegExp(f), 'g'), r);
MRO.chk_inp = (v, rx) => new RegExp(rx).test(v);
MRO.chk_inp_def = v => true; /*new RegExp("^[a-zA-Z0-9 _\/]*$").test(v);*/ //}
MRO.chk_inp_pass = v => new RegExp("^[a-zA-Z0-9\/\*]*$").test(v);
MRO.chk_inp_date = v => new RegExp("([12]\d{3}\/(0[1-9]|1[0-2])\/(0[1-9]|[12]\d|3[01]))*$").test(v);
MRO.chk_inp_dh = v => true;
MRO.display = (a, t) => { a.display = MRO.distyp[t]; }
MRO.are_in = (...args) => {
  let code = trans.name, cdln = code.length;
  for (let i = 0, n = args.length; i < n; ++i) {
    let a = args[i];
    if (cdln === a.length && a === code) return true;
  }
  return false;
}
let MEM = {};
MEM.join3 = (a, b, c) => a.concat(b, c);
MEM.join5 = (a, b, c, d, e) => a.concat(b, c, d, e);
let PCF = {};
PCF.find = (r, i) => {
  let t1 = null, t2 = null;
  let j = r.indexOf('"xfile02":', i + 11);
  let f = r.indexOf('</html>', i + 11);
  if (j !== -1 && f !== -1 && f > j) f = -1;
  if (f !== -1) {
    t1 = r.slice(i + 11, f + 7);
    f = r.indexOf('",', f + 7);
    r = MRO.del_str(r, i, f + 2);
  }
  else {
    f = r.indexOf('","', i + 11);
    if (f !== -1) {
      t1 = r.slice(i + 11, f);
      r = MRO.del_str(r, i, f + 2);
    }
  }
  if (j === -1) return [r, t1, t2];
  i = r.indexOf('"xfile02":');
  if (i !== -1) {
    let f = r.indexOf('</html>', i + 11);
    if (f !== -1) {
      t2 = r.slice(i + 11, f + 7);
      f = r.indexOf('",', f + 7);
      r = MRO.del_str(r, i, f + 2);
    }
    else {
      f = r.indexOf('","', i + 11);
      if (f !== -1) {
        t2 = r.slice(i + 11, f);
        r = MRO.del_str(r, i, f + 2);
      }
    }
  }
  return [r, t1, t2];
}
PCF.proc = (r, res, retdat) => {
  let t1 = null, t2 = null;
  let i = r.indexOf('"xfile01":');
  if (i !== -1) {
    let a = PCF.find(r, i);
    r = a[0], t1 = a[1], t2 = a[2];
  }
  if (r.charAt(0) === '"') r = '{'.concat(r, '}');
  res.set_simple(r);
  if (t1) res.set(DFS.ZFILERS, t1);
  if (t2) res.set('xfile02', t2);
  if (retdat !== undefined) res.append(retdat);
  return res;
}
PCF.post3sync = async (req, res, cb, ext) => {
  let response = await fetch(req);
  let r = await response.text();
  PCF.proc(r, res, ext);
  return res;
}
PCF.post2 = (url, data, async, cb, retdat) => {
  if (!async) return PCF.post2old(url, data, async, cb, retdat);
  let res = new pairs();
  let req = new Request(url, {
    method: 'post',
    mode: 'cors',
    redirect: 'follow',
    headers: { "Content-type": "application/x-www-form-urlencoded; charset=UTF-8" },
    body: data
  });
  //if (!async) return await PCF.post3sync(req, res, cb, ext);
  fetch(req)
    .then(response => response.text()).then(r => {
      PCF.proc(r, res, retdat);
      if (cb === undefined) MRO.handle_response(res); else cb(res);
    });
}
PCF.post2old = (url, data, async, cb, retdat) => {
  let res = new pairs(), ws = new XMLHttpRequest();
  ws.onreadystatechange = function () {
    if (ws.readyState === 4) {
      if (ws.status === 200) {
        let r = ws.responseText;
        ws = null;
        PCF.proc(r, res, retdat);
        if (async) if (cb === undefined) MRO.handle_response(res); else cb(res);
      }
    }
  };
  ws.open("POST", url, async);
  ws.setRequestHeader("Content-type", "application/x-www-form-urlencoded");
  ws.send(data);
  if (async) return null;
  return res;
}
function terminal() { this.url; this.bur; }
terminal.prototype = {
  isactive: function () { return MRO.ses.hasval(DFS.ZSESINS); },
  start_session: function (r) {
    let d = DFS;
    if (r.hasval(d.ZSERROR)) return;
    let s = MRO.ses;
    s.set(d.ZSESINS, r.get(d.ZSESINS));
    s.set(d.ZSESMAC, r.get(d.ZSESMAC));
    s.set(d.ZSESCLI, r.get(d.ZSESCLI));
    s.set(d.ZSESSES, r.get(d.ZSESSES));
    s.set(d.ZINSTAN, r.get(d.ZINSTAN));
  },
  gen_header: function (rr, rq) {
    return '{"priority":"5","retresult":"'.concat(rr,'","retqry":"', rq, '"}');
  },
  gen_bas: function (b) {
    let s = MRO.ses, d = DFS;
    b.push('{"'.concat(
      d.ZIPADDR, '":"', s.get(d.ZIPADDR), '","', d.ZMACNAM, '":"', s.get(d.ZMACNAM), '","',
      d.ZEMPLID, '":"', s.get(d.ZEMPLID), '","', d.ZUSERID, '":"', s.get(d.ZUSERID), '",'));
    b.push('"'.concat(
      d.ZSESINS, '":"', s.get(d.ZSESINS), '","', d.ZSESMAC, '":"', s.get(d.ZSESMAC), '","',
      d.ZSESCLI, '":"', s.get(d.ZSESCLI), '","', d.ZSESSES, '":"', s.get(d.ZSESSES), '",'));
    b.push('"'.concat(
      d.ZDOMAIN, '":"', s.get(d.ZDOMAIN), '","', d.ZCOMPNY, '":"', s.get(d.ZCOMPNY), '",'));
  },
  gen_basics: function (ex) {
    let m = MRO, d = DFS, b = [];
    if (m.basics.length === 0) this.gen_bas(b);
    else b = m.basics.slice(0);

    b.push('"'.concat(d.ZTRNCOD, '":"', trans.name, '","', d.ZLANGUA,
      '":"', CUR.lng, '","', d.ZAPIKEY, '":"', m.certgen(), '"'));

    if (ex && ex.length > 0) b.push(','.concat(ex, '}}'));
    else b.push('}');
    return b.join("");
  },
  // most basics webservice calling rarely used
  rawcall: function (rawdata, async, callback, retdat) {
    return PCF.post2(this.url, rawdata, async, callback, retdat);
  },
  // executes kernel C++ functions in proxy none business logic
  kernel: function (fun, p0, async, callback, retdat) {
    let p = new pairs(p0);
    p.set('funcall', fun);
    p.set('sitcall', 'gat');
    return this.proxy('atlcall', p.buffer(), async, callback, retdat);
  },
  // executes core functions in proxy, none business logic
  proxy: function (fun, p, async, callback, retdat) {
    let a = async === undefined ? false : async
    let d = 'fun=1&mac='.concat(MAC.name, '&bas=', this.gen_basics(''), '&');
    let c = 'prm={"zvalues":{"fun0":"'.concat(fun, '","prms0":', p, '}}');
    d += c.replace('?', '*');
    return PCF.post2(this.url, d, a, callback, retdat);
  },
  // executes core functions in node, none business logic
  site: function (node, page, fun, prms, async, callback, retdat) {
    let a = async === undefined ? false : async
    let post = 'server='.concat(node, '&page=', page === '' ? 'core.aspx' : page,
      '&fun=', fun, '&mac=', MAC.name, '&bas=', MRO.clie.gen_basics(''), '&');
    let c = 'prm={"zvalues":'.concat(prms, '}');
    post += c.replace('?', '*');
    return PCF.post2(this.url, post, a, callback, retdat);
  },
  // executes by event not by function in node, mostly business logic
  service: function (node, page, header, basics, evnt, values, async, callback, retdat) {
    let a = async === undefined ? false : async
    trans.sdata = 'server='.concat(node, '&page=', page, '&mac=', MAC.name,
      '&hdr=', header, '&bas=', basics, '&');
    let c = 'prm={'.concat(evnt, ',', values, '}');
    trans.sdata += c.replace('?', '*');
    return PCF.post2(this.url, trans.sdata, a, callback, retdat);
  }
}
let trans = {
  library: '',
  name: '',
  lay: null,
  json: new pairs(),
  json_raw: '',
  jctrls: new pairs(),
  saferes: null,
  lastres: null,
  menuicode: '',
  menuitems: '',
  mpopitems: '',
  mtrnitems: '',
  defmod: '',
  defcbh: '',
  tredat: '',
  hidepop: true,
  qrylayout: null,
  l_res: null,
  l_edts: null,
  l_adds: null,
  l_adbs: null,
  l_imgs: null,
  l_csrw: null,//css rows
  l_csus: null,//css user list
  l_ronlies: null,
  l_cols: null,
  lastrow: null, // lastrow colors
  listmax: 0,
  treemax: 0,
  warnings: '',
  waiting: null,
  spage: '',
  sdata: '',
  init: function (lib, name) {
    this.library = lib;
    this.name = name;
    this.json.clear();
    this.jctrls.clear();
    this.hidepop = true;
    this.listmax = 0;
    this.treemax = 0;
    this.json_raw =
      this.menuicode =
      this.menuitems =
      this.mpopitems =
      this.mtrnitems =
      this.warnings =
      this.sdata = '';
  }
}
let err = {};
err.req = m => { throw Error(m); }
err.require = (c, m) => { if (c) throw Error(m); },
  err.requireex = (c, m, x) => { if (c) { let e = new Error(m); e.extra = x; throw e; } },
  err.rescue = e => {
    let ext = e.extra === undefined ? '' : e.extra;
    MRO.stack = e.stack;
    MRO.status(e.message, ext, DFS.ERROR);
    console.log(ext);
    if (e !== null) console.log(e);
  }
var MAC = { name: '', addr: 'NA-NA-NA-NA-NA-NA', brws: '', winu: '', mobi: false }
var CUR = { csd: '', css: '', lng: '', pos: '' };
const CNS = {
  imgs: '/pics/',
  zl: ['l0', 'l1', 'l2', 'l3'],
  zlas: ['zla0', 'zla1', 'zla2', 'zla3'],
  zlst: ['list0', 'list1', 'list2', 'list3'],
  zlyc: ['z0nclsz', 'z1nclsz', 'z2nclsz', 'z3nclsz'],
  zlci: ['z0cl', 'z1cl', 'z2cl', 'z3cl'],
  zclg: ['colg0', 'colg1', 'colg2', 'colg3'],
  zlcs: [-1, -1, -1, -1], // last col selected
  zcrr: ['currow0', 'currow1', 'currow2', 'currow3'],
  ztpr: ['toprow0', 'toprow1', 'toprow2', 'toprow3'],
  zcrp: ['curpage0', 'curpage1', 'curpage2', 'curpage3'],
  ztpg: ['toppage0', 'toppage1', 'toppage2', 'toppage3'],
  tots: ['zl0rows', 'zl1rows', 'zl2rows', 'zl3rows'],
  ifls: ['imgfields0', 'imgfields1', 'imgfields2', 'imgfields3'],
  edfs: ['edifields0', 'edifields1', 'edifields2', 'edifields3'],
  adfs: ['addfields0', 'addfields1', 'addfields2', 'addfields3'],
  frms: ['theform', 'formcap0', 'formcap1', 'formcap2', 'formcap3'],
  limg: [], edis: [], adds: [], adbs: [], cols: [], vars: [], zhis: [],
  NOPAR: new pairs(),
  NVARS: 32
}
const CARS = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N',
  'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
  'AA', 'AB', 'AC', 'AD', 'AE', 'AF', 'AG', 'AH', 'AI', 'AJ', 'AK', 'AL', 'AM',
  'AN', 'AO', 'AP', 'AQ', 'AR', 'AS', 'AT', 'AU', 'AV', 'AW', 'AX', 'AY', 'AZ'
];
PRM = { shortcut: '', scprms: '', cmpy: '', entry: '' };
function $l() { }
$l.prototype.init = function () {
  this.lid = -1;
  this.curpag = -1;
  this.rwspag = -1;
  this.lstpag = -1;
  this.nrows = -1;
  this.captype = null;
  this.has_ins = false;
  this.c_crow = null;
  this.c_trow = null;
  this.c_cpag = null;
  this.c_tpag = null;
  this._a_ = [];
  this._b_ = [];
  this._c_ = [];
  this._d_ = 0;//cols
  this._f_ = [];
  this.dataset = false;
}
let linfo = [new $l(), new $l(), new $l(), new $l()];
MRO.clr_linfo = () => { for (let i = 0; i < 4; ++i) linfo[i].init(); }

MRO.first_tree = false;
MRO.cmdh = [];
MRO.ax = null;

MRO.stadsc = { okay: '', prog: '', dwnl: '' };
MRO.sbs = null;
MRO.sb = null;
MRO.ge = null;
MRO.gw = null;
MRO.btnshdn = false;//btn err/wrn
MRO.laststatus = -1;

MRO._err = '';
MRO.stack = '';
MRO.skipunld = 0;
MRO.basics = [];
MRO.pass = null;
MRO.debug = false;
MRO.errlog = [];
MRO.saveer = true;
MRO.nrunfuns = 0;
MRO.ismain = false;
MRO.isincapt = false;
MRO.islsel = false;
MRO.iscrtlsel = false;
MRO.isalt_sel = false;
MRO.isshftsel = false; // for list selection
MRO.lmods = [];
MRO.devld = false;
MRO.menld = false;
MRO.ffocus = null;//first focus
MRO.curr_list = null;
MRO.curr_row = null;
MRO.curr_real = null;
// match code stuff --
MRO._match = null; // object to the mc
MRO._action = null; // action to exec on SEND
MRO.dlgtype = -1; //-1none/2match/8note/9newapp
// clock stuff
MRO.clklf = '';   // client session time left
MRO.clkof = null; // clock client/server offset
MRO.txts = null;
MRO.errs = null;
MRO.clie = new terminal();
MRO.ses = new pairs();
MRO.ctrls2match = new pairs();
MRO.cols2vars = new pairs();
MRO.fromclie = new pairs();
MRO.ismc = false;
MRO.fapp = false;
MRO.lststy = [null, null];
MRO.lstinp = null;
MRO.fix_text = s => {
  if (s.length > 0 && (s.indexOf('{') != -1 || s.indexOf('"') != -1 || s.indexOf('}') != -1)) {
    let mr = MRO.rpl_all;
    s = mr(s, "\\", "\\\\");
    s = mr(s, "\"", "\\\"");
    s = mr(s, "\t", "\\t");
    s = mr(s, "\n", "\\n");
    s = mr(s, "\r", "\\r");
  }
  return encodeURIComponent(s);
}
MRO.repHtml = (o, h) => {
  let n = o.cloneNode(false);
  n.innerHTML = h;
  o.parentNode.replaceChild(n, o);
  return n;
}
MRO.isfree = t => t.slice(0, 2) === 'S0';
MRO.islogon = () => MRO.are_in(DFS.ZTRNPAS) || MRO.are_in('S070') || MRO.are_in('S090');
MRO.check_resp = r => {
  let d = DFS;
  if (!r.hasval(d.ZSERROR)) return;
  let error = r.get(d.ZSERROR);
  let errex = r.get(d.ZHERROR);
  trans.saferes = r;
  if (MRO.check_error(r, d.ESESLKU)) MRO.lackuse();
  else if (errex === '') err.require(true, error);
  else err.requireex(true, error, errex);
}
MRO.make_fileprms = (typ, fil, dty, frc) => {
  let p = new pairs();
  p.set(DFS.ZTYPTRN, typ);
  p.set(DFS.ZFILE01, fil);
  p.set(DFS.PDOCTYP, dty);
  p.set(DFS.ZTYPRED, frc);
  return p;
}
MRO.spwan_lists = r => {
  let m = MRO, t = trans, d = window.document, a = CNS.zlas, b = CNS.zlst;
  for (let j = 0, n = t.listmax; j < n; ++j) {
    let l = r.get(a[j]);
    if (l.length === 0) continue;
    let i = parseInt(l);
    if (i >= n) continue;
    let pl = d.getElementById(b[i]);
    if (!pl) continue;
    t.l_res = r;
    linfo[i].dataset = r.getbool('bydataset' + i);
    m._fill_list(i, t.l_res);
  }
}
MRO.show_result = (res, or) => {
  let f = $2(CNS.frms[0]);
  if (!f) return;

  let resp = null, checkresp = false;
  let fun = res.get(DFS.ZEVENTN);
  if (fun.length > 0 && trans.json.has(fun)) {
    let tmp = MRO.make(trans.json.get(fun));
    if (tmp.has('response')) {
      resp = MRO.make(tmp.get('response'));
      checkresp = resp.notempty();
    }
  }
  let ori = typeof or !== 'undefined' ? or : null;
  let hasori = ori !== null;

  let isform = true, n0 = res.getbool('zafcfrm') ? 2 : 1; //fbw
  for (let i0 = 0; i0 < n0; ++i0) {
    if (i0 === 1) {
      f = $2(res.getbool('zdivfrm')), isform = false;
      if (!f) continue;
    }

    let els = isform ? f.elements : f.children;/*f.childNodes*/;
    let len = isform ? f.length : els.length;/*els.length*/;
    for (let i = 0; i < len; ++i) {
      let el = els[i];
      if (el.nodeType !== 1) continue;//elements only
      if (el.tagName.length === 2) continue;//BR

      let clnm = el.className, clnl = 0;
      if (isform) {
        if (clnm === undefined) continue; //not sure classless vars valid?
        clnl = clnm.length;
        if (clnl === 0) continue;//not sure classless valid?
        if (clnl >= 30 && el.tagName === 'PRE') {
          clnl = 14;
          clnm = 'ace_text-input';
        }
        if (clnl !== 6 && clnl !== 7 && clnl !== 8 && clnl !== 14) continue;
        if (clnl >= 7 && clnm.substr(0, 7) === 'rowitem') continue;
        if (clnl === 8 && clnm === 'mrolabel') continue;
        if (clnl === 6 && clnm === 'rowhdr') continue;
      }

      let id = el.id;
      //if (checkresp && resp.has(id)) id = resp.get(id);
      let ised = clnl === 14 && clnm === 'ace_text-input';
      if (ised) id = '$text$';
      if (checkresp && resp.has(id)) id = resp.get(id);
      if (id.length === 0) continue;
      let hasresult = res.has(id);
      let hasorival = (hasresult === false) && hasori && ori.has(id);
      let val = null;
      if (hasresult || hasorival) {
        if (hasresult) val = res.get(id);
        if (hasorival) val = ori.get(id);
      }
      if (val === null) continue;
      if (clnl === 8 && clnm === 'mrocheck') {
        if (val.toLowerCase() === 'true') val = '1';
        el.checked = parseInt(val);
        el.value = el.checked ? '1' : '0';
      } else if (ised) DEV.set_value(val);
      /*else if(typeof(val)==='object'){let o=new pairs();o.attach(val);el.value=o.buffer();}*/
      else if (el.type === 'date') el.value = MRO.rpl_all(val.substr(0, 10), '/', '-');
      else el.value = val;
    }
  }

  // update current listrow
  if (MRO.islsel) {
    let lid = MRO.curr_list;
    let updated = false;
    for (let i = 0, cars = CARS, nc = MRO.listncols(lid); i < nc; ++i) {
      let k = 'lcell'.concat(cars[i]);
      if (res.has(k))
        MRO.set_list_data(lid, MRO.curr_row, cars[i], res.get(k)), updated = true;
    }
    if (updated) {
      let real_row = MRO.curr_real !== '*' ? MRO.curr_real : linfo[lid].nrows + 1;
      MRO.list_update(lid, real_row);
    }
  }

  if (MRO.are_in(DFS.ZTHOME)) return;//TMPFIX
  let imgs = f.getElementsByTagName('img'), ni = imgs.length;
  let lnks = f.getElementsByTagName('a');
  for (let i = 0, n = ni + lnks.length; i < n; i++) {
    let el = i < ni ? imgs[i] : lnks[i];
    if (el === null || el === undefined) continue;
    let id = el.id;
    if (checkresp && resp.has(id)) id = resp.get(id);
    if (id.length === 0) continue;
    let hasresult = res.has(id);
    let hasorival = (hasresult === false) && hasori && ori.has(id);
    let val = null;
    if (hasresult || hasorival) {
      if (hasresult) val = res.get(id);
      if (hasorival) val = ori.get(id);
    }
    if (val === null) continue;
    if (el.tagName === 'IMG') {
      el.src = val + '?dummy=' + Math.random();
      el.onerror = function () { el.src = '/files/noimage.jpg'; }
    }
    if (el.tagName === 'A') {
      el.innerHTML = val;
      el.setAttribute('href', res.get('href'));//pending detect href
    }
  }
}
MRO.get_values = retrow => {
  let a = [], m = MRO, d = window.document, group = '', frms = CNS.frms;
  let lstlbl = null;
  for (let i = 0, ilen = frms.length; i < ilen; i++) {
    let f = d.getElementById(frms[i]);
    if (!f) continue;
    let jlen = f.length;
    let els = f.elements;
    for (let j = 0; j < jlen; ++j) {
      let el = els[j];
      let clnm = el.className;
      if (clnm === undefined) continue;
      let clnl = clnm.length;

      if (clnl === 0) continue;//not sure noclass valid?
      if (clnl !== 7 && clnl !== 8 && clnl !== 14) continue;
      if (clnl >= 7 && clnm.substr(0, 7) === 'rowitem') continue;
      if (clnl === 8 && clnm === 'mrolabel') {
        lstlbl = el.value;
        continue;
      }

      let id = el.id;
      let ised = false;
      if (id.length === 0) {
        ised = clnl === 14 && clnm === 'ace_text-input';
        if (ised) id = '$text$';
        else continue;
      } else
        if (clnm.charAt(0) !== 'm') continue;

      let tyinp = 0;
      if (clnl === 8 && clnm === 'mroinput') tyinp = 1;
      else if (clnl === 7 && clnm === 'mrodesc') tyinp = 2;
      else if (clnl === 7 && clnm === 'mrodate') tyinp = 3;

      if (tyinp !== 0) {
        if (el.type === 'radio') {
          if (el.name !== group) {//find group
            let rs = d.getElementsByName(group = el.name);
            for (let i = 0, length = rs.length; i < length; i++)
              if (rs[i].checked) {
                a.push('"'.concat(id, '":"', i.toString(), '"'));
                break;
              }
          }
          continue;
        }
        let v = el.value;
        if (tyinp === 1 || tyinp === 2) {
          // if necesary make upper or lower case
          let s = el.style['text-transform'];
          if (s.length === 9) {
            if (s === 'uppercase') v = v.toUpperCase();
            else if (s === 'lowercase') v = v.toLowerCase();
          }
        }
        if (tyinp === 1) {
          let isok = true;
          if (el.placeholder !== '') { //find better way
            if (el.size === 19 && el.placeholder === '9999/99/99 99:99:99') isok = m.chk_inp_date(v);
            //else if (el.size === 10 && el.placeholder === '9999/99/99') isok = m.chk_inp_date(v);
            //else if (el.size === 8 && el.placeholder === '99:99:99') isok = m.chk_inp_hour(v);
          }
          else if (el.pattern && el.patttern !== '') isok = m.chk_inp(v, el.pattern);
          else if (el.id.indexOf('pass') != -1) isok = m.chk_inp_pass(v);
          else isok = m.chk_inp_def(v);
          if (!isok) {
            m.ldm(typeof ERR, 'err');
            ERR.proc_color(el);
            err.requireex(true, 'incorrect_input', lstlbl ? lstlbl : '');
          }
          v = encodeURIComponent(v);
        }
        if (v.length > 1 && v.charAt(0) === '{') a.push('"'.concat(id, '":', v));
        else a.push('"'.concat(id, '":"', v, '"'));
        continue;
      }

      if (clnl === 8 && clnm === 'mrocheck') {
        a.push('"'.concat(id, '":"', el.checked ? '1",' : '0"'));
        continue;
      }
      let istext = clnl === 7 && clnm === 'mrotext';
      if (istext) {
        a.push(MEM.join5('"', id, '":"', encodeURIComponent(el.value), '"'));
        continue;
      }
      if (ised) {
        a.push(MEM.join5('"', id, '":"', m.fix_text(editor.getValue()), '"'));
        continue;
      }
    }
  }
  for (let i = 0; i < trans.listmax; ++i) {
    let lid = '' + i;
    a.push(MEM.join5('"rowsperpage', lid, '":"', d.getElementById('rowspertable'.concat(lid)).value, '"'));
    a.push(MEM.join5('"currpage', lid, '":"', linfo[lid].curpag, '"'));
  }
  if (m.islsel) {
    let id = m.curr_list, row = m.curr_row, c = CARS, gen = m.litem;
    for (let i = 0, n = m.listncols(id); i < n; ++i) {
      let cell = c[i];
      let rowid = d.getElementById(gen(id, row, cell));
      if (!rowid) break;
      a.push(MEM.join5('"lcell', cell, '":"', rowid.value, '"'));
    }
  } else if (retrow)
    for (let i = 0, n = 32, c = CARS; i < n; ++i)
      a.push('"lcell'.concat(c[i], '":""'));

  a.push(MEM.join5('"', DFS.ZCURFLD, '":"', d.activeElement.id, '"'));

  return a.join();
}
MRO.get_from_json = f => {
  let m = MRO, js = trans.json;
  if (!js.has(f)) return null;
  let lprms = null;
  let fun = m.make(js.get(f));
  if (fun.has('fromloc')) {
    lprms = new pairs();
    lprms.attach(fun.get('fromloc'));
    let s = m.ses;
    let vars = CNS.vars;
    for (let i = 0, n = lprms.getint('nvars'); i < n; ++i) {
      let k = vars[i];
      let v = lprms.get(k);
      lprms.del(k);
      lprms.set(v, s.get(v));
    }
  }
  if (fun.has('listsel')) lprms = LST.l2data(fun.get('listsel'));
  if (fun.has('simplesel')) lprms = LST.l2data(fun.get('simplesel'));
  if (fun.has('excelfile')) {
    lprms = new pairs();
    let file = fun.get('excelfile');
    file = m.get_val_of(file);
    lprms.set('fileuploaded', m.upload_file('uploadedfiles', file));
    lprms.set('exc2vals', '1');//mark to be processed else on server
  }
  if (fun.has('uploadfile')) {//simple upload
    lprms = new pairs();
    let file = fun.get('uploadfile');
    file = m.get_val_of(file);
    lprms.set('fileuploaded', m.upload_file('uploadedfiles', file));
    lprms.set('usefilename', '1');//mark to use uploaded file path
  }
  return lprms;
}
MRO.show_cursor = s => {
  window.document.body.style.cursor = s ? 'wait' : 'default';
  if (trans.waiting) trans.waiting.display = s ? 'block' : 'none';
}
MRO.send = (eventn, xprms, sync) => {
  let m = MRO, d = DFS, t = trans;
  t.saferes = null;
  if (m.nrunfuns >= 3) return;
  ++m.nrunfuns;
  try {
    if (eventn !== d.ZONLOAD && eventn !== d.ZONRELD) {
      if (!t.json.has(eventn)) {
        --m.nrunfuns;
        return;
      }
    }
    m.status(m.stadsc.prog, '', d.PROCESS);
    m.show_cursor(true);

    let saveres = 0, synccall = false, retres = '1', dolocal = '', seval = '';
    let doowner = '', module = '', code = '', retprms = null, fun = '';
    let j = m.make(t.json.get(eventn));
    if (j.notempty()) {
      saveres = j.has("save_state") ? j.getint("save_state") : 0;
      synccall = j.has("sync") ? j.get("sync") === '1' : false;
      retres = j.has("retresult") ? j.get("retresult") : '1';
      dolocal = j.has("dolocal") ? j.get("dolocal") : '';
      doowner = j.has("doowner") ? j.get("doowner") : '';
      seval = j.has("eval") ? j.get("eval") : '';
      module = j.has('module') ? j.get('module') : '';
      code = j.has('codebehind') ? j.get('codebehind') : '';
      fun = j.has('webservice') ? j.get('webservice') : '';
      retprms = j.has("retprms") ? j.get("retprms") : null;
    }

    let pcode = code.length > 0 ? code : t.defcbh;
    let pmodule = module.length > 0 ? module : t.defmod;

    //decision webservice or service is if it have module or not
    let isinternal = dolocal.length > 0 | doowner.length > 0 | seval.length > 0;
    let wstype = 0; // 0-site 1-proxy 2-kernel
    if (pmodule.length > 0) {
        if ((pmodule.length === 5 && pmodule === 'proxy')
//////////////// PATH /////////////////////////////////
|| (pmodule.length === 3 && pmodule === 'sfc')) wstype = 1;
        else if (pmodule.length === 3 && (pmodule === 'gat'
//////////////// PATH ////////////////////////////////
|| pmodule === 'atl')) {
        wstype = 2;
        fun = 'atlevent';
      }
    }
    //load,unload,reload must be sync,otherwise res will be out of pashe
    let evl = eventn.length;
    if ((evl === 6 && eventn === d.ZONLOAD) ||
      (evl === 8 && (eventn === d.ZONUNLD || eventn === d.ZONRELD))) synccall = true;

    if (sync !== undefined) synccall = true;

    let clie = m.clie;
    // construct parameters
    let lparms = m.get_from_json(eventn);
    let purevals = m.get_values(false);
    if (lparms && lparms.notempty()) purevals += ','.concat(lparms.tojson());
    if (xprms) purevals += xprms;

    // final event/function/webserice and prameters/values
    let evnt = MEM.join5('"', d.ZEVENTN, '":"', eventn, '"');
    let values = MEM.join3('"zvalues":{', purevals, '}');

    if (isinternal) {
      if (seval.length > 0) {
        m.eval_js(seval);
        m.status(m.stadsc.okay, '', d.OK);
      }
      else if (dolocal.length > 0) {
        m.dolocal(dolocal, values, j, eventn);
        if (retprms) {
          let pp = m.make(retprms);
          pp.on(d.ZRESEND);
          m.handle_response(pp);
        }
        m.status(m.stadsc.okay, '', d.OK);
      }
      else if (doowner.length > 0 && parent.window.opener) {
        let p = m.get_doloc_prms(values, j);
        parent.window.opener[p.get('executor')](doowner, p);
      }
      m.show_cursor(false);
      m.status(m.stadsc.okay, '', d.OK);
      --m.nrunfuns;
      return true;
    }

    let a = $2('exjb');
    let exses = true;
    if (a && a.checked) exses = false;

    if (exses) { // normal execution
      /*if (synccall) m.handle_response(PCF.post2(clie.url, t.sdata, false)); else*/
      if (wstype === 0) {
        let retqry = $2('onsu');
        m.clie.service(pmodule, pcode,
          clie.gen_header(retres, (retqry && retqry.checked) ? '1' : '0'),
          clie.gen_basics(saveres > 0 ? MEM.join5('"', d.ZHISPOS, '":"', CUR.pos, '"') : ''),
          evnt, values, true);
      }
      else 
        m.clie.proxy(fun, '{'.concat(evnt, ',', values,',"sitcall":"', pmodule, '"}'), true);
    }
    else { // job execution
      a.checked = false; //always clean it
      m.show_dialog(DFS.ZTJOBBT, '{"utext":"'.concat(m.fix_text(clie.url.concat('?',
                                  t.sdata)), '"}'), new pairs(), 0, null);
      m.status(m.stadsc.okay, '', d.OK);
      m.show_cursor(false);
    }
  }
  catch (e) {
    m.show_cursor(false);
    --m.nrunfuns;
    err.rescue(e)
  }
}
MRO.handle_response = res => {
  let m = MRO, d = DFS, t = trans, ev = null;
  let dogoto = false, doloca = false, dodisp = false;
  let status = null, warning = null, gototrans = null, locaction = null;

  try {
    t.saferes = res;
    ev = res.get(d.ZEVENTN);
    if (res.has(d.ZUPDCLS)) m.updclises(res);
    if (res.has(d.ZDOWNLD)) m.proc_download(res);
    if (t.listmax) m.spwan_lists(res);
    m.show_result(res);
    if (res.hasval(d.ZSETCUR)) m.set_focus(res);
    //if (res.has(d.ZGOTOBC)) gototrans = res.get(d.ZGOTOBC);
    if (res.hasval(d.ZSERROR)) {
      m.ldm(typeof ERR, 'err');
      ERR.proc_onerror(res, ev);
    }
    if (res.hasval(d.ZSWARNG)) warning = res.get(d.ZSWARNG);
    if (res.hasval(d.ZSTATUS)) status = res.get(d.ZSTATUS);
    if (ev.indexOf('oninsert') !== -1 || ev.indexOf('onedit') !== -1) m.post_cap(res, ev);
    //if (res.ison(d.ZSHWDLG)) m.show_dialog(d.ZTRNDLG, res.buffer(), new pairs(), res.getint(d.ZDLGTYP), null);
    if (res.has(d.ZLOCACT)) {
      locaction = res.get(d.ZLOCACT);
      doloca = true;
    }
    if (res.are_eq(d.ZPSTMSG, 'a')) m.postmessage(res.getint(d.ZPSTMSI));
    if (res.has(d.ZDSPTCH)) dodisp = true;
    if (res.has(d.ZGOTODO)) {
      gototrans = res.get(d.ZGOTODO);
      dogoto = gototrans && gototrans !== undefined;
    }
    if (res.has(d.ZKEYPRE)) m.handle_keybd(res.getint(d.ZKEYPRE));
    err.require(!res.ison(d.ZRESEND), 'fun_not_completed');
    if (warning) m.status(warning, '', d.WARNING);
    else m.status(status ? status : m.stadsc.okay, '', d.OK);
  }
  catch (e) {
    err.rescue(e)
  }
  --m.nrunfuns;
  m.show_cursor(false);

  if (dogoto) m.do_goto(res.get(d.ZDSPTCH), gototrans);
  if (doloca) m.do_local(res = m.make(locaction));
  if (dodisp) m.do_disp(res.get(d.ZDSPPAG), res.get(d.ZDSPTCH), res, ev);
}
MRO.updclises = r => {
  MRO.ses.rpl_from(MRO.make(r.get(DFS.ZUPDCLS)));
}
MRO.keyup = e => {
  if (!e) e = window.event;
  MRO.isctrlsel = e.ctrlKey;
  MRO.isalt_sel = e.altKey;
  MRO.isshftsel = e.shiftKey;
}
MRO.KeyCheck = e => { // main key check
  if (!e) e = window.event;
  let k = e.keyCode || e.which;
  let m = MRO;

  m.isctrlsel = e.ctrlKey;
  m.isalt_sel = e.altKey;
  m.isshftsel = e.shiftKey;

  try {
    switch (k) {
      case 13:
        let t = e.target;
        if (t && t.type && t.type.length === 8 && t.type === 'textarea') {
          if (t.parentNode && t.parentNode.id === 'cmdbox') {
            let lines = $2('hiscmd').value.split('\n');
            let l = lines[lines.length - 1];
            l = CMD.proc_cmd(l);
            if (l !== '') {
              let c = l.split(' ');
              execmd(c[0], c[1] ? c[1] : '');
              break;
            }
          }
          return true;
        }
        m.proc_enter();
        return false;
        break;

      case 27: m.handle_esc(); break;
      case 33: if (m.islsel) m.btn_list_up(m.curr_list); break;
      case 34: if (m.islsel) m.btn_list_down(m.curr_list); break;
      case 36: if (m.islsel && e.ctrlKey) m.btn_list_top(m.curr_list); break;
      case 35: if (m.islsel && e.ctrlKey) m.btn_list_bottom(m.curr_list); break;
      case 38: if (m.islsel) move_row_up(); break;
      case 40: if (m.islsel) move_row_down(); break;
      case 45: if (e.altKey) m.ins_list_row();
              else if (m.has_fun('onins')) m.send('onins'); break;
      case 46: if (e.ctrlKey) m.del_list_row(); break;
      case 113: execmd('SIMDBCLK'); break;//F2
      case 114: if (e.ctrlKey) execmd('HOME');
                else {
                  execmd('F3');
                  e.preventDefault();
                }
                break;//F3
      case 115: m.ldm(typeof MTC, 'mtc');//F4
        if (e.shiftKey) MTC.insert_date();
        else MTC.match_codes(false); break;
      case 116: if (!m.debug && m.clie.isactive()) e.preventDefault(); break;//F5
      case 118: m.ldm(typeof MTC, 'mtc'); MTC.seek_codes(); break;//F7
      case 119: m.send('onf8'); break;//F8
      case 120: if (e.ctrlKey) { } else m.send('onf9'); break;//F9
      case 121: if (e.altKey) execmd('gted', '0'); else//F10
        if (e.ctrlKey) execmd('gted', '1');
        m.send('onf10');
        break;
      case 123: if (e.ctrlKey) execmd('BACK'); break;//F12
    }
    if (k === 8) {
      let t = e.target;
      let rx = /INPUT|SELECT|TEXTAREA/i;
      if (!rx.test(t.tagName) || t.disabled || t.readOnly) {
        e.preventDefault();
      }
    }
    else if (e.ctrlKey && ((k >= 65 && k <= 90) || (k >= 48 && k <= 57)))
      m.key_ctrl(m, e, k);
    else if (!e.ctrlKey && !e.altKey && !e.shiftKey && k >= 65 && k <= 122 && m.curr_list !== null)
      execmd('PKEYLST', String.fromCharCode(k));
  }
  catch (e) {
    err.rescue(e);
  }
  //return false;
}
MRO.onkeydown = e => { // form key press
  let m = MRO, d = window.document;
  if (!e) e = window.event;
  let k = d.all ? e.keyCode : e.which;
  if (k === 8) {
    let t = e.srcElement.type;
    if (t !== 'text' && t !== 'textarea' && t !== 'password') {
      e.keyCode = 0;
      if (d.all) event.returnValue = false;
      else e.preventDefault();
    }
  }
  else if (k === 13)
    if (e.srcElement.type !== 'textarea') {
      m.proc_enter();
      return false;
    }
}
MRO.set_cmpy = r => {
  try {
    MRO.check_resp(r);
    $2('tit').value = r.get('l00A');
  }
  catch (e) { err.rescue(e); };
}
MRO.load_cmpy = () => {
  let p = new pairs();
  p.set('query', 'embedded');
  p.set('sqltext', 'exec CORE.dbo.cmpy_getname ' + MRO.ses.get(DFS.ZCOMPNY) + ';');
  MRO.clie.proxy('execute_query', p.buffer(), true, MRO.set_cmpy);
}
MRO.status = (t, x, ty) => {
  let m = MRO, d = DFS, off = ' ';
  // handle error and warning
  let iserr = ty === d.ERROR, iswrn = ty === d.WARNING;
  if (iserr && m.errs.has(t)) t = m.errs.get(t);
  if (iswrn && m.errs.has(t)) t = m.errs.get(t);
  if (iserr || iswrn) {
    if (m.btnshdn) {
      m.ge.visibility = iserr ? 'visible' : 'hidden';
      m.gw.visibility = ty === d.WARNING ? 'visible' : 'hidden';
      m.btnshdn = false;
    }
    m.ge.display = m.gw.display = 'block';
    off = '      ';
  }
  else if (!m.btnshdn) {
    m.ge.display = m.gw.display = 'none';
    m.btnshdn = true;
  }
  // show the text and extra if any
  m.sb.value = off.concat(x === '' ? t : t.concat(': ', x));
  // handle logs if any
  if (iserr && m.saveer) m.errlog.push(t);
  // handle foxpro box
  let c = $2('cmdbox');
  if (c && c.style.display === 'block') CMD.pusherr(t);
  // change backcolor if must
  if (m.laststatus !== ty) {
    m.sbs.backgroundColor = d.STABKCOLS[ty];
    m.sbs.color = d.STACOLORS[ty];
  }
  m.laststatus = ty;
}
MRO.update_status = () => {
  if (MRO.ismc) return;
  let s = MRO.ses, c = CUR, d = DFS;
  let udsc = sessionStorage.getItem('usrdesc');
  $2('usrid').value = udsc === null ? '' : udsc;
  $2('fbic').value = s.get(d.ZUSERID);
  $2('fbid').value = s.get(d.ZCOMPNY);
  $2('fbix').value = c.css;
  $2('fbil').value = c.lng;
}
MRO.set_script = p => {
  try {
    MRO.check_resp(p);
    let r = p.get(DFS.ZFILERS), d = window.document;
    let s = d.createElement('script'), mr = MRO.rpl_all;
    r = mr(r, '\\"', '"');
    r = mr(r, '<\\/', '</');
    r = mr(r, ' />', '/>');
    s.appendChild(d.createTextNode(r));
    d.body.appendChild(s);
  }
  catch (e) {
    err.rescue(e)
  }
}
MRO.set_tree = p => {
  try {
    MRO.check_resp(p);
    let id = p.get('tree');
    let ul = TRE.get_tree(id);
    let lv = p.getint('expand', -1);
    if (ul) {
      ul.innerHTML = p.get(DFS.ZFILERS);
      let q = MRO.make1('id', id);
      if (MRO.are_in(DFS.ZTHOME)) TRE.TREEIA(q); // should handle better
      else if (lv === -1) TRE.TREEIC(q);
      else TRE.TREEIE(q, lv); // none home always expand
    }
  }
  catch (e) {
    err.rescue(e)
  }
}
MRO.load_tree = (o, byforce) => {
  if (!o.has('dbid')) return;
  try {
    let dbid = o.get('dbid');
    let firstim = o.getbool('loadfirst');
    let doforce = o.getbool('forceload');
    let force = (MRO.first_tree && firstim) || doforce;
    if (firstim) MRO.first_tree = false;
    if (byforce !== undefined) force = byforce;

    let p = MRO.make_fileprms('trans', dbid, 'TRE', force ? 'force' : '');
    let q = new pairs();
    q.set('tree', o.get('treeid'));
    q.set('expand', o.get('expand'));
    if (dbid !== '') MRO.clie.proxy('get_file', p.buffer(), true, MRO.set_tree, q);
    else MRO.set_tree(q); // avoid going for nothing
  }
  catch (e) {
    err.rescue(e)
  };
}
MRO.proc_tree = (rr) => {
  trans.treemax = rr._.length;
  for (let i = 0, n = trans.treemax; i < n; ++i)
    MRO.load_tree(MRO.make(rr._[i]));
}
MRO.set_menu = p => {
  try {
    MRO.check_resp(p);
    trans.menuitems = p.get(DFS.ZFILERS);
    MRO.show_menu();
  }
  catch (e) {
    err.rescue(e)
  }
}
MRO.load_menu = (mstr, force) => {
  let m = MRO, t = trans;
  if (mstr.length >= 32) { //better criteria code/embbeded
    t.menuitems = mstr;
    return;
  } 
  t.menuitems = '';
  t.menuicode = mstr;
  if (mstr === '') {
    m.show_menu();
    return;
  }
  try {
    let p = m.make_fileprms('trans', t.menuicode = mstr, 'MEN', force ? 'force' : '');
    m.clie.proxy('get_file', p.buffer(), true, m.set_menu);
  }
  catch (e) {
    t.menuicode = t.menuitems = '';
  }
}
MRO.del_menu = s => {
  trans.menuicode = trans.menuitems = '';
  let i = s.indexOf('<!--menu begin');
  if (i !== -1) {
    f = s.indexOf('menu end*-->', i + 14);
    if (f !== -1) {
      s = MRO.del_str(s, i, f + 12);
      return s;
    }
  }
  return s;
}
MRO.crt_popmenu = s => {
  trans.mtrnitems = '';
  let i = s.indexOf('<menupop>');
  if (i === -1) return s;
  let f = s.indexOf('</menupop>', i + 9);
  if (f !== -1) {
    trans.mtrnitems = s.slice(i + 9, f);
    s = MRO.del_str(s, i, f + 10);
  }
  return s;
}
MRO.add_warn = (w, t) => { trans.warnings += w + ' ' + t + '\r\n'; }
MRO.nextchar = (s, i, c) => {
  for (let j = i, n = j + 5; j < n; ++j)
    if (s[j] !== ' ') return s[j] === c ? j : -1;
  return -1;
}
MRO.get_js = hstr => {
  let m = MRO, t = trans;
  let s = hstr;
  let c = hstr.indexOf('<script');
  if (c === -1) return [s, '', '', ''];
  let a = hstr.indexOf('javascript">', c);
  if (a === -1) return [s, '', '', ''];
  let b = hstr.indexOf('script>', a);
  if (b === -1) return [s, '', '', ''];

  let j = hstr.slice(a + 12, b - 2);
  s = m.del_str(hstr, c, b + 7);
  let rest = j;
  let h = rest;

  let simple = true, uc = -1;
  let r = ['', '', '', '', '', ''];
  let f = ['lprms', 'setupctrls', 'module', 'codebehind', 'menu', 'tree'];
  for (let i = 0; i < 6; ++i) {
    let xx = a = h.indexOf(f[i]);
    if (a === -1) continue;
    a = m.nextchar(h, a + f[i].length, '=');
    if (a === -1) err.requireex(true, 'syntax_error: ', f[i] + ' = expected');
    let g = a;
    simple = true;
    a = m.nextchar(h, g + 1, "'");
    if (a === -1) {
      simple = false;
      a = m.nextchar(h, g + 1, '"');
    }
    if (a === -1) err.requireex(true, 'syntax_error: ', f[i] + ' mismatch quote');
    b = simple ? h.indexOf("';", a + 1) : h.indexOf('";', a + 1);
    if (b === -1) err.requireex('syntax_error: ', f[i] + ' ; expected');
    r[i] = h.slice(a + 1, b);
    h = m.del_str(h, xx, b + 2);
    if ((b + 2) > uc) uc = b + 2;
  }

  // set or clen values
  let js = r[0]; // javascript
  let sc = r[1]; // setup ctrls
  t.defmod = r[2]; // module
  t.defcbh = r[3]; // codebehind
  let mn = r[4]; // menu
  t.tredat = MRO.make(r[5].charAt(0) === '[' ? r[5] : '[{"dbid":"' + r[5] + '","treeid":"tree"}]'); // tree
  let ucode = (uc !== -1) ? j.substr(uc) : j;
  return [s, js, sc, ucode.trimLeft(), mn, t.tredat];
}
MRO.proc_js = (js, sc, full) => {
  let t = trans;
  try {
    t.json.set_simple(t.json_raw = js);
    if (sc.length > 0) t.jctrls.set_simple(sc);
    else t.jctrls.clear();
  }
  catch (e) {
    MRO.add_warn(e.message, 'js');
  }
  MRO.eval_js(full);
}
MRO.eval_js = a => {
  try {
    if (a.length > 0) eval.apply(window, [a]);
  }
  catch (e) {
    MRO.add_warn(e.message, 'js');
  }
}
MRO.crt_appbar = s => {
  let h = $2('thdr');//appbar 2 divhdr
  if (!h) return s;
  let o1 = s.indexOf('<input class="mrotitleid"');
  if (o1 === -1) return s;
  let o2 = s.indexOf('</div><!--inhdr-->', o1);
  if (o2 === -1) return s;
  h.innerHTML = s.slice(o1, o2 + 6);
  return MRO.del_str(s, o1, o2 + 18);
}
MRO.crt_controls = s => {
  let m = MRO, f = $2('dform');
  f = m.repHtml(f, s);
  f.onkeypress = m.onkeydown;//m.nokey;
  if (typeof BAR !== 'undefined') BAR.right = f;
}
MRO.prep_edts = (mkpr, teds, js, ls, iid, c) => {
  let eds = mkpr(js.get(c.edfs[iid]));//old
  if (eds.notempty()) {
    let ed = c.edis;
    for (let j = 0, n = eds.getint('totedis'); j < n; ++j)
      teds[j] = eds.get(ed[j]);
    return;
  }
  let o = mkpr(ls.get('onedit'));//new
  if (o.notempty()) {
    let f = mkpr(o.get('fields'));
    for (let j in f._) teds[j] = f._[j];
  }
}
MRO.prep_adds = (mkpr, tads, tabs, js, ls, iid, c) => {
  let adds = mkpr(js.get(c.adfs[iid]));//old
  if (adds.notempty()) {
    let ad = c.adds;
    let ab = c.adbs
    for (let j = 0, n = adds.getint('totadds'); j < n; ++j) {
      tads[j] = adds.get(ad[j]);
      tabs[j] = adds.get(ab[j]);
    }
    return;
  }
  let o = mkpr(ls.get('oninsert'));
  if (o.notempty()) {
    let f = mkpr(o.get('fields'));
    for (let j in f._) {
      let v = f._[j];
      if (typeof (v) === 'string') {
        tads[j] = f._[j];
        tabs[j] = '';
      }
      else {
        let bound = MRO.make(f._[j]);
        for (let k in bound._) {
          tads[j] = k;
          tabs[j] = bound._[k];
          break;
        }
      }
    }
  }
}
MRO.prep_imgs = (mkpr, tt, js, ls, iid, c) => {
  let ims = c.imgs;
  let imgs = mkpr(js.get(c.ifls[iid]));//old
  if (imgs.notempty()) {
    let i = c.limg;
    for (let j = 0, n = imgs.getint('totimgs'); j < n; ++j)
      tt[j] = ims.concat(imgs.get(i[j]));
    return;
  }
  if (ls.notempty()) {//new
    let i = mkpr(ls.get('images'));
    for (let j in i._) tt[j] = ims.concat(i._[j]);
  }
}
MRO.prep_csrw = (mkpr, trcs, ls) => {
  let a = mkpr(ls);
  if (a.isempty()) return; // nothing to check into
  for (let i = 0; i < 64; ++i) { // look for columns
    let c = 'col' + i;
    let cc = a.get(c); // column from trans
    if (cc) {
      let nvals = cc.length;
      let vals = [];
      for (let j = 0; j < nvals; ++j) { // look for values
        let ccc = new pairs();
        ccc.attach(cc[j]);
        let inf = new Object();
        inf.oper = ccc.get('oper');;
        inf.value = ccc.get('value');
        inf.numb = inf.value.replace(/[^0-9.-]+/g, "");
        inf.numb = inf.numb.length > 0 ? parseInt(Number(inf.numb)) : NaN;
        inf.className = ccc.get('className');
        inf.color = ccc.get('color');
        inf.backgroundColor = ccc.get('backgroundColor');
        vals[j] = inf;
      }
      trcs[i] = vals;
    }
  }
}
MRO.set_css_col = r => {
  let m = MRO, t = trans, lid = r.getint('lid'), rfrsh = r.getint('refresh');
  m.check_resp(r);
  let nrows = r.getint(CNS.tots[lid]);
  if (nrows === 0) return '';
  let lst = '', col = '', s = '{';
  let zls = CNS.zl[lid];
  for (let i = 0; i < nrows; ++i) {
    let row = zls.concat(i);
    col = r.get(row.concat('A'));
    if (col !== lst) {
      if (s.length > 1) s = s.substring(0, s.length - 1) + '],'; // del last comma
      s += '"col'.concat(col, '":[');
      lst = col;
    }
    s += '{"oper":"'.concat(r.get(row.concat('B')),
      '","value":"', r.get(row.concat('C')),
      '","color":"', r.get(row.concat('D')),
      '","backgroundColor":"', r.get(row.concat('E')),
      '","bold":"', r.get(row.concat('F')),
      '","italic":"', r.get(row.concat('G')), '"},');
  }
  s = s.substring(0, s.length - 1) + ']}'; // del last comma
  m.prep_csrw(m.make, t.l_csus[lid], s);
  if (rfrsh === 1) m._fill_list(lid, t.l_res);
}
MRO.get_css_list = (lid, refresh) => {
  let m = MRO, c = m.ses; d = DFS, t = trans;
  t.l_csus[lid] = [];
  let ext = m.make('{"lid":"' + lid + '","refresh":"' + (refresh === true ? '1' : '0') + '"}');
  let r = m.clie.site('core', '', 'execute_query', '{"query":"embedded","sqltext":"'.concat(
    "exec CORE.dbo.conds_get '", c.get(d.ZCOMPNY), "','", c.get(d.ZUSERID), "','",
    t.name, "','", lid, "','COL';", '"}'), true, m.set_css_col, ext);
}
MRO.prep_controls = () => {
  let m = MRO, c = CNS, t = trans, mkpr = m.make;
  t.qrylayout = [];
  t.l_imgs = [], t.l_edts = [], t.l_adds = [];
  t.l_adbs = [], t.l_csrw = [], t.l_csus = [], t.l_cols = [];
  let rls = t.l_ronlies = {};
  t.listmax = 0;
  t.l_res = null;
  m.clr_linfo();

  m.ffocus = null;
  let frs = null, def = null;
  let x = $2(c.frms[0]);
  if (!x) return;
  let nels = x.length;
  let els = x.elements;
  for (let i = 0; i < nels; ++i) {
    let el = els[i];
    let id = el.id;
    let il = id.length;
    if (il === 0) continue;
    let clnm = el.className;
    let clnl = clnm.length;

    if ((clnl === 8 && clnm === 'mroinput') ||
      (clnl === 7 && clnm === 'mrodate')) {
      let ronly = el.readOnly;
      let isfirst = el.tabIndex === 1;
      let isinv = el.type === 'hidden';
      let isronly = ronly && ronly === true;
      if (!frs && isfirst && !isinv && !isronly) frs = el;
      if (!def && !isinv && !isronly) def = el;
      if (!isronly) rls[id] = 1;
      continue;
    }
    if (clnl === 8 && clnm === 'mrocheck' && el.readOnly) {
      el.onclick = function () { return false };
      continue;
    }

    if (il !== 13) continue;
    let a = id.slice(0, 12);
    if (a !== 'rowspertable') continue;

    let lid = id.charAt(12);
    let iid = parseInt(lid);
    let lst = c.zlst[iid];
    let tb = $2(lst);
    if (tb) {
      tb.onmousewheel = m.mousewheel;
      tb.onmousedown = m.mousedown;
      tb.onmouseup = m.mouseup;
    }

    let js = t.json; //old
    let ls = mkpr(t.jctrls.get(lst)); //new

    if (js.has(lst)) t.qrylayout[iid] = mkpr(js.get(lst)).getint('dynlayo');

    m.get_css_list(lid, false);
    m.prep_csrw(mkpr, t.l_csrw[iid] = [], ls.get('css-row'));
    m.prep_edts(mkpr, t.l_edts[iid] = [], js, ls, iid, c);
    m.prep_adds(mkpr, t.l_adds[iid] = [], t.l_adbs[iid] = [], js, ls, iid, c);
    m.prep_imgs(mkpr, t.l_imgs[iid] = [], js, ls, iid, c);

    m.pre_list(lid);
    let lf = linfo[iid], d = window.document;
    lf.has_ins = m.has_fun('oninsert'.concat(lid));
    lf.c_crow = d.getElementById(c.zcrr[iid]);
    lf.c_trow = d.getElementById(c.ztpr[iid]);
    lf.c_cpag = d.getElementById(c.zcrp[iid]);
    lf.c_tpag = d.getElementById(c.ztpg[iid]);
    ++t.listmax;
  }

  let fs = mkpr(t.jctrls.get('filters')); // Install input filters.
  if (fs.notempty())
    if (typeof CTR === 'undefined') m.load('/web/', 'ctr.js', false); // delay at the end
  for (let k in fs._) {
    let a = $2(k);
    if (a === null) continue;
    switch (fs.get(k)) {
      case 'sint': CTR.InpFilter(a, v => /^-?\d*$/.test(v)); break;
      case 'uint': CTR.InpFilter(a, v => /^\d*$/.test(v)); break;
      case 'lint': CTR.InpFilter(a, v => /^\d*$/.test(v) && (v === "" || parseInt(v) <= 500)); break;
      case 'float': CTR.InpFilter(a, v => /^-?\d*[.,]?\d*$/.test(v)); break;
      case 'curr': CTR.InpFilter(a, v => /^[0-9.,\/\+\-\*()]*$//*/^-?\d*[.,]?\d{0,2}$/*/.test(v));
        a.onblur = () => { CTR.calc(a); }; break;
      case 'lat': CTR.InpFilter(a, v => /^[a-z]*$/i.test(v)); break;
      case 'hex': CTR.InpFilter(a, v => /^[0-9a-f]*$/i.test(v)); break;
      case 'phone': a.addEventListener('keydown', CTR.phone_keydown);
        a.addEventListener('input', CTR.phone_input);
        break;
    }
  }

  if (frs) {
    m.ffocus = frs;
    frs.focus();
  }
  else if (def) {
    m.ffocus = def;
    def.focus();
  }
}
MRO.unload_trans = () => {
  let m = MRO;
  if (m.ismain) {
    if (m.clie.isactive() && trans.name.length > 0) m.send(DFS.ZONUNLD);
  } else if (trans.name.length > 0) m.send(DFS.ZONUNLD);

  let ftim = trans.name === '';
  trans.name = '';
  if (ftim) return;
  m.handle_esc();
  m.close_match();
  m.del_ctrls();
}
MRO.reload_trans = (m, d, t) => {
  m.send(d.ZONRELD);
  m.send(d.ZONLOAD);
}
MRO.download_trans = (name, force) => {//mostly matchcode
  let m = MRO, d = DFS;
  let p = m.make_fileprms('trans', name, 'TRN', force ? 'force' : '');
  let pub = name.slice(0, 2) === 'S0';
  if (!pub) {
    p.set(d.ZRTRNCD, name);
    p.set(d.ZTYPRGT, 'acc');
  }
  p = m.clie.proxy('get_file', p.buffer());
  m.check_resp(p);

  let l = p.get(d.LIBRARY);
  let r = p.get(d.ZFILERS);
  err.require(r.length === 0, 'file_empty');
  if (!m.ismc) CUR.pos = p.get(d.ZHISPOS);
  return [l, r];
}
MRO.load_trans = (force, lib, name, saferes, str) => {
  name = name.toUpperCase();
  let d = DFS, m = MRO, t = trans;
  err.require(!m.isfree(name) && !m.clie.isactive(), d.EOPOUSE);

  // if html wasn't supplied we look for it
  if (str.length === 0 || force) {
    let a = m.download_trans(name, force);
    lib = a[0], str = a[1];
  }

  m.unload_trans();
  t.init(lib, name);
  if (!m.ismc) $2('fusr').value = t.library, $2('fbin').value = t.name;
  str = m.del_menu(str);
  str = m.crt_popmenu(str);
  str = m.crt_appbar(str);
  let rr = m.get_js(str);
  str = rr[0];

  m.crt_controls(str);
  m.proc_js(rr[1], rr[2], rr[3]);
  m.prep_controls();
  m.show_title();

  if (!m.ismc) m.load_menu(rr[4], false);
  if (!m.ismc) m.proc_tree(rr[5]);

  if (saferes !== '') m.show_result(new pairs(saferes));

  m.send(d.ZONLOAD);

  if (t.warnings.length > 0) m.status('compilation_with_errors', '', d.WARNING);
  if (m.fapp && m.ismc) m.fixtbl();
  else if (!MAC.mobi && (m.are_in('S020', 'S030', 'S025'))) m.adjustmc(); // fbw
}
MRO.move2trans = (lib, nm, res, frc, htm) => {
  try {
    MRO.load_trans(frc, lib, nm, res, htm);
  }
  catch (e) {
    err.rescue(e)
  }
}
MRO.isvalidjson = t => {
  return /^[\],:{}\s]*$/.test(t.replace(/\\["\\\/bfnrtu]/g, '@').
    replace(/"[^"\\\n\r]*"|true|false|null|-?\d+(?:\.\d*)?(?:[eE][+\-]?\d+)?/g, ']').
    replace(/(?:^|:|,)(?:\s*\[)+/g, ''));
}
MRO.ins_trans = (name, res, force) => {
  let p = new pairs();
  p.set(DFS.ZRTRNCD, name);
  p.set(DFS.ZTYPRED, force ? 'force' : '');
  p.set(DFS.RETPRMS, MRO.make1(DFS.ZRTRNCD, name).get_data());
  let prms = p.getjson();
  err.require(!MRO.isvalidjson(prms), 'wrong_trans_code');
  MRO.gui('gui_insert_trans', prms, res);
}
MRO.handle_gui = r => {
  try {
    let m = MRO;
    m.show_cursor(false);
    m.check_resp(r);
    m.restore_state(r, '');
    if (r.ison(DFS.ZDOWNLD)) m.proc_download(r);
    if (m.are_in(DFS.ZTHOME)) BAR.EASYACC(false);
  }
  catch (e) {
    err.rescue(e)
  }
}
MRO.gui = (f, p, lres) => {
  let m = MRO;
  m.show_cursor(true);
  if (lres === undefined || !lres) {
    m.clie.kernel(f, p, true, MRO.handle_gui);
    return;
  }
  let r = m.clie.kernel(f, p);
  m.show_cursor(false);
  m.check_resp(r);
  m.restore_state(r, lres);
  if (r.ison(DFS.ZDOWNLD)) m.proc_download(r);
}
MRO.restore_state = (res, localr) => {
  let m = MRO, d = DFS, locres = new pairs();
  if (localr) locres.attach(localr);
  let lib = res.get(d.LIBRARY);
  let name = res.get(d.ZRTRNCD);
  if (name === '') return;

  let lastprm = m.make(res.get(d.ZORIVAL));
  let lastres = m.make(res.get(d.ZLSTRES));
  let usrparm = m.make(res.get(d.ZUSRPRM));
  let eventnm = res.get(d.ZDSPTCH);
  let html = res.get(d.ZFILERS);
  if (!m.ismc) CUR.pos = res.get(d.ZHISPOS);

  if (locres && locres.notempty()) lastres.append(locres);
  if (usrparm && usrparm.notempty()) lastres.append(usrparm);

  m.move2trans(lib, name, '', false, html);

  if (lastres.notempty()) {
    trans.lastres = lastres;
    m.spwan_lists(trans.lastres);
    if (lastprm.isempty()) m.show_result(trans.lastres);
    else m.show_result(trans.lastres, lastprm);
  }
  if (eventnm.length > 0) {
    m.show_result(lastprm);
    let tr = res.get(d.ZDSPPAG);
    let ev = res.get(d.ZEVENTN);
    m.do_disp(tr, eventnm, null, ev);
  }
}
MRO.reset_basics = () => { MRO.basics.length = 0; }
MRO.add_css = p => {
  try {
    MRO.check_resp(p);
    const d = window.document, css = d.createElement("style");
    css.type = "text/css";
    css.innerHTML = p.get(DFS.ZFILERS);
    d.body.appendChild(css);
  }
  catch (e) {
    err.rescue(e)
  }
}
MRO.load_pos_css = (css, force) => {
  let p = MRO.make_fileprms('layout', css, 'CSS', force ? 'force' : '');
  MRO.clie.proxy('get_file', p.buffer(), true, MRO.add_css);
}
MRO.hide_pop_menu = () => { MRO.display($2('menupop').style, 0); }
MRO.hide_menu = () => {
  if (!MRO.menld) return;
  if (mcbtn) resetButton(mcbtn);
  mcbtn = null;
}
MRO.show_menu = () => {
  if (!MRO.ismain) return;
  let m = $2('menu'), hasmen = trans.menuitems && trans.menuitems !== '';
  if (hasmen && !MRO.menld) MRO.load('/web/', 'men.js', false);
  MRO.repHtml(m, hasmen ? trans.menuitems : DFS.NONMENU);
}
MRO.show_title = () => {
  let s = trans.name, i = MRO.ses.getint(DFS.ZINSTAN);
  if (i !== 0) s += ' ' + i
  window.document.title = s;
}
MRO.show_photo = () => {
  if (MRO.ismc) return;
  let d = DFS, u = MRO.ses.get(d.ZUSERID), p = $2('photo');
  if (u && u.length > 0) {
    p.src = ('/files/uphotos/_.jpg' + '?dummy=' + Math.random()).replace('_', u);
    p.onerror = function () { p.src = '/files/uphotos/usernotfound.jpg'; }
  } else p.src = CNS.imgs + 'company.png';
  MRO.ses.set('usrphot', p.src);
}
MRO.set_user = r => {
  if (MRO.ismc) return;
  let s = sessionStorage;
  s.setItem('usrname', r.get('usrname'));
  s.setItem('usrdesc', r.get('usrdesc'));
  s.setItem('usrcoms', r.get('usrcoms'));
  s.setItem('usrtype', r.get('usrtype'));
  s.setItem('usrlevl', r.get('usrlevl'));
}
MRO.set_css = r => {
  try {
    let m = MRO;
    m.check_resp(r);
    CUR.csd = r.get(DFS.ZFILERS);
    m.apply_css();
    m.set_lastdata();
  } catch (e) { err.rescue(e) };
}
MRO.handle_css = force => {
  let m = MRO, d = DFS, lay = m.ses.get(d.ZLAYOUT);
  if (lay.length === 0) lay = 'STD';
  if (CUR.css === lay) return;
  let p = m.make_fileprms('layout', 'css' + lay, 'CSS', force ? 'force' : '');
  m.clie.kernel('get_file', p.buffer(), true, m.set_css);
}
MRO.apply_css = () => {
  let m = MRO, c = CUR, d = DFS, s = m.ses;
  err.require(c.csd === '', 'css_not_found');
  let lay = s.get(d.ZLAYOUT);
  if (c.css === lay) return;
  c.css = lay;
  if (c.css.length === 0) c.css = 'STD';
  let wd = window.document;

  if (!m.custcss) {
    try {
      let s = wd.createElement('style');
      s.id = 'mrocss';
      s.type = 'text/css';
      s.innerHTML = c.csd;
      let os = wd.getElementById('mrocss');
      if (os) os.parentNode.replaceChild(s, os);
      else wd.getElementsByTagName('head')[0].appendChild(s);
    }
    catch (e) {
      alert(e.message);
    }
  }

  m.show_tbar();
  m.update_status();
}
MRO.set_lang = r => {
  try {
    let m = MRO;
    m.check_resp(r);
    m.txts = m.make(r.get('text_params'));
    m.errs = m.make(r.get('error_params'));
    m.apply_lang();
    m.move2trans(trans.library, trans.name, '', true, '');
    m.set_lastdata();
  }
  catch (e) {
    err.rescue(e)
  }
}
MRO.handle_lang = () => {
  let m = MRO, lang = m.ses.get(DFS.ZLANGUA);
  if (lang.length === 0) lang = 'EN';
  if (CUR.lng === lang) return;

  let p = new pairs();
  p.set(DFS.ZLANGUA, lang);
  m.clie.proxy('gui_get_texts', p.buffer(), true, m.set_lang);
}
MRO.apply_lang = () => {
  let m = MRO, c = CUR, lang = m.ses.get(DFS.ZLANGUA);
  if (c.lng === lang) return;
  c.lng = lang;
  if (c.lng.length === 0) c.lng = 'EN';

  let l = m.txts, d = m.stadsc;
  d.okay = l.get('status_ok');
  d.prog = l.get('status_prog');
  d.dwnl = l.get('status_trns_down');
  let f = (c, l, k) => { let a = $2(c); a.title = l.get(k); }
  if (!m.ismc) {
    f('bok_', l, 'enter');
    f('bhis', l, 'favorites');
    f('btsk', l, 'tasks');
    f('bbck', l, 'back');
    f('bout', l, 'out');
    f('bhom', l, 'home');
    f('bfrw', l, 'forward');
    f('bfnd', l, 'easy_access');
    f('bnwa', l, 'newsess');
    f('bsct', l, 'shortcut');
    f('bhlp', l, 'help');
    f('bdsg', l, 'design');
    f('bfav', l, 'favorites');
    f('binf', l, 'show_ses_info');
  }
  else f('bokm', l, 'enter');
  f('guiberr', l, 'show_err_info');
  f('guibwrn', l, 'show_wrngs_info');
  m.update_status();
}
MRO.handle_parms = l => {
  let m = MRO, p = PRM, a = {}; //url params
  l.search.replace(new RegExp("([^?=&]+)(=([^&]*))?", "g"), function ($0, $1, $2, $3) { a[$1] = $3; });
  p.entry = a['e'];
  p.cmpy = a['c'];
  p.shortcut = a['sc'];
  p.scprms = a['scp'];
  if (m.$undef(p.entry)) p.entry = '';
  if (m.$undef(p.cmpy)) p.cmpy = '';
  if (m.$undef(p.shortcut)) p.shortcut = '';
  p.scprms = m.$undef(p.scprms) ? '' : decodeURI(p.scprms);
}
MRO.certgen = () => '';
MRO.init_mc = (m, o, d) => {
  let mc = o.MRO.ses.get('matchcode');
  if (!mc || mc === '') return;

  // link with parent
  m.ses = o.MRO.ses;
  CUR.csd = o.CUR.csd;
  m.txts = o.MRO.txts;
  m.errs = o.MRO.errs;
  m.certgen = o.MRO.certgen;

  $2('bokm').onclick = () => { MRO.proc_enter(); }
  $2('guiberr').onclick = () => { execmd('serrinf'); }
  $2('guibwrn').onclick = () => { execmd('swrngs'); }

  m.load_pos_css('cssEXM', false);
  m.create_frame(d);
  m.move2trans('', mc, o.MRO.ctrls2match.buffer(), false, '');

  let p = o.MRO;
  if (p.dlgtype > -1 && p._action) {
    if (p._action !== '') m.send(p._action);
  }
  p._action = null;
}
MRO.post_init = s => {
  let m = MRO, d = DFS;
  m.check_resp(m.ses = s);

  m.eval_js(s.extract('certgen'));

  if (!m.fapp) {
    m.pass = new pairs();
    m.pass.set(d.LIBRARY, s.extract('library'))
    m.pass.set(d.ZFILERS, s.extract('xfile02'));
    m.pass.set(d.ZRTRNCD, s.extract('fstrncd'));
    m.pass.set(d.ZHISPOS, s.extract('fhispos'));
    m.txts = m.make(s.extract('text_params'));
    m.errs = m.make(s.extract('error_params'));
  }

  s.set(d.ZGUIAGN, MAC.brws);
  s.set(d.ZBRWTYP, MAC.mobi ? '1' : '0');
  s.set(d.ZMACADR, MAC.addr);

  if (MAC.name === '') MAC.name = s.get(d.ZMACNAM);
  CUR.csd = s.extract(d.ZFILERS);

  if (PRM.cmpy !== '') m.ses.set(d.ZCOMPNY, PRM.cmpy); //fbp

  let o = window.document;
  m.create_frame(o);
  m.first_screen();

  // sync client/server time
  let svrtime = new Date(s.get(d.ZDATTIM)), clitime = new Date();
  m.clkof = svrtime - clitime;

  let l = o.createElement('link');
  l.type = 'image/x-icon';
  l.rel = 'shortcut icon';
  l.href = CNS.imgs + 'mro.ico';
  o.getElementsByTagName('head')[0].appendChild(l);

  l = o.createElement('img');
  l.id = 'waiting';
  m.display(l.style, 0);
  l.src = '/files/gifs/progress_bar2.gif';
  $2('cmdbar').appendChild(l);
  trans.waiting = l.style;

  l = o.createElement('img');
  l.id = 'photo';
  l.onmouseenter = () => { m.show_usrinf(); };
  l.onmouseleave = () => { m.hide_usrinf(); };
  l.src = CNS.imgs.concat('company.png');
  m.ses.set('usrphot', l.src);
  $2('aphdr').appendChild(l);

  l = o.createElement('div');
  l.id = 'menupop';
  o.body.appendChild(l);

  $2('lgimg').src = CNS.imgs.concat('company.png');
  m.show_photo();

  $2('bok_').onclick = () => { MRO.proc_enter(); }
  let q = ['bhis', 'his', 'btsk', 'tasks', 'bbck', 'back', 'bout', 'out', 'bhom', 'home',
    'bfrw', 'forward', 'bfnd', 'easyacc', 'bnwa', 'newapp', 'bsct', 'shortcut', 'bhlp', 'help',
    'bdsg', 'design', 'bfav', 'addfav', 'guiberr', 'serrinf', 'guibwrn', 'swrngs', 'binf', 'ssesinf', 'fbid', 'shwcmp'];
  for (let i = 0, n = q.length; i < n; i += 2) $2(q[i]).onclick =
    (function (i) { return () => { execmd(i); } })(q[i + 1]);
}
MRO.pre_init = newsess => {
    let m = MRO, l = localStorage;
  if (newsess) { //only new session
      m.clie.rawcall(['fun=4&mac=', MAC.name, '&prm={"zvalues":{', MAC.winu,
          '"prms0":{"entryid":"', PRM.entry, '","zlayout":"', l.getItem('lstlayo'), '", "p_langu":"', l.getItem('lstlang'), '"},',
        '"fun0":"get_init_data","fun1":"gui_get_texts",',
        '"fun2":"get_first_state","fun3":"get_cert_gen"}}'].join(''),
      true, MRO.post_init);
    return;
  } 
  let p = parent.window.opener, o = p.MRO; //new app we connect
  m.pass = o.pass;
  let s = m.make(o.ses.buffer());
  m.txts = o.txts;
  m.errs = o.errs;
  m.certgen = o.certgen;
  s.set(DFS.ZFILERS, p.CUR.csd);
  m.post_init(s);
}
MRO.create_frame = d => {
  MRO.apply_css();
  MRO.display(d.body.style, 2);
  MRO.apply_lang();
}
MRO.first_screen = () => {
  let m = MRO, d = DFS, r = null, o = parent.window.opener;
  let gotot = new pairs(), extra = new pairs();
  if (o && o.MRO && o.MRO.fromclie && o.MRO.fromclie !== undefined &&
    o.MRO.fromclie.notempty()) {
    let f = o.MRO.fromclie, s = m.ses;
    r = m.make(f.get('datacli'));

    s.set(d.ZDOMAIN, f.get(d.ZDOMAIN));
    s.set(d.ZCOMPNY, f.get(d.ZCOMPNY));
    s.set(d.ZUSERID, f.get(d.ZUSERID));
    s.set(d.ZORIPAS, f.get(d.ZORIPAS));
    s.set(d.ZLANGUA, f.get(d.ZLANGUA));
    s.set(d.ZLAYOUT, f.get(d.ZLAYOUT));
    s.set(d.ZEMPLID, f.get(d.ZEMPLID));

    r.set(d.ZDOMAIN, f.get(d.ZDOMAIN));
    r.set(d.ZCOMPNY, f.get(d.ZCOMPNY));
    r.set(d.ZUSERID, f.get(d.ZUSERID));
    r.set(d.ZORIPAS, f.get(d.ZORIPAS));
    r.set(d.ZLANGUA, f.get(d.ZLANGUA));
    r.set(d.ZLAYOUT, f.get(d.ZLAYOUT));
    r.set(d.ZMACADR, s.get(d.ZMACADR));

    let a = f.get('zjsmodz');
    if (a !== '') m.lmod(a, false);
    if (f.ison('specific')) {
      gotot.set(d.ZRTRNCD, f.get(d.ZRTRNCD));
      extra.set(d.ZRTRNPR, f.get(d.ZRTRNPR));
    }

    r = m.clie.proxy('copy_session', r.buffer());
    s.set(d.ZDATTIM, r.get(d.ZDATTIM));

    m.check_resp(r);
    m.clie.start_session(r);
    if (!m.debug) parent.window.opener = null; //security
    m.load_cmpy();
    m.update_status();
  }
  else m.check_resp(r = m.pass); //pass-trans

  if (gotot.notempty()) {
    let p = extra.get(d.ZRTRNPR);
    m.restore_state(gotot, p);
    let sp = m.make(extra.get(d.ZRTRNPR));
    if (sp.notempty()) {
      p = sp.get(d.ZDOEVNT);
      if (p.length > 0) m.send(p);
    }
  }
  else m.restore_state(r, '');

  // company is set by caller else we use the last used
    if (m.islogon()) {
        let l = localStorage;
        $2('$company$').value = PRM.cmpy !== ''? PRM.cmpy : l.getItem('lstcmpy');
        $2('$_user$').value = l.getItem('lstuser');
        $2('$_lang$').value = l.getItem('lstlang');
        $2('$layout').value = l.getItem('lstlayo');
  }

  m.show_divs();
  m.reset_basics();
}
MRO.show_divs = () => {
  let m = MRO;
  if (!m.ismain) return;
  let S050 = m.are_in('S050');
  m.display($2('cmdbar').style, S050 || m.are_in('S070', 'S090') ? 0 : 2);
  m.display($2('inpbar').style, S050 ? 0 : 2);
}
MRO.proc_enter = () => {
  let m = MRO, cmd = $2('command');
  if (m.lstinp) { // nsgh - clean previous input error
    let s = m.lstinp.style;
    s.background = m.lststy[0];
    s.color = m.lststy[1];
    m.lstinp = null;
  }
  if (cmd !== null) {
    if (!m.chk_inp(cmd.value, /*'^[a-zA-Z0-9 _\/]*$'*/'^[a-zA-Z0-9 _\/()\.]*$')) {
      m.ldm(typeof ERR, 'err');
      ERR.proc_color(cmd);
      err.requireex(true, 'incorrect_input', 'command');
    }
    let code = cmd.value;
    cmd.value = '';
    if (code.length > 0) {
      m.cmdh.push(code);
      let c = code.split(' ');
      execmd(c[0], c[1] ? c[1] : '', c[2] ? c[2] : '');
      return;
    }
  }
  let lid = m.curr_list, wo = m.iswaitforsel();
  if (lid !== null && wo !== null) m.select_list_item(wo, lid, m.curr_row);
  else if (m.isincapt) m.do_list(linfo[lid].captype);
  else {
    if (m.islogon()) m.first_tree = true;
    m.send('onenter');
  }
}
MRO.show_tbar = () => {
  let m = MRO, p = 'url('.concat(CNS.imgs, 'mrostdbar.png) ?px 0px no-repeat');
  let a = m.ismc ? ['bokm', 'guiberr', 'guibwrn'] :
    ['bok_', 'bhis', 'bbck', 'bout', 'bhom', 'bfrw', 'bfnd', 'bnwa', 'bsct', 'bhlp',
      'bdsg', 'guiberr', 'guibwrn', 'binf', 'btsk', 'bfav'];
  let b = m.ismc ? ['0', '-176', '-192'] :
    ['0', '-16', '-32', '-48', '-64', '-80', '-96', '-112', '-128', '-144',
      '-160', '-176', '-192', '-208', '-224', '-240'];
  //if (!m.ismc) $2('bok_').style.background = 'url(/pics/EXEC.png)';
  for (let i = 0, n = a.length; i < n; ++i) $2(a[i]).style.background = p.replace('?', b[i]);
}
MRO.check_error = (err, code) => {
  let einf = MRO.make(err.get(DFS.ZERRORI));
  return einf.has(DFS.ZCERROR) && einf.are_eq(DFS.ZCERROR, code);
}
MRO.on_beforeunload = e => {
  try {
    if (MRO.skipunld === 1) {
      MRO.skipunld = 0;
      return false;
    } // ???
    /*if (MRO.done)*/ MRO.unload_trans();
  } catch (e) { }; // whatever happens we need endsession
  if (MRO.ismain) MRO.end_session();
}
MRO.on_load = () => {
  let m = MRO, o = parent.window.opener, d = window.document;
  let fromclie = false;
  try {
    if (o.MRO.dlgtype !== -1) fromclie = true;
  }
  catch (e) { }
  if (!fromclie) parent.window.opener = null;
  m.fapp = fromclie;
  try {
    let l = window.location, u = l.toString();
    m.clie.bur = 'http://'.concat(l.hostname, ':', l.port, '/');
    m.clie.url = m.clie.bur.concat(DFS.ZPROXYP);
    m.ismc = u.indexOf('?matchcode') !== -1;
    if (m.ismc) m.init_mc(m, o, d);
    else {
      m.load_pos_css('cssEXT', false);
      m.ismain = true;
      m.handle_parms(l);
      m.pre_init(!fromclie);
    }
    window.addEventListener("beforeunload", MRO.on_beforeunload);
    m.load('/web/', 'extra.js', true);

    if (m.ismain) {
      m.lmod('tre', false/*true*/);
      m.lmod('bar', true);
      m.lmod('ftr', true);
    }
    m.lmod('lst', true);

    d.onkeydown = m.KeyCheck;
    d.onkeyup = m.keyup;
  }
  catch (e) {
    m.display(d.body.style, 2);
    err.rescue(e)
  }
}
MRO.litem = (l, r, i) => CNS.zl[l] + r + i;
MRO.listncols = lid => linfo[lid]._d_;
MRO.fixtbl = () => {
  setTimeout(() => {
    for (let i = 0; i < trans.listmax; ++i) MRO.adjustdivtbl(i);
  }, 64);
}
MRO.adjustmc = () => {
  let f = $2(CNS.frms[0]);
  if (!f) return;
  let els = f.elements;/*f.childNodes*/;
  let len = f.length;/*els.length*/;
  let w = 0, h = 0;
  for (let i = 0; i < len; ++i) {
    let el = els[i];
    if (el.nodeType !== 1) continue;//elements only
    if (el.tagName.length === 2) continue;//BR
    if (el.type === 'hidden') continue;
    let s = el.getBoundingClientRect();
    if (s.right > w) w = s.right;
    if (s.bottom > h) h = s.bottom;
  }
  w += 100, h += 160;
  window.resizeTo(w, h);
}
MRO.adjustdivtbl = lid => {
  let l = $2('list' + lid);
  if (!l) return false;
  let a = l.clientWidth;
  if (a === 0) return false;
  let b = $2('divtbl' + lid);
  if (!b) return false;
  b.setAttribute('style', 'display:block;width:' + a + 'px');
  b.style.width = a + 'px';
  let c = $2('ztotctrl' + lid);
  if (c) {
    let l = a - 218;
    c.style.width = (l < 0 ? 0 : l) + 'px';
  }
  c = $2('ztotslst' + lid);
  if (c) {
    let l = a - 298;
    c.style.width = (l < 0 ? 0 : l) + 'px';
  }

  if (!MAC.mobi && (MRO.are_in('S020', 'S030', 'S025'))) MRO.adjustmc(); // fbw

  return true;
}
MRO.pre_list = lid => {
  let m = MRO, d = window.document, iid = parseInt(lid);
  if (!d.getElementById(CNS.zlst[iid])) return;
  let linf = linfo[iid];
  let rowptbl = d.getElementById('rowspertable'.concat(lid));
  let rowspertable = rowptbl ? rowptbl.value : 10;
  linf.rwspag = parseInt(rowspertable);
  let page = 1;
  let r_ini = (page - 1) * rowspertable;
  let r_fin = (page) * rowspertable;

  let colg = $2(CNS.zclg[iid]);
  let ncols = linf._d_ = !colg ? 0 : colg.span;
  let a = linf._a_, b = linf._b_, c = linf._c_, f = linf._f_;
  let helper = CNS.zl[iid];
  let cars = CARS;
  for (let r = 0, i = r_ini; i < r_fin; ++i, ++r) {
    let lr = helper + r;
    if (a.length <= r) a[r] = [];
    let aa = a[r];
    let j = ncols - 1; do {
      aa[j] = d.getElementById(lr.concat(cars[j]));
    } while (j--);
    b[r] = d.getElementById(lr.concat('itm'));
    c[r] = d.getElementById(lr.concat('img'));
    f[r] = c[r].style;
  }
  if (m.ismain) m.adjustdivtbl(lid);
}
MRO.clean_css_row = s => {
  s.removeProperty('color');
  s.removeProperty('background-color');
}
MRO.set_css_list = (aa, c) => {
  let v = aa.value;
  let z = v.replace(/[^0-9.-]+/g, "");
  z = z.length > 0 ? parseInt(Number(z)) : NaN;
  let s = aa.style;
  //MRO.clean_css_row(s);
  for (let i = 0, n = c.length; i < n; ++i) {
    let inf = c[i];
    if (!inf) continue;
    let o = inf.oper;
    let f = inf.value;
    let g = inf.numb;
    let p = '';

    if (!isNaN(z) && !isNaN(g)) p = "".concat(z, o, g);
    else p = "'".concat(v, "'", o, "'", f, "'");

    if (eval(p)) {
      if (inf.className !== '') aa.className = inf.className;
      if (inf.color !== '') s.color = inf.color;
      if (inf.backgroundColor !== '') s.backgroundColor = inf.backgroundColor;
    }
  }
}
MRO._fill_list = (lid, result) => {
  let m = MRO, linf = linfo[lid];
  let rowsxpage = linf.rwspag;
  let rncls = result.getint(CNS.zlyc[lid]);
  if (rncls > 0) {
    let typ = trans.qrylayout[lid];
    if ((typ === 1 && (linf.curpag === -1 || linf.curpag === 0)) || typ === 2) {
      m.new_list(result, lid, rowsxpage, rncls);
      m.pre_list(lid);
    }
  }

  // if have list row selection we close it to better working
  let selrow = null;
  if (m.islsel/* && m.curr_row*/) {
    selrow = m.curr_row;
    m.close_sel();
  }

  const ncols = m.listncols(lid);
  const nrows = result.getint(CNS.tots[lid]);
  let pages = ~~(nrows / rowsxpage);
  let rem = nrows % rowsxpage;
  pages += rem > 0 ? 1 : 0;//one more page if extras
  pages += rem === 0 && linf.has_ins ? 1 : 0;
  linf.lstpag = pages;

  if (linf.curpag === -1) linf.curpag = 1;
  if (linf.curpag === 0) linf.curpag = 1;
  if (linf.curpag > pages) linf.curpag = pages;
  let page = linf.curpag;

  let ini = (page - 1) * rowsxpage;
  let fin = (page) * rowsxpage; //range 2 show
  let helper = CNS.zl[lid];
  let images = trans.l_imgs[lid];
  let lastitm = null, lastimg = null;
  let cars = CARS, a = linf._a_, b = linf._b_, c = linf._c_, f = linf._f_;
  let img, imgid, lstimg = -1, simg;
  if (page === 0 && pages === 0) {
    ini = 0;
    fin = rowsxpage;
  } //clean list

  // detect if wee need to work on list colors by user
  const collst = trans.l_csrw[lid];
  const setcss = collst.length > 0;
  const colusr = trans.l_csus[lid];
  const setusr = colusr.length > 0;
  const clncss = setcss || setusr;

  for (let r = 0, i = ini, z = i + 1; i < fin; ++i, ++r, ++z) {
    let data_row = helper + i;
    let aa = a[r], j = ncols - 1, h = j;
    img = c[r];

    if (i < nrows) { // proc visible pages
      do {
        aa[j].value = result.get(data_row.concat(cars[j]));
        if (clncss) m.clean_css_row(aa[j].style);
        if (setcss && collst[j]) m.set_css_list(aa[j], collst[j]);
        if (setusr && colusr[j]) m.set_css_list(aa[j], colusr[j]);
      } while (j--);
      b[r].value = z;
      f[r].visibility = 'visible';
      imgid = result.get(data_row.concat('*'));
      if (imgid.length > 0) {
        let ii = parseInt(imgid);
        if (lstimg !== ii) {
          lstimg = ii;
          simg = images[lstimg];
        }
        img.src = simg;
      }
      continue;
    }
    do { // clean remain last rows
      aa[h].value = '';
      if (clncss) m.clean_css_row(aa[h].style);
    } while (h--);
    b[r].value = '';
    f[r].visibility = 'hidden';
    if (i === nrows) lastitm = b[r], lastimg = img;
  }

  if (lastitm && linf.has_ins) {
    lastimg.src = images[0];
    lastimg.style.visibility = 'visible';
    lastitm.value = '*';
  }

  let tr = linf.c_trow;
  if (tr) tr.value = nrows;
  let tp = linf.c_tpag;
  if (tp) tp.value = pages;
  m.show_linfo(lid, 0);
  linf.nrows = nrows;

  // if had list row selection we restore it
  if (selrow !== null) on_lclick(lid, selrow);

  //row(*) need to be visible/sometime lastrow was out of nrows (deletion cases)
  let bound = linf.nrows;
  bound += linf.has_ins ? 1 : 0;
  //if (m.curr_real === '*') m.close_cap();//m.handle_esc();
  if (parseInt(m.curr_real) >= bound) {
    let l = (linf.nrows + (linf.has_ins ? 1 : 0)) % linf.rwspag;
    if ((l - 1) < 0) m.close_cap();//m.handle_esc();
    else on_lclick(lid, l - 1);
  }
}
MRO.has_fun = fun => {
  let js = trans.json;
  if (!js.has(fun)) return false;
  let f = MRO.make(js.get(fun));
  return f.notempty();
}
MRO.show_linfo = (lid, row) => {
  let m = MRO, lf = linfo[lid], r = lf.c_crow, p = lf.c_cpag;
  if (r) {
    let i = $2(m.litem(lid, row, 'itm'));
    if (i) r.value = i.value;
  }
  if (p) p.value = lf.curpag;
}
MRO.mousewheel = e => {
  if (MRO.islsel) { //at this moment only selection
    var e = e || window.event;
    let up = Math.max(-1, Math.min(1, (e.wheelDelta || -e.detail))) === 1;
    if (up) {
      if (e.ctrlKey) move_page_up();
      else move_row_up();
    }
    else
      if (e.ctrlKey) move_page_down();
      else move_row_down();
  }
  //let t = e.target, rx = /INPUT|SELECT|TEXTAREA|LI/i;
  //if (!rx.test(t.tagName) || t.disabled || t.readOnly) e.preventDefault();
  e.preventDefault();
}
MRO.get_doloc_prms = (vls, pms) => {
  let r = new pairs(), p = new pairs();
  p.attach(pms.get('parameters')); // mostly trans values
  let v = MRO.make('{' + vls + '}');
  r.merge(p, MRO.make(v.get('zvalues')));
  p.attach(pms.get('guivalues')); // mostly front-end values
  for (let k in p._) {
    let v = MRO.get_val_of(p.get(k));
    if (v !== '') r.set(k, v);
  }
  return r;
}
MRO.dolocal = (cmd, values, j, parent) => {
  let p = MRO.get_doloc_prms(values, j);
  let x = p.get('executor');
  window[x === '' ? 'execmd' : x](cmd, p);
  //execmd(cmd, MRO.get_doloc_prms(values, j));
  if (j.has('after')) {
    let after = MRO.make(j.get('after'));
    for (let i = 0, n = after._.length; i < n; ++i) {
      let ev = after._[i];
      if (ev !== parent) MRO.send(ev); // avoid recursion
    }
  }
}
handle_ws = (f2call, prms) => {
  let fname = null, fprms = null;
  if (f2call.slice(0, 10) === 'dispatch') fname = f2call.slice(10);
  else fname = f2call;
  let ini = fname.indexOf("('");
  if (ini !== -1) {
    let fin = fname.indexOf("')", ini + 1);
    if (fin !== -1) {
      let t = fname;
      fname = t.slice(0, ini - 1);
      fprms = t.slice(ini + 2, fin - 1);
    }
  }
  if (fname === 'javascript') {
    MRO.eval_js(prms.get('text'));
    return;
  }
  if (trans.json.has(fname)) {
    let j = trans.json.get(fname);
    if (j !== '') {
      let p = MRO.make(j);
      let chk = MRO.make(p.get('notempty'));
      for (let k in chk._) {
        let el = $2(k);
        if (el && el.value === '') {
          MRO.ldm(typeof ERR, 'err');
          ERR.proc_color(el);
          err.require(true, ERR.get_desc(chk._[k]));
        }
      }
    }
    if (MRO.do_goto(fname, j)) return;
  }
  if (prms) MRO.send(fname, prms);
  else MRO.send(fname);
}
MRO.ldm = (a, b) => {
  if (a === 'undefined')
    MRO.load('/web/', b + '.js', false);
}
MRO.lmod = (a, c, r, db) => {
  let m = MRO.lmods, b = a.toUpperCase(), d = r !== undefined && r;
  if (d) for (let i = 0; i < m.length; i++) if (m[i] === b) m.splice(i, 1);
  if (!$4(m, b)) {
    if (db !== undefined && db) {
      try {
        let p = MRO.make_fileprms('trans', a, 'JSC', true ? 'force' : '')
        MRO.clie.proxy('get_file', p.buffer(), true, MRO.set_script), m.push(b);
      }
      catch (e) {
        err.rescue(e)
      }
    }
    else MRO.load('/web/', a + '.js', c), m.push(b);
  }
}
let bbef = ['CALL', 'LOAD', 'NEWAPP', 'HELP', 'ADDLIBLE', 'DLTLIBL'];
let fbef = ['SSESINF', 'SERRINF', 'SWRNGS'];
MRO._fns = (c, p0, p1, p2, p3, p4) => {
  let m = MRO;
  if ($4(bbef, c)) {
    if (BAR._fns(c, p0, p1, p2, p3, p4))
      return;
  }
  if ($4(fbef, c)) {
    if (FTR._fns(c, p0, p1))
      return;
  }
  //  err.require(!m.clie.isactive(), DFS.EOPOUSE);
  if (m.appyf(c, p0, p1)) return;
  err.require(!m.clie.isactive(), DFS.EOPOUSE);
  m.ins_trans(c, null, false);
}
MRO.appyf = (c, p0, p1) => {
  let a = MRO.lmods;
  for (let i in a)
    if (MRO.applym(a[i], c, p0, p1)) return true;
  return false;
}
MRO.applym = (a, c, p0, p1) => {
  let m = MRO, b = window[a];
  if (m.$undef(b)) return false;
  if (m.$undef(b._fns)) return false;
  return b._fns(c, p0, p1);
}
execmd = (c, p0, p1) => {
  try {
    MRO._fns(c.toUpperCase(), p0, p1);
  }
  catch (e) {
    err.rescue(e)
  }
};
execmd2 = (c, p) => { execmd(c, p.get('p0'), p.get('p1')); };
MRO.do_local = res => {
  let m = MRO, lp = m.make(res.get('pass_dlg_2_tran'));
  if (lp.notempty()) {
    let o = parent.window.opener;
    let vr = o.MRO.cols2vars.get("to0");
    if (vr.length > 0) {
      let v = o.MRO.cols2vars.get("from0");
      let rv = m.get_val_of(v);
      o.MRO.set_val_of(vr, rv);
    }
  }
  else if (res.has('dolocal')) handle_ws(res.get('dolocal'));
  else {
    let a = res.get('action');
    let p = m.make(res.get('parameters'));
    if (a.length > 0) execmd(a, p);
  }
  if (parent.window.opener) parent.window.opener.MRO.close_match();
}
(function () {
  let m = MRO, c = CNS, o = parent.window.opener, z = MAC;
  m.sb = $2('statusbar'), m.sbs = m.sb.style, m.ge = $2('guiberr').style, m.gw = $2('guibwrn').style;

  for (let i = 0; i < CNS.NVARS; ++i) {
    let s = i.toString();
    c.limg.push('img'.concat(s)); c.edis.push('edi'.concat(s));
    c.adds.push('add'.concat(s)); c.adbs.push('addb'.concat(s));
    c.cols.push('col'.concat(s)); c.vars.push('var'.concat(s));
    c.zhis.push('zhis'.concat(i < 10 ? '0' : '', s, 'z'));
  }

  if (typeof String.prototype.trimLeft !== 'function')
    String.prototype.trimLeft = function () { return this.replace(/^\s+/, ""); }
  let ft = true;

  try {
    ft = (typeof o === 'undefined') || o === null ||
      (typeof o.MRO === 'undefined') || o.MRO === null;
  }
  catch (e) {}

  if (ft) {
    try {
      m.ax = new ActiveXObject("WScript.Shell");
    }
    catch (e) {}

    try {
      z.name = new ActiveXObject("WScript.Network").ComputerName;
    }
    catch (e) {}

    try {
      let a = new ActiveXObject("WbemScripting.SWbemLocator");
      let b = a.ConnectServer(".");
      let c = b.ExecQuery("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled=TRUE");
      z.addr = new Enumerator(c).item(0).MACAddress;
    }
    catch (e) {}

    z.brws = navigator.userAgent.indexOf('Edge') != -1 ? '2' : '1'; //IE/Chrome

    if (m.ax !== null)
      z.winu = '"prms0":{"'.concat(DFS.ZWINUSR, '":"', MRO.ax.ExpandEnvironmentStrings("%username%"),
        '","', DFS.ZWINDOM, '":"', MRO.ax.ExpandEnvironmentStrings("%userdomain%"), '"},');
    z.mobi = /Android|webOS|iPhone|iPad|iPod|BlackBerry|BB|PlayBook|IEMobile|Windows Phone|Kindle|Silk|Opera Mini/i.test(navigator.userAgent);
    return;
  }

  z.name = o.MAC.name; z.addr = o.MAC.addr; z.brws = o.MAC.brws;
  z.winu = o.MAC.winu; z.mobi = o.MAC.mobi; m.debug = o.MRO.debug;
})();