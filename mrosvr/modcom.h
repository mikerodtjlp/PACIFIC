#pragma once

// IModpass wrapper class

// Check windows
#if _WIN32 || _WIN64
#if _WIN64
#define ENVIRONMENT64
#else
#define ENVIRONMENT32
#endif
#endif

#define MAXCOMS 6

KEYDECL(ZDCSWK1, _T("dcswrk01.COMMDCS"));						// control component
KEYDECL(ZDCSWK2, _T("dcswrk02.COMMDCS"));						// control component
KEYDECL(ZDCSWK3, _T("dcswrk03.COMMDCS"));						// control component
KEYDECL(ZDCSEX1, _T("dcsex1.comobj"));						// control component
KEYDECL(ZDCSREP, _T("dcsrp001.ObjRep"));						// control component

extern CLSID clsids[];
extern TCHAR* coms[];
extern TCHAR* libraries[];
int comparecom(TCHAR **arg1, TCHAR **arg2);
int find_com2exec(const TCHAR* key, const int keylen);

class IModpass : public COleDispatchDriver
{
public:
	IModpass() {}; 
	IModpass(LPDISPATCH pDispatch)			: COleDispatchDriver(pDispatch)		{}; 
	IModpass(const IModpass& dispatchSrc)	: COleDispatchDriver(dispatchSrc)	{}; 

	void SetParameters(LPCTSTR params, const int len)
	{
		static BYTE parms[] = VTS_BSTR VTS_I2;
		InvokeHelper(0x1, DISPATCH_METHOD, VT_EMPTY, NULL, parms, params, len);
	}
	void __get_parameters(CString& result)
	{ 
		InvokeHelper(0x2, DISPATCH_METHOD, VT_BSTR, (void*)&result, NULL); 
	}
	short DoOk(LPCTSTR fun, const int len)
	{
		short result;
		static BYTE parms[] = VTS_BSTR VTS_I2;
		InvokeHelper(0x3, DISPATCH_METHOD, VT_I2, (void*)&result, parms, fun, len);
		return result;
	}
	void SetBasics(LPCTSTR basics, const int len)
	{
		static BYTE parms[] = VTS_BSTR VTS_I2;
		InvokeHelper(0x4, DISPATCH_METHOD, VT_EMPTY, NULL, parms, basics, len);
	}
	void __get_values_to_change(CString& result)	
	{ 
		InvokeHelper(0x5, DISPATCH_METHOD, VT_BSTR, (void*)&result, NULL); 
	}

	bool mroCreateDispatch(LPCTSTR szDllName,  REFCLSID clsid, COleException* pError = NULL);
};

class CMroModule
{
public:
	CMroModule() {};
	CMroModule(			const TCHAR* component	, const int complen,
						CParameters& parameters , 
						const TCHAR* command	, const int cmdlen,
						CParameters& basics)
	{ 
		call_com_module(component, complen, parameters, command, cmdlen, basics); 
	};

	void call_com_module(const TCHAR* component	, const int complen,
						CParameters& parameters , 
						const TCHAR* command	, const int cmdlen,
						CParameters& basics)
	{
		CString result;
		IModpass comp;
		TCHAR realcomp[128];
		TCHAR curpath[128];

		try
		{
			basics.active(_T("nodbini"), 7);
			basics.get(CURPATH, curpath, CURPATHLEN, 127);

			bool isdebugging = basics.isactv(ZISDEBG, ZISDEBGLEN);

			if(isdebugging)
			{
				if(!comp.CreateDispatch(component))
				{
					CString error;
					error.Format(_T("can't find in reg the component: %s"), realcomp);
					require(true, error);
				}
			}
			else
			{
				int compid = find_com2exec(component, complen);
				require(compid == -1, _T("component to link not found"));

				mikefmt(realcomp, _T("%s\\coms\\%s"), curpath, libraries[compid]);
				if(!comp.mroCreateDispatch(realcomp, clsids[compid]))
				{
					CString error;
					error.Format(_T("can't find in path the component: %s"), realcomp);
					require(true, error);
				}
			}

			comp.SetBasics(basics.buffer(), basics.get_len());
			comp.SetParameters(parameters.buffer(), parameters.get_len());
			bool l_b_res = static_cast<bool>(comp.DoOk(command, cmdlen));
			comp.__get_parameters(result);
			_obj_params.set_value(result);
			require(!l_b_res, _obj_params.get(ZSERROR,ZSERRORLEN));
			return;
		}
		catch(const TCHAR* e)
		{
			CString error;
			error.Format(_T("TCHAR exception in call_com_module.%s.%s"), component, command);
			result = (e && e[0])? e : error;
		}
		catch(CString& e)
		{
			CString error;
			error.Format(_T("CString exception in call_com_module.%s.%s"), component, command);
			result = (!e.IsEmpty())? e : error;
		}
		catch(_com_error &er)
		{ 
			CString e = (TCHAR*) er.Description();
			CString error;
			error.Format(_T("_com_error exception in call_com_module.%s.%s"), component, command);
			result = (!e.IsEmpty())? e : error;
		}
		catch(CException *e)
		{
			TCHAR szCause[1024];
			e->GetErrorMessage(szCause,1024);
			e->Delete();
			CString error;
			error.Format(_T("CException exception in call_com_module.%s.%s"), component, command);
			result = (szCause[0])? szCause : error;
		}
		catch(mroerr& e)
		{ 
			CString error;
			error.Format(_T("mroerr exception in call_com_module.%s.%s"), component, command);
			result = (!e.description.IsEmpty())? e.description : error;
		}
		catch(...)
		{
			result.Format(_T("unhandled exception in call_com_module.%s.%s"), component, command);
		}

		comp.ReleaseDispatch();

		ensure(!result.IsEmpty(), result);
	}

	inline CParameters& GetParameters	()												{ return _obj_params;				}
	inline CParameters& operator()	()													{ return _obj_params;				}
	static sDBGblManager* get_db_pointer(TCHAR* pc) {
#ifdef ENVIRONMENT64
		return (sDBGblManager*)_wtoi64(pc);
#endif
#ifdef ENVIRONMENT32
		return (sDBGblManager*)_tstol(pc);
#endif
	}

private:
	CParameters _obj_params;
};


//MACROS FOR COMS ///////////////////////////////////////////////////////
#define MAXFUNCTIONS 160
#define MRO_COM_DECLARATION()\
	afx_msg void  SetParameters(LPCTSTR params, const int len);\
	afx_msg BSTR  GetParameters();\
	afx_msg short DoOk(LPCTSTR fun, const int len);\
	afx_msg void  SetBasics(LPCTSTR basics, const int len);\
	afx_msg BSTR  GetValuesToChange();\
\
	void reset_state();\
	void _get_functions_info();\
\
	CParameters _params;\
	CParameters _basics;\
	CParameters _values;\
	bool newbasics;\
\
	typedef void (MROCOMPONENT::*FP)();\
	struct FPD \
	{ \
		FPD() : function(0), executions(0), errors(0) {};\
		FPD(FP fun, unsigned long execs) : function(fun), executions(execs), errors(0) {}; \
		TCHAR name[64];\
		FP function;\
		unsigned long executions;\
		unsigned long errors;\
	}; \
	FPD* function;\
	static bool firsttime;\
\
	static FPD _map_functions[MAXFUNCTIONS];\
	static int _max_funs;\
	bool component_initialization();\
	void component_set_basics();\
	void component_post();\
	static bool loaded;\
	static bool initialized;\
	void process_error(const TCHAR* text, const TCHAR* extra, const TCHAR* info, const TCHAR* funname, \
						const TCHAR* errfile, const int errline, const TCHAR* errclass);\

#define MRO_DECL_FUNCTIONS_BEGIN()\
bool MROCOMPONENT::loaded = false;\
bool MROCOMPONENT::initialized = false;\
MROCOMPONENT::FPD MROCOMPONENT::_map_functions[MAXFUNCTIONS];\
int MROCOMPONENT::_max_funs = 0;\
int __compare (const void * a, const void * b)\
{\
	return _tcscmp(((MROCOMPONENT::FPD*)a)->name, ((MROCOMPONENT::FPD*)b)->name);\
}\
int __find_fun(const TCHAR* key)\
{\
	MROCOMPONENT::FPD k;\
	_tcscpy_s(k.name, key);\
	MROCOMPONENT::FPD* result = (MROCOMPONENT::FPD*)bsearch(&k, MROCOMPONENT::_map_functions, \
								MROCOMPONENT::_max_funs, sizeof(MROCOMPONENT::FPD), \
								(int (*)(const void*, const void*))__compare);\
	if(result == 0) return -1;\
	int ret = static_cast<int>(&(*result) - &(MROCOMPONENT::_map_functions[0]));\
	return ret;\
}\
bool MROCOMPONENT::firsttime;\
void __helper__(const TCHAR* name, MROCOMPONENT::FP function)\
{\
	if(MROCOMPONENT::_max_funs >= MAXFUNCTIONS) AfxMessageBox(_T("too_many_functions"));\
	_tcscpy_s(MROCOMPONENT::_map_functions[MROCOMPONENT::_max_funs].name, name);\
	MROCOMPONENT::_map_functions[MROCOMPONENT::_max_funs].function = function;\
	++MROCOMPONENT::_max_funs;\
}\
MROCOMPONENT::MROCOMPONENT()\
{\
	EnableAutomation();\
	AfxOleLockApp();\
	if(!firsttime)\
	{\
		firsttime = true;\

#define MRO_COMPONENT_DEFINE_ENTRY(__name, __function);\
		__helper__(__name, &MROCOMPONENT::__function);\

#define MRO_DECL_FUNCTIONS_END()\
		__helper__(_T("reset_state"), &MROCOMPONENT::reset_state);\
		__helper__(_T("get_functions_info"), &MROCOMPONENT::_get_functions_info);\
		qsort((FPD*)&_map_functions, _max_funs, sizeof(FPD), (int (*)(const void*, const void*))__compare);\
	}\
}

#define MRO_COM_DISP_FUNCTIONS()\
	DISP_FUNCTION(MROCOMPONENT, "SetParameters",     SetParameters    , VT_EMPTY, VTS_BSTR VTS_I2)\
	DISP_FUNCTION(MROCOMPONENT, "GetParameters",     GetParameters    , VT_BSTR , VTS_NONE)\
	DISP_FUNCTION(MROCOMPONENT, "DoOk",              DoOk             , VT_I2   , VTS_BSTR VTS_I2)\
	DISP_FUNCTION(MROCOMPONENT, "SetBasics",         SetBasics        , VT_EMPTY, VTS_BSTR VTS_I2)\
	DISP_FUNCTION(MROCOMPONENT, "GetValuesToChange", GetValuesToChange, VT_BSTR , VTS_NONE)\

#define MRO_COM_IMPLEMENTATION_HDR(setbasics)\
void MROCOMPONENT::SetParameters(LPCTSTR params, const int len)\
{\
	try\
	{\
		_params.set_value(params, len);\
		_params.optimize();\
	}\
	catch(const char* e)\
	{\
		CString err = e;\
		if(e == 0) err = _T("unhandled_error");\
		process_error(err, _T(""), _T(""), _T("SetParameters"), _T(""), -1, _T("char*"));\
	}\
	catch(const wchar_t* e)\
	{\
		if(e == 0) process_error(_T("unhandled_error"), _T(""), _T(""), _T("SetParameters"), _T(""), -1, _T("wchar_t*"));\
		else process_error(e, _T(""), _T(""), _T("SetParameters"), _T(""), -1, _T("wchar_t*"));\
	}\
	catch(CString& e)\
	{\
		if(e.IsEmpty())	process_error(_T("unhandled_error"), _T(""), _T(""), _T("SetParameters"), _T(""), -1, _T("CString&"));\
		else process_error(e, _T(""), _T(""), _T("SetParameters"), _T(""), -1, _T("CString&"));\
	}\
	catch(CException* e)\
	{\
		TCHAR szCause[512];\
		e->GetErrorMessage(szCause, 512);\
		e->Delete();\
		process_error(szCause, _T(""), _T(""), _T("SetParameters"), _T(""), -1, _T("CException*"));\
	}\
	catch(mroerr& e)\
	{\
		process_error(e.description, e.extrainfo, _T(""), e.function, e.errfile, e.errline, _T("mroerr&"));\
	}\
	catch(...)\
	{\
		process_error(_T("unhandled_exception"), _T(""), _T(""), _T("SetParameters"), _T(""), -1, _T("..."));\
	}\
}\
BSTR MROCOMPONENT::GetParameters()\
{\
	if(setbasics) component_post();\
	CString r(_params.buffer(),_params.get_len());\
	return r.AllocSysString();\
}\
void MROCOMPONENT::SetBasics(LPCTSTR basics, const int len)\
{\
	try\
	{\
		_basics.set_value(basics, len);\
		_basics.optimize();\
		newbasics = true;\
	}\
	catch(const char* e)\
	{\
		CString err = e;\
		if(e == 0) err = _T("unhandled_error");\
		process_error(err, _T(""), _T(""), _T("SetBasics"), _T(""), -1, _T("char*"));\
	}\
	catch(const wchar_t* e)\
	{\
		if(e == 0) process_error(_T("unhandled_error"), _T(""), _T(""), _T("SetBasics"), _T(""), -1, _T("wchar_t*"));\
		else process_error(e, _T(""), _T(""), _T("SetBasics"), _T(""), -1, _T("wchar_t*"));\
	}\
	catch(CString& e)\
	{\
		if(e.IsEmpty())	process_error(_T("unhandled_error"), _T(""), _T(""), _T("SetBasics"), _T(""), -1, _T("CString&"));\
		else process_error(e, _T(""), _T(""), _T("SetBasics"), _T(""), -1, _T("CString&"));\
	}\
	catch(CException* e)\
	{\
		TCHAR szCause[512];\
		e->GetErrorMessage(szCause, 512);\
		e->Delete();\
		process_error(szCause, _T(""), _T(""), _T("SetBasics"), _T(""), -1, _T("CException*"));\
	}\
	catch(mroerr& e)\
	{\
		process_error(e.description, e.extrainfo, _T(""), e.function, e.errfile, e.errline, _T("mroerr&"));\
	}\
	catch(...)\
	{\
		process_error(_T("unhandled_exception"), _T(""), _T(""), _T("SetBasics"), _T(""), -1, _T("..."));\
	}\
}\
BSTR MROCOMPONENT::GetValuesToChange()\
{\
	CString r(_values.buffer());\
	return r.AllocSysString();\
}

#define MRO_COM_IMPLEMENTATION_FUN(usememhelper,setbasics)\
short MROCOMPONENT::DoOk(LPCTSTR fun, const int funlen)\
{\
	_values.clear();\
	TCHAR error[2048];\
	function=nullptr;\
	try\
	{\
		if(setbasics && newbasics) { component_set_basics(); newbasics=false; }\
		if(!loaded)\
		{\
			if(usememhelper && mro::memhelper::memgbl == 0 && \
				_basics.has(_T("maxprcs"), 7))\
			{\
				int max_procs = _basics.getint(_T("maxprcs"), 7);\
				mro::memhelper::initialize_memhelper(max_procs);\
			}\
			if(dbhelper::dbgbl == 0 && _basics.has(_T("timetol"), 7) && \
				_basics.has(_T("dbm"), 3))\
			{\
				_basics.get(ZADOCON, dbhelper::connection, ZADOCONLEN, 1023);\
				int tolerance = _basics.getint(_T("timetol"), 7, 300);\
				int maxcons = _basics.getint(_T("maxdbcn"), 7, 16);\
				dbhelper::timeout = tolerance;\
				dbhelper::maxconnections = maxcons;\
				TCHAR pc[1024];\
				_basics.get(_T("dbm"), pc, 3, 65);\
				dbhelper::dbgbl = CMroModule::get_db_pointer(pc);\
			}\
			if(!initialized) { initialized = component_initialization(); }\
			if(dbhelper::dbgbl && initialized && !loaded) loaded = true;\
		}\
\
		int funid = __find_fun(fun);\
		if(funid != -1)\
		{\
			function = &_map_functions[funid];\
			++function->executions;\
			(*this.*function->function)();\
			return true;\
		}\
		int len = mikefmt(error, _T("function %s not fund on %s comp"), fun, COMPONENTNAME);\
		_params.set(ZSERROR, error, ZSERRORLEN, len);\
	}\
	catch(const char* e)\
	{\
		CString err = e;\
		if(e == 0) err = _T("unhandled_error");\
		process_error(err, _T(""), _T(""), fun, _T(""), -1, _T("char*"));\
	}\
	catch(const wchar_t* e)\
	{\
		if(e == 0) process_error(_T("unhandled_error"), _T(""), _T(""), fun, _T(""), -1, _T("wchar_t*"));\
		else process_error(e, _T(""), _T(""), fun, _T(""), -1, _T("wchar_t*"));\
	}\
	catch(CString& e)\
	{\
		if(e.IsEmpty())	process_error(_T("unhandled_error"), _T(""), _T(""), fun, _T(""), -1, _T("CString&"));\
		else process_error(e, _T(""), _T(""), fun, _T(""), -1, _T("CString&"));\
	}\
	catch(_com_error &er)\
	{\
		CString e = (TCHAR*) er.Description();\
		if(e.IsEmpty()) process_error(_T("unhandled_error"), _T(""), _T(""), fun, _T(""), -1,_T("_com_error"));\
		else process_error(e, _T(""), e, fun, _T(""), -1,_T("_com_error"));\
	}\
	catch(CException* e)\
	{\
		TCHAR szCause[512];\
		e->GetErrorMessage(szCause, 512);\
		e->Delete();\
		process_error(szCause, _T(""), _T(""), fun, _T(""), -1, _T("CException*"));\
	}\
	catch(mroerr& e)\
	{\
		process_error(e.description, e.extrainfo, _T(""), e.function, e.errfile, e.errline, _T("mroerr&"));\
	}\
	catch(...)\
	{\
		process_error(_T("unhandled_exception"), _T(""), _T(""), fun, _T(""), -1, _T("..."));\
	}\
	if(function) ++function->errors;\
	return false;\
}\
void MROCOMPONENT::process_error(const TCHAR* text, const TCHAR* extra, const TCHAR* info, \
				const TCHAR* funname, const TCHAR* errfile, const int errline, const TCHAR* errclass)\
{\
	try\
	{\
		CString efile(errfile);\
		efile.Replace(_T('"'),_T('\''));\
		efile.Replace(_T('\\'),_T('/'));\
		mroerr err(text, 1,extra, info, COMPONENTNAME, funname, efile, errline, errclass); \
		CString extrainfo;\
		err.to_params(extrainfo);\
		_params.set(ZSERROR	, text		, ZSERRORLEN);\
		_params.set(ZERRORI	, extrainfo	, ZERRORILEN);\
	}\
	catch(CException *e)	{ e->Delete();	}\
	catch(mroerr& e)		{				}\
	catch(...)				{				}\
}\
void MROCOMPONENT::_get_functions_info()\
{\
	CString strfinal;\
	TCHAR row[1024];\
\
	TCHAR list_row[8];\
	list_row[0] = _T('l');\
	list_row[1] = _T('0');\
\
	int irow = 0;\
	for(int i = 0; i < _max_funs; ++i)\
	{\
		FPD* f = &_map_functions[i];\
		mikefmt(&list_row[2], _T("%d"), irow);\
\
		int len = mikefmt(row, _T("[%sA:%s][%sB:%d][%sC:%d][%s*:%d]"), \
							list_row, f->name, \
							list_row, f->executions, \
							list_row, f->errors, \
							list_row, 0);\
		strfinal.Append(row, len);\
		++irow;\
	}\
	int len = gen_tot_list(row,0,irow);\
	strfinal.Append(row, len);\
	_params.append(strfinal.GetBuffer(), strfinal.GetLength());\
}

#define DCS_HELPERS()\
void setlog(const TCHAR* batch, const TCHAR* prod, const TCHAR* line, const TCHAR* part, \
			const TCHAR* tolog, const TCHAR* tylog);\
void set_log(const TCHAR* action, const TCHAR* extra, const TCHAR* key, const TCHAR* type);\
void extract_batch(TCHAR* batch, int& bl, TCHAR* prod, int& pl, TCHAR* line, int& ll, TCHAR* part, int& rl);\
void check_batch_complete(const TCHAR* batch, const int bl, const TCHAR* prod, const int pl, const TCHAR* line, const int ll, const TCHAR* part, const int rl);\
void _is_lean_line();\
void _is_coat_line();\
void _is_castcoat_line();\
void _get_location_basic_data_max_cycle();\
void _get_batch_basic_data();\
void _break_long_batch();\
void _get_batch_basic_data_insp();\

#define EXTRACT_BATCH_FROM_PARAMS()\
TCHAR batch[BATSIZE]; int batchlen=0;\
TCHAR prod[PRDSIZE];  int prodlen=0;\
TCHAR line[LINSIZE]; int linelen=0;\
TCHAR part[PRTSIZE]; int partlen=0;\
extract_batch(batch, batchlen, prod, prodlen, line, linelen, part, partlen);\

#define DCS_HELPERS_IMPLEMENTATION()\
void MROCOMPONENT::setlog(const TCHAR* batch, const TCHAR* prod, const TCHAR* line, const TCHAR* part, \
			const TCHAR* tolog, const TCHAR* tylog)\
{\
	TCHAR key[BATMAX+PRDMAX+LINMAX+PRTMAX+2];\
	set4ch(&key[0],batch[0],batch[1],batch[2],batch[3]);\
	set4ch(&key[4],prod[0],prod[1],prod[2],line[0]);\
	set4ch(&key[8],line[1],part[0],0,0);\
	set_log(tolog, _T(""), key, tylog);\
}\
void MROCOMPONENT::set_log(const TCHAR* action, const TCHAR* extra, const TCHAR* key, const TCHAR* type)\
{\
	int nlogs = 0;\
    if (!_params.isempty() && _params.has(ZZNLOGS, ZZNLOGSLEN)) nlogs = _params.getint(ZZNLOGS, ZZNLOGSLEN);\
	TCHAR log[2048];\
	int lenlog = mro::mikefmt(log, _T("[%s:%d][%s%d:%s][%s%d:%s][%s%d:%s]"), \
								_T("nologs"), nlogs+1, \
								ZTXTLOG, nlogs, action, \
								ZKEYLOG, nlogs, key, \
								ZTYPLOG, nlogs, type);\
	_params.set(ZZTOLOG, log, ZZTOLOGLEN, lenlog);\
}\
void MROCOMPONENT::extract_batch(TCHAR* batch, int& bl, TCHAR* prod, int& pl, TCHAR* line, int& ll, TCHAR* part, int& rl)\
{\
	bl = _params.get(CBATINI, batch	, CBATINILEN, BATMAX);\
	pl = _params.get(CPRDINI, prod	, CPRDINILEN, PRDMAX);\
	ll = _params.get(CLININI, line	, CLININILEN, LINMAX);\
	rl = _params.get(CPRTINI, part	, CPRTINILEN, PRTMAX);\
	check_batch_complete(batch, bl, prod, pl, line, ll, part, rl);\
}\
void MROCOMPONENT::check_batch_complete(const TCHAR* batch, const int bl, const TCHAR* prod, const int pl, const TCHAR* line, const int ll, const TCHAR* part, const int rl)\
{\
	if(_params.isactv(_T("batchchecked"), 12)) return;\
	require(!batch[0], _T("inc_dat_batch"));\
	require(!prod[0],  _T("inc_dat_prod"));\
	require(!line[0],  _T("inc_dat_line"));\
	require(!part[0],  _T("inc_dat_part"));\
	require(bl != BATMAX, _T("wrong_batch"));\
	require(pl != PRDMAX, _T("wrong_prod"));\
	require(ll != LINMAX, _T("wrong_line"));\
	require(rl != PRTMAX, _T("wrong_part"));\
	_params.active(_T("batchchecked"), 12);\
}\
void MROCOMPONENT::_is_lean_line()\
{\
	TCHAR line[LINSIZE]; _params.get(CLININI,line,CLININILEN,LINMAX);\
	require(!line[0], _T("inc_dat_line"));\
	\
	cCommand command(_basics);\
	command.Format(	_T("execute line_is_lean '%s';"), line);\
	getconnectionx(con, obj);\
	con.execute(command, obj);\
	if(obj.getint(0) > 0) \
	{\
		_params.active(_T("$isleanline$"),12);\
		_params.active(_T("isleanline"),10);\
	}\
	else \
	{\
		_params.deactive(_T("$isleanline$"),12);\
		_params.deactive(_T("isleanline"),10);\
	}\
}\
void MROCOMPONENT::_is_coat_line()\
{\
	TCHAR line[8];\
	_params.get(CLININI, line, CLININILEN);\
	cCommand command(_basics);\
	command.Format( _T("select count(*) as res from tlines with (nolock) ")\
					_T("where lineid='%s' and depto='%s'"), \
					line, _T("02"));\
	getconnectionx(con, obj);\
	con.execute(command, obj);\
	if(obj.getint(0) > 0) \
	{\
		_params.active(_T("$iscoatline$"),12);\
		_params.active(_T("iscoatline"),10);\
	}\
	else \
	{\
		_params.deactive(_T("$iscoatline$"),12);\
		_params.deactive(_T("iscoatline"),10);\
	}\
}\
void MROCOMPONENT::_is_castcoat_line()\
{\
	TCHAR line[LINSIZE]; _params.get(CLININI,line,CLININILEN,LINMAX);\
	require(!line[0], _T("inc_dat_line"));\
	\
	cCommand command(_basics);\
	command.Format(	_T("execute line_is_castcoat '%s';"), line);\
	getconnectionx(con, obj);\
	con.execute(command, obj);\
	if(obj.getint(0) > 0) \
	{\
		_params.active(_T("$iscastcoatline$"),16);\
		_params.active(_T("iscastcoatline"),14);\
	}\
	else \
	{\
		_params.deactive(_T("$iscastcoatline$"),16);\
		_params.deactive(_T("iscastcoatline"),14);\
	}\
}\
void MROCOMPONENT::_get_location_basic_data_max_cycle()\
{\
	_params.set(_T("_$stal$"), _T(""), 7);\
	\
	EXTRACT_BATCH_FROM_PARAMS();\
	\
	TCHAR loc[LOCSIZE]; _params.get(CLOCINI, loc, CLOCINILEN, LOCMAX);\
	require(!loc[0], _T("incomplete_data"));\
		\
	cCommand command(_basics);\
	command.Format(	_T("execute get_batch_header_max_cycle '%s','%s','%s','%s','%s';"), batch,prod,line,part,loc);\
	getconnectionx(con, obj);\
	con.execute(command, obj);\
	ensure(obj.IsEOF(), _T("batch_not_exist"));\
	\
	TCHAR s[512];	int ls=0;\
	TCHAR dat[256];	int ld=0;\
	CString str;\
	\
	str=obj.getdatedt(2);	ls=mikefmt(s,_T("fecha creacion: %s"),str);	_params.set(_T("$fcrea$"), s,7,ls);\
	ld=obj.get(0,dat, 255);	ls=mikefmt(s,_T("location: %s"), dat);		_params.set(_T("_$loc_$"), s,7,ls);\
	ld=obj.get(3,dat, 255);	ls=mikefmt(s,_T("status loc: %s"), dat);	_params.set(_T("_$lsta$"), s,7,ls);\
																	  _params.set(_T("_$stal$"),dat, 7,ld);\
	ld=obj.get(5, dat, 255); 										_params.set(_T("$commsb$"), dat, 8,ld);\
	_params.set(_T("$boxes$"), obj.getint(6), 7);\
	ld=obj.get(4, dat, 1);											_params.set(_T("$as400$"), dat, 7, ld);\
	\
	_values.set(_T("$cycle$"), obj.getbyte(1), 7);\
	\
	_get_batch_basic_data();\
}\
void MROCOMPONENT::_break_long_batch()\
{\
	TCHAR lot[FBTSIZE];\
	if(int lotlen = _params.get(_T("cbtlini"), lot, 7, FBTMAX))\
	{\
		require(lotlen != 10, _T("batch_bad_format"));\
		\
		TCHAR batch[BATSIZE];\
		TCHAR prod[PRDSIZE];\
		TCHAR line[LINSIZE];\
		TCHAR part[PRTSIZE];\
		\
		set4ch(batch, lot[0], lot[1], lot[2], lot[3]); batch[4] = 0;\
		set2ch(prod, lot[4], lot[5]); set2ch(&prod[2], lot[6], 0);\
		set2ch(line, lot[7], lot[8]); line[2] = 0;\
		set2ch(part, lot[9], 0);\
		\
		_values.set(_T("$batch$"), batch, 7, BATMAX);\
		_values.set(_T("$_prod$"), prod , 7, PRDMAX);\
		_values.set(_T("$_line$"), line , 7, LINMAX);\
		_values.set(_T("$_part$"), part , 7, PRTMAX);\
	}\
}\
void MROCOMPONENT::_get_batch_basic_data_insp()\
{\
	_get_batch_basic_data();\
	\
	EXTRACT_BATCH_FROM_PARAMS();\
	\
	int inspeccion = 0;\
	cCommand command(_basics);\
	command.Format(	_T("select count(*) as res from tqchdr2 with (nolock) ")\
					_T("where batch='%s' and prod='%s' and line='%s' and part='%s'"), \
					batch, prod, line, part);\
	getconnectionx(con, obj);\
	con.execute(command, obj);\
	if(obj.getint(0) >= 1)\
	{\
		command.Format(	_T("select max(noinsp) as res from tqchdr2 with (nolock) ")\
						_T("where batch='%s' and prod='%s' and line='%s' and part='%s'"), \
						batch, prod, line, part);\
		con.execute(command, obj);\
		inspeccion = obj.getint(0);\
	}\
	_params.set(_T("$_insp$"), inspeccion, 7);\
}\
void MROCOMPONENT::_get_batch_basic_data()\
{\
	EXTRACT_BATCH_FROM_PARAMS();\
	\
	cCommand command(_basics);\
	command.Format(	_T("execute get_batch '%s','%s','%s','%s';"), batch, prod, line, part);\
	getconnectionx(con, obj);\
	con.execute(command, obj);\
	ensure(obj.IsEOF(), _T("batch_not_exist"));\
	\
	int status				= obj.getint(4);\
	COleDateTime casting	= obj.getdate(0);\
	COleDateTime coating	= obj.getdate(1);\
	COleDateTime qc			= obj.getdate(2);\
	COleDateTime packaging	= obj.getdate(3);\
		\
	TCHAR s[128]; int sl=0;\
	sl=mikefmt(s, _T("fecha casting: %s"),	obj.getdatedt(0));	_params.set(_T("_$cast$"), s, 7, sl);\
	sl=mikefmt(s, _T("fecha coating: %s"),	obj.getdatedt(1));	_params.set(_T("_$coat$"), s, 7, sl);\
	sl=mikefmt(s, _T("fecha qc: %s"),		obj.getdatedt(2));	_params.set(_T("_$qc__$"), s, 7, sl);\
	sl=mikefmt(s, _T("fecha pakaging: %s"), obj.getdatedt(3));	_params.set(_T("_$pack$"), s, 7, sl);\
	sl=mikefmt(s, _T("fecha warehouse: %s"),_T("pendiente"));	_params.set(_T("_$wrh_$"), s, 7, sl);\
	\
	TCHAR hlp[128];\
	obj.get(5, hlp, 127);\
	sl=mikefmt(s, _T("comentarios: %s"),	hlp);				_params.set(_T("$comms$"), s, 7, sl);\
	sl=mikefmt(s, _T("%d"),				status);			_params.set(_T("$statushdr$"), s,11, sl);\
	\
	TCHAR lang[3]; _basics.get(ZLANGUA, lang, ZLANGUALEN, 2);\
	command.Format(	_T("execute get_status_desc %d,'%s';"), status, lang);\
	con.execute(command, obj);\
	if(obj.IsEOF()) { set2ch(hlp,'?',0); }\
	else obj.get(0,hlp,127);\
	sl=mikefmt(s, _T("status : |%d| %s"),	status, hlp);		_params.set(_T("_$sta_$"), s, 7, sl);\
	\
	set4ch(hlp, 'N','A',0,0);\
	if(status != 11)\
		mikefmt(hlp, _T("%d"), COleDateTime::GetCurrentTime().GetDayOfYear() - casting.GetDayOfYear());\
	sl=mikefmt(s, _T("wip days(s): %s"),	hlp);				_params.set(_T("$wipdy$"), s, 7, sl);\
}