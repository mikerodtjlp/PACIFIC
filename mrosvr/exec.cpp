#include "stdafx.h"

/************************************************************************************
* description   : mro server
* purpose       : main engine for execute the transactions from the clients
* author        : miguel rodriguez
* date creation : 20 junio 2004
**************************************************************************************/

#include "mrosvr.h"
#include "defs.h"

/**	
 * this function eliminates de constant mark ^, and let the expression alone
 * this is some way the quote stuff in lisp, quoting a expression is marking
 * for not be evaluated on the variables substitution(get_values)
 */
void process_constants(sProcess& proc, CParameters& prms) {
	TCHAR ckey[64];
	int clk=0;
	int maxcval = prms.get_len();
	TCHAR* cval = get_memory(proc, maxcval, 0);
	int clv = 0;

	int nkeys = prms.nkeys();
	require(nkeys > 8192, _T("constants"));
	for(register int i=0; i<nkeys; ++i)	{
		if(prms.getpair(i, ckey, cval, clk, clv, 63, maxcval-1)) {
			TCHAR* p = cval;
			int left = clv;
			while(p = _tmemchr(p, '^', left)) {
				++p;
				--left;
				_tmemcpy(p-1, p, left);
			}
			// we assume that the optimization is not change because always
			// we set a shorter len so according with the rules it remains the same(positions)
			if(left != clv) prms.set(ckey, cval, clk, left);
		}
	}
}

/**
 * this function takes a command string, and its values, and substitute each of the
 * variables with its values, for example: "select * from x where y='$var1$' and
 * z='$var2$' and its values are: [$var1$:miguel][$var2$:123] then the command string
 * is transformed to something like: "select * from x where y='miguel' and z='123' 
 */
void replace_values(sClients& clie, 
					sProcess& proc,	
					CParameters& prms, 
					CParameters& values, 
					int& nconstvals,
					CParameters* notempty,
					CParameters* result) {
	TCHAR vkey[64];
	int vlk=0;
	int maxvval = values.get_len();
	if(maxvval == 0) return;
	TCHAR* vval = get_memory(proc, maxvval, 0);
	int vlv=0;

	TCHAR ckey[64];
	int clk=0;
	int maxcval = prms.get_len();
	TCHAR* cval = get_memory(proc, maxcval, 1);
	int clv=0;

	TCHAR inc_dat[64];
	int incl=0;

	int nvals	= values.nkeys();
	if(nvals	== -1) return;
	int ncoms	= prms.nkeys();
	if(ncoms	== -1) return;
	require(ncoms > 16384, _T("values"));

	CParameters temp;
	CString sprms;
	for(register int i=0; i<ncoms; ++i)	{
		if(prms.getpair(i, ckey, cval, clk, clv, 63, maxcval-1)) {
			sprms.SetString(cval, clv);	
			int sprmslen = clv;
			for(int j=0; j<nvals; ++j) {
				if(values.getpair(j, vkey, vval, vlk, vlv, 63, maxvval-1)) {
					int pattern = -1;
					while(  (pattern + 1 < sprmslen) &&
							(pattern = sprms.Find(vkey, pattern+1)) != -1) {
						if(pattern > 0 && sprms.GetAt(pattern-1) == '^') 
							++nconstvals;
						else {
							if (!vlv && notempty != nullptr) {//check if should not be empty
								if (incl = notempty->get(vkey, inc_dat, vlk, 63)) {
									result->set(_T("zincfld"), vkey, 7, vlk);
									require(true, CString(inc_dat));
								}
							}
							sprms.Delete(pattern, vlk);
							sprms.Insert(pattern, vval);
							sprmslen = sprms.GetLength();
							pattern += vlv;
						}
					}
				}
			}
			temp.set(ckey, sprms, clk);
		}
	}
	if(!temp.isempty()) prms.rpl_from(temp);
}

/**
 * this funtcion is used to take special values from the request and if necesary it 
 * creates memory, you may wonder why we do this, whats the difference to put the 
 * value in some CString or on a TCHAR array, and the answer is that this particular 
 * value can be of any lenght and there is no guarrantie for being small or large, 
 * so we can't use array, because we dont know how large would it be, and we can't 
 * use a CString because this value is used on performance demanding proceses, so 
 * the only reasonable solution is keep and cache of large chuncks of memory, and if 
 * the current chunck is greater we still use the same otherwise we create a larger one
 *
 * we use the lenght of the entire packet as the maximun length cause the values of 
 * course could be shorter but never not longer
 */
int get_data(	sClients& clie,				
				sProcess& proc,
				int& valueslen,						// our current value string length
				TCHAR*& p,								// our destiny value
				const TCHAR* key,					// our key to be found
				const int keylen,					// the key's length
				CParameters& params) {		// where we looking for
	// how much is the maxium value that we can obtain?
	// the total lenght of the source of course
	int maxvaluelen = params.get_len();							
	if(maxvaluelen > valueslen) {									// make memory if needed
		if(proc.gcpoolid >= MAXGCPOOL) gccollect(proc, 1);
		int extraroom = maxvaluelen < 4096 ? 2048 : ((maxvaluelen*50)/100);
		valueslen = maxvaluelen + extraroom;				// make some extra room
		if(p) { 
			proc.gcpool[proc.gcpoolid] = p; 
			++proc.gcpoolid; 
		}	// destroy the garbage latter
		p = (TCHAR*)malloc(sizeof(TCHAR)*(valueslen + 1));	// create the new chunck
	}
	return params.get(key, p, keylen, valueslen);			// extract the values into a safe memory
}

/**
 * there is a nasty problem that arise when work with the 127.0.0.1
 * address so we must detected in order to take actions on it
 */
bool is_127001(const TCHAR* ip, const int ipl) {
	return ipl == 9 &&	cmp4ch(ip, '1','2','7','.') && 
						cmp4ch(&ip[4], '0','.','0','.') && 
						cmp2ch(&ip[8], '1', 0);
}

/**
 * this function checks is the user has sufficient rights in order to execute
 * the transaction, this function has to be very fast cause is a bottle neck
 * thats why we use a local cache and low level stuff
 */
/*void has_rights(sClients& clie, sProcess& proc)
{
	// first we check the desire right, or in its default is a free function
	TCHAR right2check[4];
	if(!proc.webservice.get(ZRIGHT1, right2check, ZRIGHT1LEN, ZRIGHT1MAX)) return;

	// when it comes from a gate, it'll be marked, so if it is marked but has no right is
	// because is has no rights, period
	if(proc.rights.isactv(_T("pre"), 3))
	{
		require(!proc.rights.isactv(right2check, 3), _T("insufficient_rights"));
		return;
	}

	if(clie.fromgate)	return;
	if(isgate)			return;

	// basically this part is executed if it was a direct connection with this service
	// by having not passed thorugh the gate service, and if we not passed thorugh the 
	// gate service, we dont get the rights so we must look for them, we must realize 
	// the we gonna use sockets because we cannot use the COM(we are not on gate service) 
	TCHAR command[1024];
	int cmdlen = mikefmt(command,	_T("[%s:%s][%s:1][%s:[%s:%s][%s:%s][%s:COM]]"), 
									ZBASICS, clie.basics.buffer(), 
									ZZNFUNS, 
									ZFUN00Z, ZCOMPNM, ZCTROBJ, ZFUNNAM, _T("lookrgt"), ZTYPCOM);

	// so we call the gate service to check if the request has rights
	proc.gatesock.execute(clie.pgatsvr, clie.gatprt, command, cmdlen, proc.rights);
	proc.rights.optimize();

	// any kind of error is a sign of not appropiate rights
	if(proc.rights.hasval(ZSERROR, ZSERRORLEN))
	{
		int errlen = proc.rights.get(ZSERROR, proc.hstr256, ZSERRORLEN, 255);
		ensure(errlen, proc.hstr256);
	}

	// re process a liitle
	int len = proc.rights.get(ZURGTSZ, proc.hstr256, ZURGTSZLEN, 255);
	proc.rights.set_value(proc.hstr256, len);
	if(proc.rights.isactv(_T("pre"), 3))
		require(!proc.rights.isactv(right2check, 3), _T("insufficient_rights"));
}*/

/**
 * the gate is a special node that beside of running services, it distribuites 
 * services for balancing the workload, so this function redispatch the service into
 * the real node, waits for the responde and returns it back to the original client, 
 * and the process is completly transparent for the user
 */
/*void redispatch(sClients& clie, sProcess& proc) {
	ULONGLONG cputimestart = GetTickCount64();
	clie.cputime[0] = 0;

	// for redispatch we mark it and we add the administration services
	TCHAR proxy[1024];
	int l = proc.rights.get_len();
	_tmemcpy(proxy, proc.rights.buffer(), l);
	l += cpairs::gen_pair(0, &proxy[l], ZBYGATE, _T("1"), ZBYGATELEN, 1);
	l += cpairs::gen_pair(0, &proxy[l], ZGATSVR, clie.pgatsvr, ZGATSVRLEN, clie.gatsvl);
	l += cpairs::gen_pair(0, &proxy[l], ZGATPRT, clie.gatprt, ZGATPRTLEN);
	clie.request.append(proxy, l);

	// because we dont execute the functions here, we need to puts this variables with
	// reasonable values that indicate the intention, all this for to not get confused 
	//set4ch(clie.typecom[0]  , '>','>','>',0); clie.tcmlen[0] = 3;
	//set4ch(clie.component[0], '>','>','>',0); clie.cmplen[0] = 3;
	set4ch(clie.function[0] , clie.rdsnam[0],clie.rdsnam[1],clie.rdsnam[2],0); clie.funlen[0] = 3;
	clie.cputime[0] = 0;
	clie.nfuns = 1;

	// any lack of data of course is an error
	require(clie.rdsprt == 0 || clie.rdssvr[0] == 0, _T("bad_redispatch_server_or_port"));
	// very important, if we redirect the request to our selves we will fall into a enternal cycle
	require(localprt == clie.rdsprt && _tcsncmp(localsvr, clie.rdssvr, localsvrlen) == 0, _T("cycle_call"));

	// we execute on the target service
	clie.step = 53;
	CParameters& result = get_response(clie, false);
	proc.gatesock.execute(clie.rdssvr, clie.rdsprt, clie.request.buffer(), clie.request.get_len(), result);
	result.optimize();

	// get the variables's values, only (but not necesarly) the gui clients handle 
	// values, so direct clients like external devices dont need this kind of overhead
	if(proc.workonvalues = clie.request.has(ZVALUES, ZVALUESLEN))
		clie.request.extract(ZVALUES, proc.values, ZVALUESLEN);

	// check if this request needs to save the result and this node is alloed to do it
//	proc.saveresult = result.getint(ZSAVSTA, ZSAVSTALEN);
//	result.del(ZSAVSTA, ZSAVSTALEN); // implementation detail must not be returned

	clie.cputime[0] = GetTickCount64() - cputimestart;
}*/

/** 
 *	this function extracts the specific params from the main function and process them
 *	exctract it from the main service, get the necesary memory, look for some KEYWRODS
 *	replace values and constats, and something more, in a nutshell if prepare the params
 */
int process_params(	sClients& clie, 
										sProcess& proc, CParameters* 
										fundata, 
										const int funid, 
										CParameters* result) {
	// we get the function's data
	int t = get_data(clie, proc, proc.funlen, proc.funptr, funnum[funid], 7, *fundata); 
	if(t==0) return 0;
	proc.webservice.set_value(proc.funptr, t); 

	int nconsts = 0;
	if(proc.isbyevent) 
		proc.webservice.extract(ZPARAMS, proc.fun_params, ZPARAMSLEN);
	else 
		proc.fun_params.set_value(proc.funptr, t); 

	bool donoem = proc.fun_params.extract(PNOEMPT, proc.notempty, PNOEMPTLEN) > 0;

	// some result come from the basics specially the part added for the server
	bool usebasics = proc.fun_params.isactv(ZUSEBAS, ZUSEBASLEN);

	// process the values that we have to return 
	if(proc.hasretprms = proc.webservice.has(RETPRMS, RETPRMSLEN)) {
		proc.webservice.extract(RETPRMS, proc.ret_prms, RETPRMSLEN);
		if(usebasics) {	
			replace_values(clie, proc, proc.ret_prms, clie.basics, nconsts, nullptr, nullptr); 
			if(nconsts) {
				process_constants(proc, proc.ret_prms);
				if(!proc.ret_prms.isempty()) proc.ret_prms.compact();
			}
		}
	}

	nconsts = 0;
	if(proc.workonvalues) {
		// we replace the function's variables for its real values and we get the 
		// pointer to reduce indirections, note that we dont use funptr thats because
		// it contains the data without the values within
		if(proc.fun_params.isempty()==false) { // do we have something to be replace ?
			if(usebasics) replace_values(clie, proc, proc.fun_params, clie.basics, nconsts, nullptr, nullptr);
			replace_values(clie, proc, proc.fun_params, proc.values, nconsts, 
				donoem ? &proc.notempty:nullptr, donoem ? result:nullptr);
		}
		/*
		if(proc.values.has(ZLSTDAT, ZLSTDATLEN)) {
			CString lhack;
			proc.values.get_pair(ZLSTDAT, lhack, ZLSTDATLEN);
			proc.fun_params.append(lhack);
		}
		if(proc.values.has(ZEXEDAT, ZEXEDATLEN)) {
			CString lhack;
			proc.values.get_pair(ZEXEDAT, lhack, ZEXEDATLEN);
			proc.fun_params.append(lhack);
		}
		if(proc.values.has(ZTXTDAT, ZTXTDATLEN)) {
			CString lhack;
			proc.values.get_pair(ZTXTDAT, lhack, ZTXTDATLEN);
			proc.fun_params.append(lhack);
		}*/
		if(nconsts) {
			process_constants(proc, proc.fun_params);
			if(!proc.fun_params.isempty()) proc.fun_params.compact();
		}
	}

	return t;
}

/**
 * this function reads and prepares some important data of the request that needs
 * to be before the real execution of ot for example know how to run the entire 
 * request as well as how is gonna run it this service or other
 */
void pre_execution(sClients& clie, sProcess& proc, CParameters*& fundata) {
	clie.step = 60;
	// maybe all the rights that this transaction needs, was successfuly get by the 
	// gate we must noticed that if is not a redispatch the variable rights already 
	// have the rights, so we must not need to find them, we already have them
	//if(isgate == false) clie.request.get(ZURGTSZ, proc.rights, ZURGTSZLEN);
	//else 
	//{
		int len = proc.rights.get(ZURGTSZ, proc.hstr256, ZURGTSZLEN, 255);
		proc.rights.set_value(proc.hstr256, len);
	//}

	clie.step = 65;
	// how many function we are gone to execute normally the gui clients, 
	// send events, not complete functions but exist 
	// some clients that sends a form of webservices, because they dont have
	// codebehind, so they send all the functions in the same request
	if((clie.nfuns = fundata->getint(ZZNFUNS, ZZNFUNSLEN)) == 0) {
		clie.step = 66;
		// if no functions were send, we look for them in the code behind
		if(proc.eventnamelen = fundata->get(ZEVENTN, proc.eventname, ZEVENTNLEN, 63)) 
		{
			clie.step = 67;
			if(load_function(clie, proc))
				clie.nfuns = proc.lastcbstr.getint(ZZNFUNS, ZZNFUNSLEN);	
			fundata = &proc.lastcbstr; // now our data functions come from codebehind
			proc.isbyevent = true;
		}
	}
	require(clie.nfuns > MAXFUNSPROC, _T("too_much_funs_per_process"));

	// get the variables's values, only (but not necesarly) the gui clients handle 
	// values, so direct clients like external devices dont need this kind of overhead
	if(proc.workonvalues = clie.request.has(ZVALUES, ZVALUESLEN))
		clie.request.extract(ZVALUES, proc.values, ZVALUESLEN);

	proc.sendbasics = true;
}

/**
 * executes every function in the request, is who really execute the components
 */
void execution(sClients& clie, sProcess& proc, CParameters* fundata) {
	clie.step = 70;
	// note that we dont treat the clie.nfuns == 0 as error, cause it is very
	// helpfull to test the availability of the service without doing nothing,
	// cause runing functions just to check for it is not appropiate and sane
	if(clie.nfuns == 0) clie.nfuns = 1;

	for(int funid = 0; funid < clie.nfuns; ++funid)	{
		proc.currfun = funid;

		ULONGLONG cputimestart = GetTickCount64();
		ULONGLONG* pcput = &clie.cputime[funid]; *pcput = 0;

		//TCHAR* ptcom = clie.typecom[funid];		int* ptcml = &clie.tcmlen[funid]; *(ptcom+(*ptcml=0))=0;
		//TCHAR* pcomp = clie.component[funid];	int* pcmpl = &clie.cmplen[funid]; *(pcomp+(*pcmpl=0))=0;
		TCHAR* pfunc = clie.function[funid];	int* pfunl = &clie.funlen[funid]; *(pfunc+(*pfunl=0))=0;

		CParameters& result = get_response(clie, false);

		if(!process_params(clie, proc, fundata, funid, &result)) continue;

		clie.step = 71;
		// we must check if the user has rights to rub the function
		//has_rights(clie, proc);

		// we get the type, the component and the function to execute
		//*ptcml = proc.webservice.extract(ZTYPCOM, ptcom, ZTYPCOMLEN, ZTYPCOMMAX);
		//*pcmpl = proc.webservice.extract(ZCOMPNM, pcomp, ZCOMPNMLEN, ZCOMPNMMAX); 
		*pfunl = proc.webservice.extract(ZFUNNAM, pfunc, ZFUNNAMLEN, ZFUNNAMMAX);

		// belive it or not there are function that are null
		bool nonefun =	*pfunl == 7 &&	cmp4ch(pfunc	, 'n','o','n','e') && 
										cmp4ch(pfunc + 4, 'f','u','n', 0);

		// get the result indirection for efficency reasons
		clie.step = 72;
		bool funwasok = true;

		// **** the real execution ****
		if(nonefun) result.set_value(proc.webservice);
		else
			funwasok = execute_com(	proc.fun_params, 
									pfunc, *pfunl, 
									result, proc.id, 
									&proc.newvalues); 
		clie.step = 73;

		// we add the returned parameters to the client's response
		if(proc.hasretprms) result.append(proc.ret_prms);
		// check if the result has some value to modify
		if(proc.workonvalues) {
			if(int newvalslen = proc.newvalues.GetLength())	{
				clie.step = 74;
				// if there is a new value that has to be modify here we do it
				proc.values.rpl_from(proc.newvalues); 
				// then those new values must return as part as the response
				result.append(proc.newvalues.GetBuffer(), newvalslen);
			}
		}

		// very important we check if the response is well formed
		ensure(result.is_bad_formed(), _T("response_bad_formed"));

		int loglen = 0;
		CParameters& log = proc.log[proc.nlogs];
		// we first check that if the transaction does not have a log to be saved
		if(proc.webservice.hasval(ZZTOLOG, ZZTOLOGLEN))
			loglen = proc.webservice.get(ZZTOLOG, log, ZZTOLOGLEN);
		else
			// if transaction does not have a log, we check if the function generated one
			if(result.hasval(ZZTOLOG, ZZTOLOGLEN))
				loglen = result.get(ZZTOLOG, log, ZZTOLOGLEN);
		// the normal log at this moment always save
		if(loglen) proc.savelog[proc.nlogs] = true; //proc.webservice.getint(ZSAVLOG, ZSAVLOGLEN);

		if(!funwasok) { 
			clie.step = 75;
			// get the real error description if any
			get_error_description(clie, proc, result, true);
			if(proc.savelog[proc.nlogs] = proc.webservice.getint(ZSAVERR, ZSAVERRLEN))
			{
				// we override the log if any and its type
				if(result.copyto(ZSERROR, log, ZTXTLOG, ZSERRORLEN, ZTXTLOGLEN) > (ZSERRORLEN +3))
				{
					log.set(ZTYPLOG, _T("E"), ZTYPLOGLEN, 1);
					loglen = log.get_len();
				} else loglen = 0; // if there are no text to save, we dont save it
			}
			else loglen = 0; // if there are no text to save, we dont save it
			// note: that the client's error was implicit sended into the client's response
		}

		if(loglen) {
			int dummy = 0;
			replace_values(clie, proc, log, proc.values, dummy, nullptr, nullptr);
			// we add helpfull tracking data
			//log.set(ZCOMPNM, pcomp	, ZCOMPNMLEN, *pcmpl);
			log.set(ZFUNNAM, pfunc	, ZFUNNAMLEN, *pfunl);
			++proc.nlogs;
		}

		*pcput = GetTickCount64() - cputimestart;

		if(!funwasok) break; // we cannot execute anymore if an error ocurrs
	}
}

/** 
 * this function pretty much see if wee need to return something else besides the 
 * component's response, the difference between post_execution and post_process is 
 * that post_execution really ocurrs in the execution(at least from the client point 
 * of view) and the post_process happens when the client have already receive the 
 * response
 */
void post_execution(sClients& clie, sProcess& proc, CParameters* fundata) {
	clie.step = 77;
	// this space for this extra data is arbitrary because experience has showed us 
	// that there is enough, the available funs has a limit and the list data always 
	// is small, but in theory there is a chance that the buffer would be to small, 
	// we can increse but I need a better solution
	int esize = fundata->get_len() + proc.eventnamelen + 512; //512 stuff like the [:
	TCHAR* extra = esize>4096 ? get_memory(proc, esize+1, 0):(TCHAR*)alloca(sizeof(TCHAR)*(esize+1));
	int exlen = 0;

	// we return some list info if any in the final packet
	if(fundata->has(ZLISTDT, ZLISTDTLEN)) 
		exlen += fundata->get(ZLISTDT, &extra[exlen], ZLISTDTLEN, esize - exlen);
	require((esize - exlen) <= 0, _T("wrong_memory_manage")); 

	// we must return the original function event if any
	if(proc.eventnamelen) 
		exlen += fundata->gen_pair(0, &extra[exlen], ZEVENTN, proc.eventname, ZEVENTNLEN, proc.eventnamelen);
	require((esize - exlen) <= 0, _T("wrong_memory_manage")); 

	// we must return the original transaction event if any
	if(clie.trnlen) 
		exlen += fundata->gen_pair(0, &extra[exlen], ZTRNCOD, clie.trans, ZTRNCODLEN, clie.trnlen);
	require((esize - exlen) <= 0, _T("wrong_memory_manage")); 

	// if there is an onload function the system return all functions that transaction have
	//if(fundata->has(ZAVFUNS, ZAVFUNSLEN))
	//	exlen += fundata->get_pair(&extra[exlen], ZAVFUNS, ZAVFUNSLEN, esize - exlen);
	//require((esize - exlen) <= 0, _T("wrong_memory_manage")); 

	clie.step = 78;
	// check end mark if reached here we have a full responde, not necessarly a good one
	exlen += CParameters::gen_pair(0, &extra[exlen], ZRESEND, _T("1"), ZRESENDLEN, 1);
	require((esize - exlen) <= 0, _T("wrong_memory_manage")); 

	CParameters& result = clie.restop ? clie.results[clie.restop-1] : get_response(clie, false);
	result.append(extra, exlen);

	// check if this request needs to save the result and this node is alloed to do it
	//proc.saveresult = fundata->getint(ZSAVSTA, ZSAVSTALEN);

	// we must return the key to the proxy 
	//if(clie.fromgate && proc.saveresult) result.set(ZSAVSTA, proc.saveresult, ZSAVSTALEN);
}

/**
 *	after the request was processed, there is some house keeping to be done
 *	like, close the connection, save some log if any, save the result if need it
 *	and make some garbage collection, and clean this cpu for a next running as well 
 */
void post_process(sClients& clie, sProcess& proc) {
	try	{
		// sometime one response is taken but is not used and they remain empty
		// basically we mark as they are not pending to be send on the response
		auto* pend = &clie.pendingres[0];
		auto* res = &clie.results[0];
		for(register int i=0; i<MAXFUNSPROC+2; ++i,++pend,++res) 
			if(*pend && !res->get_len()) *pend = false;

		// we notify the sender that the response is complete
		SetEvent(clie.id < 64 ? responses[clie.id] : responses2[clie.id-64]);
		// we save the actions on the log if there is any thing to save
		if(proc.nlogs) save_log(clie, proc, actionlog);			
		// we save the last result if it is requested
//		if(isgate && proc.saveresult && clie.retresult) save_result(clie, proc);
		// we clean some of the garbage that was left
		gccollect(proc, 1);
		// we acumulate the total functions executed
		gexecutedfuns += clie.nfuns;
	}
	catch(_com_error &e)	{ }
	catch(CException *e)	{ e->Delete(); }
	catch(mroerr& e)			{ }
	catch(...)						{ }
}

void finish_process(sClients& clie, sProcess& proc, const mrostatus status) {
	ResetEvent(proc.startexecuting);	// mark this funcion as ready for another client
	proc.clieid	= -1;									// proc no longer makes reference to client's slice
	proc.status	= status;							// mark this slice as free for other client
	ResetEvent(clie.procreq);					// the client does not need a proc anymore
	clie.procid	= -1;									// client no longer makes reference to proc's slice
}

void finish_request(sClients& clie) {
	close_socket(clie);
	clie.reqst	= GetTickCount64() - clie.reqst;		// we take the time and funs for statistics
	gprocesstime+= clie.reqst;						// we acumulate the global time
	clie.status	= clie.reqerror[0]==0 ? DONE:FAILED;// we mark the client as attended
}

CParameters& get_response(sClients& clie, const bool er) {
	bool* pending = &clie.pendingres[MAXFUNSPROC+1];
	CParameters* result = &clie.results[MAXFUNSPROC+1];

	if(er) {
		pending = &clie.pendingres[MAXFUNSPROC];
		result = &clie.results[MAXFUNSPROC];
	}
	else 
		if(clie.restop < MAXFUNSPROC) {
		pending = &clie.pendingres[clie.restop];
		result = &clie.results[clie.restop++];
	}

	*pending = true;
	result->clear();
	return *result;
}
/**
 *	the main purpose of this function is unpack the client's request function by 
 *	function and try to execute every function, in order to do this, it unpacks some 
 *	other data like basic data, variable values, rights access, etc.., we put this 
 *	thread with a lower	priority in order to keep the other threads to receiving 
 *	enough cpu time. 
 */
unsigned __stdcall apply_functions(LPVOID pParam) {
	int procid			= reinterpret_cast<int>(pParam);
	sProcess& proc	= procs[procid];
	proc.id					= procid;
	DWORD r					= 0;

	loop:

		if(ajst_prc[procid]) { 
			SetThreadPriority(::GetCurrentThread(), PRIO_PRC); 
			ajst_prc[procid] = false; 
		}

		SetEvent(cpus[procid]);										// we mark as a free this cpu
		r = WaitForSingleObject(proc.startexecuting, INFINITE);		// wait until this cpu is needed
		if(r != WAIT_OBJECT_0)	goto loop;
		if(proc.selfkill)		goto byecpu;
		proc.status			= SCHEDULED;							// we mark it as is ready for execution

		++gprocessesused;											// another process executed globaly)

		int clieid			= proc.clieid;							// get our index in the clients queue
		sClients& clie		= clies[clieid];						// reduce indirections
		clie.status			= EXECUTING;							// we pass the client from waiting to executing
		clie.procid			= procid;								// the client's place in process queue
		clie.restop			= 0;
		clie.resstp			= 0;
		clie.bufftop		= 0;
		memset(&clie.pendingres[0], false, sizeof(clie.pendingres));
		CParameters& er		= get_response(clie, true);

		try {
			// wait until the whole message is read
			r = WaitForSingleObject(clie.procreq, INFINITE);
			if(proc.selfkill) goto byecpu;
			proc.status	= EXECUTING;								// we mark it as it is executing

			// when the request was not read we cannot work so we close the request, no respond to
			// the client because or there is no client or simply we dont have anough information
			require(!clie.reqreaded									, _T("req_corrupt")	);	
			require(r != WAIT_OBJECT_0								, _T("req_lossed")	);

			if(!clie.retresult) close_socket(clie);

			require(clie.reqerror[0]								, clie.reqerror);		
			require(clie.reqreaded && !clie.endmark	&& clie.inlen	, _T("wrong_request"));
			require(clie.reqreaded && clie.endmark && !clie.inlen	, _T("empty_request"));

			// we identify this execution with this process slice
			clie.basics.set(ZPROCNO, procid, ZPROCNOLEN);

			clie.step = 40;
			// the gate reads the rights being a redispatch or not, why?: if it is a
			// redispatch we add the rights into the request, so the target will not 
			// look for them and if not we are gonna need them anyway for a later use
//			if(isgate) core_call(_T(""), 0, _T("lookrgt"), 7, proc.rights, proc);

			//if(clie.redispatch) {
			//	clie.step = 50;
			//	redispatch(clie, proc);
			//}
			//else {
				clie.step = 55;
				CParameters* fundata = &clie.request;
				pre_execution		(clie, proc, fundata);
				execution				(clie, proc, fundata);
				post_execution	(clie, proc, fundata);
			//}
			clie.step = 79;
		}

#define FUNEXCEPT (clie.nfuns > 0 ? clie.function[clie.nfuns-1] : _T("apply_functions"))

		catch(const CString& e) { manage_exception(er, e				, _T("CString")		,e				, FUNEXCEPT , _T("exec.cpp"), __LINE__, clie.step); }
		catch(const char* e) 	{ manage_exception(er, CString(e)		, _T("char")		,CString(e)		, FUNEXCEPT , _T("exec.cpp"), __LINE__, clie.step); }
		catch(const wchar_t* e) { manage_exception(er, e				, _T("wchar_t")		,e				, FUNEXCEPT , _T("exec.cpp"), __LINE__, clie.step); }
		catch(_com_error &e)	{ manage_exception(er, e.Description()	, _T("_com_error")	,e.Description(), FUNEXCEPT , _T("exec.cpp"), __LINE__, clie.step); }
		catch(CException *e)	{ TCHAR d[1024];  e->GetErrorMessage(d,1024); e->Delete();
														manage_exception(er, d				, _T("CException")	,d				, FUNEXCEPT , _T("exec.cpp"), __LINE__, clie.step); }
		catch(mroerr& e)			{ manage_exception(er, e.description	, e.extrainfo		,e.description	, e.function, e.errfile		, e.errline,clie.step);}
		catch(...) 						{ manage_exception(er, _T("")			, _T("...")			,_T("")			, FUNEXCEPT , _T("exec.cpp"), __LINE__, clie.step); }

		try {
			if(er.notempty()) {
				get_error_description(clie, proc, er, false);
				process_error(clie, proc, er, true, clie.step);
			}
		}
		catch(CException *e)	{ e->Delete(); }
		catch(mroerr& e)			{ }
		catch(...)						{ }

		clie.step = 80;
		post_process(clie, proc);

		clie.step = 99;
		finish_process(clie, proc, DONE);

	goto loop;

byecpu:

	{
	int clieid			= proc.clieid;							// get our index in the clients queue
	sClients& clie		= clies[clieid];	
	CParameters& result = get_response(clie, true);

	UINT thread_id;
	proc.thread = (HANDLE)_beginthreadex(0, 0, apply_functions, 
								reinterpret_cast<void*>(procid), 0, &thread_id);

	int funid = proc.currfun;
	//TCHAR typecomp[ZTYPCOMMAX+1];
	//TCHAR component[ZCOMPNMMAX+1]; 
	int complen = 0;
	//_tmemcpy(component, clie.component[funid], (complen = clie.cmplen[funid]) + 1);
	//_tmemcpy(typecomp, clie.typecom[funid], clie.tcmlen[funid]+1);
	if(funid != -1)	{
		execute_com(result, _T("reset_state"), 11, result, procid, &proc.newvalues); 
	}

	// maybe the slice was executing a sql query so for robustness we close the connection for latter reuse
	dbhelper::initialize_stack(procid);
	}
	return 0;
}