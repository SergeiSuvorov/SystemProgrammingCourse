using Mirror;
using System.Collections;
using UnityEngine;

public class RayAction : FireAction
{
    private Camera camera;
    private PlayerCharacter playerCharacter;
    private void Start()
    {
        Init();
        Debug.Log("Start RS");
        camera = GetComponentInChildren<Camera>();
        playerCharacter = gameObject.GetComponent<PlayerCharacter>();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Shooting();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Reloading();
        }

        if (Input.anyKey && !Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    protected override void Shooting()
    {
        base.Shooting();

        Debug.Log("Shoot " + gameObject.name);
        var point = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2, 0);
        var ray = camera.ScreenPointToRay(point);

        if (hasAuthority && bullets.Count>0 )
            CmdRayShoot(ray);
    }

    [Command]
    private void CmdRayShoot(Ray ray)
    {
        RaycastHit hitResult;

        RaycastHit[] hits = Physics.RaycastAll(ray);

        if (hits.Length > 1)
        {
            for (int i = hits.Length - 1; i > 0; i--)
            {
                if ((gameObject != hits[i].transform.gameObject) && (hits[i].transform.TryGetComponent(out PlayerCharacter enemy)))
                {
                    Debug.Log(enemy == playerCharacter);
                    enemy.GetDamage(10);
                    hitResult = hits[i];
                    Debug.Log("HasResult");
                    RpcShowShoot(hitResult.point, hitResult.transform);
                    
                    break;
                }
            }
        }
    
    }

    [ClientRpc]
    private void RpcShowShoot(Vector3 point, Transform targetTransform)
    {
        Debug.Log("ShowResult");
        var shoot = bullets.Dequeue();
        bulletCount = bullets.Count.ToString();
        ammunition.Enqueue(shoot);
        shoot.SetActive(true);
        shoot.transform.position = point;
        //shoot.transform.parent = targetTransform;
        StartCoroutine(ShootTimeOut(shoot));
    }



    private IEnumerator ShootTimeOut(GameObject shoot)
    {
        yield return new WaitForSeconds(2.0f);
        if (shoot != null)
        {
            shoot.SetActive(false);
            shoot.transform.parent = null;
        }

    }
}

