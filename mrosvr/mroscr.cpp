#include "stdafx.h"

//#include "mroctrl.h"
#include "SessionMan.h"

/*void CSessionMan::scr_get_request()
{
	CString command;
	command.Format(_T("select * from t_scr with (nolock) where scrid = %s"), _params.get(_T("cscrini")));
	getconnectionx(con, obj);
	con.execute(command.GetBuffer(), obj);
	require(obj.IsEOF(), _T("reg_not_exist"));

	CString helper;
	helper.Format(_T("%ld")				, obj.getlong(_T("scrid")));
	_values.set(_T("$scrid$")			, helper);
	_values.set(_T("$originator$")		, obj.get(_T("originator")));
	_values.set(_T("$cphoneext$")		, obj.get(_T("phone")));
	_values.set(_T("$datesubmitted$")	, obj.getdate(_T("datesubmitted")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$sysname$")			, obj.get(_T("systemname")));
	_values.set(_T("$versionnum$")		, obj.get(_T("versionnumber")));
	_values.set(_T("$issoftware$")		, obj.get(_T("issoftware")));
	_values.set(_T("$isdoc$")			, obj.get(_T("isdocument")));
	_values.set(_T("$changetype$")		, obj.get(_T("changetype")));
	_values.set(_T("$reason$")			, obj.get(_T("reason")));
	_values.set(_T("$priority$")		, obj.get(_T("priority")));
	_values.set(_T("$daterequired$")	, obj.getdate(_T("daterequired")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$changedesc$")		, obj.get(_T("changedesc")));
}

void CSessionMan::scr_create_request()
{
	CString command;
	command.Format(	_T("insert into t_scr (originator, phone, datesubmitted, systemname, ")
					_T("versionnumber, issoftware, isdocument, changetype, reason, priority, daterequired, changedesc) ")
					_T("values('%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s')"), 

					_params.get(_T("coriginator"))	, _params.get(_T("cphoneext")), 
					_params.get(_T("cdatesub"))		, _params.get(_T("csysname")), 
					_params.get(_T("cversion"))		, _params.get(_T("cissoft")), 
					_params.get(_T("cisdoc"))		, _params.get(_T("cchangetype")), 
					_params.get(_T("creason"))		, _params.get(_T("cpriority")), 
					_params.get(_T("cdatereq"))		, _params.get(_T("cchangedesc")));

	getconnection(con);
	con.execute(command.GetBuffer());
}

void CSessionMan::scr_get_evaluation()
{
	CString command;
	command.Format(	_T("select * from t_scr_eval with (nolock) where scrid = %s"), 
					_params.get(_T("cscrini")));
	getconnectionx(con, obj);
	con.execute(command.GetBuffer(), obj);
	require(obj.IsEOF(), _T("reg_not_exist"));

	CString helper;
	helper.Format(_T("%ld")				, obj.getlong(_T("scrid")));
	_values.set(_T("$scrid$")			, helper);
	_values.set(_T("$recievedby$")		, obj.get(_T("recievedby")));
	_values.set(_T("$daterecieved$")	, obj.getdate(_T("daterecieved")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$assignto$")		, obj.get(_T("assignto")));
	_values.set(_T("$dateassigned$")	, obj.getdate(_T("dateassigned")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$tipo$")			, obj.get(_T("softwareaffected")));
	_values.set(_T("$comps$")			, obj.get(_T("componentsaffected")));
	_values.set(_T("$spec_section$")	, obj.get(_T("spec_section")));
	_values.set(_T("$spec_page$")		, obj.get(_T("spec_page")));
	_values.set(_T("$spec_datecomp$")	, obj.getdate(_T("spec_datecomp")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$spec_initial$")	, obj.get(_T("spec_initial")));
	_values.set(_T("$design_section$")	, obj.get(_T("design_section")));
	_values.set(_T("$design_page$")		, obj.get(_T("design_page")));
	_values.set(_T("$design_datecomp$")	, obj.getdate(_T("design_datecomp")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$design_initial$")	, obj.get(_T("design_initial")));
	_values.set(_T("$test_section$")	, obj.get(_T("test_section")));
	_values.set(_T("$test_page$")		, obj.get(_T("test_page")));
	_values.set(_T("$test_datecomp$")	, obj.getdate(_T("test_datecomp")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$test_initial$")	, obj.get(_T("test_initial")));
	_values.set(_T("$train_section$")	, obj.get(_T("train_section")));
	_values.set(_T("$train_page$")		, obj.get(_T("train_page")));
	_values.set(_T("$train_datecomp$")	, obj.getdate(_T("train_datecomp")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$train_initial$")	, obj.get(_T("train_initial")));
	_values.set(_T("$user_section$")	, obj.get(_T("user_section"))); 
	_values.set(_T("$user_page$")		, obj.get(_T("user_page"))); 
	_values.set(_T("$user_datecomp$")	, obj.getdate(_T("user_datecomp")).Format(_T("%Y/%m/%d %H:%M:%S"))); 
	_values.set(_T("$user_initial$")	, obj.get(_T("user_initial")));
	_values.set(_T("$support_section$")	, obj.get(_T("support_section"))); 
	_values.set(_T("$support_page$")	, obj.get(_T("support_page"))); 
	_values.set(_T("$support_datecomp$"), obj.getdate(_T("support_datecomp")).Format(_T("%Y/%m/%d %H:%M:%S"))); 
	_values.set(_T("$support_initial$")	, obj.get(_T("support_initial")));
}

void CSessionMan::scr_create_evaluation()
{
	CString command;

	command.Format(	_T("select count(*) as res from t_scr_eval with (nolock) where scrid= %s"), _params.get(_T("cscrini")));
	getconnectionx(con, obj);
	con.execute(command.GetBuffer(), obj);
	require(obj.getint(0) != 0, _T("reg_already_exist"));

	command.Format(	_T("insert into t_scr_eval ")
					_T("(scrid, receivedby,datereceived,assignedto,dateassigned,softwareaffected,componentsaffected, ")
					_T("spec_section,spec_page,spec_datecomp,spec_initial, ")
					_T("design_section,design_page,design_datecomp,design_initial, ")
					_T("systest_section,systest_page,systest_datecomp,systest_initial, ")
					_T("training_section,training_page,training_datecomp,training_initial, ")
					_T("usrmanual_section,usrmanual_page,usrmanual_datecomp,usrmanual_initial, ")
					_T("sysrmanual_section,sysrmanual_page,sysrmanual_datecomp,sysrmanual_initial) ")
					_T("values(%s, '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', ") 
					_T("'%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', ") 
					_T("'%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s') "), 

					_params.get(_T("cscrini"))		, _params.get(_T("crecby"))		, _params.get(_T("cdaterec")), 
					_params.get(_T("cassto"))		, _params.get(_T("cdateass"))	, _params.get(_T("ctipo")), 
					_params.get(_T("ccomps"))		, _params.get(_T("cspecsec"))	, _params.get(_T("cspecpag")),
					_params.get(_T("cspecdtc"))		, _params.get(_T("$spec_initial$")), _params.get(_T("cdesgsec")), 
					_params.get(_T("cdesgpag"))		, _params.get(_T("cdesgdtc"))	, _params.get(_T("design_initial$")),
					_params.get(_T("ctestsec"))		, _params.get(_T("ctestpag"))	, _params.get(_T("ctestdtc")),
					_params.get(_T("test_initial$")), _params.get(_T("ctransec"))	, _params.get(_T("ctranpag")), 
					_params.get(_T("ctrandtc"))		, _params.get(_T("train_initial$")), _params.get(_T("cusersec")), 
					_params.get(_T("cuserpag"))		, _params.get(_T("cuserdtc"))	, _params.get(_T("user_initial$")),
					_params.get(_T("csuppsec"))		, _params.get(_T("csupppag"))	, _params.get(_T("csuppdtc")),
					_params.get(_T("support_initial$")));
	con.execute(command.GetBuffer());
}

void CSessionMan::scr_get_times()
{
	CString command;
	command.Format(	_T("select * from t_scr_times with (nolock) where scrid = %s"), _params.get(_T("cscrini")));
	getconnectionx(con, obj);
	con.execute(command.GetBuffer(), obj);
	require(obj.IsEOF(), _T("reg_not_exist"));

	CString helper;
	helper.Format(_T("%ld")				, obj.getlong(_T("scrid")));
	_values.set(_T("$scrid$")			, helper);
	_values.set(_T("$analisys_est$")	, obj.get(_T("analisysdesign_estimated")));
	_values.set(_T("$analisys_act$")	, obj.get(_T("analisysdesign_real")));
	_values.set(_T("$analisys_datecomp$"), obj.getdate(_T("analisysdesign_datecomp")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$analisys_remarks$"), obj.get(_T("analisysdesign_remarks")));
	_values.set(_T("$code_est$")		, obj.get(_T("codingtesting_estimated")));
	_values.set(_T("$code_act$")		, obj.get(_T("codingtesting_real")));
	_values.set(_T("$code_datecomp$")	, obj.getdate(_T("codingtesting_datecomp")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$code_remarks$")	, obj.get(_T("codingtesting_remarks")));
	_values.set(_T("$accept_est$")		, obj.get(_T("acceptance_estimated")));
	_values.set(_T("$accept_act$")		, obj.get(_T("acceptance_real")));
	_values.set(_T("$accept_datecomp$")	, obj.getdate(_T("acceptance_datecomp")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$accept_remarks$")	, obj.get(_T("acceptance_remarks")));
	_values.set(_T("$total_est$")		, obj.get(_T("total_hours_estimated")));
	_values.set(_T("$total_act$")		, obj.get(_T("total_hours_real")));
	_values.set(_T("$total_datecomp$")	, obj.getdate(_T("total_hours_datecomp")).Format(_T("%Y/%m/%d %H:%M:%S")));
	_values.set(_T("$total_remarks$")	, obj.get(_T("total_hours_remarks")));
	_values.set(_T("$impactanalisysfile$"), obj.get(_T("impactanalisysfile")));
}

void CSessionMan::scr_create_times()
{
	CString command;

	command.Format(	_T("select count(*) as res from t_scr_times with (nolock) where scrid= %s"), _params.get(_T("cscrini")));
	getconnectionx(con, obj);
	con.execute(command.GetBuffer(), obj);
	require(obj.getint(0) != 0, _T("reg_already_exist"));

	command.Format(	_T("insert into t_scr_times ")
					_T("(scrid, analisysdesign_estimated,analisysdesign_real,analisysdesign_datecomp,analisysdesign_remarks, ")
					_T("codingtesting_estimated,codingtesting_real,codingtesting_datecomp,codingtesting_remarks, ")
					_T("acceptance_estimated,acceptance_real,acceptance_datecomp,acceptance_remarks, ")
					_T("total_hours_estimated,total_hours_real,total_hours_datecomp,total_hours_remarks,impactanalisysfile) ")
					_T("values(%s, '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s', '%s') "), 
					_params.get(_T("cscrini")), _params.get(_T("canaest")), _params.get(_T("canaact")), 
					_params.get(_T("canadtc")), _params.get(_T("canarmk")), _params.get(_T("ccodest")), 
					_params.get(_T("ccodact")), _params.get(_T("ccoddtc")), _params.get(_T("ccodrmk")),
					_params.get(_T("caccest")), _params.get(_T("caccact")), _params.get(_T("caccdtc")), 
					_params.get(_T("caccrmk")), _params.get(_T("ctotest")), _params.get(_T("totact")), 
					_params.get(_T("ctotdtc")), _params.get(_T("ctotrmk")), _params.get(_T("cimpact")));
	con.execute(command.GetBuffer());
}
*/