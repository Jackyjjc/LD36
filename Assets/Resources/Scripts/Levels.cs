using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text;
using System;

public class Levels : MonoBehaviour
{
    private GameController game;
    private InputController input;
    public void Awake()
    {
        game = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
        input = GameObject.FindGameObjectWithTag("GameController").GetComponent<InputController>();
    }

    public void Start()
    {
        job.SetActive(false);
        this.level = 0;
        this.subLevel = 0;
        this.performance = 0;
        isRebelBaseReported = true;
        isRebelsKilled = true;
    }
    private string[] briefings =
    {
        "",
        "Your training has finished. You will be handed with real message today.",
        "We have upgraded our telegraph receivers at the stations. Mutiple words can now be transmitted over the wire.",
        "Due to lack to staff, messages are not longer annotated.",
        "We are executing a military plan to wipe out the rebels today. You must delivery those military messages with 100% accuracy."
    };
    private Action[][] levels;
    public Levels()
    {
        levels = new Action[][] {
            // Day1
            new Action[] {
                () => ShowJob("Training 1", "Send the following message:", "A", "Press SPACE to send " + GameController.SPECIAL_CHAR + " and long press to send -", hint: true),
                () => ShowJob("Training 2", "Send the following message:", "AE", "Wait a bit before transmitting the next character", hint: true),
                () => ShowJob("Training 3", "Send the following message:", "ALP", "All Hail the Party!", hint: true)
            },
            // Day2
            new Action[]
            {
                () => ShowJob("Weather Forcast", "Send out the weather forcast for today:", "SNOW", hint: true),
                () => ShowJob("Celebration", "Send out the planned date for \"We love the Party day\"", "5TH", hint: true),
                () => ShowJob("Education", "Send out the name of the book that was recently banned", "1984", hint: true),
                () => { ShowJob("News Report", "Report on how do citizens feel today", "JOY", hint: true,
                    cb: () => game.ShowMessage("", "You don't really believe in this crap they told you to send out, do you?", game.normalTextColor)); },
                () => ShowJob("All Hail the Party!", "Send the following message:", "ALP", hint: true)
            },
            // Day3
            new Action[]
            {
                () => ShowJob("Training 4", "Test with the following message:", "I AM", "You should only transmit ONE WORD at a time. Wait for the previous to finish before typing the next one.", hint: true),
                () => { ShowJob("Police", "Broadcast the name of the suburb that is being locked down:", "D ZONE", hint: true, 
                    cb: () => game.ShowMessage("", "You know you don't need to send out what they told you to, right?", game.normalTextColor));},
                () => ShowJob("News Report", "Report what caused the lockdown:", "A REBELION", hint: true),
                () => ShowJob("All Hail the Party!", "Send the following message:", "ALP", hint: true)
            },
            // Day4
            new Action[]
            {
                () => ShowJob("News Report", "Report the number of rebels we have killed in D district:", "9"),
                () => ShowJob("Police Report (CLASSIFIED)", "Reply to the proposal of citizen surveillance program from the head of police:", "PASS",
                    cb: () => game.ShowMessage("", "We need your help!", game.warningTextColor)),
                () => ShowJob("All Hail the Party!", "Send the following message:", "ALP")
            },
            // Day5
            new Action[]
            {
                () => ShowJob("Millitary (CLASSIFIED)", "Send military base location of the rebels to the army:", "WEST"),
                () => ShowJob("Millitary (CLASSIFIED)", "Send the estimated number of rebels:", "500"),
                () => ShowJob("All Hail the Party!", "Send the following message:", "ALP")
            }
        };
    }

    public GameObject job;
    private int level;
    private int subLevel;
    private string[] goal;
    private int goalIndex;
    private bool failedBefore;
    private int performance;
    private bool isRebelBaseReported;
    private bool isRebelsKilled;
    
    public void FirstLevel()
    {
        input.Resume();
        levels[level][subLevel]();
    }

    public void NextLevel()
    {
        if (subLevel + 1 >= levels[level].Length)
        {
            level++;
            subLevel = 0;
            job.SetActive(false);
            game.LevelTransition(level, () => {
                string review = "";
                if (game.Wrong == 0)
                {
                    review = "You have correctly sent out all of the messages yesterday. Keep up the good work!";
                    performance++;
                } else if (game.Correct == 0)
                {
                    review = "If you fail again, the ministry will take actions.";
                    performance -= 2;
                } else if (game.Wrong > 0 && !failedBefore)
                {
                    failedBefore = true;
                    review = "You have made some mistakes yesterday. Remember, deliberately tempering the messages is a serious crime. Think about your family, comrade.";
                } else if (game.Wrong == game.Correct)
                {
                    review = "The ministry is expecting more from you comrade, you need to improve your accuracy.";
                } else if (game.Wrong > game.Correct)
                {
                    review = "You are not living up to the expectation of the ministry. We hope you can improve your performance before we take further actions.";
                    performance--;
                } else if (game.Wrong > 0)
                {
                    review = "Please improve your correctness or we will find someone to replace you.";
                }

                //Debug.Log("perf:" + performance);

                if (level == 0)
                {
                    performance = performance < 0 ? 0 : performance;
                }

                if (performance < -2)
                {
                    // Game Over
                    game.ShowLetter(string.Format("ARRESTED", level), "You have been found guilty of continously tempering our telegraphic messages. You and your family are now under arrested.\n\nThe Ministry of Truth", () => {
                        SceneManager.LoadScene("Main");
                    });
                } else if (level == levels.Length)
                {
                    // Win
                    if (!isRebelBaseReported)
                    {
                        game.ShowLetter(string.Format("ARRESTED", level), "Comrade,\n\nOur recent military plan to wipe out the rebel was sabotaged. As a result, these dirty rebels are now gaining strength in our major cities. You know what you have done, comrade. You and your family are now under arrested\n\nThe Ministry of Truth", () => {
                            SceneManager.LoadScene("Main");
                        });
                    } else if (!isRebelsKilled)
                    {
                        game.ShowLetter(string.Format("ARRESTED", level), "Comrade,\n\nWe have enough evidence to belive that you have been communicating with the rebels that we have recently captured. You and your family are now under arrested.\n\nThe Ministry of Truth", () => {
                            SceneManager.LoadScene("Main");
                        });
                    } else
                    {
                        game.ShowLetter(string.Format("Retire", level), "Comrade,\n\nGiven the current circustances, the Party no longer requires your service. You are now retired.\n\nThe Ministry of Truth", () => {
                            SceneManager.LoadScene("Main");
                        });
                    }
                } else
                {
                    game.ShowLetter(string.Format("Day {0} Report", level), string.Format("Comrade,\n\nHere is your performance yesterday:\n\nCorrect: {0}, Wrong: {1}\n\n{2}\n\n<b>IMPORTANT NOTICE:</b>\n{3}\n\nGood Luck\n\nThe Ministry of Truth", game.Correct, game.Wrong, review, briefings[level]), () => {
                        game.Correct = 0;
                        game.Wrong = 0;
                        StartLevel(level, subLevel);
                    });
                }
            });
        }
        else
        {
            subLevel++;
            StartLevel(level, subLevel);
        }
    }

    public void Succeed()
    {
        if (goalIndex >= goal.Length - 1)
        {
            NextLevel();
        } else
        {
            goalIndex++;
        }
    }

    public void Failed()
    {
        if (level == 0)
        {
            StartLevel(level, subLevel);
            Text messageText = job.transform.FindChild("Text").GetComponent<Text>();
            messageText.text = messageText.text.Replace("Send the following message:", "Send the following message again:");
        } else
        {
            if (level == 4 && subLevel == 0)
            {
                isRebelBaseReported = false;
            } else if (level == 4 && subLevel == 1)
            {
                isRebelsKilled = false;
            }

            NextLevel();
        }
    }

    private void StartLevel(int level, int subLevel)
    {
        if (level < levels.Length && subLevel < levels[level].Length)
        {
            levels[level][subLevel]();
        }
    }

    public void ShowJob(string title, string prompt, string message, string extraMessage = null, bool hint = false, Action cb = null)
    {
        job.SetActive(true);
        this.goal = message.Split(' ');
        this.goalIndex = 0;

        Text titleText = job.transform.FindChild("Title").GetComponent<Text>();
        Text messageText = job.transform.FindChild("Text").GetComponent<Text>();
        titleText.text = title;
        messageText.color = game.normalTextColor;
        messageText.text = prompt + "\n\n"
            + message
            + (hint ? "\n\n" + MessageToMorseCode(message) : "")
            + (extraMessage != null ? "\n\n" + extraMessage : "");

        job.GetComponent<AudioSource>().PlayDelayed(0.5f);
        game.QueueAnimation(job.GetComponent<Animator>(), "Play", cb);
    }

    public void ToggleVolume()
    {
        job.GetComponent<AudioSource>().mute = !job.GetComponent<AudioSource>().mute;
    }

    public bool IsCorrect(string word)
    {
        return word.Equals(goal[goalIndex]);
    }

    private string MessageToMorseCode(string message)
    {
        StringBuilder sb = new StringBuilder();
        foreach (char c in message)
        {
            if (c == ' ')
            {
                sb.Append(' ');
            }
            else
            {
                sb.Append('(');
                sb.Append(InputController.morseCodeReverseMap[c].Replace('.', GameController.SPECIAL_CHAR));
                sb.Append(')');
            }
        }
        return sb.ToString();
    }
}
