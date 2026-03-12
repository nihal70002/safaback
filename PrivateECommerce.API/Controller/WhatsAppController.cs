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

            var message = form["Body"].ToString().Trim().ToUpper();
            var from = form["From"].ToString();

            Console.WriteLine($"📩 WhatsApp message received: {message}");
            Console.WriteLine($"📱 From: {from}");

            if (!message.Contains("-"))
                return Ok();

            var parts = message.Split("-");

            if (parts.Length != 2)
                return Ok();

            if (!int.TryParse(parts[1], out int orderId))
                return Ok();

            var command = parts[0];

            try
            {
                if (command == "ACCEPT" || command == "APPROVE")
                {
                    await _orderService.ApproveBySales(orderId, 0);

                    Console.WriteLine($"✅ Order {orderId} approved via WhatsApp");

                    // confirmation message
                    await _orderService.SendWhatsapp(
                        from.Replace("whatsapp:", ""),
                        $"✅ Order #{orderId} has been approved successfully."
                    );
                }
                else if (command == "REJECT")
                {
                    _orderService.RejectBySales(orderId, 0);

                    Console.WriteLine($"❌ Order {orderId} rejected via WhatsApp");

                    // confirmation message
                    await _orderService.SendWhatsapp(
                        from.Replace("whatsapp:", ""),
                        $"❌ Order #{orderId} has been rejected."
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Error processing WhatsApp command: {ex.Message}");
            }

            return Ok();
        }
    }
}