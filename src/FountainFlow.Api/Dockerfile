FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
WORKDIR /app
EXPOSE 80

# copy all .csproj files and restore as distinct layers.   Use of the same COPY command
# for every dockerfile in the project to take advantage of docker caching
COPY FountainFlow.sln FountainFlow.sln
COPY src/FountainFlow.Api/FountainFlow.Api.csproj src/FountainFlow.Api/FountainFlow.Api.csproj
COPY src/FountainFlow.Service/FountainFlow.Service.csproj src/FountainFlow.Service/FountainFlow.Service.csproj
COPY src/FountainFlow.UI/FountainFlowUI.csproj src/FountainFlow.UI/FountainFlowUI.csproj

# Restore package deps
RUN dotnet restore FountainFlow.sln

# Copy the app folders over
COPY src/FountainFlow.Api src/FountainFlow.Api
WORKDIR /app/src/FountainFlow.Api
RUN dotnet publish -c Release -o /app/src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/src/out .
ENTRYPOINT [ "dotnet", "FountainFlow.Api.dll" ]