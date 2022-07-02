using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using static Unity.Mathematics.math;
using float4x4 = Unity.Mathematics.float4x4;
using quaternion = Unity.Mathematics.quaternion;

public class AsteroidOrbit : NetworkBehaviour
{
    //unity_ObjectToWorld._m30_m31_m32_m33 = float4(0.0, 0.0, 0.0, 1.0);

   

    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SyncVar][SerializeField, Range(1, 20)] private int _depth = 4;
    //[SerializeField, Range(0, 360)] private int _speedRotation = 80;
    [SyncVar][SerializeField, Range(0, 4)] private float _speedRotation = .125f;

    [SerializeField, Range(0, 360)] private int _angleTurn = 80;
    [SyncVar] [SerializeField, Range(0, 360)] private int _radiusOffset = 4;
    [SyncVar] [SerializeField] private int _radius = 8;
    [SyncVar] [SerializeField] private int _maxScale = 20;
    [SyncVar] private SyncList<float> _partRadius = new SyncList<float>();
    [SyncVar] private SyncList<float> _partScale = new SyncList<float>();
    [SyncVar] private float _spinAngelDelta = 0;
    [SyncVar] private float _spinAngelResult = 0;
    // private const float _radius =5.75f;
    private const float _scaleBias = .5f;
    private const int _childCount = 5;

    private NativeArray<FractalPart>[] _parts;
    private NativeArray<Matrix4x4>[] _matrices;

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


    private static readonly quaternion[] _rotations =
            {
        quaternion.identity,
        quaternion.RotateZ(.5f * PI),
        quaternion.RotateZ(-.5f * PI),
        quaternion.RotateX(.5f * PI),
        quaternion.RotateX(-.5f * PI),
    };


    [BurstCompile(CompileSynchronously = true)]
    private struct UpdateFractalLevelJob : IJobFor
    {
        public float SpinAngleDelta;
        public float Scale;
        [ReadOnly]
        public NativeArray<FractalPart> Parents;
        public NativeArray<FractalPart> Parts;
        [WriteOnly]
        public NativeArray<Matrix4x4> Matrices;
        public void Execute(int index)
        {

            var parent = Parents[index / _childCount];
            var part = Parts[index];

            part.SpinAngle += SpinAngleDelta;
            part.Ang += SpinAngleDelta;

            part.WorldPosition = (part.Radius) * (new Vector3(Mathf.Sin(part.Ang * Mathf.Deg2Rad), 0f, Mathf.Cos(part.Ang * Mathf.Deg2Rad)));
            part.WorldRotation = parent.WorldRotation * (part.Rotation * Quaternion.Euler(0f, part.SpinAngle, 0f));

            Parts[index] = part;
            Matrices[index] = Matrix4x4.TRS(part.WorldPosition, part.WorldRotation, part.Scale * Vector3.one);
        }


    }

    struct FractalPart
    {
        public Vector3 Direction;
        public Quaternion Rotation;
        public Vector3 WorldPosition;
        public Quaternion WorldRotation;
        public float SpinAngle;
        public float Ang;
        public float Scale;
        public float Radius;
    }
    
    public void Init(int radius, int radiusOffset, float speed, int scale, int depth)
    {
        _radius = (radius > 0) ? radius : _radius;
        _radiusOffset = radiusOffset;
        _speedRotation = speed;
        _maxScale = (scale> _scaleBias)? scale: _maxScale;
        _depth = depth;

        for (var li = 0; li < _depth; li++)
        {

            for (var fpi = 0; fpi < _depth; fpi += _childCount)
            {
                for (var ci = 0; ci < _childCount; ci++)
                {
                    _partScale.Add(Random.value * _maxScale);
                    _partRadius.Add(Random.value * _radiusOffset + _radius);
                }
            }
        }
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        CreateAsteroids();
    }


    public override void OnStartClient()
    {
        base.OnStartClient();
        CreateAsteroids();
    }

    private void CreateAsteroids()
    {
        if (_parts != null || _matrices != null || _matricesBuffers != null)
            return;

        _parts = new NativeArray<FractalPart>[_depth];
        _matrices = new NativeArray<Matrix4x4>[_depth];
        _matricesBuffers = new ComputeBuffer[_depth];
        var stride = 16 * 4;
        var length = _childCount;
        for (int i = 0; i < _parts.Length; i++)
        {
            _parts[i] = new NativeArray<FractalPart>(length, Allocator.Persistent);
            _matrices[i] = new NativeArray<Matrix4x4>(length, Allocator.Persistent);
            _matricesBuffers[i] = new ComputeBuffer(length, stride);
        }

        for (var li = 0; li < _parts.Length; li++)
        {
            var levelParts = _parts[li];

            for (var fpi = 0; fpi < levelParts.Length; fpi += _childCount)
            {
                for (var ci = 0; ci < _childCount; ci++)
                {
                    levelParts[fpi + ci] = CreateAsteroid(li, ci, _partScale[fpi + ci], _partRadius[fpi + ci]);
                }
            }
        }

        if (_propertyBlock == null)
            _propertyBlock = new MaterialPropertyBlock();
    }

    
    private void OnDisable()
    {
        if (_matricesBuffers!=null)
        for (var i = 0; i < _matricesBuffers.Length; i++)
        {
            _matricesBuffers[i].Release();
            _parts[i].Dispose();
            _matrices[i].Dispose();
        }


        _parts = null;
        _matrices = null;
        _matricesBuffers = null;
    }

    private FractalPart CreateAsteroid(int levelIndex, int childIndex, float scale, float radius)
    {
        var ang = (360 / ((_depth - 1) * _childCount)) * ((levelIndex) * _childCount + (childIndex));

        return new FractalPart
        {
            Direction = _directions[childIndex],
            Rotation = _rotations[childIndex],
            Ang = ang,
            Scale = scale,
            Radius = radius
        };
    }

    [Server]
    private void ServerRotate()
    {
        _spinAngelDelta = _speedRotation * PI * Time.deltaTime;
        _spinAngelResult += _spinAngelDelta;
    }

    private void Rotate()
    {
        var rootPart = _parts[0][0];
        rootPart.SpinAngle = _spinAngelResult;
        var deltaRotation = Quaternion.Euler(.0f, rootPart.SpinAngle, .0f);
        rootPart.WorldRotation = deltaRotation;
        _parts[0][0] = rootPart;
        _matrices[0][0] = Matrix4x4.TRS(rootPart.WorldPosition, rootPart.WorldRotation, Vector3.one);
        var scale = 1.0f;

        JobHandle jobHandle = default;
        for (var li = 1; li < _parts.Length; li++)
        {
            scale *= _scaleBias;
            var parentIndex = li == 0 ? 0 : li - 1;
            jobHandle = new UpdateFractalLevelJob
            {
                SpinAngleDelta = _spinAngelDelta,
                Scale = scale,
                Parents = _parts[parentIndex],
                Parts = _parts[li],
                Matrices = _matrices[li]
            }.Schedule(_parts[li].Length, jobHandle);
        }
        jobHandle.Complete();

        var bounds = new Bounds(transform.position, 3f * Vector3.one);
        for (var i = 1; i < _matricesBuffers.Length; i++)
        {
            var buffer = _matricesBuffers[i];
            buffer.SetData(_matrices[i]);
            _propertyBlock.SetBuffer(_matricesId, buffer);
            _material.SetBuffer(_matricesId, buffer);
            Graphics.DrawMeshInstancedProcedural(_mesh, 0, _material, bounds,
            buffer.count, _propertyBlock);
        }
    }

    private void Update()
    {
        if(isServer)
        {
            ServerRotate();
        }
        Rotate();
    }
}