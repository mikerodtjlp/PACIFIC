#include "stdafx.h"

//#include "mroctrl.h"
#include "SessionMan.h"

/*void CSessionMan::rights_process()
{
	int cmpy = _params.getint(_T("ccompany"), 8);
	TCHAR user[ZUSERIDMAX + 1];
	_params.get(_T("cuserini"), user, 8, ZUSERIDMAX);
	::EnterCriticalSection(&csrights);
	try
	{
		rights_process(cmpy, user);
		::LeaveCriticalSection(&csrights);
	}
	catch(_com_error &e){ ::LeaveCriticalSection(&csrights); throw; }
	catch(CException *e){ ::LeaveCriticalSection(&csrights); throw;	}
	catch(mroerr& e) 	{ ::LeaveCriticalSection(&csrights); throw;	}
	catch(...)			{ ::LeaveCriticalSection(&csrights); throw;	}
}*/

/** 
 * it's only function is to expand the trights and leave the trights2 with all the rights
 */
/*void CSessionMan::rights_process(const int cmpy, const TCHAR* user)
{
	TCHAR query[128];
	mikefmt(query, _T("exec dbo.rights_process %d,'%s';"), cmpy, user);
	getconnection(con);
	con.execute(query);
}*/
void CSessionMan::reset_desc_cache() {
	::EnterCriticalSection(&dsclock);
	cachedescs.clear();
	::LeaveCriticalSection(&dsclock);
}
void CSessionMan::process_descriptions() {
	reset_desc_cache();
}
void CSessionMan::process_user_params() {
	load_user_params();
}
/*void CSessionMan::reset_rights_cache()
{
	CString user;
	int userlen = _params.get(_T("cuserini"), user, 8);
	// we see if the cache is loaded and if it is not we load it
	::EnterCriticalSection(&csrights);
	try
	{
		if(cacherights.empty() == false)
		{
			if(userlen) cacherights.erase(user); // single user
			else cacherights.clear(); // all user
			endrights = cacherights.end();
		}
	}
	catch(CException *e){ ::LeaveCriticalSection(&csrights); throw;	}
	catch(...)			{ ::LeaveCriticalSection(&csrights); throw;	}
	::LeaveCriticalSection(&csrights);
}*/
/*void MROCOMPONENT::load_rights()
{
	TCHAR user[ZUSERIDMAX+1]; 
	_params.get(_T("cuserini"), user, 8, ZUSERIDMAX);
	::EnterCriticalSection(&csrights);
	try
	{
		load_rights_into_cache(user);
		::LeaveCriticalSection(&csrights);
	}
	catch(_com_error &e){ ::LeaveCriticalSection(&csrights); throw; }
	catch(CException *e){ ::LeaveCriticalSection(&csrights); throw;	}
	catch(mroerr& e) 	{ ::LeaveCriticalSection(&csrights); throw;	}
	catch(...)			{ ::LeaveCriticalSection(&csrights); throw;	}
}*/
/*void CSessionMan::load_rights_into_cache(const int cmpy, const TCHAR* user)
{
	// for all users
	map<CString, map<CString, CString>> all;
	map<CString, CString>* psing =  nullptr;
	auto allend = all.end();

	// for a single user
	map<CString, CString> single;

	TCHAR helper[1024];
	int hlplen;
	TCHAR passmod[128];
	int pmlen=0;

	CString last;
	CString usr;
	CString pamd;
	bool singleuser = false;
	CString prms;

	// we get the data for a particular user or for all
	TCHAR usercmd[512];
	mikefmt(usercmd, _T("exec dbo.rights_users_count %d,'%s%%';"), cmpy, user);
	TCHAR patrcmd[512];
	mikefmt(patrcmd, _T("exec dbo.rights_get_passmod %d,'%s%%';"), cmpy, user);

	// for release the connection as soon as possible
	{
		getconnectionx(con, obj);
		con.execute(usercmd, obj);
		int howmany = obj.getint(0);
		if(howmany == 0) return;
		singleuser = howmany == 1;

		con.execute(patrcmd, obj);
		for(;!obj.IsEOF();obj.MoveNext())
		{
			// get user
			if(!obj.get(0, usr)) continue; 
			// get pass + transaccion
			pmlen = obj.get(1, passmod);
			if(pmlen == 0) continue;
			pamd.SetString(passmod, pmlen);
			// get the parameters of the account
			hlplen = obj.get(2, helper);
			prms.SetString(helper, hlplen);

			if(singleuser) single.insert(map<CString, CString>::value_type(pamd, prms));
			else
			{
				if(last != usr)
				{
					map<CString, CString> sing;
					sing.insert(map<CString, CString>::value_type(pamd, prms));
					psing = &all.insert(map<CString, map<CString, CString>>::value_type(usr, sing)).first->second;
					allend = all.end();
				}
				else psing->insert(map<CString, CString>::value_type(pamd, prms));
			}
			last = usr;
		}
	}

	if(singleuser)
	{
		usr.SetString(last);
		auto iter = cacherights.lower_bound(usr);
		if(iter != endrights && !(cacherights.key_comp()(usr, iter->first)))
			(*iter).second = single;
		else
			cacherights.insert(map<CString, map<CString, CString>>::value_type(usr, single));
	}
	else cacherights = all;
	endrights = cacherights.end();
}*/

/**
 * we dont use stack memory cause it is a recursive function and if the call depth 
 * is too deep generate stack overflow, so we use (dynamic/CString) memory/safer
 */
void CSessionMan::helper_profile(	const TCHAR* main, const int mainl, 
																	const TCHAR* profile, const int proflen) {
	TCHAR objid[32];
	TCHAR trans[64];
	TCHAR right[128];

	CString command;
	_tmemcpy(&right[0], _T("exec dbo.profile_get '"), 22);
	_tmemcpy(&right[22], profile, proflen);
	set2ch(&right[22+proflen], _T('\''),0);
	getconnectionx(con, obj);
	con.execute(right, obj);
	for(;!obj.IsEOF();obj.MoveNext())
	{
		int lo = obj.get(0, objid, 31);	
		int lt = obj.get(1, trans, 63);	
		int lr = obj.get(2, right, 127);	

		if(lr == 2 && cmp2ch(right, CParameters::LEFT,CParameters::RIGHT)) 
			helper_profile(main, mainl, trans, lt); // this is the nested profile
		else {
			//execute mro_ins_profile_desglozado '%s','%s','%s'; ")	main, trans, right);
			command.Append(_T("execute mro_ins_profile_desglozado '"), 36);
			command.Append(main, mainl);
			command.Append(_T("','"), 3);
			command.Append(trans, lt);
			command.Append(_T("','"), 3);
			command.Append(right, lr);
			command.Append(_T("'; "), 3);
		}
	}
	if(command.IsEmpty() == false) con.execute(command.GetBuffer(), obj);
}

void CSessionMan::process_profiles() {
	TCHAR command[1024];

	TCHAR profile[16];
	_params.get(_T("cprofileini"), profile, 11, 15);
	defchar(filter,128); 
	if(profile[0]) { // specific was demanded
		mikefmt(command, _T("delete t_grps_dtl2 where object_id like '%s%%'"), profile);
		mikefmt(filter,  _T(" where object_id like '%s%%' or module like '%s%%'"), profile, profile);
	}
	else _tmemcpy(command, _T("delete t_grps_dtl2"), 19);

	getconnectionx(con, obj);
	con.execute(command);

	TCHAR objid[32];
	//mikefmt(command, _T("select object_id from t_grps_hdr with (nolock) %s ") , profilefilter);
	mikefmt(command, _T("select distinct object_id from t_grps_dtl with (nolock) %s ") , filter);
	con.execute(command, obj);
	for(;!obj.IsEOF();obj.MoveNext())	{
		int lo = obj.get(0, objid, 31);	
		helper_profile(objid, lo, objid, lo);
	}
}

/*void CSessionMan::check_access_type(){
	TCHAR user[ZUSERIDMAX + 1];
	int userlen = _params.get(_T("cuserini"), user, 8, ZUSERIDMAX);
	require(!userlen, _T("inc_dat_user"));

	TCHAR agent[32];
	int agentlen = _params.get(_T("guiname"), agent, 7, 31);
	require(!agentlen, _T("inc_dat_agent"));
	bool desk = cmp4ch(agent,'m','r','o','g') && cmp4ch(&agent[4],'u','i','2',0);
	bool brow = cmp4ch(agent,'b','r','o','w') && cmp4ch(&agent[4],'s','e','r',0);

	TCHAR command[128];
	mikefmt(command, _T("exec dbo.user_access_type '%s'"), user);
	getconnectionx(con, obj);
	con.execute(command, obj);
	require(obj.IsEOF(), _T("usr_access_type_not_registered"));
	TCHAR type[2];
	obj.get(0, type);

	require(type[0] != '0' && type[0] != '1' && type[0] != '2', _T("wrong_user_access_type"));

	if(type[0]=='2') return;

	require(desk && type[0] == '1', _T("cannot_enter_by_desktop"));
	require(brow && type[0] == '0', _T("cannot_enter_by_browser"));
}*/

void CSessionMan::exist_company(const TCHAR* cmpy, const int lcmpy) {
	require(!lcmpy, _T("inc_dat_cmpy"));
	TCHAR sql[128];
	mikefmt(sql, _T("exec dbo.cmpy_exists '%s';"), cmpy);
	getconnectionx(con, obj);
	con.execute(sql, obj);
	require(obj.getint(0) == 0, _T("cmpy_not_registered"));
}

void CSessionMan::exist_user(const TCHAR* user, const int userlen) {
	require(!userlen, _T("inc_dat_user"));

	bool wronguser = false;
	for(register int i = 0; i < userlen; ++i)	{
		register TCHAR letter = user[i];
		if(!isalpha(letter)) { 
			wronguser = true; 
			break; 
		}
	}
	require(wronguser, _T("wrong_user"));

	TCHAR sql[128];
	mikefmt(sql, _T("exec dbo.user_get_dates '%s';"), user);
	getconnectionx(con, obj);
	con.execute(sql, obj);
	require(obj.IsEOF(), _T("usr_not_registered"));
	_params.set(_T("cdatini"), obj.getdatedt(0), 7);
	_params.set(_T("cdatfin"), obj.getdatedt(1), 7);
}
void CSessionMan::user_cmpy_check(const TCHAR* cmpy, const TCHAR* user) {
	TCHAR sql[128];
	mikefmt(sql, _T("exec dbo.user_cmpy_check '%s','%s';"), cmpy, user);
	getconnectionx(con, obj);
	con.execute(sql, obj);
	require(obj.getint(0) == 0, _T("usr_cmpy_not_belong"));
}
void CSessionMan::detect_cmpy_id(const TCHAR* user) {
	TCHAR sql[128];
	mikefmt(sql, _T("exec dbo.cmpy_detect_id '%s';"), user);
	getconnectionx(con, obj);
	con.execute(sql, obj);
	require(obj.IsEOF(), _T("usr_not_company"));

	int	cmpy = obj.getint(0);
	_params.set(_T("company"), cmpy);
}
void CSessionMan::detect_user_id(const TCHAR* id) {
	TCHAR sql[128];
	mikefmt(sql, _T("exec dbo.user_get_credentials '%s';"), id);
	getconnectionx(con, obj);
	con.execute(sql, obj);
	require(obj.IsEOF(), _T("id_not_registered"));

	TCHAR user[ZUSERIDMAX + 1];
	int	luser = obj.get(0, user, ZUSERIDMAX);
	_params.set(_T("cuserini"), user, 8, luser);
}
void CSessionMan::can_enter() {
	// get the generic id (email/phone/id/companyid/windowid, etc...
	TCHAR id[64 + 1];
	int lid = _params.get(_T("cuserini"), id, 8, 64);
	require(!lid, _T("inc_dat_user"));

	// resolve the real user id from the generic id
	detect_user_id(id);
	// we make the real id as the real userid
	TCHAR user[ZUSERIDMAX + 1];
	int luser = _params.get(_T("cuserini"), user, 8, ZUSERIDMAX);
	require(!luser, _T("inc_dat_user"));
	_basics.set(ZUSERID, user, ZUSERIDLEN, luser);

	// get the company if any
	TCHAR cmpy[ZCOMPNYMAX + 1];
	int cmpylen = _params.get(_T("company"), cmpy, 7, ZCOMPNYMAX);
	// if not supply company id, get according the user
	if (cmpylen == 0) {
		detect_cmpy_id(user);
		cmpylen = _params.get(_T("company"), cmpy, 7, ZCOMPNYMAX);
	}
	_basics.set(ZCOMPNY, cmpy, ZCOMPNYLEN);

	// get the pass
	TCHAR pass[ZPASSWRMAX + 1];
	int lpass = _params.get(_T("cpassini"), pass, 8, ZPASSWRMAX);
	_params.set(ZPASSWR	, pass, ZPASSWRLEN, lpass);

	// simples checks
	exist_company(cmpy, cmpylen);
	exist_user(user, luser);
	user_cmpy_check(cmpy, user);

	TCHAR command[128];
	mikefmt(command, _T("exec dbo.user_check_pass'%s','%s';"), user, pass);
	{
		getconnectionx(con, obj);
		con.execute(command, obj);
		require(obj.IsEOF(), _T("invalid_password"));
	}

	//check_access_type();
	//have_right_pass(pass, lpass);

	COleDateTime today = COleDateTime::GetCurrentTime();
	COleDateTime dayini;
	COleDateTime dayfin;

	if(	!dayini.ParseDateTime(_params.get(_T("cdatini"), 7)) ||
		!dayfin.ParseDateTime(_params.get(_T("cdatfin"), 7)))	{
		require(true, _T("user_bad_configured"));
	}

	if(today < dayini) {
		requireex(true, _T("password_not_valid_until"), dayini.Format(_T("%Y/%m/%d a las %H:%M:%S")));
	}
	if(today > dayfin) {
		requireex(true, _T("password_not_expired"), dayfin.Format(_T("%Y/%m/%d a las %H:%M:%S")));
	}

	// necesary if the client needs the encrypted password, because here in the server 
	// is the only place where it is generated
	_values.set(_T("$company$"), cmpy);
	_values.set(_T("$_user$"), user, 7, luser);
	_values.set(_T("$encrypted$"), pass, 11, lpass);

	_params.set(ZPSTMSG	, _T("a")	, ZPSTMSGLEN, 1);
	_params.set(ZPSTMSI, _T("32850"), ZPSTMSILEN, 5); // message for goto main screen(entering the client)
}

int CSessionMan::obtain_rights(	const int cmpy, 
								const TCHAR* user, const int userlen,
								const TCHAR* tran, const int trnslen,
								TCHAR* rights)
{
	require(!userlen, _T("inc_dat_user"));
	require(!trnslen, _T("inc_dat_transaction"));

	TCHAR trns[ZFILE01MAX + 1];
	for (int i = 0;i < trnslen;++i) trns[i] = towupper(tran[i]);
	trns[trnslen] = 0;

	bool lookindb = false;
	TCHAR query[256];
	rights[0] = 0;
	int lr = 0;

	// at this point the user was not on the cache, and maybe neither on DB
	mikefmt(query, _T("exec dbo.right_get %d,'%s','%s';"), cmpy, user, trns);
	{
		getconnectionx(con, obj);
		con.execute(query, obj);
		if(!obj.IsEOF())
		lr = obj.get(2, rights);
	}

	/*lr = mikefmt(query, _T("%d%s"), cmpy, user);
	//CString key(user, userlen);
	CString key(query, lr);

	// we see if the cache is loaded and if it is not we load it
	::EnterCriticalSection(&csrights);
	try
	{
		if(cacherights.empty())
		{
			rights_process(cmpy, user);
			//load_rights_into_cache(cmpy, user);
		}

		// first we try on the cache
		auto iterrights = cacherights.lower_bound(key);
		if(iterrights != endrights && !(cacherights.key_comp()(key, iterrights->first)))
		{
			auto& rightscache = (*iterrights).second;
			key.SetString(trns, trnslen);

			auto iter2 = rightscache.lower_bound(key);
			if(iter2 != rightscache.end() && !(rightscache.key_comp()(key, iter2->first)))
			{
				CString& r = (*iter2).second;
				lr = r.GetLength();
				_tmemcpy(rights, r.GetBuffer(), lr + 1);
				::LeaveCriticalSection(&csrights);
				return lr;
			}
		}

		// at this point the user was not on the cache, and maybe neither on DB
		mikefmt(query,  _T("exec dbo.right_exists %d,'%s','%s';"), cmpy, user, trns);
		{
			getconnectionx(con, obj);
			con.execute(query,obj);
			lookindb = obj.getint(0) == 1;
		}

		if(lookindb)
		{
			rights_process(cmpy, user);
			//load_rights_into_cache(cmpy, user);

			key.SetString(user, userlen);
			std::map<CString, map<CString, CString>>::iterator iterrights = cacherights.lower_bound(key);
			if(iterrights != endrights && !(cacherights.key_comp()(key, iterrights->first)))
			{
				auto& rightscache = (*iterrights).second;
				key.SetString(trns, trnslen);

				std::map<CString, CString>::iterator iter2 = rightscache.lower_bound(key);
				if(iter2 != rightscache.end() && !(rightscache.key_comp()(key, iter2->first)))
				{
					CString& r = (*iter2).second;
					lr = r.GetLength();
					_tmemcpy(rights, r.GetBuffer(), lr + 1);
					::LeaveCriticalSection(&csrights);
					return lr;
				}
			}
		}
	}
	catch(_com_error &e)	{ ::LeaveCriticalSection(&csrights); throw; }
	catch(CException *e)	{ ::LeaveCriticalSection(&csrights); throw; }
	catch(mroerr& e)		{ ::LeaveCriticalSection(&csrights); throw; }
	catch(...)				{ ::LeaveCriticalSection(&csrights); throw; }

	::LeaveCriticalSection(&csrights);*/
	return lr;
}
/**
 * note:
 * there are two kind of tables trights, whose it is used to stored the rights and 
 * its profiles and the rights whose has full of the rights and all the rights in the 
 * profiles there is a tricky part: most of the time this is executed from the client 
 * and the parameters comes from the basics, but there are situation where it does 
 * not, for example the password transaction because if we havent enterd we dont have 
 * basics ones
 */
void CSessionMan::have_right() { CALLSTACK
	// from inside the session
	int scmpy = _basics.getint(ZCOMPNY, ZCOMPNYLEN, -1);

TCHAR suser[ZUSERIDMAX + 1];
	TCHAR smodn[128];
	int suserlen = _basics.get(ZUSERID, suser, ZUSERIDLEN, ZUSERIDMAX);
	int stranlen = _basics.get(ZTRNCOD, smodn, ZTRNCODLEN, 127);

	// from outside the session
	int ccmpy = _basics.getint(_T("ccompany"), 8, -1);
	TCHAR cuser[ZUSERIDMAX+1];	
	int lenuser = _params.get(_T("cuserini"), cuser, 8, ZUSERIDMAX);

	TCHAR rmod[128];				
	TCHAR type[4];						
	TCHAR rigt[ZRIGHT1MAX+1];	
	
	int rmodlen = _params.get(ZRTRNCD, rmod, ZRTRNCDLEN, 127);
	int typrlen = _params.get(ZTYPRGT, type, ZTYPRGTLEN, 3);
	int rigtlen = _params.get(ZRIGHT1, rigt	, ZRIGHT1LEN, ZRIGHT1MAX);

	// decide inside or outside the session
	int cmpy = ccmpy == -1 ? scmpy : ccmpy;
	TCHAR* user	= cuser[0] == 0 ? suser	: cuser;
	int userlen = cuser[0] == 0 ? suserlen : lenuser;

	TCHAR* module			= rmod[0]	== 0 ? smodn		: rmod;
	rmodlen						= rmod[0]	== 0 ? stranlen	: rmodlen;
	TCHAR* right2find	= rigt[0]	== 0 ? type			: rigt;
	rigtlen						= rigt[0]	== 0 ? typrlen	: rigtlen;

	require(!userlen, _T("inc_dat_user"));
	require(!rmodlen, _T("inc_dat_transaction"));
	require(!rigtlen, _T("inc_dat_right"));

	TCHAR rights[1024];
	int l = obtain_rights(cmpy, user, userlen, module, rmodlen, rights);
	CParameters prms(rights, l);

	// does the user has the target specific right?
	bool haverights = prms.getint(right2find, rigtlen);

	// if we have problems with the require rights we notify as clear as posible
	if(!haverights)	{
		// no library means transaction does not exist
		TCHAR lib[16];
		int libl = _get_library(module, rmodlen, _T("TRN"), 3, _T(""), 0 , lib);
		requireex(libl == 0, _T("transaction_not_exist"), module);

		//TCHAR query[128];
		//mikefmt(query, _T("exec dbo.trans_exists '%s';"), module);
		//{
		//	getconnectionx(con, obj);
		//	con.execute(query,obj);
		//	requireex(obj.getint(0) == 0, _T("transaction_not_exist"), module);
		//}

		requireex(cmp4ch(right2find, 'a','c','c',0),_T("not_registerd_2_trans"), module);
		require(true, _T("insufficient_rights"));
	}
}

/**
 *	the purpose of this function is not to check the rights just get if any
 *	and if it does not, well is correct result, there is not rights that's it
 *	and it seems or at least it should this function be executed by a service only
 *	because thats the only way the the parameters comes from the basics ones
 */
void CSessionMan::look_4_rights() { CALLSTACK
	int cmpy = _basics.getint(ZCOMPNY, ZCOMPNYLEN, -1);
	TCHAR user[ZUSERIDMAX + 1];	
	TCHAR trns[64];				
	
	int ulen = _basics.get(ZUSERID, user, ZUSERIDLEN, ZUSERIDMAX);
	int tlen = _basics.get(ZTRNCOD, trns, ZTRNCODLEN, 65);

	// not always is possible to get the rights, for example when we are entering, 
	// we dont know who we are, when we are in the main screen a check rights for 
	// entering to a new transaction(we are not in that transaction), other example
	// would be satellites systems that do they work a little diferent
	defchar(rights, 1024);
	int lr = 0;
	if (ulen && tlen) lr = obtain_rights(cmpy, user, ulen, trns, tlen, rights);

	TCHAR temp[256];
	int l = 0;
	temp[l++] = cpairs::LEFT;
	_tmemcpy(&temp[l], ZURGTSZ, ZURGTSZLEN); l += ZURGTSZLEN;
	set4ch(&temp[l], cpairs::SEP,cpairs::LEFT,'p','r'); l += 4;
	set4ch(&temp[l], 'e',cpairs::SEP,'1',cpairs::RIGHT); l += 4;
	if(lr) {_tmemcpy(&temp[l], rights, lr); l += lr; }
	set2ch(&temp[l], cpairs::RIGHT,0); ++l;

	_params.append(temp, l);
}

int encrypt_pass(const TCHAR* pass, const int len, TCHAR* encpass) {
	for(int i = 0; i < len; ++i) {
		TCHAR lttr = pass[i];
		if(lttr == 65) encpass[i] = _T('!');
		else
		if(lttr == 97) encpass[i] = _T('*');
		else encpass[i] = lttr - 1;
	}
	encpass[len]=0;
	return len;
}

void CSessionMan::encrypt_password() {
	TCHAR oripass[ZPASSWRMAX + 1];
	int l = _params.get(_T("cpassini"), oripass, 8);
	TCHAR encpass[ZPASSWRMAX + 1];
	l = encrypt_pass(oripass, l, encpass);
	_values.set(_T("$encpassword$"), encpass, 13, l);
}

void CSessionMan::_validate_pass() {
	TCHAR pass[ZPASSWRMAX + 1];
	int lenpass = _params.get(_T("cnewpas"), pass, 7, ZPASSWRMAX);

	require(lenpass < 6	, _T("password_at_least_6_chars_long"));
	require(lenpass > 16, _T("password_no_more_16_chars_long"));

	bool wrongcars	= false;
	bool havecars	= false;
	bool havenums	= false;

	for (int i = 0; i < lenpass; ++i)	{
		if(isalpha(pass[i])) { havecars = true; continue; }
		if(isdigit(pass[i])) { havenums = true; continue; }
		wrongcars = true;
	}

	require(wrongcars , _T("password_only_letters_numbers")); 
	require(!havecars , _T("password_at_least_char_az"));
	require(!havenums , _T("password_at_least_num_09")); 
}
