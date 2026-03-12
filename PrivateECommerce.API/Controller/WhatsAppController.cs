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
        [Consumes("application/x-www-form-urlencoded")] // Explicitly tell ASP.NET to expect Form data
        public async Task<IActionResult> ReceiveMessage([FromForm] string Body, [FromForm] string From)
        {
            Console.WriteLine("🔥 WHATSAPP WEBHOOK HIT");

            if (string.IsNullOrEmpty(Body))
            {
                return Ok(); // Twilio needs a 200 OK even if body is empty
            }

            var message = Body.Trim().ToUpper();
            Console.WriteLine($"📩 WhatsApp message received: {message}");
            Console.WriteLine($"📱 From: {From}");

            try
            {
                // Logic: Expecting "COMMAND-ID" (e.g., APPROVE-101)
                if (!message.Contains("-"))
                {
                    return Content("<Response><Message>Invalid format. Use APPROVE-ID or REJECT-ID</Message></Response>", "text/xml");
                }

                var parts = message.Split("-");
                if (parts.Length != 2) return Ok();

                var command = parts[0].Trim();
                if (!int.TryParse(parts[1].Trim(), out int orderId)) return Ok();

                string cleanNumber = From.Replace("whatsapp:", "");

                if (command == "ACCEPT" || command == "APPROVE")
                {
                    // Assuming 0 is the UserId for system/automated changes
                    await _orderService.ApproveBySales(orderId, 0);

                    Console.WriteLine($"✅ Order {orderId} approved via WhatsApp");

                    await _orderService.SendWhatsapp(
                        cleanNumber,
                        $"✅ Order #{orderId} has been approved successfully."
                    );
                }
                else if (command == "REJECT")
                {
                    await _orderService.RejectBySales(orderId, 0);

                    Console.WriteLine($"❌ Order {orderId} rejected via WhatsApp");

                    await _orderService.SendWhatsapp(
                        cleanNumber,
                        $"❌ Order #{orderId} has been rejected."
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠ Error processing WhatsApp command: {ex.Message}");
            }

            // Always return TwiML (empty is fine) to acknowledge receipt to Twilio
            return Content("<Response></Response>", "text/xml");
        }
    }
}