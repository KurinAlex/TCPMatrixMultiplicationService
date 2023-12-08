using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;

using Server;

using Utility;

var clientsThreads = new List<(Thread Thread, TcpClient Client, int ClientId)>();
var stopper = new Stopper();

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
        var clientThread = new Thread(() => ProcessClient(client, stopper, currentClientId));
        clientsThreads.Add((clientThread, client, currentClientId));

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
    stopper.Stop = true;

    clientsThreads.ForEach(t =>
    {
        WriteClientMessage(t.ClientId, "Stopping thread...");
        t.Thread.Join();
        t.Client.Close();
        WriteClientMessage(t.ClientId, "Closed.");
    });
}

return;

static void ProcessClient(TcpClient client, Stopper stopper, int clientId)
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

        WriteClientMessage(clientId, "Data received.");

        var timer = Stopwatch.StartNew();

        WriteClientMessage(clientId, "Computing started.");
        var multiplier = new MatrixMultiplier();
        responseObject.Result = multiplier.Multiply(requestObject, stopper);

        timer.Stop();

        WriteClientMessage(clientId, $"Product computed in {timer.Elapsed.TotalSeconds} s.");

        SerializeResponseObjectAndSend(writer, responseObject, clientId);
    }
    catch (Exception ex)
    {
        WriteClientMessage(clientId, $"An error occurred: {ex.Message}");
    }
    finally
    {
        client.Close();
        WriteClientMessage(clientId, "Closed.");
    }
}

static void WriteClientMessage(int clientId, string message)
{
    Console.WriteLine($"Client {clientId}: {message}");
}

static void WriteErrorAndSend(TextWriter writer, ResponseObject responseObject, string error, int clientId)
{
    WriteClientMessage(clientId, error);

    responseObject.Error = error;
    SerializeResponseObjectAndSend(writer, responseObject, clientId);
}

static void SerializeResponseObjectAndSend(TextWriter writer, ResponseObject responseObject, int clientId)
{
    var json = JsonSerializer.Serialize(responseObject);
    writer.WriteLine(json);
    writer.Flush();

    WriteClientMessage(clientId, "Response sent.");
}
