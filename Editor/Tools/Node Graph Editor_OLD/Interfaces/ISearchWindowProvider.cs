using System.Collections.Generic;
using Konfus.Tools.Graph_Editor.Editor.Models;

namespace Konfus.Tools.Graph_Editor.Editor.Interfaces
{
    /// <summary>
    /// Based on: https://github.com/Unity-Technologies/UnityCsReference/blob/2022.2/Modules/GraphViewEditor/NodeSearch/SearchWindow.cs
    /// Converted to our own class so this works even if unity decides to change the API.
    /// This is part of the infamous UnityEditor.Experimental.GraphView namespace that is considered dprecated.
    /// However there is no substitution on the horizon for a neat search window like this one.
    /// </summary>
    public interface ISearchWindowProvider
    {
        List<SearchTreeEntry> CreateSearchTree();
    }
}