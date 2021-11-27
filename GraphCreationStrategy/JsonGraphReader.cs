using System.Collections.Generic;
using System.Linq;
using m_sort_server.Interfaces;
using m_sort_server.Models;

namespace m_sort_server.Services
{
    public class JsonGraphReader:IGraphReader
    {
         private int[][] Matrix;
         private string GraphId;



         public  Graph GetGraph(ILogFileService logFileService)
        {
           Graph graph = new Graph();
           
           graph.EdgeJsonModels = logFileService.ReadFromJsonFile<EdgeJsonModel>("edge_json_model");
          // Every edge will contain nodes in it
          // Add Nodes in a list, add its distance from the 'from node'
          // What if node already present
          // Every node will contain a edfe list.Just add into it
           graph.NodeJsonModels = CreateNodesFromEdges( graph.EdgeJsonModels);
           
           // Once we have all nodes
           // We plot XY coordinates of all Nodes
           // We plot XY coordinates of all edges
           GraphPlottingService.PlotGraph(graph.EdgeJsonModels, graph.NodeJsonModels);
           
           return graph;
        }

       
        
       
        private   List<NodeJsonModel> CreateNodesFromEdges(List<EdgeJsonModel> edges)
        {
            
            List<NodeJsonModel> nodes = new List<NodeJsonModel>();
            int NodeIndex = 0;
            foreach (var edge in edges)
            {
                foreach (var node in edge.Nodes)
                {
                    // Check if nodes list already exist
                    // If already exist then ignore then move to next logic
                    // Else add the first node in the node list
                    nodes.IfNotNull<List<NodeJsonModel>, NodeJsonModel>(x =>
                    {
                        // If Node already exist in the list
                        // We will add this edge into the node list
                        if (nodes.Find(y => y.NodeId == node) != null)
                        {
                            return IfNodeAlreadyPresent(nodes, node, edge);
                        }
                        // Node already not present, then add node in the list

                        return IfNodeAlreadyNotPresent(nodes,node,NodeIndex,edge);
                    }, () => IfNodeAlreadyNotPresent(nodes,node,NodeIndex,edge));
                }
                
                AddSourcesAndExits(edge, nodes);
                AddDropOffs(edge,nodes);
            }

            return nodes;
        }

      
        private static void AddSourcesAndExits( EdgeJsonModel edge,List<NodeJsonModel> nodes)
        {
            
                if (edge.Sources != null)
                {
                   foreach (var source in edge.Sources)
                   {
                       NodeJsonModel node = nodes
                           .Find(x => x.NodeId
                                      == source);
                       node.Source = true;
                       node.Junction = false;
                   }
                    
                      
                }
                if (edge.Exits != null)
                {
                    foreach (var exit in edge.Exits)
                    {
                        NodeJsonModel node = nodes
                            .Find(x => x.NodeId
                                       == exit);
                        node.Exit = true;
                    }
                }
            
        }

        private static void AddDropOffs(EdgeJsonModel edge,List<NodeJsonModel> nodes)
        {
            if (edge.DropOffs != null)
            {
                foreach (var dropOff in edge.DropOffs)
                {
                    NodeJsonModel node = nodes
                        .Find(x => x.NodeId
                                   == dropOff);
                    node.DropOff = true;
                }
                    
                      
            }
        }

        private static void AddForwardNodeAndEdge(NodeJsonModel node, EdgeJsonModel edge)
        {
            // Check if forward nodeDataModel exit
            if (edge.Nodes.Last() != node.NodeId )
            {
                node.ForwardNodes.Add(edge.Nodes[
                    edge.Nodes.IndexOf(node.NodeId) + 1]);
            }
            // Add Edge
            if (edge.EdgeId.Equals("")||node.Equals(null)||edge.Distances.Equals(null))
            {
                int a = 1;
            }
            
            node.Edges.Add(new KeyValuePair<string, decimal>(edge.EdgeId,
                edge.Distances[edge.Nodes.IndexOf(node.NodeId)]));
            
        }
        
        // In case Node already present in the Node List
        // Add the new edge into node's edgelist
        private NodeJsonModel IfNodeAlreadyPresent(List<NodeJsonModel> nodes,string nodeId, EdgeJsonModel edge)
        {
            NodeJsonModel node = nodes.Find(z => z.NodeId == nodeId);
            // So, forward node is the next node for our selected node on the same edge
            AddForwardNodeAndEdge(node,edge);
            // Since this node has more than one edge attached to it
            // It will ber a junction node
            node.Junction = true;
            return node;
        }
        
        // In case Node already not present in the Node List
        // Add the new node into node list
        private NodeJsonModel IfNodeAlreadyNotPresent(List<NodeJsonModel> nodes,string nodeId, int NodeIndex,EdgeJsonModel edge)
        {
            nodes.Add(CreateNodeFromId(nodeId, edge,NodeIndex));
            NodeIndex = NodeIndex + 1;
            return nodes.Find(z => z.NodeId == nodeId);;
        }
        
        private  NodeJsonModel CreateNodeFromId(string nodeId,EdgeJsonModel edge, int NodeIndex)
        {
           
            NodeJsonModel node = new NodeJsonModel()
            {
                NodeId = nodeId,
                Edges = new List<KeyValuePair<string, decimal>>(),
                GraphId = edge.GraphId,
                Index = NodeIndex.ToString(),
                Junction = false,
                ForwardNodes = new List<string>()
            };
            AddForwardNodeAndEdge(node,edge);
            // By Default assume it is a dropOff Node
            return node;
        }

    }
}