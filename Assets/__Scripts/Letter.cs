using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Letter : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float timeDuration = 0.5f;
    public string easingCuve = Easing.InOut; // ??????? ??????????? ?? Utils.cs

    [Header("Set Dynamically")]
    public TextMeshPro tMesh; // TextMeshPro ?????????? ??????
    public Renderer tRend; // ????????? Renderer ??????? 3D Text. ?? ????? ?????????? 
                           // ????????? ???????
    public bool big = false; // ??????? ? ????? ?????? ????????? ?? ???????
    private char _c; // ??????, ???????????? ?? ???? ??????
    private Renderer rend;

    // ???? ??? ???????? ????????????
    public List<Vector3> pts = null;
    public float timeStart = -1;

    // ????????? ??? ????????? ??????????? 3D Text,
    // ??? ?????? ????? ??????? ??? ????????? ??????????????.
    public bool visible
    {
        get { return tRend.enabled; }
        set { tRend.enabled = value; }
    }

    public char c
    {
        get { return _c; }
        set
        {
            _c = value;
            tMesh.text = _c.ToString();
        }
    }

    public string str
    {
        get { return _c.ToString(); }
        set { c = value[0]; }
    }

    public Color color
    {
        get { return rend.material.color; }
        set { rend.material.color = value; }
    }

    // ???????? ??? ??????/?????? ????????? ??????
    // ?????? ??????????? ?????? ????? ??? ???????? ??????????? ? ????? ??????????
    public Vector3 pos
    {
        set {
            // transform.position = value;

            // ????? ??????? ????? ?? ????????? ?????????? ?? ???????????
            // ??????? ????? ????? ??????? ? ????? (value) ?????????

            Vector3 mid = (transform.position + value) / 2f;

            // ????????? ?????????? ?? ????????? 1/4 ??????????
            //   ?? ??????????? ??????? ?????
            float mag = (transform.position - value).magnitude;
            mid += Random.insideUnitSphere * mag * 0.25f;

            // ??????? List<Vector3> ?????, ???????????? ?????? ?????
            pts = new List<Vector3>() { transform.position, mid, value };

            // ???? timeStart ???????? ???????? ?? ????????? -1,
            //  ?????????? ??????? ?????
            if (timeStart == -1) timeStart = Time.time;
        }
    }

    // ?????????? ?????????? ? ????? ???????
    public Vector3 posImmediate
    {
        set { transform.position = value; }
    }

    private void Awake()
    {
        //GameObject go1 = gameObject; // ??? ???: ??? ???????
        tMesh = GetComponentInChildren<TextMeshPro>();
        tRend = tMesh.GetComponent<Renderer>();
        rend = GetComponent<Renderer>();
        visible = false;
    }

    private void Update()
    {
        if (timeStart == -1) return;
        // ??????????? ???????? ????????????
        float u = (Time.time - timeStart) / timeDuration;
        u = Mathf.Clamp01(u);
        float u1 = Easing.Ease(u, easingCuve);
        Vector3 v = Utils.Bezier(u1, pts);
        transform.position = v;

        // ???? ???????????? ?????????, ???????? -1 ? timeStart
        if (u == 1) timeStart = -1;
    }
}
