using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using m;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public class ProfileRepository : IProfileRepository
    {
        public  ProfileEditModel GetProfileById(string profileId)
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
        
        public  List<string> GetAllProfileIds()
        {
            using (var db = new ErpContext())
            {
                return db.Profile
                    .Select(x => x.ProfileId)
                    .ToList();
            }
        }
        
        public  ProfileEditModel AuthenticateProfile(string emailId, string password)
        {
            using (var db = new ErpContext())
            {
                
                Profile existingProfile = db.Profile
                    .FirstOrDefault(x => x.EmailId == emailId && x.Password == password);
                
                // Case: If not exist
                if (existingProfile == null)
                {
                    return null;
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
        
        
        public  ProfileEditModel AddOrUpdateProfile(ProfileEditModel profileEditModel)
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
        
        public  void DeleteProfile(string profileId)
        {
            using (var db = new ErpContext())
            {
                // Get Selected Profile
                Profile existingProfile = db.Profile
                    .FirstOrDefault(x => x.ProfileId == profileId);
                
                if (existingProfile != null)
                {
                    // [check] all those profile manager pairs which contain profileId as any of two variable
                    List<ProfileManager> managersAndUsers = db.ProfileManager
                        .Where(x => x.UserId == profileId || x.ManagerId == profileId)
                        .ToList();

                    // [check] if managers and users not null then deleted them from profile-manager table
                    if (managersAndUsers != null)
                    {
                        managersAndUsers.ForEach(s =>
                        {
                            db.ProfileManager.Remove(s);
                            db.SaveChanges();
                        });
                    }
                    
                    db.Profile.Remove(existingProfile);
                    db.SaveChanges();
                }


            }
        }

        public  List<string> GetManagerIds(string userId)
        {
            using (var db = new ErpContext())
            {
                List<string> managers = db.ProfileManager
                    .Where(x => x.UserId == userId)
                    .Select(x => x.ManagerId)
                    .ToList();
                

                return managers;
            }
        }
        
        public  ProfileManagerEditModel AddManager(string userId, string managerId)
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
        
        private  ProfileManagerEditModel GetManagerForUser(string userId, string managerId)
        {
            ProfileManagerEditModel profileManagerEditModel = new ProfileManagerEditModel()
            {
                UserId = userId,
                ManagerId = managerId
            };

            return profileManagerEditModel;
        }


        public void DeleteManager(string userId, string managerId)
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
    }
}