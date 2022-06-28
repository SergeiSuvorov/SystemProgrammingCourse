using Network;
using UnityEngine;
using Mirror;


public class PlanetOrbit : NetworkBehaviour
{
    [SerializeField] private GameObject _ringObject;
    [SerializeField] private Vector3 aroundPoint;
    [SerializeField] private float smoothTime = .3f;
    [SerializeField] private float circleInSecond = 1f;

    [SerializeField] private float offsetSin = 1;
    [SerializeField] private float offsetCos = 1;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private ObjectLabel _objectLabel;
    [SerializeField] float radius;
    
    private float currentAng;
    private Vector3 currentPositionSmoothVelocity;
    private float currentRotationAngle;
    [SyncVar] private bool _hasRing=false;
    [SyncVar] private Color _groundColor;
    [SyncVar] private Color _seaColor;
    [SyncVar] private Color _mountainColor;
    [SyncVar] private Color _atmosphereColor;
    [SyncVar] private Color _planetRingColor;
    [SyncVar] private bool _hasAtmosphere;
    [SyncVar] private float _seed;
    [SyncVar] private string _name;
    [SyncVar] private float _speed;
    [SyncVar] private int _scale;



    private const float circleRadians = Mathf.PI * 2;

    [SyncVar] protected Vector3 serverPosition;
    [SyncVar] protected Vector3 serverEulers;
    [SyncVar] protected bool _serverIsReady = true;

    private void Start()
    {
        _objectLabel = gameObject.GetComponent<ObjectLabel>();
    }

    public void Init(PlanetData planetData)
    {
        if (planetData == null)
            return;

        radius = planetData.radius;
        name = planetData.Name;
        _name = planetData.Name;
        _groundColor = planetData.planetColorGroup.GroundColor;
        _hasRing = planetData.hasPlanetRing;
        _seaColor = planetData.planetColorGroup.SeaColor;
        _mountainColor = planetData.planetColorGroup.MountainColor;
        _seed = planetData.seed;
        _hasAtmosphere = planetData.hasAtmosphere;
        _atmosphereColor = planetData.AtmosphereColor;
        _planetRingColor = planetData.PlanetRingColor;
        _speed = planetData.speed;
        _scale = planetData.scale;

    }

    public override void OnStartAuthority()
    {
        if (_ringObject != null)
        _ringObject.SetActive(_hasRing);
    }
    private void FixedUpdate()
    {
        Movement();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (_ringObject != null)
            _ringObject.SetActive(_hasRing);

        name = _name;
        if (_speed<0.01)
        {
            _speed = 0.3f;
        }

        if (_scale <=0)
        {
            _scale=1;
        }
        transform.localScale= new Vector3(_scale, _scale, _scale);

        var meshRenderer = GetComponent<MeshRenderer>();

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
   
        materialPropertyBlock.SetColor("_GroundColor", _groundColor);
        materialPropertyBlock.SetColor("_SeaColor", _seaColor);
        materialPropertyBlock.SetColor("_MountainColor", _mountainColor);
        materialPropertyBlock.SetFloat("_Seed", _seed);
        materialPropertyBlock.SetColor("_AtmosphereColor", _atmosphereColor);

        if (_hasAtmosphere)
            materialPropertyBlock.SetFloat("_HasAtmosphere", 1);
        else
            materialPropertyBlock.SetFloat("_HasAtmosphere",-1);

        meshRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void ConfigPlanet()
    {
        if (_ringObject != null)
            ConfigRing();

        name = _name;

        var meshRenderer = GetComponent<MeshRenderer>();

        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();

        materialPropertyBlock.SetColor("_GroundColor", _groundColor);
        materialPropertyBlock.SetColor("_SeaColor", _seaColor);
        materialPropertyBlock.SetColor("_MountainColor", _mountainColor);
        materialPropertyBlock.SetFloat("_Seed", _seed);
        materialPropertyBlock.SetColor("_AtmosphereColor", _atmosphereColor);

        if (_hasAtmosphere)
            materialPropertyBlock.SetFloat("_HasAtmosphere", 1);
        else
            materialPropertyBlock.SetFloat("_HasAtmosphere", -1);

        meshRenderer.SetPropertyBlock(materialPropertyBlock);
    }

    private void ConfigRing()
    {
        _ringObject.SetActive(_hasRing);
        var meshRenderer = _ringObject.GetComponent<MeshRenderer>();
        MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
        materialPropertyBlock.SetColor("_MainColor", _planetRingColor);

        meshRenderer?.SetPropertyBlock(materialPropertyBlock);
    }
    protected virtual void Movement()
    {
        if (isServer)
        {
            ServerMovement();
        }
        else
        {
            FromSeverUpdate();
        }
    }

    [Server]
    protected  void ServerMovement()
    {
        if (!isServer)
            return;

        Vector3 p = aroundPoint;
        p.x += Mathf.Sin(currentAng) * radius * offsetSin;
        p.z += Mathf.Cos(currentAng) * radius * offsetCos;
        transform.position = p;
        currentRotationAngle += Time.deltaTime * rotationSpeed;
        currentRotationAngle = Mathf.Clamp(currentRotationAngle, 0, 361);
        if (currentRotationAngle >= 360)
            currentRotationAngle = 0;

        transform.rotation = Quaternion.AngleAxis(currentRotationAngle, transform.up);
        currentAng += circleRadians * circleInSecond * Time.deltaTime;

        SendToClients();
    }

    protected  void SendToClients()
    {
        serverPosition = transform.position;
        serverEulers = transform.eulerAngles;
    }

    [Client]
    protected  void FromSeverUpdate()
    {
        if (!isClient)
            return;
        //UpdateForNewPlayer();

        transform.position = Vector3.SmoothDamp(transform.position, serverPosition, ref currentPositionSmoothVelocity, _speed);
        transform.rotation = Quaternion.Euler(serverEulers);

    }


    private void OnGUI()
    {
        _objectLabel?.DrawLabel();
    }
}



