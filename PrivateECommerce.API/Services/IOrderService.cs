using PrivateECommerce.API.DTOs;
using PrivateECommerce.API.DTOs.Admin;
using PrivateECommerce.API.DTOs.Sales;

namespace PrivateECommerce.API.Services
{
    public interface IOrderService
    {
        // ===========================
        // CUSTOMER
        // ===========================
        void PlaceOrder(int customerId, PlaceOrderByCustomerDto dto);
        IEnumerable<UserOrderListDto> GetOrdersForUser(int userId);
        UserOrderDetailDto GetOrderForUser(int orderId, int userId);

        // ===========================
        // SALES EXECUTIVE
        // ===========================
        CustomerOrderDto GetCustomerOrderHistory(int salesExecutiveId, int customerId);
        List<SalesOrderListDto> GetPendingOrdersForSales(int salesExecutiveId);
        List<SalesOrderListDto> GetOrdersForSalesExecutive(int salesExecutiveId);
        void ApproveBySales(int orderId, int salesId);
        void RejectBySales(int orderId, int salesId);

        // ===========================
        // ADMIN
        // ===========================
        PagedResponseDto<AdminOrderListDto> GetFilteredOrders(string? status, bool today, int page, int pageSize);
        IEnumerable<AdminOrderListDto> GetRecentOrders(int count);
        AdminOrderDetailDto GetOrderById(int orderId);

        
         Task<OrderDetailsDto?> GetMyOrderDetailsAsync(int userId, int orderId);
        

        void RevertOrderStatus(int orderId);

        void ConfirmOrder(int orderId);
        void DispatchOrder(int orderId);
        void DeliverOrder(int orderId);

        void CancelOrder(int orderId, string reason);// ADMIN cancel (final)
    }
}
