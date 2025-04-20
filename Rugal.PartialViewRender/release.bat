
dotnet build
dotnet nuget push -s https://nuget.dtvl.com.tw -k %NUGET_API_KEY% bin/Debug/Rugal.PartialViewRender.1.0.1.nupkg

pause