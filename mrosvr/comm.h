#pragma once

void get_ip_and_name(SOCKET s, TCHAR* ip, int& iplen);
void close_socket(sClients& clie);
void respond(int index, TCHAR* p, const int len, const bool finalpacket);
bool read_request(sClients& place);
void background_process();

long send_buff_2_client(	sClients& clie, int& packets_out, 
							TCHAR* dtl, const int dtllen, 
							const bool firstpacket,
							const bool finalpacket);
long send_direct_2_client(SOCKET s, 
						  TCHAR* p, const int len, 
						  const bool firstpacket);

