using System.Text.Json;

namespace Client;

internal static class JsonSerializationData
{
    public static readonly JsonSerializerOptions IndentedSerializationOptions = new() { WriteIndented = true };
}
