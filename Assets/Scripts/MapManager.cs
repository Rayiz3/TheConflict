using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private GameManager gamemanager;

    private int r;
    private int c;

    //focused
    private static int focused_row;
    private static int focused_col;
    private Vector4 selected_color = new Vector4(0.4f, 0.4f, 0.4f, 1f);

    //selected
    private static int selected_row;
    private static int selected_col;

    //units in a row
    //private static int sum_unitF_row;
    //private static int sum_unitE_row;

    private SpriteRenderer renderer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject col_parent = collision.transform.parent.gameObject;
        if (collision.name == "Col_body" && col_parent.tag.Contains("Unit"))
        {
            col_parent.SendMessage("SetUnitR", r);
            col_parent.SendMessage("SetUnitC", c);
        }
    }

    private void OnMouseEnter()
    {
        focused_row = r;
        focused_col = c;
    }
    private void OnMouseExit()
    {
        focused_row = 0;
        focused_col = 0;
    }
    private void OnMouseUp()
    {
        selected_row = r;
        selected_col = c;

        if (gamemanager.isrally)
        {
            gamemanager.SendMessage("Setisrally", false);
            gamemanager.SendMessage("Setrallypoint", selected_row);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        r = int.Parse(name[1].ToString());
        c = int.Parse(name[3].ToString());

        //sum_unitF_row = 0;
        //sum_unitE_row = 0;

        renderer = GetComponent<SpriteRenderer>();

        gamemanager = GameObject.Find("Manager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gamemanager.isrally && focused_row == r) renderer.color = selected_color;
        else renderer.color = new Vector4(0, 0, 0, 1);
    }
}
