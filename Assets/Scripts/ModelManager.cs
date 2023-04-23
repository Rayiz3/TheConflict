using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelManager : MonoBehaviour
{
    //unit animator
    private Animator unit_anim;

    //unit color
    public SpriteRenderer[] color_parts = new SpriteRenderer[9];
    // 0 | head   2 | Rarm    4 | Rleg    6 | Rshold    8 | armed //
    // 1 | body   3 | Larm    5 | Lleg    7 | Lshold              //
    private Vector4 unit_color;

    //Unit status
    enum Status { idle, destroyed, move, attack };
    Status unit_status;

    void GetFColor(Vector4 c)
    {
        unit_color = c;
    }

    // Start is called before the first frame update
    void Start()
    {
        unit_status = Status.idle;
        unit_anim = GetComponent<Animator>();
        unit_anim.SetInteger("anim_state", 4);
    }

    // Update is called once per frame
    void Update()
    {
        //set color
        GameObject.Find("Manager").GetComponent<ColorManager>().SendMessage("SendFColor", gameObject);

        foreach (SpriteRenderer c in color_parts)
        {
            if (c) c.color = unit_color;
        }
    }
}
