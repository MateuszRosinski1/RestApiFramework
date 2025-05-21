namespace RestAPIFramework.Base
{
    public class ControllerBase
    {
        private string _path;
        public string Path {
            get { return _path; }
            set { 
                if (_path != value) 
                    _path = value;

                if(!value.EndsWith("/"))
                {
                    _path += "/";
                }
            } 
        }
    }
}
