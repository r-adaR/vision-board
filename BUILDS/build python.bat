REM make build dir
echo [VISION BOARD] deleting old build directory if existing
if exist BUILT_EXE_IN_DIST rmdir /s /q BUILT_EXE_IN_DIST
mkdir BUILT_EXE_IN_DIST
cd BUILT_EXE_IN_DIST

REM Activate virtual environment
echo [VISION BOARD] activating virtual environment
call ..\..\venv\Scripts\activate

REM Build python with PyInstaller
echo [VISION BOARD] building...
pyinstaller --paths=..\..\BACKEND\MAIN_METHOD --onefile ..\..\BACKEND\MAIN_METHOD\server.py ^
--add-data "..\..\BACKEND\MAIN_METHOD\PieceClassifierHard4.pt;." ^
--add-data "..\..\BACKEND\MAIN_METHOD\BoardLocatorHard.pt;." ^
--name=vision_board_server ^
--noconsole

echo BUILD FINISHED!!! The exe file you're looking for is inside the dist folder
pause