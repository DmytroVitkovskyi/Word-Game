using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;

public enum GameMode
{
    preGame,   // Перед началом игры
    loading,   // Список слов загружается и анализируется
    makeLevel, // Создаётся отдельный WordLevel
    levelPrep, // Создать уровень с визуальным представлением
    inLevel    // Уровень запущен
}

public class WordGame : MonoBehaviour
{
    static public WordGame S;

    [Header("Set in Inspector")]
    public GameObject prefabLetter;
    public Rect wordArea = new Rect(-24, 19, 48, 28);
    public float letterSize = 1.5f;
    public bool showAllWyrds = true;
    public float bigLetterSize = 4f;
    public Color bigColorDim = new Color(0.8f, 0.8f, 0.8f);
    public Color bigColorSelected = new Color(1f, 0.9f, 0.7f);
    public Vector3 bigLetterCenter = new Vector3(0, -16, 0);
    public Color[] wyrdPalette;
    public AudioSource as_set_paper;
    public AudioSource[] as_symbol;
    public AudioSource as_return;
    public AudioSource as_music;
    public Button bt_sound;
    public Sprite img_sound_on;
    public Sprite img_sound_off;

    [Header("Set Dynamically")]
    public GameMode mode = GameMode.preGame;
    public WordLevel currLevel;
    public List<Wyrd> wyrds;
    public List<Letter> bigLetters;
    public List<Letter> bigLettersActive;
    public string testWord;
    private string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    private Transform letterAnchor, bigLetterAnchor;

    private void Awake()
    {
        S = this;
        letterAnchor = new GameObject("LetterAnchor").transform;
        bigLetterAnchor = new GameObject("BigLetterAnchor").transform;
    }

    // Start is called before the first frame update
    void Start()
    {
        mode = GameMode.loading;
        WordList.INIT();
    }

    public void WordListParseComplete()
    {
        mode = GameMode.makeLevel;
        currLevel = MakeWordLevel();
    }

    public WordLevel MakeWordLevel(int levelNum = -1)
    {
        WordLevel level = new WordLevel();
        if (levelNum == -1)
        {
            level.longWordIndex = Random.Range(0, WordList.LONG_WORD_COUNT);
        }
        else
        {
            // код будет позже
        }
        level.levelNum = levelNum;
        level.word = WordList.GET_LONG_WORD(level.longWordIndex);
        level.charDict = WordLevel.MakeCharDict(level.word);

        StartCoroutine(FindSubWordsCoroutine(level));
        return level;
    }

    public IEnumerator FindSubWordsCoroutine(WordLevel level)
    {
        level.subWords = new List<string>();
        string str;
        List<string> words = WordList.GET_WORDS();

        for (int i = 0; i < WordList.WORD_COUNT; i++)
        {
            str = words[i];
            if (WordLevel.CheckWordInLevel(str,level))
            {
                level.subWords.Add(str);
            }
            if (i%WordList.NUM_TO_PARSE_BEFORE_YIELD == 0)
            {
                yield return null;
            }
        }

        level.subWords.Sort();
        level.subWords = SortWordsByLength(level.subWords).ToList();

        SubWordSearchComplete();
    }

    public static IEnumerable<string> SortWordsByLength(IEnumerable<string> ws)
    {
        ws = ws.OrderBy(s => s.Length);
        return ws;
    }
    public void SubWordSearchComplete()
    {
        mode = GameMode.levelPrep;
        Layout();
    }

    public GameObject GetSceneGO(string goName, string canvasName = "Canvas")
    {
        GameObject[] gos = SceneManager.GetActiveScene().GetRootGameObjects();
        GameObject canvas = null;
        GameObject go = null;
        foreach (var item in gos)
        {
            if (canvasName.Length > 0 && item.name == canvasName)
            {
                canvas = item;
                break;
            }
            if (item.name == goName)
            {
                go = item;
                break;
            }

        }
        if (canvas != null)
        {
            Component[] comps = canvas.GetComponentsInChildren<Transform>();
            foreach (var item in comps)
            {
                if (item.gameObject.name == goName)
                {
                    go = item.gameObject;
                    break;
                }
            }
        }
        return go;
    }

    void Layout()
    {
        // Поместить на экран плитки с буквами каждого возможного
        //    слова текущего уровня
        wyrds = new List<Wyrd>();

        // Объявить локальные переменные, которые будут использоваться методом
        GameObject go;
        Letter lett;
        string word;
        Vector3 pos;
        float left = 0;
        float columnWidth = 3;
        char c;
        Color col;
        Wyrd wyrd;

        // Определить, сколько рядов плиток уместится на экране
        int numRows = Mathf.RoundToInt(wordArea.height / letterSize);

        // Создать экземпляр Wyrd для каждого слова в level.subWords
        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            wyrd = new Wyrd();
            word = currLevel.subWords[i];

            // если слово длиннее, чем columnWidth, развернуть его
            columnWidth = Mathf.Max(columnWidth, word.Length);

            // создать экземпляр PrefabLetter для каждой буквы в слове
            for (int j = 0; j < word.Length; j++)
            {
                c = word[j];
                go = Instantiate<GameObject>(prefabLetter);
                go.transform.SetParent(letterAnchor);
                lett = go.GetComponent<Letter>();
                lett.c = c;

                pos = new Vector3(wordArea.x + left + j * letterSize, wordArea.y, 0);

                pos.y -= (i % numRows) * letterSize;

                // Переместить плитку lett немедленно за верхний кран экрана
                lett.posImmediate = pos + Vector3.up * (20 + i % numRows);
                // Затем начать её перемещение в новую позицию pos
                lett.pos = pos;

                // Увеличить lett.timeStart для перемещения слов в разные времена
                lett.timeStart = Time.time + i * 0.05f;

                go.transform.localScale = Vector3.one * letterSize;
                wyrd.Add(lett);
            }

            if (showAllWyrds) wyrd.visible = true;

            // Определить цвет слова, исходя из его длины
            wyrd.color = wyrdPalette[word.Length - WordList.WORD_LENGTH_MIN];
            wyrds.Add(wyrd);

            // Если достигнут последний ряд в столбце, начать новый столбец
            if(i%numRows == numRows-1)
            {
                left += (columnWidth + 0.5f) * letterSize;
            }
        }

        // Поместить на экран большие плитки с буквами
        // Инициализировать список больших букв
        bigLetters = new List<Letter>();
        bigLettersActive = new List<Letter>();

        // Создать большую плитку для каждой буквы в целевом слове
        for (int i = 0; i < currLevel.word.Length; i++)
        {
            // Напоминает процедуру создания маленьких плиток
            c = currLevel.word[i];
            go = Instantiate<GameObject>(prefabLetter);
            go.transform.SetParent(bigLetterAnchor);
            lett = go.GetComponent<Letter>();
            lett.c = c;
            go.transform.localScale = Vector3.one * bigLetterSize;

            // Первоначально поместить большие плитки ниже края экрана
            pos = new Vector3(0, -100, 0);
            lett.posImmediate = pos;
            lett.pos = pos;

            // Увеличить lett.timeStart, чтобы большие плитки
            // с буквами появились последними
            lett.timeStart = Time.time + currLevel.subWords.Count * 0.05f;
            lett.easingCuve = Easing.Sin + "-0.18"; // Bouncy easing

            col = bigColorDim;
            lett.color = col;
            lett.visible = true;
            lett.big = true;
            bigLetters.Add(lett);
        }
        // Перемешать плитки
        bigLetters = ShuffleLetters(bigLetters);

        // Вывести на экран
        ArrangeBigLetters();

        // Установить режим mode -- "в игре"
        mode = GameMode.inLevel;
    }

    // Перемешивает элементы в списке List<Letter> и возвращает результат
    List<Letter> ShuffleLetters(List<Letter> letts)
    {
        List<Letter> newL = new List<Letter>();
        int ndx;

        while (letts.Count>0)
        {
            ndx = Random.Range(0, letts.Count);
            newL.Add(letts[ndx]);
            letts.RemoveAt(ndx);
        }
        return newL;
    }

    // Выводит большие плитки на экран
    void ArrangeBigLetters()
    {
        // Найти середину для вывода больших плиток с центрированием
        // по горизонтали
        float halfWidth = ((float)bigLetters.Count) / 2f - 0.5f;
        Vector3 pos;
        for (int i = 0; i < bigLetters.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            bigLetters[i].pos = pos;
        }
        // bigLettersActive
        halfWidth = ((float)bigLettersActive.Count) / 2f - 0.5f;
        for (int i = 0; i < bigLettersActive.Count; i++)
        {
            pos = bigLetterCenter;
            pos.x += (i - halfWidth) * bigLetterSize;
            pos.y += bigLetterSize * 1.25f;
            bigLettersActive[i].pos = pos;
        }
    }

    private void Update()
    {
        // Объявить пару вспомогательных переменных
        Letter ltr;
        char c;

        switch (mode)
        {
            case GameMode.inLevel:
                // Выполнить обход всех символов, введённых игроком в этом кадре
                foreach (var item in Input.inputString)
                {
                    // Преобразовать item в верхний регистр
                    c = System.Char.ToUpperInvariant(item);

                    // Проверить, есть ли такая буква верхнего регистра
                    if (upperCase.Contains(c))
                    {
                        // Найти доступную плитку с этой буквой в bigLetters
                        ltr = FindNextLetterByChar(c);
                        // Если плитка найдена
                        if (ltr != null)
                        {
                            // ... добавить этот символ в testWord и переместить
                            //   соответствующую плитку Letter в bigLettersActive
                            PlayPressSymbol();
                            testWord += c.ToString();
                            // Переместить из списка неактивных в список активных
                            // плиток
                            bigLettersActive.Add(ltr);
                            bigLetters.Remove(ltr);
                            ltr.color = bigColorSelected; // Придать плитке активный вид
                            ArrangeBigLetters(); // Отобразить пликти
                        }
                    }

                    if (c == '\b') // Backspace
                    {
                        PlayPressSymbol();
                        // Удалить последнюю плитку Letter из bigLettersActive
                        if (bigLettersActive.Count == 0) return;
                        if(testWord.Length>1)
                        {
                            // Удалить последнюю букву из testWord
                            testWord = testWord.Substring(0, testWord.Length - 1);
                        }
                        else
                        {
                            testWord = "";
                        }

                        ltr = bigLettersActive[bigLettersActive.Count - 1];
                        // Переместить из списка активных в список неактивных плиток
                        bigLettersActive.Remove(ltr);
                        bigLetters.Add(ltr);
                        ltr.color = bigColorDim; // Придать плитке неактивный вид
                        ArrangeBigLetters(); // Отобразить плитки
                    }

                    if (c == '\n' || c == '\r') // Return/Enter macOS/Windows
                    {
                        as_return.Play();
                        // Проверить наличие сконструированного слова в WordLevel
                        CheckWord();
                    }

                    if (c == ' ') // Пробел
                    {
                        // Перемешать плитки в bigLetters
                        bigLetters = ShuffleLetters(bigLetters);
                        ArrangeBigLetters();
                    }
                }
                break;
        }
        if (Input.GetKey("escape"))
        {
            // Загрузить _Scene_0, чтобы перезапустить игру
            SceneManager.LoadScene("__WordGame_Scene_0");
        }
        
    }

    private void PlayPressSymbol()
    {
        foreach (var item in as_symbol)
        {
            if (!item.isPlaying)
            {
                item.Play();
                break;
            }
        }
    }

    // Отыскивает плитку Letter с символом c в bigLetters
    // Если такой плитки нет, возвращает null
    Letter FindNextLetterByChar(char c)
    {
        // Проверить каждую плитку Letter в bigLetters
        foreach (var item in bigLetters)
        {
            // Если содержит тот же символ, что указан в c
            if (item.c == c)
            {
                return item;
            }
        }
        return null;
    }

    public void CheckWord()
    {
        // Проверяет присутствие слова testWord в списке level.subWords
        string subWord;
        bool foundTestWord = false;

        // Создать список List<int> для хранения индексов других слов,
        //  присутствующих в testWord
        List<int> containedWords = new List<int>();

        // Обойти все слова в currLevel.subWords
        for (int i = 0; i < currLevel.subWords.Count; i++)
        {
            // Проверить, было ли уже найдено Wyrd
            if (wyrds[i].found)
            {
                continue;
            }

            subWord = currLevel.subWords[i];
            // Проверить, входит ли это слово subWord в testWord
            if (string.Equals(testWord, subWord))
            {
                HighlightWyrd(i);
                ScoreManager.SCORE(wyrds[i], 1); // Подсчитать очки
                foundTestWord = true;
            }
            else if (testWord.Contains(subWord))
            {
                containedWords.Add(i);
            }
        }

        if (foundTestWord)
        { // Если проверяемое слово присутствует в списке
            // ...подсветить другие слова, содержащиеся в testWord
            int numContained = containedWords.Count;
            int ndx;
            // Подсветить слова в обратном порядке
            for (int i = 0; i < containedWords.Count; i++)
            {
                ndx = numContained - i - 1;
                HighlightWyrd(containedWords[ndx]);
                ScoreManager.SCORE(wyrds[containedWords[ndx]], i + 2);
            }
        }

        // Очистить список активных плиток Letters, независимо от того,
        //    является ли testWord допустимым
        ClearBigLettersActive();
    }

    // Подсвечивает экземпляр Wyrd
    void HighlightWyrd(int ndx)
    {
        // Активировать слово
        wyrds[ndx].found = true; // Установить признак, что оно найдено
        // Выделить цветом
        wyrds[ndx].color = (wyrds[ndx].color + Color.white) / 2f;
        wyrds[ndx].visible = true; // Сделать компонент 3D Text видимым
    }

    // Удаляет все плитки Letters из bigLettersActive
    void ClearBigLettersActive()
    {
        testWord = ""; // Очистить testWord
        foreach (var item in bigLettersActive)
        {
            bigLetters.Add(item); // Добавить каждую плитку в bigLetters
            item.color = bigColorDim; // Предать ей неактивный вид
        }
        bigLettersActive.Clear(); // Очистить список
        ArrangeBigLetters(); // Повторно вывести плитки на экран
    }

    public void PlayMusic()
    {
        if (as_music.isPlaying)
        {
            as_music.Pause();
            bt_sound.image.sprite = img_sound_off;
        }
        else
        {
            as_music.Play();
            bt_sound.image.sprite = img_sound_on;
        }
        
    }
}
