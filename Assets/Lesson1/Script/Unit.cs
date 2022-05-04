using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    [SerializeField] private int _health;
    [SerializeField] private Button _receiveHealingButton;
    private bool _coroutineIsWorking = false;

    private void Start()
    {
        _receiveHealingButton.onClick.AddListener(ReceiveHealing);
    }
    public void ReceiveHealing()
    {
        StartCoroutine(AddHealth());
    }

    private IEnumerator AddHealth()
    {
        if (!_coroutineIsWorking)
        {
            _coroutineIsWorking = true;
            for (int i = 0; i < 6; i++)
            {
                if (_health < 100)
                {
                    yield return new WaitForSeconds(0.5f);
                    _health += 5;
                    Debug.Log("Health: " + _health);
                }
                else
                {
                    _health = 100;
                    _coroutineIsWorking = false;
                    Debug.Log("End Receive Healing. Health: " + _health);
                    yield break;
                }   
            }

            _coroutineIsWorking = false;
        }
        else
        {
            Debug.Log("Receive Healing is working");
        }
    }
}
