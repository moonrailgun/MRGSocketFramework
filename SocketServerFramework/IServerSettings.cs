namespace SocketServerFramework
{
    /// <summary>
    /// 服务器接口主要入口
    /// </summary>
    interface IServerSettings
    {
        bool isLog{get;set;}//是否启动记录
        bool isEnableTcp { get; set; }//是否打开TCP服务
        bool isEnableUdp { get; set; }//是否打开UDP服务

        /// <summary>
        /// 服务初始化
        /// </summary>
        void ServerInit();

        /// <summary>
        /// 运行服务器
        /// </summary>
        /// <returns>运行成功返回true</returns>
        bool ServerRun();

        /// <summary>
        /// 关闭服务器
        /// </summary>
        /// <returns>成功返回true</returns>
        bool ServerStop();

        /// <summary>
        /// 当接收到信息时
        /// </summary>
        /// <param name="protocolType">协议类型</param>
        void OnReceiveMsg(ProtocolType protocolType);

        /// <summary>
        /// 当发送消息时
        /// </summary>
        void OnSendMsg();
    }

    enum ProtocolType
    {
        TCP,UDP
    }
}