using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace testAPI.Controllers
{

    

    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {

        public TestController(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        // GET: api/<Test>
        [HttpGet]
        public ActionResult Get()
        {

            //if (!Env.IsProduction())
            {
                return Ok(
                    new
                    {
                        IsDevelopment = Env.IsDevelopment(),
                        EnvironmentName = Env.EnvironmentName,
                        audience = Configuration["Auth0Management:audience"],
                        client_id = Configuration["Auth0Management:client_id"],
                        Domain = Configuration["Auth0Management:Domain"]
                    }
                    );
            }
            //else
                //return NotFound();
        }

    }
}
