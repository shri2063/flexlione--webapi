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
          ProfileEditModel profileEditModel =  GetProfileByIdFromDb(profileId);

          if (include == "sprint")
          {
              profileEditModel.Sprints = SprintManagementService.GetSprintsByProfileId(profileId);
          }

          return profileEditModel;

        }
        
        public static List<ProfileEditModel> GetAllProfiles()
        {
         List<ProfileEditModel> profiles = new List<ProfileEditModel>();
            GetAllProfileIds()
               .ForEach(x => 
                   profiles.Add(GetProfileByIdFromDb(x)));

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
        
        public static ProfileEditModel GetProfileByIdFromDb(string profileId)
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
                   Type = existingProfile.Type,
                   EmailId = existingProfile.EmailId,
                   Password = existingProfile.Password
                };

                return profileEditModel;
            }

        }
        
        
        public static ProfileEditModel AuthenticateProfile(string emailId, string password)
        {
            using (var db = new ErpContext())
            {
                
                Profile existingProfile = db.Profile
                    .FirstOrDefault(x => x.EmailId == emailId && x.Password == password);
                
                // Case: Incorrect username or password
                if (existingProfile == null)
                {
                    throw  new Exception("Email Id or Password does not match");
                }


                ProfileEditModel profileEditModel = new ProfileEditModel()
                {
                    ProfileId = existingProfile.ProfileId,
                    Name = existingProfile.Name,
                    Type = existingProfile.Type,
                    EmailId = existingProfile.EmailId,
                    Password = existingProfile.Password
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
                    profile.EmailId = profileEditModel.EmailId;
                    profile.Password = profileEditModel.Password;
                    db.SaveChanges();
                }
                else
                {
                    profile = new Profile()
                    {
                        ProfileId = GetNextAvailableId(),
                        Name = profileEditModel.Name.ToLower(),
                        Type = profileEditModel.Type,
                        EmailId = profileEditModel.EmailId,
                        Password = profileEditModel.Password
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