using System.Reflection;

namespace RestAPIFramework
{
    internal class ControllerContainer
    {
        public Type Controller { get; set; }
        public MethodInfo MethodInfo { get; set; }
        public string Route { get; set; }
    }
}
