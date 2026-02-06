using SistemaLavaJato.Web.Models.ViewModels;

namespace SistemaLavaJato.Web.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}
