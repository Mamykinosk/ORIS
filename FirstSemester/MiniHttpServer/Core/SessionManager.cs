using MiniHttpServer.Models;
using System.Net;

namespace MiniHttpServer.Core
{
    public static class SessionManager
    {
        private static Dictionary<string, User> _sessions = new Dictionary<string, User>();
        
        public static string CreateSession(User user)
        {
            string sessionId = Guid.NewGuid().ToString();
            _sessions[sessionId] = user;
            return sessionId;
        }
        
        public static User? GetUser(HttpListenerContext context)
        {
            var cookie = context.Request.Cookies["session_id"];
            if (cookie != null && _sessions.ContainsKey(cookie.Value))
            {
                return _sessions[cookie.Value];
            }
            return null;
        }
        
        public static void RemoveSession(HttpListenerContext context)
        {
            var cookie = context.Request.Cookies["session_id"];
            if (cookie != null && _sessions.ContainsKey(cookie.Value))
            {
                _sessions.Remove(cookie.Value);
            }
        }
    }
}