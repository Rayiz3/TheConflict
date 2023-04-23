using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Sources
{
    public List<Unit> unit_source;
}

[System.Serializable]
public class Unit
{
    public string code;
    public string name;
    public int cost;
    public float hp;
    public float damage;
    public float speed;
    public float range;
    public float delay;
    public float cool;
    public string description;
    public void printData()
    {
        Debug.Log("name : " + name);
        Debug.Log("cost : " + cost);
        Debug.Log("hp : " + hp);
        Debug.Log("damage : " + damage);
        Debug.Log("speed : " + speed);
        Debug.Log("range : " + range);
        Debug.Log("delay : " + delay);
        Debug.Log("cool : " + cool);
    }
}

[System.Serializable]
public class Userinfo
{
    public string[] entry_unit;
}

public class LobbyManager : MonoBehaviour
{
    //slot
    public GameObject slot_unit;

    //unit info
    private Sources sources;
    private Unit focused_unit;
    public Dictionary<string, Unit> unit_list = new Dictionary<string, Unit>();

    //pad_scroll
    public GameObject scroll_content;

    //pad_info
    public Text unit_name;
    public Text unit_status;
    public Text description;

    public GameObject model_pos;
    private GameObject model;

    //entry list
    private Userinfo userinfo;
    public GameObject[] entry_slots = new GameObject[5];
    private int entry_focused;

    [System.Obsolete]
    void CreateSlot(string code)
    {
        GameObject slot = Instantiate(slot_unit);
        slot.GetComponent<RectTransform>().SetParent(scroll_content.GetComponent<RectTransform>());
        slot.GetComponent<Button>().onClick.AddListener(delegate { GetUnitInfo(code); });

        UpdateSlot(slot, code);
    }

    public void GetUnitInfo(string code)
    {
        string delay_text;
        focused_unit = unit_list[code];

        if (focused_unit.delay > 0) delay_text = (Mathf.Round(100 * 1 / focused_unit.delay) * 0.01f).ToString();
        else delay_text = "-";

        unit_name.text = focused_unit.name;
        unit_status.text =
            focused_unit.cost + "G\n" +
            focused_unit.damage + "\n" +
            focused_unit.hp + "\n" +
            focused_unit.range + "칸\n" +
            focused_unit.speed + "칸/s\n" +
            delay_text + "/s";
        description.text = focused_unit.description;

        // model
        if (model) Destroy(model);
        model = Instantiate(Resources.Load<GameObject>("Unit_models/" + focused_unit.code), model_pos.transform.position, Quaternion.identity);
        model.GetComponent<Transform>().SetParent(GameObject.Find("model_pos").GetComponent<Transform>());
    }

    public void EntryFocused(int index)
    {
        if (entry_focused != index) entry_focused = index;
        else entry_focused = -1;

        for (int i = 0; i < 5; i++)
        {
            if (i == entry_focused) entry_slots[i].GetComponent<Button>().image.color = new Vector4(1f, 0.7f, 0, 1);
            else entry_slots[i].GetComponent<Button>().image.color = new Vector4(0, 0, 0, 1);
        }
    }

    [System.Obsolete]
    public void PutInEntry()
    {
        bool isexist = false;
        foreach (string item in userinfo.entry_unit)
        {
            if (item.Equals(focused_unit.code)) isexist = true;
        }
        if (!isexist && entry_focused >= 0)
        {
            userinfo.entry_unit[entry_focused] = focused_unit.code;
            SaveUserInfo();
            UpdateSlot(entry_slots[entry_focused], focused_unit.code);
        }
    }

    void LoadUserInfo()
    {
        string json = File.ReadAllText(Application.streamingAssetsPath + "/Entry.json");
        userinfo = JsonUtility.FromJson<Userinfo>(json);
    }

    void SaveUserInfo()
    {
        File.WriteAllText(Application.dataPath + "/Entry.json", JsonUtility.ToJson(userinfo));
        LoadUserInfo();
    }

    [System.Obsolete]
    void UpdateSlot(GameObject slot, string code)
    {
        //image
        slot.transform.FindChild("back").FindChild("Image").GetComponent<Image>().sprite = Resources.Load<Sprite>("Unit_imgs/" + code);
        //cost
        slot.transform.FindChild("Text").GetComponent<Text>().text = unit_list[code].cost.ToString();
    }

    // Start is called before the first frame update
    [System.Obsolete]
    void Start()
    {
        string json = File.ReadAllText(Application.streamingAssetsPath + "/Unit.json");
        //string resource = File.ReadAllText(Application.dataPath + "/Scripts/Unit.json");

        sources = JsonUtility.FromJson<Sources>(json);

        // make unit resource dictionary
        foreach (Unit u in sources.unit_source)
        {
            unit_list.Add(u.code, u);
            CreateSlot(u.code);
        }

        // initialize
        GetUnitInfo("U_1");
        entry_focused = -1;
        LoadUserInfo();

        for (int i=0; i<5; i++) UpdateSlot(entry_slots[i], userinfo.entry_unit[i]);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
