using FluentAssertions;
using PrivateECommerce.API.Enum;

namespace PrivateECommerce.API.Tests
{
    public class WorkflowGuardTests
    {
        [Fact]
        public void OrderWorkflowContainsExpectedApprovalStates()
        {
            System.Enum.GetNames<OrderStatus>().Should().Contain(new[]
            {
                nameof(OrderStatus.PendingSalesApproval),
                nameof(OrderStatus.PendingWarehouseApproval),
                nameof(OrderStatus.Delivered)
            });
        }
    }
}
