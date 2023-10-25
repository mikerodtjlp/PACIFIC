#include "stdafx.h"

//#include "mroctrl.h"
#include "SessionMan.h"
#include "mrofns.h"
#include "mro_com.h"

/**
 * this functions reads, parse and poolish the codebehind data either old
 * style or json style keep it on a cache for performance boost
 */
int generate_action(scmdimp* cachefuns, const eventcmd* cmd, const bool parallel, int& nfuns, 
						CParameters& command, eventcmd* maincmd, 
						TCHAR* willafectlist, int& listlen);

static std::map<CString, CParameters> cachefunctions;
static std::map<CString, CParameters>::iterator endcachefuns;
static CRITICAL_SECTION csfuns;

static std::map<CString, scmdimp> cachecmds;
static std::map<CString, scmdimp>::iterator endcachecmds;

void initialize_funs_dll()
{
	endcachefuns	= cachefunctions.end();
	::InitializeCriticalSection(&csfuns);
	endcachecmds	= cachecmds.end();
}

/**
 * from the event name like accept, save, load, etc.. or from user
 * defined functions this function obtaines its' id 
 */
eventcmd* get_action(scmdimp* cache, const TCHAR* action, const int actlen)
{
	int nfuns = cache->cmdmax;
	eventcmd* cmd = &cache->mcmd[0];
	for(register int i = 0; i < nfuns; ++i,++cmd)
	{
		if(actlen != cmd->namelen) continue;
		if(cfuns[actlen](cmd->name, action)) return cmd;
	}
	return nullptr;
}

/**
 *	description:
 *	this functions generates the very function(command) string; for example
 *	[fun01:[somekey:someval][otherkey:othervalue][zztolog:[tolog:...]..]
 *  [retprms:...]... etc...
 *	and put it on the vector command
 */
int get_command(const eventcmd* cmd, const bool parallel, int& nfuns, 
				vector<CParameters>& final_command, 
				eventcmd* father, TCHAR* willafectlist, int& listlen)
{
	if(!cmd->funl) return 0; // empty functions must gone away

	eventcmd* cmdpp = const_cast<eventcmd*>(cmd);

	int cmdlen = cmdpp->params.get_len();
	int extlen = cmdpp->extprms.get_len();
	int retlen = cmdpp->retprms.get_len();

	// because we use most of the fields to conactenate we use the 
	// max = sizeof(eventcmd) and some extra space for safety
	int size = cmdlen + extlen + retlen + sizeof(eventcmd) + 2048; 
	bool dynamic = size > 4096;
	TCHAR* aux = dynamic ?	(TCHAR*)malloc(sizeof(TCHAR)*(size + 1)) : 
							(TCHAR*)alloca(sizeof(TCHAR)*(size + 1));
	TCHAR* p = aux;

	_tmemcpy(p, _T("[zfun0Xz:[zparams:"), 18);		p += 18;
	_tmemcpy(p, cmdpp->params.buffer()	, cmdlen);	p += cmdlen;
	_tmemcpy(p, cmdpp->extprms.buffer()	, extlen);	p += extlen;
	*p= cpairs::RIGHT; ++p;

	if(retlen) 
		p += cpairs::gen_pair(0, p, RETPRMS, cmdpp->retprms.buffer(), 
									RETPRMSLEN, retlen);

	if (cmd->modul) p += cpairs::gen_pair(0, p, ZWEBSIT, cmd->modu, ZWEBSITLEN, cmd->modul);
	if (cmd->compl) p += cpairs::gen_pair(0, p, ZCOMPNM, cmd->comp, ZCOMPNMLEN, cmd->compl);
	if (cmd->tcoml) p += cpairs::gen_pair(0, p, ZTYPCOM, cmd->tcom, ZTYPCOMLEN, cmd->tcoml);
	if (cmd->funl)  p += cpairs::gen_pair(0, p, ZFUNNAM, cmd->fun , ZFUNNAMLEN, cmd->funl);
	if (cmd->statl)p += cpairs::gen_pair(0, p, ZSTATUS, cmd->status, ZSTATUSLEN, cmd->statl);

	if(!cmdpp->history.isempty())//if(cmd->logtl) // form the log data if any (both fiels are required)
	{
		TCHAR logtxt[LGTSIZE];
		TCHAR logkey[LGKSIZE];
		TCHAR logtyp[LGPSIZE];
		int logtl = cmdpp->history.get(_T("log"), logtxt, 3, LGTSIZE - 1);
		int logkl = cmdpp->history.get(_T("key"), logkey, 3, LGKSIZE - 1);
		cmdpp->history.get(_T("type"), logtyp, 4, LGPSIZE - 1);

		if(logtxt[0] == cpairs::LEFT) p += cpairs::gen_pair(0, p, ZZTOLOG, logtxt, ZZTOLOGLEN, logtl);
		else
		if(logkl)
		{
			*p++ = cpairs::LEFT;
			_tmemcpy(p, ZZTOLOG, ZZTOLOGLEN); p += ZZTOLOGLEN;
			*p++ = cpairs::SEP;
			p +=	cpairs::gen_pair(0, p, ZTXTLOG, logtxt,ZTXTLOGLEN, logtl); 
			p +=	cpairs::gen_pair(0, p, ZKEYLOG, logkey,ZKEYLOGLEN, logkl); 
			p +=	cpairs::gen_pair(0, p, ZTYPLOG, logtyp,ZTYPLOGLEN, 1);
			*p++ = cpairs::RIGHT;
		}
	}

	// mark that the error must be saved
	if(cmd->saveerror) 
	{ 
		set4ch(p, cpairs::LEFT,'z','s','a');			p += 4;
		set4ch(p, 'v','e','r','r');						p += 4;
		set4ch(p, cpairs::SEP,'1', cpairs::RIGHT, 0 );	p += 3;
	}

	// will some list be affected by the result?
	if(cmdpp->params.has(ZLISTAF,ZLISTAFLEN))
	{
		int lista = cmdpp->params.getint(ZLISTAF, ZLISTAFLEN);
		int lid = (listlen/8);
		require(lid > 4, _T("too_much_lists"));
		listlen += mikefmt(&willafectlist[listlen], _T("%czla%d%c%d%c"), 
							cpairs::LEFT, lid, cpairs::SEP, lista, cpairs::RIGHT);
	}

	// add the rights if are requested
	if(cmd->seclen)
		p +=  cpairs::gen_pair(0, p, ZRIGHT1, cmd->security, ZRIGHT1LEN, cmd->seclen);
	// add the notification if are requested
	//if(cmd->notifl)
	//	p +=  cpairs::gen_pair(0, p, ZRIGHT1, cmd->notif, ZRIGHT1LEN, cmd->notifl);
	if(parallel)
	{
		set4ch(p, cpairs::LEFT, 'z', 'p', 'a');			p += 4;
		set4ch(p, 'r', 'a', 'l', 'l');					p += 4;
		set4ch(p, cpairs::SEP, '1', cpairs::RIGHT, 0);	p += 3;
	}

	// mark then of the full command
	*p++ = cpairs::RIGHT;

	// we adjust the number of the function
	//mikefmt(aux + 5, _T("%02d"), nfuns++);
	//aux[7] = _T('z');
	if(nfuns < 10) aux[6] = (TCHAR)(48+nfuns);
	else { mikefmt(aux + 5, _T("%02d"), nfuns);	aux[7] = _T('z');}
	++nfuns;

	int finallen = p - aux;
	CParameters topush(aux, finallen); 
	final_command.push_back(topush);

	if(dynamic) free(aux);

	return finallen;
}

/**
 *	description:
 *	this function process the before and afters functions (1:bef,2:aft,3:par)
 */
int apply_functions(scmdimp* cachefuns, const eventcmd* cmd, int& nfuns, 
					vector<CParameters>& final_command, 
					const int type, eventcmd* maincmd, TCHAR* willafectlist, 
					int& listlen)
{
	int cmdlen = 0;
	TCHAR fun[64];
	int lfun = 0;
	cpairs otheraction;

	cMroList* l = nullptr;
	if (type == 1) l = const_cast<cMroList*>(&cmd->beforefuns);
	else if (type == 2) l = const_cast<cMroList*>(&cmd->afterfuns);
	else if (type == 3) l = const_cast<cMroList*>(&cmd->parallelfuns);

	for(l->begin(); l->end() == false; l->next())
	{
		if((lfun = l->get(fun)) == 0) continue;

		eventcmd* thiscmd = get_action(cachefuns, fun, lfun);
		if(!thiscmd) continue;

		otheraction.clear();
		cmdlen += generate_action(cachefuns, thiscmd, type==3, nfuns, otheraction, 
									maincmd, willafectlist, listlen);
		final_command.push_back(otheraction);
	}

	return cmdlen;
}

/**
 *	description:
 *	this function process the function, finding out the function to execute from 
 *  command, event or name
 */
int generate_action(	scmdimp* cachefuns, const eventcmd* cmd, const bool parallel, int& nfuns, 
						cpairs& command, 
						eventcmd* maincmd, TCHAR* willaflist, int& listlen)
{
	vector<CParameters> commands;
	int cmdlen = 0;

	if(cmd->beforefuns.get_len())
		cmdlen += apply_functions	(cachefuns, cmd, nfuns, commands, 1,
									maincmd, willaflist, listlen);	// befores functions

	cmdlen += get_command			(cmd, parallel, nfuns, commands,
									maincmd, willaflist, listlen);	// the main function
	if (cmd->parallelfuns.get_len())
		cmdlen += apply_functions(cachefuns, cmd, nfuns, commands, 3,
									maincmd, willaflist, listlen);	// parallels functions

	if(cmd->afterfuns.get_len())
		cmdlen += apply_functions	(cachefuns, cmd, nfuns, commands, 2,
									maincmd, willaflist, listlen);	// afters functions

	bool dynamic = cmdlen > 4096;
	TCHAR* p = dynamic ?	(TCHAR*)malloc(sizeof(TCHAR)*(cmdlen + 1)) : 
							(TCHAR*)alloca(sizeof(TCHAR)*(cmdlen + 1));
	TCHAR* q = p;
	auto end = commands.end();
	for(auto iter = commands.begin(); iter != end; ++iter)
	{
		CParameters* pstr = &(*iter);
		int len = pstr->get_len();
		_tmemcpy(q, pstr->buffer(), len);
		q += len;
	}
	*q = '\0';

	command.set_value(p, cmdlen);
	if(dynamic) free(p);
	return cmdlen;
}

enum gtokens {		after, 
					before, 
					command, code, com,
					ext_prm, 
					fun,
					his,
					log_txt, /*log_ext,*/ log_key, log_type, 
					module,
					notification,
					over_ride, 
					parallel,parameters,
					retprms, 
					saveerror, save_state, security, 
					status,
					type_mod, 
					webservice,
					bad_tok
};

TCHAR* tokens[] = { _T("after"), 
					_T("before"), 
					_T("cmd"), _T("codebehind"), _T("com"), 
					_T("ext_prm"), 
					_T("fun"),
					_T("history"),
					_T("log"), /*_T("log_ext"),*/ _T("log_key"), _T("log_type"),
					_T("module"),
					_T("notification"),
					_T("override"),
					_T("parallel"),_T("parameters"),
					_T("retprms"), 
					_T("save_error"), _T("save_state"), _T("security"), 
					_T("status"),
					_T("type_module"),
					_T("webservice")
};

const int maxtokens = gtokens::bad_tok;

int compare( TCHAR **arg1, TCHAR **arg2 )
{
	return _tcscmp(*arg1, *arg2);
}
gtokens find_token(const TCHAR* key)
{
	TCHAR **result;
	result = (TCHAR **)bsearch((TCHAR *) &key, (TCHAR*)&tokens, maxtokens,
				sizeof( TCHAR * ), (int (*)(const void*, const void*))compare);
	if(result == 0 || *result == 0) return bad_tok;
	gtokens ret = static_cast<gtokens>(&(*result) - &(tokens[0]));
	return ret;
}

#define defxmlreader(child, s, attrlen, vallen)\
	TCHAR attrs[s][attrlen];\
	TCHAR vals[s][vallen];\
	int alns[s];\
	int lens[s];\
	int total = child->get_attrs_vals(&attrs, alns, &vals, lens, s, attrlen, vallen);

/**
 * description      : reads one function from the xml file 
 * author           : miguel rodriguez ojeda
 * date             : june 20 2004
 *
 * modification     : november 20 2004; apply done[? to the if conditions and 
 *						apply continue in those conditions
 *
 */
eventcmd* read_action(eventcmd* cmd, const int total, 
								const TCHAR* attrs, const int* alns,
								const TCHAR* vals, const int* lens, 
								const int* posk, const int* posv, 
								const int maxattrlen, 
								const int maxvallen)
{
	for(int i=total-1; i>=0; --i)
	{
		const TCHAR* att = &attrs[posk[i]];
		const int aln = alns[i];
		const TCHAR* val = &vals[posv[i]];
		const int len = lens[i];

		requireex(aln >= maxattrlen, _T("json_attr_long"), CString(att));
		requireex(len >= maxvallen, _T("json_value_long"), CString(att));

		switch(find_token(att))
		{
		case fun:
		case webservice:
		case over_ride:		requireex(len > FUNMAX, _T("val_2_long"), _T("fun"));
							if((cmd->funl=len)>0) 
							{ _tmemcpy(cmd->fun, val,len); cmd->fun[len] = 0;}		
							continue; 
		case command:
		case parameters:	requireex(len > maxvallen, _T("val_2_long"), _T("cmd"));
							if(len>0) cmd->params.set_value(val,len);				
							continue;   
		case com:
		case code:			requireex(len > CMPMAX, _T("val_2_long"), _T("cmp"));
							if((cmd->compl=len)>0) 
							{ _tmemcpy(cmd->comp, val,len); cmd->comp[len] = 0;}		
							continue; 
		case type_mod:		requireex(len!=0 && len!= TYCMAX, _T("val_wrong"), _T("tcm"));
							if((cmd->tcoml=len)>0) 
							{ _tmemcpy(cmd->tcom, val,len); cmd->tcom[len] = 0;}		
							continue;
		case security:		requireex(len!=0 && len!= SECMAX, _T("val_wrong"), _T("sec"));
							if((cmd->seclen=len)>0) 
							{ _tmemcpy(cmd->security, val,len); cmd->security[len] = 0;} 
							continue; 
		case before:		if(len>0) cmd->beforefuns.set_value(val,len);
							continue; 
		case after:			if(len>0) cmd->afterfuns.set_value(val,len);
							continue; 
		case save_state:	if(len>0) cmd->savestate = _tstoi(val);					
							continue; 

		case his:			requireex(len > maxvallen, _T("val_2_long"), _T("his"));
							if (len>0) cmd->history.set_value(val, len);
							continue;
		case log_txt:		requireex(len > LGTMAX, _T("val_2_long"), _T("logact"));
							if(len>0) cmd->history.set(_T("log"),val,3,len);
							continue; 
		case log_key:		requireex(len > LGKMAX, _T("val_2_long"), _T("logkey"));
							if (len>0) cmd->history.set(_T("key"), val, 3, len);
							continue; 
		case log_type:		requireex(len!=0 && len!=LGPMAX, _T("val_wrong"), _T("logtyp"));
							if (len>0) cmd->history.set(_T("type"), val, 4, len);
							continue;

		case module:		requireex(len > MODMAX, _T("val_2_long"), _T("mod"));
							if((cmd->modul=len)>0) 
							{ _tmemcpy(cmd->modu, val,len); cmd->modu[len] = 0;}		
							continue; 
		case retprms:		if(len>0) cmd->retprms.set_value(val,len);				
							continue;
		case parallel:		if(len>0) cmd->parallelfuns.set_value(val,len);
							continue;
		case ext_prm:		if(len>0) cmd->extprms.set_value(val,len);				
							continue; 
		case saveerror:		if(len>0) cmd->saveerror = _tstoi(val);					
							continue; 
		//case notification:	requireex(len!=0 && len!= LGTMAX, _T("val_wrong"), _T("notif"));
		//					if((cmd->notifl=len)>0) 
		//					{ _tmemcpy(cmd->notif, val, len); cmd->notif[len] = 0;} 
		//					continue; 
		case status:			requireex(len > 32, _T("val_2_long"), _T("onok"));
							if ((cmd->statl = len) > 0)
							{ _tmemcpy(cmd->status, val, len); cmd->status[len] = 0;}
		}
	}

	return cmd;
}

int pass_to_arrays(	CParameters& attributes, 
					TCHAR* attrs, int* alns,
					TCHAR* vals, int* lens, 
					int* posk, int* posv, const int max, 
					const int attrlen, const int vallen)
{
	int nks = attributes.nkeys();
	cpairs pair;
	int kp = 0;
	int vp = 0;
	int ck = 0;
	for(register int i=0; i<nks; ++i)
	{
		posk[ck] = kp;
		posv[ck] = vp;

		TCHAR* key = &attrs[kp];
		TCHAR* val = &vals[vp];

		int nkl = 0;
		int nvl = 0;
		if(attributes.getpair(i, key, val, nkl, nvl, attrlen, vallen))
		{
			kp += nkl+1;
			vp += nvl+1;
			lens[ck] = nvl;
			alns[ck] = nkl;
			++ck;
		}
	}
	return ck;
}

const int MAXENTRIES = 22;//32; // number of possible attributes
const int MAXVALENTRY = 8192+2048;
const int MAXATTENTRY = 22;//32;

void CSessionMan::read_js(	const TCHAR* data, scmdimp* newstruct, 
							//const bool isonloadfun, TCHAR* funlist, int& lenfunlist, 
							const int procid)
{
	int size = sizeof(TCHAR)*(MAXENTRIES*MAXVALENTRY);
	TCHAR* vals = nullptr;
	mro::memhelper::get_mem_from_gbl_manager(&vals, size, procid,1);

	TCHAR attrs[MAXENTRIES][MAXATTENTRY];
	int alns[MAXENTRIES];
	int vlns[MAXENTRIES];
	int posk[MAXENTRIES];
	int posv[MAXENTRIES];
	int nattrs = 0;

	if(const TCHAR* q = _tcsstr(data, _T("<script")))
	{
		const TCHAR* p = nullptr;
		if((p = _tcsstr(q+7, _T("lprms=\""))) || (p = _tcsstr(q+7, _T("lprms='"))))
		{
			if(int len = get_value_len(p+7))
			{
				cpairs fun;
				cpairs attributes;
				cpairs jscript;
				jscript.set_from_json(p+7,len); 
				jscript.optimize();

				int nfuns = jscript.nkeys();
				for(int i=0; i<nfuns; ++i)
				{
					if(newstruct->cmdmax >= MAX_FUNS_PER_CODEBEHIND)
					{
						CString errinf;
						errinf.Format(_T("max:%d; used:%d"), MAX_FUNS_PER_CODEBEHIND, newstruct->cmdmax);
						requireex(true, _T("too_much_funs_per_code"), errinf);
					}

					if(!jscript.getpair(i, fun)) continue;
					if(fun.isempty()) continue;
					fun.optimize();

					eventcmd* cmd = &newstruct->mcmd[newstruct->cmdmax];
					cmd->init();
					int lenname = cmd->namelen = jscript.getkey(i, cmd->name, EVNMAX);

		// super parche temporal, la informacion que no sea funcion no debe ser tratada como tal
		// cuando se elimine el viejo metodo de edist y adds, se puede eliminar este parche
        if(lenname==10){
			TCHAR* q=cmd->name;
			if(	((cmp4ch(q,'i','m','g','f') && cmp4ch(&q[4],'i','e','l','d') && q[8]=='s')) ||
				((cmp4ch(q,'e','d','i','f') && cmp4ch(&q[4],'i','e','l','d') && q[8]=='s')) ||
				((cmp4ch(q,'a','d','d','f') && cmp4ch(&q[4],'i','e','l','d') && q[8]=='s')))
				continue;
		}

					require(_tmemchr(cmd->name, '=', lenname), CString(cmd->name));
					fun.get(cmd->name, attributes, lenname);
					attributes.optimize();
					nattrs = pass_to_arrays(attributes, (TCHAR*)attrs, alns, (TCHAR*)vals, vlns, posk, posv, 
																MAXENTRIES, MAXATTENTRY, MAXVALENTRY);

					cmd = read_action(cmd, nattrs, (TCHAR*)attrs, alns, (TCHAR*)vals, vlns, posk, posv, 
																		MAXATTENTRY, MAXVALENTRY);
					if(cmd && cmd->funl)
					{
						//if(isonloadfun) 
						//{
						//	funlist[lenfunlist++] = cpairs::LEFT;
						//	_tmemcpy(&funlist[lenfunlist], cmd->name, cmd->namelen); lenfunlist += cmd->namelen;
						//	set4ch(&funlist[lenfunlist], cpairs::SEP, '1', cpairs::RIGHT, 0); lenfunlist += 3;
						//}
						++newstruct->cmdmax;
					}
				}
			}
		}
	}
}

void CSessionMan::read_cb(	const TCHAR* data, scmdimp* newstruct, 
							//const bool isonloadfun, TCHAR* funlist, int& lenfunlist, 
							const int procid)
{
	int size = sizeof(TCHAR)*(MAXENTRIES*MAXVALENTRY);
	TCHAR* vals = nullptr;
	mro::memhelper::get_mem_from_gbl_manager(&vals, size, procid,1);

	TCHAR attrs[MAXENTRIES][MAXATTENTRY];
	int alns[MAXENTRIES];
	int vlns[MAXENTRIES];
	int posk[MAXENTRIES];
	int posv[MAXENTRIES];
	int nattrs = 0;
	
	CXmlDocument doc;
	require(!doc.Parse(data), _T("bad_parsing"));
	if(CXmlElement* root = doc.GetRootElement()) 
	{
		if(CXmlElement* fun = doc.GetFirstChild(root))
		{
			for(;fun != nullptr;)
			{
				if(newstruct->cmdmax >= MAX_FUNS_PER_CODEBEHIND)
				{
					CString errinf;
					errinf.Format(_T("max:%d; used:%d"), MAX_FUNS_PER_CODEBEHIND, newstruct->cmdmax);
					requireex(true, _T("too_much_funs_per_code"), errinf);
				}

				eventcmd* cmd = &newstruct->mcmd[newstruct->cmdmax++];
				cmd->init();
				int lenname = cmd->namelen = fun->get_label(cmd->name, EVNMAX);
				require(_tmemchr(cmd->name, '=', lenname), CString(cmd->name));
				nattrs = fun->get_attrs_vals(attrs, alns, vals, vlns, posk, posv, 
											MAXENTRIES, MAXATTENTRY, MAXVALENTRY);

				cmd = read_action(cmd, nattrs, (TCHAR*)attrs, alns, (TCHAR*)vals, vlns, posk, posv, 
																	MAXATTENTRY, MAXVALENTRY);
				//if(isonloadfun) 
				//{
				//	funlist[lenfunlist++] = cpairs::LEFT;
				//	_tmemcpy(&funlist[lenfunlist], cmd->name, cmd->namelen); lenfunlist += cmd->namelen;
				//	set4ch(&funlist[lenfunlist], cpairs::SEP, '1', cpairs::RIGHT, 0); lenfunlist += 3;
				//}
				fun = doc.GetNextSibling(root);
			}
		}
	}
}

/**
 * this function is pretty important it generate the functionallity of the system
 * basically it process the orchestraition level there are 4 leves for create the 
 * function strings (1) the system first check if it is exist already the function 
 * strings if not then check on the (2)command structure cache and if not then 
 * check if we have the (3)html file on the cache if not then look it from the 
 * disk(4) (local(gate service) or remote(to gate service)) when we got something 
 * either on cache or looking it, we process what we got a create the data for the 
 * previous level in order to fill the appropiate caches
 */
void CSessionMan::get_final_fun() {
	TCHAR funname[FUNSIZE];	auto funlen = _params.get(_T("fun2find"), funname	, 8, FUNMAX);
	TCHAR document[CMPSIZE];auto doclen = _params.get(_T("document"), document	, 8, CMPMAX);
	//TCHAR library[16];      auto liblen = _params.get(_T("library"), library, 7, 15);

	TCHAR byforza[8];
	int l = _params.get(ZTYPRED, byforza, ZTYPREDLEN, 7);
	bool byforce = l == 5 && cmp4ch(byforza, 'f','o','r','c') && cmp2ch(&byforza[4], 'e', 0);

	if(!doclen) return;

	require(!funlen, _T("function_missing_on_cb"));

	CString func2find(document, doclen);
	func2find.Append(funname, funlen);

	// this variable must come in the same scope as realfunstring and realfunstringlen 
	// otherwise will be out of sync when one would be out of scope cause one have 
	// a reference to the other
	CParameters functions;			
	TCHAR* realfunstring = nullptr;
	int realfunstringlen = 0;
	bool readfromcmds = false;
	//const int MAXFUNLISTLEN  = 2048;
	//TCHAR funlist[MAXFUNLISTLEN];
	//int lenfunlist = 0;
	scmdimp* cachefuns = 0;

	// we first look in the functions's cache
	::EnterCriticalSection(&csfuns);
	try	{
		if(byforce) {
			cachefunctions.erase(func2find);
			readfromcmds = true;
		}
		else {
			auto iterfun = cachefunctions.lower_bound(func2find);
			if(iterfun != endcachefuns && !(cachefunctions.key_comp()(func2find, iterfun->first))) {
				realfunstring = (*iterfun).second.buffer(); // if it is on the cache every thing is easy
				realfunstringlen = (*iterfun).second.get_len();
			}
			else readfromcmds = true;
		}

		if(readfromcmds) {
			auto readfromdb = false;

			// the real name of the code behind
			CString doc(document, doclen);

			auto isstdfun = false;
			// we first look in the cmd's cache
			if(byforce) {
				cachecmds.erase(doc);
				readfromdb = true;
			}
			else {
				auto iter = cachecmds.lower_bound(doc);
				if(iter != endcachecmds && !(cachecmds.key_comp()(doc, iter->first)))
					cachefuns = &(*iter).second; // if it is on the cache every thing is easy
				else readfromdb = true;
			}

			if(readfromdb) {
				int procid =_basics.getint(ZPROCNO, ZPROCNOLEN);
				bool isjavascript = true;
				CString data;
				get_file(/*library, liblen,*/ doc.GetBuffer(), doc.GetLength(), _T("TRN"), 3, data,
					_T("trans"), 5, byforce);
				if(data.Find(_T("module=")) == -1) {
					isjavascript = false;
					doc.Append(_T("_cb"),3);
					data.Empty();
					get_file(/*library, liblen,*/ doc.GetBuffer(), doc.GetLength(), _T("TRN"), 3, data,
						_T("trans"), 5, byforce);
				}
				requireex(!data.GetLength(), _T("empty_file"), doc);

				// we create all the cmds from the xml file
				bool isonload	= funlen == 6 && funname[6] == 0 &&	cmp4ch(&funname[0],'o','n','l','o') && 
																	cmp2ch(&funname[4],'a','d');
				bool isonunload	= funlen == 8 && funname[8] == 0 &&	cmp4ch(&funname[0],'o','n','u','n') && 
																	cmp4ch(&funname[4],'l','o','a','d');
				isstdfun = isonload | isonunload;

				// start collecting the functions availables if any and only if is it the onload function
				//if(isonload) lenfunlist += mikefmt(&funlist[lenfunlist], _T("%czavfuns%c"),
				//												cpairs::LEFT, cpairs::SEP);

				// prepare the memory to use
				scmdimp* n = nullptr;
				mro::memhelper::get_mem_from_gbl_manager((TCHAR**)&n, sizeof(scmdimp), procid, 0);
				scmdimp* newstruct = new((void*)n)scmdimp;
				newstruct->cmdmax = 0;

				// process the commands and functions
				if(isjavascript) 
					read_js(data.GetBuffer(), newstruct, /*isonload, funlist, lenfunlist,*/ procid);
				else 
					read_cb(data.GetBuffer(), newstruct, /*isonload, funlist, lenfunlist,*/ procid);

				// save on the cache the processed functions
				cachefuns = &cachecmds.insert(map<CString, 
							scmdimp>::value_type(doc, *newstruct)).first->second;
				endcachecmds = cachecmds.end();

				// end collection the functions availables if any and only if is it the onload function
				//if(isonload) 
				//{ 
				//	if(lenfunlist == 9) // no hubo funciones para esta transaccion (home for instance)
				//	{ 
				//		set4ch(&funlist[lenfunlist], cpairs::LEFT,'z','n','o'); lenfunlist += 4;
				//		set4ch(&funlist[lenfunlist], 'f','u','n','s'); lenfunlist += 4;
				//		set4ch(&funlist[lenfunlist], cpairs::SEP,'1',cpairs::RIGHT,cpairs::RIGHT); 
				//		lenfunlist += 4;
				//		funlist[lenfunlist] = 0;
				//	}
				//	else 
				//	{ 
				//		set2ch(&funlist[lenfunlist],  cpairs::RIGHT,0); ++lenfunlist; 
				//		require(lenfunlist >= MAXFUNLISTLEN, _T("memory_overrun"));
				//	}
				//}
			}

			// once we have all the functions we must look for the function that we
			// want, and if it is not exist and it starts we on, then is a event 
			// function which work only if the exist but if they dont we are right
			// (they are optional) but a normal function must exist otherwise is an 
			// error
			eventcmd* cmd = get_action(cachefuns, funname, funlen);

			// we generate all the function mess
			int nfuns = 0;
			TCHAR listdata[64];
			int listlen = 0;

			if(cmd)	generate_action(cachefuns, cmd, false, nfuns, functions, cmd, listdata, listlen);

			// return howmany funs this document has
			functions.set(ZZNFUNS, nfuns, ZZNFUNSLEN);

			// the list will be modified the current list's data
			if(listlen) {
				TCHAR temp[256];
				int templen = mikefmt(temp, _T("%czlistdt%c")
												_T("%clstprms%c1%c")
												_T("%cnlistaf%c%d%c")
												_T("%s")
											_T("%c"), 
											cpairs::LEFT,	cpairs::SEP, 
												cpairs::LEFT, cpairs::SEP, cpairs::RIGHT,
												cpairs::LEFT, cpairs::SEP, (listlen/8), cpairs::RIGHT,
												listdata, 
											cpairs::RIGHT);
				functions.append(temp, templen);
			}

			// check is the function does not want to save the sate, it only works on onenter
			if(cmd && cmd->savestate && funlen == 7 &&	cmp4ch(&funname[0],'o','n','e','n') && 
														cmp4ch(&funname[4],'t','e','r',0  ))
				functions.set(ZSAVSTA, cmd->savestate, ZSAVSTALEN);

			// is there a list info to return
			//if(lenfunlist) functions.append(funlist, lenfunlist);

			ensure(functions.is_bad_formed(), _T("functions_bad_formed"));

			// when we have the real function we store in the cache for latter use
			if(isstdfun || nfuns > 0) {
				cachefunctions.insert(map<CString, cpairs>::value_type(func2find, functions));
				endcachefuns = cachefunctions.end();
			}
			realfunstring = functions.buffer();
			realfunstringlen = functions.get_len();
		}

		// we update the request raw data with the function to be executed
		if(realfunstring) _params.set(ZFINFUN, realfunstring, ZFINFUNLEN, realfunstringlen);
	}
	catch(const TCHAR* e)	{ ::LeaveCriticalSection(&csfuns); throw; }
	catch(CString& e)			{ ::LeaveCriticalSection(&csfuns); throw; }
	catch(_com_error &e)	{ ::LeaveCriticalSection(&csfuns); throw; }
	catch(mroerr& e)			{ ::LeaveCriticalSection(&csfuns); throw; }
	catch(CException *e)	{ ::LeaveCriticalSection(&csfuns); throw; }
	catch(...)						{ ::LeaveCriticalSection(&csfuns); throw; }
	::LeaveCriticalSection(&csfuns);
}

