using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.IRepository;
using Application.Services.Interfaces;
using Domain.Enums;

namespace Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _uow;
        public PaymentService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _uow.PaymentRepository.GetTotalRevenueAsync(PaymentStatus.Paid);
        }
    }
}
