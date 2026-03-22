using PrivateECommerce.API.Enum;

namespace PrivateECommerce.API.Helpers
{
    public static class OrderStatusHelper
    {
        public static string GetCustomerStatus(string internalStatus)
        {
            return internalStatus switch
            {
                // SALES
                nameof(OrderStatus.PendingSalesApproval) => "Pending",
                nameof(OrderStatus.ApprovedBySales) => "Pending",
                nameof(OrderStatus.RejectedBySales) => "Cancelled",

                // WAREHOUSE
                nameof(OrderStatus.PendingWarehouseApproval) => "Pending",
                nameof(OrderStatus.ApprovedByWarehouse) => "Pending",
                nameof(OrderStatus.RejectedByWarehouse) => "Cancelled",

                // ADMIN
                nameof(OrderStatus.PendingAdminApproval) => "Pending",
                nameof(OrderStatus.Confirmed) => "Confirmed",

                // FULFILLMENT
                nameof(OrderStatus.Dispatched) => "Dispatched",
                nameof(OrderStatus.Delivered) => "Delivered",
                nameof(OrderStatus.Cancelled) => "Cancelled",

                _ => "Pending"
            };
        }
    }
}
