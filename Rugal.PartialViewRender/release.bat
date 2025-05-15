
dotnet build
dotnet nuget push -s https://nuget.dtvl.com.tw -k %NUGET_API_KEY% bin/Debug/Rugal.PartialViewRender.1.1.3.nupkg

pause