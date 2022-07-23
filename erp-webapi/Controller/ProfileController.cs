using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Services;
using flexli_erp_webapi.BsonModels;
using flexli_erp_webapi.DataModels;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace flexli_erp_webapi.Controller
{
    
    [Route("api/v1/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    [EnableCors("AllowOrigin")]
    [Produces("application/json")]

    public class ProfileController : ControllerBase
    {

        /// <summary>
        /// Read a Wave.Can Include 'orderList' and 'ptlStations'.
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        [HttpGet("GetProfileById")]
        [Consumes("application/json")]

        public ProfileEditModel GetProfileById(string profileId, string include = null)
        {
            return ProfileManagementService.GetProfileById(profileId,include);
        }
        
        
        [HttpGet("AuthenticateProfile")]
        [Consumes("application/json")]

        public ProfileEditModel AuthenticateProfile(string emailId, string password)
        {
            return ProfileManagementService.AuthenticateProfile(emailId,password);
        }
        
        [HttpGet("GetAllProfiles")]
        [Consumes("application/json")]

        public List<ProfileEditModel> GetAllProfiles()
        {
            return ProfileManagementService.GetAllProfiles();
        }
        
       
        [HttpPost("AddOrUpdateProfile")]
        [Consumes("application/json")]
        public ProfileEditModel AddOrUpdateProfile(ProfileEditModel profile)
        {
            return ProfileManagementService.AddOrUpdateProfile(profile);
        }
        
        [HttpDelete("DeleteSprint")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteProfile(string profileId)
        {
          ProfileManagementService.DeleteProfile(profileId);
            return Ok();
        }

        [HttpPost("AddManager")]
        [Consumes("application/json")]

        public ProfileManagerEditModel AddManager(string userId, string managerId)
        {
            return ProfileManagementService.AddManager(userId, managerId);
        }
        
        [HttpDelete("DeleteManager")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteManager(string userId, string managerId)
        {
            ProfileManagementService.DeleteManager(userId, managerId);
            return Ok();
        }
    }
}