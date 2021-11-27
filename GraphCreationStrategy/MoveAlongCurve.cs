using System;
using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class MoveAlongCurve
    {
        public static void MoveForwardOnCurveEdgeClockWise(string direction, Coordinate
            result,Coordinate reference, decimal radius, decimal l)
        {
            if (direction == "N") 
            {
                MoveInNorthClockwise(result,
                    reference, radius,l/radius); 
            }

            if (direction == "E")
            {
                MoveInEastClockwise(result,
                    reference, radius,l/radius);
            }

            if (direction == "S") 
            {
                MoveInSouthClockwise(result,
                    reference, radius,l/radius);
            }

            if (direction == "W")
            {
                MoveInWestClockwise(result,
                    reference, radius,l/radius);
            } 
        }
        
        public static void MoveForwardOnCurveEdgeAntiClockWise(string direction, Coordinate
            result,Coordinate reference, decimal radius, decimal l)
        {
            if (direction == "N") 
            {
                MoveInNorthAntiClockwise(result,
                    reference, radius,l/radius); 
            }

            if (direction == "E")
            {
                MoveInEastAntiClockwise(result,
                    reference, radius,l/radius);
            }

            if (direction == "S") 
            {
                MoveInSouthAntiClockwise(result,
                    reference, radius,l/radius);
            }

            if (direction == "W")
            {
                MoveInWestAntiClockwise(result,
                    reference, radius,l/radius);
            } 
        }

        public static void MoveReverseOnCurveEdgeClockwise(string direction, Coordinate
            result,Coordinate reference, decimal radius, decimal l)
        {
            if (direction == "N") 
            {
                MoveInSouthClockwise(result,
                    reference, radius,l/radius); 
            }

            if (direction == "E")
            {
                MoveInWestClockwise(result,
                    reference, radius,l/radius);
            }

            if (direction == "S") 
            {
                MoveInNorthClockwise(result,
                    reference, radius,l/radius);
            }

            if (direction == "W")
            {
                MoveInEastClockwise(result,
                    reference, radius,l/radius);
            } 
        }
        
        public static void MoveReverseOnCurveEdgeAntiClockwise(string direction, Coordinate
            result,Coordinate reference, decimal radius, decimal l)
        {
            if (direction == "N") 
            {
                MoveInSouthAntiClockwise(result,
                    reference, radius,l/radius); 
            }

            if (direction == "E")
            {
                MoveInWestAntiClockwise(result,
                    reference, radius,l/radius);
            }

            if (direction == "S") 
            {
                MoveInNorthAntiClockwise(result,
                    reference, radius,l/radius);
            }

            if (direction == "W")
            {
                MoveInEastAntiClockwise(result,
                    reference, radius,l/radius);
            } 
        }
        
        private static void MoveInNorthClockwise(Coordinate result, Coordinate reference,
            decimal radius, decimal theta)
        {
            result.XVertex = Math.Round(reference.XVertex 
                             + radius * (1 - Convert.ToDecimal(Math.Cos(Convert.ToDouble(theta)))),1);

            result.YVertex = Math.Round(reference.YVertex
                                        + radius * Convert.ToDecimal(Math.Sin(Convert.ToDouble(theta))),1);
        }
        
        private static void MoveInEastClockwise(Coordinate result, Coordinate reference,
            decimal radius, decimal theta)
        {
            
            result.XVertex = Math.Round(reference.XVertex
                             + radius * Convert.ToDecimal(Math.Sin(Convert.ToDouble(theta))),1);
            result.YVertex = Math.Round(reference.YVertex 
                             - radius * (1 - Convert.ToDecimal(Math.Cos(Convert.ToDouble(theta)))),1);

        }
        
        private static void MoveInSouthClockwise(Coordinate result, Coordinate reference,
            decimal radius, decimal theta)
        {
            result.XVertex = Math.Round(reference.XVertex 
                              - radius * (1 - Convert.ToDecimal(Math.Cos(Convert.ToDouble(theta)))),1);
            result.YVertex = Math.Round(reference.YVertex
                                        - radius * Convert.ToDecimal(Math.Sin(Convert.ToDouble(theta))),1);
        }
        
        private static void MoveInWestClockwise(Coordinate result, Coordinate reference,
            decimal radius, decimal theta)
        {
            result.XVertex = Math.Round(reference.XVertex
                             - radius * Convert.ToDecimal(Math.Sin(Convert.ToDouble(theta))),1);
            result.YVertex = Math.Round(reference.YVertex 
                             + radius * (1 - Convert.ToDecimal(Math.Cos(Convert.ToDouble(theta)))),1);
        }


        private static void MoveInNorthAntiClockwise(Coordinate result, Coordinate reference,
            decimal radius, decimal theta)
        {
            result.XVertex = Math.Round(reference.XVertex
                                        - radius * (1 - Convert.ToDecimal(Math.Cos(Convert.ToDouble(theta)))), 1);

            result.YVertex = Math.Round(reference.YVertex
                                        + radius * Convert.ToDecimal(Math.Sin(Convert.ToDouble(theta))), 1);
        }

        private static void MoveInEastAntiClockwise(Coordinate result, Coordinate reference,
            decimal radius, decimal theta)
        {
            
            result.XVertex = Math.Round(reference.XVertex
                             + radius * Convert.ToDecimal(Math.Sin(Convert.ToDouble(theta))), 1);
            result.YVertex = Math.Round(reference.YVertex 
                             + radius * (1 - Convert.ToDecimal(Math.Cos(Convert.ToDouble(theta)))), 1);

        }
        
        private static void MoveInSouthAntiClockwise(Coordinate result, Coordinate reference,
            decimal radius, decimal theta)
        {
            result.XVertex = Math.Round(reference.XVertex 
                             + radius * (1 - Convert.ToDecimal(Math.Cos(Convert.ToDouble(theta)))), 1);
            result.YVertex = Math.Round(reference.YVertex
                             - radius * Convert.ToDecimal(Math.Sin(Convert.ToDouble(theta))), 1);
        }
        
        private static void MoveInWestAntiClockwise(Coordinate result, Coordinate reference,
            decimal radius, decimal theta)
        {
            result.XVertex = Math.Round(reference.XVertex
                             - radius * Convert.ToDecimal(Math.Sin(Convert.ToDouble(theta))), 1);
            result.YVertex = Math.Round(reference.YVertex 
                             - radius * (1 - Convert.ToDecimal(Math.Cos(Convert.ToDouble(theta)))), 1);
        }

       
    }
}
