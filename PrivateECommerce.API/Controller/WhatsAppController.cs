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
            Console.WriteLine("🔥 WHATSAPP WEBHOOK HIT");

            var form = Request.Form;

            var message = form["Body"].ToString().Trim().ToUpper();
            var from = form["From"].ToString();

            Console.WriteLine($"📩 WhatsApp message received: {message}");
            Console.WriteLine($"📱 From: {from}");

            try
            {
                if (!message.Contains("-"))
                    return Content("<Response></Response>", "text/xml");

                var parts = message.Split("-");

                if (parts.Length != 2)
                    return Content("<Response></Response>", "text/xml");

                if (!int.TryParse(parts[1], out int orderId))
                    return Content("<Response></Response>", "text/xml");

                var command = parts[0];

                if (command == "ACCEPT" || command == "APPROVE")
                {
                    await _orderService.ApproveBySales(orderId, 0);

                    Console.WriteLine($"✅ Order {orderId} approved via WhatsApp");

                    await _orderService.SendWhatsapp(
                        from.Replace("whatsapp:", ""),
                        $"✅ Order #{orderId} has been approved successfully."
                    );
                }
                else if (command == "REJECT")
                {
                    _orderService.RejectBySales(orderId, 0);

                    Console.WriteLine($"❌ Order {orderId} rejected via WhatsApp");

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

            return Content("<Response></Response>", "text/xml");
        }
    }
}