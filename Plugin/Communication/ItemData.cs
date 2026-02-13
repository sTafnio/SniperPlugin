using System.Text.Json;
using SniperPlugin.Utils;

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
public record PriceData(double Amount, string Currency);

public static class ItemData
{
    public static Result<ItemDataRecord> CreateFromJson(string json, JsonSerializerOptions? options = null)
    {
        try
        {
            var item = JsonSerializer.Deserialize<ItemDataRecord>(json, options);
            return item is null ? Result<ItemDataRecord>.Failure("Received empty or malformed JSON") : Validate(item);
        }
        catch (JsonException e)
        {
            return Result<ItemDataRecord>.Failure($"JSON Parse Error: {e.Message}");
        }
    }

    private static Result<ItemDataRecord> Validate(ItemDataRecord item)
    {
        if (string.IsNullOrWhiteSpace(item.Id)) return Result<ItemDataRecord>.Failure("Item ID is missing");
        if (string.IsNullOrWhiteSpace(item.Name)) return Result<ItemDataRecord>.Failure("Item Name is missing");
        if (string.IsNullOrWhiteSpace(item.Token)) return Result<ItemDataRecord>.Failure("Hideout Token is missing");

        if (item.Position.X < 0 || item.Position.X > 23 || item.Position.Y < 0 || item.Position.Y > 23)
            return Result<ItemDataRecord>.Failure($"Position ({item.Position.X}, {item.Position.Y}) is out of bounds");

        if (item.Size.W <= 0 || item.Size.H <= 0 || item.Size.W > 4 || item.Size.H > 4)
            return Result<ItemDataRecord>.Failure($"Invalid Item Size {item.Size.W}x{item.Size.H}");

        if (item.Price.Amount < 0) return Result<ItemDataRecord>.Failure($"Price cannot be negative: {item.Price.Amount}");
        if (string.IsNullOrWhiteSpace(item.Price.Currency)) return Result<ItemDataRecord>.Failure("Currency type is missing");

        return Result<ItemDataRecord>.Success(item);
    }
}
