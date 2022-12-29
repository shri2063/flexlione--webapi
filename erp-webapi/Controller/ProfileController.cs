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
        
        

        private readonly ProfileManagementService _profileManagementService;

        public ProfileController(ProfileManagementService profileManagementService)
        {
            _profileManagementService = profileManagementService;
           
        }
        
        
        /// <summary>
        /// [R] Get Any profile by Id
        /// include - sprint
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetProfileById")]
        [Consumes("application/json")]

        public ProfileEditModel GetProfileById(string profileId, string include = null)
        {
            return _profileManagementService.GetProfileById(profileId,include);
        }
        
        /// <summary>
        /// [R] Authenticate Profile
        /// </summary>
        /// <returns></returns>
        [HttpGet("AuthenticateProfile")]
        [Consumes("application/json")]

        public ProfileEditModel AuthenticateProfile(string emailId, string password)
        {
            return _profileManagementService.AuthenticateProfile(emailId,password);
        }
        /// <summary>
        /// [R] Get All profiles
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetAllProfiles")]
        [Consumes("application/json")]

        public List<ProfileEditModel> GetAllProfiles()
        {
            return _profileManagementService.GetAllProfiles();
        }
        
        /// <summary>
        /// [R] Add or update profile
        /// </summary>
        /// <returns></returns>
        [HttpPost("AddOrUpdateProfile")]
        [Consumes("application/json")]
        public ProfileEditModel AddOrUpdateProfile(ProfileEditModel profile)
        {
            return _profileManagementService.AddOrUpdateProfile(profile);
        }
        
        /// <summary>
        /// [R] Delete profile
        /// </summary>
        /// <returns></returns>
        [HttpDelete("DeleteProfile")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteProfile(string profileId)
        {
          _profileManagementService.DeleteProfile(profileId);
            return Ok();
        }

        /// <summary>
        /// Add Manager. User can have multiple manager
        /// </summary>
        /// <returns></returns>
        [HttpPost("AddManager")]
        [Consumes("application/json")]

        public ProfileManagerEditModel AddManager(string userId, string managerId)
        {
            return _profileManagementService.AddManager(userId, managerId);
        }
        
        [HttpDelete("DeleteManager")]
        [Consumes("application/json")]
        
        public ActionResult<string> DeleteManager(string userId, string managerId)
        {
            _profileManagementService.DeleteManager(userId, managerId);
            return Ok();
        }
    }
}