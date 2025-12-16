using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using HttpServer01.shared;


HttpServer server = new HttpServer();
server.Start();


public class TaskAsyncResult : IAsyncResult
{
    public Task? InnerTask { get; }

    public object? AsyncState { get; }

    public TaskAsyncResult(Task? task, object? state)
    {
        InnerTask = task;
        AsyncState = state;
    }
    
    public WaitHandle AsyncWaitHandle => ((IAsyncResult)InnerTask).AsyncWaitHandle;
    public bool CompletedSynchronously => ((IAsyncResult)InnerTask).CompletedSynchronously;
    public bool IsCompleted => ((IAsyncResult)InnerTask).IsCompleted;
    
}


public class HttpListenerWrapper
{
    public HttpListener Listener;

    public HttpListenerWrapper(HttpListener listener)
    {
        Listener = listener;
    }


    public IAsyncResult BeginGetContext(AsyncCallback? callback, object? state)
    {
        var getContext = Listener.GetContextAsync();
        IAsyncResult result = new TaskAsyncResult(getContext, state);
        if (callback != null)
        {
            getContext.ContinueWith(t => callback(result));
        }
        
        return result;
    }

    public HttpListenerContext EndGetContext(IAsyncResult result)
    {
        if (result is not TaskAsyncResult taskAsyncResult) throw new ArgumentException("Invalid IAsyncResult result provided");

        return ((Task<HttpListenerContext>)taskAsyncResult.InnerTask).Result;
    }
}


public sealed class ServerConfig
{
    public static volatile ServerConfig? _instance;

    public static readonly object _lock = new object();

    public static SettingsModel Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new ServerConfig();
                    }
                }
            }

            return _instance.Settings;
        }
    }

    public SettingsModel Settings { get; }

private ServerConfig()
    {
        string settings = File.ReadAllText("settings.json");
        Settings = JsonSerializer.Deserialize<SettingsModel>(settings)!;
    }
}


public class ContentTypeProvider
{
    private Dictionary<string, string> _fileTypes = new()
    {
        {"css","text/css"},
        {"html", "text/html"},
        {"js","text/javascript"},
        {"png","image/png"},
    };
    
    public string GetType(String path)
    {
        _fileTypes.TryGetValue(Path.GetExtension(path).TrimStart('.').ToLowerInvariant(), out var type);
        if (type != null)
        {
            return type;
        }

        return "text/html";
    }
}


class HttpServer
{
    private readonly SettingsModel settings;
    private HttpListenerWrapper listenerWrapper;
    private HttpListener listener;

    public HttpServer()
    {
        settings = ServerConfig.Instance;
        listener = new HttpListener();
        listenerWrapper = new HttpListenerWrapper(listener);
    }
    

    public void Start()
    {
        listener.Prefixes.Add("http://" + settings.Domain + ":" + settings.Port + "/");
        
        listener.Start();
        Console.WriteLine("Server is started");
        
        Receive();

        Console.ReadLine();
        Console.WriteLine("Server is stopped");
    }

    private void Receive()
    { 
        listenerWrapper.BeginGetContext(ListenerCallback, listener);
    }

    public void Stop()
    {
        listenerWrapper.Listener.Stop();
        Console.WriteLine("Server is stopped");
    }
    
  
    private async void ListenerCallback(IAsyncResult result)
    {
        if (!listener.IsListening) return;
        
        Console.WriteLine("New user came");
        
        var context = listenerWrapper.EndGetContext(result);
        var request = context.Request;
        var response = context.Response;
        var path = settings.StaticDirectoryPath + request.Url.LocalPath;
        Console.WriteLine($"Local path {request.Url.LocalPath}");
        if (path[^1] == '/') path += "index.html";

        try
        {
            /*
             * type getting
             */
            ContentTypeProvider typeProvider = new ();
            string type = typeProvider.GetType(path);
            Console.WriteLine($"File type {type}");
            response.ContentType = type;
            
            using (var output = response.OutputStream)
            using (var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                       bufferSize: 4096, useAsync: true))
            {
                response.ContentLength64 = fileStream.Length;
                await fileStream.CopyToAsync(output);
                await output.FlushAsync(); 
            }
                
            /*
             //_fileTypes.TryGetValue(path.Split(".")[1], out var type);
            var responseText = await File.ReadAllBytesAsync(path);
            //response.ContentLength64 = responseText.Length;
            await output.WriteAsync(responseText);
            */
        }
        catch (DirectoryNotFoundException e)
        {
            Console.WriteLine("static folder not found");
            response.StatusCode = 404;
        }
        catch (FileNotFoundException e)
        {
            Console.WriteLine($"index.html is not found in {path}");
            response.StatusCode = 404;
        }
        catch (Exception e)
        {
            Console.WriteLine($"There is an exception: {e.Message}");
            response.StatusCode = 500;
            Stop();
        }
        finally
        {
            response.Close();
        }
        
        Receive();
    }
}

