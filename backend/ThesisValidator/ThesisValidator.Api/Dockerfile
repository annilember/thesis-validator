FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER root
RUN apt-get update && apt-get install -y \
    fontconfig \
    libfreetype6 \
    fonts-liberation \
    fonts-dejavu \
    && rm -rf /var/lib/apt/lists/*
USER $APP_UID
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ThesisValidator.Api/ThesisValidator.Api.csproj", "ThesisValidator.Api/"]
COPY ["ThesisValidator.BLL/ThesisValidator.BLL.csproj", "ThesisValidator.BLL/"]
COPY ["ThesisValidator.BLL.Docx/ThesisValidator.BLL.Docx.csproj", "ThesisValidator.BLL.Docx/"]
COPY ["ThesisValidator.DAL/ThesisValidator.DAL.csproj", "ThesisValidator.DAL/"]
COPY ["ThesisValidator.DAL.Docx/ThesisValidator.DAL.Docx.csproj", "ThesisValidator.DAL.Docx/"]
COPY ["ThesisValidator.DAL.JSON/ThesisValidator.DAL.JSON.csproj", "ThesisValidator.DAL.JSON/"]
COPY ["ThesisValidator.Domain/ThesisValidator.Domain.csproj", "ThesisValidator.Domain/"]
RUN dotnet restore "ThesisValidator.Api/ThesisValidator.Api.csproj"
COPY . .
WORKDIR "/src/ThesisValidator.Api"
RUN dotnet build "./ThesisValidator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ThesisValidator.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ThesisValidator.Api.dll"]
