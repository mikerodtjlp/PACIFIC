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

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

struct trick 
{ 
	trick()	{ buffer = 0; lenght = 0; }
	trick(TCHAR* buf, int len) : buffer(buf), lenght(len) {}; 
	TCHAR* buffer; int lenght; 
};

CString* sqlres;
cMroList* sqlvars;
std::vector<std::vector<trick> > memstock;  

#define ROWSPERMEMSTOCK 8
void initialize_sql(/*CParameters& prms*/)
{
	//if(!prms.isactv(_T("loadsql"),7)) return;

	CnnPtr cp;
	cp.CreateInstance( __uuidof( ADODB::Connection ) );
	require(cp.GetInterfacePtr() == nullptr, _T("cound_not_create_ado_connection"));

	int tolerance = gtolerance;
	if(tolerance == 0) tolerance = 300;
	dbhelper::timeout = tolerance;
	dbhelper::initialize_dbhelper(MAX_PROCS, gmaxdbcons);

	sqlres		= new CString[MAX_PROCS];
	sqlvars		= new cMroList[MAX_PROCS];

	memstock.resize(MAX_PROCS);
	for(int i=0; i<MAX_PROCS; ++i)
		memstock[i].resize(ROWSPERMEMSTOCK);
}

void initialize_sql_connections()
{
	_tcscpy_s(dbhelper::connection, gappstr);
	for(int i=0; i<(MAX_PROCS*0.25); ++i)
	{
		dbhelper dbmanagercon(i);
		for(int j=0; j<(dbhelper::maxconnections*0.25); ++j)
			dbmanagercon.get_db_from_gbl_manager(); // force to open
	}
}

bool sqlpure(const int procid, CParameters& result, Table& obj, CParameters& params)
{
	const int MAXCOLSIZE = 1024;
	TCHAR row[MAXCOLSIZE];

	int lid = params.getint(ZLISTAF, ZLISTAFLEN);

	// + we get the columns 
	UINT cols = obj.get_column_count();
	int collen = 0;
	for(UINT i = 0; i < cols; ++i)
	{
		set2ch(&row[collen], CParameters::LEFT,'z');
		collen += 2;
		row[collen++] = 48 + lid;
		set2ch(&row[collen], 'c','l');
		collen += 2;
		_ltot(i, &row[collen], 10);
		if(i<10) ++collen;
		else collen += 2; 
		set2ch(&row[collen], 'z',CParameters::SEP);
		collen += 2;
		collen += obj.get_column_name(i, &row[collen]);
		set2ch(&row[collen++], CParameters::RIGHT, 0);
	}
	TCHAR k[32]; int kl = mikefmt(k, _T("z%dnclsz"), lid);
	TCHAR v[32]; int vl = mikefmt(v, _T("%d"), cols);
	collen += cpairs::gen_pair(0, &row[collen], k, v, kl, vl);

	result.set_value(row, collen);
	// - we get the columns 

	int irow = 0;
	TCHAR slrow[8];
	slrow[0] = 48 + lid;
	slrow[6] = _T('\0');
	int lrowlen = 0;

#define DYNVECTOR 64
	std::vector<trick> res;
	std::vector<trick>::iterator end;
	UINT dveclen = 0;
	int lon;
	TCHAR* mbuffer = 0;

#define STAVECTOR 6
	TCHAR svector[STAVECTOR * 1024];
	int sveclen = 0;
	int svectop = STAVECTOR * 1024;

	int bufflen = 0;
	int bufftop = 0;
	int buffpak = 0;

	CString date;

	for(;!obj.IsEOF(); obj.MoveNext(), ++irow)
	{
		_ltot(irow, &slrow[1], 10);
		lrowlen = _tcslen(slrow);

		TCHAR field = _T('A');
		TCHAR* p = row;
		int image = 0;
		int ldt = 0;
		TCHAR colname[64];
		int colnl = 0;
		for(UINT i = 0; i < cols; ++i)
		{
			int len = 0;
			switch(obj.get_column_type(i))
			{
				case ADODB::adSmallInt: 
					colnl = obj.get_column_name(i, colname);
					if(	(colnl == 1 && cmp2ch(colname,'*',0)) ||
						(colnl == 4 && cmp4ch(colname, '_','i','m','g')))
					{ 
						image = obj.getbyte(i); 
						continue; 
					}
					else len = mikefmt(p, _T("[l%s%c:%d]"), slrow, field, obj.getbyte(i)); 
				break;

				case ADODB::adInteger: 
					set2ch(&p[len], CParameters::LEFT,'l'); len += 2;
							 if(irow<10)	{ set2ch(&p[len], slrow[0], slrow[1]); len += 2;							}
						else if(irow<100)	{ set2ch(&p[len], slrow[0], slrow[1]); len += 2; p[len] = slrow[2]; ++len;	}
						else if(irow<1000)	{ set4ch(&p[len], slrow[0], slrow[1], slrow[2], slrow[3]); len += 4;		}
						else				{ _tmemcpy(&p[len], slrow, lrowlen); len += lrowlen;						}
					set2ch(&p[len], field, CParameters::SEP); len += 2;

					_ltot(obj.getint(i), &p[len], 10); len += _tcslen(&p[len]);
					set2ch(&p[len++], CParameters::RIGHT,0); 
				break;

				case ADODB::adDate:
				case ADODB::adDBDate:
				case ADODB::adDBTime:
				case ADODB::adDBTimeStamp: 
					set2ch(&p[len], CParameters::LEFT,'l'); len += 2;
							 if(irow<10)	{ set2ch(&p[len], slrow[0], slrow[1]); len += 2;							}
						else if(irow<100)	{ set2ch(&p[len], slrow[0], slrow[1]); len += 2; p[len] = slrow[2]; ++len;	}
						else if(irow<1000)	{ set4ch(&p[len], slrow[0], slrow[1], slrow[2], slrow[3]); len += 4;		}
						else				{ _tmemcpy(&p[len], slrow, lrowlen); len += lrowlen;						}
					set2ch(&p[len], field, CParameters::SEP); len += 2;

					obj.getdatedt(i, date);
					ldt = date.GetLength();
					_tmemcpy(&p[len], date.GetBuffer(), ldt); len += ldt;
					set2ch(&p[len++], CParameters::RIGHT,0); 
				break;

				case ADODB::adLongVarWChar:
				case ADODB::adLongVarChar:
				case ADODB::adWChar:
				case ADODB::adVarWChar:
				case ADODB::adBSTR:
				case ADODB::adChar:
				case ADODB::adVarChar:	
					set2ch(&p[len], CParameters::LEFT,'l'); len += 2;
							 if(irow<10)	{ set2ch(&p[len], slrow[0], slrow[1]); len += 2;							}
						else if(irow<100)	{ set2ch(&p[len], slrow[0], slrow[1]); len += 2; p[len] = slrow[2]; ++len;	}
						else if(irow<1000)	{ set4ch(&p[len], slrow[0], slrow[1], slrow[2], slrow[3]); len += 4;		}
						else				{ _tmemcpy(&p[len], slrow, lrowlen); len += lrowlen;						}
					set2ch(&p[len], field, CParameters::SEP); len += 2;

					len += obj.get(i, &p[len]);
					set2ch(&p[len++], CParameters::RIGHT,0); 
				break;

				case ADODB::adDouble:	len = mikefmt(p, _T("[l%s%c:%.2f]"), slrow, field, obj.getdouble(i)); break;
				case ADODB::adBinary:	len = mikefmt(p, _T("[l%s%c:%d]"), slrow, field, obj.getbyte(i)); break;
				default:
					len = mikefmt(p, _T("[l%s%c:%d]"), slrow, field, obj.getbyte(i)); break;
			}
			++field;

			p += len;
		}
		lon = s_ifmt1(p, _T("[l%s*:%d]"), slrow, image, 9, lrowlen); 
		int rowlen = p - row;
		lon += rowlen;

		require(rowlen >= 1024, _T("memory_overrun"));

		// we gonna use the static vetor as possible
		if((sveclen + lon) < svectop)
		{
			memcpy(&svector[sveclen], row, sizeof(TCHAR)*lon);
			sveclen += lon;
			continue;
		}

		// check if the buffer is full our is our first time
		if((bufflen + lon) >= bufftop || bufftop == 0) 
		{
			// we must adjust the real lenght of the previous chunk
			if(mbuffer) res[buffpak-1].lenght = bufflen;
			mbuffer = 0;
		}

		// create a bunch of DYNVECTOR rows in advance
		if(mbuffer == 0)
		{
			int chuncklen = 1024; //lon + 1024 + 1;
			bufftop = chuncklen * DYNVECTOR;

			// first we check if on the memstock we have some enough memory
			if(buffpak < ROWSPERMEMSTOCK)
			{
				trick* mp = &memstock[procid][buffpak];
				if(bufftop > mp->lenght)
				{
					mp->buffer = mbuffer = (TCHAR*)malloc(sizeof(TCHAR)*(bufftop));
					mp->lenght = bufftop;
				}
				else mbuffer = mp->buffer;
			}
			else mbuffer = (TCHAR*)malloc(sizeof(TCHAR)*(bufftop));

			bufflen = 0;
			res.push_back(trick(mbuffer, bufftop));
			++buffpak;
		}
		TCHAR* buffer = &mbuffer[bufflen];

		// putting into the vector
		memcpy(buffer, row, sizeof(TCHAR)*lon);
		dveclen += lon;
		bufflen += lon;
	}
	// we must adjust the real lenght of the previous chunk
	if(mbuffer) res[buffpak-1].lenght = bufflen;

	// if there is no buffpacks, thats mean that the static vetor contains all the 
	// data so we can return it directly as a reponse, the 64 check is because we 
	// still need a room for the totals
	if(buffpak == 0 && sveclen < ((STAVECTOR * 1024) - 128))
	{
		sveclen += gen_tot_list(&svector[sveclen],lid,irow);
		result.append(svector, sveclen);
	}
	else
	{
		// get the necessary memory for the response
		TCHAR* presult = 0;
		require(collen >= MAXCOLSIZE, _T("wrong_logic"));
		mro::memhelper::get_mem_from_gbl_manager(&presult, sveclen + dveclen + 128 + collen, procid,1);
		TCHAR* pres = presult;

		// first we process the static vector if have something
		if(sveclen) memcpy(pres, svector, sizeof(TCHAR)*sveclen);

		// pass from the temporary vector to the final string
		sProcess& proc = procs[procid];
		pres += sveclen;
		end = res.end();
		int cycle = 0;
		for(std::vector<trick>::iterator iter = res.begin(); iter != end; ++iter)
		{
			// pass into the final result
			TCHAR* q = (*iter).buffer;
			int l = (*iter).lenght;
			memcpy(pres, q, sizeof(TCHAR)*l);
			pres += l;

			if(cycle >= ROWSPERMEMSTOCK)
			{
				// get rid of the memory, we not use all the pool, left some for other stuff
				if(proc.gcpoolid < (MAXGCPOOL - (MAXGCPOOL/4))) 
				{ proc.gcpool[proc.gcpoolid] = (*iter).buffer; ++proc.gcpoolid; }
				else { ++gfreesdone; free((*iter).buffer); }
			}
			++cycle;
		}

		// put the totals on the final string
		pres += gen_tot_list(pres,lid,irow);
		result.append(presult, pres - presult);
	}

	return true;
}

bool sql2vars(const int procid, CParameters& result, Table& obj, CParameters& params)
{
	bool bres = true;
	if(obj.IsEOF())
	{
		if(params.getint(_T("cchkreg"), 7)) // check if data exist
		{
			result.set(ZSERROR, _T("reg_not_exist"), ZSERRORLEN);
			bres = false;
		}
		return bres; // if there is no data, why continue?
	}

	CString& tmpstr = sqlres[procid];
	cMroList& guivars =  sqlvars[procid];

	TCHAR data[2048];
	int ldata = params.get(_T("guivars"), data, 7, 2047);
	guivars.set_value(data, ldata);

	TCHAR variable[64];
	int col = 0;

	for(guivars.begin(); guivars.end() == false; guivars.next(), ++col)
	{
		int varlen = guivars.get(variable);
		if(!varlen) break;

		switch(obj.get_column_type(col))
		{
		case ADODB::adSmallInt:		tmpstr = mro::int_to_str(obj.getbyte(col));			break;
		case ADODB::adInteger:		tmpstr = mro::int_to_str(obj.getint(col));			break;
		case ADODB::adDate:
		case ADODB::adDBDate:
		case ADODB::adDBTime:
		case ADODB::adDBTimeStamp:	tmpstr = obj.getdate(col).Format(_T("%Y/%m/%d %H:%M:%S")); break;

		case ADODB::adLongVarWChar:
		case ADODB::adLongVarChar: tmpstr = obj.get(col);								break;

		case ADODB::adWChar:
		case ADODB::adVarWChar:
		case ADODB::adBSTR:
		case ADODB::adChar:
		case ADODB::adVarChar:		tmpstr = obj.get(col);								break;
		case ADODB::adDouble:		tmpstr.Format(_T("%.2f"), obj.getdouble(col));		break;
		default: continue;
		}

		if(variable[0] == '^') result.set(&variable[1], tmpstr, varlen-1);
		else result.set(variable, tmpstr, varlen);
	}

	return bres;
}

bool execute_sql(		const TCHAR* component, const int complen, 
						CParameters& params, 
						const TCHAR* command, const int cmdlen,
						CParameters& result, 
						const int procid, 
						CString* valuestochange)
{
	require(dbhelper::dbgbl == 0, _T("sql_not_suported"));

	dbhelper dbmanagercon(procid);
	cConnection& con = dbmanagercon.get_db_from_gbl_manager();
	Table& query = dbmanagercon.get_qry_from_gbl_manager();

	sProcess& prox = procs[procid];
	sClients& clie = clies[prox.clieid];

	try
	{
		int querysize  = params.get_len() + 256;
		TCHAR* squery  = get_memory(procs[procid], querysize, 0);

		// we identify the query
		set4ch(squery, '-', '-', ' ', ' ');
		int len = 4;
		_tmemcpy(&squery[len], clie.machine, clie.maclen);	len += clie.maclen;
		squery[len++] = ' ';
		_tmemcpy(&squery[len], clie.user, clie.usrlen);		len += clie.usrlen;
		squery[len++] = ' ';
		_tmemcpy(&squery[len], clie.trans, clie.trnlen);	len += clie.trnlen;
		set4ch(&squery[len], ' ', ' ', '\n', 0);
		len += 3;

		// we add the exeception handling begin
		set4ch(&squery[len], 'b', 'e', 'g', 'i');	len += 4;
		set4ch(&squery[len], 'n', ' ', 't', 'r');	len += 4;
		set2ch(&squery[len], 'y', '\n');			len += 2;

		// we embedd the real query
		int querylen = params.get(_T("command"), &squery[len], 7, querysize-(len+1));
		if(!querylen) querylen = params.get(_T("sqltext"), &squery[len], 7, querysize-(len+1));

		// find out if we need to change special characters
		if(TCHAR* p = (TCHAR*) _tmemchr(&squery[len],  _T('&'), querylen))
		{
			// we change the less than, but not its lenght
			TCHAR*	q = _tcsstr(p, _T("&lt;"));
			if(q) { set4ch(q, ' ', ' ', ' ', '<'); }
			// we change the greater than but not its lenght
					q = _tcsstr(p, _T("&gt;"));
			if(q) { set4ch(q, ' ', ' ', ' ', '>'); }
		}
		len += querylen;

		// we add the exeception handling end
		len += mikefmt(&squery[len],	_T("\nend try\n")
										_T("begin catch\n")
											_T("declare @errmsg varchar(256)\n")
											_T("set @errmsg = ERROR_MESSAGE()\n")
											_T("RAISERROR (@errmsg, 16,1)\n")
										_T("end catch\n"));

		// 128 is for safety, we hope that it is a buffer overflow we have
		// a little chance to not overwrite anything important
		require(len >= querysize, _T("query_2_long")); 
		con.execute(squery, query);
	}
	catch(const TCHAR* e) 	{ con.Close();	manage_exception(result, _T("sql_error"), _T(""), e, _T("execute_sql")		, _T("mro_sql.cpp"), __LINE__, clie.step); return false; }
	catch(const CString& e) { con.Close();	manage_exception(result, _T("sql_error"), _T(""), e, _T("execute_sql")		, _T("mro_sql.cpp"), __LINE__, clie.step); return false; }
	catch(_com_error &e)	{ con.Close();	manage_exception(result, _T("sql_error"), _T(""), (TCHAR*) e.Description()	,_T("execute_sql"), _T("mro_sql.cpp"), __LINE__, clie.step); return false;	}
	catch(CException *e)	{ con.Close(); 	TCHAR d[1024]; e->GetErrorMessage(d,1024); e->Delete();
											manage_exception(result, _T("sql_error"), _T(""), d, _T("execute_sql")		, _T("mro_sql.cpp"), __LINE__, clie.step); return false; }
	catch(mroerr& e)		{ con.Close();	manage_exception(result, _T("sql_error"), e.extrainfo, e.description, e.function, e.errfile, e.errline, clie.step);	return false; }
	catch(...) 				{ con.Close();	manage_exception(result, _T("sql_error") , _T(""), _T(""), _T("execute_sql"), _T("mro_sql.cpp"), __LINE__, clie.step); return false; }

	bool bres = false;

	if(cmdlen == 3 && cmp4ch(command, _T('s'), _T('q'), _T('l'), 0))			
		bres = sql2vars(procid, result, query, params);
	else 
		if(cmdlen == 7 && cmp4ch(command, _T('s'), _T('q'), _T('l'),_T('p')) && cmp4ch(&command[4], _T('u'),_T('r'),_T('e'),0))
			bres = sqlpure(procid, result, query, params);
		else
			ensure(true, _T("function_not_found"));

	valuestochange->Empty(); // at this moment this methods does not change values netheir set log, we clean those parameters

	return bres;
}

void terminate_sql(const int id2kill)
{
}
