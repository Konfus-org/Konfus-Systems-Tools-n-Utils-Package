using UnityEngine;

namespace Konfus.Systems.ThreeDGrid
{
    public interface INode
    {
        Vector3Int GridPosition { get; }
        Vector3 WorldPosition { get; }
        INode[] Neighbors { get; }
    }
}