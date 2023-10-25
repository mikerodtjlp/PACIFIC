// stdafx.h : include file for standard system include files,
// or project specific include files that are used frequently,
// but are changed infrequently

#pragma once

#ifndef _SECURE_ATL
#define _SECURE_ATL 1
#endif

#ifndef VC_EXTRALEAN
#define VC_EXTRALEAN            // Exclude rarely-used stuff from Windows headers
#endif

#include "targetver.h"

#define _ATL_CSTRING_EXPLICIT_CONSTRUCTORS      // some CString constructors will be explicit

// turns off MFC's hiding of some common and often safely ignored warning messages
#define _AFX_ALL_WARNINGS

#include <afxwin.h>         // MFC core and standard components
#include <afxext.h>         // MFC extensions

#include <afxdtctl.h>		// MFC support for Internet Explorer 4 Common Controls
#ifndef _AFX_NO_AFXCMN_SUPPORT
#include <afxcmn.h>			// MFC support for Windows Common Controls
#endif // _AFX_NO_AFXCMN_SUPPORT

//#include "atlstr.h"

//#define MRO_IOCP_IMP 

#include "mro_mem.h"

#include "mro_defs.h"
#include "mro_errs.h"
#include "mroparms.h"
#include "util.h"
using namespace mro;
#include "mrosock.h"
#include "database.h"

#include "modcom.h"
#include "UTF16File.h"

#include "mro_cmd.h"
#include "mroiis.h"
#include "xmldocument.h"

#include "defs.h"
#include "clie.h"
#include "proc.h"
#include "exec.h"
#include "sched.h"
#include "comm.h"
#include "mro_log.h"
#include "mro_err.h"
#include "mro_sys.h"
#include "mro_dll.h"
#include "mro_sql.h"
#include "mro_com.h"

#include <map>
#include <set>
#include "mro_fns.h"