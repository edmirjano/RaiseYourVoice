FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["RaiseYourVoice.Api/RaiseYourVoice.Api.csproj", "RaiseYourVoice.Api/"]
COPY ["RaiseYourVoice.Application/RaiseYourVoice.Application.csproj", "RaiseYourVoice.Application/"]
COPY ["RaiseYourVoice.Domain/RaiseYourVoice.Domain.csproj", "RaiseYourVoice.Domain/"]
COPY ["RaiseYourVoice.Infrastructure/RaiseYourVoice.Infrastructure.csproj", "RaiseYourVoice.Infrastructure/"]
RUN dotnet restore "RaiseYourVoice.Api/RaiseYourVoice.Api.csproj"
COPY . .
WORKDIR "/src/RaiseYourVoice.Api"
RUN dotnet build "RaiseYourVoice.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RaiseYourVoice.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RaiseYourVoice.Api.dll"]