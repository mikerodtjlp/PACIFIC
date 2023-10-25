// mroparms.cpp : implementation file

/**
 *	created	:	2001/10/25
 *	file		:	mroparms
 *	author	:	miguel rodriguez ojeda
 *	purpose	:	like a map<> but coud be send between boundaries?
 */

#include "stdafx.h"

#define GENKEY(llave, key, keylen)\
		llave[0] = L0;\
		_tmemcpy(&llave[1], key, keylen);\
		set2ch(&llave[keylen + 1], SP, _T('\0'));

#define GENKEY_STA(llave, key, keylen, left, sep)\
		llave[0] = left;\
		_tmemcpy(&llave[1], key, keylen);\
		set2ch(&llave[keylen + 1], sep, _T('\0'));

namespace mro {

CParameters::CParameters() { 
	init(); 
	set_data(_T(""), 0);
}
void CParameters::set_value(CParameters& extra) {
	clear(); 
	append(extra);
	require(is_bad_formed(), _T("wrong_params_structure"));
}
//CParameters::CParameters(CParameters& extra) { 
//	init(); 
//	append(extra);
//	require(is_bad_formed(), _T("wrong_params_structure"));
//}
CParameters::CParameters(CString& params) { 
	init(); 
	set_data(params.GetBuffer(), params.GetLength()); 
	require(is_bad_formed(), _T("wrong_params_structure"));
}
CParameters::CParameters(const TCHAR* params) { 
	init(); 
	set_data(params, _tcslen(params));				
	require(is_bad_formed(), _T("wrong_params_structure"));	
}
CParameters::CParameters(const TCHAR* params, const int len) { 
	init(); 
	set_data(params, len);							
	require(is_bad_formed(), _T("wrong_params_structure"));	
}
CParameters::CParameters(const TCHAR* key, const TCHAR* val, 
				const int keylen, const int vallen) { 
	init(); 
	unoptimize(); 
	set(key, val, keylen, vallen); 
	optimize(); 
	require(is_bad_formed(), _T("wrong_params_structure"));	
}
CParameters::CParameters(const TCHAR* key, const int val, 
				const int keylen) { 
	init(); 
	unoptimize(); 
	set(key, val, keylen); 
	optimize(); 
	require(is_bad_formed(), _T("wrong_params_structure"));	
}
// this function only is trigger when there is a stream of caracter where is
// no direct form of knowing what kind of type is mro or json
//void CParameters::check_type()
//{
//	const TCHAR* p = buffer();
//	for(TCHAR car; car = *p; ++p) if(car == L0) { isjson = true; break; }
//}

void CParameters::init() {	
//	L0 = PAIRLEFT;
//	SP = PAIRSEP;
//	R0 = PAIRRIGHT;
//	NA = PAIRNADA;

	optmax = 0;
	isoptimized = true; // we can save do this because is empty
}

/*ALWAYS_INLINE size_t
binary_search (unsigned key, unsigned * vector, size_t size)
{
		unsigned * low = vector;

		for (unsigned i = lb(size); i != 0; i--) {
				size /= 2;
				unsigned mid = low[size];
				if (mid <= key)
						low += size;
		}

		return (*low > key)? -1: low - vector;
}*/

const TCHAR*  CParameters::get_opt_pos(const TCHAR* key, 
												const UINT keylen,
												int& pairlen,
												TCHAR*& valpos, 
												UINT& vallen,
												int& optpos) {
	const TCHAR* data = get_data();
	int nels = optmax > MAXOPTIMIZATION ? MAXOPTIMIZATION : optmax;
	for(register int i = nels-1; i >= 0; --i) // MAXOPTIMIZATION our safety break
	{
		if(optlen[i] != keylen) continue;
		UINT beg = optbeg[i];
		const TCHAR* pp = &data[beg];
		if(cfuns[keylen](pp, key)) { 
			UINT end = optend[i];
			pairlen=end-beg+3; 
			vallen=end-(beg+keylen); 
			valpos = const_cast<TCHAR*>(pp + keylen + 1);
			optpos = i;
			return pp-1; 
		}
	}
	pairlen = 0;
	vallen = 0;
	valpos = nullptr;
	optpos = -1;
	return nullptr;
}

int CParameters::nkeys() { 
	return isoptimized ? optmax : -1;						
}

void CParameters::unoptimize() { 
	isoptimized = false; 
	optmax = 0;						
}

void CParameters::optimize() {
	if(isoptimized) return;

	int pos = 0;
	int in = 0;
	optmax = 0;

	const TCHAR* l = nullptr;
	const TCHAR* s = nullptr;
	register TCHAR car;
	for(register const TCHAR* p = get_data(); (car=*p); ++p, ++pos)	{
		//if(in==0 && (car < 255 && isspace(car))) continue;
		if(car == L0) {
			if(in++==0) { 
				if(optmax >= MAXOPTIMIZATION) { 
					isoptimized = optmax = 0; 
					return; 
				}
				l=p; 
				optbeg[optmax] = pos+1;
			}
			continue;
		}
		else if(car == SP) {
			if(in==1 && !s) {
				s=p;
				optlen[optmax++] = (p-l)-1;
			}
			continue;
		}
		else if(car == R0) {
			if(--in==0) {
				s=nullptr;
				if(optmax)
					optend[optmax-1] = pos-1;
			}
		}
	}
	isoptimized = (bool)optmax;
}

bool CParameters::add_optimization(const int keylen, const int newlen) {
	if(isoptimized && optmax < MAXOPTIMIZATION && keylen > 0)	{
		optbeg[optmax] = get_length() + 1;
		optlen[optmax] = keylen;
		optend[optmax] = optbeg[optmax] + newlen - 3; 
		++optmax;
		return true;
	}
	return false; 
}

void CParameters::append(CParameters& extra) { 
	bool wasopt = false;
	if(	isoptimized && extra.isoptimized && 
		((extra.optmax + optmax) < MAXOPTIMIZATION)) {
		int offset =  get_len();
		for(int i = 0; i < extra.optmax; ++i) {
			int j = optmax+i;
			optlen[j] = extra.optlen[i];
			optbeg[j] = extra.optbeg[i] + offset;
			optend[j] = extra.optend[i] + offset;
		}
		optmax += extra.optmax;
		wasopt = true;
	}
	append_data(extra.get_data(), extra.get_len(), wasopt);
}

/**
 * tricky function, used only for performance requirements, cause it asummes
 * correct the lenght of the value, but could be wrong, due we read the key's 
 * value on the memory reserve for the values lenght for the parameters
 */
void CParameters::del(const TCHAR* key, const UINT keylen) {
	int pairlen = 0; TCHAR* pval = nullptr; UINT vallen = 0; int optpos = 0;
	const TCHAR* p = get_start(key, keylen, pairlen, pval, vallen, optpos);	
	if(!p) return;
	if(vallen >= get_len()) return;

	TCHAR* px = const_cast<TCHAR*>(p); 
	_tmemset(px, NA, pairlen);

	if(optpos != -1)
		optbeg[optpos] = optlen[optpos] = optend[optpos] = 0;
}

void CParameters::compact(const bool dooptimize) {
	TCHAR* b	= get_data();
	TCHAR* q	= b;
	int deletes	= 0;
	TCHAR car	= 0;
	int in		= 0;

	TCHAR* l	= nullptr;
	TCHAR* s	= nullptr;
	TCHAR* r	= nullptr;

	for(register TCHAR* p=b; (car=*p); ++p) {
		if(car == L0) { 
			*q++ = car;
			if(in++==0) l=p;
			continue; 
		}
		else if(car == SP) {
			*q++ = car;
			if(in==1 && !s) s=p;
			continue;
		}
		else if(car == R0) {
			*q++ = car;
			if(--in==0) {
				l = nullptr;
				r = nullptr;
				s = nullptr;
			}
			continue;
		}
		if(!l && !s && !r && (car < 255 && isspace(car))) ++deletes;
		else *q++ = car;
	}

	if(deletes)	{
		set_length(q-b);
		if(dooptimize) optimize();
	}
}

int CParameters::get_pair_len(const TCHAR* key, const UINT keylen)
{
	int len = 0;
	UINT vallen = 0;	TCHAR* pval = nullptr; int optpos = 0;
	const TCHAR* start = get_start(key, keylen, len, pval, vallen, optpos);
	if(start) return len;
	return 0;
}

const TCHAR* CParameters::get_start(	const TCHAR* key, const UINT keylen, 
												int& pairlen,
												TCHAR*& pval, UINT& vallen,
												int& optpos)
{
	if(isoptimized) return get_opt_pos(key, keylen, pairlen, pval, vallen, optpos);

	optpos = -1;

	if(int len = get_len())
	{
		const TCHAR* l=nullptr;
		const TCHAR* s=nullptr;
		const TCHAR* r=nullptr;

		register const TCHAR* p = get_data();
		register const TCHAR* q = p+len;

		for(register int in = 0; p<q; ++p) 
		{
			register TCHAR car = *p;
			if(car < SP || car > R0) goto second;
			if(car == L0)	{	if(in++==0) l=p+1;	goto second; 	}
			if(car == SP)	{	if(in==1 && !s) s=p;goto second;	}
			if(car == R0)
			{
				if(--in==0)
				{
					r = p-1;
					size_t sz=(s-l);
					const TCHAR* ss = s;
					s = nullptr;
					if(sz!=keylen) goto second;

					if(cfuns[keylen](l,key))
					{
						pairlen = (r-l)+3;
						vallen = r-(l+keylen);
						pval = vallen ? const_cast<TCHAR*>(ss+1) : nullptr;
						return l-1;
					}
				}
			}
second:
			if(++p==q) break;
			car = *p;
			if(car < SP || car > R0) goto third;
			if(car == L0)	{	if(in++==0) l=p+1;	goto third; }
			if(car == SP)	{	if(in==1 && !s) s=p;goto third;	}
			if(car == R0)
			{
				if(--in==0)
				{
					r = p-1;
					size_t sz=(s-l);
					const TCHAR* ss = s;
					s = nullptr;
					if(sz!=keylen) goto third;

					if(cfuns[keylen](l,key))
					{
						pairlen = (r-l)+3;
						vallen = r-(l+keylen);
						pval = vallen ? const_cast<TCHAR*>(ss+1) : nullptr;
						return l-1;
					}
				}
			}
third:
			if(++p==q) break;
			car = *p;
			if(car < SP || car > R0) goto fourth;
			if(car == L0)	{	if(in++==0) l=p+1;	goto fourth; 	}
			if(car == SP)	{	if(in==1 && !s) s=p;goto fourth;	}
			if(car == R0)
			{
				if(--in==0)
				{
					r = p-1;
					size_t sz=(s-l);
					const TCHAR* ss = s;
					s = nullptr;
					if(sz!=keylen) goto fourth;

					if(cfuns[keylen](l,key))
					{
						pairlen = (r-l)+3;
						vallen = r-(l+keylen);
						pval = vallen ? const_cast<TCHAR*>(ss+1) : nullptr;
						return l-1;
					}
				}
			}
fourth:
			if(++p==q) break;
			car = *p;
			if(car < SP || car > R0) continue;
			if(car == L0)	{	if(in++==0) l=p+1;	continue; 	}
			if(car == SP)	{	if(in==1 && !s) s=p;continue;	}
			if(car == R0)
			{
				if(--in==0)
				{
					r = p-1;
					size_t sz=(s-l);
					const TCHAR* ss = s;
					s = nullptr;
					if(sz!=keylen) continue;

					if(cfuns[keylen](l,key))
					{
						pairlen = (r-l)+3;
						vallen = r-(l+keylen);
						pval = vallen ? const_cast<TCHAR*>(ss+1) : nullptr;
						return l-1;
					}
				}
			}
		}
	}

	pairlen = 0;
	vallen = 0;
	pval = nullptr;
	return nullptr;
}

// replace/update every parameter in the string and if it does not exist it'll be created
void CParameters::rpl_from(const TCHAR* newparams, const int len)
{
	if(newparams[0] == _T('\0')) return;

	// create enoguh data to hold the worst case that is none is in the other
	size_t sourcelen	= len == 0 ? _tcslen(newparams) : len;
	size_t currentlen	= get_len();
	TCHAR* presult		= nullptr;
	bool dynamic		= false;
	UINT wholesz = sourcelen + currentlen + 2048;
	if(dynamic = wholesz > 4096) presult = (TCHAR*)malloc(sizeof(TCHAR)*(wholesz+1));
	else presult = (TCHAR*)alloca(sizeof(TCHAR)*(wholesz+1)); 
	TCHAR* p			= presult;

	const TCHAR* source = newparams;
	const TCHAR* base	= get_data();
	const TCHAR* final  = base + currentlen;

	TCHAR prm[64];

	for(;*base;)
	{
		// buscamos el parametro
		TCHAR* start = (TCHAR*) _tmemchr(base,  L0, (size_t)(final - base));
		if(!start) break;
		TCHAR* sepa  = (TCHAR*) _tmemchr(start, SP, (size_t)(final - start));
		if(!sepa)   break;

		base = sepa + 1;
		size_t lenkey = sepa - start - 1;
		_tmemcpy(prm, start, lenkey+2);
		prm[lenkey+2] = 0;

		TCHAR* end = 0;
		int lefts =1; // this is for the begining ([)key:val]   
		int rights = 0;
		TCHAR car;
		for(end = sepa + 1; car = *end; ++end)
		{
			if(car == L0) ++lefts;
			if(car == R0) ++rights;
			if(lefts == rights) break;
		}
		if(end > final) { break; }; // just for security

		size_t ln = (end - start) + 1;
		base = start + ln;

		//checamos si el el parametro esta en los nuevos
		TCHAR* exist = (TCHAR*)_tcsstr(source, prm);
		if(!exist)
		{
			_tmemcpy(p, start, ln);
			p += ln;
		}
	}

	// + pasamos todos los nuevos valores ---------------------
	_tmemcpy(p, source, sourcelen); //	mikecpy(p, source, sourcelen);
	p += sourcelen;
	*p = '\0';
	// - pasamos todos los nuevos valores ---------------------

	set_data(presult, (p - presult));

	if(dynamic) free(presult);
}

/**
 * take the values and exchange them for their values on the fields (if any)
 * note: is not necesary that the parameters come optimized because the function 
 * dont use any function affected for the optimization
 */
void CParameters::merge(CParameters& source, CParameters& values)
{
	TCHAR key[64];
	CString val;
	CString value;
		
	TCHAR* mainp = source.buffer();
	const TCHAR* final	= mainp + source.get_len();
	for(;*mainp;)
	{
		TCHAR* start = (TCHAR*) _tmemchr(mainp, L0, (size_t)(final - mainp));
		if(!start) break;
		TCHAR* sepa  = (TCHAR*) _tmemchr(start, SP, (size_t)(final - start));
		if(!sepa)   break;

		TCHAR* end  = 0;
		int lefts  = 1; // this is for the begining ([)key:val]   
		int rights = 0;
		TCHAR car;
		for(end = sepa + 1; car = *end; end++)
		{
			if(car == L0/* || car == L0*/) ++lefts;
			if(car == R0/* || car == R0*/) ++rights;
			if(lefts == rights) break;
		}
		if(end > final) { break; }; // just for security

		mainp = end + 1;
		size_t lenkey = sepa - start - 1;
		size_t lenval = end - sepa - 1;

		_tmemcpy(key,++start, lenkey); key[lenkey]=0; 
		val.SetString(++sepa, lenval);

		if(values.has(val, lenval)) lenval = values.get(val, value, lenval);
		else value.SetString(val, lenval); //could be a constant

		set(key, value, lenkey, lenval);
	}
}

int CParameters::copyto(const TCHAR* keysrc, CParameters& destiny, 
	const TCHAR* keydes, const int keysrclen, const int keydeslen)
{
	CString value;
	get(keysrc, value, keysrclen);
	return destiny.set(keydes, value, keydeslen);
}

COLORREF CParameters::getrgb(const TCHAR* key, const int lenparam)
{
	TCHAR red[4];
	TCHAR green[4];
	TCHAR blue[4];

	TCHAR color[16];
	get(key, color, lenparam, 9);

	if(color[0] == _T('#'))
	{
		set4ch(red	,	color[1], color[2], 0, 0);
		set4ch(green,	color[3], color[4], 0, 0);
		set4ch(blue	,	color[5], color[6], 0, 0);
		return RGB(_httoi(red), _httoi(green), _httoi(blue));
	}
	else
	{
		__mike4cpy__(red,   color);
		__mike4cpy__(green, (color + 3));
		__mike4cpy__(blue,  (color + 6)); 
		return RGB(_tstoi(red), _tstoi(green), _tstoi(blue));
	}
}
bool CParameters::getbool(const TCHAR* key, const int lenkey)
{
	return getint(key, lenkey);
}
int CParameters::getint(const TCHAR* key, const int lenkey, const int def)
{
	TCHAR value[16];
	if(get(key, value, lenkey, 15))	return _ttoi(value);
	return def;
}
long CParameters::getlong(const TCHAR* key, const int lenkey, const long def)
{
	TCHAR value[16];
	if(get(key, value, lenkey, 15))	return _ttol(value);
	return def;
}

TCHAR CParameters::getchr(const TCHAR* key, const int lenkey)
{
	TCHAR value[16];
	get(key, value, lenkey, 15);
	return value[0];
}
double CParameters::getdouble(const TCHAR* key, const int lenkey)
{
	TCHAR value[16];
	get(key, value, lenkey, 15);
	return _tstof(value);
}

int CParameters::getkey(const int i, TCHAR* result, const int maxkeylen)
{
	result[0]=0;
	if(!isoptimized) return 0;
	if(i >= optmax) return 0;
	if(optlen[i] == 0) return 0;

	const TCHAR* dt = get_data();
	const TCHAR* pp = &dt[optbeg[i]];
	if(!pp) return 0;
	int keylen = optlen[i];
	if(keylen >= maxkeylen) keylen = maxkeylen;
	_tmemcpy(result, pp, keylen);
	result[keylen] = 0;
	return keylen;
}

int CParameters::getpair(const int i, CParameters& result)
{
	result.clear();

	if(!isoptimized) return 0;
	if(i >= optmax) return 0;
	if(optlen[i] == 0) return 0;

	const TCHAR* dt = get_data();
	const TCHAR* pp = &dt[optbeg[i]];
	if(!pp) return 0;
	--pp;
	int pairlen = (optend[i]-optbeg[i])+3;
	result.set_value(pp, pairlen);
	return pairlen;
}

int CParameters::getpair(	const int i, TCHAR* key, TCHAR* val, int& kl, int& vl, 
							const int maxkeylen, const int maxvallen)
{
	key[kl=0]=0;
	val[vl=0]=0;

	if(!isoptimized) return 0;
	if(i < 0 || i >= optmax) return 0;
	if(optlen[i] == 0) return 0;

	int beg = optbeg[i];
	int len = optlen[i];
	int end = optend[i];

	const TCHAR* pp = get_data() + beg;
	if(!pp) return 0;

	kl = len;
	if(maxkeylen && kl > maxkeylen) kl = maxkeylen;
	_tmemcpy(key, pp, kl); key[kl]=0;

	vl = end - (beg + len);
	if(maxvallen && vl > maxvallen) vl = maxvallen;
	_tmemcpy(val, pp + len + 1, vl); val[vl]=0;

	return end-beg+3;
}

void CParameters::get_pair(const TCHAR* key, CString& result, const int lenkey)
{
	get(key, result, lenkey);
	TCHAR leftpart[64];
	leftpart[0] = L0;
	_tmemcpy(&leftpart[1], key, lenkey);
	set2ch(&leftpart[lenkey+1], SP, 0);
	result.Insert(0, leftpart);
	result.AppendChar(R0);
}

int CParameters::get_pair(TCHAR* data, const TCHAR* key, const int keylen, 
									const int maxvallen)
{
	size_t klen = keylen == 0 ? _tcslen(key) : keylen;
	GENKEY(data, key, klen);
	size_t vlen = get(key, &data[klen + 2], klen, maxvallen);
	size_t len	= klen + 2 + vlen;
	set2ch(&data[len], R0, 0);
	return len + 1;
}

int CParameters::gen_pair(const int type, TCHAR* data, const TCHAR* key, 
									const TCHAR* val, const int keylen, const int vallen)
{
	int rkeylen = keylen == 0 ? _tcslen(key): keylen;
	int rvallen = vallen == 0 ? _tcslen(val): vallen;
	TCHAR left = type == 0 ? CParameters::LEFT : CParameters::LEFT;
	TCHAR sep = CParameters::SEP;
	TCHAR right = type == 0 ? CParameters::RIGHT : CParameters::RIGHT;
	GENKEY_STA(data, key, rkeylen, left, sep);
	_tmemcpy(data + rkeylen + 2, val, rvallen);
	set2ch(data + rkeylen + 2 + rvallen, right, 0);
	return rkeylen + rvallen + 3;
}
int CParameters::gen_pair(const int type, TCHAR* data, const TCHAR* key, 
									const long value, const int keylen)
{
	int rkeylen = keylen == 0 ? _tcslen(key): keylen;
	TCHAR left = type == 0 ? CParameters::LEFT : CParameters::LEFT;
	TCHAR sep = CParameters::SEP;
	TCHAR right = type == 0 ? CParameters::RIGHT : CParameters::RIGHT;
	GENKEY_STA(data, key, rkeylen, left, sep);
	_ltot(value, data + rkeylen + 2, 10);
	int rvallen = _tcslen(data + rkeylen + 2);
	set2ch(data + rkeylen + 2 + rvallen, right, 0);
	return rkeylen + rvallen + 3;
}

int CParameters::get(const TCHAR* key, CParameters& result, const int keylen)
{
	int rkeylen = keylen == 0 ? _tcslen(key): keylen;

	int l = 0;

	int vallen = get_pair_len(key, rkeylen);
	if(vallen == 0) 
	{
		result.clear();
		return 0;
	}
	else
	if(vallen < 2048)
	{
		TCHAR tmpval[2048];
		l = get(key, tmpval, rkeylen, 2047);
		result.set_value(tmpval, l);
	}
	else
	{	
		CString res;
		l = get(key, res, rkeylen);
		result.set_value(res);
	}

	result.optimize();
	return l;
}

int CParameters::get(const TCHAR* key, CString& result, const int lenparam)
{
	size_t keylen = lenparam == 0 ? _tcslen(key): lenparam;
	if(keylen == 0) { result.Empty(); return 0; }
	if(keylen > 64) keylen = 64;

	int len = 0; TCHAR* pval = nullptr; UINT vallen = 0;int optpos = 0;
	const TCHAR* data = get_data();
	const TCHAR* start = get_start(key, keylen, len, pval, vallen, optpos);

	if(!start)				{ result.Empty();					return 0;		}
	if(vallen >= get_len()) { result.Empty();					return 0;		}
	if(vallen)				{ result.SetString(pval, vallen);	return vallen;	}
	start += keylen + 2;

	const TCHAR* end = 0;
	int lefts =1; // this is for the begining ([)key:val]   
	int rights = 0;
	TCHAR car;
	for(end = start; car = *end; ++end)
	{
		if(car == L0/* || car == L0*/) ++lefts;
		if(car == R0/* || car == R0*/) ++rights;
		if(lefts == rights) break;
	}

	if(!end) { result.Empty(); return 0; }
	int inicio  = (int)(start - data);
	int fin     = (int)(end - data);
	int ln		= (int)(fin - inicio);

	result.SetString(data + inicio, ln);
	return ln;
}

int CParameters::get(	const TCHAR* key, TCHAR* val, const int lenparam, 
								const int maxvallen)
{
	if(!val) return 0;
	val[0] = 0;

	size_t keylen = lenparam == 0 ? _tcslen(key): lenparam;
	if(keylen == 0) return 0;
	if(keylen > 64) keylen = 64;

	int len = 0; TCHAR* pval = nullptr; UINT vallen = 0; int optpos = 0;
	const TCHAR* start = get_start(key, keylen, len, pval, vallen, optpos);

	if(!start) return 0;
	if(vallen >= get_len()) return 0;
	if(vallen) 
	{ 
		if(maxvallen > 0 && vallen > maxvallen) vallen = maxvallen;
		_tmemcpy(val, pval, vallen); 
		val[vallen] = 0;
		return vallen; 
	}
	start += keylen + 2;

	const TCHAR* end = 0;
	int lefts =1; // this is for the begining ([)key:val]   
	int rights = 0;
	TCHAR car;
	for(end = start; car = *end; ++end)
	{
		if(car == L0/* || car == L0*/) ++lefts;
		if(car == R0/* || car == R0*/) ++rights;
		if(lefts == rights) break;
	}

	if(!end) return 0;
	len = (int)(end - start);

	if(maxvallen > 0 && len > maxvallen) len = maxvallen;
	_tmemcpy(val, start, len);
	val[len] = 0;
	return len;
}

bool CParameters::has(const TCHAR* key, const int keylen)
{
	size_t lenkey = keylen == 0 ? _tcslen(key): keylen;
	if(lenkey == 0) return false;
	if(lenkey > 64) lenkey = 64;

	int len = 0; TCHAR* pval = nullptr; UINT vallen = 0; int optpos = 0;
	return (bool)get_start(key, lenkey, len, pval, vallen, optpos);
}

bool CParameters::hasval(const TCHAR* key, const int lenkey)
{
	size_t keylen = lenkey == 0 ? _tcslen(key) : lenkey;
	if(keylen == 0) return false;
	if(keylen > 64) keylen = 64;

	int len = 0; TCHAR* pval = nullptr; UINT vallen = 0; int optpos = 0;
	const TCHAR* start = get_start(key, keylen, len, pval, vallen, optpos);

	if(!start)	return false;
	if(vallen >= get_len()) return false;
	if(vallen)	return true; 

	start += keylen + 2; // one for [ and one for :

	return *start != R0; // si es diferente que right quiere decir que tiene algun valor;
}

bool CParameters::are_eq(	const TCHAR* key, const TCHAR* value, 
							const int keylen, const int valuelen)
{
	size_t lenkey = keylen == 0 ? _tcslen(key): keylen;
	size_t lenval = valuelen == 0 ? _tcslen(value): valuelen;
	if(lenkey == 0) return false;
	if(lenkey > 64) lenkey = 64;

	int pairlen = 0; TCHAR* pval = nullptr; UINT vallen = 0; int optpos = 0;
	const TCHAR* start = get_start(key, lenkey, pairlen, pval, vallen, optpos);
	if(!start) return false;
	if(vallen >= get_len()) return false;
	if(vallen)
	{
		if(vallen != lenval) return false;
		return cfuns[vallen](value,pval);
	}

	for(;*start;++start) if(*start == SP) break;	// nos vamos al separador 
	if(!start) return false;
	++start;										// nos vamos al inicio del valor
	const TCHAR* end = start;
	for(;*end;++end) if(*end == R0) break;		// nos vamos hasta el final
	if(!end) return false;
	vallen = (int)(end - start);

	if(vallen != lenval) return false;
	return cfuns[vallen](value,start);
}

bool CParameters::isactv(const TCHAR* key, const int keylen)
{
	size_t lenkey = keylen == 0 ? _tcslen(key): keylen;
	if(lenkey == 0) return false;
	if(lenkey > 64) lenkey = 64;

	int len = 0; TCHAR* pval = nullptr; UINT vallen = 0; int optpos = 0;
	const TCHAR* start = get_start(key, lenkey, len, pval, vallen, optpos);

	if(!start)	return false;
	if(vallen >= get_len()) return false;
	if(vallen)	return pval[0]=='1'; 

	for(;*start;++start) if(*start == SP) break; // nos vamos al separador 
	if(!start) return false;
	++start; // nos vamos al inicio del valor
	const TCHAR* end = start;
	for(;*end;++end) if(*end== R0) break; // nos vamos hasta el final
	if(!end) return false;

	len = (int)(end - start);
	if(len != 1) return false; 

	return start[0] == _T('1'); // this case is because most of the time is one letter
}

int CParameters::set(const TCHAR* key, const int value, const int keylen)
{
	TCHAR sval[128];
	_ltot(value, sval, 10);
	return set(key, sval, keylen);
}

int CParameters::set(	const TCHAR* key, const TCHAR* value, 
								const int keylen, const int lenvalue)
{
	size_t paramlen = keylen == 0 ? _tcslen(key): keylen;
	if(paramlen == 0) return 0;
	if(paramlen >= 64) paramlen = 64;

	int len = 0; TCHAR* pval = nullptr; UINT vallen = 0; int optpos = 0;
	const TCHAR* data = get_data();						// get the begining of the string
	const TCHAR* start = get_start(key, paramlen, len, pval, vallen, optpos);		// we look if already has the key

	size_t valuelen = lenvalue == -1 ? _tcslen(value): lenvalue;

	if(start)											// already has the key
	{
		if(optpos != -1 && vallen)
		{
			if(valuelen <= vallen)					// at this moment only overwritting
			{
				_tmemcpy(pval, value, valuelen);
				if(int spacelen = vallen - valuelen) 
				{
					*(pval+valuelen)=R0;
					_tmemset(pval+valuelen+1, NA, spacelen);
					// we just correct the end who is the only to be change
					optend[optpos] -= spacelen; 
				}
				return paramlen + 2 + valuelen + 1;
			}
			else
			{
				_tmemset(const_cast<TCHAR*>(start), NA, len);
				start = 0;
				if(optpos != -1)
					optbeg[optpos] = optlen[optpos] = optend[optpos] = 0;
			}
		}
		else
		{
			const TCHAR* base = start;						// we get the key base
			const TCHAR* end = 0;

			start += paramlen + 2;							// find the right

			int lefts = 1;									// this is for the begining ([)key:val]   
			int rights = 0;
			TCHAR letter;
			for(end = start; letter = *end; ++end)
			{
				if(letter == L0) ++lefts;
				if(letter == R0) ++rights;
				if(lefts == rights) break;
			}
			if(!end) return 0;								// some thing went wrong;

			int inicio		= (int)(start - data);
			int fin			= (int)(end - data);
			int oldvaluelen = (int)(fin - inicio);

			if(valuelen <= oldvaluelen)						// si cabe en el anterior
			{ 
				// basicamente sobreescribimos, y lo que sobre lo rellenamos con nada
				TCHAR* p = const_cast<TCHAR*>(start);
				_tmemcpy(p, value, valuelen);//, false);
				p += valuelen;
				*p++ = R0;
				if(int spacelen = oldvaluelen - valuelen)
				{
					_tmemset(p, NA, spacelen);
					unoptimize();// because we move the right(pair end)
				}
				return paramlen + 2 + valuelen + 1;
			}
			else// si no cabe llenamos todo con nada y marcamos como que nunca existio
			{
				++end;
				TCHAR* p = const_cast<TCHAR*>(base); 
				_tmemset(p, NA, (end - base));
				start = 0;
				unoptimize();// because we move the right(pair end)
			}
		}
	}

	if(!start)// si no esta simplemente lo agregamos
	{
		TCHAR temp[4096];
		GENKEY(temp, key, paramlen);
		int newlen = paramlen + 2 + valuelen;
		if(newlen < (4096-1))	// one for the right mark 
		{
			_tmemcpy(&temp[paramlen + 2], value, valuelen); 
			temp[newlen++] = R0;
			bool wasopt = add_optimization(paramlen, newlen);
			append_data(temp, newlen, wasopt);
		}
		else
		{
			CString tmp(temp,paramlen + 2);
			tmp.Append(value, valuelen);
			tmp.AppendChar(R0);
			newlen = tmp.GetLength();
			bool wasopt = add_optimization(paramlen, newlen);
			append_data(tmp.GetBuffer(), newlen, wasopt);
		}
		return newlen;
	}
	return 0;
}

void CParameters::append(const TCHAR* extra)							
{ 
	append_data(extra, _tcslen(extra), false);			
}
void CParameters::append(const TCHAR* extra, const int len)			
{ 
	append_data(extra, len, false);						
}

/** 
 * note: we should check the separator to but at this moment we use the [ and ] for
 * the limits of our map class, and unfortunally we still use the same for the list 
 * and if there any list inside the map it appears as a unformed pair, which is not 
 * true, but we need to fixed first
 */
const TCHAR* CParameters::is_bad_formed()
{
	if(const TCHAR* p = check_boundaries()) return p;

	int ls	= 0;
	int rs	= 0;
	register TCHAR car;
	register TCHAR* p = get_data();
	int len = get_len();
	if(!p || !len) return nullptr;
	for(register TCHAR* e = p + len; p<=e; ++p)
	{
		// avoid if any html embedded is pain on the neck
		if(cmp4ch(p, _T('<'),_T('h'),_T('t'),_T('m')))
		{
			for(UINT i=0; p<=e && i<256000; ++i,++p) //safety break;
			{
				if(cmp4ch(p, _T('<'),_T('/'),_T('h'),_T('t'))) 
				{
					p+=3;
					break;
				}
			}
		}

		car = *p;
		if(car == L0/* || car == L0*/) { ++ls; continue; } 
		if(car == R0/* || car == R0*/) ++rs;
	}
	return ls == rs ? nullptr : get_data();
}

void CParameters::set_from_json(const TCHAR* newparams, const int newparamslen)
{
	TCHAR* dest = nullptr;
	bool dynamic = false;
	if(dynamic = newparamslen > (4096)) dest = (TCHAR*)malloc(sizeof(TCHAR)*(newparamslen+1));
	else dest = (TCHAR*)alloca(sizeof(TCHAR)*(newparamslen+1));
	dest[0] = 0;
	int destpos = 0;

	bool incomillas = false;
	bool inbrackets = false;
	register TCHAR* p = const_cast<TCHAR*>(newparams);
	register TCHAR* q = p + newparamslen;
	for(; p!=q; ++p)
	{
		TCHAR car = *p;

		// swith between comillas or not
		if(car == _T('"')) incomillas = !incomillas;

		if(!incomillas)
		{
			if(car < 255 && isspace(car))				continue;
			if(car == L0 && inbrackets == false)		inbrackets = true;
			if(car == R0 && inbrackets == true )		inbrackets = false;
			if(inbrackets == false && car == _T(','))	continue;
		}

		dest[destpos++] = car;

		// if we have more data that space we are on trouble because
		// the json is supposed to be larger that mro params format
		if(destpos >= newparamslen) break;
	}
	dest[destpos] = 0;

	// destpos remains equal
	p = dest;
	q = p + destpos;
	for(; p!=q; ++p)
	{
		if(cmp2ch(p, '"','}'))
		{
			TCHAR* r = p+2;
			if(cmp2ch(r, '}','}'))					
			{
				TCHAR* s = r+2;
				if(cmp2ch(s, '}','}'))	{ set4ch(p, ']',']',']',']'); set2ch(p+4,']',']');	continue;}
				if(*s=='}')				{ set4ch(p, ']',']',']',']'); *(p+4)=']';			continue;}
				set4ch(p, ']',']',']',']');	
				continue;
			}
			if(*r=='}')					{ set2ch(p, ']',']');		  *(p+2)=']';			continue;}
			set2ch(p, ']',']');
		}
	}

	for(p=q=dest; *q=*p;)
	{
		if(cmp2ch(p, '"',':') && *(p+2)=='"')	{ *q=':';				++q; p+=3; continue; }
		if(cmp4ch(p, '"',':','{','"'))			{ set2ch(q, ':','[');	q+=2; p+=4; continue; }
		++q;++p;
	}

	for(p=q=dest; *q=*p;)
	{
		if(cmp2ch(p, '"','"'))	{ set2ch(q, ']','[');	q+=2; p+=2; continue; }
		if(cmp2ch(p, ']','"'))	{ set2ch(q, ']','[');	q+=2; p+=2; continue; }
		++q;++p;
	}

	// intended for lists
	for(p=q=dest; *q=*p;)
	{
		if(cmp4ch(p, '"',':','[','"'))	{ set2ch(q, ':','[');	q+=2; p+=4; continue; }
		++q;++p;
	}
	for(p=q=dest; *q=*p;)
	{
		if(cmp2ch(p, '"',']') && *(p+2)=='[')	{ set2ch(p, ']',']'); *(p+2)='[';	q+=3; p+=3; continue; }
		++q;++p;
	}
	for(p=q=dest; *q=*p;)
	{
		if(cmp2ch(p, '"',',') && *(p+2)=='"')	{ *q=',';	++q; p+=3; continue; }
		++q;++p;
	}
	for(p=q=dest; *q=*p;)
	{
		if(cmp2ch(p, '"',']'))
		{
			TCHAR* r = p+2;
			if(cmp2ch(r, '}','"'))					{ set4ch(p, ']',']',']','[');	q+=4; p+=4; continue; }
			if(cmp2ch(r, '}','}') && *(r+2)=='"')	{ set4ch(p, ']',']',']',']'); *(p+4)='['; q+=5; p+=5; continue; }
			if(cmp4ch(r, '}','}','}','"'))			{ set4ch(p, ']',']',']',']'); set2ch(p+4,']','['); q+=6; p+=6; continue; }
		}
		++q;++p;
	}
	for(p=q=dest; *q=*p;)
	{
		if(cmp2ch(p, '"',']') && *(p+2)=='}')		{ set2ch(p, ']',']'); *(p+2)=']';		q+=3; p+=3; continue; }
		++q;++p;
	}


for(p=q=dest; *q=*p;)
{
	if(cmp4ch(p, '"',',','{','"'))		
	{ 
		set2ch(q, ',','['); q+=2; p+=4; continue; 
	}
	++q;++p;
}
for(p=q=dest; *q=*p;)
{
	if(cmp4ch(p, ']',']',',','"'))
	{ 
		set2ch(q, ']',','); q+=2; p+=4; continue; 
	}
	if(cmp4ch(p, ']',']',']','}') && *(p+4)=='"')
	{ 
		set4ch(q, ']',']',']',']'); *(q+4) = '['; 
		q+=5; p+=5; continue; 
	}
	++q;++p;
}

	destpos = q-dest;

	if(cmp2ch(dest, _T('{'),_T('"'))) 
	{
		set2ch(dest,_T(' '),_T('['));
		dest[destpos-1] = ' ';
	}
	if(dest[0] == '"') dest[0] = '[';
	if(dest[destpos] == '"') dest[destpos] = ']';

	if(cmp2ch(&dest[destpos-3],_T(']'),_T('\''))) destpos -= 3;

	p = &dest[destpos - 5];
	if(cmp4ch(p, ']', ']', ']', '}'))
	{
		set4ch(p, ']', ']', ']', ']');
		//set2ch(p + 4, ']', ']');
		//p[6] = 0;
		//destpos += 2;
	}


	set_data(dest, destpos);
	if(dynamic) free(dest);
}

void CParameters::tojson(CString& response)
{
	int reslen = response.GetLength();
	if(reslen == 0) return;

	// we create length bigger than the orginal response because the json result is always bigger
	int maxtemp		= reslen + (reslen/4) + 1024; // 1024 for avoiding surprises
	TCHAR* tmp1		= nullptr;
	bool bystack	= maxtemp < (4096);
	tmp1			= bystack ? (TCHAR*)alloca(sizeof(TCHAR)*maxtemp) : (TCHAR*)malloc(sizeof(TCHAR)*maxtemp);
	const TCHAR* ps	= response.GetBuffer();
	TCHAR* pd		= tmp1;

	// first we eliminate all the blanks between pairs, in order to make the life easier
	bool inbeg = false;
	bool insep = false;
	bool inend = true; // because we can see the process as ending something previous

	int erases = 0 ;
	int i = 0;
	for(int j = 0; j < reslen; ++j)
	{
		TCHAR car = ps[j];

		if(car == LEFT) { inbeg = true; pd[i++] = car; continue; }
		if(car == SEP) { insep = true; pd[i++] = car; continue; }
		if(car == RIGHT) { inend = true; pd[i++] = car; continue; }

		if(car == _T('\n')) { ++erases; continue; }; 
		if(car == _T('\r')) { ++erases; continue; }; 

		if(car == ' ' || car == '\t') 
		{ 
			if(insep || inend || inbeg)  { ++erases; continue; }
		}

		inbeg = false;
		insep = false;
		inend = false;

		pd[i++] = car;
	}
	reslen -= erases;

	// bassically change the ":" que son valores validos en los values
	int lefts = 0;
	int seps = 0;
	for(int j = 0; j < reslen; j++)
	{
		TCHAR car = pd[j];
		if(car == LEFT) { ++lefts; continue; }
		if(car == SEP) 
		{
			++seps;
			if(lefts == seps) pd[j] = _T('ü');
			continue;
		}
		if(car == RIGHT) { seps = lefts; continue; }
	}
	response.SetString(pd, reslen);
	response.Replace(_T("ü["), _T("ü{\""));

/// falta implementar --> para agregar un enter, ya que esta de corrido y codigo valido detras de el desaparece
	reslen	= response.GetLength();
	ps		= response.GetBuffer();
	pd		= tmp1;
	i		= 0;
	int extras=0;
	for(int j = 0; j < reslen; )
	{
		TCHAR letter = ps[j];
		const TCHAR* pl = &ps[j];

		if(cmp4ch(pl, _T(']'), _T(']'), _T(']'), _T(']')))	{set4ch(&pd[i], _T('\"'), _T('}'), _T('}'), _T('}')); pd[4] = _T(',');	i+=5; extras+=1; j+=4; continue;} // ]]]]	\"}}},
		if(cmp2ch(pl, _T(']'), _T(']')) && pl[2]==_T(']'))	{set4ch(&pd[i], _T('\"'), _T('}'), _T('}'), _T(','));					i+=4; extras+=1; j+=3; continue;} // ]]]	\"}},
		if(cmp2ch(pl, _T(']'), _T(']')))					{set4ch(&pd[i], _T('\"'), _T('}'), _T(','), 0);							i+=3; extras+=1; j+=2; continue;} // ]]		\"},
		if(cmp4ch(pl, _T('*'), _T('-'), _T('-'), _T('>')))	{set4ch(&pd[i], _T('-'), _T('-'), _T('>'), MRO_UNICODE_RET);			i+=4; extras+=0; j+=4; continue;} // comments

		if(letter == _T('['))								{pd[i] = _T('\"');														i+=1;			 j+=1; continue;} // [")	\"
		if(letter == _T(']'))								{set2ch(&pd[i], _T('\"'), _T(','));										i+=2; extras+=1; j+=1; continue;} // ]")	\",
		if(letter == _T('ü'))								{set4ch(&pd[i], _T('\"'), _T(':'), _T('\"'), 0);						i+=3; extras+=2; j+=1; continue;} // ü		\":\"

		pd[i++] = letter;
		++j;
	}

	//require((reslen + extras) > maxtemp, _T("***internal error***"));
	if((reslen + extras) < maxtemp) 
	{
		reslen += extras;
		if(pd[reslen-1] == _T(',')) reslen--;
		response.SetString(pd, reslen);
	}
	if(!bystack) free(tmp1);

	response.Replace(_T(":\"{")	, _T(":{"));
}

int cMroList::get(TCHAR* res)
{
	int len = _value.GetLength();
	_tmemcpy(res, _value.GetBuffer(), len + 1);
	return len;
}

bool cMroList::begin()
{
	putinhead();
	return next();
}

bool cMroList::end()
{
	return !_ptr || *(_ptr-1) == cMroList::right;
}

void cMroList::putinhead() 
{ 
	if(_rep.IsEmpty()) { _ptr = 0; return; } 
	_ptr = get_data();
	_ptr = _tcschr(_ptr, cMroList::left);
	if(!_ptr) { return; }
	++_ptr;
	for(;*_ptr; ++_ptr)
	{
		TCHAR car = *_ptr;
		if(!(car < 255 && isspace(car))) break;
	}
}

bool cMroList::next()
{
	register TCHAR* p = _ptr;
	if(!p) { return false; }
	for(;*p; ++p)
	{
		TCHAR car = *p;
		if(!(car < 255 && isspace(car)) && car != _T(',')) break;
	}
	if(!p) { return false; }
	if(*p == cMroList::right) { _ptr = 0; return false; }
	TCHAR* q = p;
	for(;*q; ++q)
	{
		TCHAR car = *q;
		if((car < 255 && isspace(car))) break;
		if(car == cMroList::right) break;
		if(car == _T(',')) break;
	}
	if(!q) { _ptr = 0; return false; }
	int len = (int)(q - p);
	_value.SetString(p, len);
	_ptr = q;
	return true;
}

}
