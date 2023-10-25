#include "stdafx.h"

//#include "mroctrl.h"
#include "SessionMan.h"

bool CSessionMan::get_session_ids()
{
	if (!machines) { macid = -1; usrid = -1; sesid = -1; return false; }
	if (macid == -1 || usrid == -1 || sesid == -1) return false;
	session& ses = machines[macid].users[usrid].sessions[sesid];
	require(!ses.online && ses.reseted, ESESLKU);
	return ses.online;
}

session* CSessionMan::process_session_ids()
{
	return	get_session_ids() ? &machines[macid].users[usrid].sessions[sesid] : nullptr;
}

/**
 * this function search for specific session that in 5 mins haven't
 * been accessed, and the found ones are put them as a free one
 */
void CSessionMan::reset_ghost_session()
{
	int m = _params.getint(_T("mac"), 3, -1);
	int u = _params.getint(_T("cli"), 3, -1);
	int s = _params.getint(_T("ses"), 3, -1);

	if (m == -1 || u == -1 || s == -1) return;

	session& ses = machines[m].users[u].sessions[s];
	if (ses.online)
	{
		COleDateTime now = COleDateTime::GetCurrentTime();
		COleDateTimeSpan desf = now - ses.lastcontact;
		if (desf.GetTotalMinutes() >= 5)
		{
			ses.online = false;
			ses.reseted = true;
		}
	}
}
/**
 * this function search for all the sessions that in [sestime] mins haven't
 * been accessed, and the found ones are put them as a free one
 */
void CSessionMan::reset_ghost_sessions()
{
	COleDateTime now = COleDateTime::GetCurrentTime();
	auto* pm = &machines[0];
	for (auto* qm = pm + mactop; pm != qm; ++pm)
	{
		if (!pm->naml) continue;
		auto* pu = &pm->users[0];
		for (auto* qu = pu + pm->usrtop; pu != qu; ++pu)
		{
			if (!pu->id) continue;
			auto* ps = &pu->sessions[0];
			for (auto* qs = ps + pu->sestop; ps != qs; ++ps)
			{
				if (ps->hispos == -1) continue; //empty one
				if (ps->online)
				{
					COleDateTimeSpan desf = now - ps->lastcontact;
					if (desf.GetTotalMinutes() >= pu->sestime)
					{
						ps->online = false;
						ps->reseted = true;
					}
				}
			}
		}
	}
}
/**
 * this function release all sessions no matter what, keep only less than 1 mins
 */
void CSessionMan::release_sessions()
{
	COleDateTime now = COleDateTime::GetCurrentTime();
	auto* pm = &machines[0];
	for (auto* qm = pm + mactop; pm != qm; ++pm)
	{
		if (!pm->naml) continue;
		auto* pu = &pm->users[0];
		for (auto* qu = pu + pm->usrtop; pu != qu; ++pu)
		{
			if (!pu->id) continue;
			auto* ps = &pu->sessions[0];
			for (auto* qs = ps + pu->sestop; ps != qs; ++ps)
			{
				if (ps->hispos == -1) continue; //empty one
				if (ps->online)
				{
					COleDateTimeSpan desf = now - ps->lastcontact;
					if (desf.GetTotalMinutes() >= 1)
					{
						ps->online = false;
						ps->reseted = true;
					}
				}
			}
		}
	}
}
/**
 * add or subtract user's session time
 */
void CSessionMan::update_session_time()
{
	int m = _params.getint(_T("mac"), 3, -1);
	int u = _params.getint(_T("cli"), 3, -1);

	if (m == -1 || u == -1) return;

	usuario& usr = machines[m].users[u];
	int ct = _params.getint(_T("currtim"), 7);
	if (ct == 0) return;
	if (_params.getbool(_T("up"), 2))
	{
		require(ct >= 1440, _T("maximum_session_time"));
		usr.sestime += 10;
	}
	else
	{
		require(ct <= 10, _T("minimum_session_time"));
		usr.sestime -= 10;
	}
}
/**
 * this function is periodycally fired by the user in order to say
 * the system that it is active besides it could carry some info
 * from the client that's more or less say what is actually doing
 */
void CSessionMan::notify_use()
{
	if (session* ses = process_session_ids())
		ses->lastcontact = COleDateTime::GetCurrentTime();
}
/**
 * get or adds a tcode on the tcode(transaction) cache
 */
transactions* CSessionMan::findtrans(const TCHAR* us, const int lus)
{
	auto* p = trans;
	::EnterCriticalSection(&sestrns);
	for (auto* q = p + trntop; p != q; ++p)
	{
		if (p->len == lus)
		{
			if (lus == 4) if (cmp4ch(p->name, us[0], us[1], us[2], us[3])) goto end;
			if (_tmemcmp(p->name, us, lus) == 0) goto end;
		}
	}
	p->len = lus;
	if (lus == 4) {
		set4ch(p->name, us[0], us[1], us[2], us[3]);
		p->name[lus] = 0;
	}
	else _tmemcpy(p->name, us, lus + 1);
	++trntop;
end:
	::LeaveCriticalSection(&sestrns);
	return p;
}

/**
 * get or adds a user on the user(client) cache
 */
nombres* CSessionMan::finduser(const TCHAR* us, const int lus)
{
	auto* p = names;
	::EnterCriticalSection(&sesuser);
	for (auto* q = p + namtop; p != q; ++p)
	{
		if (p->len == lus)
		{
			if (lus == 10)
				if (cmp4ch(&p->name[0], us[0], us[1], us[2], us[3]) &&
					cmp4ch(&p->name[4], us[4], us[5], us[6], us[7]) &&
					cmp2ch(&p->name[8], us[8], us[9])) goto end;
			if (_tmemcmp(p->name, us, lus) == 0) goto end;
		}
	}
	p->len = lus;
	if (lus == 10) {
		set4ch(&p->name[0], us[0], us[1], us[2], us[3]);
		set4ch(&p->name[4], us[4], us[5], us[6], us[7]);
		set2ch(&p->name[8], us[8], us[9]);
		p->name[lus] = 0;
	}
	else _tmemcpy(p->name, us, lus + 1);
	++namtop;
end:
	::LeaveCriticalSection(&sesuser);
	return p;
}
int CSessionMan::look4macid(bool& newone, const TCHAR* us, const int lus)
{
	::EnterCriticalSection(&csmachs);
	for (int i = 0; i < mactop; ++i) // existing ones
	{
		auto* mac = &machines[i];
		if (mac->naml == lus && _tmemcmp(mac->name, us, lus) == 0)
		{
			::LeaveCriticalSection(&csmachs);
			return i;
		}
	}

	int id = -1;
	newone = true;
	auto* p = &machines[0];
	for (int i = 0; i < mactop; ++i, ++p) // deleted/empty ones
	{
		if (p->naml == 0 && p->name[0] == 0)
		{
			id = i; // use empty one
			goto end;
		}
	}
	require(mactop == MAXMACHINES, _T("machine_maximum"));
	id = mactop++; // new one
end:
	_tmemcpy(p->name, us, (p->naml = lus) + 1);
	::LeaveCriticalSection(&csmachs);
	return id;
}

int CSessionMan::look4usrid(bool& newone, const int m, const TCHAR* us, const int lus)
{
	require(m == -1, _T("wrong_macid"));

	maquina& mac = machines[m];
	int nusrs = mac.usrtop;
	for (int i = 0; i < nusrs; ++i) // existing ones
	{
		auto* p = &mac.users[i];
		if (p->id && p->id->len == lus && _tmemcmp(p->id->name, us, lus) == 0)
			return i;
	}
	newone = true;
	for (int i = 0; i < nusrs; ++i) // deleted/empty ones
	{
		auto* p = &mac.users[i];
		if (!p->id || p->id->len == 0 && p->id->name[0] == 0)
			return i;  // use empty one
	}
	require(mac.usrtop == MAXUSERS, _T("user_maximum"));
	return mac.usrtop++; // new one
}

int CSessionMan::look4sesid(bool& newone, const int m, const int u)
{
	require(m == -1, _T("wrong_macid"));
	require(u == -1, _T("wrong_usrid"));

	int sessionid = -1;
	usuario& usr = machines[m].users[u];
	int nses = usr.sestop;
	for (int i = 0; i < nses; ++i)
	{
		auto* ses = &usr.sessions[i];
		if (ses->online == false)
		{
			ses->online = true;
			ses->reseted = false;
			sessionid = i;
			break;
		}
	}
	if (sessionid == -1)	// new one
	{
		require(usr.sestop == MAXSESSIONS, _T("session_maximum"));
		sessionid = usr.sestop++;
		auto* ses = &usr.sessions[sessionid];
		ses->online = true;
		ses->reseted = false;
		newone = true;
	}
	return sessionid;
}
/**
 * deletes the a machine
 */
void CSessionMan::delete_machine_session()
{
	int m = _params.getint(_T("mac"), 3, -1);
	if (m == -1) return;

	require(macid == m, _T("cant_kill_yourself"));

	maquina& mac = machines[m];
	int nusrs = mac.usrtop;
	for (int u = 0; u < nusrs; ++u)
	{
		usuario& usr = mac.users[u];
		int nsess = usr.sestop;
		for (int s = 0; s < nsess; ++s)
		{
			session& ses = usr.sessions[s];
			int nhis = ses.histop;
			for (int h = 0; h < nhis; ++h)
			{
				historia& his = ses.history[h];
				his.init();
			}
			ses.init();
		}
		usr.init();
	}
	mac.init();

	// delete from database
	TCHAR command[64];
	getconnection(con);
	mikefmt(command, _T("exec dbo.session_delete_machine %d,%d;"), instid, m);
	con.execute(command);
}
/**
 * deletes the a user
 */
void CSessionMan::delete_user_session()
{
	int m = _params.getint(_T("mac"), 3, -1);
	int u = _params.getint(_T("cli"), 3, -1);
	if (m == -1 || u == -1) return;

	require(macid == m && usrid == u, _T("cant_kill_yourself"));

	maquina& mac = machines[m];
	usuario& usr = mac.users[u];
	int nsess = usr.sestop;
	for (int s = 0; s < nsess; ++s)
	{
		session& ses = usr.sessions[s];
		int nhis = ses.histop;
		for (int h = 0; h < nhis; ++h)
		{
			historia& his = ses.history[h];
			his.init();
		}
		ses.init();
	}
	usr.init();

	// delete from database
	TCHAR command[64];
	getconnection(con);
	mikefmt(command, _T("exec dbo.session_delete_user %d,%d,%d;"), instid, m, u);
	con.execute(command);
}
/**
 * deletes the a session
 */
void CSessionMan::delete_session()
{
	int m = _params.getint(_T("mac"), 3, -1);
	int u = _params.getint(_T("cli"), 3, -1);
	int s = _params.getint(_T("ses"), 3, -1);
	if (m == -1 || u == -1 || s == -1) return;

	require(macid == m && usrid == u && sesid == s, _T("cant_kill_yourself"));
	session& ses = machines[m].users[u].sessions[s];
	int nhis = ses.histop;
	for (int h = 0; h < nhis; ++h)
	{
		historia& his = ses.history[h];
		his.init();
	}
	ses.init();

	// delete from database
	//TCHAR cmd[64];
	//mikefmt(cmd, _T("exec dbo.session_delete_session %d,%d,%d,%d;"),instid,m,u,s);
	//getconnection(con);
	//con.execute(cmd);
}

void get_date_time_dll(CParameters& result)
{
	TCHAR buf_time[32];
	TCHAR buf_date[32];

	_tstrtime(buf_time);
	_tstrdate(buf_date);

	TCHAR res[32 + 32 + 2];
	int len = mikefmt(res, _T("%s %s"), buf_date, buf_time);
	result.set(ZDATTIM, res, ZDATTIMLEN, len);
}

void CSessionMan::session_set_company()
{
	if (session* ses = process_session_ids()) {
		int newcmpy = _params.getint(_T("cmpy"), 4, -1);
		if (newcmpy != -1) ses->cmpy = newcmpy;
	}
}

void CSessionMan::copy_session()
{
	CALLSTACK
		get_date_time_dll(_params);
	create_session();
	copy_state();
	get_last_state();
	//gui_get_session_data();
}
void CSessionMan::begin_session()
{
	CALLSTACK
		create_session();

	if (session* ses = process_session_ids()) {
		// for any new session we clean his libray list (dont know if it's best place)
		TCHAR cmd[128];
		mikefmt(cmd, _T("exec dbo.user_ins_liblist %d,%d,%d,%d,%d,'%s';"),
			instid, macid, usrid, sesid,
			ses->cmpy,
			machines[macid].users[usrid].id->name);
		{
			getconnection(con);
			con.execute(cmd);
		}

		if (ses->hispos == -1) {
			_params.set(ZRTRNCD, ZTHOME, ZRTRNCDLEN, ZTHOMELEN);
			// super parche, cuando entramos por primera vez, tenemos que insertar el home 
			// lo cual esta bien, pero en el futuro se correra una funcion llamada get_last_state 
			// y como las dos regresan el codigo html de la transaccion se esta duplicando
			// asi que cuando se crea por primera vez la session (no confundir con reuso) 
			// no regresamos html porque se regresara, esta funcion se corre cuando nunca 
			// ha habido historia
			gui_insert_trans();
		}
	}
}
void CSessionMan::create_session()
{
	CALLSTACK
		TCHAR machine[ZMACNAMMAX + 1];
	int macl = _basics.get(ZMACNAM, machine, ZMACNAMLEN, ZMACNAMMAX);
	require(!macl, _T("machine_missing"));

	// tricky part, different clients send the user on basics or params
	// so I enforce that the user code should come in the basics
	// theres something that I need to standardrize, maybe it is already solved?
	TCHAR user[ZUSERIDMAX + 1];
	int usrl = _params.get(ZUSERID, user, ZUSERIDLEN, ZUSERIDMAX);
	if (usrl) _basics.set(ZUSERID, user, ZUSERIDLEN, usrl);
	else usrl = _basics.get(ZUSERID, user, ZUSERIDLEN, ZUSERIDMAX);
	require(!usrl, _T("user_missing"));

	// we look for some place on the system's place
	bool macnew = false;
	bool usrnew = false;
	bool sesnew = false;

	maquina* mac = &machines[macid = look4macid(macnew, machine, macl)];
	usuario* usr = &mac->users[usrid = look4usrid(usrnew, macid, user, usrl)];
	session* ses = &usr->sessions[sesid = look4sesid(sesnew, macid, usrid)];

	try
	{
		// the ip or macaddress could change
		mac->adrl = _basics.get(ZIPADDR, mac->ip, ZIPADDRLEN, ZIPADDRMAX);
		mac->mcal = _params.get(ZMACADR, mac->macaddr, ZMACADRLEN, ZMACADRMAX);

		if (usrnew) // determine user and some properties for user and machine
		{
			// at this point we complete the user, after pass all validation
			usr->id = finduser(user, usrl);
			if (usr->tmplparms == nullptr) usr->tmplparms = new cpairs();
			usr->access = 0;
		}
		_params.set(_T("client"), usrid, 6);
		++usr->access;

		if (sesnew) ses->access = 0;

		ses->cmpy = _basics.getint(ZCOMPNY, ZCOMPNYLEN);
		ses->agetyp = _params.getint(ZGUIAGN, ZGUIAGNLEN);
		ses->ismobi = _params.getint(ZBRWTYP, ZBRWTYPLEN);

		ses->start = COleDateTime::GetCurrentTime();
		ses->lastcontact = ses->start;
		++ses->access;

		_params.set(ZSESINS, instid, ZSESINSLEN);
		_params.set(ZSESMAC, macid, ZSESMACLEN);
		_params.set(ZSESCLI, usrid, ZSESCLILEN);
		_params.set(ZSESSES, sesid, ZSESSESLEN);

		_basics.set(ZSESINS, instid, ZSESINSLEN);
		_basics.set(ZSESMAC, macid, ZSESMACLEN);
		_basics.set(ZSESCLI, usrid, ZSESCLILEN);
		_basics.set(ZSESSES, sesid, ZSESSESLEN);
		//_key.set_value(h, len);
	}
	catch (const TCHAR* e) { ses->online = false; throw; }
	catch (CString& e) { ses->online = false; throw; }
	catch (_com_error& e) { ses->online = false; throw; }
	catch (CException* e) { ses->online = false; throw; }
	catch (mroerr& e) { ses->online = false; throw; }
	catch (...) { ses->online = false; throw; }

	_params.set(ZINSTAN, sesid, ZINSTANLEN);
}

/**
 * this function mark the session as a free one, for latter resue
 */
void CSessionMan::end_session()
{
	_params.del(ZINSTAN, ZINSTANLEN);
	if (session* ses = process_session_ids()) ses->online = false;
}

/**
 * this function checks is there is an active session with the same user in
 * other computer
 */
void CSessionMan::get_user_logons()
{
	CString strfinal;
	TCHAR row[1024];
	TCHAR l0[16];
	set2ch(l0, _T('l'), _T('0'));
	int irow = 0;

	TCHAR user[ZUSERIDMAX + 1];	int usrl = _params.get(ZUSERID, user, ZUSERIDLEN, ZUSERIDMAX);
	//TCHAR mach[ZMACNAMMAX + 1];	int macl = _params.get(ZMACNAM, mach, ZMACNAMLEN, ZMACNAMMAX);

	auto* pm = &machines[0];
	for (auto* qm = pm + mactop; pm != qm; ++pm)
	{
		// we search only on different machine
		//if(!pm->naml || (macl == pm->naml && _tmemcmp(pm->name, mach, macl) == 0)) continue;
		auto* pu = &pm->users[0];
		for (auto* qu = pu + pm->usrtop; pu != qu; ++pu)
		{
			// we search only the same user 
			if (!pu->id || usrl != pu->id->len || _tmemcmp(pu->id->name, user, usrl) != 0) continue;
			auto* ps = &pu->sessions[0];
			for (auto* qs = ps + pu->sestop; ps != qs; ++ps)
			{
				if (ps->online)
				{
					COleDateTime st(ps->start);
					mikefmt(&l0[2], _T("%d"), irow);
					int len = mikefmt(row, _T("[%sA:%s][%sB:%s][%sC:%d]"),
						l0, pm->name,
						l0, st.GetStatus() == 0 ? st.Format(_T("%m/%d/%y %H:%M:%S")) :
						_T("invalid_date"),
						l0, 0);
					strfinal.Append(row, len);
					++irow;
				}
			}
		}
	}
	int len = gen_tot_list(row, 0, irow);
	strfinal.Append(row, len);
	_params.append(strfinal);
}

void CSessionMan::get_file_into_response()
{
	TCHAR xmlfile[ZRTRNCDMAX + 1];
	if (int len = _params.get(ZRTRNCD, xmlfile, ZRTRNCDLEN, ZRTRNCDMAX))
	{
		_params.set(ZFILE01, xmlfile, ZFILE01LEN, len);
		_params.set(ZTYPTRN, _T("trans"), ZTYPTRNLEN, 5);
		_get_file();
	}
}

void CSessionMan::get_last_result(const int ins,
	const int mac, const int cli,
	const int ses, const int his,
	const int cmpy,
	const TCHAR* tcode, const int tclen)
{
	// main menu does not have results
	if (tclen == 4 && cmp4ch(tcode, _T('S'), _T('0'), _T('0'), _T('1'))) return;

	TCHAR query[128];
	mikefmt(query, _T("exec dbo.get_last_result %d,%d,%d,%d,%d,%d,'%s';"),
		ins, mac, cli, ses, his, cmpy, tcode);
	getconnectionx(con, obj);
	con.execute(query, obj);
	if (!obj.IsEOF())
	{
		int procid = _basics.getint(ZPROCNO, ZPROCNOLEN);
		int SZPR = obj.getint(0);
		int SZVL = obj.getint(1);
		int SIZE = SZPR + SZVL + 256;//extra for extra reserved words

		if (SIZE > (1024 * 32)) return; //either errors,BUGS or too large are banned

		int size = sizeof(TCHAR) * (SIZE);
		TCHAR* res = nullptr;

		mro::memhelper::get_mem_from_gbl_manager(&res, size, procid, 0);

		cpairs r;
		r.set(ZDSPPAG, tcode, ZDSPPAGLEN, tclen);

		res[0] = cpairs::LEFT;
		_tmemcpy(&res[1], ZORIVAL, ZORIVALLEN);
		res[1 + ZORIVALLEN] = cpairs::SEP;
		int lo = 0;
		if (lo = obj.get(2, &res[1 + ZORIVALLEN + 1], SIZE))
		{
			//if (res[SZPR - 1] != cpairs::RIGHT) return; // check end 
			res[lo = (lo + 1 + ZORIVALLEN + 1)] = cpairs::RIGHT;
			r.append(res, ++lo); // params
		}

		res[0] = cpairs::LEFT;
		_tmemcpy(&res[1], ZLSTRES, ZLSTRESLEN);
		res[1 + ZLSTRESLEN] = cpairs::SEP;
		int lr = 0;
		if (lr = obj.get(3, &res[1 + ZLSTRESLEN + 1], SIZE - lo))
		{
			//if (res[SZVL - 1] != cpairs::RIGHT) return; // check end 
			res[lr = (lr + 1 + ZLSTRESLEN + 1)] = cpairs::RIGHT;
			r.append(res, ++lr); // values
		}

		/*if(int len = obj.get(1, res, SIZE))
		{
			if(len < SIZE-1)
				r.append(res, len); // res
			else
				if(plen > (ZORIVALLEN+3)) // have values ?
					r.set(ZDSPTCH, ZONENTR, ZDSPTCHLEN, ZONENTRLEN);
		}*/

		if (lo + lr >= SIZE) return;
		if (!r.is_bad_formed())
			_params.append(r); // everthing was well we return the last result
	}
}

void CSessionMan::check_session()
{
	if (session* ses = process_session_ids()) {
		if (ses->online) {
			COleDateTime now = COleDateTime::GetCurrentTime();
			COleDateTimeSpan desf = now - ses->lastcontact;
			int left = machines[macid].users[usrid].sestime - desf.GetTotalMinutes();
			_params.set(_T("timeleft"), left, 8);
		}
	}
}
//void CSessionMan::get_last_state_nosafe() { get_laststate(false); };
void CSessionMan::get_last_state() {
	get_laststate(true);
};

/**
 * this function is fired by the client when needs to know the last result
 * this function is mostly used when the client is entering again in the system
 */
void CSessionMan::get_laststate(const bool safe)
{
	session* ses = process_session_ids();

	// a brand new session we must insert main transaction
	if (!ses || ses->hispos == -1) {
		gui_get_entrance();
		_params.set(ZHISPOS, -1, ZHISPOSLEN);
	}
	else {
		// if it comes from a father we look in his history... if any
		transactions* trn = ses->history[ses->hispos].trn;
		if (trn && trn->len && trn->name[0]) {
			TCHAR user[ZUSERIDMAX + 1];
			int userlen = _basics.get(ZUSERID, user, ZUSERIDLEN, ZUSERIDMAX);

			TCHAR rights[1024];
			int l = obtain_rights(ses->cmpy, user, userlen, trn->name, trn->len, rights);
			CParameters prms(rights, l);
			if (prms.getint(ZACCRGT, ZACCRGTLEN) != 1) {
				gui_go_home();
				return;
			}
			_params.set(ZRTRNCD, trn->name, ZRTRNCDLEN, trn->len);
			get_last_result(instid, macid, usrid, sesid, ses->hispos, ses->cmpy, trn->name, trn->len);

			// get the last transcation for the history
			try {
				get_file_into_response();
			}
			catch (...) {
				if (safe) {	// on safe mode any problem (library related) ret home
					gui_go_home();
					return;
				}
				throw;
			};

			_params.set(ZHISPOS, ses->hispos, ZHISPOSLEN);
		}
		else gui_go_home();
	}
}

void CSessionMan::copy_state()
{
	// our destiny(the current session)
	if (session* ses = process_session_ids())
	{
		// get the source(the father session)
		int srcinsid = _params.getint(_T("sfatheri"), 8, -1);
		int srcmacid = _params.getint(_T("sfatherm"), 8, -1);
		int srcusrid = _params.getint(_T("sfatherc"), 8, -1);
		int srcsesid = _params.getint(_T("sfathers"), 8, -1);
		if (srcmacid == -1 || srcusrid == -1 || srcsesid == -1) return;

		// check is there is not the same(believeme it could happen)
		if (macid == srcmacid && usrid == srcusrid && sesid == srcsesid) return;

		session* src = &machines[srcmacid].users[srcusrid].sessions[srcsesid];

		// copy the history of transactions
		memcpy(&ses->history[0], &src->history[0], sizeof(historia) * MAXHISTORY);
		ses->histop = src->histop;
		ses->hispos = src->hispos;
		ses->cmpy = src->cmpy;
		ses->agetyp = src->agetyp;
		ses->ismobi = src->ismobi;

		TCHAR cmd[1024];
		mikefmt(cmd, _T("exec dbo.copy_last_result %d,%d,%d,%d,%d;")
			_T("exec dbo.user_copy_liblist %d,%d,%d,%d,%d;"),
			instid, macid, usrid, srcsesid, sesid,
			instid, macid, usrid, srcsesid, sesid);
		getconnection(con);
		con.execute(cmd);
	}
}

void CSessionMan::gui_get_top()
{
	if (session* ses = process_session_ids())
		_params.set(_T("currtop"), ses->histop, 7);
}

void CSessionMan::gui_get_pos()
{
	if (session* ses = process_session_ids())
		_params.set(_T("currpos"), ses->hispos, 7);
}

void CSessionMan::gui_insert_trans() {
	CALLSTACK
	if (session* ses = process_session_ids()) {
		TCHAR tran[CMPMAX + 1];
		int lt = _params.get(ZRTRNCD, tran, ZRTRNCDLEN, CMPMAX);

		bool islogon = lt == 4 && (	cmp4ch(tran, 'S', '0', '0', '0') ||
									cmp4ch(tran, 'S', '0', '5', '0') ||
									cmp4ch(tran, 'S', '0', '7', '0') ||
									cmp4ch(tran, 'S', '0', '9', '0'));

		require(islogon, _T("forbidden_transaction"));
		require(ses->hispos >= MAXHISTORY - 1, _T("history_maximum"));

		_params.set(ZTYPRGT, ZACCRGT, ZTYPRGTLEN, ZACCRGTLEN);
		have_right();

		//require(ses->hispos >= MAXHISTORY - 1, _T("history_maximum"));

		if (_params.isactv(_T("norethtml"), 9) == false)
			get_file_into_response();

		// return the position according to the history
		usuario& usr = machines[macid].users[usrid];
		historia& history = ses->history[++ses->hispos];
		history.trn = findtrans(tran, lt);
		ses->histop = ses->hispos;
		_params.set(ZHISPOS, ses->hispos, ZHISPOSLEN);

		TCHAR cmd[128];
		mikefmt(cmd, _T("exec dbo.favorites_set %d,'%s','%s';"),
			ses->cmpy, usr.id->name, tran);
		getconnection(con);
		con.execute(cmd);
	}
}

void CSessionMan::gui_goto_trans() {
	TCHAR whereg[ZCOMPNMMAX + 1]; 
	int wl = _params.get(_T("where"), whereg, 5, ZCOMPNMMAX);
	_params.set(ZRTRNCD, whereg, ZRTRNCDLEN, wl);

	gui_insert_trans();

	TCHAR params[1024];	int pl = _params.get(_T("params"), params, 6, 1023);
	_params.set(ZLSTRES, params, ZLSTRESLEN, pl);

	TCHAR action[ZFUNNAMMAX + 1];
	if (int al = _params.get(_T("function"), action, 8, ZFUNNAMMAX)) {
		_params.set(ZDSPPAG, whereg, ZDSPPAGLEN, wl);
		_params.set(ZDSPTCH, action, ZDSPTCHLEN, al);
	}
}

void CSessionMan::gui_go_back() {
	if (session* ses = process_session_ids()) {
		require(ses->hispos == 0, _T("no_more"));
		auto* trn = ses->history[ses->hispos - 1].trn;
		_params.set(ZTYPRGT, ZACCRGT, ZTYPRGTLEN, ZACCRGTLEN);
		_params.set(ZRTRNCD, trn->name, ZRTRNCDLEN, trn->len);
		have_right();
		--ses->hispos;
		_params.set(ZRTRNCD, trn->name, ZRTRNCDLEN, trn->len);
		get_last_result(instid, macid, usrid, sesid, ses->hispos, ses->cmpy, trn->name, trn->len);
		get_file_into_response();
		_params.set(ZHISPOS, ses->hispos, ZHISPOSLEN);
	}
}

void CSessionMan::gui_go_home()
{
	if (session* ses = process_session_ids())
	{
		ses->hispos = 0;
		auto* trn = ses->history[ses->hispos].trn;
		_params.set(ZRTRNCD, trn->name, ZRTRNCDLEN, trn->len);
		get_last_result(instid, macid, usrid, sesid, ses->hispos, ses->cmpy, trn->name, trn->len);
		get_file_into_response();
		_params.set(ZHISPOS, ses->hispos, ZHISPOSLEN);
	}
}

void CSessionMan::gui_go_forward()
{
	if (session* ses = process_session_ids())
	{
		require(ses->hispos == ses->histop, _T("no_more"));
		auto* trn = ses->history[ses->hispos + 1].trn;
		_params.set(ZTYPRGT, ZACCRGT, ZTYPRGTLEN, ZACCRGTLEN);
		_params.set(ZRTRNCD, trn->name, ZRTRNCDLEN, trn->len);
		have_right();
		++ses->hispos;
		_params.set(ZRTRNCD, trn->name, ZRTRNCDLEN, trn->len);
		get_last_result(instid, macid, usrid, sesid, ses->hispos, ses->cmpy, trn->name, trn->len);
		get_file_into_response();
		_params.set(ZHISPOS, ses->hispos, ZHISPOSLEN);
	}
}

void CSessionMan::gui_go_pos()
{
	if (session* ses = process_session_ids())
	{
		int newpos = _params.getint(_T("znewpos"), 7);
		if (newpos < -1 || newpos > ses->histop) return;
		auto* trn = ses->history[newpos].trn;
		_params.set(ZTYPRGT, ZACCRGT, ZTYPRGTLEN, ZACCRGTLEN);
		_params.set(ZRTRNCD, trn->name, ZRTRNCDLEN, trn->len);
		have_right();
		ses->hispos = newpos;
		_params.set(ZRTRNCD, trn->name, ZRTRNCDLEN, trn->len);
		get_last_result(instid, macid, usrid, sesid, ses->hispos, ses->cmpy, trn->name, trn->len);
		get_file_into_response();
		_params.set(ZHISPOS, ses->hispos, ZHISPOSLEN);
	}
}

void CSessionMan::gui_get_history()
{
	if (session* ses = process_session_ids())
	{
		TCHAR lang[ZLANGUAMAX + 1];
		_basics.get(ZLANGUA, lang, ZLANGUALEN, ZLANGUAMAX);
		CString key;

		TCHAR k[16];
		CParameters p;
		int htop = ses->histop;
		for (int i = 0; i <= htop; ++i)
		{
			key.SetString(lang, 2);
			auto* trn = ses->history[i].trn;
			if (!trn) continue;
			key.Append(trn->name, trn->len);
			auto iter = cachedescs.lower_bound(key);
			if (iter != enddescs && !(cachedescs.key_comp()(key, iter->first)))
			{
				p.set(ZHISTRN, trn->name, 7, trn->len);
				p.set(ZHISDSC, (*iter).second, 7);
				p.set(ZHISTYP, _T("T"), 7); //fake
			}
			else
			{
				p.set(ZHISTRN, trn->name, 7, trn->len);
				p.set(ZHISDSC, _T("*"), 7, 1);
				p.set(ZHISTYP, _T("T"), 7); //fake
			}

			int len = mikefmt(k, _T("zhis%02dz"), i);
			_params.set(k, p.buffer(), len, p.get_len());
		}
		_params.set(ZHISSIZ, ses->histop + 1, 7);
		_params.set(ZHISPOS, ses->hispos, ZHISPOSLEN);
	}
}

void CSessionMan::get_sessions_count()
{
	TCHAR mach[ZMACNAMMAX + 1];	int macl = _params.get(_T("machine"), mach, 7, ZMACNAMMAX);
	TCHAR user[ZUSERIDMAX + 1];	int usrl = _params.get(_T("user"), user, 4, ZUSERIDMAX);
	TCHAR trns[ZTRNCODMAX + 1];	int trnl = _params.get(_T("trans"), trns, 5, ZTRNCODMAX);

	COleDateTime now = COleDateTime::GetCurrentTime();
	COleDateTimeSpan desf;

	int all = 0;
	int active = 0;
	int free = 0;
	int doubt = 0;
	int lost = 0;
	int limb = 0;

	for (int i = 0; i < mactop; ++i)
	{
		maquina& mac = machines[i];
		if (macl && (macl != mac.naml || _tmemcmp(mach, mac.name, macl) != 0)) continue;
		int nusrs = mac.usrtop;
		for (int j = 0; j < nusrs; ++j)
		{
			usuario& usr = mac.users[j];
			if (usrl && (!usr.id || usrl != usr.id->len || _tmemcmp(user, usr.id->name, usrl) != 0)) continue;
			int nsess = usr.sestop;
			for (int k = 0; k < nsess; ++k)
			{
				session& ses = usr.sessions[k];
				if (trnl && ses.hispos != -1 &&
					(!ses.history[ses.hispos].trn || trnl != ses.history[ses.hispos].trn->len ||
						_tmemcmp(trns, ses.history[ses.hispos].trn->name, trnl) != 0)) continue;

				++all;
				if (ses.online)
				{
					int sestime = usr.sestime;
					if (sestime)
					{
						desf = now - ses.lastcontact;
						int m = desf.GetTotalMinutes();
						if (m < 5) ++active;
						else if (m >= 5) ++doubt; // doubt
						else if (m >= sestime) ++lost; // lost
					}
					else ++limb;
				}
				else ++free; // free 
			}
		}
	}

	TCHAR row[256];
	int len = mikefmt(row, _T("%all%c%d%c%cactive%c%d%c%cfree%c%d%c%cdoubt%c%d%c%clost%c%d%c%climb%c%d%c"),
		cpairs::LEFT, cpairs::SEP, all, cpairs::RIGHT,
		cpairs::LEFT, cpairs::SEP, active, cpairs::RIGHT,
		cpairs::LEFT, cpairs::SEP, free, cpairs::RIGHT,
		cpairs::LEFT, cpairs::SEP, doubt, cpairs::RIGHT,
		cpairs::LEFT, cpairs::SEP, lost, cpairs::RIGHT,
		cpairs::LEFT, cpairs::SEP, limb, cpairs::RIGHT);
	_params.append(row, len);
}

const TCHAR* ags[7] = { _T(""), _T("chrome"), _T("iexplorer"), _T("opera"),_T("firefox"), _T("safari"), _T("n/a") };
const TCHAR* aty[3] = { _T(""), _T("DSK"), _T("MOB") };

void CSessionMan::get_sessions_info()
{
	TCHAR mach[ZMACNAMMAX + 1];	int macl = _params.get(_T("machine"), mach, 7, ZMACNAMMAX);
	TCHAR user[ZUSERIDMAX + 1];	int usrl = _params.get(_T("user"), user, 4, ZUSERIDMAX);
	TCHAR trns[ZTRNCODMAX + 1];	int trnl = _params.get(_T("trans"), trns, 5, ZTRNCODMAX);

	int thisproc = _basics.getint(ZPROCNO, ZPROCNOLEN);
	int criteria = _params.getint(_T("filter"), 6);

	TCHAR SEP = cpairs::SEP;
	TCHAR RIGHT = cpairs::RIGHT;

	int usrs = 0;
	int sess = 0;

	TCHAR* row;
	int blocksize = 16;
	mro::memhelper::get_mem_from_gbl_manager(&row, (512 * blocksize) + 1024, thisproc);

	TCHAR* p = row;
	int volta = 0;
	int irow = 0;
	int len = 0;

	COleDateTime now = COleDateTime::GetCurrentTime();
	COleDateTimeSpan desf;
	COleDateTimeSpan tims;
	TCHAR l0[8];
	set4ch(l0, cpairs::LEFT, _T('l'), _T('0'), 0);

	maquina* mac = &machines[0];
	for (int i = 0; i < mactop; ++i, ++mac)
	{
		if (mac->naml == 0) continue; //empty one

		if (macl && (macl != mac->naml || _tmemcmp(mach, mac->name, macl) != 0)) continue;
		int nusrs = mac->usrtop;
		usrs += nusrs;
		usuario* usr = &mac->users[0];
		for (int j = 0; j < nusrs; ++j, ++usr)
		{
			//if (!usr->id) continue; //empty one

			if (usrl && (!usr->id || usrl != usr->id->len || _tmemcmp(user, usr->id->name, usrl) != 0)) continue;
			int nsess = usr->sestop;
			sess += nsess;
			session* ses = &usr->sessions[0];
			for (int k = 0; k < nsess; ++k, ++ses)
			{
				if (ses->hispos == -1) continue; //empty one

				transactions* tr = ses->history[ses->hispos].trn;
				if (trnl && (!tr || trnl != tr->len || _tmemcmp(trns, tr->name, trnl) != 0)) continue;

				bool sesonline = ses->online;
				int sestime = usr->sestime;

				if (volta == 0) p = row;

				desf = now - ses->lastcontact;
				tims = now - ses->start;

				int image = 0; // active
				if (sesonline)
				{
					if (sestime)
					{
						int m = desf.GetTotalMinutes();
						if (m >= 5) image = 1; // doubt
						if (m >= sestime) image = 2; // lost
					}
				}
				else image = 3; // free 

				if (criteria == 1 && (image != 0 && image != 1)) continue;

				if (criteria == 2 && image != 0) continue;
				if (criteria == 3 && image != 3) continue;
				if (criteria == 4 && image != 1) continue;
				if (criteria == 5 && image != 2) continue;

				_ltot(irow, &l0[3], 10);
				len = mikefmt(p, _T("%sA%c%d:%d:%d:%d%c")
					_T("%sB%c%s%c")
					_T("%sC%c%s%c")
					_T("%sD%c%s%c")
					_T("%sE%c%s%c")
					_T("%sF%c%ld%c")
					_T("%sG%c%s%c")
					_T("%sH%c%s%c")
					_T("%sI%c%s%c")
					_T("%sJ%c%s%c")
					_T("%sK%c%d%c")
					_T("%sL%c%d%c")
					_T("%sM%c%s%c")
					_T("%sN%c%s%c")
					_T("%sO%c%d%c")
					_T("%sP%c%d%c")
					_T("%sQ%c%d%c")
					_T("%sR%c%d%c")
					_T("%s*%c%d%c"),
					l0, SEP, instid, i, j, k, RIGHT, 	//A
					l0, SEP, mac->name, RIGHT, 	//B
					l0, SEP, mac->ip, RIGHT,	//C
					l0, SEP, mac->macaddr, RIGHT,	//D
					l0, SEP, usr->id ? usr->id->name : _T("?"), RIGHT, 	//E
					l0, SEP, ses->access, RIGHT, 	//F
					l0, SEP, sesonline ? desf.Format(_T("%H:%M:%S")) : _T(""), RIGHT, //G	
					l0, SEP, sesonline ? tims.Format(_T("%H:%M:%S")) : _T(""), RIGHT, //H
					l0, SEP, tr ? tr->name : _T("?"), RIGHT, //I
					l0, SEP, ses->lastcontact.Format(_T("%H:%M:%S")), RIGHT, 	// J
					l0, SEP, sestime, RIGHT, // K
					l0, SEP, ses->cmpy, RIGHT, // L
					l0, SEP, ags[ses->agetyp + 1], RIGHT, // M
					l0, SEP, aty[ses->ismobi + 1], RIGHT, // N
					l0, SEP, instid, RIGHT, 	//O
					l0, SEP, i, RIGHT, 	//P
					l0, SEP, j, RIGHT, 	//Q
					l0, SEP, k, RIGHT, 	//R
					l0, SEP, image, RIGHT);
				p += len;
				if (volta++ == blocksize - 1)
				{
					int lon = p - row;
					_params.append(row, lon);
					volta = 0;
				}
				++irow;
			}
		}
	}
	int lon = p - row;
	_params.append(row, lon);

	TCHAR k[1024];
	int kl = mikefmt(k, _T(", trans:%d, nams:%d, macs:%d, usrs:%d, sess:%d"),
		trntop, namtop, mactop, usrs, sess);

	len = gen_tot_list(row, 0, irow, k, kl);
	_params.append(row, len);

	len = mikefmt(row, _T("%cMAXTRNS%c%d%c%cMAXNAMS%c%d%c%cMAXMACS%c%d%c%cMAXUSRS%c%d%c%cMAXSESS%c%d%c%cMAXHIST%c%d%c"),
		cpairs::LEFT, cpairs::SEP, MAXTRANS, cpairs::RIGHT,
		cpairs::LEFT, cpairs::SEP, MAXNAMES, cpairs::RIGHT,
		cpairs::LEFT, cpairs::SEP, MAXMACHINES, cpairs::RIGHT,
		cpairs::LEFT, cpairs::SEP, MAXUSERS, cpairs::RIGHT,
		cpairs::LEFT, cpairs::SEP, MAXSESSIONS, cpairs::RIGHT,
		cpairs::LEFT, cpairs::SEP, MAXHISTORY, cpairs::RIGHT);

	COleDateTime date;
	date.ParseDateTime(CString(__DATE__ " " __TIME__));
	_params.set(_T("ctrlver"), date.Format(_T("%Y/%m/%d %H:%M:%S")), 7);
	_params.append(row, len);
}

void CSessionMan::get_client_history()
{
	CString strfinal;
	TCHAR row[1024];
	TCHAR l0[16];
	set2ch(l0, _T('l'), _T('0'));
	int irow = 0;

	TCHAR mach[ZMACNAMMAX + 1];	int macl = _params.get(_T("machine"), mach, 7, ZMACNAMMAX);
	TCHAR user[ZUSERIDMAX + 1];	int usrl = _params.get(_T("user"), user, 4, ZUSERIDMAX);

	maquina* mac = &machines[0];
	for (int i = 0; i < mactop && i < MAXMACHINES; ++i, ++mac)
	{
		if (mac->naml == 0) continue; //empty one

		if (macl && (macl != mac->naml || _tmemcmp(mach, mac->name, macl) != 0))	continue;
		int usrtop = mac->usrtop;
		usuario* usr = &mac->users[0];
		for (int j = 0; j < usrtop && j < MAXUSERS; ++j, ++usr)
		{
			if (!usr->id) continue; //empty one

			if (usrl && (!usr->id || usrl != usr->id->len || _tmemcmp(user, usr->id->name, usrl) != 0))	continue;
			int sestop = usr->sestop;
			session* ses = &usr->sessions[0];
			for (int k = 0; k < sestop && k < MAXSESSIONS; ++k, ++ses)
			{
				if (ses->hispos == -1) continue; //empty one

				int histop = ses->histop;
				historia* his = &ses->history[0];
				for (int l = 0; l <= histop && l < MAXHISTORY; ++l, ++his)
				{
					mikefmt(&l0[2], _T("%d"), irow);
					int len = mikefmt(row, _T("[%sA:%d][%sB:%d][%sC:%d]")
						_T("[%sD:%s][%sE:%s][%sF:%s][%sG:%d]")
						_T("[%sH:%d][%sI:%d][%sJ:%d][%s*:%d]"),
						l0, i,
						l0, j,
						l0, k,
						l0, mac->name,
						l0, usr->id->name,
						l0, his->trn ? his->trn->name : _T("?"),
						l0, l,
						l0, ses->hispos,
						l0, ses->histop,
						l0, ses->online,
						l0, 0);
					strfinal.Append(row, len);
					++irow;
				}
			}
		}
	}
	int len = gen_tot_list(row, 0, irow);
	strfinal.Append(row, len);
	_params.append(strfinal);
}

void CSessionMan::gui_get_session_data()
{
	if (session* ses = process_session_ids())
	{
		maquina& mac = machines[macid];
		usuario& usr = mac.users[usrid];
		auto unam = usr.id->name;
		auto ulen = usr.id->len;

		CParameters guisession;
		guisession.set(ZUSERID, unam, ZUSERIDLEN, ulen);
		guisession.set(ZCOMPNY, ses->cmpy, ZCOMPNYLEN);

		TCHAR command[512];
		getconnectionx(con, obj);
		mikefmt(command, _T("exec dbo.user_get_desc_employee %d,'%s';"), ses->cmpy, unam);
		con.execute(command, obj);
		if (!obj.IsEOF()) {
			int l = obj.get(0, command);
			_params.set(_T("usrdesc"), command, 7, l);
			l = obj.get(1, command);
			_params.set(_T("usrcoms"), command, 7, l);
			l = obj.get(2, command);
			_params.set(_T("usrtype"), command, 7, l);
			l = obj.get(3, command);
			_params.set(_T("usrlevl"), command, 7, l);

			int empl = obj.getint(2);
			guisession.set(ZEMPLID, empl, ZEMPLIDLEN);
		}
		else {
			_params.set(_T("usrdesc"), _T("*"), 7, 1);
			_params.set(_T("usrcoms"), _T("*"), 7, 1);
			_params.set(_T("usrtype"), _T("*"), 7, 1);
			_params.set(_T("usrlevl"), _T("*"), 7, 1);
		}
		_params.set(ZUPDCLS, guisession, ZUPDCLSLEN); // goes inside true condition?

		/*
		mikefmt(command, _T("exec dbo.user_get_employee %d,'%s';"), ses->cmpy, unam);
		con.execute(command, obj);
		if (!obj.IsEOF())
		{
			int empl = obj.getint(0);
			guisession.set(ZEMPLID, empl, ZEMPLIDLEN);
		}
		_params.set(ZUPDCLS, guisession, ZUPDCLSLEN);

		//mikefmt(command, _T("exec dbo.maccfg_get_sestime '%s','%s';"), mac->name, user);

		// get some db user and system info
		mikefmt(command, _T("exec dbo.user_get_desc %d,'%s';"), ses->cmpy, unam);
		con.execute(command, obj);
		if (obj.IsEOF())
		{
			_params.set(_T("usrdesc"), _T("*"), 7, 1);
			_params.set(_T("usrcoms"), _T("*"), 7, 1);
			_params.set(_T("usrtype"), _T("*"), 7, 1);
			_params.set(_T("usrlevl"), _T("*"), 7, 1);
		}
		else
		{
			int l = obj.get(0, command);
			_params.set(_T("usrdesc"), command, 7, l);
			l = obj.get(1, command);
			_params.set(_T("usrcoms"), command, 7, l);
			l = obj.get(2, command);
			_params.set(_T("usrtype"), command, 7, l);
			l = obj.get(3, command);
			_params.set(_T("usrlevl"), command, 7, l);
		}*/

		// get other session data
		_params.set(_T("usrname"), unam, 7, ulen);
		_params.set(ZUSERID, unam, ZUSERIDLEN, ulen);
		_params.set(ZMACNAM, mac.name, ZMACNAMLEN, mac.naml);
		_params.set(ZIPADDR, mac.ip, ZIPADDRLEN, mac.adrl);
		_basics.copyto(_T("locaddr"), _params, ZGATSVR, 7, ZGATSVRLEN);
	}
}

__declspec(noinline)
void CSessionMan::gui_get_entrance()
{
	auto type = _params.getint(_T("entryid"), 7);
	// default standard company long pass
	auto code = const_cast<TCHAR*>(ZTRNPAS);
	auto cdln = ZTRNPASLEN;

	TCHAR sql[32];
	getconnectionx(con, obj);
	mikefmt(sql, _T("exec dbo.entry_get %d;"), type);
	con.execute(sql, obj);
	if (!obj.IsEOF())
	{
		cdln = obj.get(0/*1*/, sql);
		code = sql;
	}

	_params.set(ZRTRNCD, code, ZRTRNCDLEN, cdln);
	_params.set(ZFILE01, code, ZFILE01LEN, cdln);
	_params.set(ZTYPTRN, _T("trans"), ZTYPTRNLEN, 5);
	_get_file();
}

/*void CSessionMan::get_last_css()
{
	TCHAR lay[ZLAYOUTMAX+1];
	int		l = _basics.get(ZLAYOUT, lay, ZLAYOUTLEN, ZLAYOUTMAX);
	if(!l)	l = _params.get(ZLAYOUT, lay, ZLAYOUTLEN, ZLAYOUTMAX);
	if(!l)	set4ch(lay, 'S','T','D',0);

	TCHAR layout[64];
	int laylen = mikefmt(layout, _T("css%s"), lay);

	_params.set(ZFILE01, layout		, ZFILE01LEN, laylen);
	_params.set(ZTYPTRN,_T("layout"), ZTYPTRNLEN, 6		);
	_get_file();
}*/

/*static const TCHAR* ts[64] = {
	_T("t0"),_T("t1"),_T("t2"),_T("t3"),_T("t4"),_T("t5"),_T("t6"),_T("t7"),_T("t8"),_T("t9"),
	_T("t10"),_T("t11"),_T("t12"),_T("t13"),_T("t14"),_T("t15"),_T("t16"),_T("t17"),_T("t18"),_T("t19"),
	_T("t20"),_T("t21"),_T("t22"),_T("t23"),_T("t24"),_T("t25"),_T("t26"),_T("t27"),_T("t28"),_T("t29"),
	_T("t30"),_T("t31"),_T("t32"),_T("t33"),_T("t34"),_T("t35"),_T("t36"),_T("t37"),_T("t38"),_T("t39"),
	_T("t40"),_T("t41"),_T("t42"),_T("t43"),_T("t44"),_T("t45"),_T("t46"),_T("t47"),_T("t48"),_T("t49"),
	_T("t50"),_T("t51"),_T("t52"),_T("t53"),_T("t54"),_T("t55"),_T("t56"),_T("t57"),_T("t58"),_T("t59"),
	_T("t60"),_T("t61"),_T("t62"),_T("t63")
};
static int tslen[64] =   {
						2,2,2,2,2,2,2,2,2,2, 3,3,3,3,3,3,3,3,3,3,
						3,3,3,3,3,3,3,3,3,3, 3,3,3,3,3,3,3,3,3,3,
						3,3,3,3,3,3,3,3,3,3, 3,3,3,3,3,3,3,3,3,3,
						3,3,3,3
					};*/
					/*void CSessionMan::gui_get_texts()
					{
						TCHAR lang[ZLANGUAMAX+1];
						int		ld = _basics.get(ZLANGUA, lang, ZLANGUALEN, ZLANGUAMAX);
						if(!ld)	ld = _params.get(ZLANGUA, lang, ZLANGUALEN, ZLANGUAMAX);
						if(!ld)	{ set2ch(lang, 'E','N'); lang[2] = 0; ld = 2;}

						_basics.set(ZLANGUA, lang, ZLANGUALEN, ld);

						CString hlp;
						CString key;
						map<CString, CString>::iterator iter;

						TCHAR data[4096];
						data[0] = 0;
						int ldata = 0;
						TCHAR v[64+2+1];
						TCHAR desc[128];

						CParameters labels;
						_params.get(_T("descs"), labels, 5);
						int nlbs = labels.getint(_T("nt"), 2);
						require(nlbs > 64, _T("too_many_descs"));
						for(int i=0; i<nlbs; ++i)
						{
							int vl = labels.get(ts[i], v, tslen[i], 64);
							if(!vl) continue;

							int dl = _get_description(lang, ld, v, vl, desc, hlp);
							ldata += cpairs::gen_pair(0, &data[ldata], v, desc, vl, dl);
						}
						_params.set(_T("text_params"), data, 11, ldata);

						data[0] = 0;
						ldata = 0;
						_params.get(_T("errs"), labels, 4);
						nlbs =  labels.getint(_T("nt"), 2);
						require(nlbs > 64, _T("too_many_errs"));
						for(int i=0; i<nlbs; ++i)
						{
							int vl = labels.get(ts[i], v, tslen[i], 64);
							if(!vl) continue;

							int dl = _get_description(lang, ld, v, vl, desc, hlp);
							ldata += cpairs::gen_pair(0, &data[ldata], v, desc, vl, dl);
						}
						_params.set(_T("error_params"), data, 12, ldata);
					}*/

					/*void CSessionMan::update_local_session()
					{
					CParameters guisession;
					_params.copyto(_T("cuserini")	, guisession, ZUSERID	, 8, ZUSERIDLEN);
					_params.copyto(_T("coripass")	, guisession, ZORIPAS	, 8, ZORIPASLEN);
					//_params.copyto(_T("cpassword")	, guisession, ZPASSWR	, 9, ZPASSWRLEN);
					_params.copyto(_T("clanguage")	, guisession, ZLANGUA	, 9, ZLANGUALEN);
					_params.copyto(_T("clayout")	, guisession, ZLAYOUT	, 7, ZLAYOUTLEN);
					_params.copyto(_T("csesstime")	, guisession, ZSESTIM	, 9, ZSESTIMLEN);
					if (session* ses = process_session_ids())
					{
					maquina& mac = machines[macid];
					usuario& usr = mac.users[usrid];
					TCHAR command[128];
					{
					getconnectionx(con, obj);
					mikefmt(command, _T("exec dbo.user_get_employee '%s';"), usr.id->name);
					con.execute(command, obj);
					if (!obj.IsEOF())
					{
					int l = obj.get(0, command);
					guisession.set(ZEMPLID, command, ZEMPLIDLEN, l);
					}
					}
					}
					_params.set(ZUPDCLS, guisession, ZUPDCLSLEN);
					}*/