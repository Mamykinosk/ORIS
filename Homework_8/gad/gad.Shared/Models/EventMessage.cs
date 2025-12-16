using GameAndDot.Shared.Enums;

namespace GameAndDot.Shared.Models
{
    public class EventMessage
    {
        public EventType Type { get; set; }
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        
        // The list of all players (sent from server to client)
        public List<PlayerInfo>? Players { get; set; }

        // Coordinates for a new point (sent from client to server)
        public int X { get; set; }
        public int Y { get; set; }
    }
}