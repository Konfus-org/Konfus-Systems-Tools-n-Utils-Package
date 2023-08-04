using UnityEngine;

namespace Konfus.Systems.Grid
{
    public interface INode
    {
        Vector3Int GetGridPosition();
        Vector3 GetWorldPosition();
    }
}