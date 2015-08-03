using MRGLogsSystem;
using SocketServerFramework.Models;
using SocketServerFramework.Utils;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace SocketServerFramework.Sockets
{
    /// <summary>
    /// TCP 服务
    /// </summary>
    class TcpServer
    {
        public static Encoding encoding = Encoding.UTF8;
        public int port = 23233;

        public TcpListener currentListener;//当前监听器
        public IAsyncResult currentListenAsyncResult;//当前的异步连接

        public List<TcpClient> ClientPool;//连接池

        #region 监听器
        /// <summary>
        /// 初始化TCP
        /// </summary>
        public void Init()
        {
            try
            {
                LogsSystem.Log("正在初始化TCP服务");
                if (currentListener == null)
                {
                    currentListener = new TcpListener(IPAddress.Parse("127.0.0.1"), port);
                    currentListener.Start();
                    this.currentListenAsyncResult = currentListener.BeginAcceptTcpClient(AcceptTcpClient, currentListener);//开始异步接受TCP连接
                    LogsSystem.Log("TCP连接创建完毕，监听端口：" + port);
                }
                else
                {
                    LogsSystem.Log("当前已有TCP服务在运行");
                }
            }
            catch (Exception ex)
            {
                LogsSystem.Log("TCP服务创建失败：" + ex.ToString(), LogLevel.ERROR);
            }

        }

        /// <summary>
        /// 异步接受tcp连接
        /// </summary>
        private void AcceptTcpClient(IAsyncResult ar)
        {
            try
            {
                LogsSystem.Log("有新的连接连入");
                TcpListener listener = (TcpListener)ar.AsyncState;

                TcpClient client = listener.EndAcceptTcpClient(ar);

                //添加到连接池
                this.ClientPool.Add(client);

                //开始异步接受数据
                Receive(client.Client);

                //继续下一轮接受
                listener.BeginAcceptTcpClient(AcceptTcpClient, listener);
            }
            catch (Exception ex)
            {
                LogsSystem.Log(ex.ToString(), LogLevel.ERROR);
            }

        }

        /// <summary>
        /// 关闭监听
        /// </summary>
        public void StopListen()
        {
            if (currentListener != null)
            {
                try
                {
                    this.currentListener.EndAcceptTcpClient(this.currentListenAsyncResult);
                    this.currentListener.Stop();
                    LogsSystem.Log("已关闭TCP服务");
                }
                catch (Exception ex)
                {
                    LogsSystem.Log("关闭TCP失败" + ex, LogLevel.ERROR);
                }
            }
        }
        #endregion

        #region 异步发送数据
        public void Send(Socket socket, DTO data)
        {
            string sendMessage = JsonCoding<DTO>.encode(data);
            byte[] sendBytes = encoding.GetBytes(sendMessage);
            Send(socket, sendBytes);
        }
        public void Send(Socket socket, byte[] data)
        {
            LogsSystem.Log(string.Format("TCP 发送数据({0}):{1}", data.Length, encoding.GetString(data)));
            if (socket.Connected && socket != null)
            {
                socket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), socket);
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState;

                int bytesSent = handler.EndSend(ar);
                LogsSystem.Log(string.Format("TCP 数据({0})发送完成", bytesSent));
            }
            catch (Exception e)
            {
                LogsSystem.Log(e.ToString(), LogLevel.ERROR);
            }
        }
        #endregion

        #region 异步接受数据
        public void Receive(Socket client)
        {
            try
            {
                if (client.Connected)
                {
                    StateObject state = new StateObject();
                    state.socket = client;
                    client.BeginReceive(state.buffer, 0, StateObject.buffSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
            }
            catch (Exception e)
            {
                LogsSystem.Log(e.ToString(), LogLevel.ERROR);
            }
        }
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject receiveState = (StateObject)ar.AsyncState;
                Socket client = receiveState.socket;

                int bytesRead = client.EndReceive(ar);
                if (bytesRead < StateObject.buffSize)
                {
                    //如果读取到数据长度较小
                    foreach (byte b in receiveState.buffer)
                    {
                        if (b != 0x00)
                        {
                            //将缓存加入结果列
                            receiveState.dataByte.Add(b);
                        }
                    }
                    receiveState.buffer = new byte[StateObject.buffSize];//清空缓存

                    //接受完成
                    byte[] receiveBytes = receiveState.dataByte.ToArray();

                    //处理数据

                    Receive(client);//继续下一轮的接受
                }
                else
                {
                    //如果读取到数据长度大于缓冲区
                    receiveState.dataByte.AddRange(receiveState.buffer);//将缓存加入结果列
                    receiveState.buffer = new byte[StateObject.buffSize];//清空缓存
                    client.BeginReceive(receiveState.buffer, 0, StateObject.buffSize, 0, new AsyncCallback(ReceiveCallback), receiveState);//继续接受下一份数据包
                }
            }
            catch (Exception e)
            {
                LogsSystem.Log(e.ToString(), LogLevel.ERROR);
            }
        }
        #endregion
    }

    class StateObject
    {
        //socket 客户端
        public Socket socket = null;
        //缓冲区大小
        public const int buffSize = 256;
        //缓冲
        public byte[] buffer = new byte[buffSize];
        //数据流
        public List<byte> dataByte = new List<byte>();
    }
}