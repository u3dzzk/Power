using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RequestTest
{
    public string savePath = "F:/UnityWebRequestTest/Test";
    public string url = "http://10.161.15.121/HtttpConent/3.%E5%A2%9E%E9%87%8F%E5%BC%8FGC.mp4";
}
public class Test : MonoBehaviour
{
    public Button btn;
    private UnityWebRequest test;

    void Start()
    {
        StartCoroutine("Request");
        btn.onClick.AddListener(() =>
        {
            test.Abort();
            print("已经停止");
        });
    }
    
    IEnumerator Request()
    {
        RequestTest request = new RequestTest();
        var loadHandler = new RIDownloadHandler(request.savePath);
        loadHandler.eventProgress += LoadProcessEvent;
        loadHandler.eventComplete += LoadCompleteEvent;
        loadHandler.eventContentLength += LoadContentEvent;
        loadHandler.eventTotalLength += LoadTotalEvent;
        
        using (var www = UnityWebRequest.Get(request.url))
        {
            test = www;
            www.chunkedTransfer = true;
            www.disposeDownloadHandlerOnDispose = true;
            www.SetRequestHeader("Range", "bytes=" + loadHandler.downloadedFileLen + "-");
            www.downloadHandler = loadHandler;
            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError)
            {
                Debug.LogFormat("【下载失败】下载文件{0}失败，失败原因：{1}", loadHandler.fileName, www.error);
                ErrorHandler(www);
                loadHandler.ErrorDispose();
            }
        }
    }

    private void LoadTotalEvent(long obj)
    {
        print(obj);
    }

    private void LoadContentEvent(long obj)
    {
        print(obj);
    }

    private void ErrorHandler(UnityWebRequest www)
    {
        print(www.error);
    }

    private void LoadCompleteEvent(string obj)
    {
        print(obj);
    }

    private void LoadProcessEvent(float obj)
    {
        print(obj);
    }
}
