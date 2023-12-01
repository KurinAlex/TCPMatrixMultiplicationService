using System.Text.Json;

namespace Client;

internal class JsonSerializationData
{
    public static readonly JsonSerializerOptions IndentedSerializationOptions = new() { WriteIndented = true };
}
