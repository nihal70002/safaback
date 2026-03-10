using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.Services;

namespace PrivateECommerce.API.Controllers
{
    [ApiController]
    [Route("api/whatsapp")]
    public class WhatsAppController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public WhatsAppController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveMessage()
        {
            var form = Request.Form;

            var message = form["Body"].ToString();
            var from = form["From"].ToString();

            Console.WriteLine($"WhatsApp message received: {message}");
            Console.WriteLine($"From: {from}");

            // Example message: APPROVE-12
            if (message.StartsWith("APPROVE-"))
            {
                var orderId = int.Parse(message.Split("-")[1]);

                // here we approve order
                await _orderService.ApproveBySales(orderId, 0);

                return Ok("Order approved");
            }

            return Ok();
        }
    }
}