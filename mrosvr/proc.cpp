#include "stdafx.h"

/************************************************************************************
* description   : mro server
* purpose       : functions that deals with the procs(cpus)
* author        : miguel rodriguez
* date creation : 20 junio 2004
* change        : 1 marzo
*                 call coms direct from the function execute instead of CMroCom::call_com)
* change        : 10 febrero 
*                 change to global queue array, and the access controled by semaphores
**************************************************************************************/

#include "mrosvr.h"

TCHAR* statdescs[]	=	{	
							_T("free"), _T("acpt"), _T("takn"), _T("rdng"),	
							_T("wait"),	_T("schd"),	_T("exec"), _T("resp"), _T("done"),
							_T("kllg"), _T("klld"), _T("fail") 
						};

stats statistics[STATISTICS_SIZE];
int statstop = 0;
sProcess* procs; // we have an executed queue wich by performamce we need it to execute only _max_queue__
HANDLE* cpus;
UINT MAX_PROCS;

void initialize_processes() {
	procs = new sProcess[MAX_PROCS];
	cpus = new HANDLE[MAX_PROCS];
	ajst_prc = new bool[MAX_PROCS];

	UINT thread_id;
	HANDLE thread_h;
	for(int i = 0; i < MAX_PROCS; ++i) {
		cpus[i]	= ::CreateEvent(NULL, TRUE, FALSE, NULL);

		sProcess& proc			= procs[i];
		proc.status					= FREE;
		proc.startexecuting	= ::CreateEvent(NULL, TRUE, FALSE, NULL);
		proc.accesses				= 0;
		proc.gcpoolid				= 0;
		proc.selfkill				= false;

		proc.memlen[0]			= 0;
		proc.memlen[1]			= 0;
		proc.memory[0]			= nullptr;
		proc.memory[1]			= nullptr;

		ajst_prc[i]				= true;

		thread_h = (HANDLE ) _beginthreadex(0, 0, apply_functions, reinterpret_cast<void*>(i), 0 , &thread_id);
		proc.thread = thread_h;
	}

	for(int i=0; i<MAXFUNSPROC; ++i)
		mikefmt(funnum[i], _T("zfun%02dz"), i); 
}

typedef void (*terminate_funs)(const int procid);

inline terminate_funs find_fun2ter(const TCHAR* key) {
	if(key[0] == '\0')  return  &terminate_com; // com by default

	if(cmp4ch(key, 'c','o','m','\0')) return &terminate_com;
	if(cmp4ch(key, 's','q','l','\0')) return &terminate_sql;

	return  &terminate_com; // com by default
}

TCHAR* get_memory(sProcess& proc, const int len, const int slot) {
	if(len > proc.memlen[slot])	{
		int extraroom = len < 4096 ? 2048 : ((len*50)/100);
		proc.memlen[slot] = len + extraroom;						// add some extra room
		if(proc.memory[slot]) { 
			if(proc.gcpoolid >= MAXGCPOOL) gccollect(proc, 1);
			proc.gcpool[proc.gcpoolid] = proc.memory[slot]; 
			++proc.gcpoolid; 
		}	
		proc.memory[slot] = (TCHAR*)malloc(sizeof(TCHAR)*(proc.memlen[slot] + 1));	// create the new chunck
	}
	return proc.memory[slot];
}

bool gccollect(sProcess& proc, const int howmany) {
	if(proc.gcpoolid==0) return false;
	// we clean all the garbage that was left
	int chunks =	howmany == 0 ? proc.gcpoolid : 
								howmany > proc.gcpoolid ?	proc.gcpoolid : howmany;
	for(int i = 0; i < chunks; ++i, ++gfreesdone, --proc.gcpoolid) 
		free(proc.gcpool[proc.gcpoolid-1]);
	return true;
}

void init_proc(sProcess& proc) {
	++proc.accesses;												// another process executed for this cpu
	proc.hasretprms		= false;
	proc.workonvalues	= false;
//proc.saveresult		= 0;
	proc.isbyevent		= false;
	proc.currfun			= -1;
	proc.nlogs				= 0;										// star with no logs
	proc.eventnamelen	= 0;
	proc.eventname[0]	= 0;
}

// this function checks is some process is already be stucked, the criteria
// depends on a paramenter called time-tolerance, there is trick here: we check
// half the time-tolerance period, in order to more often find processes stucked
// but the evaluation remains the same: it is only killed if over passes the 
// tolerance time, in order words we check double times but kill when have to

void check_state() {
	// very important note, the gate server does not do anything per se, it most of
	// the time redispatch the request ,so if we need to kill a stucked process we 
	// must wait it to be killed on the destination service, not on this service and
	// if we need anyway to kill it, we have to do it manually, not automatically but
	// there are some internal request that do must be killed, like on the services
	int proc2kill = find_pending_proc();
	if(proc2kill == -1) return;
	//if(clies[procs[proc2kill].clieid].redispatch) return;
	kill_process(proc2kill);
}

/*
	this function kills a running process and create another one in order to replace it
*/
bool kill_process(int proc2kill) {
	// process
	sProcess& proc	= procs[proc2kill];

	// the client
	int clieid		= proc.clieid;
	sClients& clie	= clies[clieid];
	bool wasterminated	= false;

	// **** try to terminate the function by itself ****
//	(*find_fun2ter(clie.typecom))(proc2kill); 
	// the idea is suspended first in order to stop it and prevent it
	// finished by itself meanwhile we try to finished it ourselves

	if(clie.step >= 79)						return false;

	// maybe a little push to out by itself, note experience tells us
	// that only in a small fraction of cases it work, mostly the
	// cpus are bassicaly stucked so they never see the flags
	proc.selfkill = true;

	if(::SuspendThread(proc.thread) == -1)	return false;

	clie.status = KILLING;

	try	{
		if(wasterminated = ::TerminateThread(proc.thread, 0)) {
			proc.selfkill = false;
			clie.restop = 0;
			clie.resstp = 0;
			ULONGLONG cputimestart = GetTickCount64();
			clie.cputime[0] = 0;
			CParameters& result = get_response(clie, true);

			// the risk of killing threads is to leave states in the middle, 
			// we attempt to reset the state and leave it secure 
			int funid = proc.currfun;
			//TCHAR typecomp[ZTYPCOMMAX+1];
			//TCHAR component[ZCOMPNMMAX+1]; 
			int complen = 0;
			//_tmemcpy(component, clie.component[funid], (complen = clie.cmplen[funid]) + 1);
			//_tmemcpy(typecomp, clie.typecom[funid], clie.tcmlen[funid]+1);
			if(funid != -1) {
				execute_com(result, _T("reset_state"), 11, result, proc2kill, &proc.newvalues); 
			}

			//set4ch(clie.typecom[0]  , 'X','X','X',0); clie.tcmlen[0] = 3;
			//set4ch(clie.component[0], 'X','X','X',0); clie.cmplen[0] = 3;
			//set4ch(clie.function[0] , clie.rdsnam[0],clie.rdsnam[1],clie.rdsnam[2],0); clie.funlen[0] = 3;
			clie.nfuns = 1;
			// we are sending to the killee not to the killer
			result.set_value(ZSERROR, _T("function_was_canceled"), ZSERRORLEN, 21);
			clie.cputime[0] = GetTickCount64() - cputimestart;

			get_error_description(clie, proc, result, false);
			process_error(clie, proc, result, true, clie.step);
			clie.step = 80;
			post_process(clie, proc);
			clie.step = 99;
			finish_process(clie, proc, KILLED);
			finish_request(clie);

			// create the new thread that will replace the just killed one
			UINT thread_id;
			proc.thread = (HANDLE)_beginthreadex(0, 0, apply_functions, reinterpret_cast<void*>(proc2kill), 0, &thread_id);

			// the risk of killing threads is to leave states in the middle, 
			// so we attempt again to reset the state and leave it secure 
			if(funid != -1) {
				execute_com(result, _T("reset_state"), 11, result, proc2kill, &proc.newvalues); 
			}

			// maybe the slice was executing a sql query so for robustness we close the connection for latter reuse
			dbhelper::initialize_stack(proc2kill);
			return true;
		}
	}
	catch(CException *e)	{ e->Delete();  }
	catch(mroerr& e)			{ }
	catch(...)						{	}

	clie.status = EXECUTING;

	return false;
}

int get_list_prcs(TCHAR* lista, const int lid, const int buffsize) {
	UINT len	= 0;
	TCHAR* p	= lista;
	int row		= 0;
	int lon		= 0;

	for(register int index = 0; index < MAX_CLIES; ++index)	{
		sClients& clie = clies[index];

		defchar(dateproc, 32);
		if(clie.status != FREE && clie.dateprocess) {
			struct tm* today = _localtime64(&clie.dateprocess);
			_tcsftime(dateproc, 15, _T("%H:%M:%S"), today);
		}

		ULONGLONG now      = GetTickCount64();
		double cputime = clie.status == EXECUTING ?	((double)(now - clie.reqst))/ CLOCKS_PER_SEC :
																((double)		clie.reqst) / CLOCKS_PER_SEC ;
		double rectime = clie.status == EXECUTING ?	((double)(now - clie.recst))/ CLOCKS_PER_SEC :
																((double)		clie.recst) / CLOCKS_PER_SEC ;
		double sndtime = clie.status == EXECUTING ?	((double)(now - clie.sndst))/ CLOCKS_PER_SEC :
																((double)		clie.sndst) / CLOCKS_PER_SEC ;

		if(clie.procid != -1) {
			mrostatus procstatus = procs[clie.procid].status;
			if(clie.status == EXECUTING || procstatus == EXECUTING) {
				lon = mikefmt(p,_T("[l%d%dA:%ld][l%d%dB:%s][l%d%dC:%s][l%d%dD:%s][l%d%dE:%s][l%d%dF:%s]")
								_T("[l%d%dG:%.3f:%.3f:%.3f][l%d%dH:%ld:%ld][l%d%dI:%ld]")
								_T("[l%d%dJ:%s][l%d%dK:%s][l%d%dL:%s][l%d%dM:%d][l%d%d*:%d]"),
								//A
								lid, row, clie.id,
								lid, row, statdescs[procstatus], 
								lid, row, clie.machine,
								lid, row, clie.user,
								lid, row, clie.trans,
								lid, row, dateproc,
								//G
								lid, row, cputime,rectime,sndtime,
								lid, row, clie.packsin,clie.packsout,
								lid, row, clie.nfuns,
								//J
								lid, row, /*clie.restop ? clie.typecom[clie.restop-1] :*/ _T(""),
								lid, row, /*clie.restop ? clie.component[clie.restop-1] :*/ _T(""), 
								lid, row, clie.restop ? clie.function[clie.restop-1] : _T(""),
								lid, row, clie.step,
								lid, row, procstatus);
				++row;
				len += lon;
				p += lon;
				require(len >= buffsize, _T("memory_overrun"));
			}
		}
	}

	int maxconns = 0;
	int connused = 0;
	for(int i=0; i<MAX_PROCS; ++i)
	{
		maxconns += dbhelper::maxconnections;
		connused += dbhelper::dbgbl[i].top;
	}

	ULONGLONG now = GetTickCount64();
	ULONGLONG elapsedtime = ((ULONGLONG)(now - gstarttime)) / CLOCKS_PER_SEC;

	TCHAR k[1024];
	int kl = mikefmt(k,	_T(", runtime:%.3f scds, ")
						_T("perf:%.2f scnd, execfuns:%ld, ")
						_T("funperf:%.2f, waithits:%ld, rejshits:%ld, frees:%ld, conns:%d:%d, exectype:%ld"),
					elapsedtime, ((double)gaccessno/ elapsedtime), gexecutedfuns, 
					((double)gexecutedfuns / elapsedtime), gwaitinghits, grejectedhits, gfreesdone,
					connused, maxconns, gexectyp);

	len += gen_tot_list(p,lid,row,k,kl);
	require(len >= buffsize, _T("memory_overrun"));
	return len;
}

void get_executing_functions(TCHAR* result) {
	TCHAR* p = result;
	try	{
		for(int i = 0; i < MAX_PROCS; ++i) {
			sProcess& proc = procs[i];
			if(proc.status == EXECUTING) {
				sClients& clie = clies[proc.clieid];
				p += mikefmt(p,_T(" {%d:%d-%s}"), i, clie.step, _T("pending"));//clie.function);
			}
		}
	}
	catch(CException *e)	{ e->Delete();	}
	catch(...)						{ }
}

int _get_statistics(TCHAR* lista, const int buffsize) {
	UINT len	= 0;
	TCHAR* p	= lista;
	int row		= 0;
	int lon		= 0;

	stats* s = statistics;
	for(int i = 0; i < STATISTICS_SIZE; ++i, s++) {
		defchar(dateproc, 32);
		struct tm* today = _localtime64(&s->when);
		_tcsftime(dateproc, 15, _T("%H:%M:%S"), today);

		lon = mikefmt(p,_T("[l0%dA:%ld][l0%dB:%ld][l0%dC:%ld][l0%dD:%s][l0%d*:%d]"), 
						row, s->clients_accepted, 
						row, s->process_runned,
						row, s->functions_executed,
						row, dateproc,
						row, 0);
		++row;
		len += lon;
		p +=lon;
		require(len >= buffsize, _T("memory_overrun"));
	}

	len += gen_tot_list(p,0,row);
	require(len >= buffsize, _T("memory_overrun"));
	return len;
}

