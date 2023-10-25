// XmlParse.h: interface for the CXmlParser class.
//
//////////////////////////////////////////////////////////////////////

#if !defined(AFX_XMLDOCUMENT_H__7D272C3A_C971_4F51_98D7_09F974F994E3__INCLUDED_)
#define AFX_XMLDOCUMENT_H__7D272C3A_C971_4F51_98D7_09F974F994E3__INCLUDED_

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

class CXmlElement : public CObject
{
public:
	CXmlElement() 
	{
		m_posFind = NULL;
		m_pParentElement = NULL;
	};
	virtual ~CXmlElement() 
	{
		while(!m_ChildElements.IsEmpty())
		{
			delete m_ChildElements.RemoveHead();
		}
	};

	CString m_strName;
	CString m_strAttributes;
	CString m_strData;
	static CString empty;

	CObList m_ChildElements;
	
	CXmlElement *m_pParentElement;

	POSITION m_posFind;
	CString m_strFind;

	CString GetValue(const TCHAR* attribute, const int attrlen);
//	void GetValue(const TCHAR* attribute, const int attrlen, CString& ret);
	int  GetValue(const TCHAR* attribute, const int attrlen, TCHAR* reto, const int maxret);
	void GetValue(const TCHAR* attribute, const int attrlen, int* reto);
	void GetValue(const TCHAR* attribute, const int attrlen, bool* reto);
	bool isactv(const TCHAR* attribute, const int attrlen);

	CString get_attributes();
	void get_attributes(CString& attributes__);
	void get_attributes(CParameters& attributes__);
	int get_attributes(void* attribute__, const int nelems, const int size);

	void get_attribute(CString& attribute__);
	void get_attribute(TCHAR* attribute__);
	int  get_attrs_vals(void* attribute__, int* alns__, void* vals__, int* lens__, int* posk__, int* posv__, const size_t nelems, const size_t sizeattrs, const size_t sizevals);
	void get_label(CString& label__) { label__ = m_strName; }
	int get_label(TCHAR* label__, const int maxvallen = 0) 
	{ 
		int len = m_strName.GetLength();  
		if(maxvallen > 0 && len > maxvallen) len = maxvallen;
		_tmemcpy(label__, m_strName.GetBuffer(), len);
		label__[len] = 0;
		return len; 
	}
};

class CXmlDocument
{
public:
	CXmlDocument();
	CXmlDocument(const CString& text) { Load_from_string(text); }
	virtual ~CXmlDocument();

//	CString Generate();
	BOOL Parse(LPCTSTR lpszString);

//	BOOL Load(LPCTSTR lpszFileName);
	BOOL Load_from_string(const CString& text);
//	BOOL Store(LPCTSTR lpszFileName);

	CXmlElement *GetFirstChild(CXmlElement *pElement);
	CXmlElement *GetNextSibling(CXmlElement *pElement);
	
	CXmlElement *GetRootElement() { return &m_RootElement; }
	
	CXmlElement *FindElement(CXmlElement *pElement, LPCTSTR lpszName, const int namelen);
	CXmlElement *FindNextElement(CXmlElement *pElement);

//	CXmlElement *AddElement(CXmlElement *pElement, LPCTSTR lpszName, LPCTSTR lpszData = NULL, LPCTSTR lpszAttributes = NULL);

	void DeleteContents();
protected:
	int m_nLevel;
	int ValidateTag(const TCHAR* strTag, const int iTag, TCHAR*& result, int& reslen);
	BOOL CreateTag(CXmlElement *pElement, CString &strTag);
	
	static CString empty;

	CXmlElement m_RootElement;
	CXmlElement *m_pCurElement;
};

#endif // !defined(AFX_XMLDOCUMENT_H__7D272C3A_C971_4F51_98D7_09F974F994E3__INCLUDED_)
