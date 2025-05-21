using System.Reflection;
using RestAPIFramework;
using RestApp;


var builder = ApplicationBuilder.CreateBuilder();

builder.AddService(typeof(ITestService), typeof(TestService), ServiceType.Scope);

var assembly = Assembly.Load(nameof(RestApp));

builder.UseControllers(assembly);



await builder.Build();