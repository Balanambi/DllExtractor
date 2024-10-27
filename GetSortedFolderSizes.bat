@echo off
setlocal

REM Set the target directory to D:\
set "targetDir=D:\"

REM Check if the directory exists
if not exist "%targetDir%" (
    echo Directory does not exist.
    exit /b
)

REM Create/clear output file
set "outputFile=folder_sizes.txt"
echo "Folder Name","Size (GB)" > "%outputFile%"

REM Get sizes of all directories and subdirectories
for /d %%D in ("%targetDir%\*") do (
    for /f "usebackq" %%S in (`powershell -command "(Get-ChildItem '%%D' -Recurse | Measure-Object -Property Length -Sum).Sum / 1GB"`) do (
        for /f "tokens=1" %%R in ('powershell -command "([math]::Round(%%S, 2))"') do (
            echo "%%~nxD",%%R >> "%outputFile%"
        )
    )
)

REM Sort the output file in descending order and create a new sorted file
powershell -Command "Import-Csv '%outputFile%' -Header 'Folder Name','Size (GB)' | Sort-Object 'Size (GB)' -Descending | Export-Csv -NoTypeInformation -Encoding UTF8 'sorted_%outputFile%'"

echo Folder sizes saved to sorted_%outputFile%.
endlocal
pause
