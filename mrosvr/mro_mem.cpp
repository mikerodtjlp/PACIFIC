#include "stdafx.h"

/************************************************************************************
* description   : mro server
* purpose       : execute any transacitions from clients
* author        : miguel rodriguez
* date creation : 20 junio 2004
* change        : 1 marzo call coms direct from the function execute instead of CMroCom::call_com)
* change        : 10 febrero  change to global queue array, and the access controled by semaphores
**************************************************************************************/

#include "mrosvr.h"
#include "mro_mem.h"

//// Declare a non-thread-safe heap just for this thread:
//CWin32Heap stringHeap( HEAP_NO_SERIALIZE, 0, 0 );
//// Declare a string manager that uses the thread's heap:
//CAtlStringMgr stringMgr( &stringHeap );	

void initialize_mem_manager()
{
	mro::memhelper::initialize_memhelper(MAX_PROCS);
}
