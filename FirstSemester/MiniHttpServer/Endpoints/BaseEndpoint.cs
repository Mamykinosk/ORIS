using System.Net;
using System.Text;

namespace MiniHttpServer.Endpoints
{
    public abstract class BaseEndpoint
    {
        protected async void SendHtml(HttpListenerContext context, string html, int statusCode = 200)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(html);
            context.Response.ContentType = "text/html; charset=UTF-8";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.StatusCode = statusCode;

            using (var output = context.Response.OutputStream)
            {
                await output.WriteAsync(buffer, 0, buffer.Length);
                await output.FlushAsync();  
            }
            context.Response.Close();
        }
        
        protected async void SendError(HttpListenerContext context, int code, string message)
        {
            context.Response.StatusCode = code;
            string errorHtml = $"<h1>Error {code}</h1><p>{message}</p>";
            byte[] buffer = Encoding.UTF8.GetBytes(errorHtml);
            context.Response.ContentType = "text/html; charset=UTF-8";
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            await context.Response.OutputStream.FlushAsync();
            context.Response.Close();
        }

        protected string ReadRequestBody(HttpListenerContext context)
        {
            if (!context.Request.HasEntityBody) return string.Empty;

            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                return reader.ReadToEnd();
            }
        }

        protected Dictionary<string, string> ParseFormData(string formData)
        {
            var result = new Dictionary<string, string>();
            if (string.IsNullOrEmpty(formData)) return result;

            var pairs = formData.Split('&');
            foreach (var pair in pairs)
            {
                var keyValue = pair.Split('=');
                if (keyValue.Length == 2)
                {
                    string key = WebUtility.UrlDecode(keyValue[0]);
                    string value = WebUtility.UrlDecode(keyValue[1]);
                    result[key] = value;
                }
            }
            return result;
        }

        protected void Redirect(HttpListenerContext context, string url)
        {
            context.Response.Redirect(url);
            context.Response.Close();
        }
    }
}