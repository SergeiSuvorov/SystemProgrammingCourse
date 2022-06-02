using UnityEngine;
using Mirror;

public class CristalSpawner : NetworkBehaviour
{
    [SerializeField]  private CristalController _cristalPrefab;
    [SerializeField]  private int _cristalCount;
    public override void OnStartServer()
    {
        base.OnStartServer();

        for (int i = 0; i < _cristalCount; i++)
        {
            GameObject cristalInstance = Instantiate(_cristalPrefab.gameObject);

            NetworkServer.Spawn(cristalInstance, NetworkServer.localConnection);
            cristalInstance.transform.position = transform.position + Random.insideUnitSphere * 500;
        }

        Destroy(gameObject);
    }
}
