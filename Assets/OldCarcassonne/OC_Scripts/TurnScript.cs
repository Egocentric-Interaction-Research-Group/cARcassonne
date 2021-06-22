using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurnScript : MonoBehaviour
{
    int nbrOfplayers;
    int turns;
    int iterator = 0;
    void Start()
    {

    }

    public int currentPlayer(int playersInRoom)
    {
        nbrOfplayers = playersInRoom;
        return iterator;
    }


    public int newTurn()
    {
        Debug.Log("PlayerCount i Room " + nbrOfplayers);
        if (iterator+1 == nbrOfplayers)
        {
            return iterator = 0;
        }
        else
        {
            iterator += 1;
            return iterator;
        }
        
    }
 
}
