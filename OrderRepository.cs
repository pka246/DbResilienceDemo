using Dapper;
using Microsoft.Data.SqlClient;
using Polly;
using Polly.Registry;

public class OrderRepository(
      IConfiguration config,
      ChaosSwitch chaos,
      ResiliencePipelineProvider<string> pipelineProvider)
{
    private readonly ResiliencePipeline _pipeline =
        pipelineProvider.GetPipeline("db-retry");

    public async Task<IEnumerable<Order>> GetOrdersAsync(CancellationToken ct) =>
        await _pipeline.ExecuteAsync(async token =>
        {
            chaos.MaybeThrowDbError();
            await using var conn = new SqlConnection(config.GetConnectionString("ShopDb"));
            return await conn.QueryAsync<Order>(
                new CommandDefinition("SELECT Id, Customer, Total FROM Orders",
                    cancellationToken: token));
        }, ct);
}
