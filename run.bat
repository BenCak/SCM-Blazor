@echo off
echo Stopping SCM3.Web and SCM3.Api...
taskkill /F /IM SCM3.Web.exe >nul 2>&1
taskkill /F /IM SCM3.Api.exe >nul 2>&1

echo Restarting SCM3.AppHost...
taskkill /F /IM SCM3.AppHost.exe >nul 2>&1
taskkill /F /IM dcp.exe >nul 2>&1

start "" dotnet run --project SCM3.AppHost
echo Done.
