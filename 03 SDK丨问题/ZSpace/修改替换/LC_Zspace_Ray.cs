using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using zSpace.Core.Input;

/// <summary>
/// 射线 
/// </summary>
public class LC_Zspace_Ray : MonoBehaviour
{


    /// <summary>
    /// 触控笔 
    /// </summary>
    public ZStylus zStylus;

    /// <summary>
    /// 线
    /// </summary>
    public GameObject line;

    /// <summary>
    /// 线的末端
    /// </summary>
    public GameObject lineEndPoint;

    /// <summary>
    ///  终点的圆球 （碰撞体）
    /// </summary>
    public GameObject endPoint;


    Vector3 oldLocalScale;


    private Vector3 lastPosition;
    private float stopTimer;
    public Transform Rayline;

    private void Start()
    {
        oldLocalScale = endPoint.transform.localScale;


        lastPosition = zStylus.transform.position;
        foreach (Transform item in transform)
        {
            item.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        endPoint.transform.localScale = new Vector3(RaySetting.endPointSize, RaySetting.endPointSize, RaySetting.endPointSize);


        float dis = Vector3.Distance(lastPosition, zStylus.transform.position);
        if (dis >= 0 && dis < 0.05)
        {
            stopTimer += Time.deltaTime;
            if (stopTimer >= RaySetting.rayHidTime)
            {
                foreach (Transform item in transform)
                {
                    item.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            stopTimer = 0f;
            lastPosition = zStylus.transform.position;
            foreach (Transform item in transform)
            {
                item.gameObject.SetActive(true);
            }
        }


        //修改线的位置
        line.transform.position = zStylus.transform.position;
        //修改线的朝向
        line.transform.eulerAngles = zStylus.transform.eulerAngles;

        // 计算 笔的射线的照射到的点的位置 与线的初始位置的差值
        float distance = Vector3.Distance(zStylus.HitInfo.worldPosition, line.transform.position);

        //差值 决定了线的长度
        line.transform.localScale = new Vector3(1, 1, distance);

        //修改 终点 球的大小 实现 近小 远大效果
        //endPoint.transform.localScale = oldLocalScale * distance * 2;

        //修改 终点 球的位置
        endPoint.transform.position = lineEndPoint.transform.position;

    }
}
