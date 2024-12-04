# 사용 중인 .NET SDK 버전 확인
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# 프로젝트 파일 복사
COPY . .
RUN dotnet restore "NetworkPlayServer/NetworkPlayServer.csproj"

# 전체 소스 코드 복사
COPY . .
RUN dotnet build "NetworkPlayServer/NetworkPlayServer.csproj" -c Release -o /app/build

# 게시
RUN dotnet publish "NetworkPlayServer/NetworkPlayServer.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 런타임 이미지
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NetworkPlayServer.dll"]