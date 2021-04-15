using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MHLab.Utilities;

public class CoinColliderASA : MonoBehaviour
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
        //Using BackgroundTasksProcessor
        Processor.Process(
            () =>
            {
                AlgorandManager.Instance.PayPlayerwithAlgorandASAFunction();
                return "Algorand ASA OK";
            },
            (result) =>
            {
                Debug.Log(result);
            }
        );
    }
}
