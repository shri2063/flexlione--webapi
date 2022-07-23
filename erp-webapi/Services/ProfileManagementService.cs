﻿using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Services
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
                        ProfileId = GetNextAvailableId("profile"),
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

        private static string GetNextAvailableId(string type)
        {
            int a = 0;
            using (var db = new ErpContext())
            {
                if (type == "profile")
                {
                    a = db.Profile
                        .Select(x => Convert.ToInt32(x.ProfileId))
                        .DefaultIfEmpty(0)
                        .Max();
                }
                
                // next id for profile
                else if (type == "manager")
                {
                    a = db.ProfileManager
                        .Select(x => Convert.ToInt32(x.Id))
                        .DefaultIfEmpty(0)
                        .Max();
                }
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

        public static ProfileManagerEditModel AddManager(string userId, string managerId)
        {
            // add manager
            ProfileManager profileManager;

            using (var db = new ErpContext())
            {
                // check if combination doesn't exists then add
                if (!db.ProfileManager.Any(x => x.UserId == userId && x.ManagerId == managerId))
                {
                    profileManager = new ProfileManager()
                    {
                        Id = GetNextAvailableId("manager"),
                        UserId = userId,
                        ManagerId = managerId
                    };

                    db.ProfileManager.Add(profileManager);
                    db.SaveChanges();
                }
            }
            return GetManagerForUser(userId, managerId);
        }
        
        public static void DeleteManager(string userId, string managerId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Manager
                ProfileManager existingProfileManager = db.ProfileManager
                    .FirstOrDefault(x => x.UserId == userId && x.ManagerId==managerId);
                
                if (existingProfileManager != null)
                {
                    
                    db.ProfileManager.Remove(existingProfileManager);
                    db.SaveChanges();
                }

            }
        }

        private static ProfileManagerEditModel GetManagerForUser(string userId, string managerId)
        {
            ProfileManagerEditModel profileManagerEditModel = new ProfileManagerEditModel()
            {
                UserId = userId,
                ManagerId = managerId
            };

            return profileManagerEditModel;
        }
        
        public static bool FindValidManager(string userId, string managerId)
        {
            using (var db = new ErpContext())
            {
                List<string> managers = db.ProfileManager
                    .Where(x => x.UserId == userId)
                    .Select(x => x.ManagerId)
                    .ToList();

                if (managers.Contains(managerId))
                    return true;

                return false;
            }
        }
    }
}