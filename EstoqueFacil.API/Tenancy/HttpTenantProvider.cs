using EstoqueFacil.Application.Constants;
using EstoqueFacil.Application.Interfaces.Services;

namespace EstoqueFacil.API.Tenancy
{
    // Lê a empresa do usuário a partir da claim company_id do JWT.
    public class HttpTenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpTenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public int? CompanyId
        {
            get
            {
                var claim = _httpContextAccessor.HttpContext?.User.FindFirst(TenantClaims.CompanyId)?.Value;
                return int.TryParse(claim, out var companyId) ? companyId : null;
            }
        }
    }
}
