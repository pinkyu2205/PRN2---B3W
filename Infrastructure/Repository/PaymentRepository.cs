using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IRepository;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
    {
        private readonly AppDbContext _appDbContext;
        public PaymentRepository(AppDbContext dbContext) : base(dbContext)
        {
            _appDbContext = dbContext;
        }

        public async Task<decimal> GetTotalRevenueAsync(PaymentStatus successStatus)
        {
            return await _dbSet
                .Where(p => !p.IsDeleted && p.PaymentStatus == successStatus)
                .SumAsync(p => p.Amount);
        }
    }
}
