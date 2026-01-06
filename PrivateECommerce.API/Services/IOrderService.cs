using PrivateECommerce.API.DTOs;

namespace PrivateECommerce.API.Services
{
    public interface IOrderService
    {
        // CUSTOMER
        void PlaceOrder(int userId, PlaceOrderDto dto);

        // ADMIN - VIEW
        IEnumerable<AdminOrderListDto> GetAllOrders();
        AdminOrderDetailDto GetOrderById(int orderId);

        // ADMIN - BUTTON ACTIONS
        void ConfirmOrder(int orderId);
        void DispatchOrder(int orderId);
        void DeliverOrder(int orderId);
        void CancelOrder(int orderId);
        IEnumerable<AdminOrderListDto> GetRecentOrders(int count);
        IEnumerable<AdminOrderListDto> GetFilteredOrders(string? status, bool today);

        // NEW – USER
        IEnumerable<UserOrderListDto> GetOrdersForUser(int userId);
        UserOrderDetailDto GetOrderForUser(int orderId, int userId);
    }
}
