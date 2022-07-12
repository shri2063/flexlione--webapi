using System;
using System.IO;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using static System.IO.Path;

namespace flexli_erp_webapi.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class FileAttachmentController : ControllerBase
    {
        // // POST
        // [HttpPost, DisableRequestSizeLimit]
        // public IActionResult Upload()
        // {
        //     try
        //     {
        //         var file = Request.Form.Files[0];
        //         Directory.CreateDirectory(Combine("C:", "Resources", "Images"));
        //         var folderName = Combine("Resources", "Images");
        //         // var pathToSave = Combine("D:", folderName);
        //          var pathToSave = Combine("C:", folderName);
        //         if (file.Length > 0)
        //         {
        //             var fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
        //             var fullPath = Combine(pathToSave, fileName);
        //             var dbPath = Combine(folderName, fileName);
        //             using (var stream = new FileStream(fullPath, FileMode.Create))
        //             {
        //                 file.CopyTo(stream);
        //             }
        //             return Ok(new { dbPath });
        //         }
        //         else
        //         {
        //             return BadRequest();
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         return StatusCode(500, $"Internal server error: {ex}");
        //     }
        // }
    }
}