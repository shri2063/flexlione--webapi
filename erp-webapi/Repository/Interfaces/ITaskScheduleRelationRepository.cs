using System;
using System.Collections.Generic;
using flexli_erp_webapi.EditModels;

namespace flexli_erp_webapi.Repository
{
    public interface ITaskScheduleRelationRepository
    {
        List<TaskScheduleEditModel> GetAllTaskScheduleByProfileIdAndMonth(string profileId, int month, int year,
            string include = null, int? pageIndex = null, int? pageSize = null);

        List<TaskScheduleEditModel> GetAllTaskScheduleByProfileIdAndDateRange(string profileId, DateTime fromDate,
            DateTime toDate);

    }
}