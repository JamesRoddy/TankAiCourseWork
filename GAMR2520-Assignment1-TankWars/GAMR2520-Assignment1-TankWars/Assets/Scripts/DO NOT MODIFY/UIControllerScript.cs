using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Class <c>UIControllerScript</c> handles UI.
/// </summary>
public class UIControllerScript : MonoBehaviour
{
    public TextMeshProUGUI tankOneName;
    public TextMeshProUGUI tankOneHealth;
    public TextMeshProUGUI tankOneFuel;
    public TextMeshProUGUI tankOneAmmo;

    public TextMeshProUGUI tankTwoName;
    public TextMeshProUGUI tankTwoHealth;
    public TextMeshProUGUI tankTwoFuel;
    public TextMeshProUGUI tankTwoAmmo;

    public GameObject tankOnePanel;
    public GameObject TankTwoPanel;

    public TextMeshProUGUI winsText;
 
    public void DisableStatsUi()
    {
        tankOnePanel.SetActive(false);
        TankTwoPanel.SetActive(false);
    }
}
