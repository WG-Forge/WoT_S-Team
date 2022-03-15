# escape=`

FROM mcr.microsoft.com/dotnet/framework/sdk:4.8

SHELL ["cmd", "/S", "/C"]

ADD https://aka.ms/vs/17/release/vs_buildtools.exe C:\vs_buildtools.exe

RUN C:\vs_buildtools.exe `
--quiet --wait --norestart --nocache modify `
--installPath "%ProgramFiles(x86)%\Microsoft Visual Studio\2022\BuildTools" `
--add Microsoft.VisualStudio.Workload.ManagedDesktopBuildTools;includeRecommended `
--add Microsoft.VisualStudio.Workload.MSBuildTools;includeRecommended `
--add Microsoft.VisualStudio.Workload.VCTools;includeRecommended `
|| IF "%ERRORLEVEL%"=="3010" EXIT 0

RUN del C:\vs_buildtools.exe

COPY . .

RUN nuget restore

RUN MSBuild GameClient.sln /t:Rebuild /p:Configuration=Release /p:Platform=x64

WORKDIR /Product/Client

ENTRYPOINT ["C:\\Product\\Client\\WOTS.exe", "&&", "powershell.exe", "-NoLogo", "-ExecutionPolicy", "Bypass"]