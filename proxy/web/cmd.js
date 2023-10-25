var CMD={mdiv:null,x_pos:0,y_pos:0,x_elem:0,y_elem:0,mhcm:null};
CMD._fns=function(code,pms){
  let f=CMD[code],r=typeof f==="function";
  if(r)f(pms);
  return r;
}
CMD.drag_init=(elem)=>{
  let m=MRO,c=CMD;
  c.x_elem=c.x_pos-elem.offsetLeft;
  c.y_elem=c.y_pos-elem.offsetTop;
  if(c.y_elem<c.mhcm.offsetTop)c.mdiv=elem;
}
CMD.destroy=()=>{CMD.mdiv=null;}
CMD.move_elem=(e)=>{
  let m=MRO,c=CMD,d=window.document;
  c.x_pos=d.all?window.event.clientX:e.pageX;
  c.y_pos=d.all?window.event.clientY:e.pageY;
  let a=c.mdiv;
  if(a!==null){
    a.style.left=(c.x_pos-c.x_elem)+'px';
    a.style.top=(c.y_pos-c.y_elem)+'px';
  }
}
CMD.proc_cmd=(c)=>{
  if(c==='')return'';
  if(c==='cls'){$2('hiscmd').value='';return'';}
  if(c==='exit'){execmd('cmdbox','');return'';}
  return c;
}
CMD.CMDBOX=(pms)=>{
  let m=MRO,d=window.document,s=$2('cmdbox').style;
  let show=pms==='1';
  if(!show){d.removeEventListener("mousemove",CMD.move_elem);s.display='none';return;}
  d.addEventListener("mousemove",CMD.move_elem);
  s.display='block';
  let h=CMD.mhcm,i=$2('hiscmd');
  i.scrollTop=h.scrollHeight;
  i.focus();
  i.setSelectionRange(i.value.length,i.value.length);
}
CMD.pusherr = (t) => { $2('hiscmd').value += '\n' + t + '\n'; }
(function(){
  let m=MRO,d=window.document,css=d.createElement("style");
  css.type="text/css";
  css.innerHTML='#cmdbox{'.concat(
  'box-shadow:0px 0px 73px 0px rgb(128,128,128);',
  'border-radius:4px;',
  'background-color:gray;',
  'display:none;',
  'box-shadow:0px 3px 16px 0px rgb(64,64,64);}',
'#hiscmd{',
  'font-family:Verdana;',
  'font-size:10px;',
  'border-style:none;',
  'border-radius:4px;',
  'position:absolute;',
  'left:0px;',
  'top:16px;',
  'width:100%;',
  'height:100%;',
  'background-color:lightgray;',
  'resize:none;',
  'box-shadow:0px 3px 16px 0px rgb(64,64,64);}');
  d.body.appendChild(css);

  let a=$2('cmdjs');
  if(a!==null)return;
  let l=d.createElement('img');
  l.id='cmdjs';
  l.src=CNS.imgs+'tbviewcode.png';
  l.style.cursor = 'pointer';
  l.onclick=function(){execmd('cmdbox','1')};
  $2('tbox').appendChild(l);

  let f=d.createDocumentFragment();
  l=CMD.mhcm=d.createElement('div');
  l.id='cmdbox';
  y=l.style;
  y.position='absolute';
  y.top='360px';
  y.left='600px';
  y.width='240px';
  y.height='16px';
  l.innerHTML='<textarea spellcheck="false"id="hiscmd"rows="4"cols="50"style="height:200px;"></textarea>';
  f.appendChild(l);
  d.body.appendChild(f);

  d.onmouseup=CMD.destroy;
  l.onmousedown=function(){CMD.drag_init(this);};
  CMD.CMDBOX('1');
})();
