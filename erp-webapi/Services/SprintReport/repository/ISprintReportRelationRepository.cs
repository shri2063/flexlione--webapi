namespace m_sort_server.Repository.Interfaces
{
    public interface ISprintReportRelationRepository
    {
        string GetSprintReportLineItemIdForCheckListId(string checkListItemId);
    }
}