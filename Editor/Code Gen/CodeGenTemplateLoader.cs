using System;
using System.Linq;
using UnityEngine;

namespace Konfus.Editor.Code_Gen
{
    internal class CodeGenTemplateLoader
    {
        public static CodeGenTemplate? Load(string name)
        {
            TextAsset[]? templateAssets = Resources.LoadAll<TextAsset>("Code Gen Templates");
            TextAsset? templateAsset = templateAssets.FirstOrDefault(t => t.name == name);
            return templateAsset == null ? null : new CodeGenTemplate(templateAsset.name, templateAsset.text);
        }

        public static CodeGenTemplate[]? LoadAll()
        {
            TextAsset[]? templateAssets = Resources.LoadAll<TextAsset>("Code Gen Templates");
            return (from t in templateAssets ?? Array.Empty<TextAsset>() select new CodeGenTemplate(t.name, t.text))
                .ToArray();
        }
    }
}