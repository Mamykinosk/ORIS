using GameAndDot.Shared.Models;
using System.Net;
using System.Net.Sockets;

namespace GameAndDot.Shared
{
    public class ServerObject
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Any, 8888);
        public List<ClientObject> Clients { get; private set; } = new();
        
        // We keep the state of all players here
        public List<PlayerInfo> ActivePlayers { get; private set; } = new();

        public void RemoveConnection(string id)
        {
            ClientObject? client = Clients.FirstOrDefault(c => c.Id == id);
            if (client != null) Clients.Remove(client);
            client?.Close();
        }

        public async Task ListenAsync()
        {
            try
            {
                tcpListener.Start();
                Console.WriteLine("Server running. Waiting for connections...");

                while (true)
                {
                    TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Clients.Add(clientObject);
                    Task.Run(clientObject.ProcessAsync);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        public async Task BroadcastMessageAllAsync(string message)
        {
            foreach (var client in Clients)
            {
                await client.Writer.WriteLineAsync(message);
                await client.Writer.FlushAsync();
            }
        }

        protected internal void Disconnect()
        {
            foreach (var client in Clients)
            {
                client.Close();
            }
            tcpListener.Stop();
        }
    }
}