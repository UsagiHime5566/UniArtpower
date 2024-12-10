using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace HimeLib
{
    public class PostToGoogleSheet
    {
        public string googleSheetUrl = "https://docs.google.com/forms/u/0/d/e/Q1FAIpQLScy7Q1J0RpfE8hiteEcwpizHPSZobvFNh9ZTmwhsg_wA8bRsg/formResponse";   //Copy from google form origin html code

        /// <summary>
        /// 發送資料至Google Sheet
        /// </summary>
        public void Post(string dummy, MonoBehaviour instance = null){
            if(instance != null)
                instance.StartCoroutine(PostTool(dummy));
            else
            {
                instance = new GameObject("Post To Google").AddComponent<MonoBehaviour>();
                instance.StartCoroutine(PostToolInstance(dummy, instance));
            }
        }

        IEnumerator PostToolInstance(string dummy, MonoBehaviour instance){
            yield return PostTool(dummy);
            MonoBehaviour.Destroy(instance.gameObject);
        }

        IEnumerator PostTool(string dummy)
        {
            string times = "";
            string diff = "";
            string circle = "";
            string result = "";

            WWWForm form = new WWWForm();
            form.AddField("entry.1772429409", times);   //Copy from google form origin html code
            form.AddField("entry.691837459", diff);     //Copy from google form origin html code
            form.AddField("entry.578058756", circle);   //Copy from google form origin html code
            form.AddField("entry.968035340", result);   //Copy from google form origin html code

            using (UnityWebRequest www = UnityWebRequest.Post(googleSheetUrl, form))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    Debug.Log("Form upload complete!");
                    //Debug.Log(www.downloadHandler.text);
                }
            }
        }
    }
}