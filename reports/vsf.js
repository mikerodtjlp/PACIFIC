//****************************************************************************************************************
//Visual Factory reported is intended to be the source of visually see producton matters.
//it uses pacific framework as its communication or data access layer, which return json string with the information
//neeede to build de charts.
//The first version of this project will only generate simple histogram charts.
//all the report are configured in a database so the current code do not know anything about the bussiness side.
//****************************************************************************************************************
var pcont = "";

	function get_reports(){
		get_core_ajax("select id,name,replace(data,'" + "\"','''') data,category1, status from t_reports", get_reports_result);   
   }
	function get_reports_result(response){
		
		var nrows = response["zl0rows"];
		
		for (var i = 0; i < nrows; i++) {
			var r_id =  "l0" + i.toString() + "A"; 
			var r_name = "l0" + i.toString() + "B"; 
			var r_params = "l0" + i.toString() + "C"; 
			//var r_procs = "l0" + i.toString() + "D"; 
			var r_cat = "l0" + i.toString() + "D";
			var r_status = "l0" + i.toString() + "E";
			
			var a_id = "rep" + response[r_id];
			
			  var newElement = {};
				newElement[i,0] = response[r_name];
				//newElement[i,1] = response[r_procs];
				newElement[i,1] = response[r_params];
				newElement[i,2] = response[r_cat];
				
			_reports.push(newElement);
			
			var shref = "<li><a href=\"#\" id =\"" + a_id + "\" title= \"" + response[r_name] +  
						"\">" + response[r_name] + "<span>" + response[r_status]+"</span></a></li>"; 	
			
			if (response[r_cat] == "OPL") $("#uopl").append(shref );
			else if (response[r_cat] == "CR") $("#ucr39").append(shref);
			else if (response[r_cat] == "POL")	$("#upol").append(shref);
			else if (response[r_cat] == "MOL")	$("#umxmo").append(shref);
			else if (response[r_cat] == "RX")	   $("#urx").append(shref);
			
			$("#" + a_id).click(function(event){
				    //_report_title = $(this).html();
					_report_title = $(this).attr('title');
					$("#pop-up").html("");
					$("div#pop-up").css('position','absolute').css('top', '50%').css('left', '40%').css('margin','-20px 0 0 -50px');
					$('div#pop-up').show();
					//set_popup_tags($(this).html());
					set_popup_tags($(this).attr('title'));
					event.preventDefault();
			});									
		}
  } 
	function set_popup_tags(name){
		var jd = {};
		
		for (var i = 0; i < _reports.length; i++) {			
			if (_reports[i][i,0]   == name){	
                
				var n = _reports[i][i,1];
				var str = n.replace(/'/g,"\"");
				//alert(str);
				jd = JSON.parse("{" + str + "}");	 //parametros
				var tittle = document.createElement("span");
				
				$(tittle).css("margin-bottom","15px");
				$(tittle).css("padding","2px");				
				$(tittle).css("font-size","15px");
				$(tittle).css("border-bottom-style","solid");				
				$(tittle).css("border-width","1px");
				$(tittle).html(name).css('width', "275px");
				
				$("#pop-up").append(tittle).append("<BR/>");
				
				var key;
				for(key in jd.params) {
					//alert(jd.params[key]["name"]);
					
					//var xspan = "<span>" + jd.params[key]["description"] +"</span>";
					var xspan = document.createElement("span");
					    $(xspan).html(jd.params[key]["description"]).css('width', jd.params[key]["description-width"]);
						
					var xi = document.createElement("input");
						xi.setAttribute("id", jd.params[key]["name"]);
						xi.setAttribute("type", "text");                     
						xi.setAttribute("value", jd.params[key]["default"]);
						xi.setAttribute("maxlength", jd.params[key]["maxlength"]);
						
					$(xi).css("text-transform", jd.params[key]["text-transform"]);
					
					if (jd.params[key]["numeric"] != null){
						
						if (jd.params[key]["numeric"] == "true"){
							$(xi).keypress(function(evt) {
								var charCode = (evt.which) ? evt.which : event.keyCode
								if (charCode > 31 && (charCode < 48 || charCode > 57))
									return false;
								return true;
							});
						}
				    } 
					
					$("#pop-up").append( xspan);
					$("#pop-up").append( xi);
					$("#pop-up").append("<BR/>");
					
					$("#" + jd.params[key]["name"]).css('width', jd.params[key]["width"]).css('height',jd.params[key]["height"]);
					
					if (jd.params[key]["default-function"] != null){						
					    get_ajax_p( "select " + jd.params[key]["default-function"] + "() value", set_input_value, xi);   
					}
				}
				
				break;
			}
	   }
			$("#pop-up").append("<a href=\"#\" id=\"run-pop-up\" >Run</a>");
			$("#pop-up").append("<a href=\"#\" id=\"close-pop-up\" >Close</a>");
			
			$("#close-pop-up").click(function(event){		
					$('div#pop-up').hide();
			});
			
			$("#run-pop-up").click(function(event){		
					call_report(name);
					$('div#pop-up').hide();
			});	
			

   }
   
	function set_input_value(response, input){	   	
	   var a = "l00A";		 
	   input.setAttribute("value", response[a]);	   
   }
//	function call_report(name){   	  
//      for (var i = 0; i < _reports.length; i++) {			
//			if (_reports[i][i,0]   == name){
//				
//				var jd = {};
//				var n = _reports[i][i,1];
//				var str = n.replace(/'/g,"\"");
//				jd = JSON.parse("{" + str + "}");	 //parametros
//				var params = get_params_values(jd);
//				
//				get_ajax("exec " + jd["function"] + params, loadx_result, jd.module);   
//				set_bars_function(jd);
//				break;
//			}
//	   }
   //   }
   function call_report(name) {
       for (var i = 0; i < _reports.length; i++) {
           if (_reports[i][i, 0] == name) {

               var jd = {};
               var n = _reports[i][i, 1];
               var str = n.replace(/'/g, "\"");
               jd = JSON.parse("{" + str + "}");  //parametros
               var params = get_params_values(jd);

               get_ajax("exec " + jd["function"] + params, create_charts, jd.module);
               //set_bars_function(jd);
               break;
           }
       }
   }
   
   function get_vector_2(response){
		
		 var data = new google.visualization.DataTable();
             data.addColumn('string', 'axis');
             data.addColumn('number', 'value');
		
		for (var i = 0; i < 1000; i++) {
			
			var ColA = "l0" + i.toString() + "C";
			var value = "l0" + i.toString() + "D";
			
			if (response[ColA] == null) continue;			
		    data.addRow([response[ColA], parseInt(response[value])]);			
		}
		
		return data;
   }
   function create_charts(response) {

	  var vec = get_vector_2(response);
      var options = {
          title: _report_title
      };

      var chart = new google.visualization.LineChart(document.getElementById('chart'));
      chart.draw(vec, options);

   }



	function set_bars_function(json){
	 for (var i=0; i<24; i++){
		
		$("#"+i).click(function(event){	
		
				get_on_row_click_params_values(json, $(this).attr("id"));				
		});
	 }
	
   }
	function get_on_row_click_params_values(json, i){		
		var key;
		var result = "";
		$.each(json.on_row_click, function() {
			try{
			var t = this;
			result += t["function"] + " ";
			$.each(t.values , function(index, val) {
			   //alert(val);
			    if (val.indexOf("*") == -1)
			       result += "'" + $("#" + val).val() + "',";
				else if (val.indexOf("h*") !== -1)				   
				   result += "'" /*+ val.replace("*","")*/ + $("#" + val.replace("*",i)).html() + "',";
				else  if (val.indexOf("d*") !== -1)				   
				   result += "'" + $("#" + val.replace("*",i)).html() + "',";
				
			});
			} catch(err){
				alert (err);
			}
		});
         result = result.substring(0, result.length - 1);
         alert(json.module);
         var module = json.module;
		 get_ajax("exec " + result, show_prod_result, module); 
   }   
	function get_params_values(json){
		var key;
		var result = "";
		for(key in json.params) {
			 //alert( $("#" + json.params[key]["name"]).val());			
			 result += "\"" + $("#" + json.params[key]["name"]).val() + "\",";
		}

        result = result.substring(0, result.length - 1);
        return result;            
   }
   
/*
     function run() {
	        setInterval(function () { loadx() }, 15000);
     }
*/
     function show_last_run() {
	        var d = new Date();
	        var n = d.toLocaleTimeString();
	        $("#last_run").html("last run:" + n);
	    }
	 function progress_bar(text, step) {
	    if (step == 0) pcont = "";
	    else pcont += ".";

	    $("#working").html(pcont + text);
	 }	     
	 function clear_bars() {
	        for (var i = 0; i < 24; i++) {
	            var hh = i.toString();
	            //barras
				var bar = document.getElementById(hh);
				style_bar(bar, 0);
				//dias
	            var day = document.getElementById("d" + hh);
				style_bar_x2(day, "");
	            /*  PORCENTAJES ********************************************************************/
	            var perc = document.getElementById(hh + "v");
				style_bar_perc(perc,0);				
	            /*  HORAS *************************************************************************/
	            var hor = document.getElementById("h" + hh);
				style_bar_x1(hor, "");
	        }
	    }
     function color_days(last_day) {
	        for (var i = 0; i < 24; i++) {
	            if ($("#d" + i).html() != last_day) {	                
	                $("#d" + i).css ("color", "red");
	                document.getElementById("d" + i).style.fontWeight = "bold";
	            } else {	                
	                $("#d" + i).css("color", "black");
	                document.getElementById("d" + i).style.fontWeight = "bold";
	            }
	        }
	    }	

     function loadx_result(response) {
			clear_bars();
			var last_day = "";
			//var line_id = $("#line1").val();
			
			for (var i = 0; i < 1000; i++) {
				var line = "l0" + i.toString() + "A";
				var hora = "l0" + i.toString() + "C";
				var yield = "l0" + i.toString() + "D";
				var dia = "l0" + i.toString() + "B";

				if (response[line] == null) continue;
				//if (response[line] != line_id) continue;
				
				var hh = response[hora]; 						
				//var bar = document.getElementById(hh);
				var bar = document.getElementById(i.toString());
					style_bar(bar, response[yield]);				/* BARRAS *********************/
				//var perc = document.getElementById(hh + "v");
				var perc = document.getElementById(i.toString() + "v");
					style_bar_perc(perc, response[yield]);  		/* PORCENTAJES ****************/
				//var hor = document.getElementById("h" + hh);
				var hor = document.getElementById("h" + i.toString());
					style_bar_x1(hor, hh);	                    	/* HORAS *********************/
				//var day = document.getElementById("d" + hh);
				var day = document.getElementById("d" + i.toString());
					style_bar_x2(day, response[dia]);				/* DIAS **********************/	
				
				set_report_title(_report_title , " **** " + response[line] + " - " + response[dia]);				
				
				last_day = response[dia];
			}
			color_days(last_day);
			progress_bar('complete', 2);
			show_last_run();
	    }
	 function show_prod(hour) {
	        
			var batch1 = "'" + $("#batch1").val() + "'";
	        var batch2 = "'" + $("#batch2").val() + "'";
	        var line1 = "'" + $("#line1").val() + "'";
	        var fecha = "'" + $("#d" + hour).html() + "'";
	        get_ajax("exec get_yield_prod_qty_per_hour_prod " + batch1 + "," + batch2 + "," + hour + ",'COT'," + line1 + "," + fecha, show_prod_result);   
	 }
     function show_prod_result(response) {
        	
			var contai = "<span id='sprod'>"
	        var contat = "<span id='sprodt'>"
	        var contae = "</span>"
	        var result = contat + "PROD" + contae +
			contat + "DTL" + contae +
			contat + "QTY" + contae + "<BR>";

	        for (var i = 0; i < 1000; i++) {
	            var prod = "l0" + i.toString() + "A";
	            var detail = "l0" + i.toString() + "B";
	            var qty = "l0" + i.toString() + "C";

	            if (response[prod] == null) continue;
	            result += contai + response[prod] + contae +
  						    contai + response[detail] + contae +
							contai + response[qty] + contae + " <BR>"
	        }
	        showPos(event, result);
	 }
	 function set_report_title(title, detail){
		$("#rpt2").html(title + "  " + detail);
	 }
	 //pop-ups
	 function showPos(event, text) {
	        var el, x, y;

	        el = document.getElementById('PopUp');
	        if (window.event) {
	            x = window.event.clientX + document.documentElement.scrollLeft
			+ document.body.scrollLeft;
	            y = window.event.clientY + document.documentElement.scrollTop +
			+document.body.scrollTop;
	        }
	        else {
	            x = event.clientX + window.scrollX;
	            y = event.clientY + window.scrollY;
	        }
	        x -= 2; y -= 2;
	        y = y + 15
	        el.style.left = x + "px";
	        el.style.top = y + "px";
	        el.style.display = "block";
	        //document.getElementById('PopUpText').innerHTML = text;			
			$("#PopUpText").html(text).show("slow");
	    }
	 //CSS	 
	 function style_bar(bar, porcentaje){
		
	   var ff = (1 - (1 - porcentaje / 100)) * 2;
	   var ee = (1 - porcentaje / 100) * 2;
		
	    if (porcentaje == 0) {
			ee = 0;
			ff = .001;
		}
		if (bar == null) { /*console.log("bar is null");*/  return; }
		
		//bar.style.setAttribute("marginTop", "0" + "in", false);
		
		bar.style.background = "green";
		if (porcentaje < 95) bar.style.background = "yellow";
		if (porcentaje < 90) bar.style.background = "red";
		
		
		bar.style.marginTop =  "0in";
		bar.style.fontSize = "7px";
		bar.style.paddingLeft = ".05in";
		bar.style.width = ".25in";
		bar.style.height = "0" + "in";
		bar.style.marginRight = ".05in";
		bar.style.borderRadius = "4px";
		//bar.style.boxShadow = "1px 2px 1px #333";
		bar.style.cssText += "-ms-transform: rotate(+.5deg);";
		bar.style.boxShadow = "inset 0 0 10px #333333";
		//bar.style.setAttribute("marginTop", ee + "in", false);
		bar.style.marginTop =  ee + "in";
		bar.style.height = ff + "in";
	 }
	 function style_bar_perc(perc, porcentaje){	
        if (perc == null) { console.log("perc is null");  return; }
		
		if (porcentaje != 0)  perc.innerHTML = porcentaje + "%";
		else perc.innerHTML = "";
		
		perc.style.cssText += "-ms-transform: rotate(-45deg);";
		perc.style.fontWeight = "bold";
		perc.style.fontSize = "10px";
		perc.style.fontFamily = "Arial";
		perc.style.cssText += "background:white; width:34px;";
		perc.style.boxShadow = "inset 0 0 10px #AAAAAA";		
	 }
	 function style_bar_x1(hor, value){
        if (hor == null) return;
		hor.innerHTML = value;
		hor.style.fontFamily = "Arial";
		hor.style.fontSize = "10px";
		hor.style.paddingLeft = ".05in";
		hor.style.width = ".25in";
		hor.style.height = ".25in";
		hor.style.marginRight = ".05in";
		hor.style.cssText += "-ms-transform: rotate(-45deg);";
	 }	 
	 function style_bar_x2(day, value){
        if (day == null) return;
		day.innerHTML = value;
		day.style.fontFamily = "Times New Roman";
		day.style.fontSize = "5.5px";
		day.style.paddingLeft = ".05in";
		day.style.width = ".25in";
		day.style.height = ".25in";
		day.style.marginTop = ".10in";		
		day.style.marginRight = ".05in";
		day.style.cssText += "-ms-transform: rotate(-75deg);";
	 }
	 //HTML *********************************************************************
	 function footer() {
		
	        var s = "<footer>  " +
           "<div>" +
		   "		<section id='Poly'>" +
		"			<header>" +
		"				<h3>Poly</h3>" +
		"			</header>" +
		"			<ul  id =\"upol\">" +
		"			</ul>" +
		"		</section>" +
		"		<section id='MxMo'>" +
		"			<header>" +
		"				<h3>MxMo</h3>" +
		"			</header>" +
		"			<ul  id =\"umxmo\">" +
		"			</ul>" +
		"		</section>" +
		"		<section id='Cr39'>" +
		"			<header>" +
		"				<h3>Cr39</h3>" +
		"			</header>" +
		"			<ul  id =\"ucr39\">" +
		"			</ul>" +
		"		</section>" +
		"		<section id='Opal'>" +
		"			<header>" +
		"				<h3>Opal</h3>" +
		"			</header>" +
		"				<ul id =\"uopl\">" +
		"				</ul>" +
		"		</section>				" +
		"		<section id='MxRx'>" +
		"			<header>" +
		"				<h3>Rx</h3>" +
		"			</header>" +
		"			<ul  id =\"urx\">" +
		"			</ul>" +
		"		</section>			" +
		"	</div>" +
        "</footer>"
        ;

	        document.write(s);
			
			get_reports();
			
	    }
     function header() {
        var s = "<header>     " +
	    "<h1> " +
			"<span id = 'rpt'>Visual Factory Reports</span>			   " +
			"<span id = 'rpt2'></span>" +
		"</h1>	      " +
        "<nav>  " +
         "       <ul>  " +
         "           <li><a href='#'>Home</a></li>  " +
		"			<li><a href='#'>More reports</a></li>  " +
		"			<li><a href='#'>About</a></li>  " +
        "        </ul>  " +
        "</nav>		    " +
        "</header> ";

        document.write(s);
    }
     function segment1() {
         var s=  "<section id='seg1'>  			"+
    				"<div id = 'chart'>"+
                	"<div id='0'  style= \"background:red;	float:left;\"><div id='0v'></div></div>"+
					"<div id='1'  style= \"background:#FCA0AA;	float:left;\"><div id='1v'></div></div>"+
					"<div id='2'  style= \"background:#A39B26;	float:left;\"><div id='2v'></div></div>"+
					"<div id='3'  style= \"background:#4EA0FE;	float: left;\"><div id='3v'></div></div>" +
					"<div id='4'  style= \"background:orange;	float:left;\"><div id='4v'></div></div>"+
					"<div id='5'  style= \"background:grey;	float:left;\"><div id='5v'></div></div>"+
					"<div id='6'  style= \"background:#E39B2A;	float:left;\"><div id='6v'></div></div>"+
					"<div id='7'  style= \"background:#F0C00A;	float:left;\"><div id='7v'></div></div>"+
					"<div id='8'  style= \"background:#AEC0F0;	float:left;\"><div id='8v'></div></div>"+
					"<div id='9'  style= \"background:#F800FE;	float: left;\"><div id='9v'></div></div>" +
					"<div id='10' style= \"background:#4EA0F0;float:left;\"><div id='10v'></div></div>"+
					"<div id='11' style= \"background:#FC00AA;float:left;\"><div id='11v'></div></div>"+
					"<div id='12' style= \"background:#CCCCCC;float:left;\"><div id='12v'></div></div>"+
					"<div id='13' style= \"background:#EE0EEE;float:left;\"><div id='13v'></div></div>"+
					"<div id='14' style= \"background:#FCC0EA;float:left;\"><div id='14v'></div></div>"+
					"<div id='15' style= \"background:#0ECEFE;float:left;\"><div id='15v'></div></div>"+
					"<div id='16' style= \"background:#0E5EFE;float:left;\"><div id='16v'></div></div>"+
					"<div id='17' style= \"background:#FC384F;float:left;\"><div id='17v'></div></div>"+
					"<div id='18' style= \"background:#AC3A41;float:left;\"><div id='18v'></div></div>"+
					"<div id='19' style= \"background:#F0C000;float:left;\"><div id='19v'></div></div>"+
					"<div id='20' style= \"background:#AC0A11;float:left;\"><div id='20v'></div></div>"+
					"<div id='21' style= \"background:#FE2EFE;float:left;\"><div id='21v'></div></div>"+
					"<div id='22' style= \"background:#6E2EFE;float:left;\"><div id='22v'></div></div>"+
					"<div id='23' style= \"background:#9EAEFE;float:left;\"><div id='23v'></div></div>"+
                    "</div>	   "+
                    "</section> ";

         document.write(s);
     }
     function segment2() {
     		 var s=  "<section id =\"seg2\"> "+					
					"<div id = \"pie\" style= \"clear:left;\" >"+
					"<div id=\"h0\" style= \"float:left;\"></div>"+
				    "<div id=\"h1\" style= \"float: left;\"></div>"+
					"<div id=\"h2\" style= \"float: left;\"></div>"+
					"<div id=\"h3\" style= \"float: left;\"></div>"+
					"<div id=\"h4\" style= \"float:left;\"></div>"+
					"<div id=\"h5\" style= \"float: left;\"></div>"+
					"		<div id=\"h6\" style= \"float: left;\"></div>"+
					"		<div id=\"h7\" style= \"float: left;\"></div>"+
				    "		<div id=\"h8\" style= \"float: left;\"></div>"+
					"		<div id=\"h9\" style= \"float: left; \"></div>"+
					"		<div id=\"h10\" style= \"float: left;\"></div>"+
					"		<div id=\"h11\" style= \"float: left;\"></div>"+
					"		<div id=\"h12\" style= \"float: left;\"></div>"+
					"		<div id=\"h13\" style= \"float: left;\"></div>"+
					"		<div id=\"h14\" style= \"float: left;\"></div>"+
					"		<div id=\"h15\" style= \"float: left;\"></div>"+
					"		<div id=\"h16\" style= \"float: left;\"></div>"+
					"		<div id=\"h17\" style= \"float: left;\"></div>"+
					"		<div id=\"h18\" style= \"float: left;\"></div>"+
					"		<div id=\"h19\" style= \"float: left;\"></div>"+
					"		<div id=\"h20\" style= \"float: left;\"></div>"+
					"		<div id=\"h21\" style= \"float: left;\"></div>"+
					"		<div id=\"h22\" style= \"float: left;\"></div>"+
					"		<div id=\"h23\" style= \"float: left;\"></div>" +
				    "</div>				   " +
	    	        "</section>             ";
            document.write(s);

        }
     function segment3() {
            var s = "<section id =\"seg3\">	" +
				    "<div id = \"pie2\" style= \"clear:left;\" >" +
                	"	<div id=\"d0\" style= \"float:left;\"></div>" +
					"	<div id=\"d1\" style= \"float: left;\"></div>" +
					"	<div id=\"d2\" style= \"float: left;\"></div>" +
					"	<div id=\"d3\" style= \"float: left;\"></div>" +
				"		<div id=\"d4\" style= \"float:left;\"></div>" +
				"		<div id=\"d5\" style= \"float: left;\"></div>" +
				"		<div id=\"d6\" style= \"float: left;\"></div>" +
				"		<div id=\"d7\" style= \"float: left;\"></div>" +
				"		<div id=\"d8\" style= \"float: left;\"></div>" +
				"		<div id=\"d9\" style= \"float: left; \"></div>" +
				"		<div id=\"d10\" style= \"float: left;\"></div>" +
				"		<div id=\"d11\" style= \"float: left;\"></div>" +
				"		<div id=\"d12\" style= \"float: left;\"></div>" +
				"		<div id=\"d13\" style= \"float: left;\"></div>" +
				"		<div id=\"d14\" style= \"float: left;\"></div>" +
				"		<div id=\"d15\" style= \"float: left;\"></div>" +
				"		<div id=\"d16\" style= \"float: left;\"></div>" +
				"		<div id=\"d17\" style= \"float: left;\"></div>" +
				"		<div id=\"d18\" style= \"float: left;\"></div>" +
				"		<div id=\"d19\" style= \"float: left;\"></div>" +
				"		<div id=\"d20\" style= \"float: left;\"></div>" +
				"		<div id=\"d21\" style= \"float: left;\"></div>" +
				"		<div id=\"d22\" style= \"float: left;\"></div>" +
				"		<div id=\"d23\" style= \"float: left;\"></div>" +
                "</div>			   " +
                "</section>  ";
            document.write(s);
        }
     function action() {
            var s = "<span id=\"last_run\"></span>		" +
					"<span id=\"working\"></span>";

            document.write(s);
        }
	 function on_ready_events(){
	      google.load("visualization", "1", {packages:["corechart"]});	      
	 }
	

	