using System;
using UnityEditor.UIElements;

namespace Konfus.Tools.Graph_Editor.Editor.Settings
{
    public class ReactiveSettings
    {
        public ReactiveSettings(Action OnSettingsChanged)
        {
            this.OnSettingsChanged = OnSettingsChanged;
            SettingsChanged(null);
            GraphSettingsSingleton.Settings.ValueChanged -= SettingsChanged;
            GraphSettingsSingleton.Settings.ValueChanged += SettingsChanged;
        }

        private Action OnSettingsChanged;

        private void SettingsChanged(SerializedPropertyChangeEvent evt)
        {
            OnSettingsChanged();
        }

        public static void Create(ref ReactiveSettings instanceField, Action OnSettingsChanged)
        {
            if (instanceField == null) instanceField = new ReactiveSettings(OnSettingsChanged);
        }
    }
}