#pragma once

#ifndef __AFXWIN_H__
	#error include 'stdafx.h' before including this file for PCH
#endif

#include "resource.h"		// main symbols

// Check windows
#if _WIN32 || _WIN64
#if _WIN64
#define ENVIRONMENT64
#else
#define ENVIRONMENT32
#endif
#endif

void goodbye(const TCHAR* message, const int msglen, const TCHAR* extrainfo =  _T(""), const int extlen=0);
unsigned __stdcall backgroud(LPVOID pParam);

extern int PRIO_LST;
extern int PRIO_DSP;
extern int PRIO_SCH;
extern int PRIO_PRC;
extern int PRIO_RSP;
extern int PRIO_NOT;

extern DWORD ggapreqs;
extern bool debug_mode;

extern bool ajst_lst;
extern bool ajst_dsp;
extern bool ajst_sch;
extern bool* ajst_prc;
extern bool ajst_rsp;
extern bool ajst_not;

