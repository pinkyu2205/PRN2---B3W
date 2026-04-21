using Application;
using Application.IRepository;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        public readonly AppDbContext _context;

        public readonly IAccountRepository _accountRepository;
        public readonly ICartItemRepository _cartItemRepository;
        public readonly ICartRepository _cartRepository;
        public readonly IInvoiceDetailRepository _invoiceDetailRepository;
        public readonly IInvoiceRepository _invoiceRepository;
        public readonly IOrderDetailRepository _orderDetailRepository;
        public readonly IOrderRepository _orderRepository;
        public readonly IPaymentMethodRepository _paymentMethodRepository;
        public readonly IPaymentRepository _paymentRepository;
        public readonly IProductRepository _productRepository;
        public readonly ICategoryRepository _categoryRepository;
        public readonly IProductDetailRepository _productDetailRepository;

        public IAccountRepository AccountRepository => _accountRepository;
        public ICartItemRepository CartItemRepository => _cartItemRepository;
        public ICartRepository CartRepository => _cartRepository;
        public IInvoiceDetailRepository InvoiceDetailRepository => _invoiceDetailRepository;
        public IInvoiceRepository InvoiceRepository => _invoiceRepository;
        public IOrderDetailRepository OrderDetailRepository => _orderDetailRepository;
        public IOrderRepository OrderRepository => _orderRepository;
        public IPaymentMethodRepository PaymentMethodRepository => _paymentMethodRepository;
        public IPaymentRepository PaymentRepository => _paymentRepository;
        public IProductRepository ProductRepository => _productRepository;
        public ICategoryRepository CategoryRepository => _categoryRepository;
        public IProductDetailRepository ProductDetailRepository => _productDetailRepository;
        public UnitOfWork(AppDbContext context,
            IAccountRepository accountRepository,
            ICartItemRepository cartItemRepository,
            ICartRepository cartRepository,
            IInvoiceDetailRepository invoiceDetailRepository,
            IInvoiceRepository invoiceRepository,
            IOrderDetailRepository orderDetailRepository,
            IOrderRepository orderRepository,
            IPaymentMethodRepository paymentMethodRepository,
            IPaymentRepository paymentRepository,
            IProductRepository productRepository,
            ICategoryRepository categoryRepository,
            IProductDetailRepository productDetailRepository)
        {
            _context = context;
            _accountRepository = accountRepository;
            _cartItemRepository = cartItemRepository;
            _cartRepository = cartRepository;
            _invoiceDetailRepository = invoiceDetailRepository;
            _invoiceRepository = invoiceRepository;
            _orderDetailRepository = orderDetailRepository;
            _orderRepository = orderRepository;
            _paymentMethodRepository = paymentMethodRepository;
            _paymentRepository = paymentRepository;
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _productDetailRepository = productDetailRepository;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public IDbContextTransaction BeginTransaction()
        {
            return _context.Database.BeginTransaction();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
    }
}
