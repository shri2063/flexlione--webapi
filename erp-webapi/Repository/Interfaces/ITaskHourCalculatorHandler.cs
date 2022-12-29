using System;

namespace flexli_erp_webapi.Repository.Interfaces
{
    public interface ITaskHourCalculatorHandler
    {
        decimal GetTotalEstimatedHoursForTask(String taskId);
        
        decimal GetTotalActualHoursForTask(String taskId);
    }
}