using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wyrd
{
    public string str; // Строковое представление слова
    public List<Letter> letters = new List<Letter>();
    public bool found = false; // Получит true, если игрок нашёл это слово

    // Свойство, управляющее видимостью компонента 3D Text каждой плитки Letter
    public bool visible
    {
        get
        {
            if (letters.Count == 0)
            {
                return false;
            }
            return letters[0].visible;
        }
        set
        {
            foreach (var item in letters)
            {
                item.visible = value;
            }
        }
    }

    // Свойство для назначения цвета каждой плитке Letter
    public Color color
    {
        get
        {
            if (letters.Count == 0)
            {
                return Color.black;
            }
            return letters[0].color;
        }
        set
        {
            foreach (var item in letters)
            {
                item.color = value;
            }
        }
    }

    // Добавляет плитку в список letters
    public void Add(Letter l)
    {
        letters.Add(l);
        str += l.c.ToString();
    }
}
