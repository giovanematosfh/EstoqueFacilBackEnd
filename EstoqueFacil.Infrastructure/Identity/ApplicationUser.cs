using EstoqueFacil.Domain.Model;
using Microsoft.AspNetCore.Identity;

namespace EstoqueFacil.Infrastructure.Identity
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;

        public int CompanyId { get; set; }
        public Company Company { get; set; } = null!;
    }
}
