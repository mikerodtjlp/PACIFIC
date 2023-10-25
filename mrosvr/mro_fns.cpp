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
#include "mro_fns.h"

struct fundata
{
	fundata(CString& funs) : functions(funs), reload(false) {}
	CParameters functions;
	bool reload;
};

typedef std::map<CString, fundata> funmap;

static funmap cachefunctions;
static funmap::iterator endcachefuns;
static CRITICAL_SECTION csfuns;

void initialize_funs()
{
	endcachefuns = cachefunctions.end();
	::InitializeCriticalSection(&csfuns);
}

bool reload_codebehind(const TCHAR* cbfile, const int cbfilelen, sProcess& proc)
{
	// we must invalidate our last function for tricky bugs
	proc.lastcbstr.clear();
	proc.lastcbfun[proc.lastcbfunlen=0]=0;
	try
	{
		::EnterCriticalSection(&csfuns);
		funmap::iterator end = cachefunctions.end();
		for(funmap::iterator iterfun = cachefunctions.begin(); iterfun != end; ++iterfun)
		{
			CString& key = const_cast<CString&>((*iterfun).first);
			if(key.GetLength() <= cbfilelen) continue; // to short to be a candidate
			if(_tmemcmp(key.GetBuffer(), cbfile, cbfilelen) == 0)
				(*iterfun).second.reload = true;
		}
		::LeaveCriticalSection(&csfuns);
	}
	catch(CException *e)	{ ::LeaveCriticalSection(&csfuns);	throw; }
	catch(mroerr& e)		{ ::LeaveCriticalSection(&csfuns);	throw; }
	catch(...)				{ ::LeaveCriticalSection(&csfuns);	throw; }

	return true;
}

bool load_function(sClients& clie, sProcess& proc)
{
	auto* ct = clie.trans;
	auto ctl = clie.trnlen;
	auto* ev = proc.eventname;
	auto evl = proc.eventnamelen;
	bool reload = evl == 8 && cmp4ch(ev, 'o','n','r','e') && cmp4ch(ev+4, 'l','o','a','d') && 
									ev[8] == 0;
	if(reload) return reload_codebehind(ct, ctl, proc);

	require(ctl	== 0, PAGE_MISSING);
	require(evl	== 0, FUN_MISSING);

	// form the key to find, example tsmpassoneter (kind of full namespace name)
	TCHAR functofind[128];
	_tmemcpy(functofind, ct, ctl);
	_tmemcpy(&functofind[ctl], ev, evl+1); 
	int functofindlen = ctl + evl;
	require(functofindlen > 63, _T("function_name_2_long"));

	auto& pl = proc.lastcbstr;
	auto* pf = proc.lastcbfun;
    // if for some reason our function data is clean we try to get it again
	if(pl.isempty()) pf[proc.lastcbfunlen = 0] = 0;

	// first of all we lookfor it on the last function(happens quite often)
	if(	(proc.lastcbfunlen != functofindlen) || (_tmemcmp(pf, functofind, functofindlen) != 0))
	{
		// obviously we invalidate our last function
		pl.clear();
		pf[proc.lastcbfunlen=0]=0;

		CString func2find(functofind, functofindlen);
		bool lookindict = false;
		bool byforce	= false;

		// second of all we lookfor it on our local cache
		try
		{
			::EnterCriticalSection(&csfuns);
			funmap::iterator iterfun = cachefunctions.lower_bound(func2find);
			if(iterfun != endcachefuns && !(cachefunctions.key_comp()(func2find, iterfun->first)))
			{
				if((*iterfun).second.functions.isempty()) lookindict = true;
				else
				{
					if(byforce = (*iterfun).second.reload) lookindict = true;
					else pl = (*iterfun).second.functions; 
				}
				if(lookindict) cachefunctions.erase(iterfun);
			}
			else lookindict = true;

			if(lookindict) // and for the last we lookfor it on the global dictionary
			{
				auto& result = proc.tmpres;
				TCHAR ws[1024+1024]; // 1024 for the basics and 1024 for the rest

				//if(isgate) {
					int l = mikefmt(ws,_T("[fun2find:%s][%s:%s][typread:%s]"), 
									ev, PDOCMNT, ct, 
									byforce ? _T("force") : _T(""));
					core_call(ws, l, _T("get_final_fun"), 13, result, proc);
				//}
				//else {
				//	int l = mikefmt(ws, _T("[%s:%s][%s:1][zfun00z:[%s:%s][%s:%s][%s:%s]")
				//						_T("[fun2find:%s][%s:%s][typread:%s]]"), 
				//					ZBASICS, clie.basics.buffer(),
				//					ZZNFUNS, ZTYPCOM, _T("com"), ZCOMPNM, ZCTROBJ, 
				//					ZFUNNAM, _T("get_final_fun"), ev, 
				//					PDOCMNT, ct, 
				//					byforce ? _T("force") : _T(""));
				//	proc.gatesock.execute(clie.pgatsvr, clie.gatprt, ws, l, result);
				//	result.optimize();
				//}

				require(result.is_bad_formed(), _T("fun_wrong_formed"));

				if(result.has(ZSERROR, ZSERRORLEN))
				{
					int errlen = result.get(ZSERROR, proc.hstr256, ZSERRORLEN, 255);
					require(errlen, proc.hstr256);
				}

				// now we have the function code
				result.get(ZFINFUN, pl, ZFINFUNLEN);
				require(pl.isempty(), _T("could_not_load_function"));
				require(pl.is_bad_formed(), _T("fun_wrong_formed"));

				// when we have the real function we store in the cache for latter use
				pair<funmap::iterator, bool> res = cachefunctions.insert(funmap::value_type(func2find, fundata(pl.copy())));
				if(res.second == true) endcachefuns = cachefunctions.end();
				else pl.clear();
			}
			::LeaveCriticalSection(&csfuns);

			// now we have our last function
			_tmemcpy(pf, functofind, (proc.lastcbfunlen=functofindlen)+1);
		}
		catch(CException *e)	{ ::LeaveCriticalSection(&csfuns);	throw; }
		catch(mroerr& e)		{ ::LeaveCriticalSection(&csfuns);	throw; }
		catch(...)				{ ::LeaveCriticalSection(&csfuns);	throw; }
	}

	return true;
}

