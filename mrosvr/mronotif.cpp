#include "stdafx.h"

//#include "mroctrl.h"
#include "SessionMan.h"

/**
 *	the first notification can be handle by the dictionary or the gate service
 *	we choose the dictionary for two reasons, 1) define a one and only one first
 *	step to begin the notification series and 2) the dictionary has less activity
 *	than the gate so we try to balance performance, for the nature of the notification
 *	process it cannot generate exceptions cause doint the things must not be stop 
 *	because it cannot notify the action
 */
/*void CSessionMan::notify_framework(	const TCHAR* model, const TCHAR* service, 
									const TCHAR* type, const TCHAR* comp, 
									const TCHAR* func, const TCHAR* params, const bool donesrc)
{
	try
	{
		TCHAR q[1024];
		// we must notify that the rights have change to any cache involved
		TCHAR addr[32]; _basics.get(_T("locaddr"), addr, 7, 31);
		int port = _basics.getint(_T("locport"), 7);
		mikefmt(q,	_T("exec core.dbo.ins_notification '%s','%s:%d','*','%s','%s','%s','%s','%s','%s';"),
					model, addr, port, service, type, comp, func, params, donesrc ? _T("1"):_T("0"));
		getconnection(con);
		con.execute(q);
	}
	catch(_com_error &e)	{				}
	catch(CException *e)	{ e->Delete();	}
	catch(mroerr& e)		{				}
	catch(...)				{				}
}
*/
