using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class PaymentMethodService : IPaymentMethodService
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<PaymentMethodService>? _logger;

        public PaymentMethodService(IUnitOfWork uow, ILogger<PaymentMethodService>? logger = null)
        {
            _uow = uow;
            _logger = logger;
        }

        public async Task<List<PaymentMethod>> GetAllAsync(CancellationToken ct = default)
        {
            return await _uow.PaymentMethodRepository.GetAllQueryable()
                .AsNoTracking()
                .OrderBy(pm => pm.Name)
                .ToListAsync(ct);
        }

        public async Task<List<PaymentMethod>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _uow.PaymentMethodRepository.GetAllQueryable()
                .AsNoTracking()
                .Where(pm => pm.IsActive)
                .OrderBy(pm => pm.Name)
                .ToListAsync(ct);
        }

        public async Task<bool> SetStatusAsync(int id, bool isActive, string modifiedBy, CancellationToken ct = default)
        {
            try
            {
                _logger?.LogInformation("=== SetStatusAsync START ===");
                _logger?.LogInformation("Input: id={Id}, isActive={IsActive}, modifiedBy={ModifiedBy}", 
                    id, isActive, modifiedBy);

                // Get entity with tracking enabled
                var method = await _uow.PaymentMethodRepository.GetByIdAsync(id);

                if (method == null)
                {
                    _logger?.LogWarning("? Payment method {Id} not found or is deleted", id);
                    return false;
                }

                _logger?.LogInformation("? Found method: Id={Id}, Name={Name}, Current IsActive={CurrentIsActive}", 
                    method.Id, method.Name, method.IsActive);

                // Update properties
                method.IsActive = isActive;
                method.ModifiedBy = string.IsNullOrWhiteSpace(modifiedBy) ? "system" : modifiedBy.Trim();
                method.ModifiedAt = DateTime.UtcNow;

                _logger?.LogInformation("? Updated properties: IsActive={NewIsActive}, ModifiedBy={ModifiedBy}, ModifiedAt={ModifiedAt}",
                    method.IsActive, method.ModifiedBy, method.ModifiedAt);

                // Update and save
                _uow.PaymentMethodRepository.Update(method);
                
                _logger?.LogInformation("Calling SaveChangesAsync...");
                var result = await _uow.SaveChangesAsync();
                
                _logger?.LogInformation("SaveChangesAsync returned: {Result}", result);
                
                var success = result > 0;
                
                if (success)
                {
                    _logger?.LogInformation("? Successfully saved changes for payment method {Id}", id);
                }
                else
                {
                    _logger?.LogWarning("?? SaveChangesAsync returned 0 - no changes saved for payment method {Id}", id);
                }
                
                return success;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "? Exception in SetStatusAsync for id={Id}", id);
                throw;
            }
        }
    }
}
