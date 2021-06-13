using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    PlayerController player;
    bool paused = false;
    int state = 0;

    public GameObject pauseMenu, startMenu, endMenu;

    // Start is called before the first frame update
    void Awake()
    {
        player = FindObjectOfType<PlayerController>();
        player.inputBlocked = true;

        pauseMenu.SetActive(false);
        startMenu.SetActive(true);
        endMenu.SetActive(false);

        Physics.autoSimulation = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == 0 && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(fadeMenuOut(startMenu));
            Physics.autoSimulation = true;
            player.inputBlocked = false;
            state = 1;
        }
        else if (state == 1 && Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(fadeMenuIn(pauseMenu));
            Physics.autoSimulation = false;
            player.inputBlocked = true;
            state = 2;
        }
        else if (state == 2)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(fadeMenuOut(pauseMenu));
                Physics.autoSimulation = true;
                player.inputBlocked = false;
                state = 1;
            }
            else if(Input.GetKey(KeyCode.Q))
            {
                Application.Quit();
            }
        }
        else if (state == 3)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                StartCoroutine(fadeMenuOut(endMenu));
                Physics.autoSimulation = true;
                player.inputBlocked = false;
                state = 1;
                player.ResetCheckpoint();
            }
            if (Input.GetKey(KeyCode.Q))
            {
                Application.Quit();
            }
        }
    }

    IEnumerator fadeMenuOut(GameObject menu)
    {
        RawImage image = menu.GetComponentInChildren<RawImage>();
        TMPro.TextMeshProUGUI text = menu.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        float alpha = 1;
        while (alpha > 0)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            text.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            alpha -= Time.deltaTime;
            yield return null;
        }
        menu.SetActive(false);
    }

    IEnumerator fadeMenuIn(GameObject menu)
    {
        menu.SetActive(true);
        RawImage image = menu.GetComponentInChildren<RawImage>();
        TMPro.TextMeshProUGUI text = menu.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        float alpha = 0;
        while (alpha < 1)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            text.color = new Color(image.color.r, image.color.g, image.color.b, alpha);
            alpha += Time.deltaTime;
            yield return null;
        }
    }

    public void AtEnd()
    {
        StartCoroutine(fadeMenuIn(endMenu));
        Physics.autoSimulation = false;
        player.inputBlocked = true;
        state = 3;
    }
}
