using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CotcSdk;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json.Linq;

public class Xtralife
{
    public string gamerId;
    public string networkId;
    public Gamer currentGamer;
    public GamerOutline gamerOutline;
    public GamerProfile gamerProfile;
    public long userCurrentScore = 0;
    public string[] leaderboardName = new string[8];
    public long[] leaderboardScore = new long[8];

    // LOGIN
    public void LoginAnonymous(CotcGameObject cotc)
    {
        cotc.GetCloud().Done(cloud => {
            cloud.LoginAnonymously()
            .Done(gamer => {
                gamerId = gamer.GamerId;
                networkId = gamer.NetworkId;
                currentGamer = gamer;
                SaveGamer();
                FetchOutline();
                GetProfileData();
                UserBestScores();
                GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = true;
                GameObject.Find("MenuXTR4L1F3Login").GetComponent<Canvas>().enabled = false;
                GameObject.Find("ErrorLogIn").GetComponent<Text>().enabled = false;
            }, ex => {
                // The exception should always be CotcException
                CotcException error = (CotcException)ex;
                Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
                GameObject.Find("ErrorLogIn").GetComponent<Text>().enabled = true;
            });
        });
    }

    public void Login(CotcGameObject cotc, string email, string password)
    {
        cotc.GetCloud().Done(cloud => {
            cloud.Login(
                network: "email",
                networkId: email,
                networkSecret: password)
            .Done(gamer => {
                gamerId = gamer.GamerId;
                networkId = gamer.NetworkId;
                currentGamer = gamer;
                SaveGamer();
                FetchOutline();
                GetProfileData();
                UserBestScores();
                GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = true;
                GameObject.Find("MenuXTR4L1F3Login").GetComponent<Canvas>().enabled = false;
                GameObject.Find("ErrorLogIn").GetComponent<Text>().enabled = false;
            }, ex => {
                // The exception should always be CotcException
                CotcException error = (CotcException)ex;
                Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
                GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = false;
                GameObject.Find("MenuXTR4L1F3Login").GetComponent<Canvas>().enabled = true;
                GameObject.Find("ErrorLogIn").GetComponent<Text>().enabled = true;
            });
        });
    }

    public void ResumeSession(CotcGameObject cotc, string gamer_id, string gamer_secret)
    {
        cotc.GetCloud().Done(cloud => {
            cloud.ResumeSession(
                gamerId: gamer_id,
                gamerSecret: gamer_secret)
            .Done(gamer => {
                gamerId = gamer.GamerId;
                networkId = gamer.NetworkId;
                currentGamer = gamer;
                SaveGamer();
                FetchOutline();
                GetProfileData();
                UserBestScores();
                GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = true;
                GameObject.Find("MenuXTR4L1F3Login").GetComponent<Canvas>().enabled = false;
                GameObject.Find("ErrorLogIn").GetComponent<Text>().enabled = false;
            }, ex => {
                // The exception should always be CotcException
                CotcException error = (CotcException)ex;
                Debug.LogError("Failed to login: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
                GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = false;
                GameObject.Find("MenuXTR4L1F3Login").GetComponent<Canvas>().enabled = true;
                GameObject.Find("ErrorLogIn").GetComponent<Text>().enabled = true;
            });
        });
    }

    public void Logout(CotcGameObject cotc)
    {
        cotc.GetCloud().Done(cloud => {
            cloud.Logout(currentGamer)
            .Done(result => {
                Debug.Log("Logout succeeded");
                GameObject.Find("MenuMainCanvas").GetComponent<Canvas>().enabled = false;
                GameObject.Find("MenuXTR4L1F3Login").GetComponent<Canvas>().enabled = true;
            }, ex => {
                // The exception should always be CotcException
                CotcException error = (CotcException)ex;
                Debug.LogError("Failed to logout: " + error.ErrorCode + " (" + error.HttpStatusCode + ")");
            });
        });
    }

    // DATA

    public void FetchOutline()
    {
        currentGamer.Profile.Outline()
        .Done(outlineRes => {
            gamerOutline = outlineRes;
        }, ex => {
            // The exception should always be CotcException
            CotcException error = (CotcException)ex;
            Debug.LogError("Outline failed due to error: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
        });
    }

    public void GetProfileData()
    {
        currentGamer.Profile.Get()
        .Done(profileRes => {
            gamerProfile = profileRes;
            JToken token = JObject.Parse(profileRes.ToJson());
            string name = (string)token.SelectToken("displayName");
            GameObject.Find("PlayerName").GetComponent<Text>().text = name;
        }, ex => {
            // The exception should always be CotcException
            CotcException error = (CotcException)ex;
            Debug.LogError("Could not get profile data due to error: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
        });
    }

    // LEADERBOARDS

    public void PostScore(int value)
    {
        currentGamer.Scores.Domain("private").Post(value, "MapCreated", ScoreOrder.HighToLow,
        "context for score", false)
        .Done(postScoreRes => {
            Debug.Log("Post score: " + postScoreRes.ToString());
        }, ex => {
            // The exception should always be CotcException
            CotcException error = (CotcException)ex;
            Debug.LogError("Could not post score: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
        });
    }

    public void BestHighScores()
    {
        currentGamer.Scores.Domain("private").BestHighScores("MapCreated", 10, 1)
        .Done(bestHighScoresRes => {
            for (int i = 0; i < ((bestHighScoresRes.Count < 6) ? bestHighScoresRes.Count : 6); i++)
            {
                leaderboardName[i] = bestHighScoresRes[i].GamerInfo["profile"]["displayName"];
                leaderboardScore[i] = bestHighScoresRes[i].Value;
            }
            GameObject namesGo = GameObject.Find("LeaderboardNames");
            GameObject scoresGo = GameObject.Find("LeaderboardScores");
            foreach (string name in leaderboardName)
            {
                namesGo.GetComponent<Text>().text += name + "\n\n";
            }
            foreach (long score in leaderboardScore)
            {
                scoresGo.GetComponent<Text>().text += score + "\n\n";
            }
        }, ex => {
            // The exception should always be CotcException
            CotcException error = (CotcException)ex;
            Debug.LogError("Could not get best high scores: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
        });
    }

    public void UserBestScores()
    {
        currentGamer.Scores.Domain("private").ListUserBestScores()
        .Done(listUserBestScoresRes => {
            foreach (var score in listUserBestScoresRes)
                userCurrentScore = score.Value.Value;
        }, ex => {
            // The exception should always be CotcException
            CotcException error = (CotcException)ex;
            Debug.LogError("Could not get user best scores: " + error.ErrorCode + " (" + error.ErrorInformation + ")");
        });
    }

    // UTILITIES

    private int SaveGamer()
    {
        //Save
        try
        {
            DeleteCurrentSaveFile();
            File.WriteAllText(Application.persistentDataPath + "/" + gamerId + ".json", currentGamer.ToJson());
            return 0;
        }
        catch (IOException ex)
        {
            Debug.LogError("Failed to save login: " + ex.Message);
        }
        return 1;
    }

    public void DeleteCurrentSaveFile()
    {
        foreach(string fileStr in Directory.GetFiles(Application.persistentDataPath))
        {
            File.Delete(fileStr);
        }
    }

    public bool isSaveFileAlreadyCreated()
    {

        if(Directory.GetFiles(Application.persistentDataPath).Length > 0)
        {
            return true;
        }
        return false;
    }
}
