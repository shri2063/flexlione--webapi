using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.DataModels;
using Microsoft.AspNetCore.Mvc;
using flexli_erp_webapi.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;


namespace flexli_erp_webapi.Controller
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]

    public class RegisterTimeStampController : ControllerBase
    {
        [HttpGet("GetStamp")]
        [Consumes("application/json")]

        public RegisterTimeStamp GetStamp([FromQuery] string stampId)
        {
            using (var db = new ErpContext())
            {

                RegisterTimeStamp existingTimeStamp = db.RegisterTimeStamp
                    .FirstOrDefault(x => x.StampId == stampId);
                // Get Selected TasK
                RegisterTimeStamp timeStampModel = new RegisterTimeStamp()
                {
                    StampId = existingTimeStamp.StampId,
                    Stamp = existingTimeStamp.Stamp
                };

                return timeStampModel;

            }
        }

        [HttpPost("AddStamp")]
        [Consumes("application/json")]

        public RegisterTimeStamp AddStamp([FromQuery] DateTime stamp)
        {
            string id = GetNextAvailableId();
            
            using (var db = new ErpContext())
            {
                RegisterTimeStamp registerTimeStamp = new RegisterTimeStamp()
                {
                    StampId = id,
                    Stamp = stamp
                };
                db.RegisterTimeStamp.Add(registerTimeStamp);
                db.SaveChanges();
            }

            return GetStamp(id);
        }

        [HttpDelete("DeleteStamp")]
        [Consumes("application/json")]
        public void DeleteStamp(string stampId)
        {
            using (var db = new ErpContext())
            {
                RegisterTimeStamp existingTimeStamp = db.RegisterTimeStamp
                    .FirstOrDefault(x => x.StampId == stampId);

                if (existingTimeStamp != null)
                {

                    db.RegisterTimeStamp.Remove(existingTimeStamp);
                    db.SaveChanges();
                }
            }
        }
        
        
        private static string GetNextAvailableId()
        {
            using (var db = new ErpContext())
            {
                var a = db.RegisterTimeStamp
                    .Select(x => Convert.ToInt32(x.StampId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
        }

    }
}