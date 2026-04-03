@REM clean.bat: script to clean obj and bin folders from each project in the current solution

echo off

set FOLDERS_LIST=(KidNest.Core KidNest.Infrastructure KidNest.Services KidNest.Web)

for %%f in %FOLDERS_LIST% do (
    if exist .\%%f\bin (
        echo "Removing bin folder from `%%f`"
        rmdir /s /q .\%%f\bin
    )

    if exist .\%%f\obj (
        echo "Removing obj folder from `%%f`"
        rmdir /s /q .\%%f\obj
    ) 
)

echo. & echo Done!

set /p key="Press any key"
