using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.DataModels;
using m_sort_server.EditModels;

namespace m_sort_server.Services
{
    public class ProfileManagementService
    {
        public static ProfileEditModel GetProfileById(string profileId, string include = null)
        {
          ProfileEditModel profileEditModel =  GetProfileById(profileId);

          if (include == "sprint")
          {
              profileEditModel.sprints = SprintManagementService.GetSprintsByProfileId(profileId);
          }

          return profileEditModel;

        }
        
        public static List<ProfileEditModel> GetAllProfiles()
        {
         List<ProfileEditModel> profiles = new List<ProfileEditModel>();
            GetAllProfileIds()
               .ForEach(x => 
                   profiles.Add(GetProfileById(x)));

            return profiles;
        }

        public static List<string> GetAllProfileIds()
        {
            using (var db = new ErpContext())
            {
               return db.Profile
                    .Select(x => x.ProfileId)
                    .ToList();
            }
        }

        public static ProfileEditModel AddOrUpdateProfile(ProfileEditModel profileEditModel)
        {
            return AddOrUpdateProfileInDb(profileEditModel);

        }
        
        public static ProfileEditModel GetProfileById(string profileId)
        {
            using (var db = new ErpContext())
            {
                
                Profile existingProfile = db.Profile
                    .FirstOrDefault(x => x.ProfileId == profileId);
                
                // Case: TaskDetail does not exist
                if (existingProfile == null)
                    return null;
                
                // Case: In case you have to update data received from db

                ProfileEditModel profileEditModel = new ProfileEditModel()
                {
                   ProfileId = existingProfile.ProfileId,
                   Name = existingProfile.Name,
                   Type = existingProfile.Type
                };

                return profileEditModel;
            }

        }
        
        private static ProfileEditModel AddOrUpdateProfileInDb(ProfileEditModel profileEditModel)
        {
            Profile profile;
            
            using (var db = new ErpContext())
            {
                profile = db.Profile
                    .FirstOrDefault(x => x.ProfileId == profileEditModel.ProfileId);


                if (profile != null) // update
                {
                    
                    profile.Name = profileEditModel.Name.ToLower();
                    profile.Type = profileEditModel.Type;
                    db.SaveChanges();
                }
                else
                {
                    profile = new Profile()
                    {
                        ProfileId = GetNextAvailableId(),
                        Name = profileEditModel.Name.ToLower(),
                        Type = profileEditModel.Type
                    };
                    db.Profile.Add(profile);
                    db.SaveChanges();
                }
            }

            return GetProfileById(profile.ProfileId);
        }

        private static string GetNextAvailableId()
        {
            using (var db = new ErpContext())
            {
                var a = db.Profile
                    .Select(x => Convert.ToInt32(x.ProfileId))
                    .DefaultIfEmpty(0)
                    .Max();
                return Convert.ToString(a + 1);
            }
          
        }
        
        public static void DeleteProfile(string profileId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Profile
                Profile existingProfile = db.Profile
                    .FirstOrDefault(x => x.ProfileId == profileId);
                
                if (existingProfile != null)
                {
                    
                    db.Profile.Remove(existingProfile);
                    db.SaveChanges();
                }


            }
        }
    }
}