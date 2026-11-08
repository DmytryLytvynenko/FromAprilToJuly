using UnityEngine;

namespace DestructableFigures
{
    [RequireComponent(typeof(DestructableFigure))]
    public class FigureBuilder : MonoBehaviour
    {
        [SerializeField] private FigureType _type = FigureType.Cube;
        [SerializeField] private GameObject _samplePart;
        [SerializeField] private Transform _center;
        private DestructablePart[] _parts;

        [SerializeField] private int _cubeSize = 3;
        [SerializeField] private float _cubeStep = 1f;

        private const string PartsRootName = "Parts";

        public void BuildFigure()
        {
            ClearParts();

            Transform partsRoot = new GameObject(PartsRootName).transform;
            partsRoot.SetParent(transform, false);

            switch (_type)
            {
                case FigureType.Cube:
                    BuildCube(partsRoot);
                    break;
                case FigureType.Cone:
                case FigureType.Pyramid:
                    break;
            }
            if (_parts != null || _parts.Length != 0)
            {
                gameObject.GetComponent<DestructableFigure>().SetParts(_parts);
            }
            else
            {
                Debug.LogError("_parts is null or empty after figure building");
            }

        }

        private void BuildCube(Transform parent)
        {
            _parts = new DestructablePart[_cubeSize * _cubeSize * _cubeSize];
            int counter = 0;

            Vector3 scale = _samplePart.transform.localScale;
            _cubeStep = Mathf.Max(_cubeStep, Mathf.Min(scale.x, scale.y, scale.z));

            float offset = (_cubeSize - 1) * _cubeStep * 0.5f;
            Vector3 origin = transform.position;
            _center.position = origin + Vector3.up * offset;
            Vector3 start = origin - new Vector3(offset, 0f, offset);

            for (int i = 0; i < _cubeSize; i++)
                for (int j = 0; j < _cubeSize; j++)
                    for (int k = 0; k < _cubeSize; k++)
                    {
                        Vector3 pos = start + new Vector3(k, i, j) * _cubeStep;
                        _parts[counter] = Instantiate(_samplePart, pos, Quaternion.identity, parent).GetComponent<DestructablePart>();
                        counter++;
                    }
        }

        private void ClearParts()
        {
            Transform old = transform.Find(PartsRootName);
            if (old == null) return;

            if (Application.isPlaying) Destroy(old.gameObject);
            else DestroyImmediate(old.gameObject);
        }
    }

    public enum FigureType
    {
        Cube,
        Cone,
        Pyramid
    }
}