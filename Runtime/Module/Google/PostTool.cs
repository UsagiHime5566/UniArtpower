using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PostTool : MonoBehaviour
{
    [Header("Google Post")]
    public string googleSheetUrl;
    public List<string> formIds;
    public bool LogMessage = false;

    public async void Post(List<string> messages){
        WWWForm form = new WWWForm();

        for (int i = 0; i < messages.Count; i++)
        {
            if(messages.Count <= formIds.Count){
                form.AddField("entry." + formIds[i], messages[i]);
            }
        }

        using (UnityWebRequest www = UnityWebRequest.Post(googleSheetUrl, form))
        {
            var asyncOperation = www.SendWebRequest();

            while (!asyncOperation.isDone)
            {
                await System.Threading.Tasks.Task.Delay(100);
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
            }
            else
            {
                if(LogMessage) Debug.Log($"Form upload complete: {www.downloadHandler.text}");
            }
        }
    }
}
