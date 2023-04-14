using UnityEngine;

namespace Konfus.Tools.Graph_Editor.Editor.Utils
{
    public static class PlatformUtils
    {
        public static readonly bool IsMac
            = Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer;
    }
}