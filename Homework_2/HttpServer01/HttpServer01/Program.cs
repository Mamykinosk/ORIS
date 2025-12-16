using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using HttpServer01.shared;


HttpListener server = new HttpListener();

try
{
    CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

    string settings = File.ReadAllText("settings.json");
    SettingsModel settingsModel = JsonSerializer.Deserialize<SettingsModel>(settings);

    server.Prefixes.Add("http://" + settingsModel.Domain + ":" + settingsModel.Port + "/");
    server.Start();
    Console.WriteLine("Server is started");
    
    var serverTask = Task.Run(async () =>
    {
        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            Console.WriteLine("Server is awaiting for request");
            
            try
            {
                var context = await server.GetContextAsync();
                if (_cancellationTokenSource.Token.IsCancellationRequested)
                {
                    break;
                }

                Console.WriteLine(context.Request.HttpMethod + " " + context.Request.Url + " " +
                                  context.Request.QueryString + " " + context.Request.ProtocolVersion + " " +
                                  context.Request.Headers + " " + context.Request.Cookies);
                Console.WriteLine(context.Request.ContentType);

                var response = context.Response;
                
                string responseText = File.ReadAllText(settingsModel.StaticDirectoryPath + "index.html");
                byte[] buffer = Encoding.UTF8.GetBytes(responseText);
                response.ContentLength64 =
                    buffer.Length; // and this makes such that when output.WriteAsync was called, there was enough place in the array to write encoded bytecode?
                // so it creates enough length for Stream's response by HttpListenerResponse.OutputStream ?
                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer);
                await output
                    .FlushAsync(); // does it burn response.ContentLength64 value, as there can value with some other lenght in next execution?

                Console.WriteLine("Запрос обработан");
            }
            catch (HttpListenerException) when (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                break;
            }
            catch (ObjectDisposedException) when (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                break;
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("static folder not found");
                _cancellationTokenSource.Cancel();
                break;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("index.html is not found in static folder");
                _cancellationTokenSource.Cancel();
                break;
            }
            catch (Exception e)
            {
                Console.WriteLine("There is an exception: " + e.Message);
                _cancellationTokenSource.Cancel();
                break;
            }
        }
    }, _cancellationTokenSource.Token);

    var inputTask = Task.Run(() =>
    {
        while (true)
        {
            string command = Console.ReadLine();
            if (command == "/stop")
            {
                Console.WriteLine("Stop command received");
                _cancellationTokenSource.Cancel();
                server.Stop();
                break;
            }
        }
    });

    await Task.WhenAll(serverTask, inputTask);
    Console.WriteLine("Server stopped");

}
catch (ObjectDisposedException)
{

} 
catch (Exception e) when (e is DirectoryNotFoundException or FileNotFoundException)
{
    Console.WriteLine("settings are not found");
}
catch (JsonException e)
{
    Console.WriteLine("settings.json is incorrect");
}
catch (Exception e)
{
    Console.WriteLine("There is an exception: " + e.Message);
}
finally
{
    if (server.IsListening)
    {
        server.Close();
    }
}