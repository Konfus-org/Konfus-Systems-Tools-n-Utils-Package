using UnityEngine;

namespace Konfus.Systems.Grids
{
    public interface INode
    {
        Color DebugColor { get; }
        Vector3Int GridPosition { get; }
        Vector3 WorldPosition { get; }
        INode[] Neighbors { get; }
    }
}