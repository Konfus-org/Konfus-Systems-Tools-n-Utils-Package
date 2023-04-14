using System;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    public static class NodeUtils
    {
        public delegate Node NodeCreationMethod(Type nodeType, Vector2 position, params object[] args);

        public delegate Node NodeCreationMethod<T>(Vector2 position, params object[] args);
    }
}