using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    public GameObject panel_credit;
    private bool iscredit;

    public void Lobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LobbyScene");
    }

    public void Credit()
    {
        iscredit = true;
        panel_credit.SetActive(iscredit);
    }
    public void Title()
    {
        SceneManager.LoadScene("TitleScene");
    }
    public void Multi()
    {
        SceneManager.LoadScene("MultiScene");
    }
    public void Gameplay()
    {
        SceneManager.LoadScene("GameplayScene");
    }

    public void Quit()
    {
        Application.Quit();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            string scene_name = SceneManager.GetActiveScene().name;
            if (scene_name == "LobbyScene")  SceneManager.LoadScene("TitleScene");
            if (scene_name == "TitleScene")
            {
                iscredit = !iscredit;
                panel_credit.SetActive(iscredit);
            }
        }
    }
}
