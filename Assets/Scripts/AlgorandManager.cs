using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlgorandManager : Singleton<AlgorandManager>
{
    [SerializeField]
    protected string m_PlayerName;

    //Public method
    public string GetPlayerName()
    {
        return m_PlayerName;
    }
}
