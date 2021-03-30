using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class RxDispatcher
{
    private IReactiveCollection<Action> ActionQueue = new ReactiveCollection<Action>();
    //private IList<Action> ActionQueue = new List<Action>();

    private static RxDispatcher _instance;

    // Typical Unity design for a static instance makes this a quasi-singleton
    public static RxDispatcher Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new RxDispatcher(); // Instantiate singleton on first use.

                // observe on the Add event and main thread
                _instance.ActionQueue
                    //.ToObservable()
                    .ObserveAdd()
                    //.ObserveOn(Scheduler.MainThread)
                    .Subscribe(action =>
                    {
                        action.Value(); // execute the function
                        _instance.ActionQueue.Remove(action.Value); // remove it from the queue, technically not necessary in every loop
                    });
            }

            return _instance;
        }
    }

    /// <summary>
    /// Schedule code for execution in the main-thread. 
    /// </summary>
    /// <param name="fn">The Action to be executed</param>
    public void Enqueue(Action fn)
    {
        if (ActionQueue == null) return; // defensive programming

        ActionQueue.Add(fn);
    }
}
