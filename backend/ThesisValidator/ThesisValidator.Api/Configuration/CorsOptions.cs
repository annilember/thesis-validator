using System.ComponentModel.DataAnnotations;

namespace ThesisValidator.Api.Configuration;

public class CorsOptions
{
    [Required]
    public string AllowedOrigins { get; set; } = string.Empty;
}