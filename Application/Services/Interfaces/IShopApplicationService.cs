using Domain.Entities;
using Domain.Enums;

namespace Application.Services.Interfaces;

public interface IShopApplicationService
{
    Task<bool> SubmitApplicationAsync(ShopApplication application);
    Task<List<ShopApplication>> GetPendingApplicationsAsync();
    Task<List<ShopApplication>> GetAllApplicationsAsync();
    Task<List<ShopApplication>> GetApplicationsByApplicantIdAsync(int applicantId);
    Task<ShopApplication?> GetApplicationByIdAsync(int id);
    Task<bool> ApproveApplicationAsync(int applicationId, int reviewerId);
    Task<bool> RejectApplicationAsync(int applicationId, int reviewerId, string reason);
    Task<bool> HasPendingApplicationAsync(int applicantId);
}
