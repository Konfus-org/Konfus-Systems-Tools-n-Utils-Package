using System.Reflection;

namespace Konfus.Systems.Node_Graph
{
    /// <summary>
    ///     Simplifies to construction of UnityPath.
    ///     To get start use the static Init() method.
    /// </summary>
    public class UnityPathFactory
    {
        private string path;

        private UnityPathFactory()
        {
            path = string.Empty;
        }

        private UnityPathFactory(string path)
        {
            this.path = path;
        }

        public static UnityPathFactory Init()
        {
            return new();
        }

        private static string Append(string basePath, string value)
        {
            return $"{basePath}{(string.IsNullOrEmpty(basePath) ? string.Empty : '.')}{value}";
        }

        public UnityPathFactory Append(string value)
        {
            path = Append(path, value);
            return this;
        }

        public UnityPathFactory Append(MemberInfo value)
        {
            path = Append(path, value.Name);
            return this;
        }

        /// <summary>
        ///     Returns UnityPath with value appended to path.
        ///     This method does not affect the factory path.
        /// </summary>
        public UnityPath Assemble(string value)
        {
            return new(Append(path, value));
        }

        /// <summary>
        ///     Returns UnityPath with value appended to path.
        ///     This method does not affect the factory path.
        /// </summary>
        public UnityPath Assemble(MemberInfo value)
        {
            return new(Append(path, value.Name));
        }

        /// <summary>
        ///     Effectively clones the factory.
        ///     Use when multiple children require the same base path.
        /// </summary>
        public UnityPathFactory Branch()
        {
            return new(path);
        }
    }
}