using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Admin;
using PrivateECommerce.API.DTOs.Sales;
using PrivateECommerce.API.Models;
namespace PrivateECommerce.API.Services
{
    public interface IOrderService
    {
        // ===========================
        // CUSTOMER
        // ===========================
        Task PlaceOrder(int customerId, PlaceOrderByCustomerDto dto);
        IEnumerable<UserOrderListDto> GetOrdersForUser(int userId);
        UserOrderDetailDto GetOrderForUser(int orderId, int userId);

        // ===========================
        // SALES EXECUTIVE
        // ===========================
        CustomerOrderDto GetCustomerOrderHistory(int salesExecutiveId, int customerId);
        List<SalesOrderListDto> GetPendingOrdersForSales(int salesExecutiveId);
        List<SalesOrderListDto> GetOrdersForSalesExecutive(int salesExecutiveId);
        Task<Order> ApproveBySales(int orderId, int approverUserId, bool isAdmin = false);

        Task RejectBySales(int orderId, int salesId, string reason);

        // ===========================
        // ADMIN
        // ===========================
        PagedResponseDto<AdminOrderListDto> GetFilteredOrders(string? status, bool today, int page, int pageSize);
        IEnumerable<AdminOrderListDto> GetRecentOrders(int count);
        AdminOrderDetailDto GetOrderById(int orderId);
        IEnumerable<object> GetPendingOrdersForWarehouse();

        Task SendWhatsapp(string phone, string message);

        Task<OrderDetailsDto?> GetMyOrderDetailsAsync(int userId, int orderId);


        void RevertOrderStatus(int orderId, bool isConfirmed);


        void ConfirmOrder(int orderId);
        void DispatchOrder(int orderId);
        void DeliverOrder(int orderId);

        void CancelOrder(int orderId, string reason);// ADMIN cancel (final)
    }
}
