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
        public async Task<IActionResult> ReceiveMessage([FromForm] string Body, [FromForm] string From)
        {
            Console.WriteLine("🔥 WHATSAPP WEBHOOK HIT");

            try
            {
                if (string.IsNullOrWhiteSpace(Body))
                {
                    Console.WriteLine("⚠ Empty WhatsApp message received");
                    return Content("<Response></Response>", "text/xml");
                }

                var message = Body.Trim().ToUpper();
                var sender = From?.Replace("whatsapp:", "");

                Console.WriteLine($"📩 Message: {message}");
                Console.WriteLine($"📱 Sender: {sender}");

                if (!message.Contains("-"))
                {
                    await _orderService.SendWhatsapp(
                        sender,
                        "❌ Invalid format. Use APPROVE-ID or REJECT-ID"
                    );

                    return Content("<Response></Response>", "text/xml");
                }

                var parts = message.Split('-', 3);

                var command = parts[0].Trim();

                if (!int.TryParse(parts[1].Trim(), out int orderId))
                {
                    Console.WriteLine("⚠ Invalid order id");

                    await _orderService.SendWhatsapp(
                        sender,
                        "❌ Invalid Order ID."
                    );

                    return Content("<Response></Response>", "text/xml");
                }

                Console.WriteLine($"📦 Parsed OrderId: {orderId}");
                Console.WriteLine($"📌 Command: {command}");

                if (command == "APPROVE" || command == "ACCEPT")
                {
                    Console.WriteLine("🚀 Calling ApproveBySales service...");

                    await _orderService.ApproveBySales(orderId, 0);

                    Console.WriteLine($"✅ Order {orderId} approved in DB");

                    await _orderService.SendWhatsapp(
                        sender,
                        $"✅ Order #{orderId} has been approved successfully."
                    );
                }
                else if (command == "REJECT")
                {
                    Console.WriteLine("🚀 Calling RejectBySales service...");

                    var reason = parts.Length > 2 ? parts[2].Trim() : "";

                    if (string.IsNullOrWhiteSpace(reason))
                    {
                        await _orderService.SendWhatsapp(
                            sender,
                            $"⚠ Please provide a reason.\nExample:\nREJECT-{orderId}-Out of stock"
                        );

                        return Content("<Response></Response>", "text/xml");
                    }

                    await _orderService.RejectBySales(orderId, 0, reason);

                    Console.WriteLine($"❌ Order {orderId} rejected in DB");

                    await _orderService.SendWhatsapp(
                        sender,
                        $"❌ Order #{orderId} has been rejected.\nReason: {reason}"
                    );
                }
                else
                {
                    Console.WriteLine("⚠ Unknown command received");

                    await _orderService.SendWhatsapp(
                        sender,
                        "❌ Unknown command. Use APPROVE-ID or REJECT-ID"
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"🔥 WEBHOOK ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }

            return Content("<Response></Response>", "text/xml");
        }
    }
}