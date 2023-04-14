using System.Collections.Generic;

namespace Konfus.Systems.Node_Graph
{
    public class NodeDelegates
    {
        public delegate IEnumerable<PortData> CustomPortBehaviorDelegate(List<SerializableEdge> edges);

        public delegate IEnumerable<PortData> CustomPortTypeBehaviorDelegate(string fieldName, string displayName,
            object value);

        public class CustomPortBehaviorDelegateInfo
        {
            public CustomPortBehaviorDelegateInfo(CustomPortBehaviorDelegate deleg, bool cloneResults)
            {
                Delegate = deleg;
                CloneResults = cloneResults;
            }

            public CustomPortBehaviorDelegate Delegate { get; }

            public bool CloneResults { get; }
        }

        public class CustomPortTypeBehaviorDelegateInfo
        {
            public CustomPortTypeBehaviorDelegateInfo(CustomPortTypeBehaviorDelegate deleg, bool cloneResults)
            {
                Delegate = deleg;
                CloneResults = cloneResults;
            }

            public CustomPortTypeBehaviorDelegate Delegate { get; }

            public bool CloneResults { get; }
        }
    }
}