using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

    public Color normalTextColor;
    public Color warningTextColor;

    private static readonly int MAX_CHAR_IN_BUBBLE = 35;
    public static readonly char SPECIAL_CHAR = '\u00B0';
    public bool DEBUG = false;

    public GameObject timeText;
    public GameObject morseCodeText;
    public GameObject resultText;

    public Text dateText;
    private int correct;
    public Text correctText;
    public int Correct
    {
        set { correct = value; correctText.text = correct.ToString(); }
        get { return correct; }
    }
    private int wrong;
    public Text wrongText;
    public int Wrong
    {
        set { wrong = value; wrongText.text = wrong.ToString(); }
        get { return wrong; }
    }

    public GameObject letter;
    public Image telegraph;
    public Sprite telegraphUp;
    public Sprite telegraphDown;
    public AudioClip telegraphUpClip;
    public AudioClip telegraphDownClip;
    public float volume = 1f;

    private InputController input;

    public GameObject reference;
    public GameObject transitionScreen;
    public GameObject messageObj;
    public GameObject audioButton;
    public Sprite audioOn;
    public Sprite audioOff;

    public GameObject bubble;
    private List<Action> animationWaitingQueue;
    private List<Pair<Animator, Action>> animationPlayingQueue;

    private bool isGameStarted;
    private bool isTyping;
    private Levels levels;

    public void Start()
    {
        input = GetComponent<InputController>();
        input.Pause();
        input.keyDownListener = () => {
            telegraph.sprite = telegraphDown;
            telegraph.GetComponent<AudioSource>().PlayOneShot(telegraphDownClip, volume);
            EnterTyping();
        };
        input.keyUpListener = duration => {
            telegraph.sprite = telegraphUp;
            telegraph.GetComponent<AudioSource>().PlayOneShot(telegraphUpClip, volume);
            timeText.GetComponent<Text>().text = duration.ToString();
        };
        input.morseCodeListener = OnMorseCode;
        input.charListener = onCharacter;
        input.spaceKeyUpListener = () => { StartGame(); input.spaceKeyUpListener = null; };

        animationWaitingQueue = new List<Action>();
        animationPlayingQueue = new List<Pair<Animator, Action>>();

        isGameStarted = false;
        isTyping = false;

        levels = GetComponent<Levels>();
        transitionScreen.SetActive(false);
    }

    public void Update()
    {
        List<Pair<Animator, Action>> toRemove = new List<Pair<Animator, Action>>();
        animationPlayingQueue.ForEach(pair =>
        {
            if (pair._1.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
            {
                if (pair._2 != null) pair._2();
                toRemove.Add(pair);
            }
        });
        animationPlayingQueue.RemoveAll(pair => toRemove.Contains(pair));
        if (animationPlayingQueue.Count > 0)
        {
            return;
        }

        if (animationWaitingQueue.Count > 0)
        {
            animationWaitingQueue[0]();
            animationWaitingQueue.RemoveAt(0);
            return;
        }

        if (DEBUG && !timeText.activeSelf)
        {
            timeText.SetActive(true);
            morseCodeText.SetActive(true);
            resultText.SetActive(true);
        } else if (!DEBUG && timeText.activeSelf)
        {
            timeText.SetActive(false);
            morseCodeText.SetActive(false);
            resultText.SetActive(false);
        }
    }

    private StringBuilder sb = new StringBuilder();
    private void onCharacter(char c)
    {
        if (c == ' ')
        {
            string input = sb.ToString();
            OnWordFinish(input);
            sb = new StringBuilder();
            resultText.GetComponent<Text>().text = "Result: ";
        } else if (c != InputController.INVALID_CHAR)
        {
            resultText.GetComponent<Text>().text += c;
            sb.Append(c);
        }
    }
    
    public void ToggleVolume()
    {
        letter.GetComponent<AudioSource>().mute = !letter.GetComponent<AudioSource>().mute;
        telegraph.GetComponent<AudioSource>().mute = !telegraph.GetComponent<AudioSource>().mute;
        if (telegraph.GetComponent<AudioSource>().mute)
        {
            audioButton.GetComponent<Image>().sprite = audioOff;
        } else
        {
            audioButton.GetComponent<Image>().sprite = audioOn;
        }
    }

    public void ShowMessage(string title, string message, Color color)
    {
        messageObj.SetActive(true);
        Text titleText = messageObj.transform.FindChild("Title").GetComponent<Text>();
        Text messageText = messageObj.transform.FindChild("Text").GetComponent<Text>();
        titleText.text = title;
        messageText.text = message;
        titleText.color = color;
        messageText.color = color;
        messageObj.GetComponent<Animator>().Play("MessageEntry");
        animationPlayingQueue.Add(new Pair<Animator, Action>(messageObj.GetComponent<Animator>(), null));
    }

    private void OnMorseCode(char c)
    {
        morseCodeText.GetComponent<Text>().text += c;

        if (c != InputController.WORD_BREAK)
        {
            Text bubbleText = bubble.GetComponentInChildren<Text>();
            string currentText = bubbleText.text;
            int newLineIndex = currentText.LastIndexOf('\n');
            int currentLineLength = newLineIndex < 0 ? currentText.Length : (currentText.Substring(newLineIndex + 1).Length);
            if (currentLineLength >= MAX_CHAR_IN_BUBBLE)
            {
                bubbleText.text += '\n';
            }

            if (c == '.')
            {
                bubbleText.text += SPECIAL_CHAR;
            } else if (c == InputController.LETTER_BREAK)
            {
                bubbleText.text += "  ";
            } else
            {
                bubbleText.text += c;
            }
        }
    }

    private void OnWordFinish(string word)
    {
        if (levels.IsCorrect(word))
        {
            Correct++;
            bubble.GetComponentInChildren<Text>().color = Color.green;
            ExitTyping(levels.Succeed);
        }
        else
        {
            Wrong++;
            bubble.GetComponentInChildren<Text>().color = Color.red;
            ExitTyping(levels.Failed);
        }
    }

    public void StartGame()
    {
        if (isGameStarted)
        {
            return;
        }

        isGameStarted = true;
        letter.GetComponent<AudioSource>().PlayOneShot(letter.GetComponent<AudioSource>().clip, volume);
        QueueAnimation(letter.GetComponent<Animator>(), "LetterExit", () => { letter.SetActive(false);  levels.FirstLevel(); });
    }

    private void EnterTyping()
    {
        if (isTyping)
        {
            return;
        }

        isTyping = true;
        bubble.GetComponentInChildren<Text>().text = "";
        bubble.GetComponent<Animator>().Play("BubbleEntry");
        animationPlayingQueue.Add(new Pair<Animator, Action>(bubble.GetComponent<Animator>(), null));
    }

    private void ExitTyping(Action cb)
    {
        if (!isTyping)
        {
            return;
        }

        isTyping = false;
        bubble.GetComponent<Animator>().Play("BubbleExit");
        animationPlayingQueue.Add(new Pair<Animator, Action>(bubble.GetComponent<Animator>(), () =>
        {
            bubble.GetComponentInChildren<Text>().color = Color.black;
            cb();
        }));
    }

    public void ShowLetter(string title, string content, Action cb)
    {
        input.Pause();
        letter.SetActive(true);
        Text titleText = letter.transform.FindChild("Title").GetComponent<Text>();
        Text messageText = letter.transform.FindChild("Content").GetComponent<Text>();
        titleText.text = title;
        messageText.text = content;
        letter.GetComponent<AudioSource>().PlayOneShot(letter.GetComponent<AudioSource>().clip, volume);
        QueueAnimation(letter.GetComponent<Animator>(), "LetterEntry", () => {
            input.spaceKeyUpListener = () => {
                input.spaceKeyUpListener = null;
                letter.GetComponent<AudioSource>().PlayOneShot(letter.GetComponent<AudioSource>().clip, volume);
                QueueAnimation(letter.GetComponent<Animator>(), "LetterExit", () => { input.Resume(); cb(); });
            };
        });
    }

    public void QueueAnimation(Animator animator, string startState, Action cb = null)
    {
        animationWaitingQueue.Add(() => {
            animationPlayingQueue.Add(new Pair<Animator, Action>(animator, cb));
            animator.Play(startState);
        });
    }

    public void LevelTransition(int level, Action cb)
    {
        transitionScreen.SetActive(true);
        messageObj.SetActive(false);
        dateText.text = "Day " + (level + 1);
        transitionScreen.GetComponentInChildren<Text>().text = "- Day " + (level + 1) + " -";
        if (level == 3)
        {
            reference.SetActive(true);
        }

        QueueAnimation(transitionScreen.GetComponent<Animator>(), "LevelTransition", () => {
            transitionScreen.SetActive(false);
            cb();
        });
    }
}
