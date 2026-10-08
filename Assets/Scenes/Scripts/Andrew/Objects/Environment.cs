using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public struct DisplayTextResponse
{
    public List<string> displayTexts;
    public PlayerIntentSO playerIntent;
}

public class Environment : Object_
{
    public List<DisplayTextResponse> displayTextResponses = new();

    public override void ReceivePlayerAction(PlayerIntentSO intent)
    {
        base.ReceivePlayerAction(intent);
        foreach (var response in displayTextResponses)
        {
            if (response.playerIntent == intent)
            {
                GameManager.Instance.LogDisplayTextsToRecentLog(response.displayTexts);
                
                foreach(var text in response.displayTexts)
                {
                    Debug.Log(text);
                }

                return;
            }
        }
    }
}
