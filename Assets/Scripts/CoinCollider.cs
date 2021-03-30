using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityToolbag;

public class CoinCollider : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(AlgorandManager.Instance.GetPlayerName());
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
        //UnityMainThreadDispatcher.Instance().Enqueue(AlgorandManager.Instance.PlayerWithAlgorandLoopCoroutine()); //ABBASTANZA OK!
        UnityMainThreadDispatcher.Instance().EnqueueAsync(() => AlgorandManager.Instance.PayPlayerwithAlgorandFunction()); //INSOMMA!
        //Dispatcher.InvokeAsync(() => AlgorandManager.Instance.PayPlayerwithAlgorandFunction());
        //Dispatcher.InvokeAsync(() => AlgorandManager.Instance.PayPlayerWithAlgorand());
        /*
        RxDispatcher.Instance.Enqueue(() =>
        {
            AlgorandManager.Instance.PayPlayerwithAlgorandFunction();
        });
        */
    }
}
