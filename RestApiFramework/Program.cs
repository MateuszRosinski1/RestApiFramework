using System.Net;
using RestAPIFramework;
using RestAPIFramework.Base;
using System.Text.Json;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Web;
using System.ComponentModel;
using RestAPIFramework.Attribiutes;
using System.Text.RegularExpressions;
using System.Linq;
class Program
{
    public static async Task Main(string[] args)
    {
        await StartApplication();
    }

    private const string base_url = "http://localhost:5000/";
    public static async Task StartApplication()
    {
        using var cts = new CancellationTokenSource();
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nShutting down server...");
            Console.ForegroundColor = ConsoleColor.White;
            cts.Cancel();
            e.Cancel = true;
        };

        HttpListener listener = new HttpListener();
        listener.Prefixes.Add(base_url);
        listener.Start();

        Console.Write("Server is listening on ");
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("http://localhost:5000/");
        Console.ForegroundColor = ConsoleColor.White;

        try
        {
            while (!cts.Token.IsCancellationRequested)
            {
                try
                {
                    HttpListenerContext context = await listener.GetContextAsync().WaitAsync(cts.Token);

                    if (context.Request.RawUrl.StartsWith("/favicon")) continue;

                    _ = ProcessRequestAsync(context);
                }
                catch (HttpListenerException ex) when (cts.Token.IsCancellationRequested)
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine("Critical error server shutdown.");
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                }
                catch (Exception ex)
                {
                    if (!(ex.GetType() == typeof(TaskCanceledException)))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error: {ex.Message}");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }
            }
        }
        finally
        {
            Console.WriteLine();
            listener.Stop();
            Console.ForegroundColor = ConsoleColor.Red;
            foreach(var c in "HttpListener listening aborted.")
            {
                Console.Write(c);
                Task.Delay(5).Wait();
            }
            Console.WriteLine();
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            listener.Close();
            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var c in "HttpListener closed succsefuly.")
            {
                Console.Write(c);
                Task.Delay(5).Wait();
            }
            Console.WriteLine();
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.White;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Server stopped.");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    private static async Task ProcessRequestAsync(HttpListenerContext context)
    {
        try
        {
            HttpListenerResponse response = context.Response;

            var rawUrl = context.Request.RawUrl;
            var result = InvokeControllerFunc(rawUrl,context);
            string json = System.Text.Json.JsonSerializer.Serialize(result);
            byte[] buffer = context.Request.ContentEncoding.GetBytes(json);
            response.ContentLength64 = buffer.Length;
            await response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            response.OutputStream.Close();

            CloseScope(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Invalid request handle: {ex.Message}");
            byte[] buffer = context.Request.ContentEncoding.GetBytes(ex.Message);
            context.Response.ContentLength64 = buffer.Length;
            await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
            context.Response.OutputStream.Close();

        }
    }

    private static void CloseScope(HttpListenerContext ctx)
    {
        var scopesToRemove = ApplicationBuilder.ContextIdenityfiers
            .Where(c => c.Context == ctx)
            .ToList();

        foreach (var scope in scopesToRemove)
        {
            ApplicationBuilder.ContextIdenityfiers.Remove(scope); 
        }     
    }

    private static object InvokeControllerFunc(string rawUrl,HttpListenerContext context)
    {        
        var ctrl = InstantiateController(rawUrl,context);
        var methodParams = GetMethodParams(context.Request, ctrl.Method, ctrl.Route);
        var results = ctrl.Method.Invoke(ctrl.ControllerInstance,methodParams);
        return results;        
    }

    private static object[] GetMethodParams(HttpListenerRequest request, MethodInfo method,string fullRoute)
    {
        JObject jobj = null;
        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
        {
            jobj = JObject.Parse(reader.ReadToEnd());
        }

        List<object?> parameters = new();

        var requestParams = HttpUtility.ParseQueryString(request.Url.Query);

        foreach (var param in method.GetParameters()) {

            FromRouteAttribute? fromroute = (FromRouteAttribute?)Attribute.GetCustomAttribute(param, typeof(FromRouteAttribute));
            FromBodyAttribute? frombody = (FromBodyAttribute?)Attribute.GetCustomAttribute(param, typeof(FromBodyAttribute));

            var paramName = param.Name.ToLower();
            var paramType = param.ParameterType;

            if (frombody != null) {
                var paramjObj = jobj.GetValue(paramName, StringComparison.CurrentCultureIgnoreCase);

                if (paramjObj != null) {
                    var paramObj = paramjObj.ToObject(paramType);
                    parameters.Add(paramObj);
                    continue;
                }
            }
            else if(fromroute != null) {

                var templateSegments = ("/" + fullRoute).Split('/').ToList();
                var actualSegments = request.RawUrl.Substring(0,request.RawUrl.IndexOf('?')).Split('/').ToList();

                var index = templateSegments.IndexOf("{"+param.Name+"}");
                var actuallValue = actualSegments[index];

                parameters.Add(Convert.ChangeType(actuallValue, paramType));
                continue;

            }
            else {

                var urlParam = requestParams.Get(paramName);

                var converter = TypeDescriptor.GetConverter(paramType);

                if (urlParam != null && converter != null && converter.CanConvertFrom(typeof(string)))
                {
                    var results = converter.ConvertFrom(urlParam);
                    parameters.Add(results);
                    continue;
                }

                parameters.Add(null);
                continue;
            }
            parameters.Add(null);
        }

        return parameters.ToArray();
    }
    
    private static ControllerCache InstantiateController(string rawurl, HttpListenerContext context)
    {

        string url = rawurl;
        int queryIndex = rawurl.IndexOf('?');
        if (queryIndex >= 0)
            url = rawurl.Substring(0, queryIndex);

        var urlSegments = url.Trim('/').Split('/');

        var ctrl = ApplicationBuilder.ControllerContainers.FirstOrDefault(ctrlContainer =>
        {
            var route = ctrlContainer.Route.Trim('/');
            var routeSegments = route.Split('/');

            if (routeSegments.Length != urlSegments.Length)
                return false;

            for (int i = 0; i < routeSegments.Length; i++)
            {
                if (routeSegments[i].StartsWith("{") && routeSegments[i].EndsWith("}"))
                    continue; 

                if (!string.Equals(routeSegments[i], urlSegments[i], StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        });
        //string url = rawurl.Substring(0, rawurl.IndexOf('?'));
        ////change controlers to controller type and then on each request init a controller istead of store them in list
        //var ctrl = ApplicationBuilder.ControllerContainers.First(c => c.Route == url.Substring(1));

        if (ctrl is null) throw new Exception($"There is no matching endpoint: {url}");

        var constructor = ctrl.Controller.GetConstructors().FirstOrDefault();

        if (constructor is null) Console.WriteLine("///////////////////////////////////");

        object[] @params = GetConstructorSerivceParams(constructor,context);

        var instance = (ControllerBase)Activator.CreateInstance(type: ctrl.Controller, args: @params);

        return new ControllerCache() { ControllerInstance = instance, Method = ctrl.MethodInfo,Route = ctrl.Route };
    }

    private static object[] GetConstructorSerivceParams(ConstructorInfo constructor, HttpListenerContext context)
    {
        List<object> parameters = new List<object>();
        foreach (var param in constructor.GetParameters())
        {
            try
            {               
                var service = ApplicationBuilder.Services.First(s => s.Interface == param.ParameterType);
                var cls = service.GetInstance(context);
                parameters.Add(cls);
            }
            catch
            {
                throw new Exception($"Cannot create instance of {constructor.DeclaringType.FullName} cause {param.ParameterType} was not registered in DependencyContainer");
            }
        }
        return parameters.ToArray();
    }

    internal class ControllerCache
    {
        public required ControllerBase ControllerInstance { get; set; }

        public required MethodInfo Method { get; set; }

        public required string Route { get; set; }
    }
}