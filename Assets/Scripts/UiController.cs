using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UiController : MonoBehaviour
{
    public GameObject player = null;
    private float startPosition;
    public TextMeshProUGUI uiDistance;
    public int highScore;
    public float distance = 0.0f;
    private float oldDistance = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        startPosition = this.player.transform.position.z;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        this.LoadSave();
    }
    void SaveGame()
    {
        PlayerPrefs.SetInt(PlayerController.highScore);
        PlayerPrefs.Save();
        Debug.Log("Game data saved!");
    }
    void LoadGame()
    {
        if (PlayerPrefs.HasKey("HighestScore"))
        {
            this.highScore = PlayerPrefs.GetInt("SavedInteger");
            Debug.Log("Game data loaded!");
        }
        else
        {
            this.highScore = 0;
        }

    }

    // Update is called once per frame
    void Update()
    {
        distance = this.player.transform.position.z - startPosition;
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
