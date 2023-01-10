using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Letter : MonoBehaviour
{
    [Header("Set in Inspector")]
    public float timeDuration = 0.5f;
    public string easingCuve = Easing.InOut; // Функция сглаживания из Utils.cs

    [Header("Set Dynamically")]
    public TextMeshPro tMesh; // TextMeshPro отображает символ
    public Renderer tRend; // Компонент Renderer объекта 3D Text. Он будет определять 
                           // видимость символа
    public bool big = false; // Большие и малые плитки действуют по разному
    private char _c; // Символ, отображаемый на этой плитке
    private Renderer rend;

    // Поля для линейной интерполяции
    public List<Vector3> pts = null;
    public float timeStart = -1;

    // Разрешает или запрещает отображение 3D Text,
    // что делает букву видимой или невидимой соответственно.
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

    // Свойство для чтения/записи координат плитки
    // Теперь настраивает кривую Безье для плавного перемещения в новые координаты
    public Vector3 pos
    {
        set {
            // transform.position = value;

            // Найти среднюю точку на случайном расстоянии от фактической
            // средней точки между текущей и новой (value) позициями

            Vector3 mid = (transform.position + value) / 2f;

            // Случайное расстояние не превышает 1/4 расстояния
            //   до фактической средней точки
            float mag = (transform.position - value).magnitude;
            mid += Random.insideUnitSphere * mag * 0.25f;

            // Создать List<Vector3> точек, определяющих кривую Безье
            pts = new List<Vector3>() { transform.position, mid, value };

            // Если timeStart содержит значение по умолчанию -1,
            //  установить текущее время
            if (timeStart == -1) timeStart = Time.time;
        }
    }

    // Немедленно перемещает в новую позицию
    public Vector3 posImmediate
    {
        set { transform.position = value; }
    }

    private void Awake()
    {
        //GameObject go1 = gameObject; // мой код: для отладки
        tMesh = GetComponentInChildren<TextMeshPro>();
        tRend = tMesh.GetComponent<Renderer>();
        rend = GetComponent<Renderer>();
        visible = false;
    }

    private void Update()
    {
        if (timeStart == -1) return;
        // Стандартная линейная интерполяция
        float u = (Time.time - timeStart) / timeDuration;
        u = Mathf.Clamp01(u);
        float u1 = Easing.Ease(u, easingCuve);
        Vector3 v = Utils.Bezier(u1, pts);
        transform.position = v;

        // Если интерполяция закончена, записать -1 в timeStart
        if (u == 1) timeStart = -1;
    }
}
