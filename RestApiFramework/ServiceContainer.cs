using System.Net;

namespace RestAPIFramework
{
    internal class ServiceContainer
    {
        public Type Interface { get; init; }
        public Type Implementation { get; init; }
        public ServiceType ServiceType { get; init; }

        private object _instance;
        private object GetSingleton()
        {
            if (_instance == null)
            {
                _instance = Activator.CreateInstance(Implementation);
            }
            return _instance;
        }
        public object GetInstance(HttpListenerContext ctx)
        {
            switch (ServiceType)
            {
                case ServiceType.Singleton:
                    return GetSingleton();
                case ServiceType.Transient:
                    return Activator.CreateInstance(Implementation);
                case ServiceType.Scope:
                    var right = ctx.Request.RemoteEndPoint.ToString() + this.Implementation.ToString();
                    var idenity = ApplicationBuilder.ContextIdenityfiers.FirstOrDefault(c => c.REP_ID + c.ScopeInstance.ToString() == right);
                    if (idenity == null)
                    {
                        var httprct = new HttpRequestContextIdenityfier() { Context = ctx, ScopeInstance = Activator.CreateInstance(Implementation) };
                        ApplicationBuilder.ContextIdenityfiers.Add(httprct);
                        return httprct.ScopeInstance;
                    }
                    else
                    {
                        if (idenity.ScopeInstance is null) throw new ArgumentNullException("");

                        return idenity.ScopeInstance;
                    }
            }
            throw new Exception("Service scope not found");
        }
    }

    public enum ServiceType
    {
        Singleton = 1,
        Transient = 2,
        Scope = 3
    }
}
