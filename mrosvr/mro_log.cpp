#include "stdafx.h"

/************************************************************************************
* description   : mro_log
* purpose       : deals with the functions for managing the log
* author        : miguel rodriguez
* date creation : 20 junio 2004
* change        : 1 marzo call coms direct from the function execute instead of CMroCom::call_com)
* change        : 10 febrero  change to global queue array, and the access controled by semaphores
**************************************************************************************/

#include "mrosvr.h"

slog* actionlog;
//slog* errorlog;

void initialize_log(slog*& plog)
{
	plog = new slog[MAX_PROCS];
	for(int i=0; i<MAX_PROCS; ++i)
	{
		slog& alog	= plog[i];
		alog.final_command[alog.pos	= 0] = 0;
		for(int j=0; j<MAXLOGQUEUE; ++j)
		{
			slogimp& logimp	= alog.thequeue[j];
			logimp.status	= FREE;
			logimp.params[0]= _T('\0');
			logimp.prmslen	= 0;
			logimp.saveit	= false;		
		}
	}
}

void pass_to_db(slog& alog, const int procid)
{
	// check if this service have sql support
	if(dbhelper::dbgbl)
	{
		dbhelper dbmanagercon(procid);
		cConnection& con = dbmanagercon.get_db_from_gbl_manager();
		try
		{
			if(alog.final_command[0])
				con.execute(alog.final_command);
		}
		catch(_com_error &e)	{	con.Close();					}
		catch(CException *e)	{ 	con.Close();	e->Delete();	}
		catch(mroerr& e)		{	con.Close();					}
		catch(...)				{	con.Close();					}
	}
}

void flush_logs(slog* plog, const int procid)
{
	slog& alog = plog[procid];
	if(alog.pos == 0) return;

	CParameters params;
	TCHAR cmpy[3 + 1];			int lcpy = 0;
	TCHAR trans[64 + 1];		int ltrn = 0;
	TCHAR user[ZUSERIDMAX + 1];	int lusr = 0;
	TCHAR mach[16 + 1];			int lmac = 0;
	TCHAR txt[128 + 1];			int ltxt = 0;
	TCHAR key[64 + 1];			int lkey = 0;
	TCHAR typ[1 + 1];			int ltyp = 0;

	try
	{
		TCHAR* p = alog.final_command;
		*p = 0;
		int fsize = (sizeof(alog.final_command)/sizeof(TCHAR)-1);
		int len = 0;
		TCHAR k[32]; int kl=0;

		for(int j=0; j<MAXLOGQUEUE; ++j)
		{
			slogimp& logimp = alog.thequeue[j];

			// only the data that is waiting to be saved
			if(logimp.status == WAITING) 
			{
				// not every thing goes to the database
				if(logimp.saveit && logimp.params[0]) 
				{
					params.set_value(logimp.params, logimp.prmslen);

					lcpy = params.get(ZCOMPNY, cmpy  , ZCOMPNYLEN, 3);
					ltrn = params.get(ZTRNCOD, trans , ZTRNCODLEN, 64);
					lusr = params.get(ZUSERID, user	 , ZUSERIDLEN, ZUSERIDMAX);
					lmac = params.get(ZMACNAM, mach	 , ZMACNAMLEN, 32);

					int nlogs =- 1;
					bool alone = false;
					if(params.has(ZZNLOGS, ZZNLOGSLEN)) nlogs = params.getint(ZZNLOGS,ZZNLOGSLEN);
					if(nlogs == -1 || nlogs == 0) { alone = true; nlogs = 1; }
					for(int i=0; i< nlogs; ++i)
					{
						if(i ==0 && alone)
						{
							ltxt=params.get(ZTXTLOG, txt, ZTXTLOGLEN, 128);
							lkey=params.get(ZKEYLOG, key, ZKEYLOGLEN, 64);
							ltyp=params.get(ZTYPLOG, typ, ZTYPLOGLEN, 1);
						}
						else
						{
							kl=mikefmt(k,_T("%s%d"), ZTXTLOG, i); ltxt=params.get(k, txt, kl, 128);
							kl=mikefmt(k,_T("%s%d"), ZKEYLOG, i); lkey=params.get(k, key, kl, 64);
							kl=mikefmt(k,_T("%s%d"), ZTXTLOG, i); ltyp=params.get(k, typ, kl, 1);
						}

						int rsize = (32 +(lusr+2)+(lmac+2)+(ltrn+2)+(3+2)+(ltxt+2)+(lkey+2)+(ltyp+2));
						if(rsize > fsize) continue;	// too big even for the whole log
						if(rsize+len > fsize) // the remain space is to short, we flush it and release it
						{
							pass_to_db(alog, procid);
							len = 0;
							p = alog.final_command;
							*p = 0;
						}
						int lon = mikefmt(p, _T("exec insert_log %s,'%s','%s','%s','XXX','%s','%s','%s'; "), 
												cmpy, user, mach, trans, txt, key, typ);
						len += lon;
						p += lon;
						require(len >= fsize, _T("memory_overrun"));
					}
				}
				logimp.status = DONE;
			}
		}
		alog.pos = 0;
	}
	catch(CException *e)	{ 	alog.pos = 0;	e->Delete();	return; }
	catch(mroerr& e)		{	alog.pos = 0;					return; }
	catch(...)				{	alog.pos = 0;					return; }

	pass_to_db(alog, procid);
}

void save_log(sClients& clie, sProcess& proc, slog* plog)
{
	if(!clie.nfuns) return;				// why bother
	slog& alog = plog[proc.id];
	if(alog.pos == MAXLOGQUEUE) return; // no more room

	TCHAR basdata[256];
	// we need the basic data for saving into the db
	int datlen = mikefmt(basdata, _T("[%s:%s][%s:%s][%s:%s]"), 
									ZMACNAM , clie.machine,
									ZTRNCOD	, clie.trans,
									ZUSERID	, clie.user);
	if(datlen >= (sizeof(basdata)/sizeof(TCHAR))-1) return;

	// we process any any meanwhile there is a room
	for(int i=0; i<proc.nlogs && alog.pos<MAXLOGQUEUE; ++i)	
	{
		// we take a valid entry for the queue log
		slogimp& ai = alog.thequeue[alog.pos];
		// we get the logdata
		TCHAR* logdata = proc.log[i].buffer();
		int loglen = proc.log[i].get_len();
		if(!loglen  || !logdata[0]) continue;

		int totlen = loglen + datlen;
		if(totlen >= (sizeof(ai.params)/sizeof(TCHAR))-1) continue;

		// we form the params to be used on saving into db
		TCHAR* p = ai.params;
		_tmemcpy(p, logdata	, loglen);		p += loglen;
		_tmemcpy(p, basdata	, datlen+1);	p += datlen; // +1 to include endmark
		ai.prmslen = totlen; 

		// last details 
		ai.status = WAITING;
		ai.saveit = proc.savelog[i];
		++alog.pos;
	}
}

int get_list_log(slog* plog, TCHAR* lista, const int lid, const int buffsize)
{
	CParameters basparams;

	UINT len	= 0;
	TCHAR* p	= lista;
	int row		= 0;
	int lon		= 0;
	
	TCHAR mach[32];
	TCHAR user[ZUSERIDMAX + 1];
	TCHAR trans[64];
	TCHAR comp[64];
	TCHAR fun[64];
	TCHAR text[128];

	for(int i = 0; i < MAX_PROCS; ++i)
	{
		slog& elog = plog[i];
		for(int iter = 0; iter < MAXLOGQUEUE; ++iter)
		{
			slogimp& errorlogimp = elog.thequeue[iter];
			basparams.set_value(errorlogimp.params, errorlogimp.prmslen);
			basparams.get(ZMACNAM, mach	, ZMACNAMLEN, 31);
			basparams.get(ZUSERID, user	, ZUSERIDLEN, ZUSERIDMAX);
			basparams.get(ZTRNCOD, trans, ZTRNCODLEN, 63); 
			basparams.get(ZCOMPNM, comp	, ZCOMPNMLEN, 63);
			basparams.get(ZFUNNAM, fun	, ZFUNNAMLEN, 63);
			basparams.get(ZTXTLOG, text	, ZTXTLOGLEN, 127);

			lon = mikefmt(p,_T("[l%d%dA:%ld][l%d%dB:%s][l%d%dC:%s][l%d%dD:%s][l%d%dE:%s]")
							_T("[l%d%dF:%s][l%d%dG:%s][l%d%dH:%s][l%d%d*:%d]"),
							lid, row, iter, 
							lid, row, statdescs[errorlogimp.status], 
							lid, row, mach,
							lid, row, trans,
							lid, row, user,
							lid, row, text,
							lid, row, comp,
							lid, row, fun,
							lid, row, errorlogimp.status);
			++row;
			len += lon;
			p+=lon;
			require(len >= buffsize, _T("memory_overrun"));
		}
	}
	require(len >= buffsize, _T("memory_overrun"));
	return len += gen_tot_list(p,lid,row);
}
