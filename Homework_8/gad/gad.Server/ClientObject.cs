using System.Net.Sockets;
using System.Text.Json;
using GameAndDot.Shared.Enums;
using GameAndDot.Shared.Models;

namespace GameAndDot.Shared
{
    public class ClientObject
    {
        protected internal string Id { get; } = Guid.NewGuid().ToString();
        protected internal StreamWriter Writer { get; }
        protected internal StreamReader Reader { get; }

        TcpClient client;
        ServerObject server;
        private string _userName = "";
        private string _colorHex = "#000000"; // Default black

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            client = tcpClient;
            server = serverObject;
            var stream = client.GetStream();
            Reader = new StreamReader(stream);
            Writer = new StreamWriter(stream);
        }

        public async Task ProcessAsync()
        {
            try
            {
                while (true)
                {
                    string? message = await Reader.ReadLineAsync();
                    if (message == null) break;

                    var eventMsg = JsonSerializer.Deserialize<EventMessage>(message);
                    if (eventMsg == null) continue; 

                    switch (eventMsg.Type)
                    {
                        case EventType.PlayerConnected:
                            _userName = eventMsg.Username;
                            _colorHex = GetRandomColor();
                            
                            // Add new player to server state
                            server.ActivePlayers.Add(new PlayerInfo(_userName, _colorHex, 0, 0));
                            
                            Console.WriteLine($"{_userName} connected.");
                            break;

                        case EventType.PointPlaced:
                            // Update the specific player's position in the server list
                            var player = server.ActivePlayers.FirstOrDefault(p => p.Username == _userName);
                            if (player != null)
                            {
                                server.ActivePlayers.Remove(player);
                                server.ActivePlayers.Add(player with { DotX = eventMsg.X, DotY = eventMsg.Y });
                            }
                            break;
                    }

                    // Always broadcast the latest state of ALL players to ALL clients
                    var response = new EventMessage
                    {
                        Type = eventMsg.Type,
                        Players = server.ActivePlayers,
                        Username = _userName
                    };

                    await server.BroadcastMessageAllAsync(JsonSerializer.Serialize(response));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                server.RemoveConnection(Id);
                // Remove player from active list
                var p = server.ActivePlayers.FirstOrDefault(x => x.Username == _userName);
                if(p != null) server.ActivePlayers.Remove(p);
                Close();
            }
        }

        private string GetRandomColor()
        {
            var random = new Random();
            return String.Format("#{0:X6}", random.Next(0x1000000));
        }

        protected internal void Close()
        {
            Writer.Close();
            Reader.Close();
            client.Close();
        }
    }
}