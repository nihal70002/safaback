namespace PrivateECommerce.API.Enum
{
    public enum OrderStatus
    {
        // SALES SIDE
        PendingSalesApproval,
        ApprovedBySales,
        RejectedBySales,

        // ADMIN SIDE
        PendingAdminApproval,
        Confirmed,
        Dispatched,
        Delivered,
        Cancelled,
        
    }
}
