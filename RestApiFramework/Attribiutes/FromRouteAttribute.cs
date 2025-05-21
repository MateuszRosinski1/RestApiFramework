using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestAPIFramework.Attribiutes
{
    [AttributeUsage(AttributeTargets.Parameter,AllowMultiple = false)]
    public class FromRouteAttribute : Attribute
    {
    }
}
