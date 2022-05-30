using System.Collections.Generic;
using m_sort_server.BsonModels;
using m_sort_server.DataModels;
using m_sort_server.EditModels;
using m_sort_server.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace m_sort_server.Controller
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
    }
}