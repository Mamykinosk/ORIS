using MiniHttpServer.Core.Abstracts;
using MiniHttpServer.Shared;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MiniHttpServer.Core.Handlers
{
    internal class StaticFilesHandler : Handler
    {
        private const string DEFAULT_FILE = "index.html";
        
        public override async void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            try
            {
                bool isGetMethod = request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase);
                
                if (!isGetMethod)
                {
                    PassToSuccessor(context);
                    return;
                }

                string requestedPath = request.Url?.AbsolutePath ?? "/";
                
                string path = requestedPath.Trim('/');

                bool isRootPath = string.IsNullOrEmpty(path);
                bool isStaticFile = !string.IsNullOrEmpty(path) && path.Contains('.');

                if (isRootPath || isStaticFile)
                {
                    await HandleStaticFileRequest(context, path, isRootPath);
                }
                else
                {
                    PassToSuccessor(context);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"taticFilesHandler error: {ex.Message}");
                Console.ResetColor();
                
                if (!response.OutputStream.CanWrite)
                    return;

            }
        }
        
        private async Task HandleStaticFileRequest(HttpListenerContext context, string path, bool isRootPath)
        {
            var response = context.Response;
            byte[]? buffer = null;
            string filePath = path;

            try
            {
                if (isRootPath)
                {
                    filePath = DEFAULT_FILE;
                }

                buffer = GetResponseBytes.Invoke(filePath);

                if (buffer != null)
                {
                    string contentType = ContentType.GetContentType(filePath);
                    
                    response.ContentType = contentType;
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = 200;

                    using (Stream output = response.OutputStream)
                    {
                        await output.WriteAsync(buffer, 0, buffer.Length);
                        await output.FlushAsync();
                    }

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"200 OK - {filePath} ({contentType}, {buffer.Length} bytes)");
                    Console.ResetColor();
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error serving file {filePath}: {ex.Message}");
                Console.ResetColor();
                
            }
        }
        
        private void PassToSuccessor(HttpListenerContext context)
        {
            if (Successor != null)
            {
                Console.WriteLine($"Passing to next handler: {context.Request.Url?.AbsolutePath}");
                Successor.HandleRequest(context);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"No successor handler available for: {context.Request.Url?.AbsolutePath}");
                Console.ResetColor();
            }
        }
    }
}
