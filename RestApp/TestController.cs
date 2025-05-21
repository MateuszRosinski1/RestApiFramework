using RestAPIFramework.Attribiutes;
using RestAPIFramework.Base;

namespace RestApp
{
    [Controller("test")]
    public class TestController : ControllerBase
    {

        [Route("test/{testParasd}")]
        public string Test([FromBody]User user, [FromBody] Guid guid,string phonenumber,[FromRoute]int testParam)
        {
            return "s";
        }
    }

    public class User
    {
        public string Username { get; set; }

        public string Email { get; set; }
    }
}