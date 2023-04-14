using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Konfus.Systems.Node_Graph
{
    [Serializable]
    public class UnityPath
    {
        private Dictionary<object, List<MemberInfo>> pathAsMemberInfoArrayByOrigin = new();

        [SerializeField] private string path;

        [SerializeField] private string[] pathArray;

        public UnityPath(string path)
        {
            this.path = path;
            pathArray = null;
        }

        public UnityPath(MemberInfo member)
        {
            path = member.Name;
            pathArray = null;
        }

        public UnityPath(IEnumerable<string> pathArray)
        {
            path = null;
            this.pathArray = pathArray.ToArray();
        }

        public UnityPath(IEnumerable<MemberInfo> members)
        {
            path = null;
            pathArray = members.Select(x => x.Name).ToArray();
        }

        public string LastFieldName => PathArray.Last();

        public string Path =>
            PropertyUtils.LazyLoad(ref path, BuildPathFromArray, value => string.IsNullOrEmpty(value));

        public string[] PathArray => PropertyUtils.LazyLoad(ref pathArray, SplitPathIntoArray,
            value => value == null || value.Length == 0);

        public static implicit operator string(UnityPath unityPath)
        {
            return unityPath.Path;
        }

        public static implicit operator string[](UnityPath unityPath)
        {
            return unityPath.PathArray;
        }

        public List<MemberInfo> GetPathAsMemberInfoList(object startValue)
        {
            if (pathAsMemberInfoArrayByOrigin.ContainsKey(startValue))
                return pathAsMemberInfoArrayByOrigin[startValue];

            List<MemberInfo> fieldInfoPath = new();
            object value = startValue;
            for (int i = 0; i < PathArray.Length; i++)
            {
                MemberInfo[] members = value.GetType().GetMember(PathArray[i],
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (members.Length == 0)
                {
                    Debug.LogWarning(Path + " " + "(" + PathArray[i] + ")" + " not found.");
                    return null;
                }

                MemberInfo info = members[0];
                fieldInfoPath.Add(info);
                if (i + 1 < PathArray.Length) value = fieldInfoPath[i].GetValue(value);
            }

            pathAsMemberInfoArrayByOrigin[startValue] = fieldInfoPath;
            return fieldInfoPath;
        }

        public object GetValueOfMemberAtPath(object startingValue)
        {
            return GetPathAsMemberInfoList(startingValue).GetFinalValue(startingValue);
        }

        public void SetValueOfMemberAtPath(object startingValue, object finalValue)
        {
            GetPathAsMemberInfoList(startingValue).SetValue(startingValue, finalValue);
        }

        private string BuildPathFromArray()
        {
            string path = "";
            foreach (string fragment in pathArray)
            {
                path = string.Concat(path, fragment);
                path += ".";
            }

            return path;
        }

        private string[] SplitPathIntoArray()
        {
            string[] paths = path.Split('.');
            return paths;
        }
    }
}