using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Systems.Grid
{
    public interface IGrid
    {
        IEnumerable<INode> Nodes { get; }
        Vector3Int Scale { get; }
        float CellSize { get; }
        bool InGridBounds(int x, int y, int z);
        bool InGridBounds(Vector3 worldPosition);
        Vector3 WorldPosFromGridPos(int x, int y, int z);
        Vector3 WorldPosFromGridPos(Vector3Int gridPos);
        void GridPosFromWorldPos(Vector3 worldPosition, out int x, out int y, out int z);
        Vector3Int GridPosFromWorldPos(Vector3 worldPosition);
        void SetNode(int x, int y, int z, INode value);
        void SetNode(Vector3 worldPosition, INode value);
        INode GetNode(int x, int y, int z);
        INode GetNode(Vector3Int gridPosition);
        INode GetNode(Vector3 worldPosition);
    }

}