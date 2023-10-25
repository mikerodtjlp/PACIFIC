//globals
var _ip = "50.21.179.57";
var _port = "8930";
var _core_port = "8989";
var _proxy_port = "8900";
var _reports = new Array();
var _report_title = "";
	function get_ajax(func_name, callbackfunction, module) {
		var xmlhttp;
		var jd = {};
		if (window.XMLHttpRequest) {// code for IE7+, Firefox, Chrome, Opera, Safari			
			xmlhttp = new XMLHttpRequest();
		}
		else {// code for IE6, IE5			
			xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
		}
		xmlhttp.onreadystatechange = function () {
		    if (xmlhttp.readyState == 4) {
		        if (xmlhttp.status != 200) {
		            alert(xmlhttp.responseText);
		        }

		        jd = JSON.parse("{" + xmlhttp.responseText + "}");
		        callbackfunction(jd);
		    }
		}
        if (module === "hidalgo") _port = "8104";
        if (module === "sfc") _port = "8930";

        var strx = "http://" + _ip + ":" + _port + "/core.aspx?fun=execute_query&hdr=[zhdr:[retjson:1]]&prm=[zvalues:[query:embedded][sqltext: " + func_name + " ]]&bas=[zbasics:[p_langu:ES]]&proxysvr=" + _ip + "&proxyprt=" + _proxy_port;        
        xmlhttp.open("GET", strx, true);
		xmlhttp.send();
}

	function get_core_ajax(func_name, callbackfunction) {
	var xmlhttp;
    var jd = {};
    if (window.XMLHttpRequest) {// code for IE7+, Firefox, Chrome, Opera, Safari			
        xmlhttp = new XMLHttpRequest();
    }
    else {// code for IE6, IE5			
        xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
    }
    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4) {  
		
			if (xmlhttp.status != 200)
			{				
				return;
			}
			jd = JSON.parse("{" + xmlhttp.responseText + "}");			
            callbackfunction(jd);
        }
    }
    var strx = "http://" + _ip + ":" + _core_port + "/core.aspx?fun=execute_query&hdr=[zhdr:[retjson:1]]&prm=[zvalues:[query:embedded][sqltext: " + func_name + " ]]&bas=[zbasics:[p_langu:ES]]&proxysvr=" + _ip + "&proxyprt=" + _proxy_port;
    xmlhttp.open("GET", strx, true);
    xmlhttp.send();
}

	function get_ajax_p(func_name, callbackfunction, passedObject) {   
    var xmlhttp;
    var jd = {};
    if (window.XMLHttpRequest) {// code for IE7+, Firefox, Chrome, Opera, Safari			
        xmlhttp = new XMLHttpRequest();
    }
    else {// code for IE6, IE5			
        xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
    }
    xmlhttp.onreadystatechange = function () {
        if (xmlhttp.readyState == 4 && xmlhttp.status == 200) {
            jd = JSON.parse("{" + xmlhttp.responseText + "}");			
            callbackfunction(jd , passedObject);
        }
    }
    var strx = "http://" + _ip + ":" + _port + "/core.aspx?fun=execute_query&hdr=[zhdr:[retjson:1]]&prm=[zvalues:[query:embedded][sqltext: " + func_name + " ]]&bas=[zbasics:[p_langu:ES]]&proxysvr=" + _ip + "&proxyprt=" + _proxy_port;
    xmlhttp.open("GET", strx, true);
    xmlhttp.send();
}
