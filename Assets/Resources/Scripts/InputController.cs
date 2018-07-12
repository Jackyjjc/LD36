#define UNITY_IOS
#undef UNITY_STANDALONE

using UnityEngine;
using System.Collections.Generic;
using System.Text;
using System;

public class InputController : MonoBehaviour
{
    public static readonly char INVALID_CHAR = 'x';

    private static readonly float DOT_TIME = 0.25f;
    private static readonly float LETTER_BREAK_TIME = 4 * DOT_TIME;
    private static readonly float WORD_BREAK_TIME = 2 * LETTER_BREAK_TIME;
    public static readonly char LETTER_BREAK = '*';
    public static readonly char WORD_BREAK = '|';

    public static readonly Dictionary<string, char> morseCodeMap = new Dictionary<string, char>()
    {
        { ".-", 'A' },
        { "-...", 'B' },
        { "-.-.", 'C' },
        { "-..", 'D' },
        { ".", 'E' },
        { "..-.", 'F' },
        { "--.", 'G' },
        { "....", 'H' },
        { "..", 'I' },
        { ".---", 'J' },
        { "-.-", 'K' },
        { ".-..", 'L' },
        { "--", 'M' },
        { "-.", 'N' },
        { "---", 'O' },
        { ".--.", 'P' },
        { "--.-", 'Q' },
        { ".-.", 'R' },
        { "...", 'S' },
        { "-", 'T' },
        { "..-", 'U' },
        { "...-", 'V' },
        { ".--", 'W' },
        { "-..-", 'X' },
        { "-.--", 'Y' },
        { "--..", 'Z' },
        { ".----", '1' },
        { "..---", '2' },
        { "...--", '3' },
        { "....-", '4' },
        { ".....", '5' },
        { "-....", '6' },
        { "--...", '7' },
        { "---..", '8' },
        { "----.", '9' },
        { "-----", '0' },
    };

    public static readonly Dictionary<char, string> morseCodeReverseMap = new Dictionary<char, string>()
    {
        { 'A', ".-" },
        { 'B', "-..." },
        { 'C', "-.-." },
        { 'D', "-.." },
        { 'E', "." },
        { 'F', "..-." },
        { 'G', "--." },
        { 'H', "...." },
        { 'I', ".." },
        { 'J', ".---" },
        { 'K', "-.-" },
        { 'L', ".-.." },
        { 'M', "--" },
        { 'N', "-." },
        { 'O', "---" },
        { 'P', ".--." },
        { 'Q', "--.-" },
        { 'R', ".-." },
        { 'S', "..." },
        { 'T', "-" },
        { 'U', "..-" },
        { 'V', "...-" },
        { 'W', ".--" },
        { 'X', "-..-" },
        { 'Y', "-.--" },
        { 'Z', "--.." },
        { '1', ".----" },
        { '2', "..---" },
        { '3', "...--" },
        { '4', "....-" },
        { '5', "....." },
        { '6', "-...." },
        { '7', "--..." },
        { '8', "---.." },
        { '9', "----." },
        { '0', "-----" },
    };

    bool isPaused;
    bool isKeyDown;
    bool isFirstChar;
    bool isLastCharLetterBreak;
    bool isLastCharWordBreak;
    float keyDownTime;
    float keyUpTime;

    public Action spaceKeyUpListener;
    public Action keyDownListener;
    public Action<float> keyUpListener;
    public Action<char> morseCodeListener;
    public Action<char> charListener;

    void Start()
    {
        isPaused = true;
        isKeyDown = false;
        isFirstChar = true;
        isLastCharLetterBreak = false;
        isLastCharWordBreak = false;
    }

    void Update()
    {
        if (isPaused)
        {
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
            if (Input.GetKeyUp("space") && spaceKeyUpListener != null)
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
            if (isTouchUp() && spaceKeyUpListener != null)
#endif
            {
                spaceKeyUpListener();
            }
            return;
        }

        float currentTime = Time.time;

#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
        if (Input.GetKeyDown("space"))
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        if (isTouchDown())
#endif
        {
            // TODO: see if i can easily get more accurate time
            isKeyDown = true;
            keyDownTime = currentTime;
            if (keyDownListener != null) keyDownListener();
        }
#if UNITY_STANDALONE || UNITY_WEBPLAYER || UNITY_WEBGL
        else if (Input.GetKeyUp("space"))
#elif UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        else if (isTouchUp())
#endif
        {
            isKeyDown = false;
            keyUpTime = currentTime;
            if (keyUpListener != null) keyUpListener(currentTime - keyDownTime);

            if (currentTime - keyDownTime < DOT_TIME)
            {
                OnMorseCodeCharacter('.');
            }
            else
            {
                OnMorseCodeCharacter('-');
            }
            isFirstChar = false;
            isLastCharLetterBreak = false;
            isLastCharWordBreak = false;
        }
        else if (!isKeyDown && !isFirstChar)
        {
            if (currentTime - keyUpTime >= LETTER_BREAK_TIME && !isLastCharLetterBreak)
            {
                isLastCharLetterBreak = true;
                OnMorseCodeCharacter(LETTER_BREAK);
            }

            if (currentTime - keyUpTime >= WORD_BREAK_TIME && !isLastCharWordBreak)
            {
                isLastCharWordBreak = true;
                OnMorseCodeCharacter(WORD_BREAK);
            }
        }
    }

    private bool isTouchDown()
    {
        if (Input.touchCount <= 0)
        {
            return false;
        }

        return Input.GetTouch(0).phase == TouchPhase.Began;
    }

    private bool isTouchUp()
    {
        if (Input.touchCount <= 0)
        {
            return false;
        }

        return Input.GetTouch(0).phase == TouchPhase.Ended;
    }

    private StringBuilder sb = new StringBuilder();
    private void OnMorseCodeCharacter(char code)
    {
        if (morseCodeListener != null) morseCodeListener(code);

        char c;
        if (code == WORD_BREAK)
        {
            c = ' ';
            if (charListener != null) charListener(c);
        } else if (code == LETTER_BREAK)
        {
            string morseCodeChar = sb.ToString();
            if (morseCodeMap.ContainsKey(morseCodeChar))
            {
                c = morseCodeMap[morseCodeChar];
            } else
            {
                c = INVALID_CHAR;
            }
            if (charListener != null) charListener(c);
            sb = new StringBuilder();
        } else
        {
            sb.Append(code);
        }
    }

    public void Pause()
    {
        this.isPaused = true;
    }

    public void Resume()
    {
        this.isPaused = false;
    }
}
