using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrivateECommerce.API.DTOs;

[ApiController]
[Route("api/upload")]
public class UploadController : ControllerBase
{
    private readonly ICloudinaryService _cloudinary;

    public UploadController(ICloudinaryService cloudinary)
    {
        _cloudinary = cloudinary;
    }

    [HttpPost("image")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null)
            return BadRequest(ApiResponse.Fail("No file selected"));

        var url = await _cloudinary.UploadImageAsync(file);
        return Ok(ApiResponse.Ok("Image uploaded successfully", new { imageUrl = url }));
    }
}
