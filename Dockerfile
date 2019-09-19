FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["BillingAPI/BillingAPI.csproj", "BillingAPI/"]
COPY ["Jobs/Jobs.csproj", "Jobs/"]
RUN dotnet restore "BillingAPI/BillingAPI.csproj"


COPY . .
WORKDIR /src
RUN dotnet build "BillingAPI/BillingAPI.csproj" -c Release -o /app
RUN dotnet build "Jobs/Jobs.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "BillingAPI/BillingAPI.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "BillingAPI.dll"]