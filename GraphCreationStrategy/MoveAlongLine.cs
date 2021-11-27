using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class MoveAlongLine
    {
        public static void MoveForwardOnLinearEdge(string direction, Coordinate
            result,Coordinate reference, decimal l)
        {
            if (direction == "N") 
            {
                MoveInNorth(result,
                    reference, l); 
            }

            if (direction == "E")
            {
                MoveInEast(result,
                    reference, l);
            }

            if (direction == "S") 
            {
                MoveInSouth(result,
                    reference, l);
            }

            if (direction == "W")
            {
                MoveInWest(result,
                    reference, l);
            } 
        }
        
        
        public static void MoveReverseOnLinearEdge(string direction, Coordinate
            result,Coordinate reference, decimal l)
        {
            if (direction == "N") 
            {
                MoveInSouth(result,
                    reference, l); 
            }

            if (direction == "E")
            {
                MoveInWest(result,
                    reference, l);
            }

            if (direction == "S") 
            {
                MoveInNorth(result,
                    reference, l);
            }

            if (direction == "W")
            {
                MoveInEast(result,
                    reference, l);
            } 
        }

        private static void MoveInNorth(Coordinate result, Coordinate reference,
            decimal l)
        {
            result.XVertex = reference.XVertex;
            result.YVertex = reference.YVertex + l;
        }
        
        private static void MoveInEast(Coordinate result, Coordinate reference,
            decimal l)
        {
            result.XVertex = reference.XVertex + l;
            result.YVertex = reference.YVertex ;
        }
        
        private static void MoveInSouth(Coordinate result, Coordinate reference,
            decimal l)
        {
            result.XVertex = reference.XVertex;
            result.YVertex = reference.YVertex - l;
        }
        
        private static void MoveInWest(Coordinate result, Coordinate reference,
            decimal l)
        {
            result.XVertex = reference.XVertex - l;
            result.YVertex = reference.YVertex;
        }

    }       
}
