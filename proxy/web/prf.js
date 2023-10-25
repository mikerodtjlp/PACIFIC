var PRF = { status: 0 };
PRF.show_usrinf = () => {
  if (!MRO.clie.isactive()) return;
  if (PRF.status !== 0) return;
  PRF.status = 1;
  let a = $2('prof'), b = a.style;
  if (b.display === 'none') {
    let t = $2('photo').getBoundingClientRect();
    b.top = t.bottom + 'px';
    b.left = t.right + 'px';
    b.width = '400px';
    b.height = '250px';
    b.removeProperty('overflow');
    b.removeProperty('background');
    b.removeProperty('border');
    MRO.load_html('USRPRF');
  }
  PRF.status = 2;
};
PRF.hide_usrinf = () => {
  if (!MRO.clie.isactive()) return;
  if (PRF.status !== 2) return;
  let a = $2('prof'), b = a.style;
  MRO.display(b, 0);
  PRF.status = 0;
};
(function(){
  let d=window.document,css=d.createElement("style");
  css.type="text/css";
  css.innerHTML='#profhbg{'.concat(
  'width:400px;',
  'height:120px;',
  'border-radius:4px 4px 0px 0px;}',
  '#profhim{',
  'position:absolute;',
  'top:80px;',
  'left:10px;',
  'width:90px;',
  'height:98px;',
  'border-radius:4px;',
  'border-width:10px;',
  'border:solid;',
  'border-color:#DDDDDD;}',
  '#profb{font-size:12px;}',
  '#proff{',
  'border:1px solid #AAAAAA;',
  'width:399px;',
  'height:35px;',
  'background-color:rgb(220,220,202);',
  'font-size:10px;}',
  '.pb{position:relative;left:120px;margin:6px;}',
  '.pf{position:relative;top:10px;}');
  d.body.appendChild(css);
})();
