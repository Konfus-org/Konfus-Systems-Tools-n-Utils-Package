using System;
using System.Collections.Generic;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    /// <summary>
    ///     Data container for the StackNode views
    /// </summary>
    [Serializable]
    public class StackNode
    {
        /// <summary>
        ///     List of node GUID that are in the stack
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public List<PropertyName> nodeGUIDs = new();

        public Vector2 position;
        public string title = "New Stack";

        /// <summary>
        ///     Is the stack accept drag and dropped nodes
        /// </summary>
        public bool acceptDrop;

        /// <summary>
        ///     Is the stack accepting node created by pressing space over the stack node
        /// </summary>
        public bool acceptNewNode;

        public StackNode(Vector2 position, string title = "Stack", bool acceptDrop = true, bool acceptNewNode = true)
        {
            this.position = position;
            this.title = title;
            this.acceptDrop = acceptDrop;
            this.acceptNewNode = acceptNewNode;
        }
    }
}