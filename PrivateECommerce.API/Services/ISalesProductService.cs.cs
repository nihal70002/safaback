public interface ISalesProductService
{
    Task<List<SalesProductPerformanceDto>> GetProductPerformanceAsync(int salesExecutiveId);
}
