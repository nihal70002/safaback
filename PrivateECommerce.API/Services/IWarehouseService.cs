public interface IWarehouseService
{
    // DASHBOARD
    object GetTodayOrderSummary();
    IEnumerable<object> GetTodayOrders();

    // ORDER DETAILS
    object GetOrderDetails(int orderId);

    // INVENTORY
    IEnumerable<object> GetInventory();
    IEnumerable<object> GetLowStockProducts(int threshold);
    IEnumerable<object> GetLowStockAlerts();
    IEnumerable<object> GetLowStockAlerts(int threshold);
    IEnumerable<object> GetStockMovements();
    IEnumerable<object> GetAllOrders();

    // WAREHOUSE ACTIONS
    void ApproveOrder(int orderId, int approverUserId, bool isAdmin = false);

    void RejectOrder(int orderId, int warehouseUserId, string reason);
    void DispatchOrder(int orderId);   // ✅ FIXED
    void DeliverOrder(int orderId);    // ✅ FIXED


 

    // SALES
    IEnumerable<object> GetSalesExecutivesWithOrders();
    IEnumerable<object> GetOrdersBySalesExecutive(
        int salesExecutiveId,
        string? status,
        DateTime? fromDate,
        DateTime? toDate
    );

    // PENDING
    IEnumerable<object> GetPendingOrders();
}
