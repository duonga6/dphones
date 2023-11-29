# Runtime stage 
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY app/publish/ .

RUN chmod -R +rwx /app/BackupDB

RUN rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

EXPOSE 8090

WORKDIR /app
ENTRYPOINT ["dotnet", "App.dll"]