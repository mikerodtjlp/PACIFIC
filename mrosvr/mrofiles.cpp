#include "stdafx.h"

#include "SessionMan.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#undef THIS_FILE
static char THIS_FILE[] = __FILE__;
#endif

/************************************************************************************
* description   : mro server
* purpose       : execute any transacitions from clients
* author        : miguel rodriguez
* date creation : 20 junio 2004
* change        : 1 marzo
*                 call coms direct from the function execute instead of CMroCom::call_com)
* change        : 10 febrero
*                 change to global queue array, and the access controled by semaphores
**************************************************************************************/

// this is wrong, we temporary handle constant, this should be eliminated
TCHAR* lbl_beg = _T("<input class=\"mrolabel\"readonly=\"true\"tabindex=\"-1\"disabled=\"disabled\""); UINT llabel_beg = 70;
TCHAR* lbl_end = _T("</input>"); UINT llabel_end = 8;
TCHAR* img_beg = _T("<input type=\"image\"tabindex=\"-1\""); UINT limg_beg = 32;
TCHAR* img_end = _T("</input>"); UINT limg_end = 8;
TCHAR* dat_beg = _T("<input class=\"mrodate\"type=\"date\""); UINT ldate_beg = 33;
TCHAR* dat_end = _T("</input>"); UINT ldate_end = 8;
TCHAR* inp_beg = _T("<input class=\"mroinput\""); UINT linput_beg = 23;
TCHAR* inp_end = _T("</input>"); UINT linput_end = 8;
TCHAR* txt_beg = _T("<textarea class=\"mrotext\""); UINT ltext_beg = 25;
TCHAR* txt_end = _T("</textarea>"); UINT ltext_end = 11;
TCHAR* tbl_beg = _T("<div class=\"search-table-outter wrapper\"id=\"divtbl?\"><table cellspacing=\"1\""); UINT ltable_beg = 75;
TCHAR* tbl_end = _T("</table></div>"); UINT ltable_end = 14;
TCHAR* chk_beg = _T("<input class=\"mrocheck\"type=\"checkbox\""); UINT lcheck_beg = 38;
TCHAR* chk_end = _T("</input>"); UINT lcheck_end = 8;
TCHAR* dsc_beg = _T("<input class=\"mrodesc\"readonly=\"true\"tabindex=\"-1\"disabled=\"disabled\""); UINT ldesc_beg = 69;
TCHAR* dsc_end = _T("</input>"); UINT ldesc_end = 8;
TCHAR* btn_match_beg = _T("<button class=\"mrobtnmatch\"tabindex=\"-1\""); UINT lbtn_match_beg = 40;
TCHAR* btn_match_end = _T("</button>"); UINT lbtn_match_end = 9;
TCHAR* btn_tbar_beg = _T("<button class=\"mrobtntbar\"tabindex=\"-1\""); UINT lbtn_tbar_beg = 39;
TCHAR* btn_tbar_end = _T("</button>"); UINT lbtn_tbar_end = 9;
TCHAR* hdr_beg = _T("<input class=\"mrohdr\"readonly=\"true\"tabindex=\"-1\"disabled=\"disabled\""); UINT lheader_beg = 68;
TCHAR* hdr_end = _T("</input>"); UINT lheader_end = 8;

void rpl_tag(TCHAR* pbase,
	const TCHAR* tagname, const int taglen,
	CString& dest,
	const TCHAR* src, const int srclen)
{
	TCHAR* base = dest.GetBuffer();
	TCHAR* p = pbase ? pbase : (TCHAR*)_tcsstr(base, tagname);
	if (p)
	{
		int destlen = dest.GetLength();
		TCHAR* work = nullptr;
		bool dynamic = false;
		UINT wholesz = destlen + srclen;
		if (dynamic = (wholesz > 4096)) work = (TCHAR*)malloc(sizeof(TCHAR) * (wholesz + 1));
		else work = (TCHAR*)alloca(sizeof(TCHAR) * (wholesz + 1));

		int nchars = p - base;
		_tmemcpy(work, base, nchars);
		_tmemcpy(work + nchars, src, srclen);
		nchars += srclen;
		_tmemcpy(work + nchars, p + taglen, destlen - ((p - base) + taglen));
		nchars += destlen - ((p - base) + taglen);
		work[nchars] = 0;
		dest.SetString(work, nchars);

		if (dynamic) free(work);
	}
}
/**
 * checks if exists some tag (checking if its starts with <) and replace it
 * with sone predefiend patter of code containing mainly a specific class
 */
int prepare_line_to_xhtml(CString& dest) {
	// controls
	for (int i = 0, j = 0;; j = i + 1) {
		i = dest.Find(_T('<'), j);
		if (i == -1) break;					// no '<' means nothing to do 
		TCHAR* p = dest.GetBuffer() + i;	// cause dest could change we play safe
		TCHAR* q = p + 1;
		int l = 0;

		for (TCHAR* r = q, a = 0; l < 1024; ++l, ++r) {
			a = *r;
			if (a == '>' || a == ' ' || a == 0) break;
		}

		//if ((l == 4 && cmp4ch(q, 'h', 't', 'm', 'l')) ||
		//	(l == 5 && cmp4ch(q, 't', 'i', 't', 'l') && p[5] == 'e') ||
		//	(l == 4 && cmp4ch(q, 'b', 'o', 'd', 'y')) ||
		//	(l == 4 && cmp4ch(q, 'f', 'o', 'r', 'm')) ||
		//	(l == 3 && cmp2ch(q, 'd', 'i') && p[3] == 'v'))
		//	break;							// this are not process

		if (l == 3) continue; // div

		if (l == 5) {
			if (cmp4ch(q, 'i', 'n', 'p', 'u') && p[5] == 't') {
				rpl_tag(p, _T("<input"), 6, dest, inp_beg, linput_beg);
				continue;
			}
			if (cmp4ch(q, 'l', 'a', 'b', 'e') && p[5] == 'l') {
				rpl_tag(p, _T("<label"), 6, dest, lbl_beg, llabel_beg);
				continue;
			}
			if (cmp4ch(q, 'i', 'm', 'a', 'g') && p[5] == 'e') {
				rpl_tag(p, _T("<image"), 6, dest, img_beg, limg_beg);
				continue;
			}
			if (cmp4ch(q, '/', 'd', 'e', 's') && p[5] == 'c') {
				rpl_tag(p, _T("</desc>"), 7, dest, dsc_end, ldesc_end);
				continue;
			}
			if (cmp4ch(q, '/', 'd', 'a', 't') && p[5] == 'e') {
				rpl_tag(p, _T("</date>"), 7, dest, dat_end, ldate_end);
				continue;
			}
			if (cmp4ch(q, 'c', 'h', 'e', 'c') && p[5] == 'k') {
				rpl_tag(p, _T("<check"), 6, dest, chk_beg, lcheck_beg);
				continue;
			}
		}
		if (l == 6) {
			if (cmp4ch(q, '/', 'i', 'n', 'p') && cmp2ch(p + 5, 'u', 't')) {
				rpl_tag(p, _T("</input>"), 8, dest, inp_end, linput_end);
				continue;
			}
			if (cmp4ch(q, '/', 'l', 'a', 'b') && cmp2ch(p + 5, 'e', 'l')) {
				rpl_tag(p, _T("</label>"), 8, dest, lbl_end, llabel_end);
				continue;
			}
			if (cmp4ch(q, '/', 'i', 'm', 'a') && cmp2ch(p + 5, 'g', 'e')) {
				rpl_tag(p, _T("</image>"), 8, dest, img_end, limg_end);
				continue;
			}
			if (cmp4ch(q, 'h', 'e', 'a', 'd') && cmp2ch(p + 5, 'e', 'r')) {
				rpl_tag(p, _T("<header"), 7, dest, hdr_beg, lheader_beg);
				continue;
			}
			if (cmp4ch(q, '/', 'c', 'h', 'e') && cmp2ch(p + 5, 'c', 'k')) {
				rpl_tag(p, _T("</check>"), 8, dest, chk_end, lcheck_end);
				continue;
			}
		}
		if (l == 4) {
			if (cmp4ch(q, 'd', 'e', 's', 'c')) {
				rpl_tag(p, _T("<desc"), 5, dest, dsc_beg, ldesc_beg);
				continue;
			}
			if (cmp4ch(q, 'd', 'a', 't', 'e')) {
				rpl_tag(p, _T("<date"), 5, dest, dat_beg, ldate_beg);
				continue;
			}
		}
		if (l == 8) {
			if (cmp4ch(q, 'b', 't', 'n', 'm') && cmp4ch(p + 5, 'a', 't', 'c', 'h')) {
				rpl_tag(p, _T("<btnmatch"), 9, dest, btn_match_beg, lbtn_match_beg);
				continue;
			}
			if (cmp4ch(q, '/', 'b', 't', 'n') && cmp4ch(p + 5, 't', 'b', 'a', 'r')) {
				rpl_tag(p, _T("</btntbar>"), 10, dest, btn_tbar_end, lbtn_tbar_end);
				continue;
			}
			if (cmp4ch(q, 'm', 'r', 'o', 't') && cmp4ch(p + 5, 'a', 'b', 'l', 'e')) {
				rpl_tag(p, _T("<mrotable"), 9, dest, tbl_beg, ltable_beg);
				continue;
			}
			if (cmp4ch(q, 't', 'e', 'x', 't') && cmp4ch(p + 5, 'a', 'r', 'e', 'a')) {
				rpl_tag(p, _T("<textarea"), 9, dest, txt_beg, ltext_beg);
				continue;
			}
		}
		if (l == 9) {
			if (cmp4ch(q, '/', 'b', 't', 'n') && cmp4ch(p + 5, 'm', 'a', 't', 'c') && p[9] == 'h') {
				rpl_tag(p, _T("</btnmatch>"), 11, dest, btn_match_end, lbtn_match_end);
				continue;
			}
			if (cmp4ch(q, '/', 'm', 'r', 'o') && cmp4ch(p + 5, 't', 'a', 'b', 'l') && p[9] == 'e') {
				rpl_tag(p, _T("</mrotable>"), 11, dest, tbl_end, ltable_end);
				continue;
			}
			if (cmp4ch(q, '/', 't', 'e', 'x') && cmp4ch(p + 5, 't', 'a', 'r', 'e') && p[9] == 'a') {
				rpl_tag(p, _T("</textarea>"), 11, dest, txt_end, ltext_end);
				continue;
			}
		}
		if (l == 7) {
			if (cmp4ch(q, 'b', 't', 'n', 't') && cmp2ch(p + 5, 'b', 'a') && p[7] == 'r') {
				rpl_tag(p, _T("<btntbar"), 8, dest, btn_tbar_beg, lbtn_tbar_beg);
				continue;
			}
			if (cmp4ch(q, '/', 'h', 'e', 'a') && cmp2ch(p + 5, 'd', 'e') && p[7] == 'r') {
				rpl_tag(p, _T("</header>"), 9, dest, hdr_end, lheader_end);
				continue;
			}
		}

		/////
		if (cmp4ch(q, 'n', 'a', 'k', 'e') && cmp2ch(p + 5, 'd', '_'))
		{
			rpl_tag(p, _T("<naked_"), 7, dest, _T("<"), 1);
			continue;
		}
		if (cmp4ch(q, '/', 'n', 'a', 'k') && cmp2ch(p + 5, 'e', 'd') && p[7] == '_')
		{
			rpl_tag(p, _T("</naked_"), 8, dest, _T("</"), 2);
			continue;
		}
	}
	return dest.GetLength();
}

int CSessionMan::_get_description(const TCHAR* lang, const int ll,
																	const TCHAR* codeid, const int cl,
																	TCHAR* desc,
																	CString& key) {
	int dscl = 0;
	key.SetString(lang, ll);
	key.Append(codeid, cl);

	try	{
		::EnterCriticalSection(&dsclock);
		auto iter = cachedescs.lower_bound(key);
		if (iter != enddescs && !(cachedescs.key_comp()(key, iter->first))) {
			auto* d = &(*iter).second;
			dscl = d->GetLength();
			_tmemcpy(desc, d->GetBuffer(), dscl + 1);
		}
		else {
			TCHAR cmd[256];
			_tmemcpy(cmd, _T("exec dbo.desc_get '"), 19);
			_tmemcpy(&cmd[19], codeid, cl);
			set4ch(&cmd[cl + 19], _T('\''), _T(','), _T('\''), lang[0]);
			set4ch(&cmd[cl + 23], lang[1], _T('\''), _T(';'), 0);
			getconnectionx(con, obj);
			con.execute(cmd, obj);
			if (!obj.IsEOF()) dscl = obj.get(0, desc, 63);
			else { 
				_tmemcpy(desc, codeid, dscl = cl); 
				desc[cl] = 0; 
			}
			cachedescs.insert(map<CString, CString>::value_type(key, desc));
			enddescs = cachedescs.end();
		}
		::LeaveCriticalSection(&dsclock);
	}
	catch (_com_error& e) { ::LeaveCriticalSection(&dsclock); throw; }
	catch (CException* e) { ::LeaveCriticalSection(&dsclock); throw; }
	catch (mroerr&)				{ ::LeaveCriticalSection(&dsclock); throw; }
	catch (...)						{ ::LeaveCriticalSection(&dsclock); throw; }
	return dscl;
}

void CSessionMan::get_description() {
	TCHAR lang[ZLANGUAMAX + 1];	int ll = _basics.get(ZLANGUA, lang, ZLANGUALEN, ZLANGUAMAX);
	if (!ll) return;

	TCHAR code[64];
	int cl = _params.get(_T("codeid"), code, 6, 63);
	if (!cl) return;

	CString key;
	TCHAR desc[128];
	int dl = _get_description(lang, ll, code, cl, desc, key);
	_params.set(_T("realdesc"), desc, 8, dl);
}
/*
 * replaces all the language includes {@id} on the document, mostly transactions
 */
void CSessionMan::apply_lang(CString& dest) {
	static const CString notfound = _T("*");
	TCHAR rdesc[128];
	TCHAR param[128];

	TCHAR lang[3];
	int ll = _basics.get(ZLANGUA, lang, ZLANGUALEN, ZLANGUAMAX);
	if (!ll) return;

	const TCHAR* patt2 = _T("{@"); // pattern to find
	CString key;

	int lst = 0;
	int ini = 0;
	int fin = 0;
	for (;;) {
		TCHAR* base = dest.GetBuffer();
		int baselen = dest.GetLength();

		// pattern begin
		TCHAR* p = _tcsstr(base + lst, patt2);
		if (p) ini = p - base;
		else break;
		lst = ini;

		// pattern end
		p = _tmemchr(p + 2, _T('}'), baselen - ini);
		if (p) fin = p - base;
		else break;

		// extract the value of the pattern that is the code
		int lenp = (fin - ini) + 1;
		if (lenp >= 128) {
			CString err;
			if ((ini + 8) < dest.GetLength()) err.SetString(dest.GetBuffer() + ini, 8);
			requireex(true, _T("wrong_language_pattern"), err);
		}
		_tmemcpy(param, base + ini, lenp);
		param[lenp] = 0;

		// we form the key for the cache example: ENreg_not_exist
		int lenv = (fin - ini) - 2;
		int dl = _get_description(lang, ll, param + 2, lenv, rdesc, key);
		if (dl) dest.Replace(param, rdesc);
		else dest.Replace(param, notfound);
	}
}

/*
 * for every key found we replace the value, and if it has default value we
 * get rid of the default cause we have the value which is the mero mero
 */
void replace_from_prms(CParameters& prms, CString& dest) {
	TCHAR key[64];
	TCHAR val[1024];
	int lk = 0;
	int lv = 0;

	int nkeys = prms.nkeys();
	for (register int i = 0; i < nkeys; ++i) {
		if (prms.getpair(i, key, val, lk, lv, 63, 1023)) {
			int last = 0;
			for (int i = 0; i < 1024; ++i) {

				int f = dest.Find(key, last);
				if (f == -1) break;
				bool hasdef = dest.GetAt(f + lk) == '=' && dest.GetAt(f + lk + 1) == '|';
				if (hasdef) {
					int s = f + lk + 1; // first |
					int e = dest.Find('|', s + 1);
					if (e == -1) break;
					dest.Delete(s - 1, (e - s) + 2); // get rid of default
				}

				dest.Delete(f, lk);
				dest.Insert(f, val);
				last = f + lv;
			}
		}
	}
}

/**
 * generates the whole html code to represent a list
 */
void generate_list(const int id, CParameters& prms, CString& dest) {
	int len = prms.get_len();
	if (len == 0) return;

	int nrows = prms.getint(_T("nrows"), 5);
	int ncols = prms.getint(_T("ncols"), 5);

	TCHAR colname[3];
	TCHAR desc[64];
	TCHAR width[4];
	TCHAR cltotodd[32]; int cltotoddl = 0;
	TCHAR cltotevn[32]; int cltotevnl = 0;
	TCHAR helper[1024];

	dest.Format(_T("<colgroup id=\"colg%d\"span=\"%d\">\r\n"), id, ncols);
	len = mikefmt(helper, _T("<input type=\"hidden\"id=\"rowspertable%d\"value=\"%d\"></input>\r\n")
		_T("<tr class=\"rowheader\">\r\n"), id, nrows);
	CString rowhdr(helper, len);
	rowhdr.Append(_T("<td><input class=\"rowhdr\"readonly=\"true\"tabindex=\"-1\" ")
		_T("value=\"item\"maxlength=\"7\"size=\"7\"></input></td>\r\n"));
	CParameters coldata;
	TCHAR ca = 'A';
	TCHAR c2 = 'A';
	bool dcol = false;
	for (int i = 0; i < ncols; ++i)	{
		prms.get(mdefs::cols[i], coldata, mdefs::colslen[i]);
		coldata.get(_T("name"), desc, 4, 63);
		coldata.get(_T("width"), width, 5, 3);

		if (!dcol) { 
			set2ch(colname, ca++, 0); 
			if (ca == 91) dcol = true; 
		}
		else { 
			set2ch(colname, 'A', c2++); 
			colname[2] = 0; 
		}

		len = mikefmt(helper, _T("<col id=\"%s\"dsc=\"%s\"width=\"%s\"></col>\r\n"), colname, desc, width);
		dest.Append(helper, len);
		len = mikefmt(helper, _T("<td ondblclick=\"colondblclick('%d','%d');\"")
			_T("onclick=\"colonclick('%d','%d');\">")
			_T("<input class=\"rowhdr\"readonly=\"true\"tabindex=\"-1\"")
			_T("id=\"l%dc%s\"title=\"%s\"value=\"%s\"maxlength=\"%s\"size=\"%s\"></input></td>\r\n"),
			id, i,
			id, i,
			id, colname, desc, desc, width, width);
		rowhdr.Append(helper, len);
	}
	dest.Append(_T("</colgroup>\r\n"), 13);
	rowhdr.Append(_T("</tr>\r\n"), 7);

	for (int row = 0; row < nrows; ++row)	{
		bool isodd = row % 2 == 0;
		if (isodd) len = mikefmt(helper, _T("<tr class=\"rowodd\"onclick=\"on_lclick('%d','%d');\">\r\n"), id, row);
		else len = mikefmt(helper, _T("<tr class=\"roweven\"onclick=\"on_lclick('%d','%d');\">\r\n"), id, row);
		rowhdr.Append(helper, len);

		len = mikefmt(helper, _T("<td ondblclick=\"on_ldclick('%d','%d','itm');\">")
			_T("<img src=\"\"class=\"mroimgrow\"id=\"l%d%dimg\"style=\"visibility:hidden;\"/>")
			_T("<input class=\"rowitem\"readonly=\"true\"tabindex=\"-1\"disabled=\"disabled\"")
			_T("id=\"l%d%ditm\"maxlength=\"5\"size=\"5\"></input></td>\r\n"),
			id, row,
			id, row,
			id, row);
		rowhdr.Append(helper, len);

		TCHAR ca = 'A';
		TCHAR c2 = 'A';
		bool dcol = false;
		for (int col = 0; col < ncols; ++col) {
			prms.get(mdefs::cols[col], coldata, mdefs::colslen[col]);
			coldata.get(_T("width"), width, 5, 3);
			cltotoddl = coldata.get(_T("classod"), cltotodd, 7, 31);
			cltotevnl = coldata.get(_T("classev"), cltotevn, 7, 31);
			bool cl = cltotoddl && cltotevnl;

			if (!dcol) { 
				set2ch(colname, ca++, 0); 
				if (ca == 91) 
					dcol = true; 
			}
			else { 
				set2ch(colname, 'A', c2++); 
				colname[2] = 0; 
			}

			len = mikefmt(helper, _T("<td ondblclick=\"on_ldclick('%d','%d','%s');\">")
				_T("<input class=\"%s\"readonly=\"true\"tabindex=\"-1\"disabled=\"disabled\"")
				_T("id=\"l%d%d%s\"maxlength=\"%s\"size=\"%s\"></input></td>\r\n"),
				id, row, colname,
				cl ? (isodd ? cltotodd : cltotevn) : _T("rowitem"),
				id, row, colname, width, width);
			rowhdr.Append(helper, len);
		}
		rowhdr.Append(_T("</tr>\r\n"), 7);
	}
	dest.Append(rowhdr);
}
/**
 * read only one line from the whole string/object
 */
int read_line(const TCHAR* psrc, const int len, int& dbpos, CString& helper) {
	helper.Empty();
	int nread = -1;

	if (dbpos == -1) return nread;
	int left = len - dbpos;
	if (left <= 0) return nread;

	TCHAR* p = const_cast<TCHAR*>(psrc);
	TCHAR* r = _tmemchr(p + dbpos, '\r', left);
	int ini = r ? r - p : -1;

	if (ini == -1) {
		ini = len;
		nread = ini - dbpos;
		helper.SetString(p + dbpos, nread);
		dbpos = -1;
	}
	else {
		int advance = (r + 1 && *(r + 1) == '\n') ? 2 : 1;
		nread = (ini - dbpos) + advance;
		helper.SetString(p + dbpos, nread);
		dbpos = ini + advance;
	}
	return nread;
}


/**
 * after reading data from a the DB, we expand is macros, this can cause load
 * again new data and enter again in run this functoin recursively
 */
void CSessionMan::expand_macros(cConnection& con, 
																Table& obj,											// reuse same db connection
																CParameters& lparms,						// work with parameters
																const bool byforce,							// read from cache or DB
																map<CString, CString>& cache,		// objects cache
																set<CString>& procdocs,
															//const TCHAR* library,						// library
															//const int liblen,
																const TCHAR* dbsource,					// original copy from db
																const int sbsrclen,
															//const TCHAR* typ,								// object type 
															//const int typlen,
																CString& dest,									// target full string object
																const bool replacehtml) {					// some are not controls
	const int MAXIINCLUDEFILELEN = 128;
	TCHAR include[MAXIINCLUDEFILELEN + 1];
	TCHAR tail[128];
	CString helper;

	TCHAR lastinc[MAXIINCLUDEFILELEN + 1];
	int lastlen = 0;
	CString last;

	CParameters prms;
	int dbpos = 0;
	for (int safety = 0; safety < 4096; ++safety)	{
		int nread = -1;
		int incllen = 0;
		int taillen = 0;
		TCHAR* pincl = nullptr;

		if ((nread = read_line(dbsource, sbsrclen, dbpos, helper)) == -1) break;

		register TCHAR* phelper = helper.GetBuffer();

		// check we have potential include <@> or html
		register TCHAR* phlp = _tmemchr(phelper, _T('<'), nread);
		if (replacehtml && phlp) {
			if (!cmp2ch(phlp + 1, 'b', 'r') && !cmp4ch(phlp + 1, '/', 'b', 'r', '>') &&
				!cmp2ch(phlp + 1, '@', '>')   && *(phlp + 1) != ' ') {
				// if html substitue specically
				nread = prepare_line_to_xhtml(helper);
				phelper = helper.GetBuffer();
				phlp = _tmemchr(phelper, _T('<'), nread);
			}
		}

		if (phlp) { // check for includes <@>
			TCHAR* p = nullptr;
			int taglen = 0;
			if (cmp2ch(phlp + 1, '@', '>')) taglen = 3;
			if (taglen) p = phlp;
			int ini = p ? p - phlp : -1;

			if (ini != -1) {
				TCHAR* q = taglen == 3 ? _tcsstr(p + taglen, _T("</@>")) : nullptr;//?
				int fin = q ? q - phlp : -1;
				// in the include we must read the whole block not only one line in order 
				// to deal with includes that are to long to work with in one line
				if (fin == -1) {
					CString ayuda;
					for (int safe = 0; safe < 1024; ++safe) {
						int nnrd = -1;
						if ((nnrd = read_line(dbsource, sbsrclen, dbpos, ayuda)) != -1) {
							helper.Append(ayuda, nnrd);
							phlp = helper.GetBuffer(); // poiters could change

							q = _tcsstr(phlp, _T("</@>"));
							fin = q ? q - phlp : -1;
							if (fin == -1) continue;

							nread = helper.GetLength();
							phelper = helper.GetBuffer();

							p = _tcsstr(phlp, _T("<@>"));
							ini = p ? p - phlp : -1;
							if (ini == -1) fin = -1;
							break;
						}
						else break;
					}
				}

				// we have a include <@>
				if (fin != -1) {
					// get the tail \r\n
					taillen = nread - ((q + taglen) - phelper) - 1;
					if (taillen < 0) taillen = 0;
					if (taillen) {
						_tmemcpy(tail, q + taglen + 1, taillen);
						tail[taillen] = 0;
					}

					prms.clear();
					// lets is is the include comes with parameters
					p = _tcschr(p + taglen, CParameters::LEFT);
					int iniprms = p ? p - phlp : -1;
					if (iniprms != -1) {
						// get all the final end of the parameters
						for (TCHAR* r = p; r = _tcschr(r + 1, CParameters::RIGHT);) q = r;

						// get the parameters as object
						prms.set_value(p, (q - p) + 1);
						if (prms.is_bad_formed()) {
							dest.Append(helper.GetBuffer(), helper.GetLength());
							continue;
						}

						incllen = (iniprms - (ini + taglen));
					}    
					else incllen = (fin - (ini + taglen));

					// extracting the include/object ID
					if (incllen > MAXIINCLUDEFILELEN) incllen = MAXIINCLUDEFILELEN;
					if (incllen < 10) {
						TCHAR* pp = phlp + (ini + taglen);
						     if (incllen == 9) { set4ch(&include[0], pp[0], pp[1], pp[2], pp[3]); set4ch(&include[4], pp[4], pp[5], pp[6], pp[7]); include[8] = pp[8]; }
						else if (incllen == 8) { set4ch(&include[0], pp[0], pp[1], pp[2], pp[3]); set4ch(&include[4], pp[4], pp[5], pp[6], pp[7]); }
						else if (incllen == 7) { set4ch(&include[0], pp[0], pp[1], pp[2], pp[3]); set4ch(&include[4], pp[4], pp[5], pp[6], 0); }
						else if (incllen == 6) { set4ch(&include[0], pp[0], pp[1], pp[2], pp[3]); set2ch(&include[4], pp[4], pp[5]); }
						else if (incllen == 5) { set4ch(&include[0], pp[0], pp[1], pp[2], pp[3]); include[4] = pp[4]; }
						else if (incllen == 4) { set4ch(&include[0], pp[0], pp[1], pp[2], pp[3]); }
						else if (incllen == 3) { set4ch(&include[0], pp[0], pp[1], pp[2], 0); }
						else if (incllen == 2) { set2ch(&include[0], pp[0], pp[1]); }
						else if (incllen == 1) include[0] = pp[0];
					}
					else _tmemcpy(include, phlp + (ini + taglen), incllen);
					include[incllen] = 0;
					pincl = include;

					// handle trim left
					if (incllen && pincl[0] < 255 && isspace(pincl[0])) {
						TCHAR* e = &pincl[incllen - 1];
						for (; e != pincl && *pincl < 255 && isspace(*pincl); ++pincl, --incllen);
					}
					// handle trim right
					if (incllen && pincl[incllen - 1] < 255 && isspace(pincl[incllen - 1])) {
						TCHAR* p = &pincl[incllen];
						for (TCHAR* b = pincl; b != p && *(p - 1) < 255 && isspace(*(p - 1)); --p, --incllen);
						*p = 0;
					}

					auto hasprms = prms.notempty();

					// specific process for lists
					if (hasprms && prms.get(_T("genlist"), lparms, 7)) {
						//FIX divtbl?
						int lid = lparms.getint(_T("id"), 2);
						TCHAR fix[8]; set4ch(fix, 'd', 'i', 'v', 't'); set4ch(&fix[4], 'b', 'l', TCHAR(48 + lid), 0);
						dest.Replace(_T("divtbl?"), fix);
						//FIX divtbl?
						helper.Empty();
						generate_list(lid, lparms, helper);
					}
					else
					// embbedd the include into the current final object
					if (pincl) {
						if (incllen == lastlen && _tmemcmp(pincl, lastinc, incllen) == 0)
							helper = last;
						else {
							helper.Empty();

							// check if get data from store procedure or normal LIBDATA
							if (hasprms && prms.getbool(_T("qrysrc"), 6))
								read_file(con, obj, lparms, byforce, cache, procdocs, pincl, incllen,
									/*library, liblen,*/ _T(""), 0,      _T(""), 0, helper, replacehtml);
							else {
								const auto doneit = procdocs.find(CString(pincl, incllen)) != procdocs.end();
								read_file(con, obj, lparms, byforce && !doneit, cache, procdocs, _T(""), 0,
									/*library, liblen,*/ pincl, incllen, _T(""), 0, helper, replacehtml);
								if (!doneit) procdocs.insert(CString(pincl, incllen));
							}

							last = helper;
							if (incllen < 10) {
								TCHAR* pp = pincl;
								     if (incllen == 9) { set4ch(&lastinc[0], pp[0], pp[1], pp[2], pp[3]); set4ch(&lastinc[4], pp[4], pp[5], pp[6], pp[7]); lastinc[8] = pp[8]; }
								else if (incllen == 8) { set4ch(&lastinc[0], pp[0], pp[1], pp[2], pp[3]); set4ch(&lastinc[4], pp[4], pp[5], pp[6], pp[7]); }
								else if (incllen == 7) { set4ch(&lastinc[0], pp[0], pp[1], pp[2], pp[3]); set4ch(&lastinc[4], pp[4], pp[5], pp[6], 0); }
								else if (incllen == 6) { set4ch(&lastinc[0], pp[0], pp[1], pp[2], pp[3]); set2ch(&lastinc[4], pp[4], pp[5]); }
								else if (incllen == 5) { set4ch(&lastinc[0], pp[0], pp[1], pp[2], pp[3]); lastinc[4] = pp[4]; }
								else if (incllen == 4) { set4ch(&lastinc[0], pp[0], pp[1], pp[2], pp[3]); }
								else if (incllen == 3) { set4ch(&lastinc[0], pp[0], pp[1], pp[2], 0); }
								else if (incllen == 2) { set2ch(&lastinc[0], pp[0], pp[1]); }
								else if (incllen == 1) lastinc[0] = pp[0];
								lastlen = incllen;
							}
							else _tmemcpy(lastinc, pincl, lastlen = incllen);
							lastinc[lastlen] = 0;
						}
						if (hasprms) replace_from_prms(prms, helper);
					}
					if (taillen) helper.Append(tail, taillen);
					dest.Append(helper.GetBuffer(), helper.GetLength());

				}
				else dest.Append(helper.GetBuffer(), helper.GetLength());
			}
			else dest.Append(phelper, nread);
		}
		else dest.Append(phelper, nread);
	}
}

void CSessionMan::handle_defaults(CString& dest)
{
	CParameters* tmplparms = nullptr;
	int nkeys = 0;

	// load from DB parameters only (|..|)
	if (machines)
	{
		if (get_session_ids())
		{
			usuario& usr = machines[macid].users[usrid];
			if (usr.tmplparms)
			{
				tmplparms = usr.tmplparms;
				if (tmplparms->isempty())
				{
					TCHAR cmd[64];
					_tmemcpy(&cmd[0], _T("exec dbo.user_get_params '"), 26);
					_tmemcpy(&cmd[26], usr.id->name, usr.id->len);
					set4ch(&cmd[usr.id->len + 26], '\'', ';', 0, 0);
					TCHAR key[32];
					getconnectionx(con, obj);
					con.execute(cmd, obj);
					for (; !obj.IsEOF(); obj.MoveNext())
					{
						int kl = obj.get(0, key, 31);
						if (kl && key[0] == '|')
						{
							int vl = obj.get(1, cmd, 63);
							tmplparms->set(key, cmd, kl, vl);
						}
					}
					if (tmplparms->isempty())
						tmplparms->active(_T("calc"));
				}
				nkeys = tmplparms->nkeys();
			}
		}
	}

	// eliminate optionals and replace it with their defaults if any
	TCHAR parm[128 + 1];
	TCHAR key[64 + 1];
	TCHAR val[128 + 1];
	int lk = 0;
	int lv = 0;
	int last = 0;
	for (;;)
	{
		int l = dest.GetLength();
		int s = dest.Find('|', last);
		if (s == -1 || s >= l) break;
		int e = dest.Find('|', s + 1);
		if (e == -1) break;
		last = e + 1;
		int ll = (e - s) + 1;

		if (ll > 127) {	// too big probably garbage
			dest.Delete(s, ll);
			last = s;
			continue;
		}
		if (ll == 2) continue; // if deleted embedded javascript loses || operator
		//else
		//{
		bool containsdef = ((s + ll) < l) && dest[s + ll] == '=';
		bool delparam = true;
		bool usedefault = containsdef;

		_tmemcpy(parm, dest.GetBuffer() + s, ll); parm[ll] = 0;

		if (nkeys)
		{
			for (register int i = 0; i < nkeys; ++i)
			{
				if (tmplparms->getpair(i, key, val, lk, lv, 64, 128))
				{
					if (ll == lk && _tmemcmp(key, parm, lk) == 0)
					{
						dest.Delete(s, ll); // replace user db param
						dest.Insert(s, val);
						last = s + lv + 1;
						delparam = usedefault = false;
						break;
					}
				}
			}
		}

		if (containsdef)
		{
			int xl = dest.GetLength();
			int eq = dest.Find('=', s);
			if (eq == -1) continue;
			int xs = dest.Find('|', eq + 1);
			if (xs == -1 || xs >= xl) continue;
			int xe = dest.Find('|', xs + 1);
			if (xe == -1) continue;
			int xll = (xe - eq) + 1;
			if (xll > 127) // get rid garbage
			{
				dest.Delete(eq, xll);
				continue;
			}

			// extract and get rid of the default used or not
			lv = xll - 3;
			_tmemcpy(val, dest.GetBuffer() + xs + 1, lv);
			val[lv] = 0;
			dest.Delete(eq, xll);

			if (usedefault) // replace with the default
			{
				dest.Delete(s, ll);
				dest.Insert(s, val);
				last = s + lv + 1;
				continue;
			}
		}
		if (delparam)
		{
			if (s > 0 && dest[s - 1] == _T('=')) {
				dest.Delete(s - 1, ll + 1);
				last = s - 1;
			}
			else {
				dest.Delete(s, ll);  // just get rid of it
				last = s;
			}
		}
		//}
	}
}

/**
 * reads from the database the specific component, could be 2 ways at the moment
 * 1) normal: read from the master table LIBDATA through document_get_data
 * 2) specific store procedure: do whata ever it wants to create data
 */
void CSessionMan::load_fileDB(cConnection& con,
															Table& obj,
															CParameters& lparms,
															const bool byforce,
															map<CString, CString>& cache,
															set<CString>& procdocs,
															const TCHAR* query, const int qrylen,
															const TCHAR* library, const int liblen,
															const TCHAR* document, const int doclen,
															const TCHAR* typ, const int typlen,
															CString& dest,
															const bool replacehtml) {
	TCHAR* raw = nullptr;
	TCHAR _raw[8192];
	CString sraw;
	TCHAR cmd[256];
	bool isqrysrc = qrylen > 0;

	if (qrylen > 0) {
		if (get_session_ids()) {
			usuario& usr = machines[macid].users[usrid];
			session& ses = usr.sessions[sesid];
			mikefmt(cmd, _T("exec core.dbo.%s %d,'%s';"),
									query, ses.cmpy, usr.id->name);
		}
	}
	else 
		mikefmt(cmd,	_T("exec core.dbo.document_get_data '%s','%s','%s';"),
									library, document, typ);

	con.execute(cmd, obj);
	bool hasdata = !obj.IsEOF();
	requireex(!isqrysrc && !hasdata, _T("transaction_not_exist"), document);
	long rl = 0;
	if (hasdata) {
		rl = obj.getlong(1);
		if (rl < 8192) {
			rl = obj.get(0, _raw, 8191);
			raw = _raw;
		}
		else {
			rl = obj.get(0, sraw);
			raw = sraw.GetBuffer();
		}
	}

	requireex(!isqrysrc && !rl, _T("empty_document"), document);
	if (rl) expand_macros(con, obj, lparms, byforce, cache, procdocs,
		/*library, liblen,*/ raw, rl, /*typ, typlen,*/ dest, replacehtml);
}

/**
 * from the user library list we look for the specific document on top library
 */
int CSessionMan::_get_library(const TCHAR* document, const int doclen,
															const TCHAR* typ, const int typlen, 
															const TCHAR* deflib, const int defliblen,  
															TCHAR* lib) {
	int libl = 0;
	TCHAR cmd[256];

	if (typlen) // with type document
		mikefmt(cmd, _T("exec dbo.lib_get_top_type %d,%d,%d,%d,'%s','%s';"),
								 instid, macid, usrid, sesid, document, typ);
	else  // without type document
		mikefmt(cmd, _T("exec dbo.lib_get_top %d,%d,%d,%d,'%s';"),
								 instid, macid, usrid, sesid, document);

	{
		getconnectionx(con2, obj);
		con2.execute(cmd, obj);
		if (!obj.IsEOF()) libl = obj.get(0, lib, 15);
	}
	if (!libl) { // not found library we take at least the KERNEL one
		if(defliblen)
			_tmemcpy(lib, deflib, libl = defliblen);
		//else
		//	_tmemcpy(lib, _T("KERNEL"), libl = 6);
		lib[libl] = 0;
	}

	return libl;
}

/**
 * gets the document either from cache or from DB 
 */
void CSessionMan::read_file(cConnection& con,
														Table& obj,
														CParameters& lparms,
														const bool byforce,
														map<CString, CString>& cache,
														set<CString>& procdocs,
														const TCHAR* query, const int qrylen,
													//const TCHAR* library, const int liblen,
														const TCHAR* document, const int doclen,
														const TCHAR* typ, const int typlen,
														CString& strXML,
														const bool replacehtml) {
	int libl = 0;
	TCHAR lib[16];
	TCHAR cmd[256];
	int cl = 0;

	// generate full cache library id
	cl = mikefmt(cmd, _T("LIB%d%d%d%d%s%s"), instid, macid, usrid, sesid, document, typ);
	CString libkey(cmd, cl);

	if (byforce) {
		// library process
		libl = _get_library(document, doclen, typ, typlen, _T("KERNEL"), 6, lib);
		if (!_params.hasval(_T("library"), 7)) // only the first doc (??? find better way)
			_params.set(_T("library"), lib, 7, libl);

		map<CString, CString>::iterator iter = cache.lower_bound(libkey);
		if (iter != cache.end() && !(cache.key_comp()(libkey, iter->first)))
			(*iter).second = CString(lib, libl);
		else cache.insert(map<CString, CString>::value_type(libkey, CString(lib, libl)));

		// file process: LIBRARYTYPFILE sample KERNELCSScssBAS
		cl = 0;
		_tmemcpy(cmd, lib, libl);
		_tmemcpy(&cmd[libl], typ, typlen);		    cl = libl + typlen;
		_tmemcpy(&cmd[cl], document, doclen + 1); cl += doclen;
		CString dockey(cmd, cl);

		load_fileDB(con, obj, lparms, byforce, cache, procdocs, query, qrylen,
			lib, libl, document, doclen, typ, typlen, strXML, replacehtml);

		iter = cache.lower_bound(dockey);
		if (iter != cache.end() && !(cache.key_comp()(dockey, iter->first)))
			(*iter).second = strXML;
		else cache.insert(map<CString, CString>::value_type(dockey, strXML));
	}
	else {
		// library process
		map<CString, CString>::iterator iter = cache.lower_bound(libkey);
		if (iter != cache.end() && !(cache.key_comp()(libkey, iter->first))) {
			CString& it = (*iter).second;
			_tmemcpy(lib, it, libl = it.GetLength());
			lib[libl] = 0;
		}
		else {
			libl = _get_library(document, doclen, typ, typlen, _T("KERNEL"), 6, lib);
			if (!_params.hasval(_T("library"), 7)) // only the first doc (??? find better way)
				_params.set(_T("library"), lib, 7, libl);
			cache.insert(map<CString, CString>::value_type(libkey, CString(lib, libl)));
		}

		// file process: LIBRARYTYPFILE sample KERNELCSScssBAS
		cl = 0;
		_tmemcpy(cmd, lib, libl);
		_tmemcpy(&cmd[libl], typ, typlen);		    cl = libl + typlen;
		_tmemcpy(&cmd[cl], document, doclen + 1); cl += doclen;
		CString dockey(cmd, cl);

		iter = cache.lower_bound(dockey);
		if (iter != cache.end() && !(cache.key_comp()(dockey, iter->first)))
			strXML = (*iter).second;
		else {
			load_fileDB(con, obj, lparms, byforce, cache, procdocs, query, qrylen,
				lib, libl, document, doclen, typ, typlen, strXML, replacehtml);
			cache.insert(map<CString, CString>::value_type(dockey, strXML));
		}
	}
}

/*void CSessionMan::reset_document_cache()
{
	TCHAR document[ZFILE01MAX +1];
	int doclen = _params.get(_T("document"), document, 8, ZFILE01MAX);

	_params.set(ZFILE01, document, ZFILE01LEN, doclen);
	_params.set(ZTYPTRN, _T("trans"), ZTYPTRNLEN, 5);
	_params.set(ZTYPRED, _T("force"), ZTYPREDLEN, 5); // force to reload the data
	_get_file();

	TCHAR library[64];
	int liblen = _params.get(_T("library"), library, 7, 63);
	TCHAR document[64];
	int doclen = _params.get(_T("document"), document, 8, 63);
	CString key(library, liblen);
	key.Append(document, doclen);
	CString data;

	try
	{
		getconnectionx(con, obj);
		CParameters lparms;
		::EnterCriticalSection(&cstrans);
		load_file(con,obj,lparms,true,cache_docs,
			library, liblen, document, doclen, data,true);
		map<CString, CString>::iterator iter = cache_docs.lower_bound(key);
		if(iter != cache_docs.end() && !(cache_docs.key_comp()(key, iter->first)))
			(*iter).second = data;
		else cache_docs.insert(map<CString, CString>::value_type(key, data));
		::LeaveCriticalSection(&cstrans);
		return;
	}
	catch(const TCHAR* e)	{	::LeaveCriticalSection(&cstrans); throw; }
	catch(CString& e)		{	::LeaveCriticalSection(&cstrans); throw; }
	catch(_com_error &e)	{	::LeaveCriticalSection(&cstrans); throw; }
	catch(CException *e)	{ 	::LeaveCriticalSection(&cstrans); throw; }
	catch(mroerr& e)		{	::LeaveCriticalSection(&cstrans); throw; }
	catch(...)				{	::LeaveCriticalSection(&cstrans); throw; }
}*/

void CSessionMan::read_file(const bool byforce,
														map<CString, CString>& cache,
													//const TCHAR* lib, const int liblen,
														const TCHAR* document, const int doclen,
														const TCHAR* typ, const int typlen,
														CString& strXML,
														const bool replacehtml) {
	getconnectionx(con, obj);
	CParameters lparms;
	set<CString> procdocs;

	read_file(con, obj, lparms, byforce, cache, procdocs, _T(""), 0,
		/*lib, liblen,*/ document, doclen,
		typ, typlen, strXML, replacehtml);
}

/**
 * get the file and do some language subtitution from ids or from transaction name
 */
void CSessionMan::get_file(	//const TCHAR* lib, const int liblen,
														const TCHAR* document, const int doclen,
														const TCHAR* type, const int typlen,
														CString& rawXML,
														const TCHAR* typetrans, const int tytrlen,
														const bool byforce) {
	try	{
		if (tytrlen == 5 && cmp4ch(typetrans, 't', 'r', 'a', 'n') && typetrans[4] == 's') {
			TCHAR doc[ZFILE01MAX + 1];
			for (int i = 0; i < doclen; ++i) 
				doc[i] = towupper(document[i]);
			doc[doclen] = 0;

			::EnterCriticalSection(&cstrans);
			read_file(byforce, cache_docs, /*lib, liblen,*/ doc, doclen, type, typlen, rawXML, true);
			::LeaveCriticalSection(&cstrans);
			apply_lang(rawXML);
		}
		else
			/*if (tytrlen == 6 && cmp4ch(typetrans, 'l', 'a', 'y', 'o') && cmp2ch(&typetrans[4], 'u', 't'))*/ {
				::EnterCriticalSection(&cstrans);
				read_file(byforce, cache_docs, /*lib, liblen,*/ document, doclen, type, typlen, rawXML, false);
				::LeaveCriticalSection(&cstrans);
			}
		handle_defaults(rawXML);
	}
	catch (const TCHAR* e) { ::LeaveCriticalSection(&cstrans); throw; }
	catch (CString& e)		 { ::LeaveCriticalSection(&cstrans); throw; }
	catch (_com_error& e)  { ::LeaveCriticalSection(&cstrans); throw; }
	catch (CException* e)  { ::LeaveCriticalSection(&cstrans); throw; }
	catch (mroerr& e)      { ::LeaveCriticalSection(&cstrans); throw; }
	catch (...)            { ::LeaveCriticalSection(&cstrans); throw; }
}

/**
 * this function is executed from the clients, it gets the parameters and executes
 * the internal get_file generic function and put the content on the resulting
 * variable xfile01
 */
void CSessionMan::_get_file() {
	TCHAR typetrans[ZTYPTRNMAX + 1];
	int tytrlen  = _params.get(ZTYPTRN   , typetrans  , ZTYPTRNLEN, ZTYPTRNMAX);
	bool byforce = _params.are_eq(ZTYPRED, _T("force"), ZTYPREDLEN, 5);

	//TCHAR library[64];
	//int liblen = _params.get(_T("library"), library, 7, 63);

	TCHAR type[3 + 1];
	int typlen = _params.get(PDOCTYP, type, PDOCTYPLEN, PDOCTYPMAX);
	if (typlen == 0) {
		set4ch(type, 'T', 'R', 'N', 0); typlen = 3;
	}

	TCHAR document[ZFILE01MAX + 1];
	if (int doclen = _params.get(ZFILE01, document, ZFILE01LEN, ZFILE01MAX)) {
		CString rawXML;
		get_file(/*library, liblen,*/ document, doclen, type, typlen, rawXML, typetrans, tytrlen, byforce);
		_params.set(ZFILERS, rawXML, ZFILERSLEN, rawXML.GetLength());
	}
}
