set SERVICEURL=http://r.eim.cc
set APPPATH=/
set VERSION=3.0.0.0
set FILEROOT=Files
set DBSERVER=localhost\SQLEXPRESS
set DBUSER=sa
set DBPWD=123456
set DBNAME=eim_new

set IMPATH=%~dp0..
set RELPATH=%~dp0Release
set INSPATH=%~dp0Install
set TMPPATH=%~dp0temp
set JDK=C:\Program Files\Java\jdk1.6.0
set YUI=%IMPATH%\Tools\yuicompressor-2.4.2\build\yuicompressor-2.4.2.jar
set ZIP=%IMPATH%\Tools\7z.exe

if exist %RELPATH% rd /S /Q %RELPATH%
if exist %TMPPATH% rd /S /Q %TMPPATH%

if exist "%RELPATH%" (
  echo 请删除Release文件夹
  goto end
)

if exist "%TMPPATH%" (
  echo 请删除temp文件夹
  goto end
)

md %TMPPATH%

echo wwwroot\CurrentVersion\Tests > "%TMPPATH%\copy_wwwroot_exclude"
echo wwwroot\Files\Users >> "%TMPPATH%\copy_wwwroot_exclude"
echo wwwroot\Files\trace.txt >> "%TMPPATH%\copy_wwwroot_exclude"
echo wwwroot\Debug.aspx >> "%TMPPATH%\copy_wwwroot_exclude"
echo wwwroot\Debug.aspx.cs >> "%TMPPATH%\copy_wwwroot_exclude"
xcopy "%IMPATH%\wwwroot" "%RELPATH%\wwwroot\" /E /Y /Q /EXCLUDE:%TMPPATH%\copy_wwwroot_exclude
ren "%RELPATH%\wwwroot\CurrentVersion" %VERSION% 

if "%1"=="compress" (
 for /r "%RELPATH%\wwwroot\%VERSION%\Core" %%p in (*.js) do (
    "%JDK%\bin\java.exe" -jar "%YUI%" --charset utf-8 -o "%%p" "%%p"
 )
 for /r "%RELPATH%\wwwroot\%VERSION%\UI" %%p in (*.js) do (
    "%JDK%\bin\java.exe" -jar "%YUI%" --charset utf-8 -o "%%p" "%%p"
 )
)

xcopy "%IMPATH%\Client\bin\Release\data" "%RELPATH%\Client\data\" /E /Y /Q
copy "%IMPATH%\Client\bin\Release\Client.exe" "%RELPATH%\Client\Client.exe"
copy "%IMPATH%\Client\bin\Release\Client.pdb" "%RELPATH%\Client\Client.pdb"
copy "%IMPATH%\Client\bin\Release\Update.exe" "%RELPATH%\Client\Update.exe"
copy "%IMPATH%\Client\bin\Release\Setting.conf" "%RELPATH%\Client\Setting.conf"
copy "%IMPATH%\Client\bin\Release\*.dll" "%RELPATH%\Client\"

%~dp0replace.exe "%RELPATH%\Client\Setting.conf" "<ServiceUrl>[^\x3C\x3E]*</ServiceUrl>" "<ServiceUrl>%SERVICEURL%</ServiceUrl>"
%~dp0replace.exe "%RELPATH%\Client\Setting.conf" "<AppPath>[^\x3C\x3E]*</AppPath>" "<AppPath>%APPPATH%</AppPath>"
%~dp0replace.exe "%RELPATH%\Client\Setting.conf" "<ResPath>[^\x3C\x3E]*</ResPath>" "<ResPath>%VERSION%</ResPath>"

%~dp0replace.exe "%RELPATH%\wwwroot\web.config" "<add key=\x22FileRoot\x22 value=\x22[^\x3C\x3E\x22]*\x22/>" "<add key=\"FileRoot\" value=\"%FILEROOT%\"/>"
%~dp0replace.exe "%RELPATH%\wwwroot\web.config" "<add key=\x22ResPath\x22 value=\x22[^\x3C\x3E\x22]*\x22/>" "<add key=\"ResPath\" value=\"%VERSION%\"/>"
%~dp0replace.exe "%RELPATH%\wwwroot\web.config" "Server=[^\x3B\x22]*" "Server=%DBSERVER%"
%~dp0replace.exe "%RELPATH%\wwwroot\web.config" "User ID=[^\x3B\x22]*" "User ID=%DBUSER%"
%~dp0replace.exe "%RELPATH%\wwwroot\web.config" "Password=[^\x3B\x22]*" "Password=%DBPWD%"
%~dp0replace.exe "%RELPATH%\wwwroot\web.config" "Initial Catalog=[^\x3B\x22]*" "Initial Catalog=%DBNAME%"
%~dp0replace.exe "%RELPATH%\wwwroot\web.config" "<compilation[^\x3C\x3E]*>" ""

cd /d "%RELPATH%
"%ZIP%" a "%RELPATH%\wwwroot\%VERSION%\Client.zip" Client

cd /d "%RELPATH%\Client

"%ZIP%" a %RELPATH%\wwwroot\Update\UPDATE-%VERSION%.zip data -r
"%ZIP%" a %RELPATH%\wwwroot\Update\UPDATE-%VERSION%.zip Client.exe
"%ZIP%" a %RELPATH%\wwwroot\Update\UPDATE-%VERSION%.zip Client.pdb
"%ZIP%" a %RELPATH%\wwwroot\Update\UPDATE-%VERSION%.zip Common.dll
"%ZIP%" a %RELPATH%\wwwroot\Update\UPDATE-%VERSION%.zip Interop.IWshRuntimeLibrary.dll
"%ZIP%" a %RELPATH%\wwwroot\Update\UPDATE-%VERSION%.zip MsHtmHstInterop.dll
"%ZIP%" a %RELPATH%\wwwroot\Update\UPDATE-%VERSION%.zip Setting.conf
"%ZIP%" a %RELPATH%\wwwroot\Update\UPDATE-%VERSION%.zip System.Net.Json.dll

if "%APPPATH:~-1%" == "/" (%~dp0replace.exe "%RELPATH%\wwwroot\Update\latest.xml" "URL=\"UPDATE.zip\"" "URL=\"%SERVICEURL%/Update/UPDATE-%VERSION%.zip\"") ^
else (%~dp0replace.exe "%RELPATH%\wwwroot\Update\latest.xml" "URL=\"UPDATE.zip\"" "URL=\"%SERVICEURL%%APPPATH%/Update/UPDATE-%VERSION%.zip\"")
%~dp0replace.exe "%RELPATH%\wwwroot\Update\latest.xml" "Latest=\"0.0.0.0\"" "Latest=\"%VERSION%\""

if exist %TMPPATH% rd /S /Q %TMPPATH%

if "%2"=="install" (
	if exist "%INSPATH%" rd /S /Q "%INSPATH%"
	if exist "%INSPATH%" (
	  echo 请删除Install文件夹
	  goto end
	)
	md %INSPATH%
	xcopy "%RELPATH%\wwwroot" "%INSPATH%\wwwroot\" /E /Y /Q
	copy "%IMPATH%\..\Db\Db.sql" "%INSPATH%\wwwroot\App_Data\"
	echo 请将wwwroot中的文件上传到根目录，打开install.aspx页面安装 > "%INSPATH%\安装说明.txt"
)

cd %~dp0

:end