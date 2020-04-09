ECHO APPVEYOR_REPO_BRANCH: %APPVEYOR_REPO_BRANCH%
ECHO APPVEYOR_REPO_TAG: %APPVEYOR_REPO_TAG%
ECHO APPVEYOR_BUILD_NUMBER : %APPVEYOR_BUILD_NUMBER%
ECHO APPVEYOR_BUILD_VERSION : %APPVEYOR_BUILD_VERSION%
Call Tools\nuget.exe restore ..\Slimsy.sln

cd buildpackage

SET toolsFolder=%CD%\tools\
IF NOT EXIST "%toolsFolder%" (
	MD tools
)

IF NOT EXIST "%toolsFolder%vswhere.exe" (
	ECHO vswhere not found - fetching now
	nuget install vswhere -Version 2.8.4 -Source nuget.org -OutputDirectory tools
)

FOR /f "delims=" %%A in ('dir "%toolsFolder%vswhere.*" /b') DO SET "vswhereExePath=%toolsFolder%%%A\"
MOVE "%vswhereExePath%tools\vswhere.exe" "%toolsFolder%vswhere.exe"

SETLOCAL EnableDelayedExpansion

set vswherestr=^"!%CD%\tools\vswhere.exe^" -latest -prerelease -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe
for /f "usebackq tokens=*" %%i in (`!vswherestr!`) do (  
  set MsBuildDir=%%i
)

ECHO.
ECHO MsBuild is installed in: %MsBuildDir%

CALL "%MsBuildDir%" package.proj %~1

@IF %ERRORLEVEL% NEQ 0 GOTO err
@EXIT /B 0
:err
@EXIT /B 1