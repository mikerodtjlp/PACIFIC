set winsock = createobject("MSWinsock.Winsock")

sub shutdown (port)

	winsock.Remotehost = "127.0.0.1"
	winsock.RemotePort = port
	winsock.Connect
	Do until winsock.state = 7
		wscript.sleep(16)
	Loop
	winsock.SendData "[zhdr:[retresult:0]][zznfuns:1][zfun00z:[ztypcom:sys][dcsfunc:reset_service]]"
	wscript.sleep(16)
	winsock.Close

end sub

shutdown 8181 ' dic control/dictionary
shutdown 8830 ' gat gate
shutdown 8886 ' app application  
shutdown 8887 ' ap1 application extra one
shutdown 8888 ' rep reports 
shutdown 8889 ' re1 reports extra one
shutdown 8901 ' pkg packaging/fast
