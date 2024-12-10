using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PostTool : MonoBehaviour
{
    [Header("Google Post")]
    public string googleSheetUrl;
    public List<string> postIds;

    public async void Post(string msg, string imgurl){
        WWWForm form = new WWWForm();

        form.AddField("entry." + postIds[0], msg);      //Copy from google form origin html code
        form.AddField("entry." + postIds[1], imgurl);   //Copy from google form origin html code

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
                //Debug.Log("Form upload complete!");
                //Debug.Log(www.downloadHandler.text);
            }
        }
    }
}
