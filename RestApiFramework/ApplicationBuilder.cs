using System.Net;
using System.Reflection;
using RestAPIFramework.Attribiutes;
using RestAPIFramework.Base;

namespace RestAPIFramework
{
    internal class HttpRequestContextIdenityfier
    {
        public required HttpListenerContext Context
        {
            get;
            set;
        }

        public object ScopeInstance;

        public string REP_ID
        {
            get
            {
                if (Context is null)
                    throw new ArgumentException("");

                return Context.Request.RemoteEndPoint.ToString();
            }
        }
    }
    public class ApplicationBuilder
    {
        internal static HashSet<ControllerContainer> ControllerContainers { get; set; } = new();

        internal static List<HttpRequestContextIdenityfier> ContextIdenityfiers { get; set; } = new();

        internal static HashSet<ServiceContainer> Services { get; set; } = new();
        /// <summary>
        /// Returns fresh instance of <see cref="ApplicationBuilder"/>
        /// </summary>
        /// <returns></returns>
        public static ApplicationBuilder CreateBuilder() {
            return new();
        }

        /// <summary>
        /// Appends all controllers in assembly to controller collection
        /// </summary>
        /// <param name="assembly"></param>
        public void UseControllers(Assembly assembly)
        {      
            var controllerTypes = assembly.GetTypes()
               .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(ControllerBase)))
               .ToList();

            foreach (var controllerType in controllerTypes) {
                foreach (var method in controllerType.GetMethods()) {

                    var test = controllerType.GetMethods();

                    var methodRoute = (RouteAttribute?)Attribute.GetCustomAttribute(method, typeof(RouteAttribute));

                    if(methodRoute == null) { continue; }

                    ControllerAttribute? ctrl = (ControllerAttribute?)Attribute.GetCustomAttribute(controllerType, typeof(ControllerAttribute));

                    RouteAttribute? route = (RouteAttribute?)Attribute.GetCustomAttribute(controllerType, typeof(RouteAttribute));

                    string path = string.Empty;

                    if (route == null)
                    {
                        path = ctrl.ControllerPath + methodRoute.Path;
                    }
                    else if (ctrl == null)
                    {
                        path = route.Path + methodRoute.Path;
                    }
                    else continue;

                    ControllerContainers.Add(
                        new ControllerContainer() 
                        { 
                            Controller = controllerType,
                            MethodInfo = method, 
                            Route = path
                        }
                    );

                    Console.Write($"Controler added: "); Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"{controllerType.Name}."); Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        /// <summary>
        /// Adds service type and implementation to service scope.
        /// </summary>
        /// <param name="interface"><see langword="typeof"/>(<see langword="interface"/> - service)</param>
        /// <param name="class"><see langword="typeof"/>(<see langword="class"/> - service implementation)</param>
        /// <param name="serviceType"><see langword="enum"/>service scope</param>
        /// <exception cref="ArgumentException"></exception>
        public void AddService(Type @interface, Type @class, ServiceType serviceType)
        {
            if (!@interface.IsInterface)
            {
                throw new ArgumentException($"{@interface.Name} must be an interface.");
            }

            if (!@class.IsClass)
            {
                throw new ArgumentException($"{@class.Name} must be a class.");
            }

            if (!@interface.IsAssignableFrom(@class))
            {
                throw new ArgumentException($"{@class.Name} does not implement {@interface.Name}");
            }

            if(Services.FirstOrDefault(s => s.Interface.Name == @interface.Name && s.Implementation.Name == @class.Name) == null)
            {
                Services.Add(new ServiceContainer() { Interface = @interface, Implementation = @class, ServiceType = serviceType });
                Console.Write($"Registered Service: ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{@class.Name}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" of type ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{@interface.Name}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" as ");
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"{serviceType}.");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                Console.Write("Operation was ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("canceled");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write($" due to ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{@class.Name}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" of type ");
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{@interface.Name}");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($" is already registered.");

            }

        }

        /// <summary>
        /// Asynchronus function that starts the API application.
        /// </summary>
        /// <returns>Program <see cref="Task"/></returns>
        public async Task Build()
        {
            await Program.StartApplication();
        }
    }
}