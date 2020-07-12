using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace MSGraphAuthApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthCodeController : ControllerBase
    {
        [HttpGet]
#nullable enable
        public string Get(string? code, string? error)
#nullable disable
        {
            if (!string.IsNullOrWhiteSpace(code))
                return JsonSerializer.Serialize(new { code });

            if (!string.IsNullOrWhiteSpace(error))
                return JsonSerializer.Serialize(new { error });

            return "https://github.com/NTUT-SELab/MicrosoftGraphBot";
        }
    }
}
