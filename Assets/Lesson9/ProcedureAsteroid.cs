using UnityEngine;

public class ProcedureAsteroid : MonoBehaviour
{
    //unity_ObjectToWorld._m30_m31_m32_m33 = float4(0.0, 0.0, 0.0, 1.0);

    struct FractalPart
    {
        public Vector3 Direction;
        public Quaternion Rotation;
        public Vector3 WorldPosition;
        public Quaternion WorldRotation;
        public float SpinAngle;
        public float Ang;
        public float Scale;
        public float Offset;
    }

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField, Range(1, 8)] private int _depth = 4;
    [SerializeField, Range(0, 360)] private int _speedRotation = 80;
    [SerializeField, Range(0, 360)] private int _radiusOffset = 4;
    private const float _radius = 5.5f;

    private const float _scaleBias = .5f;
    private const int _childCount = 5;
    private FractalPart[][] _parts;
    private Matrix4x4[][] _matrices;
    private ComputeBuffer[] _matricesBuffers;
    private static readonly int _matricesId = Shader.PropertyToID("_Matrices");
    private static MaterialPropertyBlock _propertyBlock;
    private static readonly Vector3[] _directions =
    {
        Vector3.up,
        Vector3.left,
        Vector3.right,
        Vector3.forward,
        Vector3.back
    };

    private static readonly Quaternion[] _rotations =
    {
        Quaternion.identity,
        Quaternion.Euler(.0f, .0f, 90.0f),
        Quaternion.Euler(.0f, .0f, -90.0f),
        Quaternion.Euler(90.0f, .0f, .0f),
        Quaternion.Euler(-90.0f, .0f, .0f)
    };

    private void OnEnable()
    {
        _parts = new FractalPart[_depth][];
        _matrices = new Matrix4x4[_depth][];
        _matricesBuffers = new ComputeBuffer[_depth];
        var stride = 16 * 4;
        var length = _childCount;
        for (int i = 0; i < _parts.Length; i++)
        {
            _parts[i] = new FractalPart[length];
            _matrices[i] = new Matrix4x4[length];
            _matricesBuffers[i] = new ComputeBuffer(length, stride);
        }

        _parts[0][0] = CreatePart(0,0);

        for (var li = 1; li < _parts.Length; li++)
        {
            var levelParts = _parts[li];

            for (var fpi = 0; fpi < levelParts.Length; fpi += _childCount)
            {
                for (var ci = 0; ci < _childCount; ci++)
                {
                    levelParts[fpi + ci] = CreatePart(li,ci);
                }
            }
        }

        if (_propertyBlock == null)
            _propertyBlock = new MaterialPropertyBlock();
    }
    private void OnDisable()
    {
        for (var i = 0; i < _matricesBuffers.Length; i++)
        {
            _matricesBuffers[i].Release();
        }

        _parts = null;
        _matrices = null;
        _matricesBuffers = null;
    }
    private void OnValidate()
    {
        if (_parts is null || !enabled)
        {
            return;
        }
        OnDisable();
        OnEnable();
    }
    private FractalPart CreatePart(int levelIndex,int childIndex)
    {
        var ang = (360 / ((_depth) * _childCount)) * ((levelIndex+1) * _childCount + (childIndex + 1));

        return new FractalPart
        {
            Direction = _directions[childIndex],
            Rotation = _rotations[childIndex],
            Ang = ang,
            Scale =  Random.value,
            Offset = Random.value* _radiusOffset
        };
    }

    private void Update()
    {
        var spinAngelDelta = _speedRotation * Time.deltaTime;
        var rootPart = _parts[0][0];
        rootPart.SpinAngle += spinAngelDelta;
        var deltaRotation = Quaternion.Euler(.0f, rootPart.SpinAngle, .0f);
        rootPart.WorldRotation = rootPart.Rotation * deltaRotation;
        _parts[0][0] = rootPart;
        _matrices[0][0] = Matrix4x4.TRS(rootPart.WorldPosition,
        rootPart.WorldRotation, Vector3.one);
        //var scale = 1.0f/ _scaleBias;
        for (var li = 0; li < _parts.Length; li++)
        {
            //scale *= _scaleBias;
            var parentParts = _parts[li];
            if (li!=0)
            {
                parentParts = _parts[li - 1];
            }

            var levelParts = _parts[li];
            var levelMatrices = _matrices[li];

            for (var fpi = 0; fpi < levelParts.Length; fpi++)
            {
                var parent = parentParts[fpi / _childCount];
                var part = levelParts[fpi];
                part.SpinAngle += spinAngelDelta;
                part.Ang += spinAngelDelta;
                part.WorldPosition = (_radius+part.Offset) * (new Vector3(Mathf.Sin(part.Ang * Mathf.Deg2Rad), Mathf.Cos(part.Ang * Mathf.Deg2Rad), 0f));
                part.WorldRotation = parent.WorldRotation * (part.Rotation * Quaternion.Euler(0f, part.SpinAngle, 0f));
               
                levelParts[fpi] = part;
                levelMatrices[fpi] = Matrix4x4.TRS(part.WorldPosition, part.WorldRotation, part.Scale * Vector3.one);
            }
        }

        var bounds = new Bounds(rootPart.WorldPosition, 3f * Vector3.one);
        for (var i = 0; i < _matricesBuffers.Length; i++)
        {
            var buffer = _matricesBuffers[i];
            buffer.SetData(_matrices[i]);
            _propertyBlock.SetBuffer(_matricesId, buffer);
            _material.SetBuffer(_matricesId, buffer);
            Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, bounds, buffer.count, _propertyBlock);
        }
    }
}