using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class AndroidServer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartServer();
    }

    /// <summary>
    /// adb forward tcp:10086 tcp:10087
    /// 手机 tcp端口:10087
    /// </summary>
    int serverPort = 10087;

    void StartServer()
    {
        // 手机Server IP默认local
        IPAddress ip = IPAddress.Parse("127.0.0.1");

        Socket sock = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );
        sock.Bind(new IPEndPoint(ip, serverPort));
        sock.Listen(100);

        byte[] buffer = new byte[512]; // 字节缓冲区

        while (true) // 循环接收请求
        {
            // 为客户端建立服务连接
            Socket client = sock.Accept(); 
            // 接收客户端传入的数据
            int offset = client.Receive(buffer, buffer.Length, SocketFlags.None); 
            Debug.LogWarning(string.Format("服务器收到数据：{0}", Encoding.Default.GetString(buffer, 0, offset)));
            
            // 服务器回应客户端
            client.Send(Encoding.Default.GetBytes("收到数据，Over!"), SocketFlags.None);
        }
    }
}