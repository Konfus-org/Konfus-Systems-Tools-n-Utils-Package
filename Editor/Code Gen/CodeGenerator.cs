using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Konfus.Editor.Automation;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Konfus.Editor.Code_Gen
{
    public class CodeGenerator
    {
        public static string GenerateCode(CodeGenTemplate template, string? intendedFolder = null)
        {
            intendedFolder ??= Application.dataPath + "/Assets";
            string namespaceName = GenerateNamespace(intendedFolder);
            string contents = template.Content
                .Replace("#NAMESPACE#", namespaceName, StringComparison.Ordinal)
                .Replace("#SCRIPTNAME#", template.Name, StringComparison.Ordinal);
            contents += '\n';
            return contents;
        }

        /// <summary>
        /// Generates script files from given templates.
        /// </summary>
        public static void GenerateScripts(CodeGenTemplate[] templates, string? targetFolder = null)
        {
            using var scope = new AssetDatabase.AssetEditingScope();
            targetFolder ??= ProjectManager.EditorGeneratedCodePath;
            if (!AssetDatabase.IsValidFolder(targetFolder))
                ProjectManager.Ensure(targetFolder);

            foreach (CodeGenTemplate template in templates)
            {
                GenerateScript(template, targetFolder, false);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Generates a script file, does not ensure the target folder exists, ensure it exists before calling.
        /// </summary>
        public static void GenerateScript(CodeGenTemplate codeGenTemplate, string? targetFolder = null,
            bool updateAssetDatabase = true)
        {
            targetFolder ??= Application.dataPath + "/Assets";
            string contents = GenerateCode(codeGenTemplate, targetFolder);
            string path = $"{targetFolder}/{ProjectManager.Sanitize(codeGenTemplate.Name)}.cs"
                .Replace("\\", "/");

            if (updateAssetDatabase &&
                !AssetDatabase.IsValidFolder(ProjectManager.EditorGeneratedCodePath))
                ProjectManager.Ensure(ProjectManager.EditorGeneratedCodePath);
            File.WriteAllText(ProjectManager.Absolute(path), contents, new UTF8Encoding(false));
            if (updateAssetDatabase) AssetDatabase.ImportAsset(path);

            var created = AssetDatabase.LoadAssetAtPath<Object>(path);
            if (created != null)
                ProjectWindowUtil.ShowCreatedAsset(created);
        }

        public static string GenerateNamespace(string folder)
        {
            string root = GenerateIdentifier(Application.productName, true);
            if (string.IsNullOrWhiteSpace(root))
                root = "Project";

            folder = folder.Replace("\\", "/");

            string[] parts = folder.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

            // Find "Code" or "Scripts" segment
            int idx = Array.FindIndex(parts, p =>
                p.Equals("Code", StringComparison.OrdinalIgnoreCase) ||
                p.Equals("Scripts", StringComparison.OrdinalIgnoreCase));

            if (idx < 0 || idx == parts.Length - 1)
                return root;

            // Take segments after Code/Scripts
            string[] segments = parts
                .Skip(idx + 1)
                .Where(s => !s.Equals("Editor", StringComparison.OrdinalIgnoreCase)) // keep namespaces cleaner
                .Select(s => GenerateIdentifier(s, true))
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToArray();

            if (segments.Length == 0)
                return root;

            return root + "." + string.Join(".", segments);
        }

        public static string GenerateIdentifier(string value, bool pascalCase)
        {
            if (string.IsNullOrWhiteSpace(value))
                return "";

            // Split on non-alphanumeric, then stitch as PascalCase if requested.
            var tokens = new List<string>();
            var cur = new StringBuilder();

            foreach (char c in value)
            {
                if (char.IsLetterOrDigit(c))
                    cur.Append(c);
                else
                {
                    if (cur.Length > 0)
                    {
                        tokens.Add(cur.ToString());
                        cur.Clear();
                    }
                }
            }

            if (cur.Length > 0)
                tokens.Add(cur.ToString());

            if (tokens.Count == 0)
                return "";

            string result;
            if (pascalCase)
            {
                result = string.Concat(tokens.Select(t =>
                {
                    string lower = t.ToLowerInvariant();
                    return char.ToUpperInvariant(lower[0]) + lower.Substring(1);
                }));
            }
            else
                result = string.Join("_", tokens);

            // Identifiers can't start with a digit
            if (char.IsDigit(result[0]))
                result = "_" + result;

            return result;
        }
    }
}