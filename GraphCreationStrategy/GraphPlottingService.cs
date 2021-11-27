using System;
using System.Collections.Generic;
using System.Linq;
using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class GraphPlottingService
    {
        public static List<EdgeJsonModel> EdgePlottingQueue = 
            new List<EdgeJsonModel>();


        public static List<EdgeJsonModel> PlotGraph(List<EdgeJsonModel> edges,
            List<NodeJsonModel> nodes)
        
        {
            
            EdgeJsonModel edge = edges.Find(
                x => x.Origin);
            
            edge.FromCoordinate = new Coordinate();
            // Set Origin and Add Edge in Queue
            SetOrigin(edge);
            EdgePlottingQueue.Add(edge);
            
            // Plot Coordinates of all edges and nodes
            // We move serially from one node to another
            // Once we find a node we will add all edges containing that node in a Edge plotting queue
            // Since, now we can find XY coordinates of all these edges
            // Loop will be continued till Edge plotting queue is empty
            PlotCoordinatesOfEdgesAndNodes(edges,nodes);
            
            return edges;
        }

        private static void PlotCoordinatesOfEdgesAndNodes(
            List<EdgeJsonModel>edges,List<NodeJsonModel> nodes)
        {
            while (EdgePlottingQueue.Count > 0)
            {
                EdgeJsonModel selectedEdge = EdgePlottingQueue.First();
                // How do you get To Coordinate of all edges?
                // Well you have from coordinate and you know direction
                SetToCoordinateOfEdge(selectedEdge);

                for (int i = 0; i < selectedEdge.Nodes.Count; i++)
                {
                    NodeJsonModel node = nodes
                        .Find(x =>
                            x.NodeId == selectedEdge.Nodes[i]);
                    decimal length = selectedEdge.Distances[i];
                    // How do you calculate Node Coordiantes?
                    // You have edge coordinates
                    // And you have length of the node from the origin
                    SetCoordinateOfNode(node, selectedEdge, length);
                    
                    // How do you get From coordinate of all edges?
                    // If any node has adjacent edges then 
                    // Node has to be from coordinate of that edge
                    AddAdjacentEdgesToQueue(node,edges);
                    
                }

                EdgePlottingQueue.RemoveAt(0);
            }
        }

        private static void SetToCoordinateOfEdge(EdgeJsonModel edge)
        {
            edge.ToCoordinate = new Coordinate();
            if (edge.EdgeType == "L")
            {
                MoveAlongLine.MoveForwardOnLinearEdge(
                    edge.Direction,edge.ToCoordinate
                    ,edge.FromCoordinate,edge.Length);
            }
            else
            {
                if (edge.ClockWise)
                {
                    MoveAlongCurve.MoveForwardOnCurveEdgeClockWise(
                        edge.Direction,
                        edge.ToCoordinate
                        ,edge.FromCoordinate,edge.Radius
                        ,edge.Length);    
                }
                else
                {
                    MoveAlongCurve.MoveForwardOnCurveEdgeAntiClockWise(
                        edge.Direction,
                        edge.ToCoordinate
                        ,edge.FromCoordinate,edge.Radius
                        ,edge.Length);  
                }
            }
        }
        
        private static void SetCoordinateOfNode(NodeJsonModel node
            ,EdgeJsonModel edge, decimal length)
        {
            node.Coordinates = new Coordinate();
            if (edge.EdgeType == "L")
            {
                MoveAlongLine.MoveForwardOnLinearEdge(
                    edge.Direction,node.Coordinates
                    ,edge.FromCoordinate,length);
            }
            else
            {
                if (edge.ClockWise)
                {
                    MoveAlongCurve.MoveForwardOnCurveEdgeClockWise(
                        edge.Direction,
                        node.Coordinates
                        ,edge.FromCoordinate,edge.Radius
                        ,length);
                }
                else
                {
                    MoveAlongCurve.MoveForwardOnCurveEdgeAntiClockWise(
                        edge.Direction,
                        node.Coordinates
                        ,edge.FromCoordinate,edge.Radius
                        ,length);
                }

               

            }
        }
        
        
        private static void FindBaseCoordinateOfEdge(NodeJsonModel node
            ,EdgeJsonModel edge, decimal length)
        {
            edge.FromCoordinate = new Coordinate();
            if (edge.EdgeType == "L")
            {
                MoveAlongLine.MoveReverseOnLinearEdge(
                    edge.Direction,edge.FromCoordinate
                    ,node.Coordinates,length);
            }
            else
            {
                if (edge.ClockWise)
                {
                    MoveAlongCurve.MoveReverseOnCurveEdgeClockwise(
                        edge.Direction,
                        edge.FromCoordinate
                        ,node.Coordinates,edge.Radius
                        ,length);
                }
                else
                {
                    MoveAlongCurve.MoveReverseOnCurveEdgeAntiClockwise(
                        edge.Direction,
                        edge.FromCoordinate
                        ,node.Coordinates,edge.Radius
                        ,length);
                }
            }
        }
        private static void AddAdjacentEdgesToQueue(NodeJsonModel node, 
            List<EdgeJsonModel> edges)
        {
            
            foreach (var edge in node.Edges)
            {
                // We will use extension to check if Edge plotting queue is null
                // If Edge plotting queue is not null and it also contains selected edge Id
                // Then move to next Edge Id
                if (EdgePlottingQueue.IfNotNull(
                    s => s.Find(
                        x => x.EdgeId == edge.Key)
                    , () => throw new Exception ("Error in Adding edges in queue")) != null)
                {
                    continue;
                }

                

                if (edges.Find(x => 
                        x.EdgeId == edge.Key).ToCoordinate != null)
             
                    {
                        continue;
                    }
                    
                    EdgePlottingQueue.Add(edges.Find(x =>
                        x.EdgeId == edge.Key));
                    FindBaseCoordinateOfEdge(node,edges.Find(x =>
                            x.EdgeId == edge.Key)
                    ,edge.Value);
                    
            }
        }

        

        
            

        private static void SetOrigin(EdgeJsonModel edge)
        {
            edge.FromCoordinate = new Coordinate();
            edge.FromCoordinate.XVertex = 0m;
            edge.FromCoordinate.YVertex = 0m;
          
        }
    }
}
