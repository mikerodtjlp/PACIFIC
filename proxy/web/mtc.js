var MTC={};
MTC.get_focus_name = function () {
  let el = window.document.activeElement;
  return el ? el.id : '';
}
MTC.insert_date=function(){
    let curfield=MTC.get_focus_name();
    let cflen=curfield.length;
    let frms=CNS.frms;
    for(let i=0,caplen=frms.length;i<caplen;++i){
        let f=$2(frms[i]);
        if(!f)continue;
        let el=f.firstChild;
        do{
            let id=el.id;
            if(id===undefined)continue;
            let idl=id.length;
            if(idl===0)continue;
            let clsnm=el.className;
            let clsnl=clsnm.length;
            if(clsnl===8&&clsnm==='mroinput'&&idl===cflen&&id===curfield&&
                el.size === 10 && el.placeholder==='9999/99/99'){
                let t=new Date();
                let d=t.getDate();
                let m=t.getMonth() + 1;
                let y=t.getFullYear();
                if(d<10)d='0'+d;
                if(m<10)m='0'+m;
                el.value=y+'/'+m+'/'+d;
                return;
            }
        }while(el=el.nextSibling);
    }
}
MTC.match_codes=function(seek){
    let curfield=MTC.get_focus_name();
    let cflen=curfield.length;
    let found=false;
    let d=window.document,frms=CNS.frms;
    for(let i=0,n=frms.length;i<n;++i){
        let f=d.getElementById(frms[i]);
        if(!f)continue;
        let el=f.firstChild;
        do{
            let id=el.id;
            if(id===undefined)continue;
            let clsnm=el.className;
            let clsnl=clsnm.length;
            if(!found&&clsnl===8&&clsnm==='mroinput'&&id.length===cflen&&id===curfield){
                found=true;
                continue;
            }
            if(found){
                if(seek)MRO.send('seek_'+MRO.rpl_all(curfield,'$',''));
                else
                if(((clsnl===11&&clsnm==='mrobtnmatch')||(clsnl===9&&clsnm==='mrobtnimg'))
                    &&el.onclick)el.onclick();
                return;
            }
        }while(el=el.nextSibling);
    }
}
MTC.seek_codes=function(){MTC.match_codes(true);}
