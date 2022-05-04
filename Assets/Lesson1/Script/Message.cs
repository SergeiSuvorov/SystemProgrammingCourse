using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Message : MonoBehaviour
{
    private CancellationTokenSource cancelTokenSource;

    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _startButton;
    [SerializeField] private float _secondsCount;
    [SerializeField] private int _frameCount;

    void Start()
    {
        _cancelButton.onClick.AddListener(TaskCancle);
        _startButton.onClick.AddListener(TaskStart);
    }

    private async void TaskStart()
    {
        Debug.Log("Start " );
        cancelTokenSource = new CancellationTokenSource();
        CancellationToken cancelToken = cancelTokenSource.Token;

        var result = await WhatTaskFasterAsync(cancelToken, TaskSec(_secondsCount, cancelToken), TaskFrame(_frameCount, cancelToken));
        Debug.Log("Первая задача закончилась раньше? "+  result);

        cancelTokenSource.Dispose();

    }
    private void TaskCancle()
    {
        Debug.Log("Cancle");
        cancelTokenSource.Cancel();
    }

    public async Task TaskSec(float seconds, CancellationToken cancelToken)
    {
        while(seconds>0.01f)
        {
            if (cancelToken.IsCancellationRequested)
            {
                Debug.Log("Задача 1 прервана токеном.");
                return;
            }
           
            if (seconds > 0.5f)
            {
                await Task.Delay(500);
                seconds -= 0.5f;
            }
            else
            {
                await Task.Delay((int)(seconds*1000));
                seconds = 0f;
            }
        }
            Debug.Log("Превая задача завершена");
    }

    public async Task TaskSecWithCheck(float seconds, CancellationToken cancelToken)
    {
        
        //Task task= Task.Run(async delegate
        //{
        //    await Task.Delay((int)(seconds*1000), cancelToken);
        //});
        try
        {
            //await task.Wait();
            await Task.Delay((int)(seconds * 1000), cancelToken);
        }
        catch (AggregateException ae)
        {
            foreach (var e in ae.InnerExceptions)
            {
                Debug.Log(string.Format("{0}: {1}", e.GetType().Name, e.Message));
            }
        }
        finally
        {
            if (!cancelToken.IsCancellationRequested)
                Debug.Log("Превая задача завершена");
            else
                Debug.Log("Задача 1 прервана токеном.");

        }
    }

    public async Task TaskFrame(int frameCount, CancellationToken cancelToken)
    {
        for (int i = 0; i < frameCount; i++)
        {
            if (cancelToken.IsCancellationRequested)
            {
                Debug.Log("Задача 2 прервана токеном.");
                return;
            }
            await Task.Yield();
        }
        Debug.Log("Вторая задача завершена");
    }

    public async static Task<bool> WhatTaskFasterAsync(CancellationToken cancelToken, Task task1, Task task2)
    {
        Task t1 = Task.Run(() => task1);
        Task t2 = Task.Run(() => task2);

        Task finishedTask = await Task.WhenAny(t1, t2);
        if (!cancelToken.IsCancellationRequested)
        {
            return (finishedTask == t1);
        }
        else
            return false;
    }
}
