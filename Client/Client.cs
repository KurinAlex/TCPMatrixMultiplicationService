using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using Client;

using Utility;

try
{
    const int minMatrixSize = 1000;
    const int maxMatrixSize = 2000;

    var n = Generator.GenerateInt32(minMatrixSize, maxMatrixSize);
    var m = Generator.GenerateInt32(minMatrixSize, maxMatrixSize);
    var l = Generator.GenerateInt32(minMatrixSize, maxMatrixSize);

    var m1 = Generator.GenerateMatrix(n, m);
    var m2 = Generator.GenerateMatrix(m, l);

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

    await writer.WriteLineAsync(json);
    await writer.FlushAsync();
    Console.WriteLine($"Sent two matrices of sizes {n}*{m} and {m}*{l} to server.");

    Console.WriteLine("Waiting for response from server...");
    var responseJson = await reader.ReadLineAsync();
    Console.WriteLine("Received response from server.");

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
        Console.WriteLine($"Received error message from server: {responseObject.Error}");
    }
    else
    {
        var matrixJson = JsonSerializer.Serialize(
            responseObject.Result,
            JsonSerializationData.IndentedSerializationOptions);

        Console.WriteLine("Saving result to 'result.txt'...");
        File.WriteAllText("result.txt", matrixJson);
        Console.WriteLine("Result saved to 'result.txt'.");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"A fatal exception occurred: {ex.Message}");
}
finally
{
    Console.Write("Press any key to close this window...");
    Console.Read();
}
