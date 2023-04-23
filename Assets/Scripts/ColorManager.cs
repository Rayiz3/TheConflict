using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorManager : MonoBehaviour
{
    private static Vector4 F_color;
    private static Vector4 E_color = new Vector4(1f, 0, 0, 1f);
    private static string selected_color;
    private GameObject pad_color;

    Dictionary<string, Vector4> palette = new Dictionary<string, Vector4>();

    void SendFColor(GameObject gameObject)
    {
        gameObject.SendMessage("GetFColor", F_color);
    }

    void SendEColor(GameObject gameObject)
    {
        gameObject.SendMessage("GetEColor", E_color);
    }

    public void SelectColor(string c)
    {
        F_color = palette[c];
        selected_color = c;

        for (int i=0; i<pad_color.transform.childCount; i++)
        {
            GameObject but_color = pad_color.transform.GetChild(i).gameObject;
            Image frame = but_color.transform.GetChild(0).gameObject.GetComponent<Image>();

            if (but_color.name.Substring(7) == selected_color) frame.color = new Vector4(1, 1, 1, 1);
            else if (frame) frame.color = new Vector4(0, 0, 0, 1);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        // dictionary
        palette.Add("Blue", new Vector4(0, 0.3921f, 1f, 1f));
        palette.Add("Red", new Vector4(1f, 0, 0, 1f));
        palette.Add("Green", new Vector4(0, 0.8588f, 0, 1f));

        // initialize
        pad_color = GameObject.Find("pad_color");
        SelectColor("Blue");
    }

    // Update is called once per frame
    void Update()
    {

    }
}
