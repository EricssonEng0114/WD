@echo off
setlocal enabledelayedexpansion

:: Get today's date
for /f "tokens=2 delims==" %%a in ('wmic OS Get localdatetime /value') do set "dt=%%a"
set /a "yy=%dt:~0,4%", "mm=1%dt:~4,2%", "dd=1%dt:~6,2%"

:: Subtract a day
set /a dd-=1
if !dd! lss 100 (
    set /a mm-=1
    if !mm! lss 100 (
        set /a yy-=1, mm=112
    )
    for /f "tokens=!mm! delims=," %%a in ("131,128,131,130,131,130,131,131,130,131,130,131") do set /a dd=100+%%a
    if !mm!==102 (
        set /a leap=yy %% 4
        if !leap!==0 (set /a dd+=1)
    )
)

:: Format the date
set "yesterday=!yy!!mm:~1!!dd:~1!"
set "folder=!yy!!mm:~1!"


:: Take ownership of the folder
takeown /f "C:\UploadPath\ZIP_!folder!\!yesterday!" /r /d y

:: Grant full permissions to the current user
icacls "C:\UploadPath\ZIP_!folder!\!yesterday!" /grant %username%:F /t

:: Delete the folder forcefully
rd /s /q "C:\UploadPath\ZIP_!folder!\!yesterday!"

endlocal
