FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["app/Bookstore.Cdk/Bookstore.Cdk.csproj", "app/Bookstore.Cdk/"]
COPY ["app/Bookstore.Common/Bookstore.Common.csproj", "app/Bookstore.Common/"]
COPY ["app/Bookstore.Data/Bookstore.Data.csproj", "app/Bookstore.Data/"]
COPY ["app/Bookstore.Domain/Bookstore.Domain.csproj", "app/Bookstore.Domain/"]
COPY ["app/Bookstore.Web/Bookstore.Web.csproj", "app/Bookstore.Web/"]
RUN dotnet restore "app/Bookstore.Web/Bookstore.Web.csproj"

COPY . .
WORKDIR "/src/app/Bookstore.Web"
RUN dotnet publish "Bookstore.Web.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "Bookstore.Web.dll"]
