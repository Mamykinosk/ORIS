using MiniHttpServer.Core.Abstracts;
using MiniHttpServer.Core.Attributes;
using System.Net;
using System.Reflection;

namespace MiniHttpServer.Core.Handlers
{
    internal class EndpointsHandler : Handler
    {
        public override void HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var pathSegments = request.Url?.AbsolutePath
                .Split('/', StringSplitOptions.RemoveEmptyEntries);
            
            if (pathSegments == null || pathSegments.Length == 0)
            {
                Successor?.HandleRequest(context);
                return;
            }

            var endpointName = pathSegments[0];  
            var route = pathSegments.Length > 1 ? pathSegments[1] : null;

            var assembly = Assembly.GetExecutingAssembly();
            var endpoint = assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<EndpointAttribute>() != null)
                .FirstOrDefault(end => IsCheckedNameEndpoint(end.Name, endpointName));
            
            if (endpoint == null)
            {
                if (Successor != null)
                {
                    Successor.HandleRequest(context);
                }
                else
                {
                    Send404(context);
                }
                return;
            }

            var method = endpoint.GetMethods()
                .FirstOrDefault(m => MatchesRoute(m, request.HttpMethod, route));

            if (method == null)
            {
                Send404(context);
                Console.WriteLine("Null method");
                return;
            }
            
            Console.WriteLine("Sending message");
            InvokeMethod(endpoint, method, context);
        }

        private bool MatchesRoute(MethodInfo method, string httpMethod, string? route)
        {
            var httpAttr = method.GetCustomAttributes(true)
                .FirstOrDefault(attr => attr.GetType().Name
                    .Equals($"Http{httpMethod}", StringComparison.OrdinalIgnoreCase));
            
            if (httpAttr == null) return false;

            var routeProp = httpAttr.GetType().GetProperty("Route");
            var attrRoute = routeProp?.GetValue(httpAttr) as string; 

            
            Console.WriteLine(attrRoute);
            if (route == null && string.IsNullOrEmpty(attrRoute)) return true;
            if (route != null && attrRoute != null && 
                route.Equals(attrRoute, StringComparison.OrdinalIgnoreCase)) return true;
            
            return false;
        }

        private void InvokeMethod(Type endpoint, MethodInfo method, HttpListenerContext context)
        {
            try
            {
                var instance = Activator.CreateInstance(endpoint);
                var parameters = method.GetParameters();
                
                if (parameters.Length > 0 && parameters[0].ParameterType == typeof(HttpListenerContext))
                {
                    Console.WriteLine($"Invoking method {method.Name}");
                    method.Invoke(instance, new object[] { context });
                }
                else
                {   
                    method.Invoke(instance, null);
                    context.Response.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error invoking endpoint method: {ex.InnerException?.Message ?? ex.Message}");
                context.Response.StatusCode = 500;
                
                var errorHtml = "<html><body><h1>500 - Internal Server Error</h1></body></html>";
                var buffer = System.Text.Encoding.UTF8.GetBytes(errorHtml);
                context.Response.ContentType = "text/html; charset=UTF-8";
                context.Response.ContentLength64 = buffer.Length;
                context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                context.Response.Close();
            }
        }

        private void Send404(HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            var html = "<html><body><h1>404 - Not Found</h1></body></html>";
            var buffer = System.Text.Encoding.UTF8.GetBytes(html);
            context.Response.ContentType = "text/html; charset=UTF-8";
            context.Response.ContentLength64 = buffer.Length;
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            context.Response.Close();
        }

        private bool IsCheckedNameEndpoint(string className, string endpointName) =>
            className.Equals(endpointName, StringComparison.OrdinalIgnoreCase) ||
            className.Equals($"{endpointName}Endpoint", StringComparison.OrdinalIgnoreCase);
    }
}