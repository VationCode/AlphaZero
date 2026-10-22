using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace alpha.ingame.player
{
    /// <summary>프리팹의 외형만 복제해 설치 가능 여부를 반투명 홀로그램으로 표시한다.</summary>
    public sealed class SkillPlacementPreview
    {
        private readonly List<Renderer> _renderers = new();
        private readonly MaterialPropertyBlock _properties = new();
        private readonly GameObject _root;
        private readonly Material _material;

        public SkillPlacementPreview(GameObject prefab)
        {
            if (prefab == null) return;

            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                Debug.LogWarning("스킬 홀로그램용 URP Unlit 셰이더를 찾을 수 없습니다.");
                return;
            }

            _material = new Material(shader) { name = "Skill Placement Hologram" };
            _material.SetFloat("_Surface", 1f);
            _material.SetFloat("_Blend", 0f);
            _material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
            _material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            _material.SetInt("_ZWrite", 0);
            _material.SetOverrideTag("RenderType", "Transparent");
            _material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            _material.renderQueue = (int)RenderQueue.Transparent;

            _root = new GameObject(prefab.name + " Hologram");
            _root.SetActive(false);
            CopyVisual(prefab.transform, _root.transform);
        }

        public void Show(Pose pose, bool hasTarget, bool canPlace)
        {
            if (_root == null) return;
            if (_root.activeSelf != hasTarget) _root.SetActive(hasTarget);
            if (!hasTarget) return;

            _root.transform.SetPositionAndRotation(pose.position, pose.rotation);

            Color color = canPlace ? new Color(0.1f, 0.8f, 1f, 0.45f)
                : new Color(1f, 0.18f, 0.12f, 0.55f);
            color.a *= 0.85f + 0.15f * Mathf.Sin(Time.time * 5f);
            _properties.SetColor("_BaseColor", color);
            foreach (Renderer renderer in _renderers)
                renderer.SetPropertyBlock(_properties);
        }

        public void Dispose()
        {
            if (_root != null) Object.Destroy(_root);
            if (_material != null) Object.Destroy(_material);
        }

        private void CopyVisual(Transform source, Transform destination)
        {
            destination.localPosition = source.localPosition;
            destination.localRotation = source.localRotation;
            destination.localScale = source.localScale;

            MeshFilter sourceFilter = source.GetComponent<MeshFilter>();
            MeshRenderer sourceRenderer = source.GetComponent<MeshRenderer>();
            if (sourceFilter != null && sourceRenderer != null)
            {
                destination.gameObject.AddComponent<MeshFilter>().sharedMesh = sourceFilter.sharedMesh;
                MeshRenderer renderer = destination.gameObject.AddComponent<MeshRenderer>();
                renderer.enabled = sourceRenderer.enabled;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;

                int materialCount = Mathf.Max(1, sourceRenderer.sharedMaterials.Length);
                Material[] materials = new Material[materialCount];
                for (int i = 0; i < materialCount; i++) materials[i] = _material;
                renderer.sharedMaterials = materials;
                _renderers.Add(renderer);
            }

            foreach (Transform sourceChild in source)
            {
                GameObject child = new GameObject(sourceChild.name);
                child.transform.SetParent(destination, false);
                CopyVisual(sourceChild, child.transform);
                child.SetActive(sourceChild.gameObject.activeSelf);
            }
        }
    }
}
