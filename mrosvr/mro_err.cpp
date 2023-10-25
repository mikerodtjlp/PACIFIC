#include "stdafx.h"

/************************************************************************************
* description   : mro_err
* purpose       : functions that deals the function for the errors
* author        : miguel rodriguez
* date creation : 20 junio 2004
* change        : 1 marzo call coms direct from the function execute instead of CMroCom::call_com)
* change        : 10 febrero  change to global queue array, and the access controled by semaphores
**************************************************************************************/

#include "mrosvr.h"

void get_error_description(	sClients& clie, 
														sProcess& proc, 
														CParameters& result, 
														const bool fromcomp) {
	defchar(desc,512);
	defchar(extra,512);
	defchar(translated,1024);
	int len = result.get(ZSERROR, desc, ZSERRORLEN, 511);
	if(len > 0 && len < 64 && !_tmemchr(desc, ' ', len)) {
		int translen = 0;
		try	{ 
			translen = get_error(clie, proc, desc, len, translated, 512, fromcomp); 
		}
		catch(CException* e)	{	e->Delete();	_tcscpy_s(translated, _T("get_error")); translen = 17; }
		catch(mroerr& e)			{								_tcscpy_s(translated, _T("get_error")); translen = 17; }
		catch(...)						{								_tcscpy_s(translated, _T("get_error")); translen = 17; }

		if(translen != len || _tcscmp(translated, desc)) {
			CParameters errinf;
			if(result.get(ZERRORI, errinf, ZERRORILEN)) {
				if(int extlen = errinf.get(ZHERROR, extra, ZHERRORLEN)) {
					translated[translen++] = ':';
					_tmemcpy(&translated[translen], extra, extlen);
					translen += extlen;
					if(translen >= (sizeof(translated)/sizeof(TCHAR))-1) { set2ch(translated, '?',0); translen = 1; }
					else translated[translen] = 0;
				}
				errinf.set(ZSERROR, translated, ZSERRORLEN, translen);
				result.set(ZERRORI, errinf, ZERRORILEN);
			}
			result.set(ZSERROR, translated, ZSERRORLEN, translen);
		}
	}
}

/*
	this function write the error to the event viewer
*/
void write2event(DWORD type, const TCHAR* msg, const int step) {
	TCHAR date_time[128];
	TCHAR buf_time[32];
	TCHAR buf_date[32];
	_tstrtime(buf_time);
	_tstrdate(buf_date);
	mikefmt(date_time, _T("(%s %s)"), buf_date, buf_time);

	TCHAR message[2048];
	int l = mikefmt(message, _T("%s - service(%s:%d)(%d) %s"), date_time, gsvrtype, localprt, step, msg);
	if(l < 2047) {
		if(!gexename[0]) return;
		TCHAR logname[128];
		mikefmt(logname, _T("%sLOG.htm"), gexename);
		CUTF16File outputfile;
		if(mro::exist_file(logname)) {
			outputfile.Open(logname, CFile::modeWrite);
			outputfile.SeekToEnd();
		}
		else {
			outputfile.Open(logname, CFile::modeCreate | CFile::modeWrite);
            outputfile.WriteString(	_T("<title>startup</title>")
									_T("<body>"));
		}
		if(type == EVENTLOG_ERROR_TYPE) {
            outputfile.WriteString(_T("<font style=\"font-size: 12px; color: #ff0000; font-family:consolas;\">"));
			outputfile.WriteString(_T("error: "));
		}

		outputfile.WriteString(message);

		if(type == EVENTLOG_ERROR_TYPE) 
            outputfile.WriteString(_T("<font style=\"font-size: 12px; color: #000000; font-family:consolas;\">"));

		outputfile.WriteString(_T("<br/>"));
		outputfile.Close();
	}
}

void manage_exception(	CParameters& result, const TCHAR* text, const TCHAR* extra, 
						const TCHAR* info, const TCHAR* funname, const TCHAR* errfile, 
						const int errline, const int step) {
	try {
		// must check if it must return json
		CString efile(errfile);
		efile.Replace(_T('"'),_T('\''));
		efile.Replace(_T('\\'),_T('/'));
		mroerr err(text, 1,extra, info, _T("mrosvr"), _T(""), funname, efile, errline, _T(""));
		err.step = step;
		CString extrainfo;
		err.to_params(extrainfo);

		result.set(ZSERROR		, text		, ZSERRORLEN);
		result.set(ZERRORI		, extrainfo	, ZERRORILEN);
	}
	catch(CException *e)	{ e->Delete();	}
	catch(mroerr& e)			{ }
	catch(...)						{ }
}

void process_error(sClients& clie, sProcess& proc,
				   CParameters& result,
				   const bool saveerror,
				   const int errid) {
	if(!result.hasval(ZSERROR, ZSERRORLEN))
		result.set(ZSERROR, _T("unhandled_error"), ZSERRORLEN, 15);

	CParameters einf;
	if(result.get(ZERRORI, einf, ZERRORILEN))	{
		if(!einf.hasval(ZSERROR, ZSERRORLEN))
			einf.set(ZSERROR, _T("unhandled_error"), ZSERRORLEN, 15);
		einf.set(ZSVRTYP, gsvrtype	, ZSVRTYPLEN, ZSVRTYPMAX);
		einf.set(ZDOMAIN, gdomain	, ZDOMAINLEN, ZDOMAINMAX);
		result.set(ZERRORI, einf, ZERRORILEN);
	}
}

int get_error(	sClients& clie, sProcess& proc, 
								const TCHAR* code, const int codelen, 
								TCHAR* errdesc, const int errdesclen, 
								const bool fromcomp) {
    // is obvious that codes that are too long are descriptions not codes so we know that they dont
    // exist on the database so the best we can do is return the string to be inspected by the client
    if(codelen > 64) {
		_tmemcpy(errdesc, code, codelen + 1); 
		return codelen; 
	}

	CParameters params;
	//if(isgate) {
		params.set(_T("codeid"), code, 6, codelen);
		core_call(params, _T("get_descerror"), 13, params, proc);
	//}
	//else {
	//	TCHAR cmd[1024];
	//	int len = mikefmt(cmd, _T("[%s:%s][%s:1][zfun00z:[%s:%s][%s:%s][%s:%s][%s:%s]]"), 
	//							ZBASICS, clie.basics.buffer(),
	//							ZZNFUNS, 
	//							ZTYPCOM, _T("com"), ZCOMPNM, ZCTROBJ, ZFUNNAM, 
	//							_T("get_descerror"), _T("codeid"), code);
	//	proc.gatesock.execute(clie.pgatsvr, clie.gatprt, cmd, len, params);
	//	params.optimize();
	//}

	int l = params.get(_T("realdesc"), errdesc, 8, errdesclen -1);
	if(fromcomp) return l;
	if(!cmp2ch(errdesc, '?', 0)) return l;

	_tmemcpy(errdesc, code, codelen + 1); 
	return codelen; 
}
