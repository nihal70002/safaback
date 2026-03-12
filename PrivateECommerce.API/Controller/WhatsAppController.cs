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
        [Consumes("application/x-www-form-urlencoded")]
        public IActionResult ReceiveMessage([FromForm] string Body, [FromForm] string From)
        {
            Console.WriteLine("🔥 WHATSAPP WEBHOOK HIT");

            if (string.IsNullOrEmpty(Body))
            {
                return Content("<Response></Response>", "text/xml");
            }

            var message = Body.Trim().ToUpper();
            var sender = From.Replace("whatsapp:", "");

            Console.WriteLine($"📩 WhatsApp message received: {message}");
            Console.WriteLine($"📱 From: {sender}");

            // Run processing in background so Twilio doesn't timeout
            _ = Task.Run(async () =>
            {
                try
                {
                    if (!message.Contains("-"))
                    {
                        await _orderService.SendWhatsapp(sender,
                            "❌ Invalid format. Use APPROVE-ID or REJECT-ID");
                        return;
                    }

                    var parts = message.Split("-");
                    if (parts.Length != 2) return;

                    var command = parts[0].Trim();
                    if (!int.TryParse(parts[1].Trim(), out int orderId)) return;

                    if (command == "ACCEPT" || command == "APPROVE")
                    {
                        await _orderService.ApproveBySales(orderId, 0);

                        Console.WriteLine($"✅ Order {orderId} approved via WhatsApp");

                        await _orderService.SendWhatsapp(
                            sender,
                            $"✅ Order #{orderId} has been approved successfully."
                        );
                    }
                    else if (command == "REJECT")
                    {
                        _orderService.RejectBySales(orderId, 0);

                        Console.WriteLine($"❌ Order {orderId} rejected via WhatsApp");

                        await _orderService.SendWhatsapp(
                            sender,
                            $"❌ Order #{orderId} has been rejected."
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠ Webhook error: {ex.Message}");
                }
            });

            // Respond immediately to Twilio
            return Content("<Response></Response>", "text/xml");
        }
    }
}