namespace RestAPIFramework.Attribiutes
{
    [AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
    public class ControllerAttribute : Attribute
    {
        private string _path;
        public string ControllerPath { get { return _path; } }

        public ControllerAttribute(string path)
        {
            if(path.StartsWith("/")) path = path.Substring(1);
            _path = path;   
        }

        public ControllerAttribute()
        {
            _path = "";
        }
    }
}
