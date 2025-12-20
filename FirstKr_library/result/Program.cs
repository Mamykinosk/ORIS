using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;

namespace SimpleAuthRightSide
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AnonymousAttribute : Attribute { }

    public static class SessionManager
    {
        public static HashSet<string> ValidSessions = new HashSet<string>();
        public const string CookieName = "SessionKey";
    }

    [Authorize]
    public class MainController
    {
        public void Index(HttpListenerContext context)
        {
            var cookie = context.Request.Cookies[SessionManager.CookieName];
            string status = (cookie == null) ? "Anonymous Mode" : "Authorized User";

            string rightSideMessage = "";
            if (context.Request.QueryString["msg"] == "success")
            {
                rightSideMessage = "SUCCESS";
            }

            string html = $@"
                <html>
                <body style='font-family: sans-serif; padding: 20px;'>
                    <h1>Main Controller</h1>
                    <p>Current Status: <b>{status}</b></p>
                    
                    <div style='display: flex; justify-content: space-between; align-items: flex-start;'>
                        
                        <div style='width: 60%; border: 1px solid #ccc; padding: 20px;'>
                            <h3>Login Form</h3>
                            <form action='/Login' method='POST'>
                                <p>
                                    Password: <input type='text' name='password'>
                                </p>
                                <p>
                                    <button type='submit' name='mode' value='login'>Login (Password 123)</button>
                                </p>
                                <p>
                                    <button type='submit' name='mode' value='anonymous'>Enter as Anonymous</button>
                                </p>
                            </form>
                            
                            <h3>Actions</h3>
                            <form action='/DeleteSession' method='POST'>
                                <button type='submit'>Delete Session Key (Trigger 401)</button>
                            </form>
                        </div>

                        <div style='width: 35%; text-align: right; color: green; font-size: 30px; font-weight: bold; padding-top: 50px;'>
                            {rightSideMessage}
                        </div>

                    </div>
                </body>
                </html>";

            SendResponse(context, 200, html);
        }

        [Anonymous]
        public void Login(HttpListenerContext context)
        {
            if (context.Request.HttpMethod != "POST")
            {
                Redirect(context, "/");
                return;
            }

            string body;
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                body = reader.ReadToEnd();
            }
            var formData = ParseFormData(body);
            
            string mode = formData.ContainsKey("mode") ? formData["mode"] : "";
            string password = formData.ContainsKey("password") ? formData["password"] : "";

            if (mode == "anonymous")
            {
                SetCookie(context, null);
                Redirect(context, "/?msg=success");
            }
            else if (mode == "login")
            {
                if (password == "123")
                {
                    string key = Guid.NewGuid().ToString();
                    SessionManager.ValidSessions.Add(key);
                    SetCookie(context, key);
                    Redirect(context, "/?msg=success");
                }
                else
                {
                    SetCookie(context, "INVALID_" + Guid.NewGuid());
                    Redirect(context, "/");
                }
            }
            else
            {
                Redirect(context, "/");
            }
        }

        [Anonymous]
        public void Logout(HttpListenerContext context)
        {
            SetCookie(context, null);
            Redirect(context, "/");
        }

        [Anonymous]
        public void DeleteSession(HttpListenerContext context)
        {
            var cookie = context.Request.Cookies[SessionManager.CookieName];
            if (cookie != null)
            {
                SessionManager.ValidSessions.Remove(cookie.Value);
            }
            Redirect(context, "/");
        }

        private void SendResponse(HttpListenerContext context, int statusCode, string html)
        {
            context.Response.StatusCode = statusCode;
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }

        private void Redirect(HttpListenerContext context, string url)
        {
            context.Response.Redirect(url);
            context.Response.Close();
        }

        private void SetCookie(HttpListenerContext context, string value)
        {
            if (value == null)
                context.Response.AddHeader("Set-Cookie", $"{SessionManager.CookieName}=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT");
            else
                context.Response.AddHeader("Set-Cookie", $"{SessionManager.CookieName}={value}; Path=/");
        }

        private Dictionary<string, string> ParseFormData(string body)
        {
            var dict = new Dictionary<string, string>();
            var pairs = body.Split('&');
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=');
                if (parts.Length == 2)
                {
                    string key = Uri.UnescapeDataString(parts[0]);
                    string val = Uri.UnescapeDataString(parts[1].Replace("+", " "));
                    dict[key] = val;
                }
            }
            return dict;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8080/");
            listener.Start();
            Console.WriteLine("Server running on http://localhost:8080/");

            while (true)
            {
                var context = listener.GetContext();
                ProcessRequest(context);
            }
        }

        static void ProcessRequest(HttpListenerContext context)
        {
            string path = context.Request.Url.AbsolutePath.Trim('/');
            if (string.IsNullOrEmpty(path)) path = "Index";

            var controllerType = typeof(MainController);
            var method = controllerType.GetMethod(path);

            if (method == null)
            {
                RespondError(context, 404, "Not Found");
                return;
            }

            bool isAnonymous = Attribute.IsDefined(method, typeof(AnonymousAttribute)) ||
                               Attribute.IsDefined(controllerType, typeof(AnonymousAttribute));

            if (isAnonymous)
            {
                InvokeMethod(method, controllerType, context);
                return;
            }

            bool isAuthorize = Attribute.IsDefined(method, typeof(AuthorizeAttribute)) ||
                               Attribute.IsDefined(controllerType, typeof(AuthorizeAttribute));

            if (isAuthorize)
            {
                var cookie = context.Request.Cookies[SessionManager.CookieName];

                if (cookie == null)
                {
                    InvokeMethod(method, controllerType, context);
                }
                else
                {
                    if (SessionManager.ValidSessions.Contains(cookie.Value))
                    {
                        InvokeMethod(method, controllerType, context);
                    }
                    else
                    {
                        RespondError(context, 401, "<h1>401 Unauthorized</h1><p><a href='/Logout'>Reset/Logout</a></p>");
                    }
                }
            }
            else
            {
                InvokeMethod(method, controllerType, context);
            }
        }

        static void InvokeMethod(MethodInfo method, Type controllerType, HttpListenerContext context)
        {
            try
            {
                object instance = Activator.CreateInstance(controllerType);
                method.Invoke(instance, new object[] { context });
            }
            catch (Exception ex)
            {
            }
        }

        static void RespondError(HttpListenerContext context, int statusCode, string htmlBody)
        {
            context.Response.StatusCode = statusCode;
            string html = $"<html><body>{htmlBody}</body></html>";
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();
        }
    }
}













