using Application.IRepository;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Repository
{
    public class NotificationRepository : GenericRepository<Notification>, INotificationRepository
    {
        public NotificationRepository(AppDbContext context) : base(context)
        {
        }
    }
}
