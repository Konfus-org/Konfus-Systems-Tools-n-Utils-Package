using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    public static partial class PortGeneration
    {
        #region Reflection Generation Of Ports

        internal static List<Node.NodeFieldInformation> GetAllPortInformation(object owner,
            UnityPathFactory proxiedFieldPath = null)
        {
            if (proxiedFieldPath == null)
                proxiedFieldPath = UnityPathFactory.Init();

            List<Node.NodeFieldInformation> nodePortInformation = new();

            Type dataType = owner.GetType();

            PortMemberInfoDataForType portMemberInfoDataForType = GeneratePortMemberInfoDataForType(dataType, owner);
            foreach (MemberInfo member in portMemberInfoDataForType.MembersWithInputOrOutputAttribute)
            {
                portMemberInfoDataForType.CustomBehaviorInfoByMember.TryGetValue(member,
                    out NodeDelegates.CustomPortBehaviorDelegateInfo customBehavior);
                nodePortInformation.Add(new Node.NodeFieldInformation(owner, member, customBehavior,
                    proxiedFieldPath.Assemble(member)));
            }

            foreach (MemberInfo member in portMemberInfoDataForType.MembersWithNestedPortsAttribute)
            {
                // If the NestedPorts member is null, try to supply a value
                if (member.GetValue(owner) == null)
                {
                    if (!member.GetUnderlyingType().TryInstantiate(out object instance))
                    {
                        Debug.LogError(
                            $"Skipping NestedPorts member {member.Name} as it's null and doesn't contain a parameterless constructor. Please either provide a parameterless constructor or initialise the member manually.");
                        continue;
                    }

                    member.SetValue(owner, instance);
                }

                nodePortInformation.AddRange(GetAllPortInformation(member.GetValue(owner),
                    proxiedFieldPath.Branch().Append(member)));
            }

            return nodePortInformation;
        }

        internal static PortMemberInfoDataForType GeneratePortMemberInfoDataForType(Type type, object owner)
        {
            MemberInfo[] members = type.GetInstanceFieldsAndProperties();
            IEnumerable<MemberInfo> membersWithInputAttribute =
                members.Where(x => x.HasCustomAttribute<InputAttribute>());
            IEnumerable<MemberInfo> membersWithOutputAttribute =
                members.Where(x => x.HasCustomAttribute<OutputAttribute>());
            IEnumerable<MemberInfo> membersWithNestedPortsAttribute =
                members.Where(x => x.HasCustomAttribute<NestedPortsAttribute>());
            MethodInfo[] methodsWithCustomPortBehavior =
                type.GetInstanceMethodsByAttribute<CustomPortBehaviorAttribute>();

            Dictionary<MemberInfo, NodeDelegates.CustomPortBehaviorDelegateInfo> customBehaviorInfoByMember = new();
            foreach (MethodInfo customPortBehaviorMethod in methodsWithCustomPortBehavior)
            {
                var customPortBehaviorAttribute =
                    customPortBehaviorMethod.GetCustomAttribute<CustomPortBehaviorAttribute>();

                MemberInfo field = membersWithInputAttribute.Concat(membersWithOutputAttribute)
                    .FirstOrDefault(x => x.Name == customPortBehaviorAttribute.fieldName);

                if (field == null)
                {
                    // InvalidCustomPortBehaviorFieldNameErrorMessage
                    Debug.LogError(
                        $"Invalid field name for custom port behavior: {customPortBehaviorMethod}, {customPortBehaviorAttribute.fieldName}");
                    continue;
                }

                try
                {
                    var deleg = Delegate.CreateDelegate(typeof(NodeDelegates.CustomPortBehaviorDelegate), owner,
                        customPortBehaviorMethod, true);
                    NodeDelegates.CustomPortBehaviorDelegateInfo delegInfo =
                        new(deleg as NodeDelegates.CustomPortBehaviorDelegate,
                            customPortBehaviorAttribute.cloneResults);
                    customBehaviorInfoByMember.Add(field, delegInfo);
                }
                catch
                {
                    // InvalidCustomPortBehaviorSignatureErrorMessage
                    Debug.LogError(
                        $"The function {customPortBehaviorMethod} cannot be converted to the required delegate format: {typeof(NodeDelegates.CustomPortBehaviorDelegate)}");
                }
            }

            return new PortMemberInfoDataForType(type, membersWithInputAttribute, membersWithOutputAttribute,
                membersWithNestedPortsAttribute, customBehaviorInfoByMember);
        }

        #endregion
    }
}