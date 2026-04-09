using PrivateECommerce.API.DTOs.Admin;

public interface IAdminSalesExecutiveService
{
    // existing – KEEP
    List<CustomerOrderDto> GetCustomersWithOrders(int salesExecutiveId);

    // new – ADD
    SalesExecutivePerformanceDto GetSalesExecutivePerformance(int salesExecutiveId);
    List<SalesExecutiveOrderDetailDto> GetOrdersByStatus(int salesExecutiveId, string type);
    List<CustomerBasicDto> GetCustomersForSalesExecutive(int salesExecutiveId);

}
