@echo off
echo Initializing program... please wait

set "APP1=vision_board_server.exe"
set "APP2=vision_board.exe
set "RUNNING=0"

tasklist /FI "ImageName eq %APP1%" | find /I "%APP1%" >nul
if %ERRORLEVEL%==0 (
	set "RUNNING=1"
)

tasklist /FI "ImageName eq %APP2%" | find /I "%APP2%" >nul
if %ERRORLEVEL%==0 (
	set "RUNNING=1"
)

if %RUNNING%==1 (
	echo one or more apps are already open
) else (
	start "" "%~dp0\python\vision_board_server.exe"
	start "Tic-Tac-Toe X-Treme-O!" "%~dp0\unity\vision_board.exe"
)