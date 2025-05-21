using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestApp
{
    public interface ITestService
    {
        DateTime i { get; set; }
    }
    public class TestService : ITestService
    {
        DateTime ITestService.i { get; set; } = DateTime.Now;
    }
}
