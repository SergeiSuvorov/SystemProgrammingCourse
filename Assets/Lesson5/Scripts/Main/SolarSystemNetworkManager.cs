using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Linq;
using System;

namespace Main
{
    public class SolarSystemNetworkManager : NetworkManager
    {
        [SerializeField] private InputField _inputField;
        private List<CristalController> cristalControllers = new List<CristalController>();
        private Dictionary<ShipController, int> resultTable = new Dictionary<ShipController, int>();
        public Dictionary<ShipController, int> ResultTable
        {
            get => resultTable;
        }

        public Action onGameEnd;

        public string PlayerName
        {
            get => _inputField.text==null? string.Empty: _inputField.text;
        }
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            Transform startPos = GetStartPosition();
            GameObject player = startPos != null
                ? Instantiate(playerPrefab, startPos.position, startPos.rotation)
                : Instantiate(playerPrefab);

 
            player.name = $"{playerPrefab.name} [connId={conn.connectionId}]";
            NetworkServer.AddPlayerForConnection(conn, player);

            ShipController shipController = player.GetComponent<ShipController>();
            if(shipController!=null)
            {
                shipController.UpdateNameOwner();
                if (!resultTable.ContainsKey(shipController))
                {
                    resultTable.Add(shipController, 0);
                }
            }

        }

        public void SetNewPositionToPlayer(GameObject player)
        {
            Transform startPos = GetStartPosition();
            ShipController shipController = player.GetComponent<ShipController>();

            if (shipController != null)
            {
                shipController.SetStarhipRestartPosition(startPos.position, startPos.rotation.eulerAngles);
            }
        }

        public void AddCristalToList(CristalController cristal)
        {
            if (!cristalControllers.Contains(cristal))
            {
                cristalControllers.Add(cristal);
            }
        }

        public void RemoveCristalFromList(CristalController cristal, ShipController ship )
        {
            if (cristalControllers.Contains(cristal))
            {
                cristalControllers.Remove(cristal);

                NetworkServer.Destroy(cristal.gameObject);

                if(resultTable.ContainsKey(ship))
                {
                    resultTable[ship]++;
                }

                if (cristalControllers.Count == 0)
                {
                    EndGame();
                }
            }
        }

        private void EndGame()
        {
            onGameEnd?.Invoke();
        }

    }
}
