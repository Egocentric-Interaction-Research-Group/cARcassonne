using UnityEngine;

namespace Carcassonne
{
    public class TurnScript : MonoBehaviour
    {
        private int iterator;
        private int nbrOfplayers;
        private int turns;

        private void Start()
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
            if (iterator + 1 == nbrOfplayers)
            {
                return iterator = 0;
            }

            iterator += 1;
            return iterator;
        }
    }
}