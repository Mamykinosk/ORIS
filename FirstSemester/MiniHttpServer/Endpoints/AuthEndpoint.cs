using MiniHttpServer.Core;
using MiniHttpServer.Core.Attributes;
using MiniHttpServer.Models;
using MiniHttpServer.Settings;
using MiniORM;
using TemplateEngine;
using System.Net;

namespace MiniHttpServer.Endpoints
{
    [Endpoint]
    internal class AuthEndpoint : BaseEndpoint
    {
        private readonly ORMContext _orm;

        public AuthEndpoint()
        {
            var settings = Singleton.GetInstance().Settings;
            _orm = new ORMContext(settings.ConnectionString);
        }

        [HttpGet("login")]
        public void LoginPage(HttpListenerContext context)
        {
            if (SessionManager.GetUser(context) != null)
            {
                Redirect(context, "/");
                return;
            }

            var renderer = new HtmlTemplateRenderer();
            string html = renderer.RenderFromFile("Public/login.html", new { Error = "" });
            SendHtml(context, html);
        }

        [HttpPost("login")]
        public void LoginProcess(HttpListenerContext context)
        {
            string formData = ReadRequestBody(context);
            var data = ParseFormData(formData);

            string email = data.ContainsKey("email") ? data["email"] : "";
            string password = data.ContainsKey("password") ? data["password"] : "";

            var users = _orm.ReadWhere<User>("users", "Email", email);
            var user = users.FirstOrDefault();

            if (user != null && user.Password == password)
            {
                string sessionId = SessionManager.CreateSession(user);
                
                context.Response.AddHeader("Set-Cookie", $"session_id={sessionId}; Path=/; HttpOnly");

                if (user.Role == "admin") Redirect(context, "/admin/dashboard");
                else Redirect(context, "/tours/list");
            }
            else
            {
                var renderer = new HtmlTemplateRenderer();
                string html = renderer.RenderFromFile("Public/login.html", new { Error = "Неверный email или пароль" });
                SendHtml(context, html, 401);
            }
        }

        [HttpGet("logout")]
        public void Logout(HttpListenerContext context)
        {
            SessionManager.RemoveSession(context);
            context.Response.AddHeader("Set-Cookie", "session_id=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT");
            Redirect(context, "/tours/list");
        }
        
        [HttpGet("register")]
        public void RegisterPage(HttpListenerContext context)
        {
            if (SessionManager.GetUser(context) != null)
            {
                Redirect(context, "/");
                return;
            }

            var renderer = new HtmlTemplateRenderer();
            string html = renderer.RenderFromFile("Public/register.html", new { Error = "" });
            SendHtml(context, html);
        }

        [HttpPost("register")]
        public void RegisterProcess(HttpListenerContext context)
        {
            string formData = ReadRequestBody(context);
            var data = ParseFormData(formData);

            string name = data.ContainsKey("name") ? data["name"] : "User";
            string email = data.ContainsKey("email") ? data["email"] : "";
            string password = data.ContainsKey("password") ? data["password"] : "";

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                var renderer = new HtmlTemplateRenderer();
                string html = renderer.RenderFromFile("Public/register.html", new { Error = "Заполните все поля" });
                SendHtml(context, html);
                return;
            }

            var existingUsers = _orm.ReadWhere<User>("users", "Email", email);
            if (existingUsers.Count > 0)
            {
                var renderer = new HtmlTemplateRenderer();
                string html = renderer.RenderFromFile("Public/register.html", new { Error = "Пользователь с таким Email уже существует" });
                SendHtml(context, html);
                return;
            }
    
            /*
            string sql = $"INSERT INTO \"users\" (\"Name\", \"Email\", \"Password\", \"Role\") VALUES ('{name}', '{email}', '{password}', 'customer')";
            
            _orm.ExecuteSql(sql);
            */

            User user = new User()
            {
                Name = name,
                Email = email,
                Password = password,
                Role = "customer"
            };
            _orm.Insert("users", user);

            Redirect(context, "/auth/login");
        }
    }
}