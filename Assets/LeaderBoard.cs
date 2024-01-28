using System.Linq;
using TMPro;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{
    public TextMeshProUGUI text;

    // Update is called once per frame
    void Update()
    {
        if (ScoreManager.managers == null || ScoreManager.managers.Count == 0)
            return;

        ScoreManager.managers.Sort();
        RefreshText(ScoreManager.managers.Select(manager => manager.photonView.Controller.NickName).ToArray());
    }

    
    void RefreshText(string[] leadersNames)
    {
        string leaders = "";

        for (int i = 0; i < Mathf.Min(5, leadersNames.Length); i++)
            leaders += $"{i + 1}. {leadersNames[i]}\n";

        text.text = leaders;
    }

    
}
