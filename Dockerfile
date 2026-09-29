FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src
COPY ["KtMobileTestManager.csproj", "./"]
RUN dotnet restore "KtMobileTestManager.csproj"
COPY . .
RUN dotnet publish "KtMobileTestManager.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS final
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "KtMobileTestManager.dll"]
