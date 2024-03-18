using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MenuButtons : MonoBehaviour
{

    public GameObject menu;

    void Update() {
        if (Input.GetButtonDown("Cancel"))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (menu.activeSelf)
            menu.SetActive(false);
        else
            menu.SetActive(true);
    }

    public void PauseGame()
    {
        menu.SetActive(true);
    }

    public void ResumeGame()
    {
        menu.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
