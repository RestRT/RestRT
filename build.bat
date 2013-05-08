%windir%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe RestRT.sln /t:Clean,Rebuild /p:Configuration=Release /fileLogger

if not exist Download\package\lib\windows8 mkdir Download\package\lib\windows8\

copy readme.txt Download\package\

copy RestRT\bin\Release\RestRT.winmd Download\Package\lib\windows8\

.nuget\nuget.exe update -self
.nuget\nuget.exe pack restrt.nuspec -BasePath Download\Package -Output Download