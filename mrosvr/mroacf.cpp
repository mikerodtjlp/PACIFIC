#include "stdafx.h"

//#include "mroctrl.h"
#include "SessionMan.h"

/*void CSessionMan::acf_get_default_data()
{
	getconnectionx(con, obj);
	con.execute(_T("select address from t_acf_approvers with (nolock) where nivel=1 and isdefault=1"), obj);
	_values.set(_T("$uno$"), !obj.IsEOF() ? obj.get(_T("address")) : _T(""));
	con.execute(_T("select address from t_acf_approvers with (nolock) where nivel=2 and isdefault=1"), obj);
	_values.set(_T("$dos$"), !obj.IsEOF() ? obj.get(_T("address")) : _T(""));
	con.execute(_T("select address from t_acf_approvers with (nolock) where nivel=3 and isdefault=1"), obj);
	_values.set(_T("$tres$"), !obj.IsEOF() ? obj.get(_T("address")) : _T(""));
	con.execute(_T("select address from t_acf_approvers with (nolock) where nivel=4 and isdefault=1"), obj);
	_values.set(_T("$applyby$"), !obj.IsEOF() ? obj.get(_T("address")) : _T(""));
}

void CSessionMan::acf_get_form()
{
	CString order = _params.get(_T("cordini"), 7);

	require(order.IsEmpty(), _T("inc_dat_order"));

	cCommand command(_basics);
	command.Format(	_T("select * from t_right_forms_hdr with (nolock) ")
					_T("where object_id= %s"), _params.get(_T("cordini")));
	getconnectionx(con, obj);
	con.execute(command, obj);
	require(obj.IsEOF(), _T("form_not_exist"));

	int status = obj.getint(_T("status"));

	CString helper;
	helper.Format(_T("%ld")			, obj.getlong(_T("object_id")));
	_values.set(_T("$order$")		,helper);
	_values.set(_T("$solicitante$")	, obj.get(_T("solicitante")));
	_values.set(_T("$user$")		, obj.get(_T("targetuser")));
	_values.set(_T("$status$")		, status);
	_values.set(_T("$uno$")			, obj.get(_T("approveone")));
	_values.set(_T("$dos$")			, obj.get(_T("approvetwo")));
	_values.set(_T("$tres$")		, obj.get(_T("approvethree")));
	_values.set(_T("$applyby$")		, obj.get(_T("final")));
	_values.set(_T("$reason$")		, obj.get(_T("reason")));
	_values.set(_T("$comments$")	, obj.get(_T("comments")));

	bool english =	_basics.get(ZLANGUA, ZLANGUALEN) == _T("EN") || 
					_basics.get(ZLANGUA, ZLANGUALEN) == _T("GE");

	_values.set(_T("$dategenerated$"), english ?	_T("created: ") : 
													_T("creada: ") + obj.getdate(_T("dategenerated")).Format(_T("%Y/%m/%d %H:%M:%S")));

	if(_tstoi(obj.getdate(_T("dateappone")).Format(_T("%Y"))) == 1899) 
		_values.set(_T("$dateappone$"), english ? _T("approve one: ") : _T("aprobacion uno: "));
	else
		_values.set(_T("$dateappone$"), english ? _T("approve one: ") : _T("aprobacion uno: ") + obj.getdate(_T("dateappone")).Format(_T("%Y/%m/%d %H:%M:%S")));

	if(_tstoi(obj.getdate(_T("dateapptwo")).Format(_T("%Y"))) == 1899) 
		_values.set(_T("$dateapptwo$"), english ? _T("approve two: ") : _T("aprobacion dos: "));
	else
		_values.set(_T("$dateapptwo$"), english ? _T("approve two: ") : _T("aprobacion dos: ") + obj.getdate(_T("dateapptwo")).Format(_T("%Y/%m/%d %H:%M:%S")));

	if(_tstoi(obj.getdate(_T("dateappthree")).Format(_T("%Y"))) == 1899) 
		_values.set(_T("$dateappthree$"), english ? _T("approve three: ") : _T("aprobacion tres: "));
	else
		_values.set(_T("$dateappthree$"), english ? _T("approve three: ") : _T("aprobacion tres: ") + obj.getdate(_T("dateappthree")).Format(_T("%Y/%m/%d %H:%M:%S")));

	if(_tstoi(obj.getdate(_T("dateafinal")).Format(_T("%Y"))) == 1899) 
		_values.set(_T("$dateafinal$"), english ? _T("apply: ") : _T("aplicada: "));
	else
		_values.set(_T("$dateafinal$"), english ? _T("apply: ") : _T("aplicada: ") + obj.getdate(_T("dateafinal")).Format(_T("%Y/%m/%d %H:%M:%S")));

	if(status == 0) _values.set(_T("$statusdesc$"), _T("0 - creada"));
	if(status == 1) _values.set(_T("$statusdesc$"), _T("1 - mandada"));
	if(status == 2) _values.set(_T("$statusdesc$"), _T("2 - aprovada control 1"));
	if(status == 3) _values.set(_T("$statusdesc$"), _T("3 - aprovada control 2"));
	if(status == 4) _values.set(_T("$statusdesc$"), _T("4 - aprovada control 3"));
	if(status == 5) _values.set(_T("$statusdesc$"), _T("5 - aprovacion final"));
	if(status == 6) _values.set(_T("$statusdesc$"), _T("6 - aplicada"));
	if(status == 9) _values.set(_T("$statusdesc$"), _T("9 - cancelada"));
}

void CSessionMan::acf_create_form()
{
	CString order			= _params.get(_T("cordini"), 7);
	CString solicitante		= _params.get(_T("csolicitante"));
	CString tagertuser		= _params.get(_T("ctargetuser"));
	CString approveone		= _params.get(_T("capproveone"));
	CString approvetwo		= _params.get(_T("capprovetwo"));
	CString approvethree	= _params.get(_T("capprovethree"));
	CString final			= _params.get(_T("cfinal"));
	CString reason			= _params.get(_T("creason"));
	CString comments		= _params.get(_T("ccomments"));

	if(comments.IsEmpty()) comments = _T("n/a");

	require(solicitante.IsEmpty() || tagertuser.IsEmpty() || approveone.IsEmpty() || approvetwo.IsEmpty(),  _T("incomplete_data"));
	require(approvethree.IsEmpty() || final.IsEmpty() || reason.IsEmpty() || comments.IsEmpty(), _T("incomplete_data"));

	require(!order.IsEmpty(),	_T("field_order_mustbe_empty"));

	solicitante.TrimRight();
	tagertuser.TrimRight();
	approveone.TrimRight();
	approvetwo.TrimRight();
	approvethree.TrimRight();

	// + parche
	int pos = solicitante.Find(_T('@')); if (pos != -1) solicitante.Delete(pos, solicitante.GetLength() - pos);
	pos = approveone.Find(_T('@')); if (pos != -1) approveone.Delete(pos, approveone.GetLength() - pos);
	pos = approvetwo.Find(_T('@')); if (pos != -1) approvetwo.Delete(pos, approvetwo.GetLength() - pos);
	pos = approvethree.Find(_T('@')); if (pos != -1) approvethree.Delete(pos, approvethree.GetLength() - pos);
	// - parche

	final.TrimRight();
	reason.TrimRight();
	comments.TrimRight();

	CString helper;
	cCommand command(_basics);

	// + create the header
	command.Format(	_T("insert into t_right_forms_hdr ")
					_T("(solicitante, targetuser, status, approveone, approvetwo, approvethree, final, reason, comments,  dategenerated) ")
					_T("values('%s@sola.com', '%s', 0, '%s@sola.com', '%s@sola.com', '%s@sola.com', '%s@sola.com', '%s', '%s', getdate())"),
					solicitante, tagertuser, approveone, approvetwo, approvethree, final, reason, comments);
	getconnectionx(con, obj);
	con.execute(command);

	// we just looking for the just createrd object in order to get the new order id
	command.Format(	_T("select top 1 * from t_right_forms_hdr with (nolock) ")
					_T("where solicitante='%s@sola.com' and targetuser='%s' and status=0 and approveone='%s@sola.com' and ")
					_T("approvetwo='%s@sola.com' and approvethree='%s@sola.com' and final='%s@sola.com' order by dategenerated desc "),
					solicitante, tagertuser, approveone, approvetwo, approvethree, final);
	con.execute(command, obj);
	require(obj.IsEOF(), _T("internal_error"));
	_params.set(_T("cordini"), obj.getint(_T("object_id")));
	_values.set(_T("$order$"), _params.get(_T("cordini")));
}

void CSessionMan::acf_send_form()
{
	CString order = _params.get(_T("cordini"), 7);

	acf_get_form();

	int count = -1;

	cCommand command(_basics);
	{
		command.Format(_T("select count(*) as res from t_right_forms_prf with (nolock) where docid=%s"), order);
		getconnectionx(con, obj);
		con.execute(command, obj);
		count = obj.getint(0);
		command.Format(_T("select count(*) as res from t_right_forms_dtl with (nolock) where docid=%s"), order);
		con.execute(command, obj);
		count += obj.getint(0);
	}

	require(count == 0								, _T("request_empty"));
	require(_params.get(_T("$status$")) != _T("0")	, _T("request_already_sent"));

	CString solicitante = _params.get(_T("$solicitante$"));
	CString approveone = _params.get(_T("$uno$"));

	solicitante.TrimRight();
	approveone.TrimRight();

	CSmtp mail;
	mail.m_strUser = _T("");
	mail.m_strPass = _T("");
	bool connected = false;
	require(!mail.Connect(_T("osm-mail.mx.sola.com")), _T("could_not_connact_email_server"));
	connected = true;

	try
	{
		CSmtpMessage msg;
		CSmtpMessageBody body;

		msg.Subject = _T("DCS - Access Control - Notificacion");
		msg.Sender.Name = _T("DCS - Access Control System");
		msg.Sender.Address = _T("Dservices@sola.com");
		msg.Recipient.Name = solicitante;
		msg.Recipient.Address = solicitante;
		body.Encoding = _T("text/html");
		body = _T("<body lang=EN-US link=blue vlink=purple> <div class=Section1> ")
					_T("<p class=MsoNormal><b><span style='font-size:20.0pt;font-family:Arial; ")
					_T("color:white;background:black'>Data collection system  Notificación ACF: ") + order + _T("</span></b></p> ")
					_T("<p class=MsoNormal><span style='font-size:10.0pt;font-family:Arial'>&nbsp;</span></p> ")
					_T("<p class=MsoNormal><b><span lang=ES-MX style='font-family:Arial'>Descripción</span></b><span ")
					_T("lang=ES-MX style='font-family:Arial'>:</span></p> ")
					_T("<p class=MsoNormal><span lang=ES-MX style='font-size:10.0pt;font-family:Arial'>Su ")
					_T("petición de derechos ha sido mandada al primer nivel de autorización: ") + approveone + _T("</span></p> ")
					_T("</div></body>");
		msg.Message.Add(body);
		mail.SendMessage(msg);

		msg.Subject = _T("DCS - Access Control - Authorization");
		msg.Recipient.Name = approveone;
		msg.Recipient.Address = approveone;
		CString dir;
		dir.Format(_T("http://10.1.25.1:1000/Default.aspx?&step=1&id=%s&code=92999"), order);
		body = _T("<body lang=EN-US link=blue vlink=purple> ")
					_T("<div class=Section1> ")
					_T("<p class=MsoNormal><b><span style='font-size:20.0pt;font-family:Arial; ")
					_T("color:white;background:black'>Data collection system  Autorizacion ACF: ") + order + _T("</span></b></p> ")
					_T("<p class=MsoNormal><span style='font-size:10.0pt;font-family:Arial'>&nbsp;</span></p> ")
					_T("<p class=MsoNormal><b><span lang=ES-MX style='font-family:Arial'>Descripción</span></b><span ")
					_T("lang=ES-MX style='font-family:Arial'>:</span></p> ")
					_T("<p class=MsoNormal><span lang=ES-MX style='font-size:10.0pt;font-family:Arial'>El ")
					_T("sistema DCS necesita la aprobación para otorgar derechos según el control de ")
					_T("accesos, la cual viene de: Dservices@sola.com</span></p> ")
					_T("<p class=MsoNormal><span lang=ES-MX style='font-size:10.0pt;font-family:Arial'>&nbsp;</span></p> ")
					_T("<p class=MsoNormal><b><span lang=ES-MX style='font-family:Arial'>Aprobación en</span></b><span ")
					_T("lang=ES-MX style='font-family:Arial'>:</span></p> ")
					_T("<p class=MsoNormal><span style='font-size:10.0pt;font-family:Arial'><a ")
					_T("href='") + dir + _T("'>") + dir + _T("</a></span></p> ")
					_T("<p class=MsoNormal><span style='font-size:10.0pt;font-family:Arial'>&nbsp;</span></p> ")
					_T("</div></body>"),
		msg.Message.RemoveAll();
		msg.Message.Add(body);
		mail.SendMessage(msg);
		mail.Close();
	}
	catch(CException *e)	{ if(connected) mail.Close();	throw; 	}
	catch(mroerr& e)		{ if(connected) mail.Close();	throw;	}
	catch(...)				{ if(connected) mail.Close();	throw;	}

	// + lo marcamos como mandado ---------------------------------------------------------------------------------------------------------
	command.Format(_T("update t_right_forms_hdr set status=1 where object_id=%s"), order);
	getconnection(con);
	con.execute(command);
	// - lo marcamos como mandado ---------------------------------------------------------------------------------------------------------
	
}


void CSessionMan::acf_add_trans()
{
//	acf_edit_trans();
}

void CSessionMan::acf_edit_trans()
{
	CString order	= _params.get(_T("cordini"),7);
	CString status	= _params.get(_T("cstatus"),7);
	CString trans	= _params.get(_T("ctrans"),6);

	TCHAR sys[ZSYSTEMMAX+1];
	_params.get(ZSYSTEM, sys, ZSYSTEMLEN, ZSYSTEMMAX);

	require(status != _T("0"), _T("request_already_sent"));

	CString command;
	command.Format(	_T("select desc_long from t_modules with (nolock) ")
					_T("where module_id='%s' and (system='%s' or system='MRO')"), trans, sys);
	getconnectionx(con, obj);
	con.execute(command.GetBuffer(), obj);
	CString desc = obj.IsEOF() ? _T("") : obj.get(_T("desc_long"));
	require(desc.IsEmpty(), _T("transaction_not_exist"));
	
	command.Format(	_T("delete t_right_forms_dtl where docid=%s and module_id='%s'; ")
					_T("insert into t_right_forms_dtl (docid, module_id, module_desc, ")
					_T("accesar, consultar, crear, modificar, borrar, comentarios, listas) ")
					_T("values(%s, '%s', '%s', '1','1', '1', '1','1', '1', '1'); "),
					order, trans,
					order, trans, desc);
	con.execute(command.GetBuffer());
}

void CSessionMan::acf_delete_trans()
{
	CString order	= _params.get(_T("cordini"),7);
	CString status	= _params.get(_T("cstatus"),7);
	CString trans	= _params.get(_T("ctrans"),6);

	require(status != _T("0"), _T("request_already_sent"));

	CString command;
	command.Format(_T("delete t_right_forms_dtl where docid=%s and module_id='%s'"), order, trans);
	getconnection(con);
	con.execute(command.GetBuffer());
}

void CSessionMan::acf_add_profile()
{
//	acf_edit_profile();
}

void CSessionMan::acf_edit_profile()
{
	CString order	= _params.get(_T("cordini"),7);
	CString status	= _params.get(_T("cstatus"),7);
	CString profile = _params.get(_T("cprofile"),8);

	require(status != _T("0"), _T("request_already_sent"));

	CString command;

	command.Format(	_T("select name from t_grps_hdr with (nolock) where object_id='%s'"), profile);
	getconnectionx(con, obj);
	con.execute(command.GetBuffer(), obj);
	CString desc = obj.IsEOF() ? _T("") : obj.get(_T("name"));
	require(desc.IsEmpty(), _T("profile_not_exist"));

	command.Format(	_T("delete t_right_forms_prf where docid=%s and group_id='%s'; ")
					_T("insert into t_right_forms_prf (docid, group_id, group_desc) ")
					_T("values(%s, '%s', '%s'); "),
					order, profile, 
					order, profile, desc);
	con.execute(command.GetBuffer());
}

void CSessionMan::acf_delete_profile()
{
	CString order	= _params.get(_T("cordini"),7);
	CString status	= _params.get(_T("cstatus"),7);
	CString profile = _params.get(_T("cprofile"),8);

	require(status != _T("0"), _T("request_already_sent"));

	CString command;
	command.Format(_T("delete t_right_forms_prf where docid=%s and group_id='%s'"), order, profile);
	getconnection(con);
	con.execute(command.GetBuffer());
}
*/
