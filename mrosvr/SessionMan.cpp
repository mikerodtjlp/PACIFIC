#include "stdafx.h"

//#include "mroctrl.h"
#include "SessionMan.h"
#include "mrofns.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/////////////////////////////////////////////////////////////////////////////
// CSessionMan

int MAXHISTORY		= -1;
int MAXSESSIONS		= -1;
int MAXUSERS		= -1;
int MAXMACHINES		= -1;
int MAXNAMES		= -1;
int MAXTRANS		= -1;

transactions* trans	= nullptr;
int trntop			= 0;
nombres* names		= nullptr;
int namtop			= 0;
maquina* machines = nullptr;
int mactop			= 0;

int instid			= -1;

CRITICAL_SECTION CSessionMan::sestrns;
CRITICAL_SECTION CSessionMan::sesuser;
CRITICAL_SECTION CSessionMan::csmachs;

map<CString, CString> CSessionMan::cache_docs;
CRITICAL_SECTION CSessionMan::cstrans;

//map<CString, map<CString, CString>> CSessionMan::cacherights;
//map<CString, map<CString, CString>>::iterator CSessionMan::endrights = CSessionMan::cacherights.end();
//CRITICAL_SECTION CSessionMan::csrights;

map<CString, CString> CSessionMan::cachedescs;
map<CString, CString>::iterator CSessionMan::enddescs = CSessionMan::cachedescs.end();
CRITICAL_SECTION CSessionMan::dsclock;

bool CSessionMan::loaded = false;
bool CSessionMan::initialized = false;
CSessionMan::FPD CSessionMan::_map_functions[MAXFUNCTIONS];
int CSessionMan::_max_funs = 0;
int __compare(const void* a, const void* b)
{
return _tcscmp(((CSessionMan::FPD*)a)->name, ((CSessionMan::FPD*)b)->name);
}
int __find_fun(const TCHAR* key)
{
	CSessionMan::FPD k;
	_tcscpy_s(k.name, key); 
	CSessionMan::FPD* result = (CSessionMan::FPD*)bsearch(&k, CSessionMan::_map_functions,
	CSessionMan::_max_funs, sizeof(CSessionMan::FPD),
	(int (*)(const void*, const void*))__compare); 
	if (result == 0) return -1; 
	int ret = static_cast<int>(&(*result) - &(CSessionMan::_map_functions[0]));
	return ret; 
}
bool CSessionMan::firsttime;
void __helper__(const TCHAR* name, CSessionMan::FP function)
{
	if (CSessionMan::_max_funs >= MAXFUNCTIONS) AfxMessageBox(_T("too_many_functions"));
	_tcscpy_s(CSessionMan::_map_functions[CSessionMan::_max_funs].name, name);
	CSessionMan::_map_functions[CSessionMan::_max_funs].function = function;
	++CSessionMan::_max_funs;
}
CSessionMan::CSessionMan()
{
	//EnableAutomation(); 
	//AfxOleLockApp(); 
	if (!firsttime)	{
		firsttime = true; 

		::InitializeCriticalSection(&cstrans);
	//	::InitializeCriticalSection(&csrights);
		::InitializeCriticalSection(&dsclock);
		::InitializeCriticalSection(&sestrns);
		::InitializeCriticalSection(&sesuser);
		::InitializeCriticalSection(&csmachs);

		// + functions to manipulate the access on the system
		__helper__(_T("can_enter")				, &CSessionMan::can_enter);
	//	__helper__(_T("process_rights")			, &CSessionMan::rights_process);
		__helper__(_T("process_profiles")		, &CSessionMan::process_profiles);
	//	__helper__(_T("load_rights")			, &CSessionMan::load_rights);
	//	__helper__(_T("reset_rights_cache")		, &CSessionMan::reset_rights_cache);
		__helper__(_T("haverig")				, &CSessionMan::have_right);
		__helper__(_T("lookrgt")				, &CSessionMan::look_4_rights);
		__helper__(_T("encrypt_password")		, &CSessionMan::encrypt_password);

		// functions that work with the multilingual stuff
		__helper__(_T("load_descriptions")		, &CSessionMan::load_descriptions);
		//__helper__(_T("load_error_descriptions"), &CSessionMan::load_error_descriptions);
		__helper__(_T("load_user_params")		, &CSessionMan::load_user_params);
		//__helper__(_T("reset_document_cache")	, &CSessionMan::reset_document_cache);
		__helper__(_T("reset_desc_cache")		, &CSessionMan::reset_desc_cache);
		//__helper__(_T("reset_error_cache")		, &CSessionMan::reset_error_cache);
		__helper__(_T("process_descriptions")	, &CSessionMan::process_descriptions);
		//__helper__(_T("process_error_descriptions"),&CSessionMan::process_error_descriptions);
		__helper__(_T("process_user_params")	, &CSessionMan::process_user_params);

		//__helper__(_T("transform_to_language")	, transform_to_language);
		__helper__(_T("get_description")		, &CSessionMan::get_description);
		__helper__(_T("get_descerror")			, &CSessionMan::get_description);

		// + functions to manipulate the session in the system
		__helper__(_T("begin_session"), &CSessionMan::begin_session);
		__helper__(_T("end_session"), &CSessionMan::end_session);
		__helper__(_T("reset_ghost_session"), &CSessionMan::reset_ghost_session);
		__helper__(_T("reset_ghost_sessions"), &CSessionMan::reset_ghost_sessions);
		__helper__(_T("release_sessions"), &CSessionMan::release_sessions);
		__helper__(_T("delete_session"), &CSessionMan::delete_session);
		__helper__(_T("delete_user_session"), &CSessionMan::delete_user_session);
		__helper__(_T("delete_machine_session"), &CSessionMan::delete_machine_session);
		__helper__(_T("copy_session"), &CSessionMan::copy_session);
		__helper__(_T("session_set_company"), &CSessionMan::session_set_company);

		// + functions for the session movement only applicable on the gui client
		__helper__(_T("gui_get_history"), &CSessionMan::gui_get_history);
		__helper__(_T("gui_get_session_data"), &CSessionMan::gui_get_session_data);
		__helper__(_T("gui_get_top"), &CSessionMan::gui_get_top);
		__helper__(_T("gui_get_pos"), &CSessionMan::gui_get_pos);
		__helper__(_T("gui_insert_trans"), &CSessionMan::gui_insert_trans);
		__helper__(_T("gui_get_entrance"), &CSessionMan::gui_get_entrance);
		__helper__(_T("gui_goto_trans"), &CSessionMan::gui_goto_trans);
		__helper__(_T("gui_go_back"), &CSessionMan::gui_go_back);
		__helper__(_T("gui_go_home"), &CSessionMan::gui_go_home);
		__helper__(_T("gui_go_frwd"), &CSessionMan::gui_go_forward);
		__helper__(_T("gui_go_pos"), &CSessionMan::gui_go_pos);
		//__helper__(_T("gui_get_texts"), &CSessionMan::gui_get_texts);

		// + functions to download some data, config files, etc..
		__helper__(_T("get_file"), &CSessionMan::_get_file);
		//__helper__(_T("get_logon_options"),		&CSessionMan::get_logon_options);

		// + functions to control the last data used in the system
		//__helper__(_T("get_last_css"), &CSessionMan::get_last_css);
		__helper__(_T("copy_state"), &CSessionMan::copy_state);
		__helper__(_T("get_user_logons"), &CSessionMan::get_user_logons);
	//	__helper__(_T("set_last_state"),		&CSessionMan::set_last_state);
		__helper__(_T("get_last_state"), &CSessionMan::get_last_state);
		//__helper__(_T("get_last_state_nosafe"), &CSessionMan::get_last_state_nosafe);

		// important function it generates the function string to be executed by the service
		__helper__(_T("get_final_fun"), &CSessionMan::get_final_fun);

		// + functions to monitoring the statistics
		__helper__(_T("get_files_info"), &CSessionMan::get_files_info);
		__helper__(_T("get_client_history"), &CSessionMan::get_client_history);
		__helper__(_T("get_sessions_info"), &CSessionMan::get_sessions_info);
		__helper__(_T("get_sessions_count"), &CSessionMan::get_sessions_count);

		__helper__(_T("notify_use"), &CSessionMan::notify_use);
		__helper__(_T("check_session"), &CSessionMan::check_session);
		__helper__(_T("save_sessions"), &CSessionMan::save_sessions);
		__helper__(_T("update_session_time"), &CSessionMan::update_session_time);

		// functions for helping out the debugging
		__helper__(_T("execute"), &CSessionMan::execute);

		initialize_funs_dll();
		__helper__(_T("reset_state"), &CSessionMan::reset_state); 
		__helper__(_T("get_functions_info"), &CSessionMan::_get_functions_info); 
		qsort((FPD*)&_map_functions, _max_funs, sizeof(FPD), (int (*)(const void*, const void*))__compare); 
	}
}

#define COMPONENTNAME _T("mrosvr")

void CSessionMan::SetParameters(LPCTSTR params, const int len) {
	try	{
		_params.set_value(params, len); 
		_params.optimize(); 
	}
	catch (const TCHAR* e) {
		if (e == 0) process_error(_T("unhandled_error"), _T(""), _T(""), callstack, _T("SetParameters"), _T(""), -1, _T("wchar_t*"));
		else process_error(e, _T(""), _T(""), callstack, _T("SetParameters"), _T(""), -1, _T("wchar_t*"));
	}
	catch (CString & e)	{
		if (e.IsEmpty())	process_error(_T("unhandled_error"), _T(""), _T(""), callstack, _T("SetParameters"), _T(""), -1, _T("CString&"));
		else process_error(e, _T(""), _T(""), callstack, _T("SetParameters"), _T(""), -1, _T("CString&"));
	}
	catch (CException * e) {
		TCHAR szCause[512]; 
		e->GetErrorMessage(szCause, 512); 
		e->Delete(); 
		process_error(szCause, _T(""), _T(""), callstack, _T("SetParameters"), _T(""), -1, _T("CException*"));
	}
	catch (mroerr & e) {
		process_error(e.description, e.extrainfo, _T(""), callstack, e.function, e.errfile, e.errline, _T("mroerr&"));
	}
	catch (...)	{
		process_error(_T("unhandled_exception"), _T(""), _T(""), callstack, _T("SetParameters"), _T(""), -1, _T("...")); 
	}
}
BSTR CSessionMan::GetParameters()
{
	if (true) component_post(); 
	CString r(_params.buffer(), _params.get_len()); 
	return r.AllocSysString(); 
}
void CSessionMan::SetBasics(LPCTSTR basics, const int len) {
	try	{
		_basics.set_value(basics, len); 
		_basics.optimize(); 
		newbasics = true; 
	}
	catch (const TCHAR* e)	{
		if (e == 0) process_error(_T("unhandled_error"), _T(""), _T(""), callstack, _T("SetBasics"), _T(""), -1, _T("wchar_t*"));
		else process_error(e, _T(""), _T(""), callstack, _T("SetBasics"), _T(""), -1, _T("wchar_t*"));
	}
	catch (CString & e)	{
		if (e.IsEmpty())	process_error(_T("unhandled_error"), _T(""), _T(""), callstack, _T("SetBasics"), _T(""), -1, _T("CString&"));
		else process_error(e, _T(""), _T(""), callstack, _T("SetBasics"), _T(""), -1, _T("CString&"));
	}
	catch (CException * e)	{
		TCHAR szCause[512]; 
		e->GetErrorMessage(szCause, 512); 
		e->Delete(); 
		process_error(szCause, _T(""), _T(""), callstack, _T("SetBasics"), _T(""), -1, _T("CException*"));
	}
	catch (mroerr & e)	{
		process_error(e.description, e.extrainfo, _T(""), callstack, e.function, e.errfile, e.errline, _T("mroerr&"));
	}
	catch (...)	{
		process_error(_T("unhandled_exception"), _T(""), _T(""), callstack, _T("SetBasics"), _T(""), -1, _T("..."));
	}
}
BSTR CSessionMan::GetValuesToChange()
{
	CString r(_values.buffer()); 
	return r.AllocSysString(); 
}

bool CSessionMan::DoOk(LPCTSTR fun, const int funlen) {
	_values.clear();
	TCHAR error[2048];
	function = nullptr;
	try
	{
		if(true && newbasics) { component_set_basics(); newbasics=false; }
		if(!loaded)
		{
			if(true && mro::memhelper::memgbl == 0 && 
				_basics.has(_T("maxprcs"), 7))
			{
				int max_procs = _basics.getint(_T("maxprcs"), 7);
				mro::memhelper::initialize_memhelper(max_procs);
			}
			if(dbhelper::dbgbl == 0 && _basics.has(_T("timetol"), 7) && 
				_basics.has(_T("dbm"), 3))
			{
				_basics.get(ZADOCON, dbhelper::connection, ZADOCONLEN, 1023);
				int tolerance = _basics.getint(_T("timetol"), 7, 300);
				int maxcons = _basics.getint(_T("maxdbcn"), 7, 16);
				dbhelper::timeout = tolerance;
				dbhelper::maxconnections = maxcons;
				TCHAR pc[1024];
				_basics.get(_T("dbm"), pc, 3, 65);
				dbhelper::dbgbl = CMroModule::get_db_pointer(pc);
			}
			if(!initialized) { initialized = component_initialization(); }
			if(dbhelper::dbgbl && initialized && !loaded) loaded = true;
		}

#ifdef _DEBUG
		callstack.Empty();
#endif

		int funid = __find_fun(fun);
		if(funid != -1)
		{
			function = &_map_functions[funid];
			++function->executions;
			(*this.*function->function)();
			return true;
		}
		int len = mikefmt(error, _T("function %s not fund on %s comp"), fun, COMPONENTNAME);
		_params.set(ZSERROR, error, ZSERRORLEN, len);
	}
	catch(const TCHAR* e)
	{
		if(e == 0) process_error(_T("unhandled_error"), _T(""), _T(""), callstack, fun, _T(""), -1, _T("wchar_t*"));
		else process_error(e, _T(""), _T(""), callstack, fun, _T(""), -1, _T("wchar_t*"));
	}
	catch(CString& e)
	{
		if(e.IsEmpty())	process_error(_T("unhandled_error"), _T(""), _T(""), callstack, fun, _T(""), -1, _T("CString&"));
		else process_error(e, _T(""), _T(""), callstack, fun, _T(""), -1, _T("CString&"));
	}
	catch(_com_error &er)
	{
		CString e = (TCHAR*) er.Description();
		if(e.IsEmpty()) process_error(_T("unhandled_error"), _T(""), _T(""), callstack, fun, _T(""), -1,_T("_com_error"));
		else process_error(e, _T(""), e, callstack, fun, _T(""), -1,_T("_com_error"));
	}
	catch(CException* e)
	{
		TCHAR szCause[512];
		e->GetErrorMessage(szCause, 512);
		e->Delete();
		process_error(szCause, _T(""), _T(""), callstack, fun, _T(""), -1, _T("CException*"));
	}
	catch(mroerr& e)
	{
		process_error(e.description, e.extrainfo, _T(""), callstack, e.function, e.errfile, e.errline, _T("mroerr&"));
	}
	catch(...)
	{
		process_error(_T("unhandled_exception"), _T(""), _T(""), callstack, fun, _T(""), -1, _T("..."));
	}
	if(function) ++function->errors;
	return false;
}
void CSessionMan::process_error(const TCHAR* text, 
								const TCHAR* extra, 
								const TCHAR* info,
								const TCHAR* callstack,
								const TCHAR* funname, 
								const TCHAR* errfile, 
								const int errline, 
								const TCHAR* errclass)
{
	try
	{
		CString efile(errfile);
		efile.Replace(_T('"'),_T('\''));
		efile.Replace(_T('\\'),_T('/'));
		mroerr err(text, 1,extra, info, COMPONENTNAME, callstack, funname, efile, errline, errclass); 
		CString extrainfo;
		err.to_params(extrainfo);
		_params.set(ZSERROR	, text		, ZSERRORLEN);
		_params.set(ZERRORI	, extrainfo	, ZERRORILEN);
	}
	catch(CException *e)	{ e->Delete();	}
	catch(mroerr& e)		{				}
	catch(...)				{				}
}
void CSessionMan::_get_functions_info()
{
	CString strfinal;
	TCHAR row[1024];

	TCHAR list_row[8];
	list_row[0] = _T('l');
	list_row[1] = _T('0');

	int irow = 0;
	for(int i = 0; i < _max_funs; ++i)
	{
		FPD* f = &_map_functions[i];
		mikefmt(&list_row[2], _T("%d"), irow);

		int len = mikefmt(row, _T("[%sA:%s][%sB:%d][%sC:%d][%s*:%d]"), 
							list_row, f->name, 
							list_row, f->executions, 
							list_row, f->errors, 
							list_row, 0);
		strfinal.Append(row, len);
		++irow;
	}
	int len = gen_tot_list(row,0,irow);
	strfinal.Append(row, len);
	_params.append(strfinal.GetBuffer(), strfinal.GetLength());
}

void CSessionMan::reset_state()
{
	::LeaveCriticalSection(&cstrans);
	//::LeaveCriticalSection(&csrights);
	::LeaveCriticalSection(&dsclock);
	::LeaveCriticalSection(&sestrns);
	::LeaveCriticalSection(&sesuser);
	::LeaveCriticalSection(&csmachs);
}

bool CSessionMan::component_initialization()
{
	if(!machines)
	{
		CParameters comp;
		comp.set_value(gcfgpcom);
		//if(_basics.get(ZCTROBJ, comp, ZCTROBJLEN))
		//{
			MAXTRANS = comp.getint(_T("maxtrns"), 7, -1);
			if (MAXTRANS == -1 || MAXTRANS == 0) MAXTRANS = DEFMAXTRANS;
			MAXNAMES = comp.getint(_T("maxnams"), 7, -1);
			if (MAXNAMES == -1 || MAXNAMES == 0) MAXNAMES = DEFMAXNAMES;
			MAXMACHINES = comp.getint(_T("maxmacs"), 7, -1);
			if (MAXMACHINES == -1 || MAXMACHINES == 0) MAXMACHINES = DEFMAXMACHINES;
			MAXUSERS = comp.getint(_T("maxusrs"), 7, -1);
			if (MAXUSERS == -1 || MAXUSERS == 0) MAXUSERS = DEFMAXUSERS;
			MAXSESSIONS = comp.getint(_T("maxsess"), 7, -1);
			if (MAXSESSIONS == -1 || MAXSESSIONS == 0) MAXSESSIONS = DEFMAXSESSIONS;
			MAXHISTORY = comp.getint(_T("maxhist"), 7, -1);
			if (MAXHISTORY == -1 || MAXHISTORY == 0) MAXHISTORY = DEFMAXHISTORY;

			if(machines = (maquina*)malloc(sizeof(maquina)*MAXMACHINES))
			{
				trans = (transactions*)malloc(sizeof(transactions)*MAXTRANS);
				for (int i=MAXTRANS-1; i>= 0;--i) trans[i].init();
				names = (nombres*)malloc(sizeof(nombres)*MAXNAMES);
				for (int i=MAXNAMES-1; i>= 0;--i) names[i].init();

				memset(machines, 0, sizeof(maquina)*MAXMACHINES);
				for(int i=MAXMACHINES-1; i>=0; --i)
				{
					machines[i].init();
					usuario* usr = machines[i].users = (usuario*)malloc(sizeof(usuario)*MAXUSERS);//machines[i].users;
					for(int j=MAXUSERS-1; j>=0; --j)
					{
						usr[j].init();
						session* sess = usr[j].sessions = (session*)malloc(sizeof(session)*MAXSESSIONS);//usr[j].sessions;
						for(int k=MAXSESSIONS-1; k>=0; --k)
						{
							sess[k].init();
							historia* his = sess[k].history = (historia*)malloc(sizeof(historia)*MAXHISTORY);//sess[k].history;
							for(int l=MAXHISTORY-1; l>=0; --l) 
								his[l].init();
						}
					}
				}
				load_sessions();
				// preload the most common languages ES and EN
				_params.set(ZLANGUA,_T("ES"),ZLANGUALEN,2);
				load_descriptions();
				_params.set(ZLANGUA,_T("EN"),ZLANGUALEN,2);
				load_descriptions();
				return true;
			}
		//}
	}

	return false;
}

void CSessionMan::component_set_basics()
{
	macid = usrid = sesid = -1;
	if ((macid = _basics.getint(ZSESMAC, ZSESMACLEN, -1)) == -1) return;
	if ((usrid = _basics.getint(ZSESCLI, ZSESCLILEN, -1)) == -1) return;
	if ((sesid = _basics.getint(ZSESSES, ZSESSESLEN, -1)) == -1) return;
}

void CSessionMan::component_post(){}

void CSessionMan::load_sessions()
{
	TCHAR sys[ZSYSTEMMAX+1]; 
	_basics.get(ZSYSTEM,sys,ZSYSTEMLEN,ZSYSTEMMAX);

	TCHAR sql[1024];
	mikefmt(sql,_T("exec dbo.env_get_mactop '%s';"),sys);
	getconnectionx(con, obj);
	con.execute(sql, obj);
	if(!obj.IsEOF())
	{
		instid = obj.getint(0);
		mactop = obj.getint(1);
	}

	mikefmt(sql, _T("exec dbo.env_clean %d,%d,%d,%d,%d;"),
		instid, MAXMACHINES, MAXUSERS, MAXSESSIONS, MAXHISTORY);
	con.execute(sql);

	mikefmt(sql,_T("exec dbo.env_get_his %d;"),instid);
	con.execute(sql, obj);
	for(;!obj.IsEOF();obj.MoveNext())
	{
		historia* his = &machines[obj.getint(1)].
						users[obj.getint(2)].
						sessions[obj.getint(3)].
						history[obj.getint(4)];
		int l = obj.get(5, sql, CMPMAX);
		his->trn = findtrans(sql, l);
	}

	mikefmt(sql,_T("exec dbo.env_get_ses %d;"),instid);
	con.execute(sql, obj);
	for(;!obj.IsEOF();obj.MoveNext())
	{
		session* ses =	&machines[obj.getint(1)].
						users[obj.getint(2)].
						sessions[obj.getint(3)];
		ses->online  = obj.getint(4);
		ses->reseted = obj.getint(5);
		ses->hispos  = obj.getint(6);
		ses->histop  = obj.getint(7);
		ses->access  = obj.getint(8);
		obj.getdate(9, ses->lastcontact);
		obj.getdate(10, ses->start);
		ses->cmpy    = obj.getint(11);
		ses->agetyp  = obj.getint(12);
		ses->ismobi  = obj.getint(13);
	}

	CString tmplparms;
	mikefmt(sql,_T("exec dbo.env_get_usr %d;"),instid);
	con.execute(sql, obj);
	for(;!obj.IsEOF();obj.MoveNext())
	{
		usuario* usr = &machines[obj.getint(1)].
						users[obj.getint(2)];
		int lus = obj.get(3, sql, USRSIZE);
		usr->id = finduser(sql,lus);
		usr->sestop = obj.getint(4);
		usr->access = obj.getint(5);
		obj.get(6,tmplparms);
		if(usr->tmplparms == nullptr) 
			usr->tmplparms = new cpairs(tmplparms);
	}

	mikefmt(sql,_T("exec dbo.env_get_mac %d;"),instid);
	con.execute(sql, obj);
	for(;!obj.IsEOF();obj.MoveNext())
	{
		maquina* mac = &machines[obj.getint(1)];
		mac->naml	= obj.get(2,mac->name	, ZMACNAMMAX);
		mac->adrl	= obj.get(3,mac->ip		, ZIPADDRMAX);
		mac->mcal	= obj.get(4,mac->macaddr, ZMACADRMAX);
		mac->usrtop = obj.getint(5);
	}

	reset_ghost_sessions();		// eliminate wasteful information
}

/*void CSessionMan::save_sessions()
{
	if (instid == -1) return;	// concurrency triggered save while loading
	reset_ghost_sessions();		// eliminate wasteful information

	CString macs;
	CString usrs;
	CString sesn;
	CString hist;
	TCHAR cmd[1024];
	int len = 0;

	// 0 full, 1 machines, 2 users, 3 sessions, 4 history
	int type = _params.getint(_T("type"),4);
	if(type == 0 || type == 1)
	{
		if (mactop < MAXMACHINES)
		{
			len = mikefmt(cmd, _T("exec env_set_mactop %d,%d;\r\n"), instid, mactop);
			macs.Append(cmd, len);
		}
	}
	for(int i=0; i<mactop && i<MAXMACHINES; ++i)
	{
		maquina* mac = &machines[i];
		if(type == 0 || type == 1)
		{
			len = mikefmt(cmd, _T("exec mac_ins %d,%d,'%s','%s','%s',%d;\r\n"),
								instid,i,
								mac->name,
								mac->ip, 
								mac->macaddr,
								mac->usrtop);
			macs.Append(cmd, len);
		}
		int usrtop = mac->usrtop;
		for(int j=0; j<usrtop && j<MAXUSERS; ++j)
		{
			usuario* usr = &mac->users[j];
			if(type == 0 || type == 2)
			{
				len = mikefmt(cmd, _T("exec usr_ins %d,%d,%d,'%s',%d,%d,'%s';\r\n"),
									instid,i,j,
									usr->id->name,usr->sestop,usr->access,
									usr->tmplparms ? usr->tmplparms->buffer():_T(""));
				usrs.Append(cmd, len);
			}
			int sestop = usr->sestop;
			for(int k=0; k<sestop && k<MAXSESSIONS; ++k)
			{
				session* sess = &usr->sessions[k];
				if(type == 0 || type == 3)
				{
					len = mikefmt(cmd,	_T("exec ses_ins ")
										_T("%d,%d,%d,%d,%d,%d,%d,%d,%d,'%s','%s',%d,%d,%d;\r\n"),
										instid,i,j,k,
										sess->online,sess->reseted,
										sess->hispos,sess->histop,sess->access,
										sess->lastcontact.Format(_T("%Y/%m/%d %H:%M:%S")),
										sess->start.Format(_T("%Y/%m/%d %H:%M:%S")),
										sess->guityp,
										sess->agetyp,
										sess->guiver);
					sesn.Append(cmd, len);
				}

				if(!sess->online) continue;
				int histop = sess->histop;
				for(int l=0; l<=histop && l<MAXHISTORY; ++l)
				{
					historia* his = &sess->history[l];
					if(type == 0 || type == 4)
					{
						len = mikefmt(cmd, _T("exec his_ins %d,%d,%d,%d,%d,'%s';\r\n"),
										instid,i,j,k,l,
										his->trn ? his->trn->name : _T(""));
						hist.Append(cmd, len);
					}
				}
			}
		}
	}

	getconnection(con);
	if(!macs.IsEmpty()) con.execute(macs);
	if(!usrs.IsEmpty()) con.execute(usrs);
	if(!sesn.IsEmpty()) con.execute(sesn);
	if(!hist.IsEmpty()) con.execute(hist);
}
*/
void CSessionMan::save_sessions()
{
	if (instid == -1) return;	// concurrency triggered save while loading
	reset_ghost_sessions();		// eliminate wasteful information

	CString macs;
	CString usrs;
	CString sesn;
	CString hist;
	TCHAR cmd[1024];
	int len = 0;

	if (mactop < MAXMACHINES)
	{
		len = mikefmt(cmd, _T("exec dbo.env_set_mactop %d,%d;\r\n"), instid, mactop);
		macs.Append(cmd, len);
	}
	for (int i = 0; i<mactop && i<MAXMACHINES; ++i)
	{
		maquina* mac = &machines[i];
		int usrtop = mac->usrtop;
		bool maconline = false;
		for (int j = 0; j<usrtop && j<MAXUSERS; ++j)
		{
			usuario* usr = &mac->users[j];
			int sestop = usr->sestop;
			bool usronline = false;
			for (int k = 0; k<sestop && k<MAXSESSIONS; ++k)
			{
				session* sess = &usr->sessions[k];
				bool online = sess->online;
				if (online) {
					usronline = maconline = true;
					len = mikefmt(cmd, _T("exec dbo.ses_ins ")
						_T("%d,%d,%d,%d,%d,%d,%d,%d,%d,'%s','%s',%d,%d,%d;\r\n"),
						instid, i, j, k,
						online, sess->reseted,
						sess->hispos, sess->histop, sess->access,
						sess->lastcontact.Format(_T("%Y/%m/%d %H:%M:%S")),
						sess->start.Format(_T("%Y/%m/%d %H:%M:%S")),
						sess->cmpy,
						sess->agetyp,
						sess->ismobi);
					sesn.Append(cmd, len);
					int histop = sess->histop;
					for (int l = 0; l <= histop && l < MAXHISTORY; ++l)
					{
						historia* his = &sess->history[l];
						len = mikefmt(cmd, _T("exec dbo.his_ins %d,%d,%d,%d,%d,'%s';\r\n"),
							instid, i, j, k, l,
							his->trn ? his->trn->name : _T(""));
						hist.Append(cmd, len);
					}//history
				}
			}//sessions
			if (!usronline) continue;
			len = mikefmt(cmd, _T("exec dbo.usr_ins %d,%d,%d,'%s',%d,%d,'%s';\r\n"),
				instid, i, j,
				usr->id->name, usr->sestop, usr->access,
				usr->tmplparms ? usr->tmplparms->buffer() : _T(""));
			usrs.Append(cmd, len);
		}//users
		if (!maconline) continue;
			len = mikefmt(cmd, _T("exec dbo.mac_ins %d,%d,'%s','%s','%s',%d;\r\n"),
			instid, i,
			mac->name,
			mac->ip,
			mac->macaddr,
			mac->usrtop);
		macs.Append(cmd, len);
	}//machines

	// set all to offline and only save the online turning on
	mikefmt(cmd, _T("update sessions set online=0 where instid=%d;"), instid);
	getconnection(con);
	con.execute(cmd);
	if (!macs.IsEmpty()) con.execute(macs);
	if (!usrs.IsEmpty()) con.execute(usrs);
	if (!sesn.IsEmpty()) con.execute(sesn);
	if (!hist.IsEmpty()) con.execute(hist);
}

void CSessionMan::execute()
{
	TCHAR function[64];
	int funlen = _params.get(_T("fun2exe"), function, 7, 63);
	require(function[0] == 0,	FUN_MISSING);

	// belive it or not there are function that are null
	bool nonefun =	funlen == 7 &&	cmp4ch(&function[0], _T('n'),_T('o'),_T('n'),_T('e')) & 
									cmp4ch(&function[4], _T('f'),_T('u'),_T('n'),0);

	if(!nonefun) DoOk(function, funlen);
}

void CSessionMan::get_files_info()
{
	CString strfinal;
	TCHAR row[1024];

	TCHAR list_row[8];
	list_row[0] = _T('l');
	list_row[1] = _T('0');

	int irow = 0;
	std::map<CString, CString>::iterator end = cache_docs.end();
	for(std::map<CString, CString>::iterator iter = cache_docs.begin(); iter != end; ++iter)
	{
		//CString* f = &(*iter).second;
		mikefmt(&list_row[2], _T("%d"), irow);
		int len = mikefmt(row, _T("[%sA:%s][%sB:%d][%sC:%d][%s*:%d]"), 
							list_row, (*iter).first, 
							list_row, 0, //f->accesses, 
							list_row, 0, 
							list_row, 0);
		strfinal.Append(row, len);
		++irow;
	}
	int len = gen_tot_list(row,0,irow);
	strfinal.Append(row, len);
	_params.append(strfinal);
}

void CSessionMan::load_descriptions()
{
	TCHAR lang[3];
	int ll = _params.get(ZLANGUA,lang,ZLANGUALEN,2);

	map<CString, CString> local;

	TCHAR key[128+1];
	TCHAR val[128];
	CString skey;
	CString sval;
	mikefmt(key, _T("exec dbo.desc_all_by_lang '%s';"), lang);
	{
		getconnectionx(con, obj);
		con.execute(key, obj);
		set2ch(key,lang[0],lang[1]); key[2] = 0;
		for(;!obj.IsEOF();obj.MoveNext())
		{
			int keylen = obj.get(0, &key[2], 64);
			skey.SetString(key, keylen+2);

			keylen = obj.get(1, val, 64);
			sval.SetString(val, keylen);
			local.insert(map<CString, CString>::value_type(skey, sval));
		}
	}

	try
	{
		::EnterCriticalSection(&dsclock);
		cachedescs = local;
		enddescs = cachedescs.end();
		::LeaveCriticalSection(&dsclock);
	}
	catch(_com_error &e){ ::LeaveCriticalSection(&dsclock); throw; }
	catch(CException *e){ ::LeaveCriticalSection(&dsclock); throw; }
	catch(mroerr& e)	{ ::LeaveCriticalSection(&dsclock); throw; }
	catch(...)			{ ::LeaveCriticalSection(&dsclock); throw; }
}

void CSessionMan::load_user_params()
{
	TCHAR user[ZUSERIDMAX + 1];
	int ulen = _basics.get(ZUSERID, user, ZUSERIDLEN, ZUSERIDMAX);
	for(int i = 0; i < mactop; ++i)
	{
		maquina& mac = machines[i];
		int nusrs = mac.usrtop;
		for(int j = 0; j < nusrs; ++j)
		{
			usuario& usr = mac.users[j];
			nombres* id = usr.id;
			if(!id || id->len == 0 || id->len > 16 || !usr.tmplparms) continue; 
			if(ulen)
			{
				if(ulen == id->len && _tmemcmp(id->name, user, ulen) == 0)
					usr.tmplparms->clear();
			}
			else usr.tmplparms->clear();
		}
	}
}


