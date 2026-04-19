using System;
using System.Linq;
using Konfus.Fx_System;
using Konfus.Fx_System.Effects;
using UnityEditor;
using UnityEngine;

namespace Konfus.Editor.Fx_System
{
    [CustomPropertyDrawer(typeof(FxItem), false)]
    internal class FxItemPropertyDrawer : PropertyDrawer
    {
        private const float TimelineRowHeight = 22f;
        private const float TimingFieldGap = 8f;
        private static string[]? _choices;
        private static Type[] _availableEffectTypes = Array.Empty<Type>();

        private readonly struct AutoTrackTiming
        {
            public AutoTrackTiming(float start, float stop, bool isAsync, bool hasEffect, float timelineEnd)
            {
                Start = start;
                Stop = stop;
                IsAsync = isAsync;
                HasEffect = hasEffect;
                TimelineEnd = timelineEnd;
            }

            public float Start { get; }
            public float Stop { get; }
            public bool IsAsync { get; }
            public bool HasEffect { get; }
            public float TimelineEnd { get; }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            CacheChoicesIfNotAlreadyCached();

            int effectTypeIndex = DeserializeEffectType(property, out SerializedProperty effectTypeProperty);
            SerializedProperty? fxItemsProperty = property.serializedObject.FindProperty("fxItems");
            SerializedProperty? effectProperty = property.FindPropertyRelative("effect");
            bool hasBeenDuped = HasBeenDuplicated(property, fxItemsProperty);

            if (effectProperty == null) return;

            Color originalColor = GUI.color;
            if (effectProperty.managedReferenceValue is Effect { IsPlaying: true }) GUI.color = Color.green;

            bool serializedEffectNoLongerExists = false;
            if (effectTypeIndex == -1)
            {
                GUI.color = Color.red;
                serializedEffectNoLongerExists = true;
            }

            float y = position.y;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;

            Rect effectTypeRect = new Rect(position.x, y, position.width, lineHeight);
            EditorGUI.BeginChangeCheck();
            {
                if (serializedEffectNoLongerExists)
                {
                    string[]? choices = _choices?
                        .Append($"ERROR: \"{effectTypeProperty.stringValue}\" no longer exists!")
                        .ToArray();
                    effectTypeIndex = EditorGUI.Popup(effectTypeRect, choices?.Length - 1 ?? 0, choices);
                }
                else
                {
                    effectTypeIndex = EditorGUI.Popup(effectTypeRect, effectTypeIndex, _choices);
                }
            }

            if (EditorGUI.EndChangeCheck() || hasBeenDuped || effectProperty.managedReferenceValue == null)
            {
                CreateEffect(effectTypeIndex, effectProperty, effectTypeProperty);
                if (hasBeenDuped && fxItemsProperty != null)
                {
                    // This is a hack... for some reason we reset the original value instead of the duped value so for now, we will just swap them :,)
                    fxItemsProperty.MoveArrayElement(fxItemsProperty.arraySize - 2, fxItemsProperty.arraySize - 1);
                }
            }

            Effect? effect = effectProperty.managedReferenceValue as Effect;
            AutoTrackTiming timing = GetAutoTrackTiming(property, fxItemsProperty, effect);

            y += lineHeight + spacing;

            Rect startTimeRect = new Rect(position.x, y, (position.width - TimingFieldGap) * 0.5f, lineHeight);
            Rect stopTimeRect = new Rect(startTimeRect.xMax + TimingFieldGap, y, (position.width - TimingFieldGap) * 0.5f, lineHeight);
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUI.FloatField(startTimeRect, new GUIContent("Start (Auto)"), timing.Start);
                EditorGUI.FloatField(stopTimeRect, new GUIContent("Stop (Auto)"), timing.Stop);
            }

            y += lineHeight + spacing;

            Rect timelineRect = new Rect(position.x, y, position.width, TimelineRowHeight);
            DrawTrackTimeline(timelineRect, timing);

            y += TimelineRowHeight + spacing;

            if (effectTypeIndex > 0)
            {
                Rect effectRect = new Rect(position.x, y, position.width, EditorGUI.GetPropertyHeight(effectProperty, true));
                EditorGUI.PropertyField(effectRect, effectProperty, new GUIContent("Settings"), true);
            }

            GUI.color = originalColor;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            CacheChoicesIfNotAlreadyCached();

            float lineHeight = EditorGUIUtility.singleLineHeight;
            float spacing = EditorGUIUtility.standardVerticalSpacing;
            float total = lineHeight + spacing + lineHeight + spacing + TimelineRowHeight + spacing;

            int effectTypeIndex = DeserializeEffectType(property, out _);
            SerializedProperty? effectProperty = property.FindPropertyRelative("effect");
            if (effectTypeIndex > 0 && effectProperty != null)
                total += EditorGUI.GetPropertyHeight(effectProperty, label, true) + spacing;

            return total;
        }

        private static AutoTrackTiming GetAutoTrackTiming(SerializedProperty currentItemProperty,
            SerializedProperty? fxItemsProperty,
            Effect? currentEffect)
        {
            if (fxItemsProperty == null || !fxItemsProperty.isArray)
            {
                float fallbackDuration = Mathf.Max(0f, currentEffect?.Duration ?? 0f);
                bool fallbackHasEffect = currentEffect != null && currentEffect is not NoEffect;
                return new AutoTrackTiming(
                    0f,
                    fallbackDuration,
                    currentEffect?.ShouldPlayAsync == true,
                    fallbackHasEffect,
                    Mathf.Max(1f, fallbackDuration));
            }

            float sequentialCursor = 0f;
            float maxEnd = 0f;
            AutoTrackTiming timing = default;
            bool found = false;

            for (int i = 0; i < fxItemsProperty.arraySize; i++)
            {
                SerializedProperty itemProperty = fxItemsProperty.GetArrayElementAtIndex(i);
                SerializedProperty? itemEffectProperty = itemProperty.FindPropertyRelative("effect");
                Effect? itemEffect = itemEffectProperty?.managedReferenceValue as Effect;

                float duration = Mathf.Max(0f, itemEffect?.Duration ?? 0f);
                float start = sequentialCursor;
                float stop = start + duration;
                maxEnd = Mathf.Max(maxEnd, stop);

                bool isAsync = itemEffect?.ShouldPlayAsync == true;
                bool hasEffect = itemEffect != null && itemEffect is not NoEffect;

                if (itemProperty.propertyPath == currentItemProperty.propertyPath)
                {
                    timing = new AutoTrackTiming(start, stop, isAsync, hasEffect, 0f);
                    found = true;
                }

                if (!isAsync)
                    sequentialCursor = stop;
            }

            if (found)
                return new AutoTrackTiming(timing.Start, timing.Stop, timing.IsAsync, timing.HasEffect, Mathf.Max(1f, maxEnd));

            float currentDuration = Mathf.Max(0f, currentEffect?.Duration ?? 0f);
            bool currentHasEffect = currentEffect != null && currentEffect is not NoEffect;
            return new AutoTrackTiming(0f, currentDuration, currentEffect?.ShouldPlayAsync == true, currentHasEffect,
                Mathf.Max(1f, maxEnd));
        }

        private static void DrawTrackTimeline(Rect rect, AutoTrackTiming timing)
        {
            EditorGUI.DrawRect(rect, new Color(0.145f, 0.145f, 0.145f, 1f));
            DrawTrackGrid(rect);

            float startX = rect.x + rect.width * Mathf.Clamp01(timing.Start / timing.TimelineEnd);
            float endX = rect.x + rect.width * Mathf.Clamp01(timing.Stop / timing.TimelineEnd);
            if (endX < startX + 2f) endX = Mathf.Min(rect.xMax, startX + 2f);

            Rect clipRect = Rect.MinMaxRect(startX, rect.y + 1f, endX, rect.yMax - 1f);
            EditorGUI.DrawRect(
                clipRect,
                timing.IsAsync
                    ? new Color(0.95f, 0.48f, 0.19f, 0.95f)
                    : new Color(0.31f, 0.64f, 0.95f, 0.95f));

            if (timing.IsAsync)
                DrawAsyncStripes(clipRect);

            EditorGUI.DrawRect(new Rect(startX, rect.y, 1f, rect.height), new Color(1f, 1f, 1f, 0.7f));
            EditorGUI.DrawRect(new Rect(endX, rect.y, 1f, rect.height), new Color(0.9f, 0.25f, 0.25f, 0.85f));

            string timelineLabel;
            if (!timing.HasEffect)
                timelineLabel = "No Effect";
            else if (timing.IsAsync)
                timelineLabel = $"ASYNC {timing.Start:0.##}s -> {timing.Stop:0.##}s";
            else
                timelineLabel = $"{timing.Start:0.##}s -> {timing.Stop:0.##}s";

            EditorGUI.LabelField(rect, timelineLabel, EditorStyles.centeredGreyMiniLabel);
        }

        private static void DrawTrackGrid(Rect rect)
        {
            Color gridColor = new Color(1f, 1f, 1f, 0.07f);
            for (int tick = 1; tick <= 4; tick++)
            {
                float x = rect.x + rect.width * (tick / 5f);
                EditorGUI.DrawRect(new Rect(x, rect.y, 1f, rect.height), gridColor);
            }
        }

        private static void DrawAsyncStripes(Rect rect)
        {
            if (Event.current.type != EventType.Repaint) return;

            Handles.BeginGUI();
            Color previousColor = Handles.color;
            Handles.color = new Color(1f, 1f, 1f, 0.22f);

            const float stripeStep = 6f;
            for (float x = rect.x - rect.height; x < rect.xMax; x += stripeStep)
                Handles.DrawLine(new Vector3(x, rect.yMax), new Vector3(x + rect.height, rect.y));

            Handles.color = previousColor;
            Handles.EndGUI();
        }

        private void CacheChoicesIfNotAlreadyCached()
        {
            if (_choices != null) return;

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
            if (effectTypeIndex <= 0 || effectTypeIndex > _availableEffectTypes.Length)
            {
                effectProperty.managedReferenceValue = new NoEffect();
                effectTypeProperty.stringValue = "None";
            }
            else
            {
                Type effectType = _availableEffectTypes[effectTypeIndex - 1];
                object effect = Activator.CreateInstance(effectType);
                effectProperty.managedReferenceValue = effect;
                effectTypeProperty.stringValue = effectType.Name;
            }
        }

        private static int DeserializeEffectType(SerializedProperty property, out SerializedProperty effectTypeProperty)
        {
            int effectTypeIndex = 0;
            effectTypeProperty = property.FindPropertyRelative("effectType");
            if (!EffectTypeIsNone(effectTypeProperty))
            {
                string? effectTypeName = effectTypeProperty.stringValue;
                Type? effectType = _availableEffectTypes.FirstOrDefault(type => type.Name == effectTypeName);
                if (effectType == null) return -1;
                effectTypeIndex = Array.IndexOf(_availableEffectTypes, effectType) + 1;
            }

            return effectTypeIndex;
        }

        private static bool EffectTypeIsNone(SerializedProperty effectTypeProperty)
        {
            return effectTypeProperty.stringValue == "None" || effectTypeProperty.stringValue == string.Empty;
        }

        private static bool HasBeenDuplicated(SerializedProperty property, SerializedProperty? fxItemsProperty)
        {
            if (fxItemsProperty is not { arraySize: > 1 }) return false;

            SerializedProperty fxItemPotentialDuplicateProperty =
                fxItemsProperty.GetArrayElementAtIndex(fxItemsProperty.arraySize - 1);
            if (fxItemPotentialDuplicateProperty.contentHash != property.contentHash) return false;

            SerializedProperty fxItemDuplicatedProperty =
                fxItemsProperty.GetArrayElementAtIndex(fxItemsProperty.arraySize - 2);
            bool hasBeenDuped = fxItemPotentialDuplicateProperty.contentHash == fxItemDuplicatedProperty.contentHash;
            if (hasBeenDuped) return true;

            return false;
        }
    }
}
