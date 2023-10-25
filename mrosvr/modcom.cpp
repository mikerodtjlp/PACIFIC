#include "stdafx.h"

#include "modcom.h"

CLSID clsids[] = {	
					{0x32ceaaa5, 0x9144, 0x4f6a, 0xa1, 0x49, 0x68, 0x87, 0x19, 0xcc, 0xc2, 0x25},
					{0xe86d7bd3, 0xee4f, 0x46ce, 0x9b, 0xd, 0x50, 0x17, 0x15, 0x43, 0x86, 0x51},
//					{0x93268767, 0xf55d, 0x4f1c, 0x83, 0x8c, 0x13, 0x8e, 0x79, 0xc5, 0x9f, 0xf1},
					{0x125ca1e0, 0xaf70, 0x41ea, 0xba, 0xd5, 0x30, 0x21, 0x54, 0x9b, 0xdc, 0xb2},
					{0x8ef41531, 0x61d4, 0x4873, 0x92, 0xa4, 0x2a, 0xd3, 0xab, 0x37, 0xc, 0xea},
					{0xf032eead, 0x6857, 0x424b, 0xb0, 0xf, 0xa1, 0x2b, 0x9e, 0xc1, 0x22, 0xeb},
					{0xe71f3066, 0x6fa5, 0x480e, 0xb1, 0xd5, 0xa6, 0x2b, 0x5f, 0x5, 0x32, 0x98},
//					{0x4f8ad711, 0x15fe, 0x4b8f, 0x96, 0x69, 0x12, 0xb, 0xf, 0xac, 0x8a, 0xa6}
				};

TCHAR* coms[] =	{ 
					_T("dcsex1.comobj"), 
					_T("dcsrp001.ObjRep"), 
//					_T("dcsutl.SeekObj"), 
					_T("dcswrk01.COMMDCS"),
					_T("dcswrk02.COMMDCS"), 
					_T("dcswrk03.COMMDCS"),
			 		_T("mroctrl.SessionMan"), 
//					_T("mroutl.MROCOM")
				};

TCHAR* libraries[] =	{ 
					_T("dcsex1.dll"), 
					_T("dcsrp001.dll"), 
//					_T("dcsutl.dll"), 
					_T("dcswrk01.dll"),
					_T("dcswrk02.dll"), 
					_T("dcswrk03.dll"),
			 		_T("mroctrl.dll"), 
//					_T("mroutl.dll")
				};

int comparecom(TCHAR **arg1, TCHAR **arg2)
{
	return _tcscmp(*arg1, *arg2);
}

int find_com2exec(const TCHAR* key, const int keylen)
{
	/*TCHAR **result = (TCHAR**)bsearch(	(TCHAR*)&key, 
										(TCHAR*)&coms, 
										MAXCOMS,
										sizeof(TCHAR *), 
										(int (*)(const void*, const void*))comparecom
									  );
	require(result == 0 || *result == 0, key);
	return &(*result) - &(coms[0]);*/
	/*if(keylen == ZCTROBJLEN && _tmemcmp(key,ZCTROBJ, ZCTROBJLEN) == 0) return 5;
	if(keylen == ZDCSWK1LEN)
	{
		if(_tmemcmp(key,ZDCSWK1, ZDCSWK1LEN) == 0) return 2;
		if(_tmemcmp(key,ZDCSWK2, ZDCSWK2LEN) == 0) return 3;
		if(_tmemcmp(key,ZDCSWK3, ZDCSWK3LEN) == 0) return 4;
	}
	if(keylen == ZDCSEX1LEN && _tmemcmp(key,ZDCSEX1, ZDCSEX1LEN) == 0) return 0;
	if(keylen == ZDCSREPLEN && _tmemcmp(key,ZDCSREP, ZDCSREPLEN) == 0) return 1;*/
	return 5;
}

HRESULT __stdcall mroCoCreateInstance(  LPCTSTR szDllName,  
										IN REFCLSID rclsid,  
										IUnknown* pUnkOuter,
										IN REFIID riid,  
										OUT LPVOID FAR* ppv)
{
  HRESULT hr = REGDB_E_KEYMISSING;

  HMODULE hDll = ::LoadLibrary(szDllName);
  if (hDll == 0)
    return hr;

  typedef HRESULT (__stdcall *pDllGetClassObject)(IN REFCLSID rclsid, 
                   IN REFIID riid, OUT LPVOID FAR* ppv);

  pDllGetClassObject GetClassObject = 
     (pDllGetClassObject)::GetProcAddress(hDll, "DllGetClassObject");
  if (GetClassObject == 0)
  {
	//TCHAR err[128];
	//mikefmt(err, _T("%ld"), GetLastError());
	//AfxMessageBox(err);
    ::FreeLibrary(hDll);
    return hr;
  }

  IClassFactory *pIFactory;
  hr = GetClassObject(rclsid, IID_IClassFactory, (LPVOID *)&pIFactory);
  if (!SUCCEEDED(hr))
    return hr;

  hr = pIFactory->CreateInstance(pUnkOuter, riid, ppv);
  pIFactory->Release();

  return hr;
}

bool IModpass::mroCreateDispatch(LPCTSTR szDllName,  REFCLSID clsid, COleException* pError)
{
	ASSERT(m_lpDispatch == NULL);

	m_bAutoRelease = TRUE;  // good default is to auto-release

	// create an instance of the object
	LPUNKNOWN lpUnknown = NULL;
	SCODE sc = mroCoCreateInstance(szDllName, clsid, NULL, IID_IUnknown, (LPVOID *)&lpUnknown);
	if(FAILED(sc)) goto Failed;

	// query for IDispatch interface
	HRESULT res = lpUnknown->QueryInterface(IID_IDispatch, (void**)&m_lpDispatch);
	if(FAILED(sc)) goto Failed;
	if(m_lpDispatch == NULL) goto Failed;

	lpUnknown->Release();
	ASSERT(m_lpDispatch != NULL);
	return TRUE;

Failed:
	lpUnknown->Release();
	if (pError != NULL) pError->m_sc = sc;

	return FALSE;
}

/////////////////////////////////////////////////////////////////////////////
// IModpass operations

/*void IModpass::SetParameters(LPCTSTR p_strParameters)
{
	static BYTE parms[] = VTS_BSTR;
	InvokeHelper(0x1, DISPATCH_METHOD, VT_EMPTY, NULL, parms, p_strParameters);
}

CString IModpass::GetParameters()
{
	CString result;
	InvokeHelper(0x2, DISPATCH_METHOD, VT_BSTR, (void*)&result, NULL);
	return result;
}

short IModpass::DoOk(LPCTSTR p_strParameters)
{
	short result;
	static BYTE parms[] = VTS_BSTR;
	InvokeHelper(0x3, DISPATCH_METHOD, VT_I2, (void*)&result, parms, p_strParameters);
	return result;
}

void IModpass::SetBasics(LPCTSTR p_strParameters)
{
	static BYTE parms[] = VTS_BSTR;
	InvokeHelper(0x4, DISPATCH_METHOD, VT_EMPTY, NULL, parms, p_strParameters);
}*/

//CString IModpass::GetValuesToChange()
//{
//	CString result;
//	InvokeHelper(0x5, DISPATCH_METHOD, VT_BSTR, (void*)&result, NULL);
//	return result;
//}

//CString IModpass::getlog()
//{
//	CString result;
//	InvokeHelper(0x6, DISPATCH_METHOD, VT_BSTR, (void*)&result, NULL);
//	return result;
//}
