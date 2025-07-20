using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuntimeLoader : MonoBehaviour
{
    public GameObject xrrig;
    [SerializeField]
    KmaxXR.ReportEvent onReport;
    [SerializeField]
    UnityEngine.UI.Button.ButtonClickedEvent onComplete;

    IEnumerator Start()
    {
        onReport?.Invoke("欢迎进入<color=yellow>动态加载示例</color>");
        yield return new WaitForSeconds(2);
        onReport?.Invoke("XR对象将在1s后加载");
        yield return new WaitForSeconds(1);
        onReport?.Invoke("正在加载");
        Instantiate(xrrig);
        onReport?.Invoke("加载完毕");
        yield return new WaitForSeconds(1);
        onComplete?.Invoke();
    }
}
