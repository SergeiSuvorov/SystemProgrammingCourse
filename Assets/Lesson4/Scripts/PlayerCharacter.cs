using UnityEngine;
using Mirror;
using System.Linq;

public class PlayerCharacter : Character
{
   

    [Range(0.5f, 10.0f)] [SerializeField] private float movingSpeed = 8.0f;
    [SerializeField] private float acceleration = 3.0f;
    private const float gravity = -9.8f;
    private CharacterController characterController;
    private MouseLook mouseLook;
    private int healthChange = 0;
    private Vector3 currentVelocity;
    private int connectionId;


    protected override FireAction fireAction { get; set; }

    public NetworkConnection connection { get; set; }

    protected override void Initiate()
    {
        base.Initiate();
        connectionId = NetworkServer.connections.Where(x => x.Value == connectionToClient).FirstOrDefault().Key;
        Debug.Log(NetworkServer.connections.ContainsValue(connectionToClient));
        Debug.Log(connectionId);

        //fireAction = gameObject.AddComponent<RayAction>();
        fireAction = gameObject.GetComponent<RayAction>();
        fireAction.Reloading();

        characterController = GetComponentInChildren<CharacterController>();
        if(characterController==null)
            characterController = gameObject.AddComponent<CharacterController>();

        mouseLook = GetComponentInChildren<MouseLook>();
        if(mouseLook==null)
            mouseLook = gameObject.AddComponent<MouseLook>();

        serverHealth = health;
    }

    public override void Movement()
    {
        if (mouseLook != null && mouseLook.PlayerCamera != null)
        {
            mouseLook.PlayerCamera.enabled = hasAuthority;
        }

        if (hasAuthority)
        {
            var moveX = Input.GetAxis("Horizontal") * movingSpeed;
            var moveZ = Input.GetAxis("Vertical") * movingSpeed;
            var movement = new Vector3(moveX, 0, moveZ);
            movement = Vector3.ClampMagnitude(movement, movingSpeed);
            movement *= Time.deltaTime;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                movement *= acceleration;
            }

            movement.y = gravity;
            movement = transform.TransformDirection(movement);

            characterController.Move(movement);
            mouseLook.Rotation();

            CmdUpdateTransform(transform.position, transform.rotation);
        }
        else
        {
            transform.position = Vector3.SmoothDamp(transform.position, serverPosition, ref currentVelocity, movingSpeed * Time.deltaTime);
            transform.rotation = serverRotation;
        }
    }

    private void Start()
    {
        Initiate();
    }

    public void GetDamage(int damagePoint)
    {
        if (isServer)
            UpdateHealth(serverHealth - damagePoint);
        else
        {
            CmdUpdateHealth(serverHealth - damagePoint);
        }
    }

    [Server]
    private void UpdateHealth(int updateValue)
    {
        serverHealth = updateValue;
        CheckPlayerHealth();
    }

    [Command(requiresAuthority =false)]
    protected void CmdUpdateHealth(int updateValue)
    {
        UpdateHealth(updateValue);
    }

    [Server]
    protected void CheckPlayerHealth()
    {
        Debug.Log(gameObject.name + " " + health);
        Debug.Log(gameObject.name + " s " + serverHealth);
        if (serverHealth <= 0)
        {
            //NetworkServer.RemoveConnection((int)netId);
            NetworkServer.connections[connectionId].Disconnect();
            //NetworkServer.Destroy(gameObject);
        }

    }

    private void OnDestroy()
    {
        Debug.Log("itDestroy");
    }

    private void OnGUI()
    {
        if (Camera.main == null)
        {
            return;
        }

        var info = $"Health: {health}\nClip: {fireAction.bulletCount}";
        var size = 12;
        var bulletCountSize = 50;
        var posX = Camera.main.pixelWidth / 2 - size / 4;
        var posY = Camera.main.pixelHeight / 2 - size / 2;
        var posXBul = Camera.main.pixelWidth - bulletCountSize * 2;
        var posYBul = Camera.main.pixelHeight - bulletCountSize;
        GUI.Label(new Rect(posX, posY, size, size), "+");
        GUI.Label(new Rect(posXBul, posYBul, bulletCountSize * 2, bulletCountSize * 2), info);
    }
}
