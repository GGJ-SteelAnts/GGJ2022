using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UiController : MonoBehaviour
{
    public GameObject player = null;
    private Vector3 startPosition;
    private Vector3 playerPosition;
    public TextMeshProUGUI uiDistance;
    public float distance = 0.0f;
    private float oldDistance = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = this.player.transform.position;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Update is called once per frame
    void Update()
    {
        playerPosition = this.player.transform.position;
        distance = Vector3.Distance(this.startPosition, this.playerPosition);
        if (oldDistance < distance)
        {
            uiDistance.text = "Distance : " + distance.ToString("0");
            oldDistance = distance;
        }
    }

    //MENU
    public void exitGame()
    {
        Application.Quit();
    }
}
