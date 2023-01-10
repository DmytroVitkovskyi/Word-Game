using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    static private ScoreManager S;

    [Header("Set In Inspector")]
    public List<float> scoreFontSizes = new List<float> { 36, 64, 64, 1 };
    public Vector3 scoreMidPoint = new Vector3(1, 1, 0);
    public float scoreTravelTime = 3f;
    public float scoreComboDelay = 0.5f;

    private RectTransform rectTrans;

    private void Awake()
    {
        S = this;
        rectTrans = GetComponent<RectTransform>();
    }

    static public void SCORE(Wyrd wyrd, int combo)
    {
        S.Score(wyrd, combo);
    }

    // Добавить очки за это слово
    // int combo - номер этого слова в комбинации
    void Score(Wyrd wyrd, int combo)
    {
        // Создать список List<Vector2> с точками, определяющими кривую Безье
        //  для FloatingScore.
        List<Vector2> pts = new List<Vector2>();

        // Получить позицию плитки с первой буквой в wyrd
        Vector3 pt = wyrd.letters[0].transform.position;
        pt = Camera.main.WorldToViewportPoint(pt);

        pts.Add(pt); // Сделать pt первой точкой кривой безье

        // Добавить вторую точку кривой Безье
        pts.Add(scoreMidPoint);

        // Сделать Scoreboard последней точкой кривой Безье
        pts.Add(rectTrans.anchorMax);

        // Определить значение для FloatingScore
        int value = wyrd.letters.Count * combo;
        FloatingScore fs = Scoreboard.S.CreateFloatingScore(value, pts);

        fs.timeDuration = scoreTravelTime;
        fs.timeStart = Time.time + combo * scoreComboDelay;
        fs.fontSizes = scoreFontSizes;

        // Удвоить эффект InOut из Easing
        fs.easingCurve = Easing.InOut + Easing.InOut;

        // Вывести в FloatingScore текст вида "3 x 2"
        string txt = wyrd.letters.Count.ToString();
        if (combo > 1)
        {
            txt += " x " + combo;
        }
        fs.GetComponent<TextMeshProUGUI>().text = txt;
    }
}
