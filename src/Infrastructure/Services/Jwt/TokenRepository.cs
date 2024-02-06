using Beseler.Infrastructure.Data;
using Dapper;

namespace Beseler.Infrastructure.Services.Jwt;

public sealed class TokenRepository(IDatabaseConnector connector)
{
    public async Task<TokenLog?> GetByIdAsync(Guid tokenId, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("TokenId", tokenId);

        using var connection = await connector.ConnectAsync(stoppingToken);
        return await connection.QuerySingleOrDefaultAsync<TokenLog>("""
            SELECT *
            FROM dbo.TokenLog
            WHERE TokenId = @TokenId;
            """, parameters);
    }

    public async Task SaveAsync(TokenLog log, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters(log);
        using var connection = await connector.ConnectAsync(stoppingToken);
        _ = log.TokenLogId == 0 ?
            await connection.ExecuteAsync(InsertQuery, parameters) :
            await connection.ExecuteAsync(UpdateQuery, parameters);
    }

    public async Task RevokeChainAsync(Guid tokenId, CancellationToken stoppingToken = default)
    {
        var parameters = new DynamicParameters();
        parameters.Add("TokenId", tokenId);
        using var connection = await connector.ConnectAsync(stoppingToken);
        await connection.ExecuteAsync("""
            SET NOCOUNT ON;
            SET TRANSACTION ISOLATION LEVEL READ COMMITTED;

            DECLARE @Tokens TABLE (TokenLogId INT NOT NULL);

            WITH TokenDescendants AS (
            	SELECT
            		TokenLogId,
            		TokenId,
            		ReplacedByTokenId
            	FROM dbo.TokenLog WHERE TokenId = @TokenId

            	UNION ALL

            	SELECT
            		tl.TokenLogId,
            		tl.TokenId,
            		tl.ReplacedByTokenId
            	FROM dbo.TokenLog tl
            	INNER JOIN TokenDescendants td
            		ON td.ReplacedByTokenId = tl.TokenId
            	WHERE IsRevoked = 0
            		AND ExpiresOn > GETDATE()
            ),
            TokenAncestors AS (
            	SELECT
            		TokenLogId,
            		TokenId,
            		ReplacedByTokenId
            	FROM dbo.TokenLog WHERE ReplacedByTokenId = @TokenId

            	UNION ALL

            	SELECT
            		tl.TokenLogId,
            		tl.TokenId,
            		tl.ReplacedByTokenId
            	FROM dbo.TokenLog tl
            	INNER JOIN TokenAncestors ta
            		ON ta.TokenId = tl.ReplacedByTokenId
            	WHERE IsRevoked = 0
            		AND ExpiresOn > GETDATE()
            )
            INSERT INTO @Tokens (TokenLogId)
            SELECT ta.TokenLogId FROM TokenAncestors ta
            UNION ALL
            SELECT td.TokenLogId FROM TokenDescendants td
            OPTION (MAXRECURSION 10000);

            UPDATE tl
            SET tl.IsRevoked = 1
            FROM dbo.TokenLog tl
            WHERE EXISTS (SELECT 1 FROM @Tokens t WHERE t.TokenLogId = tl.TokenLogId);
            """, parameters);
    }

    private const string InsertQuery = """
        INSERT INTO dbo.TokenLog
        (
            TokenId,            
            CreatedOn,
            ExpiresOn,
            IsRevoked,
            AccountId
        )
        VALUES
        (
            @TokenId,
            @CreatedOn,
            @ExpiresOn,
            @IsRevoked,
            @AccountId
        );
        """;

    private const string UpdateQuery = """
        UPDATE dbo.TokenLog
        SET ReplacedByTokenId = @ReplacedByTokenId,
            @IsRevoked = IsRevoked
        WHERE TokenLogId = @TokenLogId;
        """;
}
