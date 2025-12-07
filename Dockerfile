FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "SmartGearOnline.csproj"
RUN dotnet publish "SmartGearOnline.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://*:${PORT:-8080}
ENTRYPOINT ["dotnet", "SmartGearOnline.dll"]