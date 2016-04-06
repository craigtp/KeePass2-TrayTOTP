del TrayTOTP.plgx /F /A
del src.plgx /F /A
rd src\obj /S /Q
rd src\bin /S /Q
Keepass.exe --plgx-create "%~dp0src"
ren src.plgx TrayTOTP.plgx
