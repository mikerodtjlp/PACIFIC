#pragma once

extern HANDLE* incomings;
extern HANDLE* incomings2;
extern HANDLE* requests;
extern HANDLE* requests2;
extern HANDLE* responses;
extern HANDLE* responses2;

void initialize_requests();
void initialize_socket(SOCKET& socklistener);
void listener(SOCKET socklistener);
void initialize_threads();
