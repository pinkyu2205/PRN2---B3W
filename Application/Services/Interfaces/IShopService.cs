using Domain.Entities;

namespace Application.Services.Interfaces;

public interface IShopService
{
    Task<Shop?> GetShopByOwnerIdAsync(int ownerId);
    Task<Shop?> GetShopByIdAsync(int id);
    Task<bool> CreateShopAsync(Shop shop);
    Task<bool> UpdateShopAsync(Shop shop);
    Task<bool> HasShopAsync(int ownerId);
}
