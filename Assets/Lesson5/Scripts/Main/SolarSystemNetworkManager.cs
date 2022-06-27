using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;
using System.Linq;
using System;


public class SolarSystemNetworkManager : NetworkManager
{
    [SerializeField] public InputField _inputField;

    [SerializeField] private PlanetSpawner _planetSpawner;
    [SerializeField] private Vector3 _spawnPosition;
    [SerializeField] PlanetOrbit _planetPrefab;
    [SerializeField] private GameObject _spawner;
    private SpaceShipSettings _spaceShipSettings;

    private List<CristalController> cristalControllers = new List<CristalController>();
    private Dictionary<ShipController, int> resultTable = new Dictionary<ShipController, int>();
    public Dictionary<ShipController, int> ResultTable
    {
        get => resultTable;
    }

    public Action onGameEnd;
    public Action onAddPlayer;
    public event Action OnServerStart;
    private string _playerName;



    public string PlayerName
    {
        get => _inputField == null ? string.Empty : _inputField.text;
        set => _playerName = value;
    }
    public void Init(SpaceShipSettings spaceShipSettings)
    {
        _spaceShipSettings = spaceShipSettings;
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
        if (shipController != null)
        {
            shipController.Init(_spaceShipSettings);

            shipController.UpdateNameOwner();
            if (!resultTable.ContainsKey(shipController))
            {
                resultTable.Add(shipController, 0);
            }
        }

        
    }

    public override void OnStartClient()
    {
        onAddPlayer?.Invoke();
    }
    public override void OnStartServer()
    {
        base.OnStartServer();
        OnServerStart?.Invoke();
    }


    [Server]
    public void SetNewPositionToPlayer(GameObject player)
    {
        Transform startPos = GetStartPosition();
        ShipController shipController = player.GetComponent<ShipController>();

        if (shipController != null)
        {
            shipController.SetStarhipRestartPosition(startPos.position, startPos.rotation.eulerAngles);
        }
    }

    [Server]
    public void AddCristalToList(CristalController cristal)
    {
        if (!cristalControllers.Contains(cristal))
        {
            cristalControllers.Add(cristal);
        }
    }

    [Server]
    public void RemoveCristalFromList(CristalController cristal, ShipController ship)
    {
        if (cristalControllers.Contains(cristal))
        {
            cristalControllers.Remove(cristal);

            NetworkServer.Destroy(cristal.gameObject);

            if (resultTable.ContainsKey(ship))
            {
                resultTable[ship]++;
            }

            if (cristalControllers.Count == 0)
            {
                EndGame();
            }
        }
    }

    [Server]
    private void EndGame()
    {
        onGameEnd?.Invoke();
    }

}

