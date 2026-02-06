using Microsoft.AspNetCore.Identity;

namespace SistemaLavaJato.Web.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string? NomeCompleto { get; set; }
}
