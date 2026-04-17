@echo off
setlocal enabledelayedexpansion

:: List of directories to process
set "directories=C:\InternalRptStoragePath\CIMB C:\InternalRptStoragePath\CTB C:\InternalRptStoragePath\OCBC C:\InternalRptStoragePath\RHB"

:: Get the current date
for /f "tokens=2 delims==" %%I in ('wmic os get localdatetime /format:list') do set "datetime=%%I"
set "Year=%datetime:~0,4%"
set "Month=%datetime:~4,2%"
set "Day=%datetime:~6,2%"

:: Subtract 7 years
set /a "Year-=7"

:: Loop through each directory
for %%d in (%directories%) do (
    :: Set the folder path for 1 year ago
    set "folderPath=%%d\!Year!\!Month!\!Year!!Month!!Day!"

    :: Check if folder exists and delete
    if exist "!folderPath!" (
        rd /s /q "!folderPath!"
    )

    :: Check if parent month folder exists and is empty, then delete
    set "folderPath=%%d\!Year!\!Month!"
    dir /b /a "!folderPath!" | findstr "^" >nul || (      
        rd /s /q "!folderPath!"
    )

    :: Check if parent year folder exists and is empty, then delete
    set "folderPath=%%d\!Year!"
    if exist "!folderPath!" (       
        set "emptyYear=true"
        for /f %%a in ('dir /b /a "!folderPath!"') do (
            set "subfolder=%%d\!Year!\%%a"
            dir /b /a "!subfolder!" | findstr "^" >nul && (
                set "emptyYear=false"                
            )
        )
        if "!emptyYear!"=="true" (            
            rd /s /q "!folderPath!"
        )
    )

:: Now let's check and delete older date folders within the same month
for /l %%d in (1,1,31) do (
    set "CurrDay=%%d"
    if %%d lss 10 (
        set "CurrDay=0%%d"
    )

  if %%d lss %Day% (
    set "folderPath=!Year!\!Month!\!Year!!Month!!CurrDay!"

    for %%f in (%directories%) do (
        set "fullFolderPath=%%f\!folderPath!"

        if exist "!fullFolderPath!" (           
            rd /s /q "!fullFolderPath!"
        )
    )
 )
)



    :: Delete old month folders (excluding the current month)
    for /l %%m in (!Month!,-1,1) do (
        set "CurrMonth=%%m"
        if %%m lss 10 (
            set "CurrMonth=0%%m"
        )
        if %%m neq %Month% (
            set "folderPath=%%d\!Year!\!CurrMonth!"
            if exist "!folderPath!" (                  
                rd /s /q "!folderPath!"
            )
        )
    )

    :: Check if !Year! folder exists and delete if empty
    set "folderPath=%%d\!Year!"
    if exist "!folderPath!" (
        dir /b /a "!folderPath!" | findstr "^" >nul || (            
            rd /s /q "!folderPath!"
        )
    )
)

endlocal
