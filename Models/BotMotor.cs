namespace m_sort_server.Models
{
    public class 
        BotMotor
    {
        public string BotId { get; set; }

        public Motor Motor { get; set; }
    }
    
    
    public enum Motor 
    {
       Reverse = -1,
        Min = 0,
        Max = 1
            
    }
}