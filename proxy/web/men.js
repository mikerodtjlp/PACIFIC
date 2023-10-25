var mcbtn=null;
MRO.menu={
getContainerWith:function(node,tagName,className){
  while(node){
    if(node.tagName&&node.tagName===tagName&&
      MRO.menu.hasClassName(node,className)) return node;
    node=node.parentNode;
  }
  return node;
},
hasClassName:function(el,name){
  let a=el.className.split(" ");
  for(let i=0,n=a.length;i<n;++i)if(name.localeCompare(a[i])===0)return true;
  return false;
},
removeClassName:function(el,name){
  if(!el.className)return;
  var l=[],a=el.className.split(" ");
  for(let i=0,n=a.length;i<n;++i)if(name.localeCompare(a[i])!==0)l.push(a[i]);
  el.className=l.join(" ");
},
getPageOffsetLeft:function(el){
  let x=el.offsetLeft;
  if(el.offsetParent)x+=MRO.menu.getPageOffsetLeft(el.offsetParent);
  return x;
},
getPageOffsetTop:function(el){
  let y=el.offsetTop;
  if(el.offsetParent)y+=MRO.menu.getPageOffsetTop(el.offsetParent);
  return y;
},
pageMousedown:function(event){
  if(!mcbtn)return;
  let el=(event.target.tagName?event.target:event.target.parentNode);
  if(el===mcbtn)return;
  if(MRO.menu.getContainerWith(el,"DIV","menu")===null){
    resetButton(mcbtn);
    mcbtn=null;
  }
},
closeSubMenu:function(m){
  if(!m||!m.activeItem)return;
  if(m.activeItem.subMenu){
    MRO.menu.closeSubMenu(m.activeItem.subMenu);
    m.activeItem.subMenu.style.visibility="hidden";
    m.activeItem.subMenu=null;
  }
  MRO.menu.removeClassName(m.activeItem,"menuItemHighlight");
  m.activeItem=null;
}};
window.document.onmousedown=MRO.menu.pageMousedown;
buttonClick = (event,menuId) => {
  let b=event.currentTarget;
  b.blur();
  if(!b.menu)b.menu=$2(menuId);
  if(mcbtn)resetButton(mcbtn);
  if(b!==mcbtn){
    depressButton(b,65,15);
    mcbtn=b;
  }else mcbtn=null;
  return false;
}
buttonMouseover = (event,menuId) => {
  let b=event.currentTarget;
  if(mcbtn&&mcbtn!==b)buttonClick(event,menuId);
}
depressButton = (b,offx,offy) => {
  b.className+=" menuButtonActive";
  let x=MRO.menu.getPageOffsetLeft(b)-offx;
  let y=MRO.menu.getPageOffsetTop(b)-offy+b.offsetHeight;
  s=b.menu.style;
  s.left=x+"px";
  s.top=y+"px";
  s.visibility="visible";
}
resetButton = b => {
  MRO.menu.removeClassName(b,"menuButtonActive");
  let m=b.menu;
  if(m){MRO.menu.closeSubMenu(m);m.style.visibility="hidden";}
}
menuMouseover = event => {
  let m=event.currentTarget;
  if(m.activeItem)MRO.menu.closeSubMenu(m);
}
mimo = (e, menuId) => {//menuItemMouseover
  let i = e.currentTarget;
  let menu = MRO.menu.getContainerWith(i, "DIV", "menu");
  if (menu.activeItem) MRO.menu.closeSubMenu(menu);
  menu.activeItem = i;
  i.className += " menuItemHighlight";
  if (!i.subMenu) i.subMenu = $2(menuId);
  let x = MRO.menu.getPageOffsetLeft(i) + i.offsetWidth - 75;
  let y = MRO.menu.getPageOffsetTop(i) - 24;
  let maxX = window.scrollX + window.innerWidth;
  let maxY = window.scrollY + window.innerHeight;
  let d = window.document.documentElement;
  maxX = d.scrollLeft + d.clientWidth;
  maxY = d.scrollTop + d.clientHeight;

  let isub = i.subMenu;
  if (isub) {
    maxX -= isub.offsetWidth;
    maxY -= isub.offsetHeight;
    if (x > maxX) x = Math.max(0, x - i.offsetWidth - isub.offsetWidth
      + (menu.offsetWidth - i.offsetWidth));
    y = Math.max(0, Math.min(y, maxY));

    let s = isub.style;
    MRO.display(s, 0);
    s.left = x + "px";
    s.top = y + "px";
    s.visibility = "visible";
    MRO.display(s, 2);
  }
  if (!e) e = window.event;
  e.cancelBubble = true;
  if (e.stopPropagation) e.stopPropagation();
};
(function () { MRO.menld = true; })();