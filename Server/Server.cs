using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using Server;

using Utility;

var clientsThreads = new List<(Thread Thread, TcpClient Client)>();

try
{
    using var listener = new TcpListener(IPAddress.Any, Data.Port);
    listener.Start();

    Console.WriteLine("Server stared. Listening for connections...");

    var clientId = 1;
    while (true)
    {
        var client = await listener.AcceptTcpClientAsync();
        WriteClientMessage(clientId, "Accepted.");

        var currentClientId = clientId;
        var clientThread = new Thread(() => ProcessClient(client, currentClientId));
        clientsThreads.Add((clientThread, client));

        clientThread.Start();
        ++clientId;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"A fatal error occurred: {ex.Message}");
}
finally
{
    clientsThreads.ForEach(t =>
    {
        t.Thread.Join();
        t.Client.Close();
    });
}

return;

static void ProcessClient(TcpClient client, int clientId)
{
    try
    {
        using var stream = client.GetStream();

        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream);

        ResponseObject responseObject = new();

        var json = reader.ReadLine();
        if (json == null)
        {
            WriteErrorAndSend(writer, responseObject, "Request cannot be read.", clientId);
            return;
        }

        var requestObject = JsonSerializer.Deserialize<MultiplicationData>(json);
        if (requestObject == null)
        {
            WriteErrorAndSend(writer, responseObject, "Request was in incorrect format.", clientId);
            return;
        }

        WriteClientMessage(clientId, "Received two matrices.");

        WriteClientMessage(clientId, "Computing started.");
        var multiplier = new MatrixMultiplier();
        responseObject.Result = multiplier.Multiply(requestObject);
        WriteClientMessage(clientId, "Product computed.");

        SerializeResponseObjectAndSend(writer, responseObject, clientId);
    }
    catch (Exception ex)
    {
        WriteClientMessage(clientId, $"An error occurred while processing client request: {ex.Message}");
    }
}

static void WriteErrorAndSend(TextWriter writer, ResponseObject responseObject, string error, int clientId)
{
    responseObject.Error = error;
    WriteClientMessage(clientId, error);
    SerializeResponseObjectAndSend(writer, responseObject, clientId);
}

static void SerializeResponseObjectAndSend(TextWriter writer, ResponseObject responseObject, int clientId)
{
    var json = JsonSerializer.Serialize(responseObject);
    writer.WriteLine(json);
    writer.Flush();
    WriteClientMessage(clientId, "Response sent.");
}

static void WriteClientMessage(int clientId, string message)
{
    Console.WriteLine($"Client {clientId}: {message}");
}
