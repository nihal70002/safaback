using PrivateECommerce.API.DTOs;

namespace PrivateECommerce.API.Services
{
    public interface IOrderService
    {
        // ===========================
        // CUSTOMER
        // ===========================
        void PlaceOrder(int userId, PlaceOrderDto dto);

        IEnumerable<UserOrderListDto> GetOrdersForUser(int userId);
        UserOrderDetailDto GetOrderForUser(int orderId, int userId);

        // ===========================
        // ADMIN - VIEW
        // ===========================
        PagedResponseDto<AdminOrderListDto> GetFilteredOrders(
     string? status,
     bool today,
     int page,
     int pageSize
 );

        AdminOrderDetailDto GetOrderById(int orderId);

        // ===========================
        // ADMIN - ACTIONS
        // ===========================
        void ConfirmOrder(int orderId);
        void DispatchOrder(int orderId);
        void DeliverOrder(int orderId);
        void CancelOrder(int orderId);
        void RevertOrderStatus(int orderId);

        // ===========================
        // ADMIN - DASHBOARD
        // ===========================
        IEnumerable<AdminOrderListDto> GetRecentOrders(int count);
        IEnumerable<MyOrderDto> GetMyOrders(int userId);
    }
}
