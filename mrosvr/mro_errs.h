#pragma once

namespace mro {
struct mroerr
{
	// fun and fileerr are char* because they are capture from __FUNCTION__ 
	// and __FILE__ wich are macro functions and those return char* 
#ifdef UNICODE
	mroerr(	const TCHAR* error, 
			const int codeid, 
			const TCHAR* extra, 
			const TCHAR* info,
			const TCHAR* comp,
			const TCHAR* calls,
			const char* fun, 
			const char* fileerr, 
			const int lineerr, 
			const TCHAR* errclass) : 
			description(error), code(codeid), extrainfo(extra), errinfo(info),
			component(comp), callstack(calls), function(fun), errfile(fileerr), errline(lineerr) , errclass(errclass), step(-1)
			{ extrainfo.Trim(); };
#endif
	mroerr(	const TCHAR* error, 
			const int codeid, 
			const TCHAR* extra, 
			const TCHAR* info,
			const TCHAR* comp, 
			const TCHAR* calls,
			const TCHAR* fun, 
			const TCHAR* fileerr, 
			const int lineerr, 
			const TCHAR* errclass) : 
			description(error), code(codeid), extrainfo(extra), errinfo(info),
			component(comp), callstack(calls), function(fun), errfile(fileerr), errline(lineerr) , errclass(errclass), step(-1)
			{ extrainfo.Trim(); };
	CString description;
	int code;
	CString extrainfo;
	CString errinfo;
	CString component;
	CString callstack;
	CString function;
	CString errfile;
	CString errclass;
	int errline;
	int step;
	void to_params(CString& errinf);
};
}
#define require(pred, error)		{ if(pred) throw mroerr(error, -1, _T(""),  _T(""),_T(""),_T(""),__FUNCTION__, __FILE__, __LINE__,_T("")); }
#define requireex(pred, error,extra){ if(pred) throw mroerr(error, -1, extra ,  _T(""),_T(""),_T(""),__FUNCTION__, __FILE__, __LINE__,_T("")); }
#define ensure(pred, error)			{ if(pred) throw mroerr(error, -1, _T(""),  _T(""),_T(""),_T(""),__FUNCTION__, __FILE__, __LINE__,_T("")); }
#define ensureex(pred, error,extra)	{ if(pred) throw mroerr(error, -1, extra ,  _T(""),_T(""),_T(""),__FUNCTION__, __FILE__, __LINE__,_T("")); }

#define secure()\
	catch(CException* e)	{ e->Delete();	}\
	catch(mroerr& e)		{				}\
	catch(...)				{				}

#define rescue()\
		catch(const char* e)\
		{\
			CString err;\
			err = e;\
			status(err, ev_error); \
		}\
		catch(const wchar_t* e)\
		{\
			CString err = e;\
			status(err, ev_error); \
		}\
		catch(CString& e) \
		{ \
			status(e, ev_error); \
		}\
		catch(CException* e)\
		{\
			TCHAR   err[1024];\
			e->GetErrorMessage(err, 1024);\
			e->Delete();\
			status(err, ev_error);\
		}\
		catch(mroerr& e) \
		{ \
			CString errinf;\
			e.to_params(errinf);\
			status(e.description, ev_error, e.description.GetLength(), false, &errinf); \
		}\
		catch(...) \
		{ \
			status(_T("error"), ev_error, 5);\
		}

