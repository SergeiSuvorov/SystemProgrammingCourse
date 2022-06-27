using UnityEngine;

public class Asterroid : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;

    [SerializeField, Range(1, 8)] private int _depth = 4;

    [SerializeField, Range(1, 360)] private int _rotationSpeed;

    [SerializeField, Range(1, 360)] private int _angleOfset;
    private FractalPart[][] _parts;

    private const float _positionOffset = 5.75f;
    private const float _scaleBias = .5f;
    private int _childCount = 5;

    private struct FractalPart
    {
        public Transform Transform;
        public float Ang;
    }


    private static readonly Vector3[] _directions = new Vector3[]
    {
        Vector3.up,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back,
    };

    private static readonly Quaternion[] _rotations = new Quaternion[]
    {
        Quaternion.identity,
        Quaternion.Euler(0f, 0f, 90f),
        Quaternion.Euler(0f, 0f, -90f),
        Quaternion.Euler(90f, 0f, 0f),
        Quaternion.Euler(-90f, 0f, 0f),
    };


    private void OnEnable()
    {
        _parts = new FractalPart[_depth][];

        for (int i = 0; i < _parts.Length; i++)
        {
            _parts[i] = new FractalPart[_childCount];
        }

        for (var li = 0; li < _parts.Length; li++)
        {
           
            var levelParts = _parts[li];
            for (var fpi = 0; fpi < levelParts.Length; fpi += _childCount)
            {
                for (var ci = 0; ci < _childCount; ci++)
                {
                    var scale = Random.value;
                    levelParts[fpi + ci] = CreatePart(li, ci, scale);
                }
            }
        }
    }
    // Update is called once per frame
    private void Update()
    {
        var deltaRotation = Quaternion.Euler(0f, _rotationSpeed * Time.deltaTime, 0f);
        var rootPart = _parts[0][0];

        _parts[0][0] = rootPart;
        var ang = _angleOfset;
        for (var li = 0; li < _parts.Length; li++)
        {
           
            var levelParts = _parts[li];
            for (var fpi = 0; fpi < levelParts.Length; fpi++)
            {
                var parentTransform = transform;
                var part = levelParts[fpi];

                part.Ang += _angleOfset*Time.deltaTime;
                part.Transform.localPosition = parentTransform.localPosition + _positionOffset * (new Vector3(Mathf.Sin(part.Ang * Mathf.Deg2Rad), Mathf.Cos(part.Ang * Mathf.Deg2Rad), 0f));
                levelParts[fpi] = part;
            }
        }

    }
 
private FractalPart CreatePart(int levelIndex, int childIndex, float scale)
    {
        var ang = (360 / (_depth * _childCount)) * ((levelIndex + 1) * _childCount + (childIndex + 1));
        var go = new GameObject($"Fractal Path L{levelIndex} C{childIndex} Ang{ang}" );
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().material = material;

        go.transform.localScale = scale * Vector3.one;
       
        go.transform.localPosition = (_positionOffset * (new Vector3(Mathf.Sin(ang * Mathf.Deg2Rad), Mathf.Cos(ang * Mathf.Deg2Rad), 0f)));

        return new FractalPart()
        {
            Transform = go.transform,
            Ang = ang
        };

    }

}