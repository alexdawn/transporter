using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class NetworkSquare: MonoBehaviour {
    public Network network;
    public List<Node> nodes;
    public SquareCell parentCell;

    public Network templateCrossRoad;

    public void Start()
    {
        nodes = new List<Node>();
        network = GameObject.FindWithTag("RoadNetwork").GetComponent<Network>();
    }

    public NetworkSquare(SquareCell parent)
    {
        parentCell = parent;
    }

    public void AddCrossRoad()
    {
        AddNetworkToTile(templateCrossRoad);
    }

    public void ClearTile()
    {
        if(nodes.Count > 0)
        {
            foreach (Node node in nodes)
                network.DeleteNode(node);
        }
    }

    public void TrimLinksToRoadType()
    {
        List<Node> nodesToCull = new List<Node>();
        for(int i = 0; i < 8; i += 2)
        {
            if(!parentCell.HasRoadThroughEdge((GridDirection)i))
            {
                nodesToCull.AddRange(nodes.FindAll(n => n.compassDirection == (GridDirection)i));
            }
        }
        foreach(Node n in nodesToCull)
        {
            network.DeleteNode(n);
        }
    }

    public void AddNetworkToTile(Network networkToCopy)
    {
        ClearTile();
        int oldCount = network.nodes.Count;
        network.JoinNetwork(networkToCopy);
        int newCount = network.nodes.Count;
        nodes.AddRange(network.nodes.GetRange(oldCount, newCount - oldCount));
        foreach(Node node in nodes)
        {
            node.Location += new Vector3(parentCell.coordinates.X, parentCell.GridElevations.AverageElevation * GridMetrics.elevationStep, parentCell.coordinates.Z);
        }
        TrimLinksToRoadType();
        ConnectWithNeighbours();
    }

    public Node GetNode(NodeDirection direction, GridDirection compass)
    {
        return nodes.Find(ni => ni.boundDirect == direction && ni.compassDirection == compass);
    }

    public void ConnectWithNeighbours()
    {
        for(int i=0; i<8; i+=2)
        {
            if (parentCell.HasRoadThroughEdge((GridDirection)i) && parentCell.GetNeighbor((GridDirection)i).roadNetwork.nodes.Count > 0)
                ConnectWithNeigbour(parentCell.GetNeighbor((GridDirection)i), (GridDirection)i);
        }
    }

    public void ConnectWithNeigbour(SquareCell neighbour, GridDirection direction)
    {
        //if (parentCell.GetVertexElevations.AverageElevation == neighbour.GetVertexElevations.AverageElevation)
        //{
        ConnectNodes(GetNode(NodeDirection.In, direction), neighbour.roadNetwork.GetNode(NodeDirection.Out, direction.Opposite()));
        ConnectNodes(GetNode(NodeDirection.Out, direction), neighbour.roadNetwork.GetNode(NodeDirection.In, direction.Opposite()));
        network.DeleteLink(GetNode(NodeDirection.Out, direction), GetNode(NodeDirection.In, direction));
        network.DeleteLink(neighbour.roadNetwork.GetNode(NodeDirection.Out, direction.Opposite()), neighbour.roadNetwork.GetNode(NodeDirection.In, direction.Opposite()));
        //}
    }

    public void ConnectNodes(Node anode, Node bnode)
    {
        if(anode != null && bnode != null)
        {
            if (anode.boundDirect == NodeDirection.Out && bnode.boundDirect == NodeDirection.In)
                network.MakeLink(anode, bnode);
            if (bnode.boundDirect == NodeDirection.Out && anode.boundDirect == NodeDirection.In)
                network.MakeLink(bnode, anode);
        }
    }
}
