using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NameManager : MonoBehaviour
{
    private static string playername;

    public Text textbox;
    public Text textshow;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (textbox)
        {
            playername = textbox.text;
        }

        if (textshow)
        {
            textshow.text = playername;
        }
    }
}
