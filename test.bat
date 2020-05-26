call dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

call cover-report.bat
