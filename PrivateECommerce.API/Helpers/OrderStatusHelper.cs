using PrivateECommerce.API.Enum;

namespace PrivateECommerce.API.Helpers
{
    public static class OrderStatusHelper
    {
        public static string GetCustomerStatus(string internalStatus)
        {
            return internalStatus switch
            {
                // SALES SIDE
                nameof(OrderStatus.PendingSalesApproval) => "Pending",
                nameof(OrderStatus.ApprovedBySales) => "Pending",
                nameof(OrderStatus.RejectedBySales) => "Cancelled",

                // ADMIN SIDE
                nameof(OrderStatus.PendingAdminApproval) => "Pending",
                nameof(OrderStatus.Confirmed) => "Confirmed",
                nameof(OrderStatus.Dispatched) => "Dispatched",
                nameof(OrderStatus.Delivered) => "Delivered",
                nameof(OrderStatus.Cancelled) => "Cancelled",

                _ => "Pending"
            };
        }
    }
}
