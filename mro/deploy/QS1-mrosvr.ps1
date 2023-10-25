remove-item c:\pacific\QS1\services\QS1gatXXX.exe 
rename-item c:\pacific\QS1\services\QS1gat.exe 	c:\pacific\QS1\services\QS1gatXXX.exe 	
#copy-item c:\pacific\deploy\QS1\mrosvr.exe	c:\pacific\QS1\services\QS1gat.exe
robocopy c:\pacific\deploy\QS1\mrosvr.exe c:\pacific\QS1\services\QS1gat.exe