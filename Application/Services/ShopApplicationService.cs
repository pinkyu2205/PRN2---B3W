using Application.Services.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services;

public class ShopApplicationService : IShopApplicationService
{
    private readonly IUnitOfWork _uow;
    private readonly IShopService _shopService;
    private readonly IRealtimeService _realtimeService;
    private readonly INotificationService _notificationService;

    public ShopApplicationService(IUnitOfWork uow, IShopService shopService, IRealtimeService realtimeService, INotificationService notificationService)
    {
        _uow = uow;
        _shopService = shopService;
        _realtimeService = realtimeService;
        _notificationService = notificationService;
    }

    public async Task<bool> SubmitApplicationAsync(ShopApplication application)
    {
        try
        {
            application.Status = ShopApplicationStatus.Pending;
            await _uow.ShopApplicationRepository.AddAsync(application);
            await _uow.SaveChangesAsync();

            // Real-time notification to admin
            await _realtimeService.SendNotificationToRoleAsync("Administrator", "Hồ sơ mới", $"Người dùng {application.ApplicantId} nộp hồ sơ mở shop.", "info");
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<List<ShopApplication>> GetPendingApplicationsAsync()
    {
        try
        {
            var apps = await _uow.ShopApplicationRepository.GetByStatusAsync(ShopApplicationStatus.Pending);
            return apps.ToList();
        }
        catch (Exception)
        {
            return new List<ShopApplication>();
        }
    }

    public async Task<List<ShopApplication>> GetAllApplicationsAsync()
    {
        try
        {
            var apps = await _uow.ShopApplicationRepository.GetAllAsync();
            return apps.ToList();
        }
        catch (Exception)
        {
            return new List<ShopApplication>();
        }
    }

    public async Task<List<ShopApplication>> GetApplicationsByApplicantIdAsync(int applicantId)
    {
        try
        {
            var apps = await _uow.ShopApplicationRepository.GetByApplicantIdAsync(applicantId);
            return apps.ToList();
        }
        catch (Exception)
        {
            return new List<ShopApplication>();
        }
    }

    public async Task<ShopApplication?> GetApplicationByIdAsync(int id)
    {
        try
        {
            return await _uow.ShopApplicationRepository.FindOneAsync(
                a => a.Id == id,
                includeProperties: "Applicant");
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> ApproveApplicationAsync(int applicationId, int reviewerId)
    {
        try
        {
            var application = await _uow.ShopApplicationRepository.FindOneAsync(
                a => a.Id == applicationId,
                includeProperties: "Applicant");

            if (application == null || application.Status != ShopApplicationStatus.Pending)
                return false;

            // Update application status
            application.Status = ShopApplicationStatus.Approved;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedBy = reviewerId;
            application.ModifiedAt = DateTime.UtcNow;
            _uow.ShopApplicationRepository.Update(application);

            // Create shop for the applicant
            var shop = new Shop
            {
                ShopName = application.ShopName,
                Description = application.Description,
                PhoneNumber = application.PhoneNumber,
                Address = application.Address,
                OwnerId = application.ApplicantId,
                IsActive = true,
                CreatedBy = "System",
                ModifiedBy = "System"
            };

            await _uow.ShopRepository.AddAsync(shop);
            await _uow.SaveChangesAsync();

            // Notify user
            await _notificationService.CreateNotificationAsync(application.ApplicantId, "Chúc mừng", "Hồ sơ mở shop của bạn đã được duyệt!", "success");

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> RejectApplicationAsync(int applicationId, int reviewerId, string reason)
    {
        try
        {
            var application = await _uow.ShopApplicationRepository.GetByIdAsync(applicationId);
            if (application == null || application.Status != ShopApplicationStatus.Pending)
                return false;

            application.Status = ShopApplicationStatus.Rejected;
            application.AdminNote = reason;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedBy = reviewerId;
            application.ModifiedAt = DateTime.UtcNow;
            _uow.ShopApplicationRepository.Update(application);
            await _uow.SaveChangesAsync();

            // Notify user
            await _notificationService.CreateNotificationAsync(application.ApplicantId, "Rất tiếc", $"Hồ sơ mở shop của bạn đã bị từ chối. Lý do: {reason}", "warning");
            
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> HasPendingApplicationAsync(int applicantId)
    {
        try
        {
            var apps = await _uow.ShopApplicationRepository.GetByApplicantIdAsync(applicantId);
            return apps.Any(a => a.Status == ShopApplicationStatus.Pending);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
