using UnityEngine;

namespace Konfus.Systems.Grids
{
    public interface INode
    {
        Vector3Int GridPosition { get; }
        Vector3 WorldPosition { get; }
        INode[] Neighbors { get; }
    }
}