
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
    [Route(".info")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly IConfiguration _config;
        public ConfigurationController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet]
        public async Task<IActionResult> GetInfo()
        {
            AppInfo res = _config.GetSection("AppInfo").Get<AppInfo>() ?? throw new ArgumentNullException("Missing configuration: 'AppInfo'");
            return Ok(res);
        }
    }
}

public class AppInfo
{
    public string TenantId {get; set;} = String.Empty;
    public string ClientId {get; set;} = String.Empty;
    public string RedirectUri {get; set;} = String.Empty;
    public string Authority {get; set;} = String.Empty;
    public string[] KnownAuthorities {get; set;} = new string[] {};
    public string[] Scopes {get; set;} = new string[] {};
    public string ApiUri {get; set;} = String.Empty;

}
