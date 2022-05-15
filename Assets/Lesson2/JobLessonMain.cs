using UnityEngine;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;

public class JobLessonMain : MonoBehaviour
{
    [SerializeField] private Vector3[] _startPositions;
    [SerializeField] private Vector3[] _startVelocities;

    [SerializeField] private int[] _intArray;

    [SerializeField] private Transform[] _gameObjectsTransform;
    [SerializeField] private float turnSpeed;

    private TransformAccessArray _transformAccessArray;
    private TurnTransformJob _turnTransformJob;

    void Start()
    {
        ExecuteChangeArrayValue();
        ExecuteCalculateFinalPosition();

        CreateTurnTransformJob();
    }

    private void Update()
    {
        ExecuteTurnTransform();
    }

    private void OnDestroy()
    {
        if (_transformAccessArray.isCreated)
            _transformAccessArray.Dispose();
    }

    private void ExecuteChangeArrayValue()
    {
        Debug.Log("Task №1");

        NativeArray<int> intArray = new NativeArray<int>(_intArray, Allocator.Persistent);
        ChangeArrayValueJob changeArrayValueJob = new ChangeArrayValueJob(intArray);

        JobHandle handle = changeArrayValueJob.Schedule();
        handle.Complete();

        Debug.Log("Array Value After Change");
        for (int i = 0; i < intArray.Length; i++)
        {
            Debug.Log(intArray[i].ToString());
        }

        intArray.Dispose();
    }
    private void ExecuteCalculateFinalPosition()
    {
        Debug.Log("Task №2");

        int commonLenght = _startPositions.Length <= _startVelocities.Length ?  _startPositions.Length :  _startVelocities.Length;

        NativeArray<Vector3> finalPositions = new NativeArray<Vector3>(new Vector3[commonLenght], Allocator.Persistent);
        NativeArray<Vector3> startPositions = new NativeArray<Vector3>(_startPositions, Allocator.Persistent);
        NativeArray<Vector3> startVelocities = new NativeArray<Vector3>(_startVelocities, Allocator.Persistent);

        CalculateFinalPositionJob calculateFinalPositionJob = new CalculateFinalPositionJob(startPositions, startVelocities, finalPositions);
        JobHandle handle = calculateFinalPositionJob.Schedule(commonLenght, 0);

        handle.Complete();

        Debug.Log("Final Position");
        for (int i = 0; i < finalPositions.Length; i++)
        {
            Debug.Log(finalPositions[i].ToString());
        }
       
        startPositions.Dispose();
        startVelocities.Dispose();
        finalPositions.Dispose();
    }

    private void CreateTurnTransformJob()
    {
        _transformAccessArray = new TransformAccessArray(_gameObjectsTransform);
        _turnTransformJob = new TurnTransformJob(turnSpeed, Time.deltaTime);
    }

    private void ExecuteTurnTransform()
    {
        _turnTransformJob = new TurnTransformJob(turnSpeed, Time.deltaTime);
        JobHandle jobHandle = _turnTransformJob.Schedule(_transformAccessArray);
        jobHandle.Complete();
    }

}

public struct ChangeArrayValueJob : IJob
{
    public NativeArray<int> _nativeArray;

    public ChangeArrayValueJob(NativeArray<int> nativeArray)
    {
        _nativeArray = nativeArray;
    }

    public void Execute()
    {
        for(int i=0; i< _nativeArray.Length; i++)
        {
            _nativeArray[i] = _nativeArray[i]>10? 0: _nativeArray[i];
        }
    }
}

public struct CalculateFinalPositionJob : IJobParallelFor
{
    private NativeArray<Vector3> _positions;
    private NativeArray<Vector3> _velocities;
    private NativeArray<Vector3> _finalPositions;

    public CalculateFinalPositionJob(NativeArray<Vector3> positions, NativeArray<Vector3> velocities, NativeArray<Vector3> finalPositions)
    {
        _positions = positions;
        _velocities = velocities;
        _finalPositions = finalPositions;
    }

    public void Execute(int index)
    {
        _finalPositions[index] = _positions[index] + _velocities[index];
    }
}

public struct TurnTransformJob : IJobParallelForTransform
{
    private float _turnSpeed;
    private float _deltaTime;

    public TurnTransformJob(float turnSpeed, float deltaTime)
    {
        _turnSpeed = turnSpeed;
        _deltaTime = deltaTime;
    }

    public void Execute(int index, TransformAccess transform)
    {
        float turnAngle = transform.rotation.eulerAngles.y + _turnSpeed * _deltaTime;
        Quaternion quaternion = Quaternion.Euler(0, turnAngle, 0);

        transform.rotation = quaternion;
    }
}
