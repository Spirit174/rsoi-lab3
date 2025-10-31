FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8050

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Booking.System/Booking.System.LoyaltyService/Booking.System.LoyaltyService.csproj", "Booking.System/Booking.System.LoyaltyService/"]
RUN dotnet restore "./Booking.System/Booking.System.LoyaltyService/Booking.System.LoyaltyService.csproj"
COPY . .
WORKDIR "/src/Booking.System/Booking.System.LoyaltyService"
RUN dotnet build "Booking.System.LoyaltyService.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Booking.System.LoyaltyService.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Booking.System.LoyaltyService.dll"]