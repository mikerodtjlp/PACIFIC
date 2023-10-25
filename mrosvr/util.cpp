#include "stdafx.h"

#include "Tlhelp32.h"

namespace mro {

int gethome(TCHAR* foldermain)
{
	if(!GetEnvironmentVariable(_T("PACIFICHOME"), foldermain, MAX_PATH))
		GetCurrentDirectory(MAX_PATH, foldermain);
	return _tcslen(foldermain);
}

void mroerr::to_params(CString& result)
{
	if(extrainfo.GetAt(0) == cpairs::LEFT) // maybe already processed
	{
		result.SetString(extrainfo.GetBuffer(), extrainfo.GetLength());
		return;
	}

	cpairs errinf;
	try
	{
		errinf.set(ZSERROR	, description	, ZSERRORLEN);
		errinf.set(ZCERROR	, description	, ZCERRORLEN);
		errinf.set(ZNERROR	, _T("1")		, ZNERRORLEN);
		errinf.set(ZHERROR	, extrainfo		, ZHERRORLEN);

		cpairs ei;
		ei.set(_T("errorin"), 1, 7);
		ei.set(_T("errori0"), errinfo, 7);

		cpairs el;
		el.set(_T("errorln"), 1, 7);

		TCHAR specinfo[1024]; 
		int l=0;
		int t=0;
		_tmemcpy(&specinfo[l], _T("comp:"), 5); l+=5;
		_tmemcpy(&specinfo[l], component.GetBuffer(), t=component.GetLength()); l+=t;
		set2ch(&specinfo[l], ';',' '); l+=2;
		_tmemcpy(&specinfo[l], _T("file:"), 5); l+=5;
		_tmemcpy(&specinfo[l], errfile.GetBuffer(), t=errfile.GetLength()); l+=t;
		set2ch(&specinfo[l], ';',' '); l+=2;
		_tmemcpy(&specinfo[l], _T("func:"), 5); l+=5;
		_tmemcpy(&specinfo[l], function.GetBuffer(), t=function.GetLength()); l+=t;

		set2ch(&specinfo[l], ';',' '); l+=2;
		_tmemcpy(&specinfo[l], _T("line:"), 5); l+=5;
		_ltot(errline, &specinfo[l], 10); 
		if(errline < 9) ++l; 
		else if(errline < 99) l+=2; 
		else if(errline < 999) l+=3; 
		else if(errline < 9999) l+=4; else l+=5;

		set2ch(&specinfo[l], ';', ' '); l += 2;
		_tmemcpy(&specinfo[l], _T("step:"), 5); l += 5;
		_ltot(step, &specinfo[l], 10);
		if (step < 9) ++l;
		else if (step < 99) l += 2;
		else if (step < 999) l += 3;
		else if (step < 9999) l += 4; else l += 5;

		set2ch(&specinfo[l], ';',' '); l+=2;
		_tmemcpy(&specinfo[l], _T("clas:"), 5); l+=5;
		_tmemcpy(&specinfo[l], errclass.GetBuffer(), t=errclass.GetLength()); l+=t;
		set2ch(&specinfo[l], ';',' '); l+=2;
		el.set(_T("errorl0"), specinfo, 7, l);

		errinf.set(ZERRORM, ei, ZERRORMLEN);
		errinf.set(ZERRORS, el, ZERRORSLEN);

		errinf.set(_T("ZERRSTK"), callstack, 7);
	}
	catch(CException *e)	{ e->Delete();	}
	catch(mroerr& e)		{				}
	catch(...)				{				}
	result.SetString(errinf.buffer(), errinf.get_len());
}

void memhelper::initialize_memhelper(const UINT maxprocs)
{
	if(memgbl == 0)
	{
		memgbl = new sMemGblManager[maxprocs];
		for(int i = 0; i < maxprocs; i++)
		{
			for(int j = 0; j < 2; j++)
			{
				memgbl[i].pmem[j] = 0;
				memgbl[i].len[j]  = 0;
			}
		}
	}
}

void memhelper::get_mem_from_gbl_manager(TCHAR** presult, const int lon, const int procplace, const int slot)
{
	if(lon > memgbl[procplace].len[slot])
	{
		free(memgbl[procplace].pmem[slot]);
		memgbl[procplace].pmem[slot] = (TCHAR*)malloc(sizeof(TCHAR)*(lon + 1));
		memgbl[procplace].len[slot] = lon;
	}
	*presult = memgbl[procplace].pmem[slot];
}

sMemGblManager* memhelper::memgbl = 0;


CString int_to_str(const int val)
{	
	TCHAR result[16];	
	mikefmt(result, _T("%d"), val);	
	return result; 
}

//CString double_to_str(const double val)
//{	
//	TCHAR result[64];	
//	mikefmt(result, _T("%f"), val);	
//	return result; 
//}

int mikefmtA(char* p, const char *fmt, ...)
{
	va_list ap;
	va_start(ap, fmt);
//    register int len = vsprintf(p, fmt, ap);
		#pragma warning(push)
		#pragma warning(disable:4996) // Disable deprecation warning since calling function is also deprecated
			register int len = _vsprintf_l(p, fmt, 0, ap);
		#pragma warning(pop)
	va_end(ap);
	return len;
}

int mikefmt(TCHAR* p, const TCHAR *fmt, ...)
{
	va_list ap;
	va_start(ap, fmt);
//    register int len = vsprintf(p, fmt, ap);
		#pragma warning(push)
		#pragma warning(disable:4996) // Disable deprecation warning since calling function is also deprecated
			register int len = _vstprintf_l(p, fmt, 0, ap);
		#pragma warning(pop)
	va_end(ap);
	return len;
}

void get_exename(TCHAR* exename)
{
	defchar(exe,256);
	if(int l = ::GetModuleFileName(0, exe, 255))
	{
		int n = 0;
		for(; l > 0; --l, ++n) if(exe[l] == '\\') { ++l; --n; break; }
		_tmemcpy(exename, &exe[l], n+1);
		if(TCHAR* p = _tcsstr(exename, _T(".exe"))) *p=0;
	}
}

int strfmt1(TCHAR* p, const TCHAR* ftm, const TCHAR* val, const int fmtlen, const int vallen)
{
	int fl = fmtlen == 0 ? _tcslen(ftm) : fmtlen;
	int vl = vallen == 0 ? _tcslen(val) : vallen;
	int i=0;
	const TCHAR* r = ftm + fl;
	for(register const TCHAR* q = ftm; q!=r; ++q)
	{
		if(cmp2ch(q, '%','s'))
		{
			_tmemcpy(&p[i], val, vl);
			i+=vl; 
			++q;
			continue;
		}
		p[i++]=*q;
	}
	p[i] = 0;
	return i;
}

int intfmt1(TCHAR* p, const TCHAR* ftm, const int val, const int fmtlen)
{
	int fl = fmtlen == 0 ? _tcslen(ftm) : fmtlen;
	int i=0;
	const TCHAR* r = ftm + fl;
	for(register const TCHAR* q = ftm; q!=r; ++q)
	{
		if(cmp2ch(q, '%','d'))
		{
			_ltot(val, &p[i], 10);
			i+=_tcslen(&p[i]);
			++q;
			continue;
		}
		p[i++]=*q;
	}
	p[i] = 0;
	return i;
}

int intfmt2(TCHAR* p, const TCHAR* ftm, const int val1, const int val2, const int fmtlen)
{
	int fl = fmtlen == 0 ? _tcslen(ftm) : fmtlen;
	int val = val1;
	int i=0;
	const TCHAR* r = ftm + fl;
	for(register const TCHAR* q = ftm; q!=r; ++q)
	{
		if(cmp2ch(q, '%','d'))
		{
			_ltot(val, &p[i], 10);
			i+=_tcslen(&p[i]);
			++q;
			val = val2;
			continue;
		}
		p[i++]=*q;
	}
	p[i] = 0;
	return i;
}

int chrfmt1(TCHAR* p, const TCHAR* ftm, const int val, const int fmtlen)
{
	int fl = fmtlen == 0 ? _tcslen(ftm) : fmtlen;
	int i=0;
	const TCHAR* r = ftm + fl;
	for(register const TCHAR* q = ftm; q!=r; ++q)
	{
		if(cmp2ch(q, '%','c'))
		{
			p[i++] = (TCHAR)val;
			++q;
			continue;
		}
		p[i++]=*q;
	}
	p[i] = 0;
	return i;
}

int s_ifmt1(TCHAR* p, const TCHAR* ftm, const TCHAR* sval, const int ival, 
									const int fmtlen, const int svallen)
{
	int fl = fmtlen == 0 ? _tcslen(ftm) : fmtlen;
	int vl = svallen == 0 ? _tcslen(sval) : svallen;
	int i=0;
	const TCHAR* r = ftm + fl;
	for(register const TCHAR* q = ftm; q!=r; ++q)
	{
		if(cmp2ch(q, '%','s'))
		{
			_tmemcpy(&p[i], sval, vl);
			i+=vl;
			++q;
			continue;
		}
		if(cmp2ch(q, '%','d'))
		{
			_ltot(ival, &p[i], 10);
			i+=_tcslen(&p[i]);
			++q;
			continue;
		}
		p[i++]=*q;
	}
	p[i] = 0;
	return i;
}

int strfmt2(TCHAR* p, const TCHAR* ftm, const TCHAR* val1, const TCHAR* val2,
						const int fmtlen, const int val1len, const int val2len)
{
	int fl = fmtlen == 0 ? _tcslen(ftm) : fmtlen;
	int v1 = val1len == 0 ? _tcslen(val1) : val1len;
	int v2 = val2len == 0 ? _tcslen(val2) : val2len;
	const TCHAR* pval = val1;
	int vallen = v1;
	int i=0;
	const TCHAR* r = ftm + fl;
	for(register const TCHAR* q = ftm; q!=r; ++q)
	{
		if(cmp2ch(q, '%','s'))
		{
			_tmemcpy(&p[i], pval, vallen);
			i+=vallen;
			++q;
			pval = val2;
			vallen = v2;
			continue;
		}
		p[i++]=*q;
	}
	p[i] = 0;
	return i;
}

bool create_process(const TCHAR* process, DWORD priority, const TCHAR* currdir, const TCHAR* name)
{
	STARTUPINFO si;
	PROCESS_INFORMATION pi;
	memset(&si,0,sizeof(si));
	si.cb			= sizeof(si);
	si.dwFlags		= STARTF_USESHOWWINDOW;
	si.wShowWindow	= SW_SHOW;
	return ::CreateProcess(name, (TCHAR*)process, NULL, NULL, FALSE, priority, NULL, currdir, &si, &pi);
}

bool exist_file(const TCHAR* filename)
{
	return _taccess( filename, 0 ) != -1;
}

bool is_unicode_file(const TCHAR* filename)
{
	// Byte-order mark goes at the begining of the UNICODE file
	_TCHAR bom;

	CFile* pFile = new CFile();
	pFile->Open( filename, CFile::modeRead );
	pFile->Read( &bom, sizeof(_TCHAR) );
	pFile->Close();

	// If we are reading UNICODE file
	return bom == _TCHAR(0xFEFF);
}

CString get_user_name()
{
	TCHAR   szBuffer[MAX_COMPUTERNAME_LENGTH + 1];
	DWORD  dwNameSize = MAX_COMPUTERNAME_LENGTH + 1;
	GetUserName( szBuffer, &dwNameSize );
	return CString(szBuffer);
}

bool write_event(DWORD type, LPTSTR pszSrcName)
{
	// use local computer and event source name 
	HANDLE h= RegisterEventSource(NULL,pszSrcName);
	if (h == NULL) return false;

	LPCTSTR m2 = NULL;
	LPCTSTR* lpStrings = &m2;

	// Report the event.
	if (!ReportEvent(h,				// event log handle 
			type,					// event type 
			1,						// event category  
			1,						// event identifier 
			NULL,					// no user security identifier 
			1,						// number of substitution strings 
			0,						// no data 
			lpStrings,              // pointer to strings 
			NULL))					// no data 
	{
		return false;
	}
 
	DeregisterEventSource(h); 
	return true;
}

int get_process_running_count(const TCHAR* processname)
{
	int result = 0;
	HANDLE handle;
	// Create snapshot of the processes
	handle = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);
	PROCESSENTRY32 info;
	info.dwSize = sizeof(PROCESSENTRY32);

	// Get the first process
	int first = Process32First(handle, &info);
	// If we failed to get the first process, throw an exception
	if (first == 0) throw _T("Process32First");

	// While there's another process, retrieve it
	do
	{
		if(_tcscmp(info.szExeFile, processname) == 0)
		{
			++result;
		}
	}
	while (Process32Next(handle, &info) != 0);
	CloseHandle(handle);
	return result;
}

/** 
 * description		:	*** very import note ***
 *						because when the IIS does not find the file, it does not return 
 *						some error code what really happends is in fact return a file 
 *						telling you that the file(or page) coud not be foud, some it'll 
 *						always returns a file, so we must check it if it is not a 
 *						page cannot be found file 
 *
 * author           : miguel rodriguez ojeda
 * date             : june 20 2004
 *
 * modification     : november 20 2004; apply done[? to the if conditions and apply continue in those conditions
 *
 */
bool evaluate_downloaded_file(const TCHAR* to)
{
	TCHAR* ext = const_cast<TCHAR*>(_tcsrchr(to, '.'));
	int typeext = -1;
	int nlines = 4;
	if(ext) 
	{
		++ext;
		if(_tmemcmp(ext, _T("jpg"), 4) == 0) { typeext = 1; }
		else
		if(_tmemcmp(ext, _T("bmp"), 4) == 0) { typeext = 2; }
		else
		if(_tmemcmp(ext, _T("gif"), 4) == 0) { typeext = -1; }
		else
		if(_tmemcmp(ext, _T("ico"), 4) == 0) { typeext = -1; }
		else
		if(_tmemcmp(ext, _T("exe"), 4) == 0) { typeext = 3; nlines = 5; }
		else 
		if(_tmemcmp(ext, _T("txt"), 4) == 0) { typeext = 3; nlines = 5; }
	}

	bool isgood = true;
	defchar(sentinel, 1024);
	if(mro::exist_file(to))
	{
		CUTF16File inputfile;
		inputfile.Open(to, CFile::modeRead);
		int size = inputfile.GetLength();
		for(int i=0; i < nlines; ++i)
		{
			if(size == 0) break;

			int senlen = 1023;
			if(size < 1024) senlen = size;
			inputfile.ReadString(sentinel, senlen);
			sentinel[senlen] = '\0';

			if(typeext == 1)
			{
				isgood = sentinel[0] == 0xFF;
				break;
			}
			else
			if(typeext == 2)
			{
				isgood = cmp2ch(sentinel, 0x42,0x4D);
				break;
			}
			else
			if(typeext == 3 && sentinel[0] && 
				_tcsstr(sentinel, _T("<title>404 - File or directory not found.</title>")))
			{
				isgood = false;
				break;
			}
			else
			if(typeext == -1 && sentinel[0] && 
				_tcsstr(sentinel, _T("<HTML><HEAD><TITLE>The page cannot be found</TITLE>")))
			{
				isgood = false;
				break;
			}
		}
		inputfile.Close();
	}

	if(!isgood)
		_tremove(to);

	return isgood;
}

void show_file(const TCHAR* file, const TCHAR* specific) 
{
	TCHAR filex[128];
	if(specific) _tcscpy_s(filex, specific);
	else _tmemcpy(filex, _T("notepad"), 8); 		
	ShellExecute(nullptr, _T("open"), filex, file, NULL, SW_SHOWNORMAL);
}

void clear_folder(const TCHAR* folder, const TCHAR* filter, const int keeplessthan)
{
	TCHAR tempdir[64];
	mro::mikefmt(tempdir, _T("%s\\"), folder);
	_tfinddata_t data;
	_tcscat_s(tempdir, filter);
	intptr_t handle = _tfindfirst(tempdir, &data);
	if(handle != -1)
	{
		COleDateTime today = COleDateTime::GetCurrentTime();
		do
		{
			if(	_tcscmp(data.name, _T(".")) == 0 || 
				_tcscmp(data.name, _T("..")) == 0) continue;
			COleDateTime datefile = data.time_write;
			COleDateTimeSpan diff = today - datefile;
			if(diff.GetDays() < keeplessthan) continue; // keep the less than a day
			TCHAR file[128];
			mro::mikefmt(file, _T("%s\\%s"), folder, data.name);
			_tremove(file);
		}
		while(_tfindnext(handle, &data) == 0);
		_findclose(handle);
	}
}

void split_address(	cpairs& lastconfig, const TCHAR* pnode, const int nl, 
					TCHAR* server, const TCHAR* keysvr,
					TCHAR* port, const TCHAR* keyprt,
					TCHAR* domain, const TCHAR* keydom)
{
	server[0] = port[0] = domain[0] = 0;

//	TCHAR configuration[64];

	TCHAR* p = (TCHAR*)_tmemchr(pnode, ':', nl);
	int sini = p? p-pnode:-1;
	if(sini!=-1) // have server and port
	{
		_tmemcpy(server, pnode, sini);
		server[sini] = 0;
		
		p = (TCHAR*)_tmemchr(pnode+sini+1, ':', nl-(sini+1));
		int pini = p? p-pnode:-1;
		if(pini!=-1) // have server, port and domain
		{
			_tmemcpy(port, pnode+sini+1, nl-(pini));
			port[nl-(pini)] = 0;

			_tmemcpy(domain, pnode+pini+1, nl-(pini+1));
			domain[nl-(pini+1)] = 0;
		}
		else
		{
			_tmemcpy(port, pnode+sini+1, nl-(sini+1));
			port[nl-(sini+1)] = 0;
		}
	}
	else // have server
	{
		_tmemcpy(server, pnode, nl);
		server[nl] = 0;
	}

	if(!server[0])	lastconfig.get(keysvr, server); 
	if(!port[0])	lastconfig.get(keyprt, port); 
	if(!domain[0])	lastconfig.get(keydom, domain); 
}

int get_value_len(const TCHAR* p)
{
	TCHAR l = 0;
	int first = 0;
	int single = 0;
	int quotes = 0;
	register const TCHAR* q = p;
	for(; l =* q; ++q) // we start searching from the lprms in order to count its quote
	{
		if(l == _T('"'))
		{
			quotes += quotes ? 1:-1;
			if(first == 0) first = 1;
		}
		if(l == _T('\''))
		{
			single += single ? 1:-1;
			if(first == 0) first = 2;
		}
		if(first == 1 && l==';' && quotes ==0 ) break;
		if(first == 2 && l==';' && single ==0 ) break;
	}

	if(q > p && *q == ';') // do we reach the end?
	{
		for(--q; q!=p; --q)
		{
			l=*q;
			if(l!=' ') break;
		}
	}
	return q - p;
}
int gen_tot_list(TCHAR* p, const int lid, const int nrows, const TCHAR* ex, const int exl)
{
	TCHAR v[16];	int vl = mikefmt(v, _T("%d"), nrows);
	TCHAR y[1024];	int yl = mikefmt(y, _T("%d%s"), nrows, ex && exl ? ex:_T(""));

	TCHAR k[32]; int kl = 0;
	const TCHAR* q=p;

	kl = mikefmt(k, _T("zl%drows"), lid);
	p += cpairs::gen_pair(0, p, k, v, kl, vl);
	kl = mikefmt(k, _T("ztotslst%d"), lid);
	p += cpairs::gen_pair(0, p, k, y, kl, yl);
	
	return p-q;
}

TCHAR* mdefs::cols[64] =   {  
					   _T("col0"), _T("col1"), _T("col2"), _T("col3"), _T("col4"), _T("col5"), 
					   _T("col6"), _T("col7"), _T("col8"), _T("col9"), _T("col10"), _T("col11"), 
					   _T("col12"), _T("col13"), _T("col14"), _T("col15"), _T("col16"), _T("col17"),
					   _T("col18"), _T("col19"), _T("col20"), _T("col21"), _T("col22"), _T("col23"), 
					   _T("col24"), _T("col25"), _T("col26"), _T("col27"), _T("col28"), _T("col29"), 
					   _T("col30"), _T("col31"),_T("col32"),_T("col33"),_T("col34"),_T("col35"),_T("col36"), 
					   _T("col37"), _T("col38"),_T("col39"),_T("col40"),_T("col41"),_T("col42"),_T("col43"), 
					   _T("col44"), _T("col45"),_T("col46"),_T("col47"),_T("col48"),_T("col49"),_T("col50"), 
					   _T("col51"),_T("col52"),_T("col53"),_T("col54"),_T("col55"),_T("col56"),_T("col57"), 
					   _T("col58"),_T("col59"),_T("col60"),_T("col61"),_T("col62"),_T("col63")
					   };
int mdefs::colslen[64] =   {
						4,4,4,4,4,4,4,4,4,4, 5,5,5,5,5,5,5,5,5,5, 
						5,5,5,5,5,5,5,5,5,5, 5,5,5,5,5,5,5,5,5,5, 
						5,5,5,5,5,5,5,5,5,5, 5,5,5,5,5,5,5,5,5,5, 
						5,5,5,5
					};

TCHAR* mdefs::vars[64] =   {  
					   _T("var0"), _T("var1"), _T("var2"), _T("var3"), _T("var4"), _T("var5"), 
					   _T("var6"), _T("var7"), _T("var8"), _T("var9"), _T("var10"), _T("var11"), 
					   _T("var12"), _T("var13"), _T("var14"), _T("var15"), _T("var16"), _T("var17"),
					   _T("var18"), _T("var19"), _T("var20"), _T("var21"), _T("var22"), _T("var23"), 
					   _T("var24"), _T("var25"), _T("var26"), _T("var27"), _T("var28"), _T("var29"), 
					   _T("var30"), _T("var31"),_T("var32"),_T("var33"),_T("var34"),_T("var35"),_T("var36"), 
					   _T("var37"), _T("var38"),_T("var39"),_T("var40"),_T("var41"),_T("var42"),_T("var43"), 
					   _T("var44"), _T("var45"),_T("var46"),_T("var47"),_T("var48"),_T("var49"),_T("var50"), 
					   _T("var51"),_T("var52"),_T("var53"),_T("var54"),_T("var55"),_T("var56"),_T("var57"), 
					   _T("var58"),_T("var59"),_T("var60"),_T("var61"),_T("var62"),_T("var63")
					   };
int mdefs::varslen[64] =   {
						4,4,4,4,4,4,4,4,4,4, 5,5,5,5,5,5,5,5,5,5, 
						5,5,5,5,5,5,5,5,5,5, 5,5,5,5,5,5,5,5,5,5, 
						5,5,5,5,5,5,5,5,5,5, 5,5,5,5,5,5,5,5,5,5, 
						5,5,5,5
					};
}
