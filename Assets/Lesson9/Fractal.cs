using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fractal : MonoBehaviour
{
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material material;

    [SerializeField, Range(1, 8)] private int _depth = 4;
    [SerializeField, Range(1, 360)] private int _rotationSpeed;
    private FractalPart[][] _parts;
    
    private const float _positionOffset = 2.75f;
    private const float _scaleBias = .5f;
    private int _childCount=5;

    private struct FractalPart
    {
        public Vector3 Direction;
        public Quaternion Rotation;
        public Transform Transform;
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

    // Start is called before the first frame update
    //void Start()
    //{
    //    name = "Fractal " + _depth;
    //    if (_depth <= 1)
    //    {
    //        return;
    //    }
    //    var childA = CreateChild(Vector3.up, Quaternion.identity);
    //    var childB = CreateChild(Vector3.right, Quaternion.Euler(0f, 0f, -90f));
    //    var childC = CreateChild(Vector3.left, Quaternion.Euler(0f, 0f, 90f));
    //    var childD = CreateChild(Vector3.forward, Quaternion.Euler(90f, 0f, 0f));
    //    var childE = CreateChild(Vector3.back, Quaternion.Euler(-90f, 0f, 0f));
    //    childA.transform.SetParent(transform, false);
    //    childB.transform.SetParent(transform, false);
    //    childC.transform.SetParent(transform, false);
    //    childD.transform.SetParent(transform, false);
    //    childE.transform.SetParent(transform, false);
    //}
    private void OnEnable()
    {
        _parts = new FractalPart[_depth][];

        for (int i = 0, length = 1; i < _parts.Length; i++, length *=
        _childCount)
        {
            _parts[i] = new FractalPart[length];
        }
        var scale = 1f;
        _parts[0][0] = CreatePart(0, 0, scale);

        for (var li = 1; li < _parts.Length; li++)
        {
            scale *= _scaleBias;
            var levelParts = _parts[li];
            for (var fpi = 0; fpi < levelParts.Length; fpi += _childCount)
            {
                for (var ci = 0; ci < _childCount; ci++)
            {
                    levelParts[fpi + ci] = CreatePart(li, ci, scale);
                }
            }
        }
    }
    // Update is called once per frame
    private void Update()
    {
        var deltaRotation = Quaternion.Euler(0f, _rotationSpeed * Time.deltaTime,0f);
        var rootPart = _parts[0][0];
        rootPart.Rotation *= deltaRotation;
        rootPart.Transform.localRotation = rootPart.Rotation;
        _parts[0][0] = rootPart;
        for (var li = 1; li < _parts.Length; li++)
        {
            var parentParts = _parts[li - 1];
            var levelParts = _parts[li];
            for (var fpi = 0; fpi < levelParts.Length; fpi++)
            {
                var parentTransform = parentParts[fpi / _childCount].Transform;
                var part = levelParts[fpi];
                part.Rotation *= deltaRotation;
                part.Transform.localRotation =   parentTransform.localRotation * part.Rotation;
                part.Transform.localPosition =   parentTransform.localPosition + parentTransform.localRotation * (_positionOffset * part.Transform.localScale.x *  part.Direction);
                levelParts[fpi] = part;
            }
        }
    }
    private Fractal CreateChild(Vector3 direction, Quaternion rotation)
    {
        var child = Instantiate(this);
        child._depth = _depth - 1;
        child.transform.localPosition = _positionOffset * direction;
        child.transform.localRotation = rotation;
        child.transform.localScale = _scaleBias * Vector3.one;
        return child;
    }
    private FractalPart CreatePart(int levelIndex, int childIndex, float scale)
    {

        var go = new GameObject($"Fractal Path L{levelIndex} C{childIndex}");
        go.transform.SetParent(transform, false);
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().material = material;
        go.transform.localScale = scale * Vector3.one;
        return new FractalPart()
        {
            Direction = _directions[childIndex],
            Rotation = _rotations[childIndex],
            Transform = go.transform
        };
    }
}
