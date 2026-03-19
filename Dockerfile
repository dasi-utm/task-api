FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 3001

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["TaskManager.slnx", "."]
COPY ["src/TaskManager.API/TaskManager.API.csproj", "src/TaskManager.API/"]
COPY ["src/TaskManager.Application/TaskManager.Application.csproj", "src/TaskManager.Application/"]
COPY ["src/TaskManager.Domain/TaskManager.Domain.csproj", "src/TaskManager.Domain/"]
COPY ["src/TaskManager.Infrastructure/TaskManager.Infrastructure.csproj", "src/TaskManager.Infrastructure/"]
RUN dotnet restore "src/TaskManager.API/TaskManager.API.csproj"
COPY . .
WORKDIR "/src/src/TaskManager.API"
RUN dotnet build "TaskManager.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManager.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:3001
ENTRYPOINT ["dotnet", "TaskManager.API.dll"]
