using System.Diagnostics.CodeAnalysis;

namespace RestAPIFramework.Attribiutes
{
    [AttributeUsage(AttributeTargets.All)]
    public class RouteAttribute : Attribute
    {
        private string _path;

        [StringSyntax("Route")]
        public string Path { 
            get 
            { 
                return _path;
            } 
            init
            {
                _path = value;
                if (!_path.StartsWith("/"))
                {
                    _path = "/" + _path;
                }
            }
        }
        
        public RouteAttribute([StringSyntax("Route")] string path)
        {
            Path = path;
        }
    }
}
