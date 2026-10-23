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
        private Mesh _footprintMesh;

        public SkillPlacementPreview(GameObject prefab, float clearanceRadius)
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
            if (_renderers.Count == 0) CreateFootprint(clearanceRadius);
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
            if (_footprintMesh != null) Object.Destroy(_footprintMesh);
        }

        private void CreateFootprint(float radius)
        {
            const int segments = 32;
            float size = Mathf.Max(0.05f, radius);
            var vertices = new Vector3[segments + 2];
            var triangles = new int[segments * 3];
            vertices[0] = new Vector3(0f, 0.03f, 0f);
            for (int i = 0; i <= segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                vertices[i + 1] = new Vector3(Mathf.Cos(angle) * size, 0.03f,
                    Mathf.Sin(angle) * size);
                if (i == segments) continue;
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 2;
                triangles[i * 3 + 2] = i + 1;
            }

            _footprintMesh = new Mesh { name = "Skill Placement Footprint" };
            _footprintMesh.vertices = vertices;
            _footprintMesh.triangles = triangles;
            _footprintMesh.RecalculateNormals();
            _footprintMesh.RecalculateBounds();

            var marker = new GameObject("Placement Footprint");
            marker.transform.SetParent(_root.transform, false);
            // 프리팹 루트가 축소되어 있어도 지정한 반지름을 월드 크기로 표시한다.
            Vector3 rootScale = _root.transform.localScale;
            marker.transform.localScale = new Vector3(
                1f / Mathf.Max(Mathf.Abs(rootScale.x), 0.0001f),
                1f / Mathf.Max(Mathf.Abs(rootScale.y), 0.0001f),
                1f / Mathf.Max(Mathf.Abs(rootScale.z), 0.0001f));
            marker.AddComponent<MeshFilter>().sharedMesh = _footprintMesh;
            MeshRenderer renderer = marker.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = _material;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            _renderers.Add(renderer);
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
