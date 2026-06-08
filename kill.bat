@echo off
echo Stopping SCM3 app processes...
taskkill /F /IM SCM3.AppHost.exe >nul 2>&1
taskkill /F /IM SCM3.Api.exe >nul 2>&1
taskkill /F /IM SCM3.Web.exe >nul 2>&1
taskkill /F /IM dcp.exe >nul 2>&1
echo Done.
