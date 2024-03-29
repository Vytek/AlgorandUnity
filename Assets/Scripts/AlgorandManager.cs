﻿using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

//ALGORAND
using Algorand;
using Algorand.V2;
using Algorand.Client;
using Algorand.V2.Model;
using Account = Algorand.Account;

public class AlgorandManager : Singleton<AlgorandManager>
{
    [SerializeField]
    protected string m_PlayerName;

    /// <summary>
    /// GetPlayerName()
    /// </summary>
    /// <returns>PlayerName</returns>
    public string GetPlayerName()
    {
        return m_PlayerName;
    }

    /// <summary>
    /// PayPlayerWithAlgorand()
    /// </summary>
    public void PayPlayerWithAlgorand()
    {
        //var task = CoroutineRunner.RunAsync(PlayerWithAlgorandLoopCoroutine());
        StartCoroutine(PlayerWithAlgorandLoopCoroutine());
    }

    //Function
    public void PayPlayerwithAlgorandFunction()
    {
        Debug.Log("Starting Algorand Transaction.");
        string ALGOD_API_ADDR = "https://testnet-algorand.api.purestake.io/ps2";
        if (ALGOD_API_ADDR.IndexOf("//") == -1)
        {
            ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
        }

        string ALGOD_API_TOKEN = "Your API Key Here";
        string SRC_ACCOUNT = "typical permit hurdle hat song detail cattle merge oxygen crowd arctic cargo smooth fly rice vacuum lounge yard frown predict west wife latin absent cup";
        string DEST_ADDR = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";
        if (!Address.IsValid(DEST_ADDR))
            Debug.LogError("The address " + DEST_ADDR + " is not valid!");
        Account src = new Account(SRC_ACCOUNT);
        Debug.Log("My account address is:" + src.Address.ToString());

        AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);

        TransactionParametersResponse transParams;
        try
        {
            transParams = algodApiInstance.TransactionParams();
        }
        catch (ApiException e)
        {
            Debug.LogError("Could not get params: "+ e.Message);
            throw new Exception("Could not get params", e);
        }
        var amount = Utils.AlgosToMicroalgos(0.01);
        var tx = Utils.GetPaymentTransaction(src.Address, new Address(DEST_ADDR), amount, "pay message", transParams);
        var signedTx = src.SignTransaction(tx);

        Debug.Log("Signed transaction with txid: " + signedTx.transactionID);

        // send the transaction to the network
        try
        {
            var id = Utils.SubmitTransaction(algodApiInstance, signedTx);
            Debug.Log("Successfully sent tx with id: " + id.TxId);
            var resp = Utils.WaitTransactionToComplete(algodApiInstance, id.TxId);
            Debug.Log("Confirmed Round is: " + resp.ConfirmedRound);
        }
        catch (ApiException e)
        {
            // This is generally expected, but should give us an informative error message.
            Debug.LogError("Exception when calling algod#rawTransaction: " + e.Message);
        }
        Debug.Log("Algorand transaction to Player completed.");
    }

    //Coroutines
    public IEnumerator PlayerWithAlgorandLoopCoroutine()
    {
        yield return null;
        Debug.Log("Starting Algorand Transaction.");
        string ALGOD_API_ADDR = "https://testnet-algorand.api.purestake.io/ps2";
        if (ALGOD_API_ADDR.IndexOf("//") == -1)
        {
            ALGOD_API_ADDR = "http://" + ALGOD_API_ADDR;
        }

        string ALGOD_API_TOKEN = "IkwGyG4qWg8W6VegMFfCa3iIIj06wi0x6Vn7FO5j";
        string SRC_ACCOUNT = "typical permit hurdle hat song detail cattle merge oxygen crowd arctic cargo smooth fly rice vacuum lounge yard frown predict west wife latin absent cup";
        string DEST_ADDR = "KV2XGKMXGYJ6PWYQA5374BYIQBL3ONRMSIARPCFCJEAMAHQEVYPB7PL3KU";
        if (!Address.IsValid(DEST_ADDR))
            Debug.LogError("The address " + DEST_ADDR + " is not valid!");
        Account src = new Account(SRC_ACCOUNT);
        Debug.Log("My account address is:" + src.Address.ToString());
        yield return null;
        AlgodApi algodApiInstance = new AlgodApi(ALGOD_API_ADDR, ALGOD_API_TOKEN);

        TransactionParametersResponse transParams;
        try
        {
            transParams = algodApiInstance.TransactionParams();
        }
        catch (ApiException e)
        {
            throw new Exception("Could not get params", e);
        }
        var amount = Utils.AlgosToMicroalgos(1);
        var tx = Utils.GetPaymentTransaction(src.Address, new Address(DEST_ADDR), amount, "pay message", transParams);
        var signedTx = src.SignTransaction(tx);

        yield return null;

        Debug.Log("Signed transaction with txid: " + signedTx.transactionID);

        // send the transaction to the network
        try
        {
            var id = Utils.SubmitTransaction(algodApiInstance, signedTx);
            Debug.Log("Successfully sent tx with id: " + id.TxId);
            //var resp = Utils.WaitTransactionToComplete(algodApiInstance, id.TxId);
            //Debug.Log("Confirmed Round is: " + resp.ConfirmedRound);
        }
        catch (ApiException e)
        {
            // This is generally expected, but should give us an informative error message.
            Debug.LogError("Exception when calling algod#rawTransaction: " + e.Message);
        }

        yield return null;

        Debug.Log("Algorand transaction to Player completed.");
    }
}
