using Main;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UiManager : NetworkBehaviour
{
    [SerializeField] private InputField _inputField;
    [SerializeField] private Text _textField;
    [SerializeField] private GameObject _canvas;
 
    public InputField InputField => _inputField;
    public void Init(ShipController shipController)
    {
        shipController.onStartAuthority += DeactivateInputField;
        shipController.onDestroy += ActivateInputField;

        CmdInit(shipController);
    }

    [ClientRpc]
    private void RpcShowGameResult(string result)
    {
        _canvas?.SetActive(true);
        _textField.text = result;
    }

    private void ShowGameResult()
    {
        var networkManager = (SolarSystemNetworkManager)NetworkManager.singleton;
        var gameResult = networkManager.ResultTable;
        var keys = gameResult.Keys;
        Debug.Log(keys.Count);
        string result = "                                   Result             "+ "\n" + "\n" + "\n" + "\n";
        foreach (ShipController ship in keys)
        {
            Debug.Log(ship.gameObject ==null);
            Debug.Log( ship.gameObject.name) ; 
            result += ship.gameObject.name + "         " + gameResult[ship] + "\n";
        }

        _canvas?.SetActive(true);
        _textField.text = result;
        RpcShowGameResult(result);
    }
    public override void OnStartClient()
    {
        base.OnStartClient();
        var networkManager = (SolarSystemNetworkManager)NetworkManager.singleton;
        networkManager._inputField = _inputField;

    }
    private void DeactivateInputField()
    {
        _inputField.gameObject.SetActive(false);
       
    }

    private void ActivateInputField()
    {
        if (_inputField != null)
            _inputField.gameObject.SetActive(true);
        if (_canvas != null)
            _canvas.SetActive(false);
    }


    [Command(requiresAuthority = false)]
    private void CmdInit(ShipController shipController)
    {
        var networkManager = (SolarSystemNetworkManager)NetworkManager.singleton;
        //Debug.Log("UI manager " + networkManager == null);
        //Debug.Log("shipController " + shipController.name);
        networkManager.onGameEnd += ShowGameResult;
        networkManager._inputField = _inputField  ;
    }
}
