#pragma once

//// Declare a non-thread-safe heap just for this thread:
//extern CWin32Heap stringHeap;
//// Declare a string manager that uses the thread's heap:
//extern CAtlStringMgr stringMgr;	

void initialize_mem_manager();
