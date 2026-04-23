FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["app/Bookstore.Cdk/Bookstore.Cdk.csproj", "app/Bookstore.Cdk/"]
COPY ["app/Bookstore.Data/Bookstore.Data.csproj", "app/Bookstore.Data/"]
COPY ["app/Bookstore.Domain/Bookstore.Domain.csproj", "app/Bookstore.Domain/"]
COPY ["app/Bookstore.Web/Bookstore.Web.csproj", "app/Bookstore.Web/"]
COPY ["app/Bookstore.Common/Bookstore.Common.csproj", "app/Bookstore.Common/"]
RUN dotnet restore "app/Bookstore.Web/Bookstore.Web.csproj"
COPY . .
WORKDIR "/src/app/Bookstore.Web"
RUN dotnet build "Bookstore.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Bookstore.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bookstore.Web.dll"]
