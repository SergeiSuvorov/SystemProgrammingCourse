using System;
using Main;
using Mechanics;
using Network;
using UnityEngine;
using Mirror;
using System.Collections.Generic;

public class ShipController : NetworkMovableObject
{
    [SerializeField] private Transform _cameraAttach;
    private CameraOrbit _cameraOrbit;
    [SerializeField] private ObjectLabel playerLabel;
    private float _shipSpeed;
    private Rigidbody _rigidbody;
   
    private UiManager _uiManager;

    public Action onStartAuthority;
    public Action onDestroy;
    public Action onGameEnd;

    public delegate void GameEnd(Dictionary<ShipController, int> gameResult);
    public event GameEnd onEndGame;

    [SyncVar(hook = nameof(onUpdateShipName))] private string _serverPlayerName;

    private Vector3 currentPositionSmoothVelocity;

    protected override float speed => _shipSpeed;
    private SpaceShipSettings _spaceShipSettings;

   

    public string PlayerName
    {
        get => gameObject.name;
        set => _serverPlayerName = value;
    }

    private void OnGUI()
    {
        playerLabel.DrawLabel();
    }

    void Awake()
    {
        var collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = false;

       
        var networkManager = (SolarSystemNetworkManager)NetworkManager.singleton;
        Debug.Log(networkManager.PlayerName == string.Empty);

    }
    public override void OnStartAuthority()
    {

        base.OnStartAuthority();

        var collider = GetComponent<Collider>();
        if (collider != null)
            collider.enabled = true;

        _rigidbody = GetComponent<Rigidbody>();
        if (_rigidbody == null)
            return;

        _uiManager = FindObjectOfType<UiManager>();
        _uiManager.Init(this);

        var networkManager = (SolarSystemNetworkManager)NetworkManager.singleton;
        PlayerName = networkManager.PlayerName == string.Empty ? gameObject.name : networkManager.PlayerName;
        gameObject.name = networkManager.PlayerName == string.Empty ? gameObject.name : networkManager.PlayerName;
        Subscribe();
        _cameraOrbit = FindObjectOfType<CameraOrbit>();
        _cameraOrbit.Initiate(_cameraAttach == null ? transform : _cameraAttach);

        Debug.Log(networkManager.PlayerName == string.Empty);

            onStartAuthority?.Invoke();
    }

    [ClientCallback]
    private void LateUpdate()
    {
        _cameraOrbit?.CameraMovement();
    }

    protected override void HasAuthorityMovement()
    {
        if (_spaceShipSettings == null)
            return;

        var isFaster = Input.GetKey(KeyCode.LeftShift);
        var speed = _spaceShipSettings.shipSpeed;
        var faster = isFaster ? _spaceShipSettings.faster : 1.0f;

        _shipSpeed = Mathf.Lerp(_shipSpeed, speed * faster, _spaceShipSettings.acceleration);

        var currentFov = isFaster ? _spaceShipSettings.fasterFov : _spaceShipSettings.normalFov;
        _cameraOrbit.SetFov(currentFov, _spaceShipSettings.changeFovSpeed);

        var velocity = _cameraOrbit.transform.TransformDirection(Vector3.forward) * _shipSpeed;
        _rigidbody.velocity = velocity * (_updatePhase == UpdatePhase.FixedUpdate ? Time.fixedDeltaTime : Time.deltaTime);

        if (!Input.GetKey(KeyCode.C))
        {
            var targetRotation = Quaternion.LookRotation(Quaternion.AngleAxis(_cameraOrbit.LookAngle, -transform.right) * velocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * speed);
        }

        if (Input.GetKey(KeyCode.A))
        {
            TrigerExecute();
            CmdTrigerExecute();
        }

        if (isServer)
        {
            SendToClients();
        }
        else
        {
            CmdSendTransform(transform.position, transform.rotation.eulerAngles);
        }
    }


    protected override void FromOwnerUpdate()
    {
        transform.position = Vector3.SmoothDamp(transform.position, serverPosition, ref currentPositionSmoothVelocity, speed);
        transform.rotation = Quaternion.Euler(serverEulers);
    }

    private void OnDestroy()
    {
        onDestroy?.Invoke();

        onStartAuthority = null;
        onGameEnd = null;
    }

    [Server]
    public void Init(SpaceShipSettings spaceShipSettings)
    {
        if (isServer)
        {
            _spaceShipSettings = spaceShipSettings;
            RpcInit(spaceShipSettings);
        }
    }

    [ClientRpc]
    public void RpcInit(SpaceShipSettings spaceShipSettings)
    {
        _spaceShipSettings = spaceShipSettings;
    }


    #region SubscribeMethod
    private void Subscribe()
    {
        if(isServer)
        {
            ServerSubscribe();
        }
        else
        {
            CmdSubscribe();
        }
    }
    
    private void ServerSubscribe()
    {
        var networkManager = (SolarSystemNetworkManager)NetworkManager.singleton;
        networkManager.onGameEnd += onGameOver;
    }

    [Command]
    private void CmdSubscribe()
    {
        ServerSubscribe();
    }
    #endregion

    #region DeactiveMethod
    private void onGameOver()
    {
        DeactivateShip();
    }

    public void DeactivateShip()
    {
        if (isServer)
        {
            onGameEnd?.Invoke();
            DeactivateShipOnServer();
        }
        else
        {
            CmdDeactivateShip();
        }
    }

    private void DeactivateShipOnServer()
    {
        gameObject.SetActive(false);
        RpcDeactivateShip();
    }

    [ClientRpc]
    private void RpcDeactivateShip()
    {
        onGameEnd?.Invoke();
        gameObject.SetActive(false);
    }

    [Command]
    private void CmdDeactivateShip()
    {
        DeactivateShipOnServer();
    }

    #endregion
   
    #region ChangeTransformMethod
    [Command]
    private void CmdSendTransform(Vector3 position, Vector3 eulers)
    {
        serverPosition = position;
        serverEulers = eulers;
    }

    [TargetRpc]
    private void TargetSendTransform(Vector3 position, Vector3 eulers)
    {
        transform.position = position;
        transform.eulerAngles = eulers;
    }


    [Command]
    private void CmdSendStartTransform(Vector3 position, Vector3 eulers)
    {

        serverPosition = position;
        serverEulers = eulers;

        SendToClients();
    }

    public void SetStarhipRestartPosition(Vector3 position, Vector3 eulers)
    {
        Debug.Log("SetStarhipRestartPosition " + gameObject.name);
        if (isServer)
        {
            SetPosition(position, eulers);
            TargetSendTransform(position, eulers);
        }
        else
        {
            transform.position = position;
            transform.eulerAngles = eulers;

            CmdSendStartTransform(position, eulers);
        }
    }

    protected override void SendToClients()
    {
        serverPosition = transform.position;
        serverEulers = transform.eulerAngles;
    }

    private void SetPosition(Vector3 position, Vector3 eulers)
    {

        serverPosition = position;
        serverEulers = eulers;
        SendToClients();
    }

    #endregion

    #region NameMethod
    [TargetRpc]
    private void TargetUpdateNameOwner()
    {
        CmdUpdateName(PlayerName);
    }

    private void onUpdateShipName(string oldName, string newName)
    {
        gameObject.name = newName;
    }

    [Command]
    private void CmdUpdateName(string name)
    {
        _serverPlayerName = name;
        gameObject.name = name;
    }
    public void UpdateNameOwner()
    {
        TargetUpdateNameOwner();
    }
    #endregion

    #region TrigerMethod

    //[ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        var planet = other.gameObject.GetComponent<PlanetOrbit>();
        if (planet != null )
        {
           // TrigerExecute();
            CmdTrigerExecute();
        }
    }

    [ServerCallback]
    private void TrigerExecute()
    {
        Debug.Log("TrigerExecute");
        var networkManager = (SolarSystemNetworkManager)NetworkManager.singleton;
        networkManager.SetNewPositionToPlayer(gameObject);
    }

    [Command]
    private void CmdTrigerExecute()
    {
        Debug.Log("TrigerExecute");
        var networkManager = (SolarSystemNetworkManager)NetworkManager.singleton;
        networkManager.SetNewPositionToPlayer(gameObject);
    }
    #endregion
}

