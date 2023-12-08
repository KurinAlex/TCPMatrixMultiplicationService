using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using Client;

using Utility;

try
{
    const int minMatrixSize = 1000;
    const int maxMatrixSize = 2000;

    const double numbersMinValue = -1000;
    const double numbersMaxValue = 1000;

    var n = Generator.GenerateInt32(minMatrixSize, maxMatrixSize);
    var m = Generator.GenerateInt32(minMatrixSize, maxMatrixSize);
    var l = Generator.GenerateInt32(minMatrixSize, maxMatrixSize);

    var m1 = Generator.GenerateMatrix(n, m, numbersMinValue, numbersMaxValue);
    var m2 = Generator.GenerateMatrix(m, l, numbersMinValue, numbersMaxValue);

    var transferObject = new MultiplicationData(n, m, l, m1, m2);

    var json = JsonSerializer.Serialize(transferObject);

    using var client = new TcpClient();

    var remoteIp = new IPEndPoint(Data.IpAddress, Data.Port);

    Console.WriteLine($"Connecting to {remoteIp}...");
    await client.ConnectAsync(remoteIp);
    Console.WriteLine($"Connected to {remoteIp}.");

    await using var stream = client.GetStream();
    await using var writer = new StreamWriter(stream);
    using var reader = new StreamReader(stream);

    Console.WriteLine("Sending data to server...");
    await writer.WriteLineAsync(json);
    await writer.FlushAsync();
    Console.WriteLine($"Sent two matrices of sizes {n}*{m} and {m}*{l} to server.");

    Console.WriteLine("Waiting for response from server...");
    var responseJson = await reader.ReadLineAsync();
    Console.WriteLine("Received response from server.");

    if (responseJson == null)
    {
        Console.WriteLine("Error occurred while reading response.");
        return;
    }

    var responseObject = JsonSerializer.Deserialize<ResponseObject>(responseJson);

    if (responseObject == null)
    {
        Console.WriteLine("Response was in incorrect format.");
    }
    else if (responseObject.Error != null)
    {
        Console.WriteLine($"Received error message from server: {responseObject.Error}");
    }
    else
    {
        var matrixJson = JsonSerializer.Serialize(
            responseObject.Result,
            JsonSerializationData.IndentedSerializationOptions);

        var resultFileName = $"{Guid.NewGuid()}.txt";

        Console.WriteLine($"Saving result to '{resultFileName}'...");
        File.WriteAllText(resultFileName, matrixJson);
        Console.WriteLine($"Result saved to '{resultFileName}'.");

        Console.Write("Press any key to write response to console...");
        Console.Read();

        Console.Write(matrixJson);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"A fatal error occurred: {ex.Message}");
}
finally
{
    Console.Write("Press any key to close this window...");
    Console.Read();
}
