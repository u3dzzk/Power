using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class RIDownloadHandler  : DownloadHandlerScript
{
        string m_SavePath = "";
        string m_TempFilePath = "";
        FileStream fs;

        public long totalFileLen { get; private set; }
        public long downloadedFileLen { get; private set; }
        public string fileName { get; private set; }
        public string dirPath { get; private set; }

        #region 事件
        /// <summary>
        /// 返回这条URL下需要下载的文件的总大小
        /// </summary>
        public event Action<long> eventTotalLength = null;

        /// <summary>
        /// 返回这次请求时需要下载的大小（即剩余文件大小）
        /// </summary>
        public event Action<long> eventContentLength = null;

        /// <summary>
        /// 每次下载到数据后回调进度
        /// </summary>
        public event Action<float> eventProgress = null;

        /// <summary>
        /// 当下载完成后回调下载的文件位置
        /// </summary>
        public event Action<string> eventComplete = null;
        #endregion

        /// <summary>
        /// 初始化下载句柄，定义每次下载的数据上限为200kb
        /// </summary>
        /// <param name="filePath">保存到本地的文件路径</param>
        public RIDownloadHandler(string filePath) : base(new byte[1024 * 200])
        {
            m_SavePath = filePath.Replace('\\', '/');
            fileName = m_SavePath.Substring(m_SavePath.LastIndexOf('/') + 1);
            dirPath = m_SavePath.Substring(0, m_SavePath.LastIndexOf('/'));
            m_TempFilePath = Path.Combine(dirPath, fileName + ".temp");

            fs = new FileStream(m_TempFilePath, FileMode.Append, FileAccess.Write);
            downloadedFileLen = fs.Length;
        }

        /// <summary>
        /// 请求下载时的第一个回调函数，会返回需要接收的文件总长度
        /// </summary>
        /// <param name="contentLength">如果是续传，则是剩下的文件大小；本地拷贝则是文件总长度</param>
        protected override void ReceiveContentLength(int contentLength)
        {
            if (contentLength == 0)
            {
                Debug.Log("【下载已经完成】");
                CompleteContent();
            }
            totalFileLen = contentLength + downloadedFileLen;
            eventTotalLength?.Invoke(totalFileLen);
            eventContentLength?.Invoke(contentLength);
        }

        /// <summary>
        /// 从网络获取数据时候的回调，每帧调用一次
        /// </summary>
        /// <param name="data">接收到的数据字节流，总长度为构造函数定义的200kb，并非所有的数据都是新的</param>
        /// <param name="dataLength">接收到的数据长度，表示data字节流数组中有多少数据是新接收到的，即0-dataLength之间的数据是刚接收到的</param>
        /// <returns>返回true为继续下载，返回false为中断下载</returns>
        protected override bool ReceiveData(byte[] data, int dataLength)
        {
            if(data == null || data.Length == 0)
            {
                Debug.LogFormat("【下载中】<color=yellow>下载文件{0}中，没有获取到数据，下载终止</color>", fileName);
                return false;
            }
            fs?.Write(data, 0, dataLength);
            downloadedFileLen += dataLength;

            var progress = (float)downloadedFileLen / totalFileLen;
            eventProgress?.Invoke(progress);

            return true;
        }

        /// <summary>
        /// 当接受数据完成时的回调
        /// </summary>
        protected override void CompleteContent()
        {
            Debug.LogFormat("【下载完成】<color=green>完成对{0}文件的下载，保存路径为{1}</color>", fileName, m_SavePath);
            fs.Close();
            fs.Dispose();
            if (File.Exists(m_TempFilePath))
            {
                if (File.Exists(m_SavePath))
                    File.Delete(m_SavePath);
                File.Move(m_TempFilePath, m_SavePath);
            }
            else
            {
                Debug.LogFormat("【下载失败】<color=red>下载文件{0}时失败</color>", fileName);
            }
            eventComplete?.Invoke(m_SavePath);
        }

        public void ErrorDispose()
        {
            fs.Close();
            fs.Dispose();
            if (File.Exists(m_TempFilePath))
            {
                //File.Delete(m_TempFilePath);
            }
            Dispose();
        }
}
