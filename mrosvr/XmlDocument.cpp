/****************************************************************/
/*																*/
/*  XmlDocument.cpp												*/
/*																*/
/*  Implementation of the CXmlDocument class.					*/
/*																*/
/*  Programmed by Pablo van der Meer							*/
/*  Copyright Pablo Software Solutions 2003						*/
/*	http://www.pablovandermeer.nl								*/
/*																*/
/*  Last updated: 09 February 2003								*/
/*																*/
/****************************************************************/

#include "stdafx.h"
#include "XmlDocument.h"

#ifdef _DEBUG
#undef THIS_FILE
#endif

CString CXmlElement::empty;

void CXmlElement::get_attribute(CString& attribute__)
{
//	if(m_strAttributes.IsEmpty())
	if(m_strAttributes.GetLength() == 0)
	{
		attribute__ = empty;
		return;
	}
	int pos = m_strAttributes.Find(_T('='));
	if(pos == -1)
	{
		attribute__ = empty;
		return;
	}
	attribute__ = m_strAttributes.Mid(0, pos);
}

void CXmlElement::get_attribute(TCHAR* attribute__)
{
//	if(m_strAttributes.IsEmpty())
	if(m_strAttributes.GetLength() == 0)
	{
		attribute__[0] = _T('\0');
		return;
	}
	int pos = m_strAttributes.Find(_T('='));
	if(pos == -1)
	{
		attribute__[0] = _T('\0');
		return;
	}
	_tmemcpy(attribute__, m_strAttributes.GetBuffer(), pos);
	attribute__[pos] = _T('\0');
}

void CXmlElement::get_attributes(CParameters& attribute__)
{
	attribute__.set_value(m_strAttributes);
}

void CXmlElement::get_attributes(CString& attributes__)
{
	attributes__ = m_strAttributes;
}

CString CXmlElement::get_attributes()
{
	return m_strAttributes;
}
//CString CXmlElement::get_attribute()
//{
//	if(m_strAttributes.IsEmpty()) return empty;
//	int pos = m_strAttributes.Find(_T('='));
//	if(pos == -1) return empty;
//	CString result = m_strAttributes.Mid(0, pos);
//	return result;
//}

int CXmlElement::get_attributes(void* attributes__, 
								const int nelems, 
								const int size)
{
	int nattrs = 0;
	if(m_strAttributes.IsEmpty()) return nattrs;
	TCHAR* begin = m_strAttributes.GetBuffer();
	int len = m_strAttributes.GetLength();
	TCHAR* end = begin + len;
	
	TCHAR* p = begin;
	TCHAR* q = begin;
	for(; nattrs < nelems;)
	{
		q = (TCHAR*)_tmemchr(q, _T('='), end - q);
		if(!q) return nattrs;
		for(p = q - 1; (isalnum(*p) || *p == _T('_')) && p !=  begin; --p);
		if(p != begin) ++p;

		int lon = q - p;

		TCHAR* dest = (TCHAR*)(((TCHAR*)attributes__)+(size*nattrs));
		_tmemcpy(dest, p, lon);
		dest[lon] = 0;

		++q;
		++nattrs;
	}
	return 0;
}

int CXmlElement::get_attrs_vals(void* attributes__, int* alns__,
								void* vals__, int* lens__, 
								int* posk, int* posv, 
								const size_t nelems, 
								const size_t sizeattrs, 
								const size_t sizevals)
{
	int nattrs = 0;
	if(m_strAttributes.GetLength() == 0) return nattrs;
	TCHAR* begin = m_strAttributes.GetBuffer();
	int len = m_strAttributes.GetLength();
	TCHAR* end = begin + len;
	
	int kl = 0;
	int vl = 0;

	TCHAR* p = begin;
	TCHAR* q = begin;
	for(; nattrs < nelems;)
	{
		posk[nattrs] = kl;
		posv[nattrs] = vl;

		q = (TCHAR*)_tmemchr(q, _T('='), sizeof(TCHAR)*(end - q));
		if(!q) return nattrs;
		int spaces = 0;
		if((*(q-1) < 255 && isspace(*(q-1))))
		{
			for(p = q - 1; ((*p < 255 && isspace(*p))) && p !=  begin; --p,++spaces);
		} else p = q - 1;

		for(; (isalnum(*p) || *p == _T('_')) && p !=  begin; --p);
		if(p != begin) ++p;

		int lon = (q - p)-spaces;

		TCHAR* d = (TCHAR*)(((TCHAR*)attributes__)+posk[nattrs]);
		if(lon >= sizeattrs) lon = (sizeattrs-1);
		if(lon == 5)		{set4ch(d,*p,*(p+1),*(p+2),*(p+3)); d[4]=*(p+4);} 
		else if(lon == 7)	{set4ch(d,*p,*(p+1),*(p+2),*(p+3)); set4ch(&d[4],*(p+4),*(p+5),*(p+6),0);} 
		else if(lon == 8)	{set4ch(d,*p,*(p+1),*(p+2),*(p+3)); set4ch(&d[4],*(p+4),*(p+5),*(p+6),*(p+7));} 
		else _tmemcpy(d, p, lon);
		d[lon]=0;
		kl += lon + 1;

		alns__[nattrs] = lon; 

		++q;

		q = (TCHAR*)_tmemchr(q, _T('"'), sizeof(TCHAR)*(end - q));
		if(!q) return nattrs;
		++q;
		p = q;
		q = (TCHAR*)_tmemchr(q, _T('"'), sizeof(TCHAR)*(end - q));
		if(!q) return nattrs;

		lens__[nattrs] = lon = q - p;

		d = (TCHAR*)(((TCHAR*)vals__)+posv[nattrs]);
		if(lon >= sizevals) lon = (sizevals-1);
		if(lon == 5)		{set4ch(d,*p,*(p+1),*(p+2),*(p+3)); d[4]=*(p+4);} 
		else if(lon == 7)	{set4ch(d,*p,*(p+1),*(p+2),*(p+3)); set4ch(&d[4],*(p+4),*(p+5),*(p+6),0);} 
		else if(lon == 8)	{set4ch(d,*p,*(p+1),*(p+2),*(p+3)); set4ch(&d[4],*(p+4),*(p+5),*(p+6),*(p+7));} 
		else _tmemcpy(d, p, lon);
		d[lon]=0;
		vl += lon + 1;

		++q;
		++nattrs;
	}
	return nattrs;
}

CString CXmlElement::GetValue(const TCHAR* attribute, const int attrlen)
{
	TCHAR att[128];
	_tmemcpy(att, attribute, attrlen); set2ch(&att[attrlen], '=',0);
	int pos = m_strAttributes.Find(att);
	if(pos >= 0)
	{
		CString ret(m_strAttributes.GetBuffer()+(pos + attrlen + 2));
		int n = ret.Find('"');
		if(n >= 0) ret = ret.Left(n);
		return ret;
	}
	return empty;
}

/*void CXmlElement::GetValue(const TCHAR* attribute, , const int attrlen, CString& ret)
{
	ret = m_strAttributes;
	TCHAR att[128];
	_tmemcpy(att, attribute, attrlen); set2ch(&att[attrlen], '=',0);
	int pos = m_strAttributes.Find(att);
	if(pos >= 0)
	{
		ret.Delete(0,pos + attrlen + 2);
		int n = ret.Find('"');
		if (n >= 0)	ret = ret.Left(n);
	}
	else ret = empty;
}*/

int CXmlElement::GetValue(const TCHAR* attribute, 
									const int attrlen, 
									TCHAR* reto, const int maxret)
{
	TCHAR att[128];
	_tmemcpy(att, attribute, attrlen); set2ch(&att[attrlen], '=',0);
	int pos = m_strAttributes.Find(att);
	if(pos >= 0)
	{
		CString ret(m_strAttributes.GetBuffer()+(pos + attrlen + 2));
		int n = ret.Find('"');
		if (n >= 0)	ret = ret.Left(n);
		int len = ret.GetLength();
		if(len > maxret) len = maxret;
		_tmemcpy(reto, ret.GetBuffer(), len + 1);
		return len;
	}
	reto[0]=0;
	return 0;
}
void CXmlElement::GetValue(const TCHAR* attribute, const int attrlen, int* reto)
{
	TCHAR tmp[16];
	GetValue(attribute,attrlen,tmp,15);
	*reto = _tstoi(tmp);
}
void CXmlElement::GetValue(const TCHAR* attribute, const int attrlen, bool* reto)
{
	TCHAR tmp[16];
	GetValue(attribute,attrlen,tmp,15);
	*reto = _tstoi(tmp);
}
bool CXmlElement::isactv(const TCHAR* attribute, const int attrlen)
{
	TCHAR tmp[16];
	GetValue(attribute,attrlen,tmp,15);
	return _tstoi(tmp);
}

CString CXmlDocument::empty;

CXmlDocument::CXmlDocument()
{
	m_nLevel = -1;
}

CXmlDocument::~CXmlDocument()
{
	DeleteContents();
}


/********************************************************************/
/*																	*/
/* Function name : DeleteContents									*/
/* Description   : Initialize variables to their initial values.	*/
/*																	*/
/********************************************************************/
void CXmlDocument::DeleteContents()
{
	// clean up any previous data
	while(!m_RootElement.m_ChildElements.IsEmpty())
	{
		delete m_RootElement.m_ChildElements.RemoveHead();
	}
	m_pCurElement = &m_RootElement;
	m_pCurElement->m_pParentElement = NULL;
	m_RootElement.m_strName = empty;
	m_RootElement.m_strData = empty;
	m_RootElement.m_strAttributes = empty;
	m_RootElement.m_strFind = empty;
	m_RootElement.m_posFind = NULL;
}

TCHAR START = '<';
TCHAR END	= '>';
TCHAR ADMI	= '!';
TCHAR QUES	= '?';
TCHAR SLASH = '/';
/********************************************************************/
/*																	*/
/* Function name : Parse											*/
/* Description   : Parse XML data.									*/
/*																	*/
/********************************************************************/
BOOL CXmlDocument::Parse(LPCTSTR lpszString)
{
	// clean previous document data
	DeleteContents();

	auto bInsideTag = false;

	TCHAR* strTag;
	int iTag = 0;
	TCHAR* pData = nullptr;
	int iData = 0;
	TCHAR* result = nullptr;
	int reslen=0;

	register TCHAR ch = 0;
	for(register TCHAR* p = (TCHAR*)lpszString; ch=*p; ++p)
    {
		// begin of tag ?
		if(ch == START)
        {
			strTag = p;
			iTag = 1;

			// add data to element
			if(iData && pData)
			{
				m_pCurElement->m_strData.SetString(pData, iData);
				// trim spaces
				//if(*pData == ' ')			m_pCurElement->m_strData.TrimLeft();
				//if(pData[iData-1] == ' ')	m_pCurElement->m_strData.TrimRight();
			}

			// clear data
			iData = 0;
         
			// processing tag...
			bInsideTag = true;
            continue;        
        }
		// end of tag ?
        if(ch == END)
        {
			++iTag;
			pData = &strTag[iTag];

			// determine type and name of the tag
			if(strTag[0] == '\0') continue;
			int nType = ValidateTag(strTag, iTag, result, reslen=0);
			// skip errors/comments/declaration
			if (nType == -1) continue;
			if (reslen == 0 || result == nullptr) continue;

			// start or start-end tag -> add new element
			if(nType == 0 || nType == 2)
			{
				// currently processing root element ?
				if (m_RootElement.m_strName.GetLength() == 0)
				{
					// split name and attributes
					TCHAR* p = (TCHAR*)_tmemchr(result, ' ', reslen);
					int nPos = p ? p - result : -1;
					if (nPos != -1)
					{
						// set properties of root element
						m_RootElement.m_strName.SetString(&result[0], nPos);
						m_RootElement.m_strAttributes.SetString(&result[nPos+1], reslen-(nPos+1));
						// trim spaces
						m_RootElement.m_strAttributes.TrimLeft();
						m_RootElement.m_strAttributes.TrimRight();
					}
					else m_RootElement.m_strName.SetString(result, reslen);
				}
				else
				{
					// create new element
					CXmlElement *pElement = new CXmlElement;

					pElement->m_pParentElement = m_pCurElement;
					
					// split name and attributes
					TCHAR* p = (TCHAR*)_tmemchr(result, ' ', reslen);
					int nPos = p ? p - result : -1;
					if (nPos != -1)
					{
						// set properties of current element
						pElement->m_strName.SetString(&result[0], nPos);
						// sometimes the user press enter(1310) after the label name
						pElement->m_strName.TrimRight();

						pElement->m_strAttributes.SetString(&result[nPos+1], reslen-(nPos+1));
						// trim spaces
						pElement->m_strAttributes.TrimLeft();
						pElement->m_strAttributes.TrimRight();
					}
					else pElement->m_strName.SetString(result, reslen);

					m_pCurElement->m_ChildElements.AddTail(pElement);
					m_pCurElement = pElement;
				}
			}

			// end or start-end tag -> finished with current tag
			if(nType == 1 || nType == 2)
			{
				// go back to parent level
				if (m_pCurElement->m_pParentElement != nullptr)
					m_pCurElement = m_pCurElement->m_pParentElement;
			}

			// processing data...
			bInsideTag = false;
            continue;
        }
        
		if(bInsideTag) ++iTag;
        else ++iData;
    }
	return true;
}


/********************************************************************/
/*																	*/
/* Function name : ValidateTag										*/
/* Description   : Determine type and name of the tag.				*/
/*				   0 = start tag									*/
/*				   1 = end tag										*/
/*				   2 = start-end tag								*/
/*				   -1 = comments or declaration						*/
/*																	*/
/********************************************************************/
int CXmlDocument::ValidateTag(const TCHAR* strTag, const int iTag, TCHAR*& result, int& reslen)
{
	result = nullptr;
    register TCHAR ch = 0;
	TCHAR chPrevious = '0';
	
	int nResult = 0;
	//int nCount = 0;

	// determine tag type
	register int i=0;
	for(register TCHAR* p = (TCHAR*)strTag; i<iTag; ++p,++i)
    {
        // get next character
		ch = *p;

		// sometimes the user press enter(1310) after the label name
		if(ch == 13 || ch == 10) ch = ' ';

		// skip comments '<!' and declaration '<?'
        if ((chPrevious == START && ch == ADMI) || 
			(chPrevious == START && ch == QUES))
		{
            return -1;
		}
        else
		// is it an end-tag '</' ?
        if(chPrevious == START && ch == SLASH) 
        {
            nResult = 1;
        }
        else
		// is it a start-end-tag '<..../>' ?
        if(chPrevious == SLASH && ch == END) 
        {
            nResult = 2;
			// remove last character
			--reslen;
        }
        else 
		if(ch != START && ch != END)
		{
			// add character
			if(result == nullptr) result = p;
			++reslen;
        }
        chPrevious = ch;
    }
	return nResult;
}



/********************************************************************/
/*																	*/
/* Function name : GetFirstChild									*/
/* Description   : Get first child of element.						*/
/*																	*/
/********************************************************************/
CXmlElement *CXmlDocument::GetFirstChild(CXmlElement *pElement) 
{
	pElement->m_posFind = NULL;
	
	POSITION pos = pElement->m_ChildElements.GetHeadPosition();
	if (pos != NULL)
	{
		CXmlElement *pResult = (CXmlElement *)pElement->m_ChildElements.GetNext(pos);
		pElement->m_posFind = pos;
		return pResult;
	}
	return NULL;
}


/********************************************************************/
/*																	*/
/* Function name : GetNextSibling									*/
/* Description   : Get next child of specified element.				*/
/*																	*/
/********************************************************************/
CXmlElement *CXmlDocument::GetNextSibling(CXmlElement *pElement) 
{
	if (pElement->m_posFind)
		return (CXmlElement *)pElement->m_ChildElements.GetNext(pElement->m_posFind);
	else
		return NULL;
}


/********************************************************************/
/*																	*/
/* Function name : FindElement										*/
/* Description   : Find first occurence of specified tag.			*/
/*																	*/
/********************************************************************/
CXmlElement *CXmlDocument::FindElement(CXmlElement *pElement, LPCTSTR lpszName, const int namelen) 
{
	pElement->m_posFind = NULL;
	
	pElement->m_strFind = lpszName;
	
	POSITION pos = pElement->m_ChildElements.GetHeadPosition();
	while (pos != NULL)
	{
		CXmlElement *pResult = (CXmlElement *)pElement->m_ChildElements.GetNext(pos);
		if(_tmemcmp(pResult->m_strName.GetBuffer(), lpszName, namelen) == 0)
		{
			pElement->m_posFind = pos;
			return pResult;
		}
	}
	return NULL;
}


/********************************************************************/
/*																	*/
/* Function name : FindNextElement									*/
/* Description   : Find next occurence of specified tag				*/
/*																	*/
/********************************************************************/
CXmlElement *CXmlDocument::FindNextElement(CXmlElement *pElement) 
{
	while(pElement->m_posFind != NULL)
	{
		CXmlElement *pResult = (CXmlElement *)pElement->m_ChildElements.GetNext(pElement->m_posFind);
		if(_tmemcmp(pResult->m_strName.GetBuffer(), pElement->m_strFind.GetBuffer(), pResult->m_strName.GetLength()) == 0)
		{
			return pResult;
		}
	}
	return NULL;
}


/********************************************************************/
/*																	*/
/* Function name : AddElement										*/
/* Description   : Add new element									*/
/*																	*/
/********************************************************************/
/*CXmlElement *CXmlDocument::AddElement(CXmlElement *pElement, LPCTSTR lpszName, LPCTSTR lpszData, LPCTSTR lpszAttributes) 
{
	CXmlElement *pNewElement = new CXmlElement;

	pNewElement->m_strName = lpszName;
	pNewElement->m_strName.TrimLeft();
	pNewElement->m_strName.TrimRight();

	if (lpszData)
	{
		pNewElement->m_strData = lpszData;
		pNewElement->m_strData.Replace(_T("&"), _T("&amp;"));
		pNewElement->m_strData.Replace(_T("<"), _T("&lt;"));
		pNewElement->m_strData.Replace(_T(">"), _T("&gt;"));
	}
	if (lpszAttributes)
		pNewElement->m_strAttributes = lpszAttributes;
	
	pElement->m_ChildElements.AddTail(pNewElement);

	return pNewElement;
}*/


/********************************************************************/
/*																	*/
/* Function name : Generate											*/
/* Description   : Generate a XML string from elements				*/
/*																	*/
/********************************************************************/
/*CString CXmlDocument::Generate()
{
	CString strResult;

	strResult = _T("<?xml version=\"1.0\"?>\r\n");

	CString strTag;

	m_nLevel = -1;
	CreateTag(&m_RootElement, strTag);

	strResult += strTag;
	return strResult;
}*/


/********************************************************************/
/*																	*/
/* Function name : CreateTag										*/
/* Description   : Create tag and tags from all child elements		*/
/*																	*/
/********************************************************************/
BOOL CXmlDocument::CreateTag(CXmlElement *pElement, CString &strResult)
{
	int i;

	++m_nLevel;

	// make sure we start empty
	strResult = empty;

	// add spaces before start-tag
	for (i=0; i<m_nLevel; i++)
		strResult += _T(" ");

	// add start-tag
	strResult += _T("<");
	strResult += pElement->m_strName;

	if (!pElement->m_strAttributes.IsEmpty())
	{
		strResult += _T(" ");
		strResult += pElement->m_strAttributes;
	}
	
	strResult += _T(">");

	if (!pElement->m_strData.IsEmpty())
	{
		strResult += pElement->m_strData;
	}
	else
	{
		strResult += _T("\r\n");
	}

	// process child elements
	POSITION pos = pElement->m_ChildElements.GetHeadPosition();
	while (pos != NULL)
	{
		CXmlElement *pChildElement = (CXmlElement *)pElement->m_ChildElements.GetNext(pos);

		CString strTag;
		CreateTag(pChildElement, strTag);
		strResult += strTag;
	}
	
	if (pElement->m_strData.IsEmpty())
	{
		// add spaces before end tag
		for (i=0; i<m_nLevel; i++)
			strResult += _T(" ");
	}

	// add end-tag
	strResult += _T("</");
	strResult += pElement->m_strName;
	strResult += _T(">\r\n");

	m_nLevel--;
	return TRUE;
}


/********************************************************************/
/*																	*/
/* Function name : Load												*/
/* Description   : Load document from file							*/
/*																	*/
/********************************************************************/
/*BOOL CXmlDocument::Load(LPCTSTR lpszFileName)
{
//	CWaitCursor waitCursor;

	CString strXML;

	try
	{
		CFile inputFile(lpszFileName, CFile::modeRead);
	
		DWORD dwLength = inputFile.GetLength();

		inputFile.Read(strXML.GetBuffer(dwLength), dwLength);
		strXML.ReleaseBuffer();

		inputFile.Close();
	}
	catch(CFileException *ex)
	{
		ex->Delete();
		return FALSE;
	}

	// remove endofline and tabs
//	strXML.Remove('\n');
//	strXML.Remove('\r');
//	strXML.Remove('\t');

	return Parse(strXML);
}
*/
/********************************************************************/
/*																	*/
/* Function name : Load_from_string												*/
/* Description   : Load document from a string bypassing the file							*/
/*																	*/
/********************************************************************/
BOOL CXmlDocument::Load_from_string(const CString& text)
{
//	CString strXML = text;

	// remove endofline and tabs
//	strXML.Remove('\n');
//	strXML.Remove('\r');
//	strXML.Remove('\t');

//	return Parse(strXML);
	return Parse(text);
}
/********************************************************************/
/*																	*/
/* Function name : Store											*/
/* Description   : Save document to file							*/
/*																	*/
/********************************************************************/
/*BOOL CXmlDocument::Store(LPCTSTR lpszFileName)
{
//	CWaitCursor waitCursor;

	CString strXML = Generate();

	try
	{
		CFile outputFile(lpszFileName, CFile::modeCreate | CFile::modeWrite);
		outputFile.Write(strXML, strXML.GetLength());
		outputFile.Close();
	}
	catch(CFileException *ex)
	{
		ex->Delete();
		return FALSE;
	}
	return TRUE;
}*/

