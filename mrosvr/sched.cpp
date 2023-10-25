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

HANDLE* incomings;
HANDLE* incomings2;
HANDLE* requests;
HANDLE* requests2;
HANDLE* responses;
HANDLE* responses2;

/**
 *	initialize the socket reader, we intend to gain performance putting TCP_NO_DELAY
 *	and in consecuence the sockets created with this socket will have the same feature
 *	and use a global socket in order to gain some performance
 */
void initialize_socket(SOCKET& listener) {
	listener = socket(PF_INET, SOCK_STREAM, IPPROTO_TCP);
	require(listener == INVALID_SOCKET, _T("Failed_socket()"));

	TCHAR prt[8]; // the idea is gatport or appport or repport, etc...
	set4ch(&prt[0], gsvrtype[0],gsvrtype[1],gsvrtype[2],_T('p'));
	set4ch(&prt[4], _T('o'),_T('r'),_T('t'),0);

	SOCKADDR_IN sin;
	sin.sin_family = PF_INET;
	sin.sin_port = htons(gservers.getint(prt));

	sin.sin_addr.s_addr = INADDR_ANY;
	memset(sin.sin_zero, 0, sizeof(sin.sin_zero));

	require(bind(listener, (LPSOCKADDR)&sin, sizeof(sin)) == SOCKET_ERROR, _T("bind"));
	require(listen(listener, SOMAXCONN) == SOCKET_ERROR, _T("listen"));
//	int on = 1;
//  require(setsockopt(listener, SOL_SOCKET, SO_REUSEADDR, (char*)&on, sizeof(on)), _T("SO_REUSEADDR"));
}

/**  
 *	there is a little difference between bussy and overload(at least on this server)
 *	bussy means that there are no threads/cpus/process available to run some request and 
 *	overload means that there are comming too many request to this server that it has 
 *	the ability to accept. and of course both can happen at the same time but not necessarly
 */
TCHAR SERVERBUSSY[]			=	_T("server_bussy_try_later");
TCHAR SERVEROVERLOADED[]	=	_T("server_overloaded_try_later");

/*	
	this function basically clean some important variables for start the process, 
	it has to be clear the we are cleaning the client data not the process(cpu) state
*/
void init_request(sClients& clie) {
	time(&clie.dateprocess);							// when the request was accepted
	clie.reqst						= GetTickCount64();	// start the execution's time
	clie.recst						= 0;

	clie.sndst						= 0;
	clie.step							= 0;
	clie.accessno					= ++gaccessno;		// one more access to the server

	clie.maclen = clie.machine[0]	= 0;
	clie.trnlen = clie.trans[0]		= 0;
	clie.usrlen = clie.user[0]		= 0;

	clie.retresult					= true;
	clie.fromgate						= false;

	clie.inlen							= 0;
	clie.packsin						= 0;
	clie.outlen							= 0;				// response lenght
	clie.packsout						= 0;				// packets send
	clie.isunicode					= false;
	clie.isfromexplorer			= false;
	clie.reqreaded					= false;
	clie.endmark						= false;

	clie._gatsvr[0]					= 0;
	clie.pgatsvr						= nullptr;
	clie.gatsvl							= 0;
	clie.gatprt							= 0;

	clie.reqerror[0]				= 0;

#ifdef MRO_IOCP_IMP 
	WSAResetEvent(clie.RecvOverlapped.hEvent);
#endif
}

/** 
 *	this function check if the request is a valid one, but add a tag that say so
 *	because this request could be redispatched, and by doing this, we can avoid 
 *	checking twice
 */
void verify_request(sClients& clie) {
	clie.step = 20;
	if(!clie.request.isactv(REQCHKD, REQCHKDLEN))	{
		if(clie.request.is_bad_formed()) {
			// we give a chance to us if we could retrive the machine because
			// they are important altough we know that it is a bad request
			get_ip_and_name(clie.socket, clie.machine, clie.maclen);
			require(true, _T("wrong_formed_request"));
		}
		// we mark it cause if this point is reached it does not exist
		clie.request.active(REQCHKD, REQCHKDLEN);
	}
}

/**
 *	this function process the header
 *	key name : ZHEADER
 *	attriubtes: retresult
 *				priority	(not implemented at this version)
 *				zpkgname
 *				server
 */
void handle_header(sClients& clie) {
	clie.priority	= 5;			// by default normal priority
	clie.retresult	= true;			// by default we respond
	register cpairs& h = clie.header;

	if(clie.request.get(ZHEADER, h, ZHEADERLEN)) {
		// verify if the request will return response
		clie.retresult = h.getbool(ZRETRES, ZRETRESLEN);
		// check its priority if any
		clie.priority = h.getint(ZPRIORI, ZPRIORILEN);
		// the are free functions or packages of free functions
		clie.trnlen = h.get(ZPKGNAM, clie.trans, ZPKGNAMLEN, ZPKGNAMMAX);

		TCHAR service[ZSERVERMAX+1];
		// only the gate service can check for some other service
		/*if (h.get(ZSERVER, service, ZSERVERLEN, ZSERVERMAX)) {
			wchar_t key[8];
			// try first to find out if we are gonna use a group of nodes(services)
			set4ch(&clie.rdsnam[0], service[0], service[1], service[2], 0); // original
			set4ch(&key[0], service[0], service[1], service[2], '_');		// original
			set4ch(&key[4], 'g','r','p',0); 

			if(gdefgrps.get(key, clie.grpsvrs, 7)) {
				TCHAR grpkey[8];
				set4ch(grpkey, key[0], key[1], key[2], clie.accessno % clie.grpsvrs.getint(_T("nsvrs"), 5) + 48);
				grpkey[4] = 0;
				clie.grpsvrs.get(grpkey, key, 4, 7);				// distributed
				set4ch(&clie.rdsnam[0], key[0],key[1],key[2],0);	// distributed
			}

			set4ch(&key[3], '_','s','v','r'); 
			clie.rdssvrlen = gservers.get(key, clie.rdssvr,  7, 15);

			if(is_127001(clie.rdssvr, clie.rdssvrlen)) {
				_tmemcpy(clie.rdssvr, localsvr, localsvrlen); 
				clie.rdssvr[clie.rdssvrlen = localsvrlen] = 0;
			}

			//set4ch(&key[3], 'p','o','r','t'); 
			//clie.rdsprt = gservers.getint(key,	7);
			//clie.redispatch = true;
		}*/
	}
}

void handle_basics(sClients& clie) {
	register cpairs& b = clie.basics;
	clie.request.get(ZBASICS, b, ZBASICSLEN);
	b.append(gwork, gworklen);
	// client ip address
	clie.maclen = b.get(ZMACNAM, clie.machine, ZMACNAMLEN, ZMACNAMMAX);
	// if the header get the package name we dont get it 
	if(clie.trnlen == 0)
		clie.trnlen = b.get(ZTRNCOD, clie.trans, ZTRNCODLEN, ZTRNCODMAX);
	// get the user rights purposes
	clie.usrlen = b.get(ZUSERID, clie.user, ZUSERIDLEN, ZUSERIDMAX); 
}

/** 
 * get some impotant data from the request, refering to indenty the client, we check
 * against 4 for the size cause it is the most common size
 */
void get_identity(sClients& clie) {
	clie.pgatsvr = gatsvr;
	clie.gatprt	 = gatprt;
	clie.gatsvl  = gatsvl;
}
/**
 * validate that if is it redispatch dont redispatch to itself otherwise will hung up
 */
void validate_request(sClients& clie) {
	require(clie.gatprt==0 || clie.gatsvl==0, _T("missing_proxy"));

	// ****** probably the info is already in basics we need to check it out *******
	clie.basics.set(ZGATSVR, clie.pgatsvr, ZGATSVRLEN, clie.gatsvl);
	clie.basics.set(ZGATPRT, clie.gatprt, ZGATPRTLEN);

	// prevent call recursive and run for ever, and dispatch the normal(if gate)
	/*if (clie.redispatch) {
		int rl = clie.rdssvrlen;
		if(	clie.rdsprt == clie.gatprt && rl == clie.gatsvl) {
			TCHAR* rd = clie.rdssvr;
			TCHAR* pg = clie.pgatsvr;
			if((rl >= 10 && rl <= 12) &&   
				(cmp4ch(&rd[0], pg[0],pg[1],pg[2],pg[3]) && 
				cmp4ch(&rd[4], pg[4],pg[5],pg[6],pg[7]) && 
				cmp2ch(&rd[8], pg[8],pg[9]))) {
				if(rl == 10) clie.redispatch = false;
				else if(rl == 11 && rd[10] == pg[10]) clie.redispatch = false;
				else if(rl == 12 && cmp2ch(&rd[10], pg[10],pg[11])) clie.redispatch = false;
			}
			else if(_tmemcmp(rd, pg, rl + 1) == 0)
					clie.redispatch = false;
		}
	}*/
}

/**
 *	this function return some message(some error) to the client
 *	and writes it at the same time to the event viewer for analisis
 */
void ret_error(SOCKET s, TCHAR* error) {
	send_direct_2_client(s, error, _tcslen(error), true);
}

/**
 *	this function tries to get some information about the state of the process and
 *	write it on the event viewer for debuging purposes
 */
void overworking(SOCKET s, TCHAR* error) {
	ret_error(s, error);
	TCHAR toevent[1024];
	int len = mikefmt(toevent, _T("%s procs:"), error);
	get_executing_functions(&toevent[len]);
	write2event(EVENTLOG_INFORMATION_TYPE, toevent);
}

/**
 *	this functions solve a nasty problems with WaitforMul... function, that is
 *	of it does not support more than 64 entries, so it uses a rude trick to give
 *	the impression of handling at least 128, it has to be done propperly but
 *	at this moment do the job quiet a decent
 */
DWORD get_client(HANDLE* req1, HANDLE* req2, DWORD timeout) {
	if(MAX_CLIES <= 64)	
		return WaitForMultipleObjects(MAX_CLIES,req1,false,timeout);
	else {
		DWORD timeout2 = 0;
		loopclie:
		DWORD r1 = WaitForMultipleObjects(64,req1,false, ggapreqs);
		if(r1<0 || r1>=MAX_CLIES) {
			DWORD r2 = WaitForMultipleObjects(MAX_CLIES-64,req2,false,ggapreqs);
			if(r2<0 || r2>=(MAX_CLIES-64)) {
				if(timeout != INFINITE)
					if((timeout2 += ggapreqs*2) >= timeout) return WAIT_TIMEOUT; 
				goto loopclie;
			}
			else return r2 + 64;
		}
		else return r1;
	}
	return -1;
}

bool wait_response(sClients& clie) {
	DWORD c = WaitForSingleObject(clie.id < 64 ?	responses[clie.id] : 
													responses2[clie.id-64], 16);
	if(c != WAIT_OBJECT_0) return false;

	ResetEvent(clie.id < 64 ? responses[clie.id] : responses2[clie.id-64]);

	return true;
}

void start_request(sClients& clie) {
	clie.step		= 10;
	clie.recst	= GetTickCount64();	
	clie.status = READING;
}

bool send_respond(sClients& clie) {
	int stp = clie.resstp;
	if(clie.pendingres[stp]) { // do we have a response slice pending to be sent?
		// find out if we reach the end of respond
		bool endres = clie.pendingres[MAXFUNSPROC] ?	stp == MAXFUNSPROC :
													(stp+1) == clie.restop;

		if(clie.results[stp].get_len()) {// maybe the response has no data
			clie.results[stp].compact(false);
			respond(clie.id,clie.results[stp].buffer(), clie.results[stp].get_len(), endres);
		}

		// we are donde with this response slice
		clie.pendingres[stp] = false; 
	}

	// if reach normal responses end we move to the error (last slice)
	if ((stp + 1) == clie.restop)	{
		clie.resstp = MAXFUNSPROC;
		return clie.pendingres[clie.resstp] == false; // no need to look for it 
	}

	// if reach the end of response sclices we are done
	if(++clie.resstp >= (MAXFUNSPROC+1)) return true;

	return false;
}

/**
 *	this function is the loop for process the requests, but dont get confused because
 *	they are not executed, this function basically reads the socket and put the request
 *	on one CParameter object in order to be used on the execution, one could see this
 *	function as the transformation between meaningless raw socket data entry to a
 *	meaningfull request object
 */
unsigned __stdcall dispatcher(LPVOID pParam) {
	// the first step is just wating for a new sockets to arrive, then we must process
	// and validate the request, and at the last we we mark the client as read and
	// processed in order to the dispatcher can see it and take for execution
	HANDLE thrd = ::GetCurrentThread();

	size_t szb = sizeof(bool) * MAX_CLIES;
	size_t szi = sizeof(int)  * MAX_CLIES;

	bool* waiting	= (bool*)_malloca(szb);
	int* stps			= (int*) _malloca(szi);
	int* posic		= (int*) _malloca(szi);
	DWORD idle		= 0;

	loop:

		memset(waiting,false, szb);
		memset(stps		,0		, szi);

		wait:
		if (ajst_dsp) { 
			SetThreadPriority(thrd, PRIO_DSP); 
			ajst_dsp = false; 
		}

		// basycally our main trigger
		if(get_client(requests,requests2,INFINITE) == WAIT_TIMEOUT) goto wait;

		int pos=0;

		extract:
		if(MAX_CLIES <= 64) {
			for(register int i=0; i<MAX_CLIES; ++i) {
				DWORD ev = WaitForMultipleObjects(MAX_CLIES, requests, false, 0);
				if(ev<0 || ev>=MAX_CLIES) goto loop2; // when no more goto process them
				ResetEvent(requests[int(ev)]);
				posic[pos]       = ev;
				waiting[pos++]   = true;
				clies[ev].status = TAKEN;
			}
		}
		else {
			for(register int i=0; i<64; ++i) {
				DWORD ev = WaitForMultipleObjects(64, requests, false, 0);
				if(ev<0 || ev>=64) break; // when no more goto process them
				ResetEvent(requests[int(ev)]);
				posic[pos]       = ev;
				waiting[pos++]   = true;
				clies[ev].status = TAKEN;
			}
			for(register int i=0; i<(MAX_CLIES-64); ++i) {
				DWORD ev = WaitForMultipleObjects(MAX_CLIES-64, requests2, false, 0);
				if(ev<0 || ev>=(MAX_CLIES-64)) break; // when no more goto process them
				ResetEvent(requests2[int(ev)]);
				ev += 64;
				posic[pos]       = ev;
				waiting[pos++]   = true;
				clies[ev].status = TAKEN;
			}
		}
		if(pos == 0) goto wait;

	loop2:

		int pendings = 0;
		for(register int ev=0; ev<=pos; ++ev) {
			if(ev==pos) {ev = 0;}			// we make another round
			if(ev==0) pendings = 0;
			pendings += waiting[ev];
			if(ev==(pos-1) && pendings == 0) break;
			if(!waiting[ev]) continue;		// check only the waiting ones

			sClients& clie = clies[posic[ev]];
			if(gexectyp == 1) {
				try {
					switch(stps[ev]) {
					case  0: start_request(clie);			++stps[ev];	break;
					case  1: if(read_request(clie))		++stps[ev];	
							 else ++idle;
							 break;
					case  2: if(!clie.reqreaded)			goto dispatchproc;
									 verify_request(clie);		++stps[ev];	break;
					case  3: handle_header(clie);			++stps[ev];	break;
					case  4: handle_basics(clie);			++stps[ev];	break;
					case  5: get_identity(clie);			++stps[ev];	break;
					case  6: validate_request(clie);	++stps[ev];	break;
					dispatchproc:
					case  7: SetEvent(clie.procreq);	++stps[ev];	break;
					case  8: if(wait_response(clie))	++stps[ev];	
							 else ++idle;
							 break;
					case  9: if(send_respond(clie))		++stps[ev];	
							 else ++idle;
							 break;
					case 10: goto endrequest;
					default: goto endrequest;
					}
					if(idle == 64) { idle = 0; goto extract; };
					continue;
				}
				catch(CString& e)		{	_tcscpy_s(clie.reqerror, e.GetBuffer());					}
				catch(char* e)			{	_tcscpy_s(clie.reqerror, _T("req_char_error"));		}
				catch(wchar_t* e)		{	_tcscpy_s(clie.reqerror, e);											}
				catch(_com_error &e){	_tcscpy_s(clie.reqerror, (TCHAR*)e.Description());}
				catch(CException *e){	TCHAR d[1024];	e->GetErrorMessage(d,1024);
															e->Delete();	
															_tcscpy_s(clie.reqerror, d);						
														}
				catch(mroerr& e)		{	_tcscpy_s(clie.reqerror, e.description);					}
				catch(...)					{	_tcscpy_s(clie.reqerror, _T("req_unhandled_error"));}

				// just for be careful
				clie.reqerror[CLIEERRLEN] = 0;					

				// if havent respond we try to respond at least the error
				if(stps[ev] < 7) { stps[ev] = 7; continue; }

			endrequest:
				finish_request(clie);
				waiting[ev] = false;
			}
			else {
				try {
					switch(stps[ev]) {
					case  0: start_request(clie);			++stps[ev];	break;
					case  1: if(read_request(clie))		++stps[ev];	break;
					case  2: if(!clie.reqreaded)			goto execute; 
								   verify_request(clie);		++stps[ev];	break;
					case  3: handle_header(clie);			++stps[ev];	break;
					case  4: handle_basics(clie);			++stps[ev];	break;
					case  5: get_identity(clie);			++stps[ev];	break;
					case  6: validate_request(clie);	goto execute;
					}
					continue;
				}
				catch(CString& e)		{	_tcscpy_s(clie.reqerror, e.GetBuffer());						}
				catch(char* e)			{	_tcscpy_s(clie.reqerror, _T("req_char_error"));			}
				catch(wchar_t* e)		{	_tcscpy_s(clie.reqerror, e);												}
				catch(_com_error &e){	_tcscpy_s(clie.reqerror, (TCHAR*)e.Description());	}
				catch(CException *e){	TCHAR d[1024];	e->GetErrorMessage(d,1024);
															e->Delete();	
															_tcscpy_s(clie.reqerror, d);												}
				catch(mroerr& e)		{	_tcscpy_s(clie.reqerror, e.description);						}
				catch(...)					{	_tcscpy_s(clie.reqerror, _T("req_unhandled_error"));}

				// just for be careful
				clie.reqerror[CLIEERRLEN] = 0;

			execute:
				waiting[ev] = false;
				SetEvent(clie.procreq);			// what ever happens corresponds to client
			}
		}
	goto loop;
}

/**
 *	this function is the loop for dispatch the requests to execute, this function
 *	does not know nothing about how the clients comes, it has one very specific 
 *	task, that is of execute a limited number of process at the same time, in order 
 *	to minimize the context switching between threads, this limit is arbitrary 
 *	and it is setup in the initilization process (MAX_PROCS)
 */
unsigned __stdcall scheduler(LPVOID pParam) {
	// so the basic idea is simple, we wait first for client to be processed and then 
	// wait at least for one cpu available, then when we got both the client processed 
	// and the cpu, we mark the cpu taken in order to that in the next cycle does not 
	// appear(at least meanwhile is running), then we engage the cpu with the client, 
	// then we start off the cpu, and at the last we we mark the client taken in order 
	// to that the client does not appear int the next cycle(at least meanwhile running)

	UINT gcturn = 0;
	UINT busies = 0;
	HANDLE thrd = ::GetCurrentThread();

	loop:

		try {
			read:
			if(ajst_sch) { 
				SetThreadPriority(thrd, PRIO_SCH); 
				ajst_sch = false; 
			}
			// wait for any processed client
			DWORD clieid	= get_client(incomings, incomings2, (1024*4)); 
			if(clieid == WAIT_TIMEOUT) 
			{
				if(gccollect(procs[gcturn], 1))	{ 
					if(++gcturn==MAX_PROCS) gcturn = 0;	
				}
				else { }
				goto read;
			}
			sClients& clie	= clies[clieid];

			// wait for any cpu available
			UINT tries4cpu = 0;
			forcpus:
			DWORD procid	= WaitForMultipleObjects(MAX_PROCS, cpus, false, 128);
			sProcess& proc	= procs[procid];

			if((procid > (MAX_PROCS - 1)) || (procid < 0)) {		// check out for any error
				++gwaitinghits;
				if(++tries4cpu < 40) goto forcpus;
				++grejectedhits;
				// if we have any kind of error we do not wait at all we simple send a error message 
				// to the client, this is because we dont want to hung the server waiting for cpus
				clie.status	= FAILED;												// we mark the client as failed
				overworking(clie.socket, SERVERBUSSY);			// we return to the client the error
				close_socket(clie);													// we close the client
				if(busies++>8) goodbye(_T("reseted_for_busy"),16); // too busy obvious something is wrong
			}
			else {
				busies					= 0;												// we return the counter to normally
				ResetEvent(cpus[procid]);										// we take this cpu

				init_proc(proc);
				proc.clieid				= clieid;									// we engage the proc queue with the client queue
				proc.status				= WAITING;								// we mark it as is ready for execution
				SetEvent(proc.startexecuting);							// *** execute the transaction's operations ***

				if(clieid < 64)	ResetEvent(incomings[clieid]);	// we mark as free the event to process the client
				else ResetEvent(incomings2[clieid-64]);			// we mark as free the event to process the client
			}
		}
		catch(CException *e)	{ e->Delete(); }
		catch(...)						{ }

	goto loop;
}

/**
 *	this function is the loop for dispatch the requests to execute, this function
 *	does not know nothing about how the clients comes, it has one very specific 
 *	task, that is of execute a limited number of process at the same time, in order 
 *	to minimize the context switching between threads, this limit is arbitrary 
 *	and it is setup in the initilization process (MAX_PROCS)
 */
/*unsigned __stdcall responder(LPVOID pParam)
{
	// the first step is just wating for a new sockets to arrive, then we must process
	// and validate the request, and at the last we we mark the client as read and
	// processed in order to the dispatcher can see it and take for execution
	HANDLE thrd = ::GetCurrentThread();

	bool* waiting	= (bool*)alloca(sizeof(bool)*MAX_CLIES);
	int* steps		= (int*)alloca(sizeof(int)*MAX_CLIES);
	int* posic		= (int*)alloca(sizeof(int)*MAX_CLIES);

	loop:

		memset(waiting	,false	,sizeof(bool)*MAX_CLIES);
		memset(steps	,0		,sizeof(int) *MAX_CLIES);

		read:
		if(ajst_rsp) 
		{ 
			SetThreadPriority(thrd, PRIO_RSP); 
			ajst_rsp = false; 
		}

		if(get_client(responses,responses2,INFINITE) == WAIT_TIMEOUT) goto read;

		int pos=0;
		if(MAX_CLIES <= 64)
		{
			for(register int i=0; i<MAX_CLIES; ++i)
			{
				DWORD ev = WaitForMultipleObjects(MAX_CLIES, responses, false, 0);
				if(ev<0 || ev>=MAX_CLIES) goto loop2; // when no more goto process them
				ResetEvent(responses[int(ev)]);
				posic[pos]=ev;
				waiting[pos++]=true;
			}
		}
		else
		{
			for(register int i=0; i<64; ++i)
			{
				DWORD ev = WaitForMultipleObjects(64, responses, false, 0);
				if(ev<0 || ev>=64) break; // when no more goto process them
				ResetEvent(responses[int(ev)]);
				posic[pos]=ev;
				waiting[pos++]=true;
			}
			for(register int i=0; i<(MAX_CLIES-64); ++i)
			{
				DWORD ev = WaitForMultipleObjects(MAX_CLIES-64, responses2, false, 0);
				if(ev<0 || ev>=(MAX_CLIES-64)) break; // when no more goto process them
				ResetEvent(responses2[int(ev)]);
				ev+=64;
				posic[pos]=ev;
				waiting[pos++]=true;
			}
		}

	loop2:

		int pendings = 0;
		for(register int ev=0; ev<pos; ++ev)
		{
			if(ev==0) pendings = 0;
			pendings += waiting[ev];
			if(ev==(pos-1) && pendings == 0) break;
			if(!waiting[ev]) continue;		// check only the 

			sClients& clie = clies[posic[ev]];
			clie.status = RESPONDING;

			int stp = steps[ev];
			try
			{
				if(clie.pendingres[stp])
				{
					bool endres = clie.pendingres[MAXFUNSPROC] ?stp == MAXFUNSPROC :
																(stp+1) == clie.restop;
					if(clie.results[stp].get_len())
					{
						clie.results[stp].compact(false);
						respond(clie.id,clie.results[stp].buffer(), clie.results[stp].get_len(), endres);
					}
					clie.pendingres[stp] = false;

					// if reach normal responses we move to the error if any
					if((stp+1) == clie.restop) 
					{ 
						steps[ev] = MAXFUNSPROC; 
						goto loop2; 
					}
				}
				if(++steps[ev] == (MAXFUNSPROC+1)) goto finish;

				goto loop2;
			}
			catch(CString& e)	{	_tcscpy_s(clie.reqerror, e.GetBuffer());			}
			catch(char* e)		{	_tcscpy_s(clie.reqerror, _T("req_char_error"));		}
			catch(wchar_t* e)	{	_tcscpy_s(clie.reqerror, e);						}
			catch(_com_error &e){	_tcscpy_s(clie.reqerror, (TCHAR*)e.Description());	}
			catch(CException *e){	TCHAR d[1024]; e->GetErrorMessage(d,1024);
									e->Delete();	
									_tcscpy_s(clie.reqerror, d);						}
			catch(mroerr& e)	{	_tcscpy_s(clie.reqerror, e.description);			}
			catch(...)			{	_tcscpy_s(clie.reqerror, _T("unhandled_error"));	}

			clie.reqerror[CLIEERRLEN] = 0;	// just for be careful
			respond(clie.id, clie.reqerror, _tcslen(clie.reqerror), true);	

		finish:
			waiting[ev] = false;
			finish_request(clie);
		}
	goto loop;
}*/

void initialize_threads()
{
	UINT ti;
	HANDLE		t =	(HANDLE) _beginthreadex(0, 0, dispatcher, 0, 0, &ti);
						t = (HANDLE) _beginthreadex(0, 0, scheduler	, 0, 0, &ti);
	//if(gexectyp == 0)	t = (HANDLE) _beginthreadex(0, 0, responder	, 0, 0, &ti);
						t = (HANDLE) _beginthreadex(0, 0, backgroud , 0, 0, &ti);
	// our background thread for check the stability of the system
	// very important note: we do not put this thread in lowest priority because
	// when the others threads are jamed, waiting for example the database to return
	// it will never receive cpu in other to restablished the database
	// so the situation becomes a paradox: it try to help you but you not give me
	// the time to help you, put it to below normal it is a guarranty that at some 
	// point we are trying to reconnect to the database
}

/**
 *	this function is responsible for finding a place free in the request queue so the
 *	criteria is simple find a slice where it's status is free, killed, failed or done
 */
int get_client_slot(int slot) {
	int round = 0;														// we start from the first round
	sClients* clie = &clies[slot];

	for(register int start = slot; ;++slot, ++clie)	{
		if(slot == MAX_CLIES) {									// check the limit for do other round
			clie = &clies[slot = 0];							// we start from the bottom	
			++round;															// we have another round
		}

		if(slot == start && round > 1) return -1;	// we reach a round and there is none

		mrostatus status = clie->status;				// for concurrency matters we make a copy

		if(	status == DONE ||	status == FREE ||	// find out if it is available
			status == KILLED ||	status == FAILED) { 
			clie->status = ACCEPTED;							// we mark it as taken
			return slot; 
		}
 	}

	return -1;																// not a place available
}

/**
 *	this function is the main loop that receives requests and put them in a request
 *	queue in order to be dispatched later, but it is important to notice that this 
 *	function does neither dispatch, read data or execute any processes at all, 
 *	it's main purpose is just one and only one it receive clients and put them where 
 *	the dispatcher later in the future will eventually execute them, at this version
 *	the accepting is blocking, later version will use ICP 
 */
void listener(SOCKET l) {
	// this thread is settled with a higher priority because if not, and when a lot of
	// threads (functions) are executed its time slice sometimes is reduced to the
	// point of can't read the incomming sockets queue, and therefore not empty it 
	// appropretally, causing some clients to start not being able of get a connection,
	// so with a litte attention to this thread we can assure that the accepting sockets 
	// process is more secure, because it will have more time to try to receive clients

	int c					= 0;										
	int rejs			= 0;
	int invs			= 0;
	bool isvalid	= false;
	HANDLE thrd		= ::GetCurrentThread();

	loop:

	try	{
		if(ajst_lst) {													// maybe we need adjust its priority
			SetThreadPriority(thrd, PRIO_LST); 
			ajst_lst = false; 
		}

		c = get_client_slot(c);									// in advance we get a place if any

		SOCKET s = WSAAccept(l, 0, 0, 0, 0);		// we wait for a new client

		if(!(isvalid = s != INVALID_SOCKET))		// nothing guarranties that we have a valid client
			if(++invs > 16)	
				goodbye(_T("invalids"),8);					// just for precaution, we force a fresh restart

		if(c == -1) {														// another chance if it a place wasn't get
			c = 0;																// we cannot start searching from -1 in the next cycle
			if(!isvalid) goto loop;								// no client no socket we've got nothing so we go next cycle
			if((c = get_client_slot(c)) == -1) {	// retry and we give up if there is no place available
				c = 0;															// we cannot start searching from -1 in the next cycle
				if(isvalid) {												// if dont have a client we can't return anything
					closesocket(s);										// we close the client
					if(++rejs > 32)	
						goodbye(_T("overload"),8);			// just for precaution, we force a fresh restart
					++grejectedreqs;
				}
				goto loop;													// another cycle
			}
		} 
		rejs = 0;

		if(!isvalid) {													// some error could happen 
			clies[c].status = FAILED;							// we return the precious slice to the stock
			goto loop;														// another cycle
		}
		invs = 0;

		sClients& cl = clies[c];

		init_request(cl);			

		if(cl.socket != INVALID_SOCKET) 
			close_socket(cl);											// for any chance to have a slice with open socket;
		cl.socket = s;													// we engage the client and request slot

		SetEvent(c<64 ? requests[c] : requests2[c-64]);		// notify the destiler there is a new client
		SetEvent(c<64 ? incomings[c] : incomings2[c-64]);	// notify the dispatcher there is a new client
	}
	catch(CException *e)	{ e->Delete(); }
	catch(...)						{ }

	goto loop;
}


/*
unsigned __stdcall dispatcher(LPVOID pParam)
{
	// the first step is just wating for a new sockets to arrive, then we must process
	// and validate the request, and at the last we we mark the client as read and
	// processed in order to the dispatcher can see it and take for execution
	HANDLE thrd = ::GetCurrentThread();

	bool* waiting	= (bool*)alloca(sizeof(bool)*MAX_CLIES);
	int* steps		= (int*)alloca(sizeof(int)*MAX_CLIES);
	int* posic		= (int*)alloca(sizeof(int)*MAX_CLIES);

	loop:

		memset(waiting,		false	,sizeof(bool)*MAX_CLIES);
		memset(steps,		0		,sizeof(int)*MAX_CLIES);

		read:
		if(ajst_dsp) 
		{ 
			SetThreadPriority(thrd, PRIO_DSP); 
			ajst_dsp = false; 
		}

		DWORD value = get_client(requests, requests2, INFINITE); 
		if(value == WAIT_TIMEOUT) goto read;

		int pos=0;
		if(MAX_CLIES <= 64)
		{
			for(register int i=0; i<MAX_CLIES; ++i)
			{
				DWORD ev = WaitForMultipleObjects(MAX_CLIES, requests, false, 0);
				if(ev<0 || ev>=MAX_CLIES) goto loop2; // when no more goto process them
				ResetEvent(requests[int(ev)]);
				posic[pos]=ev;
				waiting[pos++]=true;
				clies[ev].status = TAKEN;
			}
		}
		else
		{
			for(register int i=0; i<64; ++i)
			{
				DWORD ev = WaitForMultipleObjects(64, requests, false, 0);
				if(ev<0 || ev>=64) break; // when no more goto process them
				ResetEvent(requests[int(ev)]);
				posic[pos]=ev;
				waiting[pos++]=true;
				clies[ev].status = TAKEN;
			}
			for(register int i=0; i<(MAX_CLIES-64); ++i)
			{
				DWORD ev = WaitForMultipleObjects(MAX_CLIES-64, requests2, false, 0);
				if(ev<0 || ev>=(MAX_CLIES-64)) break; // when no more goto process them
				ResetEvent(requests2[int(ev)]);
				ev+=64;
				posic[pos]=ev;
				waiting[pos++]=true;
				clies[ev].status = TAKEN;
			}
		}

	loop2:

		int pendings = 0;
		for(register int ev=0; ev<pos; ++ev)
		{
			if(ev==0) pendings = 0;
			pendings += waiting[ev];
			if(ev==(pos-1) && pendings == 0) break;
			if(!waiting[ev]) continue;		// check only the 

			sClients& clie = clies[posic[ev]];

			try
			{
				switch(steps[ev])
				{
				case  0: clie.recst = GetTickCount64();	
						 clie.status = READING;			steps[ev]++;	break;
				case  1: if(read_request(clie))			steps[ev]++;	break;
				case  2: if(!clie.reqreaded)			goto execute; 
						 verify_request(clie);			steps[ev]++;	break;
				case  3: handle_header(clie);			steps[ev]++;	break;
				case  4: handle_basics(clie);			steps[ev]++;	break;
				case  5: get_identity(clie);			steps[ev]++;	break;
				case  6: validate_request(clie);		goto execute;
				}
				goto loop2;
			}
			catch(CString& e)	{	_tcscpy_s(clie.reqerror, e.GetBuffer());			}
			catch(char* e)		{	_tcscpy_s(clie.reqerror, _T("req_char_error"));	}
			catch(wchar_t* e)	{	_tcscpy_s(clie.reqerror, e);						}
			catch(_com_error &e){	_tcscpy_s(clie.reqerror, (TCHAR*)e.Description());	}
			catch(CException *e){	TCHAR szCause[1024];	e->GetErrorMessage(szCause,1024);
									e->Delete();	
									_tcscpy_s(clie.reqerror, szCause);					}
			catch(mroerr& e)	{	_tcscpy_s(clie.reqerror, e.description);			}
			catch(...)			{	_tcscpy_s(clie.reqerror, _T("req_unhandled_error"));}
			clie.reqerror[CLIEERRLEN] = 0;					// just for be careful

		execute:
			waiting[ev] = false;
			SetEvent(clie.procreq);			// what ever happens corresponds to client
		}
	goto loop;
}
*/