#include "stdafx.h"

/************************************************************************************
* description   : mro server
* purpose       : execute any transacitions from clients
* author        : miguel rodriguez
* date creation : 20 junio 2004
* change        : 1 marzo call coms direct from the function execute instead of CMroCom::call_com)
* change        : 10 febrero  change to global queue array, and the access controled by semaphores
**************************************************************************************/

#include "mrosvr.h"

void get_date_time(CParameters& result) {
	TCHAR buf_time[32];
	TCHAR buf_date[32];

	_tstrtime(buf_time);
	_tstrdate(buf_date);

	TCHAR res[32+32+2];
	int len = mikefmt(res, _T("%s %s"), buf_date, buf_time);
	result.set(ZDATTIM, res, ZDATTIMLEN, len);
}

int find_pending_proc() {
	for(int i = 0; i < MAX_PROCS; ++i) {
		sProcess& proc  = procs[i];
		if(proc.status == EXECUTING) {
			sClients& clie = clies[proc.clieid];
			ULONGLONG now      = GetTickCount64();
			ULONGLONG duracion = (now - clie.reqst) / CLOCKS_PER_SEC;
			
			long tolerance = /*clie.redispatch ? gchildtol :*/ gtolerance;
			if(duracion > tolerance) return i;
		}
	}
	return -1;
}

int get_slice_to_kill(const int procid, const TCHAR* a2kill, const int actlen) {
	sClients& clie = clies[procs[procid].clieid];
	int zsesins = clie.basics.getint(ZSESINS, -1);
	int zsesmac = clie.basics.getint(ZSESMAC, -1);
	int zsescli = clie.basics.getint(ZSESCLI, -1);
	int zsesses = clie.basics.getint(ZSESSES, -1);

	int result = -1;
	TCHAR ses[64];
	TCHAR act[16];
	for(int i = 0; i < MAX_PROCS; ++i) {
		if(procs[i].status != EXECUTING) continue;
		CParameters& bas = clies[procs[i].clieid].basics;
		int ins = bas.getint(ZSESINS, -1);
		int mac = bas.getint(ZSESMAC, -1);
		int cli = bas.getint(ZSESCLI, -1);
		int ses = bas.getint(ZSESSES, -1);
		bas.get(ZACTINO, act, ZACTINOLEN, 15);

		if(	/*_tmemcmp(session, ses, slen) == 0 &&*/
			zsesins == ins && zsesmac ==mac && zsescli == cli && zsesses == ses &&
			_tmemcmp(a2kill, act, actlen) == 0) return i;
	}
	return result;
}

/* 
	this function execute only internal functions, that is function for the internal 
	control of this	program, and for obtaining information about its current state
*/
bool execute_sys(	const TCHAR* component	, const int complen, 
					CParameters& params, 
					const TCHAR* function	, const int funlen,
					CParameters& result, 
					const int procid, 
					CString* valuestochange) {
	// at this moment this methods does not change values, we clean them
	valuestochange->Empty(); 

	// we dont use the parameters var directly because, we have to converted 
	// again in a parameters object, instead for performance reason we use the 
	// original parameters, other types of execution dont have to do this, because 
	// they are external ones, thats why a raw TCHAR* is the fast way, but here 
	// that idea is useless because we are on the same component(the server)
	result.clear();

	if(funlen == 13 && _tmemcmp(_T("get_date_time"), function, 13) == 0) {
		get_date_time(result);
		return true;
	}
	if(funlen == 12 && _tmemcmp(_T("get_identity"), function, 12) == 0)	{
		sClients& clie = clies[procs[procid].clieid];
		TCHAR mach[64];
		int machlen;
		get_ip_and_name(clie.socket, mach, machlen);
		result.set(ZMACNAM, mach, ZMACNAMLEN, machlen);
		// ******************************************************************************
		// every time the valuestochange is modify we need to change workonvalues to true, 
		// I know I havent done this but it must be done cuase it can cause subtles bugs
		result.get_pair(ZMACNAM, *valuestochange, ZMACNAMLEN);
		sProcess& proc = procs[procid];
		proc.workonvalues = true;
		return true;
	}
	if(funlen == 8 && _tmemcmp(_T("giveback"), function, 8) == 0) {
		return params.get(_T("giveback"), result, 8);
	}
	if(funlen == 19 && _tmemcmp(_T("get_perfomance_vars"), function, 19) == 0) {
		TCHAR s[128];
		int l = 0;
		l += cpairs::gen_pair(0, &s[l],_T("uperaccesses")	, gaccessno		, 12);
		l += cpairs::gen_pair(0, &s[l],_T("uperexecfuns")	, gexecutedfuns	, 12);
		l += cpairs::gen_pair(0, &s[l],_T("uperwaitings")	, gwaitinghits	, 12);
		l += cpairs::gen_pair(0, &s[l],_T("uperrejecteds")	, grejectedhits	, 13);
		l += cpairs::gen_pair(0, &s[l],_T("upermallocs")	, 0L			, 11);
		l += cpairs::gen_pair(0, &s[l],_T("upermallocssql")	, 0L			, 14);
		l += cpairs::gen_pair(0, &s[l],_T("uperfrees")		, gfreesdone	, 9);
		l += cpairs::gen_pair(0, &s[l],_T("uperfresssql")	, 0L			, 12);
		result.set_value(s, l);
		return true;
	}
	if(funlen == 12 && _tmemcmp(_T("get_act_prcs"), function, 12) == 0) {
		int cachelen = sizeof(TCHAR)*((MAX_PROCS * 128) + 1024);
		TCHAR* pmem = get_memory(procs[procid], cachelen, 0);
		int len = get_list_prcs(pmem, 0, cachelen - 1024);
		result.set_value(pmem, len);
		return true;
	}
	if(funlen == 12 && _tmemcmp(_T("get_act_reqs"), function, 12) == 0) {
		int cachelen = sizeof(TCHAR)*((MAX_CLIES * 256) + 1024);
		TCHAR* pmem = get_memory(procs[procid], cachelen, 0);
		int len = get_list_reqs(pmem, 1, cachelen - 1024);
		result.set_value(pmem, len);
		return true;
	}
	if(funlen == 12 && _tmemcmp(_T("get_svr_logs"), function, 12) == 0) {
		int cachelen = sizeof(TCHAR)*((MAXLOGQUEUE * MAX_PROCS * 128) + 1024);
		TCHAR* pmem = get_memory(procs[procid], cachelen, 0);
		int len = get_list_log(actionlog, pmem, 2, cachelen - 1024);
		result.set_value(pmem, len);
		return true;
	}
	if(funlen == 12 && _tmemcmp(_T("get_svr_errs"), function, 12) == 0) {
//		int cachelen = sizeof(TCHAR)*((MAXLOGQUEUE * MAX_PROCS * 128) + 1024);
//		TCHAR* pmem = get_memory(procs[procid], cachelen, 0);
//		int len = get_list_log(errorlog, pmem, 3, cachelen - 1024);
//		result.set_value(pmem, len);
		return true;
	}
	if(funlen == 13 && _tmemcmp(_T("get_svr_stats"), function, 13) == 0) {
		int cachelen = sizeof(TCHAR)*(((STATISTICS_SIZE) * 64) + 1024);
		TCHAR* pmem = get_memory(procs[procid], cachelen, 0);
		int len = _get_statistics(pmem, cachelen - 1024);
		result.set_value(pmem, len);
		return true;
	}
	if(funlen == 17 && _tmemcmp(_T("reload_codebehind"), function, 17 == 0)) {
		TCHAR cbfile[64];
		int cbfilelen = params.get(_T("cbfile"), cbfile, 6, 63);
		reload_codebehind(cbfile, cbfilelen, procs[procid]);
		return true;
	}
	if(funlen == 9 && _tmemcmp(_T("flush_log"), function, 9) == 0) {
		for(int i = 0; i < MAX_PROCS; i++) flush_logs(actionlog, i);
		return true;
	}
	if(funlen == 12 && _tmemcmp(_T("flush_errors"), function, 12) == 0)	{
//		for(int i = 0; i < MAX_PROCS; i++) flush_logs(errorlog, i);
		return true;
	}
	if(funlen == 10 && _tmemcmp(_T("flush_logs"), function, 10) == 0)	{
		for(int i = 0; i < MAX_PROCS; i++)
		{
			sProcess& proc = procs[i];
			if(procid == i || proc.status != EXECUTING)
			{
				flush_logs(actionlog, i);
//				flush_logs(errorlog, i);
			}
		}
		return true;
	}
	if(funlen == 13 && _tmemcmp(_T("reset_service"), function, 13) == 0) {
		goodbye(_T("reseted_by_admin"),16);
		return true;
	}
	if(funlen == 9 && _tmemcmp(_T("kill_proc"), function, 9) == 0) {
		int proc2kill = params.getint(_T("lst0B0"), 6);
		if(proc2kill == -1) {
			TCHAR a2kill[64]; 
			if(int a2klen = params.get(ZA2KILL, a2kill, ZA2KILLLEN, 63))
				proc2kill = get_slice_to_kill(procid, a2kill, a2klen);
		}
		require(proc2kill == -1						,	_T("cant_find_process"));
		require(proc2kill == procid					,	_T("cant_kill_yourself"));
		require(procs[proc2kill].status != EXECUTING,	_T("only_trans_in_process"));
		require(!kill_process(proc2kill)			,	_T("transaction_not_killed"));
		return true;
	}
	if(funlen == 17 && _tmemcmp(_T("kill_pending_proc"), function, 17) == 0) {
		int proc2kill = find_pending_proc();
		if( proc2kill != -1) {
			require(proc2kill == procid		, _T("cant_kill_yourself"));
			require(!kill_process(proc2kill), _T("transaction_not_killed"));
		}
		return true;
	}
	if(funlen == 4 && _tmemcmp(_T("ping"), function, 4) == 0) {
		TCHAR data[2048];
		int ldata = params.get(_T("guivars"), data, 7, 2047);
		cMroList guivars(data, ldata);
		defchar(variable,64);
		int lenvar = 0;
		for(guivars.begin(); guivars.end() == false; guivars.next()) {
			lenvar = guivars.get(variable);	break;
		}
		if(lenvar) result.set(&variable[1], _T("ok"), lenvar-1, 2);
		return true;
	}

	require(true, _T("function_not_found"));
}