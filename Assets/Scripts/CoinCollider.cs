using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MHLab.Utilities;

public class CoinCollider : MonoBehaviour
{
    public BackgroundTasksProcessor Processor;
    
    public GameObject Coin;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Player Name: "+AlgorandManager.Instance.GetPlayerName());
    }

    void Update()
    {
        if (!Processor.IsReady)
        {
            return;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject otherObj = collision.gameObject;
        Debug.Log("Collided with: " + otherObj);
    }

    void OnTriggerEnter(Collider collider)
    {
        GameObject otherObj = collider.gameObject;
        Debug.Log("Triggered with: " + otherObj);
        //Star Algorand Transaction
        //AlgorandManager.Instance.PayPlayerWithAlgorand();
        //USING DISPATCHER
        //UnityMainThreadDispatcher.Instance().Enqueue(AlgorandManager.Instance.PlayerWithAlgorandLoopCoroutine()); //VERY GOOD!
        //UnityMainThreadDispatcher.Instance().EnqueueAsync(() => AlgorandManager.Instance.PayPlayerwithAlgorandFunction()); //NOT GOOD!
        //UnityMainThreadDispatcher.Instance().EnqueueAsync(() => AlgorandManager.Instance.PayPlayerWithAlgorand()); //NOT GOOD!
        //Dispatcher.InvokeAsync(() => AlgorandManager.Instance.PayPlayerwithAlgorandFunction()); //TRY?
        //Dispatcher.InvokeAsync(() => AlgorandManager.Instance.PayPlayerWithAlgorand()); //TRY?
        /* TEST USING UNIRX
        RxDispatcher.Instance.Enqueue(() =>
        {
            AlgorandManager.Instance.PayPlayerwithAlgorandFunction();
        });
        */

        //Using BackgroundTasksProcessor
        Processor.Process(
            () =>
            {
                AlgorandManager.Instance.PayPlayerwithAlgorandFunction();
                return "Algorand OK";
            },
            (result) =>
            {
                Debug.Log(result);
            }
        );
    }
}
