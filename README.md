# DbResilienceDemo (Video 2: Self-Healing Database Calls in .NET 10)

Web API + SQL Server (Docker) + Dapper. A chaos switch makes ~50% of database calls time out,
so you can show a named pipeline from the **Microsoft.Extensions.Resilience** package fixing it.

## Requirements
.NET 10 SDK, Docker, VS Code (C# Dev Kit and REST Client extensions).

## 1. Start SQL Server and seed it
```bash
docker run -e ACCEPT_EULA=Y -e MSSQL_SA_PASSWORD='Your_password123' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
docker ps -q                       # copy the container id
docker cp seed.sql <container-id>:/seed.sql
docker exec <container-id> /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P Your_password123 -C -i /seed.sql
```
Wait about 15 seconds after the container starts before running the last command.

## 2. Run
```bash
dotnet restore
dotnet run        # listens on http://localhost:5081
```

## Packages (from scratch)
```bash
dotnet new web -n DbResilienceDemo
dotnet add package Microsoft.Extensions.Resilience
dotnet add package Dapper
dotnet add package Microsoft.Data.SqlClient
```

## Files
| File | Purpose |
|---|---|
| `Program.cs` | STARTING state (no resilience). Run it first to show ~50% failures. |
| `Program.final.cs.txt` | FINAL state with the `db-retry` pipeline. Your cheat sheet. |
| `chaos.http` | One-click requests (REST Client). |
| `loop.ps1` / `loop.sh` | 20 requests, prints status codes. |
| `seed.sql` | Creates ShopDb and the Orders table. |

## Tutorial flow
1. Starting `Program.cs`: send `GET /api/orders` from `chaos.http` many times. About half fail.
2. Add the usings and `AddResiliencePipeline("db-retry", ...)`, then change `OrderRepository` to use `ResiliencePipelineProvider<string>`.
3. Send the requests again. Almost all succeed, and the console prints `[db-retry] Attempt 1 failed...`.
4. Send `POST /chaos/db/100`: when retries run out the error still surfaces. Reset with `POST /chaos/db/50`.

## Notes
- Do not use `dotnet watch`; service registration changes need a clean restart.
- Pin exact package versions in the `.csproj` before publishing.
- Retry only idempotent operations, and do not stack EF Core's EnableRetryOnFailure on top of this pipeline.
