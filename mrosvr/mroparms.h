#pragma once

//#define NAKEDCHAR

#ifdef NAKEDCHAR
using namespace std;
#include <vector>
#endif

#define __mike4cpy__(d, s) set4ch(d, s[0], s[1], s[2], 0);

#define __mike8cpy__(d, s)\
		set4ch(d, s[0], s[1], s[2], s[3]);\
		set4ch(&d[4], s[4], s[5], s[6], 0);

//extern inline void mikecpy(TCHAR* dest, const TCHAR* src, size_t len, bool withnull = true);
//extern inline bool mikecmp(const TCHAR* one, const TCHAR* two, size_t len);

namespace mro {

class CParameters
{
public:
	CParameters();
	//CParameters(CParameters& params);
	CParameters(CString& params);
	CParameters(const TCHAR* params);
	CParameters(const TCHAR* params, const int len);
	CParameters(const TCHAR* key, const TCHAR* val, const int keylen = 0, const int vallen = -1);
	CParameters(const TCHAR* key, const int val, const int keylen);

	void init();
	//void check_type();
	void set_from_json(const TCHAR* newparams, const int newparamslen);
	static void tojson(CString& result);

	CString		get(CString& key)							{ return get(key.GetBuffer(), key.GetLength());		}
	int			get(CString& key, CString& val)				{ return get(key.GetBuffer(), val, key.GetLength());}
	CString		get(const TCHAR* key, const int lenkey= 0)	{ CString r; get(key, r, lenkey); return r;			}
	int			get(const TCHAR* key, CString& val, const int lenkey = 0);
	int			get(const TCHAR* key, TCHAR* val, const int lenkey = 0, const int maxvallen = 0);
	int			get(const TCHAR* key, CParameters& val, const int lenkey = 0);

	bool		getbool(const TCHAR* key, const int lenkey = 0);
	int			getint(const TCHAR* key, const int lenkey = 0, const int def = 0);
	long		getlong(const TCHAR* key, const int lenkey = 0, const long def = 0);
	TCHAR		getchr(const TCHAR* key, const int lenkey = 0);
	COLORREF	getrgb(const TCHAR* key, const int lenkey = 0);
	double		getdouble(const TCHAR* key, const int lenkey = 0);

	inline void set_value(const TCHAR* params, const int len)		{ set_data(params, len);							}
	inline void set_value(const TCHAR* key, const TCHAR* val, const int keylen, const int vallen) 
																	{ clear(); set(key, val, keylen, vallen);			}
	inline void set_value(const TCHAR* params)						{ set_data(params, _tcslen(params));				}
	inline void set_value(CString& params)							{ set_data(params.GetBuffer(), params.GetLength());	}
	//inline void set_value(std::wstring& params)							{ set_data(params.c_str(), params.length()); }
	void set_value(CParameters& params);//						{ set_data(params.buffer()	, params.get_len());	}

	int  set(const TCHAR* key, const TCHAR* val, const int keylen = 0, const int vallen = -1);
	int  set(const TCHAR* key, CString& val)	{ return set(key			, val.GetBuffer()	, _tcslen(key)		, val.GetLength());	}
	int  set(CString& key, const TCHAR* val)	{ return set(key.GetBuffer(), val				, key.GetLength()	, _tcslen(val));	}
	int  set(CString& key, CString& val)		{ return set(key.GetBuffer(), val.GetBuffer()	, key.GetLength()	, val.GetLength());	}
	int  set(const TCHAR* key, const int val, const int keylen = 0);
	int  set(const TCHAR* key, CParameters& val, const int keylen = 0)
														{ return set(key			, val.buffer()		, keylen			, val.get_len());	}
	int  set(const TCHAR* key, const bool pred, const TCHAR* one, const TCHAR* two, const int keylen = 0)
	{
		if(pred) return set(key, one, keylen);
		return set(key, two, keylen);
	}

	int getpair(const int idx, CParameters& result);
	int getpair(const int idx, TCHAR* key, TCHAR* val, int& lk, int& lv, const int maxkeylen=0, const int maxvallen=0);
	int getkey(const int idx, TCHAR* key, const int maxkeylen);

	static int	gen_pair(const int type, TCHAR* data, const TCHAR* key, const TCHAR* val, const int keylen=0, const int vallen=0);
	static int	gen_pair(const int type, TCHAR* data, const TCHAR* key, const long val, const int keylen=0);

	int  get_pair	(TCHAR* data, const TCHAR* key, const int keylen = 0, const int maxvallen = 0);
	void  get_pair	(const TCHAR* key, CString& res, const int lenkey= 0);

	bool  are_eq	(const TCHAR* key, const TCHAR* val, const int keylen = 0, const int vallen = 0);
	bool  has	    (const TCHAR* key, const int keylen = 0);
	bool  hasval	(const TCHAR* key, const int keylen = 0);
	bool  isactv	(const TCHAR* key, const int keylen = 0);
	void  active	(const TCHAR* key, const int keylen = 0) { set(key, _T("1"), keylen, 1); };
	void  deactive	(const TCHAR* key, const int keylen = 0) { set(key, _T("0"), keylen, 1); };

	int get_len()											{ return get_length(); }
	TCHAR* buffer()										{ return get_data();   }
	bool isempty()										{ return isoptimized ? optmax == 0 : get_length() == 0;	}
	bool notempty()										{ return isoptimized ? optmax != 0 : get_length() != 0; }
	const TCHAR* is_bad_formed();

	// modify state
	void del(const TCHAR* key, const UINT keylen);
	void rpl_from(const TCHAR* newparams, const int len = 0);											
	void rpl_from(CParameters& newparams)				{ rpl_from(newparams.buffer(), newparams.get_len()); }	
	void merge(CParameters& source, CParameters& values);

	int  get_pair_len(const TCHAR* key, const UINT keylen);
	int nkeys();
	void unoptimize();
	void clear()											{ clear_data();											}
	void optimize();
	int copyto(const TCHAR* key, CParameters& destiny, const TCHAR* keydes, const int keysrclen, const int keydeslen);
	void compact(const bool optimize = true);
	int extract(const TCHAR* key, TCHAR* val, const int keylen, const int maxvallen = 0)
	{
		int r = get(key, val, keylen, maxvallen);
		if(r) del(key, keylen);
		return r;
	}
	int extract(const TCHAR* key, CString& val, const int keylen)
	{
		int r = get(key, val, keylen);
		if(r) del(key, keylen);
		return r;
	}
	CString extract(const TCHAR* key, const int keylen)
	{
		CString r = get(key, keylen);
		if (r.GetLength()) del(key, keylen);
		return r;
	}
	int extract(const TCHAR* key, CParameters& val, const int keylen)
	{
		int r = get(key, val, keylen);
		if(r) del(key, keylen);
		return r;
	}

	void append(const TCHAR* extra);
	void append(const TCHAR* extra, const int len);
	void append(CParameters& extra);

	inline CParameters clone()		{ return CParameters(get_data(), get_len());}
	inline CString	copy()			{ return CString(get_data(), get_len());	}

#define PAIRLEFT _T('[')
#define PAIRLEFT2 _T('{')
#define PAIRSEP _T(':')
#define PAIRRIGHT _T(']')
#define PAIRRIGHT2 _T('}')
#define PAIRNADA _T(' ')

	static const TCHAR LEFT		= PAIRLEFT;
//	static const TCHAR LEFT2	= PAIRLEFT2;
	static const TCHAR RIGHT	= PAIRRIGHT;
//	static const TCHAR RIGHT2	= PAIRRIGHT2;
	static const TCHAR SEP		= PAIRSEP;
	static const TCHAR NADA		= PAIRNADA;

private:

	bool  add_optimization(const int len, const int vallen);
	const TCHAR*  get_opt_pos(	const TCHAR* key, const UINT keylen,
										int& pairlen,
										TCHAR*& valpos, UINT& vallen,
										int& optpos);
	const TCHAR*  get_start(	const TCHAR* key, const UINT keylen, int& pairlen,
										TCHAR*& pval, UINT& vallen,
										int& optpos);

	TCHAR* get_data()								{ return _get_data();						}
	int    get_length()								{ return _get_length();						}
	void   set_length(const int len)				{ _set_length(len);							}
	void clear_data()								{ _clear_(); isoptimized = true; optmax=0;	}
	void set_data(const TCHAR* data, const int len)			
	{ 
		_set_(data, len);	
		unoptimize();
		if(len == 0) return;
		optimize(); 
	}
	void append_data(const TCHAR* data, const int len, const bool wasopt)		
	{ 
		if(len == 0) return;
		_append_(data, len);		
		// if it is optimized and was optimized the newpart we do nothing
		if(isoptimized && wasopt) 
		{
			if(const TCHAR* er = check_boundaries())
			{
				CString err(er);
				int dotpos = err.Find(_T(':'));
				if(dotpos != -1) err.Delete(dotpos,err.GetLength()-dotpos);
				if(err.GetLength() > 32) err.Delete(32,err.GetLength()-32);
				err.Replace(_T('['),_T(' '));
				err.Replace(_T(']'),_T(' '));
				err.Trim();
				requireex(true, _T("wrong_params_opt"), err);
			}
			return;
		}
		// if it is optimized and couldnt optimized (maybe we are on the limit)
		// we must unoptimized otherwise we lose the reference on the new part
		if(isoptimized && !wasopt) unoptimize();
		// if it is not optimized and we couldnt optimized 
		if(!isoptimized) optimize();
	}

	// implementation dependant

#ifdef NAKEDCHAR
	vector<TCHAR> _rep;
	TCHAR* _get_data()							{ return _rep.size() ? &_rep[0] : nullptr;		}
	int    _get_length()						{ return _rep.size();							}
	void   _set_length(const int l)				{ _rep.resize(l);								}
	void   _clear_()							{ _rep.clear();									}
	void   _del_(const int p, const int hm)		{ _rep.erase(_rep.begin()+p,_rep.begin()+p+hm);	}
	void   _set_(const TCHAR* d, const int l)	{ _clear_(); _append_(d,l);						}	
	void   _append_(const TCHAR* d, const int l){ if(d) for(register int i=0; i<l; ++i) _rep.push_back(d[i]);	}
#else
	CFixedStringT< CString, 1024 > _rep; //CString _rep;
	TCHAR* _get_data()							{ return _rep.GetBuffer();		}
	int    _get_length()						{ return _rep.GetLength();		}
	void   _set_length(const int l)				{ _rep.GetBufferSetLength(l);	}
	void   _clear_()							{ _rep.Empty();					}
	void   _del_(const int p, const int hm)		{ _rep.Delete(p, hm);			}
	void   _set_(const TCHAR* d, const int l)	{ _rep.SetString(d, l);			}	
	void   _append_(const TCHAR* d, const int l){ _rep.Append(d, l);			}
#endif
	// implementation dependant

	static const BYTE MAXOPTIMIZATION = 128;
	UINT optlen[MAXOPTIMIZATION];
	UINT optbeg[MAXOPTIMIZATION];
	UINT optend[MAXOPTIMIZATION];
	BYTE optmax;
	bool isoptimized;

	//bool isjson;

	static const TCHAR L0 = PAIRLEFT;
//	static const TCHAR L1;
	static const TCHAR SP = PAIRSEP;
	static const TCHAR R0 = PAIRRIGHT;
//	static const TCHAR R1;
	static const TCHAR NA = PAIRNADA;

	const TCHAR* check_boundaries()
	{
		TCHAR* p = _get_data();
		UINT len = _get_length();
		if(!p || !len) return nullptr;
		int li = optmax-1;
		int lm = li/2;

		if(li>1)
		{
			UINT b = optbeg[0]; UINT l = optlen[0]; UINT e = optend[0];
			if(b >= len) return p;
			if(l >= len || e >= len) return p+b;
			if((b+l+e))
			{
				if(*(p + b - 1) != L0) 	return p+b;
				if(*(p + b + l) != SP)	return p+b;
				if(*(p + e + 1) != R0) 	return p+b;
			}
		}
		if(lm > 0 && lm < li)
		{
			UINT b = optbeg[lm]; UINT l = optlen[lm]; UINT e = optend[lm];
			if(b >= len) return p;
			if(l >= len || e >= len) return p+b;
			if((b+l+e))
			{
				if(*(p + b - 1) != L0)	return p+b;
				if(*(p + b + l) != SP) 	return p+b;
				if(*(p + e + 1) != R0) 	return p+b;
			}
		}
		if(li > -1)
		{
			UINT b = optbeg[li]; UINT l = optlen[li]; UINT e = optend[li];
			if(b >= len) return p;
			if(l >= len || e >= len) return p+b;
			if((b+l+e))
			{
				if(*(p + b - 1) != L0) 	return p+b;
				if(*(p + b + l) != SP) 	return p+b;
				if(*(p + e + 1) != R0) 	return p+b;
			}
		}
		return nullptr;
	}
};

class cMroList
{
#define LISTLEFT _T('{')
#define LISTRIGHT _T('}')

public:
	cMroList(const TCHAR* value = _T(""))				{ set_data(value, 0);		}
	cMroList(const TCHAR* value, const int vallen)		{ set_data(value, vallen);	}

	void set_value(const TCHAR* value, const int lenval){ set_data(value, lenval);	}

	int get_len() { return get_length(); }
	const TCHAR* buffer() { return get_data(); }
	void clear() { clear_data(); }
	void putinhead() ;
	bool begin();
	bool end();
	bool next();
	int get(TCHAR* res);
	void add(TCHAR* value, const int lenval);
	int get_len() const { return get_length(); }

private:
	void set_data(const TCHAR* value, const int lenval) 
	{ 
		_rep.SetString(value, lenval); 
/////////////////////////////
///// TEMPORARY
_rep.Replace('[', LISTLEFT);
_rep.Replace(']', LISTRIGHT);
/////////////////////////////
		putinhead(); 
	}
	TCHAR* get_data() { return _rep.GetBuffer(); }
	int get_length() const { return _rep.GetLength(); }
	void clear_data() { _rep.Empty(); }

	CString _rep;
	CString _value;
	TCHAR* _ptr;

public:
	static const TCHAR left		= LISTLEFT;
	static const TCHAR right	= LISTRIGHT;
};

typedef CParameters cpairs;
}