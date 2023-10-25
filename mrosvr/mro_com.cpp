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
#include "SessionMan.h"
#include "proc.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

struct smrocom {
	smrocom() {} 
	CSessionMan ses;
};

static smrocom* stock;

void initialize_com() {
	stock =  new smrocom[MAX_PROCS];
	for(int i=0; i<MAX_PROCS; ++i)	{
	}
}

bool core_call(	CParameters& params, 
								const TCHAR* command, const int cmdlen, 
								CParameters& result, 
								sProcess& proc) {
	proc.sendbasics = true;
	bool res = execute_com(params, command, cmdlen, result, proc.id, 0);
	result.optimize();
	return res;
}
bool core_call(	const TCHAR* params, const int paramslen, 
								const TCHAR* command, const int cmdlen, 
								CParameters& result, 
								sProcess& proc) {
	proc.tmpres.set_value(params, paramslen);
	return core_call(proc.tmpres, command, cmdlen, result, proc);
}

bool execute_com(	CParameters& params,
									const TCHAR* command, const int cmdlen,
									CParameters& result,
									const int procid,
									CString* valuestochange) {
	sProcess& proc	= procs[procid];
	sClients& clie	= clies[proc.clieid];
	smrocom& pcom	= stock[procid];

	CSessionMan& ses = pcom.ses;

	// the first time of the request execution, we send the basics
	// and mark of all posibles components as the firsttime to execute
	if(proc.sendbasics)
	{
		proc.sendbasics = false;
		ses.SetBasics(clie.basics.buffer(), clie.basics.get_len());	// our basics params
	}
	ses.SetParameters(params.buffer(), params.get_len());			// our working params

	bool bres = ses.DoOk(command, cmdlen);							// the real execution

	result.unoptimize();
	clie.tempstr = ses.GetParameters();								// our working params changed
	result.set_value(clie.tempstr);
	result.optimize();

	if(valuestochange)
		*valuestochange = ses.GetValuesToChange();					// the variables to change

	return bres;
}

void terminate_com(const int procid) {
}
