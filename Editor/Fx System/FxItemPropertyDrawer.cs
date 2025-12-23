namespace Konfus.Editor.Fx_System
{
    /*[CustomPropertyDrawer(typeof(FxItem), false)]
    internal class FxItemPropertyDrawer : PropertyDrawer
    {
        private static string[]? _choices;
        private static Type[] _availableEffectTypes = Array.Empty<Type>();

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Initialize if we haven't already...
            CacheChoicesIfNotAlreadyCached();

            // Deserialize the effect type and index
            int effectTypeIndex = DeserializeEffectType(property, out SerializedProperty effectTypeProperty);
            SerializedProperty? fxItemsProperty = property.serializedObject.FindProperty("fxItems");
            SerializedProperty? effectProperty = property.FindPropertyRelative("effect");
            bool hasBeenDuped = HasBeenDuplicated(property, fxItemsProperty);

            // If we are playing, render as green!
            Color originalColor = GUI.color;
            if (effectProperty.managedReferenceValue is Effect { IsPlaying: true }) GUI.color = Color.green;

            // If our effect couldn't be deserialized
            var serializedEffectNoLongerExists = false;
            if (effectTypeIndex == -1)
            {
                GUI.color = Color.red;
                serializedEffectNoLongerExists = true;
            }

            // Draw choice dropdown
            EditorGUI.BeginChangeCheck();
            {
                if (serializedEffectNoLongerExists)
                {
                    string[]? choices = _choices?
                        .Append($"ERROR: \"{effectTypeProperty.stringValue}\" no longer exists!")
                        .ToArray();
                    effectTypeIndex = EditorGUI.Popup(
                        new Rect(position) { height = EditorGUIUtility.singleLineHeight },
                        choices?.Length - 1 ?? 0,
                        choices);
                }
                else
                {
                    effectTypeIndex = EditorGUI.Popup(
                        new Rect(position) { height = EditorGUIUtility.singleLineHeight },
                        effectTypeIndex,
                        _choices);
                }
            }

            // Set the effect type if we chose one and our choice is valid
            if (EditorGUI.EndChangeCheck() || hasBeenDuped || effectProperty.managedReferenceValue == null)
            {
                CreateEffect(effectTypeIndex, effectProperty, effectTypeProperty);
                if (hasBeenDuped)
                {
                    // This is a hack... for some reason we reset the original value instead of the duped value so for now, we will just swap them :,)
                    fxItemsProperty.MoveArrayElement(fxItemsProperty.arraySize - 2, fxItemsProperty.arraySize - 1);
                }
            }

            if (effectTypeIndex > 0)
            {
                // Draw effect property if our choice is something other than none
                var effectPos = new Rect(position)
                {
                    width = position.width,
                    height = EditorGUI.GetPropertyHeight(effectProperty)
                };
                EditorGUI.PropertyField(effectPos, effectProperty, GUIContent.none);
            }

            // Set color back to original color
            GUI.color = originalColor;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty? effectProperty = property.FindPropertyRelative("effect");
            if (effectProperty.isExpanded && effectProperty.managedReferenceValue is not NoEffect)
                return EditorGUI.GetPropertyHeight(effectProperty, label, true);

            return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private void CacheChoicesIfNotAlreadyCached()
        {
            // We've already initialized return
            if (_choices != null) return;

            // Get available choices and types
            _availableEffectTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(type => typeof(Effect).IsAssignableFrom(type) && !type.IsAbstract && !type.IsGenericType &&
                               typeof(NoEffect) != type)
                .ToArray();
            string[] choices = { "None" };
            _choices = choices.Union(_availableEffectTypes.Select(type => type.Name)).ToArray();
        }

        private static void CreateEffect(int effectTypeIndex, SerializedProperty effectProperty,
            SerializedProperty effectTypeProperty)
        {
            if (effectTypeIndex == 0)
            {
                // We chose none, set the effect property to null and save choice
                effectProperty.managedReferenceValue = new NoEffect();
                effectTypeProperty.stringValue = "None";
            }
            else
            {
                // Create chosen type and set the effect property to it and save choice
                Type effectType = _availableEffectTypes[effectTypeIndex - 1];
                object effect = Activator.CreateInstance(effectType);
                effectProperty.managedReferenceValue = effect;
                effectTypeProperty.stringValue = effectType.Name;
            }
        }

        private static int DeserializeEffectType(SerializedProperty property, out SerializedProperty effectTypeProperty)
        {
            var effectTypeIndex = 0;
            effectTypeProperty = property.FindPropertyRelative("effectType");
            if (!EffectTypeIsNone(effectTypeProperty))
            {
                string? effectTypeName = effectTypeProperty.stringValue;
                Type? effectType = _availableEffectTypes.FirstOrDefault(type => type.Name == effectTypeName);
                if (effectType == null) return -1; // no longer exists!
                effectTypeIndex = Array.IndexOf(_availableEffectTypes, effectType) + 1;
            }

            return effectTypeIndex;
        }

        private static bool EffectTypeIsNone(SerializedProperty effectTypeProperty)
        {
            return effectTypeProperty.stringValue == "None" || effectTypeProperty.stringValue == string.Empty;
        }

        private static bool HasBeenDuplicated(SerializedProperty property, SerializedProperty fxItemsProperty)
        {
            if (fxItemsProperty is not { arraySize: > 1 }) return false;

            SerializedProperty? fxItemPotentialDuplicateProperty =
                fxItemsProperty.GetArrayElementAtIndex(fxItemsProperty.arraySize - 1);
            if (fxItemPotentialDuplicateProperty.contentHash != property.contentHash) return false;

            SerializedProperty? fxItemDuplicatedProperty =
                fxItemsProperty.GetArrayElementAtIndex(fxItemsProperty.arraySize - 2);
            bool hasBeenDuped = fxItemPotentialDuplicateProperty.contentHash == fxItemDuplicatedProperty.contentHash;
            if (hasBeenDuped) return true;

            return false;
        }
    }*/
}