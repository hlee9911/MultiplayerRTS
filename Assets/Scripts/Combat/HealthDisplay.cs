using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthDisplay : MonoBehaviour
{
    [SerializeField] private HealthManager healthManager = null;
    [SerializeField] private GameObject healthBarParent = null;
    [SerializeField] private Image healthBarImage = null;

    [Header("Units only fields")]
    [SerializeField] private UnitManager unitManager = null;

    void Awake()
    {
        // subscribing the event
        healthManager.ClientOnHealthUdpated += HandleHealthUpdated;
    }

    private void Start()
    {
        TurnHealthBarOff();
    }

    void OnDestroy()
    {
        // unsubscribing the event
        healthManager.ClientOnHealthUdpated -= HandleHealthUpdated;
    }

    void OnMouseEnter()
    {
        TurnHealthBarOn();
    }

    void OnMouseExit()
    {
        if (unitManager && unitManager.IsSelected) return;
        TurnHealthBarOff();
    }

    public void TurnHealthBarOn()
    {
        healthBarParent.SetActive(true);
    }

    public void TurnHealthBarOff()
    {
        healthBarParent.SetActive(false);
    }

    void HandleHealthUpdated(int currentHealth, int maxHealth)
    {
        healthBarImage.fillAmount = (float)currentHealth / maxHealth;
        // TurnHealthBarOn();
    }
}
