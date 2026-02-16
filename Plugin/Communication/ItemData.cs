using System;
using System.Text.Json;
using ExileCore;

namespace SniperPlugin.Communication;

public record ItemDataRecord(
    string Id,
    string Name,
    PositionData Position,
    SizeData Size,
    string Token,
    int Fee,
    PriceData Price
);

public record PositionData(int X, int Y);
public record SizeData(int W, int H);
public record PriceData(int Amount, string Currency);

public static class ItemData
{
    public static ItemDataRecord? Create(string json, JsonSerializerOptions? options = null)
    {
        try
        {
            var item = JsonSerializer.Deserialize<ItemDataRecord>(json, options);
            if (item is
                {
                    Id: not null,
                    Name: not null,
                    Token: not null,
                    Position: { X: >= 0 and <= 11, Y: >= 0 and <= 11 },
                    Size: { W: > 0 and <= 4, H: > 0 and <= 4 },
                    Price: { Amount: >= 0, Currency: not null }
                })
                return item;

            DebugWindow.LogError("[Sniper] Received malformed or incomplete ItemData");
            return null;
        }
        catch (Exception ex)
        {
            DebugWindow.LogError($"[Sniper] ItemData Parse Error: {ex.Message}");
            return null;
        }
    }
}
