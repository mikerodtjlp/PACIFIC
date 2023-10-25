var DLG={};
DLG.show_dialog = (code,send,rcv,dlgt,prms,act) => {
  let m = MRO;
  m.close_match();

  if (code === 'lookforfile'){
    let r=new pairs();
    if (rcv) r.attach(rcv);
    let v=r.get('col1');
    if(!v||v.length===0)return false;
    let f=$2(v);
    if(!f)return false;
    let x=$2('file_upload_form');
    if(!x)x=m.creupld();
    if(x)x.reset();
    let t=$2('upload_target');
    if(t)t.innerHTML="";
    let f2=$2('_file');
    f2.click();
    f2.onchange=function(){//FIX name
      let s=f2.value,i=s.indexOf('fakepath\\');
      if(i!==-1)f.value=s.substr(i+9);//chrome
      else{
        let j=s.lastIndexOf('\\');
        if(j!==-1)f.value=s.substr(j+1);//ie
      }
    };
    return true;
  }

  m.ses.set('matchcode',code);
  let vls = m.gen_values(false);
  let res = new pairs();
  res.attach(send);
  let src = new pairs();
  src.attach(send);
  res.merge(src,vls);

  m.ctrls2match = res;
  m.cols2vars.attach(rcv ? rcv : CNS.NOPAR);
  m._action = act ? act:'';
  m.dlgtype = dlgt;

  let t = window.screenY + 96;//(t.height - 400) / 2;
  let l = window.screenX + 96;
  let w = 500, h = 540;

  m._match = window.open(m.clie.bur+'web/pac.html?matchcode','_blank',
    'directories=no,location=0,menubar=0,status=0,toolbar=0,resizable=1,width='
    + w + ',height=' + h +',top='+ t +',left=' + l);

  if (m._match.focus) m._match.focus();
};