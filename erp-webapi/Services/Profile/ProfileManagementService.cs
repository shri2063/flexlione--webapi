using System;
using System.Collections.Generic;
using System.Linq;
using flexli_erp_webapi;
using flexli_erp_webapi.DataModels;
using flexli_erp_webapi.EditModels;
using flexli_erp_webapi.Repository.Interfaces;
using flexli_erp_webapi.Services.Interfaces;
using m;

namespace flexli_erp_webapi.Services
{
    public class ProfileManagementService

    {
        private readonly ISprintRelationRepository _sprintRelationRepository;
        private readonly ISprintRepository _sprintRepository;
        private readonly IProfileRepository _profileRepository;
        public ProfileManagementService(ISprintRelationRepository sprintRelationRepository, ISprintRepository sprintRepository, IProfileRepository profileRepository)
        {
            _sprintRelationRepository = sprintRelationRepository;
            _sprintRepository = sprintRepository;
            _profileRepository = profileRepository;
        }
        public  ProfileEditModel GetProfileById(string profileId, string include = null)
        {
            ProfileEditModel profileEditModel = _profileRepository.GetProfileById(profileId);

            // [check]: Profile exists

            if (profileEditModel == null)
            {
                return null;
            }

            if (include == "sprint")
            {
                profileEditModel.Sprints = _sprintRelationRepository.GetSprintsForProfileId(profileId);
            }

            profileEditModel.Managers = GetAllManagersForUser(profileId);
            return profileEditModel;

        }

        private  List<ProfileManagerEditModel> GetAllManagersForUser(string profileId)
        {
            List<ProfileManagerEditModel> managers = new List<ProfileManagerEditModel>();
            List<string> managerIds;
            managerIds = _profileRepository.GetManagerIds(profileId);

            managerIds.ForEach(managerId =>
            {
                 
                ProfileManagerEditModel profileManagerEditModel = new ProfileManagerEditModel()
                {
                    UserId = profileId,
                    ManagerId = managerId
                };
                managers.Add(profileManagerEditModel);
            });

            return managers;
        }

        public  List<ProfileEditModel> GetAllProfiles()
        {
            

            List<ProfileEditModel> profiles = new List<ProfileEditModel>();

            _profileRepository.GetAllProfileIds()
                .ForEach(x =>
                    profiles.Add(_profileRepository.GetProfileById(x)));

                return profiles;
        }

        public  ProfileEditModel AddOrUpdateProfile(ProfileEditModel profileEditModel)
        {
            return AddOrUpdateProfileInDb(profileEditModel);

        }
        
        
        public  ProfileEditModel AuthenticateProfile(string emailId, string password)
        {
            ProfileEditModel authenticatedProfile =  _profileRepository.AuthenticateProfile(emailId, password);
            
            if (authenticatedProfile == null)
            {
                throw  new Exception("Email Id or Password does not match");
            }

            return authenticatedProfile;

        }
        
        private  ProfileEditModel AddOrUpdateProfileInDb(ProfileEditModel profileEditModel)
        {

            return _profileRepository.AddOrUpdateProfile(profileEditModel);
        }

       
        public  void DeleteProfile(string profileId)
        {
            _profileRepository.DeleteProfile(profileId);
        }

        public  ProfileManagerEditModel AddManager(string userId, string managerId)
        {
            return _profileRepository.AddManager(userId, managerId);
        }
        
        public  void DeleteManager(string userId, string managerId)
        {
             _profileRepository.DeleteManager(userId, managerId);
        }

    }
}