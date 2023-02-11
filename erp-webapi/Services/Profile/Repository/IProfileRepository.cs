using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace m
{
    public interface IProfileRepository
    {
         ProfileEditModel GetProfileById(string profileId);

        
         List<string> GetAllProfileIds();

         void DeleteProfile(string profileId);

         List<string> GetManagerIds(string userId);

         ProfileEditModel AuthenticateProfile(string emailId, string password);

         ProfileEditModel AddOrUpdateProfile(ProfileEditModel profileEditModel);

         ProfileManagerEditModel AddManager(string userId, string managerId);

         void DeleteManager(string userId, string managerId);
    }
}