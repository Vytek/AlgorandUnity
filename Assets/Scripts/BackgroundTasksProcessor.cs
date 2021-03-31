/*
The MIT License (MIT)

Copyright (c) 2021 Emanuele Manzione

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace MHLab.Utilities
{
    public class BackgroundTasksProcessor : MonoBehaviour
    {
        private class BackgroundTask
        {
            public Func<object>   Task;
            public object         Result;
            public Action<object> OnComplete;
        }

        private CancellationTokenSource _cancellationTokenSource;

        private ConcurrentQueue<BackgroundTask> _inputQueue;
        private ConcurrentQueue<BackgroundTask> _outputQueue;

        public bool IsReady { get; private set; }

        public int FrequencyInHz = 10;

        private void OnEnable()
        {
            InitializeQueues();

            StartBackgroundProcessor();
        }

        private void OnDisable()
        {
            StopBackgroundProcessor();
        }

        private void InitializeQueues()
        {
            if (_inputQueue == null)
                _inputQueue = new ConcurrentQueue<BackgroundTask>();
            else
                while (!_inputQueue.IsEmpty)
                    _inputQueue.TryDequeue(out _);

            if (_outputQueue == null)
                _outputQueue = new ConcurrentQueue<BackgroundTask>();
            else
                while (!_outputQueue.IsEmpty)
                    _outputQueue.TryDequeue(out _);
        }

        private void StartBackgroundProcessor()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Factory.StartNew(
                RunBackgroundProcessor,
                _cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning
            );
        }

        private void StopBackgroundProcessor()
        {
            _cancellationTokenSource.Cancel();
        }

        private void RunBackgroundProcessor(object obj)
        {
            Debug.Log("Background Processor has been started.");

            var token = (CancellationToken) obj;
            var timer = new Stopwatch();

            var targetWaitingTime = 1000 / FrequencyInHz;

            IsReady = true;

            while (true)
            {
                timer.Restart();

                if (token.IsCancellationRequested)
                {
                    IsReady = false;
                    break;
                }
                
                while (!_inputQueue.IsEmpty)
                {
                    if (_inputQueue.TryDequeue(out var currentTask))
                    {
                        Task.Run(() =>
                        {
                            var result = currentTask.Task.Invoke();
                            currentTask.Result = result;

                            _outputQueue.Enqueue(currentTask);
                        });
                    }
                }
                
                var spentTime     = (int) timer.ElapsedMilliseconds;
                var remainingTime = targetWaitingTime - spentTime;

                if (remainingTime > 0)
                    Thread.Sleep(remainingTime);
            }
            
            _cancellationTokenSource.Dispose();
            Debug.Log("Background Processor has been stopped.");
        }

        private void Update()
        {
            while (!_outputQueue.IsEmpty)
            {
                if (_outputQueue.TryDequeue(out var completedTask))
                {
                    var result = completedTask.Result;
                    completedTask.OnComplete.Invoke(result);
                }
            }
        }

        public void Process(Func<object> task, Action<object> onComplete)
        {
            if (task == null || onComplete == null)
                throw new ArgumentException("Methods passed in cannot be null");

            var result = new BackgroundTask()
            {
                Task       = task,
                OnComplete = onComplete
            };

            _inputQueue.Enqueue(result);
        }
    }
}