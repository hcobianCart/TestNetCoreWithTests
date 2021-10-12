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
                        Auth0Management = new
                        {
                            audience = Configuration["Auth0Management:audience"],
                            client_id = Configuration["Auth0Management:client_id"],
                            Domain = Configuration["Auth0Management:Domain"],
                            client_secret = Configuration["Auth0Management:client_secret"]
                        },
                        Auth0 = new
                        {
                            Domain = Configuration["Auth0:Domain"],
                            ApiIdentifier = Configuration["Auth0:ApiIdentifier"]
                        },
                        Serilog = new 
                        {
                            Name = Configuration["Serilog:WriteTo:1:Name"],
                            applicationName = Configuration["Serilog:WriteTo:1:Args:applicationName"],
                            endpointUrl = Configuration["Serilog:WriteTo:1:Args:endpointUrl"],
                            restrictedToMinimumLevel = Configuration["Serilog:WriteTo:1:Args:restrictedToMinimumLevel"]
                        }
                    }
                    );
            }
            //else
                //return NotFound();
        }

    }
}
