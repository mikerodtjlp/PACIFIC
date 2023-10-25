var SCK={
execute:function(url,data,cb){
    let s=new WebSocket(url);
    s.onmessage=function(event){
        let r=event.data;
        //r=MRO.mro2json(r);
        let res=new pairs(r);
        s.close();
        if(cb===undefined)MRO.handle_response(res);else cb(res);
    }
    s.send(data);
}}
/*function process(k,v){console.log(k+':'+v);}
function traverse(o, func) {
    for (let i in o) {
        func.apply(this, [i, o[i]]);
        if (o[i] !== null && typeof (o[i]) == "object") {
            //going one step down in the object tree!!
            traverse(o[i], func);
        }
    }
}*/
//(function(){
//    let s=MRO.ses;
//    if(s.get(DFS.ZCLIVER)!==s.get(DFS.ZGUIVER))
//        if(confirm('new version: server:'+s.get(DFS.ZCLIVER)+' local:'+s.get(DFS.ZGUIVER))){
//            window.location.reload(true);
//        }
//console.log('extra.js-loaded');
//})();
//MRO.runcmd=function(c){if(MRO.ax===null)return;try{MRO.ax.run(c);}catch(e){alert(c+'///'+e.message);}}
//(function(){
//console.log('exec.js-loaded');
//})();