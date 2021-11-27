namespace m_sort_server.Interfaces
{
    public interface ILogFileService
    {
        dynamic ReadFromJsonFile<T>(string name);

        void WriteIntoJsonFile<T>(T data, string name);

        void DeleteJsonFile(string name);

    }
}