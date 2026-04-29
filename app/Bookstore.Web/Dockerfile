FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["app/Bookstore.Cdk/Bookstore.Cdk.csproj", "app/Bookstore.Cdk/"]
COPY ["app/Bookstore.Data/Bookstore.Data.csproj", "app/Bookstore.Data/"]
COPY ["app/Bookstore.Domain/Bookstore.Domain.csproj", "app/Bookstore.Domain/"]
COPY ["app/Bookstore.Web/Bookstore.Web.csproj", "app/Bookstore.Web/"]
COPY ["app/Bookstore.Common/Bookstore.Common.csproj", "app/Bookstore.Common/"]
RUN dotnet restore "app/Bookstore.Web/Bookstore.Web.csproj" -a amd64
COPY . .
WORKDIR "/src/app/Bookstore.Web"
RUN dotnet build "Bookstore.Web.csproj" -c Release -o /app/build -a amd64

FROM build AS publish
RUN apt-get update -yq \
    && apt-get install -yq ca-certificates curl gnupg \
    && mkdir -p /etc/apt/keyrings \
    && curl -fsSL https://deb.nodesource.com/gpgkey/nodesource-repo.gpg.key | gpg --dearmor -o /etc/apt/keyrings/nodesource.gpg \
    && echo "deb [signed-by=/etc/apt/keyrings/nodesource.gpg] https://deb.nodesource.com/node_18.x nodistro main" | tee /etc/apt/sources.list.d/nodesource.list \
    && apt-get update -yq \
    && apt-get install nodejs -yq
RUN dotnet publish "Bookstore.Web.csproj" -c Release -o /app/publish -a amd64

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Bookstore.Web.dll"]
