namespace PrivateECommerce.API.Enum;
public enum OrderStatus
{
    // SALES
    PendingSalesApproval,
    ApprovedBySales,
    RejectedBySales,

    // WAREHOUSE
    PendingWarehouseApproval,
    ApprovedByWarehouse,
    RejectedByWarehouse,

    // ADMIN (OPTIONAL / FUTURE)
    PendingAdminApproval,
    Confirmed,

    // FULFILLMENT
    Dispatched,
    Delivered,
    Cancelled
}
