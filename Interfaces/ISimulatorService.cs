namespace m_sort_server.Interfaces
{
    public interface ISimulatorService
    {
        void RunSimulation(int bots, decimal timeStep, 
            decimal acceleration, decimal nodeDistance);
    }
}