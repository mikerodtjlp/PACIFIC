#include "stdafx.h"

/************************************************************************************
* description   : scheduler
* purpose       : receive the clients and put them in certain order to be ready for execution
* author        : miguel rodriguez ojeda
* date creation : 20 junio 2004
* change        : 1 marzo 2005  - call coms direct from the function execute instead of CMroCom::call_com)
* change        : 10 febrero 2005 - change to global queue array, and the access controled by semaphores
* change        : 30 octubre 2005 -  create limited number of request to be processed
**************************************************************************************/

#include "mrosvr.h"

sClients* clies;
UINT MAX_CLIES;
UINT DEF_CLXPR;
UINT MAX_SBSIZ;
UINT DEF_SBSIZ;

void initialize_requests() {

	clies = new sClients[MAX_CLIES];

	if(MAX_CLIES <= 64) {
		incomings = new HANDLE[MAX_CLIES];
		requests = new HANDLE[MAX_CLIES];
		responses = new HANDLE[MAX_CLIES];
	}
	else {
		incomings = new HANDLE[64];
		incomings2 = new HANDLE[MAX_CLIES-64];
		requests = new HANDLE[64];
		requests2 = new HANDLE[MAX_CLIES-64];
		responses = new HANDLE[64];
		responses2 = new HANDLE[MAX_CLIES-64];
	}

	for(UINT i = 0; i < MAX_CLIES; ++i) {
		if(i<64) {
			incomings[i]	= ::CreateEvent(NULL, TRUE, FALSE, NULL);
			requests[i]		= ::CreateEvent(NULL, TRUE, FALSE, NULL);
			responses[i]	= ::CreateEvent(NULL, TRUE, FALSE, NULL);
		}
		else {
			incomings2[i-64]= ::CreateEvent(NULL, TRUE, FALSE, NULL);
			requests2[i-64]	= ::CreateEvent(NULL, TRUE, FALSE, NULL);
			responses2[i-64]= ::CreateEvent(NULL, TRUE, FALSE, NULL);
		}

		sClients& clie	= clies[i];
		clie.id					= i;
		clie.socket			= INVALID_SOCKET;
		clie.status			= FREE;
		clie.inlen			= 0;
		clie.packsin		= 0;
		clie.packsout		= 0;
		clie.outlen			= 0;
		clie.reqreaded	= false;
		clie.endmark		= false;
		clie.retresult	= false;
		clie.priority		= 5;
		clie.procreq		= ::CreateEvent(NULL, TRUE, FALSE, NULL);

		clie.dateprocess= 0;
		clie.reqst			= 0;
		clie.recst			= 0;
		clie.sndst			= 0;

		clie.request.clear();

		clie.nfuns			= 0;
		clie.restop			= 0;
		clie.resstp			= 0;

		clie.machine[0]	= clie.maclen = 0;
		clie.trans[0]		= clie.trnlen = 0;
		clie.user[0]		= clie.usrlen = 0;

#ifdef MRO_IOCP_IMP 
		// Make sure the RecvOverlapped struct is zeroed out
		SecureZeroMemory((PVOID) &clie.RecvOverlapped, sizeof(WSAOVERLAPPED) );
		// Create an event handle and setup an overlapped structure.
		clie.RecvOverlapped.hEvent = WSACreateEvent();
#endif
	}
}

int get_list_reqs(TCHAR* lista, const int lid, const int buffsize) {
	UINT len			= 0;
	TCHAR* p			= lista;
	int row				= 0;
	int lon				= 0;
	const int MAXFUNBUF = ZTYPCOMMAX+1+ZFUNNAMMAX+1+16;
	TCHAR funs[MAXFUNBUF * MAXFUNSPROC];

	for(register UINT index = 0; index < MAX_CLIES; ++index) {
		sClients& clie = clies[index];

		defchar(dateproc, 32);
		if(clie.status != FREE && clie.dateprocess) {
			struct tm* today = _localtime64(&clie.dateprocess);
			_tcsftime(dateproc, 15, _T("%H:%M:%S"), today);
		}

		double cputime = clie.status == EXECUTING ?	((double)0.0) :	((double)clie.reqst) / CLOCKS_PER_SEC ;
		double rectime = clie.status == EXECUTING ?	((double)0.0) :	((double)clie.recst) / CLOCKS_PER_SEC ;
		double sndtime = clie.status == EXECUTING ?	((double)0.0) :	((double)clie.sndst) / CLOCKS_PER_SEC ;

		TCHAR* pfuns = funs;
		for(register int i=0; i<clie.restop; ++i) {
			//int cl = clie.tcmlen[i];
			//if(cl>=0 && cl<ZTYPCOMMAX)	{ _tmemcpy(pfuns, clie.typecom[i] , cl); pfuns+=cl; *(pfuns++)=':'; }
			int fl = clie.funlen[i];
			if(fl>=0 && fl<ZFUNNAMMAX)	{ _tmemcpy(pfuns, clie.function[i], fl); pfuns+=fl; *(pfuns++)=':'; }
			pfuns += mikefmt(pfuns, _T("(%.3f)-"), ((double)clie.cputime[i]) / CLOCKS_PER_SEC);
		}
		int flen = pfuns-funs;
		funs[flen?flen-1:flen] = 0;
		require(flen >= (MAXFUNBUF * MAXFUNSPROC), _T("memory_overrun"));

		lon = mikefmt(p,_T("[l%d%dA:%ld][l%d%dB:%s][l%d%dC:%s][l%d%dD:%s][l%d%dE:%s][l%d%dF:%s]")
						_T("[l%d%dG:%.3f:%.3f:%.3f][l%d%dH:%ld:%ld][l%d%dI:%ld]")
						_T("[l%d%dJ:%s][l%d%dK:%d][l%d%d*:%d]"),
						//A
						lid, row, index,
						lid, row, statdescs[clie.status], 
						lid, row, clie.machine,
						lid, row, clie.user,
						lid, row, clie.trans,
						lid, row, dateproc,
						//G
						lid, row, cputime,rectime,sndtime,
						lid, row, clie.packsin,clie.packsout,
						lid, row, clie.nfuns,
						//J
						lid, row, funs,
						lid, row, clie.step,
						lid, row, clie.status);
		++row;
		len += lon;
		p+=lon;
		require(len >= buffsize, _T("memory_overrun"));
	}

	TCHAR k[1024]; int kl=0;
	for(UINT i = 0; i < MAX_PROCS; ++i)
		kl += mikefmt(&k[kl],_T(" %d:%ld;"), i, procs[i].accesses);
	kl += mikefmt(&k[kl],_T(" rejs:%ld, gapr:%ld, list(%d),disp(%d),sche(%d),proc(%d),resp(%d),notf(%d),cxpr(%d)"),
					grejectedreqs, ggapreqs, PRIO_LST, PRIO_DSP, PRIO_SCH, PRIO_PRC, PRIO_RSP, PRIO_NOT, DEF_CLXPR);

	len += gen_tot_list(p,lid,row,k,kl);
	require(len >= buffsize, _T("memory_overrun"));
	return len;
}
