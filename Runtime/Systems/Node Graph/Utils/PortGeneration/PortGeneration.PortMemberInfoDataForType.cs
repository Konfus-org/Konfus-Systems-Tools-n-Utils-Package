using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Konfus.Systems.Node_Graph
{
    public static partial class PortGeneration
    {
        public struct PortMemberInfoDataForType
        {
            public PortMemberInfoDataForType(Type type, IEnumerable<MemberInfo> membersWithInputAttribute,
                IEnumerable<MemberInfo> membersWithOutputAttribute,
                IEnumerable<MemberInfo> membersWithNestedPortsAttribute,
                Dictionary<MemberInfo, NodeDelegates.CustomPortBehaviorDelegateInfo> customBehaviorInfoByMember)
            {
                this.Type = type;
                this.MembersWithInputAttribute = membersWithInputAttribute;
                this.MembersWithOutputAttribute = membersWithOutputAttribute;
                this.MembersWithNestedPortsAttribute = membersWithNestedPortsAttribute;
                this.CustomBehaviorInfoByMember = customBehaviorInfoByMember;
            }

            public Type Type { get; }

            public IEnumerable<MemberInfo> MembersWithInputAttribute { get; }

            public IEnumerable<MemberInfo> MembersWithOutputAttribute { get; }

            public IEnumerable<MemberInfo> MembersWithInputOrOutputAttribute =>
                MembersWithInputAttribute.Concat(MembersWithOutputAttribute);

            public IEnumerable<MemberInfo> MembersWithNestedPortsAttribute { get; }

            public Dictionary<MemberInfo, NodeDelegates.CustomPortBehaviorDelegateInfo> CustomBehaviorInfoByMember
            {
                get;
            }
        }
    }
}