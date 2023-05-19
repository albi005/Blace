[![thumbnail](thumbnail.webp)](https://blace.alb1.hu)

# Blace 

Reddit's r/place recreated using C#

## Running locally

1. Install the .NET 7 SDK: https://dot.net/download or use a package manager (e.g. `winget install dotnet-sdk-7` on Windows).
2. (Optional, but data won't be persisted when restarting the server) Set up CosmosDB emulator (C:\Program Files\Azure Cosmos DB Emulator\Microsoft.Azure.Cosmos.Emulator.exe should exist).
3. Open the Blace.Server folder in a terminal and execute `dotnet run`.
4. Open the Blace.Client folder in a terminal and execute `dotnet run`.
5. Open http://localhost:7150 in a browser.