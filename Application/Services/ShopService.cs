using Application.Services.Interfaces;
using Domain.Entities;

namespace Application.Services;

public class ShopService : IShopService
{
    private readonly IUnitOfWork _uow;

    public ShopService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Shop?> GetShopByOwnerIdAsync(int ownerId)
    {
        try
        {
            return await _uow.ShopRepository.GetByOwnerIdAsync(ownerId);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<Shop?> GetShopByIdAsync(int id)
    {
        try
        {
            return await _uow.ShopRepository.GetByIdAsync(id);
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> CreateShopAsync(Shop shop)
    {
        try
        {
            await _uow.ShopRepository.AddAsync(shop);
            await _uow.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> UpdateShopAsync(Shop shop)
    {
        try
        {
            _uow.ShopRepository.Update(shop);
            await _uow.SaveChangesAsync();
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<bool> HasShopAsync(int ownerId)
    {
        try
        {
            var shop = await _uow.ShopRepository.GetByOwnerIdAsync(ownerId);
            return shop != null;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
