using System;
using alpha.system;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace alpha.ingame.enemy.boss.editor
{
    /// <summary>공격 이벤트별 이펙트, 생성 위치와 수명을 인스펙터에 표시한다.</summary>
    [CustomEditor(typeof(BossAttackEffectView))]
    public sealed class BossAttackEffectViewEditor : Editor
    {
        private ReorderableList _list;

        private void OnEnable()
        {
            SerializedProperty bindings = serializedObject.FindProperty("_bindings");
            if (bindings.arraySize == 0 && targets.Length == 1)
                ConnectAttack1(bindings);

            _list = new ReorderableList(serializedObject, bindings, true, true, true, true);
            _list.drawHeaderCallback = DrawHeader;
            _list.drawElementCallback = DrawElement;
            _list.elementHeightCallback = GetElementHeight;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            _list.DoLayoutList();
            if (_list.count == 0)
                EditorGUILayout.HelpBox("Attack1 이펙트를 찾지 못했습니다. 배치된 인스턴스를 연결하세요.",
                    MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
        }

        private void ConnectAttack1(SerializedProperty bindings)
        {
            BossAttackEffectView view = (BossAttackEffectView)target;
            Transform model = view.transform.root.Find("Model");
            if (model == null) return;

            GameObject effect1 = null;
            GameObject effect2 = null;
            GameObject smallClawTrail = null;
            foreach (Transform child in model.GetComponentsInChildren<Transform>(true))
                if (child.name == "Attack1Effect") effect1 = child.gameObject;

            foreach (TrailRenderer trail in model.GetComponentsInChildren<TrailRenderer>(true))
            {
                if (trail.name.EndsWith("Effect2Trail", StringComparison.Ordinal))
                    effect2 = trail.gameObject;
                if (trail.name == "Attack1SmallClawTrail")
                    smallClawTrail = trail.gameObject;
            }

            GameObject effect3 = view.transform.Find("Attack1Pos/Effect3")?.gameObject;
            if (effect1 == null || effect2 == null || effect3 == null) return;

            bindings.arraySize = smallClawTrail != null ? 4 : 3;
            SetBinding(bindings.GetArrayElementAtIndex(0), "Attack1", "Effect1", effect1);
            SetBinding(bindings.GetArrayElementAtIndex(1), "Attack1", "Effect2", effect2);
            SetBinding(bindings.GetArrayElementAtIndex(2), "Attack1", "Effect3", effect3);
            if (smallClawTrail != null)
                SetBinding(bindings.GetArrayElementAtIndex(3), "Attack1", "Effect4", smallClawTrail);
            serializedObject.ApplyModifiedProperties();

            BossAnimationView animationView = view.transform.root.GetComponentInChildren<BossAnimationView>(true);
            if (animationView != null)
            {
                SerializedObject animation = new SerializedObject(animationView);
                animation.FindProperty("_attackEffects").objectReferenceValue = view;
                animation.ApplyModifiedProperties();
            }

            EditorSceneManager.MarkSceneDirty(view.gameObject.scene);
        }

        private static void SetBinding(SerializedProperty binding, string attackId,
            string eventKey, GameObject effect)
        {
            binding.FindPropertyRelative("_attackId").stringValue = attackId;
            binding.FindPropertyRelative("_eventKey").stringValue = eventKey;
            binding.FindPropertyRelative("_effectObject").objectReferenceValue = effect;
            binding.FindPropertyRelative("_mode").enumValueIndex = 0;
            binding.FindPropertyRelative("_spawnPositionMode").enumValueIndex = 0;
            binding.FindPropertyRelative("_spawnPoints").ClearArray();
            binding.FindPropertyRelative("_spawnLifetime").floatValue = 2f;
        }

        private static void DrawHeader(Rect rect)
        {
            GetColumns(rect, out Rect attack, out Rect eventKey, out Rect effect, out Rect mode);
            EditorGUI.LabelField(attack, "Attack ID");
            EditorGUI.LabelField(eventKey, "Event Key");
            EditorGUI.LabelField(effect, "Effect / Prefab");
            EditorGUI.LabelField(mode, "Mode");
        }

        private float GetElementHeight(int index)
        {
            SerializedProperty binding = _list.serializedProperty.GetArrayElementAtIndex(index);
            bool spawn = binding.FindPropertyRelative("_mode").enumValueIndex == 1;
            float height = EditorGUIUtility.singleLineHeight + 6f;
            if (!spawn) return height;

            height += EditorGUIUtility.singleLineHeight + 2f;
            if (binding.FindPropertyRelative("_spawnPositionMode").enumValueIndex == 2)
                height += EditorGUI.GetPropertyHeight(
                    binding.FindPropertyRelative("_spawnPoints"), true) + 2f;
            if (!HasObjectMove(binding))
                height += EditorGUIUtility.singleLineHeight + 2f;
            height += EditorGUIUtility.singleLineHeight + 2f;
            height += EditorGUIUtility.singleLineHeight + 2f;
            return height;
        }

        private void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            SerializedProperty binding = _list.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2f;
            rect.height = EditorGUIUtility.singleLineHeight;
            GetColumns(rect, out Rect attack, out Rect eventKey, out Rect effect, out Rect mode);
            EditorGUI.PropertyField(attack, binding.FindPropertyRelative("_attackId"), GUIContent.none);
            EditorGUI.PropertyField(eventKey, binding.FindPropertyRelative("_eventKey"), GUIContent.none);
            EditorGUI.PropertyField(effect, binding.FindPropertyRelative("_effectObject"), GUIContent.none);
            EditorGUI.PropertyField(mode, binding.FindPropertyRelative("_mode"), GUIContent.none);
            if (binding.FindPropertyRelative("_mode").enumValueIndex != 1) return;

            Rect positionMode = new Rect(rect.x, rect.yMax + 2f, rect.width, rect.height);
            SerializedProperty spawnPositionMode = binding.FindPropertyRelative("_spawnPositionMode");
            EditorGUI.PropertyField(positionMode, spawnPositionMode,
                new GUIContent("Spawn Position",
                    "Targeting: 타겟 위치에 1개, Current: 현재 위치에 1개, Specified: 지정한 위치마다 1개, RandomTargetArea: 타겟 주변에 여러 개"));
            float nextY = positionMode.yMax + 2f;
            if (spawnPositionMode.enumValueIndex == 2)
            {
                SerializedProperty points = binding.FindPropertyRelative("_spawnPoints");
                Rect pointsRect = new Rect(rect.x, nextY, rect.width,
                    EditorGUI.GetPropertyHeight(points, true));
                EditorGUI.PropertyField(pointsRect, points,
                    new GUIContent("Spawn Points"), true);
                nextY = pointsRect.yMax + 2f;
            }
            if (!HasObjectMove(binding))
            {
                Rect lifetime = new Rect(rect.x, nextY, rect.width, rect.height);
                EditorGUI.PropertyField(lifetime, binding.FindPropertyRelative("_spawnLifetime"),
                    new GUIContent("Lifetime (s)"));
                nextY = lifetime.yMax + 2f;
            }
            Rect preserve = new Rect(rect.x, nextY, rect.width, rect.height);
            EditorGUI.PropertyField(preserve,
                binding.FindPropertyRelative("_preserveInactiveChildren"),
                new GUIContent("Preserve Inactive Children"));
            Rect randomYaw = new Rect(rect.x, preserve.yMax + 2f, rect.width, rect.height);
            EditorGUI.PropertyField(randomYaw, binding.FindPropertyRelative("_randomYaw"),
                new GUIContent("Random Y Rotation"));
        }

        private static bool HasObjectMove(SerializedProperty binding)
        {
            GameObject effect = binding.FindPropertyRelative("_effectObject").objectReferenceValue as GameObject;
            return effect != null && effect.GetComponent<ObjectMove>() != null;
        }

        private static void GetColumns(Rect rect, out Rect attack, out Rect eventKey,
            out Rect effect, out Rect mode)
        {
            const float gap = 4f;
            float keyWidth = (rect.width - gap * 3f) * 0.2f;
            attack = new Rect(rect.x, rect.y, keyWidth, rect.height);
            eventKey = new Rect(attack.xMax + gap, rect.y, keyWidth, rect.height);
            mode = new Rect(rect.xMax - keyWidth, rect.y, keyWidth, rect.height);
            effect = new Rect(eventKey.xMax + gap, rect.y,
                mode.xMin - eventKey.xMax - gap, rect.height);
        }
    }
}
