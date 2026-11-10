using UnityEngine;

namespace DestructableFigures
{
    public class FigureBuilderFromMesh : FigureBuilder
    {
        [SerializeField] private MeshCollider _collider;
        [SerializeField] private LayerMask _meshLayer;
        [SerializeField] private float _step = 0.25f;
        [SerializeField] private int _maxParts = 10000;

        public override void BuildFigure() => BuildFromMesh();

        public void BuildFromMesh()
        {
            ClearParts();

            Bounds b = _collider.bounds;
            Vector3 size = b.size;
            int nx = Mathf.CeilToInt(size.x / _step);
            int ny = Mathf.CeilToInt(size.y / _step);
            int nz = Mathf.CeilToInt(size.z / _step);

            // Верхняя оценка (по ограничивающему боксу), реальных частей будет меньше
            if (nx * ny * nz > _maxParts * 20)
                Debug.LogWarning($"Сетка {nx}x{ny}x{nz} = {nx * ny * nz} ячеек, расчёт может быть долгим");

            Transform parent = new GameObject(PartsRootName).transform;
            parent.SetParent(transform, false);

            Physics.SyncTransforms();
            bool prev = Physics.queriesHitBackfaces;
            Physics.queriesHitBackfaces = true;

            int count = 0;
            float maxDist = size.magnitude + 1f;

            try
            {
                for (int i = 0; i < ny; i++)
                    for (int j = 0; j < nx; j++)
                        for (int k = 0; k < nz; k++)
                        {
                            Vector3 p = b.min + new Vector3(j + 0.5f, i + 0.5f, k + 0.5f) * _step;
                            if (!IsInside(p, maxDist)) continue;

                            if (++count > _maxParts)
                            {
                                Debug.LogError($"Превышен лимит {_maxParts} частей. Увеличь шаг или лимит.");
                                return;
                            }

                            var part = Instantiate(_samplePart, p, Quaternion.identity, parent);
                        }
            }
            finally
            {
                Physics.queriesHitBackfaces = prev;
            }

            if (_center) _center.position = b.center;
            GetComponent<DestructableFigure>()
                .SetParts(parent.GetComponentsInChildren<DestructablePart>());

            Debug.Log($"Построено частей: {count}");
        }

        private bool IsInside(Vector3 p, float maxDist)
        {
            Vector3 dir = Vector3.up;
            return Physics.Raycast(p, dir, out var hit, maxDist, _meshLayer, QueryTriggerInteraction.Ignore)
                && hit.collider == _collider
                && Vector3.Dot(hit.normal, dir) > 0f;
        }
    }
}