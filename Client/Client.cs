using System.Net.Sockets;
using System.Text.Json;

using Client;

using Utility;

try
{
    const int minMatrixSize = 1000;
    const int maxMatrixSize = 10000;

    var n = Generator.GenerateInt32(minMatrixSize, maxMatrixSize);
    var m = Generator.GenerateInt32(minMatrixSize, maxMatrixSize);
    var l = Generator.GenerateInt32(minMatrixSize, maxMatrixSize);

    var m1 = Generator.GenerateMatrix(n, m);
    var m2 = Generator.GenerateMatrix(m, l);

    var transferObject = new RequestObject(n, m, l, m1, m2);

    var json = JsonSerializer.Serialize(transferObject);

    using var client = new TcpClient();
    await client.ConnectAsync(Data.IpAddress, Data.Port);

    await using var stream = client.GetStream();

    await using var writer = new StreamWriter(stream);
    await writer.WriteLineAsync(json);
    await writer.FlushAsync();

    using var reader = new StreamReader(stream);
    var responseJson = await reader.ReadLineAsync();

    if (responseJson == null)
    {
        Console.WriteLine("Error while reading response.");
        return;
    }

    var responseObject = JsonSerializer.Deserialize<ResponseObject>(responseJson);

    if (responseObject == null)
    {
        Console.WriteLine("Response was in incorrect format.");
    }
    else if (responseObject.Error != null)
    {
        Console.WriteLine($"An error occurred while computing: {responseObject.Error}");
    }
    else
    {
        var matrixJson = JsonSerializer.Serialize(
            responseObject.Result,
            JsonSerializationData.IndentedSerializationOptions);

        Console.WriteLine("Result matrix is:");
        Console.WriteLine(matrixJson);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"An exception occurred: {ex.Message}");
}
