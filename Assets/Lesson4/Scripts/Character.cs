using System;
using UnityEngine;
using Mirror;

[RequireComponent(typeof(CharacterController))]
public abstract class Character : NetworkBehaviour
{
    protected Action OnUpdateAction { get; set; }
    protected abstract FireAction fireAction { get; set; }

    [SyncVar] protected Vector3 serverPosition;
    [SyncVar] protected Quaternion serverRotation;
    [SyncVar(hook=nameof(UpdateHealth))] protected int serverHealth;
    [Range(0, 100)] [SerializeField] protected int health = 100;


    protected virtual void Initiate()
    {
        OnUpdateAction += Movement;
    }

    private void Update()
    {
        OnUpdate();
    }

    private void OnUpdate()
    {
        OnUpdateAction?.Invoke();
    }

    [Command]
    protected void CmdUpdateTransform(Vector3 position, Quaternion rotation)
    {
        serverPosition = position;
        serverRotation = rotation;
    }

    [Command]
    protected void CmdSetDamage(int damagePoint)
    {
        serverHealth -= damagePoint;
        Debug.Log(gameObject.name + " " + serverHealth);
    }

   
    private void UpdateHealth(int oldValue, int newValue)
    {
        health = newValue;
        Debug.Log("ServerHealth " + serverHealth);
        Debug.Log("Health " + health);
    }
    public abstract void Movement();
}
