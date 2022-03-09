using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class PCClient : MonoBehaviour
{
	const int wait_ms = 100;
	const int timeOut = 5000;

	TcpClient tcpClient;
	BinaryReader reader;
	BinaryWriter writer;


    private void Start()
    {
		
		StartClient();
    }

	/// <summary>
	/// adb forward tcp:10086 tcp:10087
	/// PC tcp端口10086
	/// </summary>
	int clientPort = 10086;
	void StartClient()
    {
		IPAddress ip = IPAddress.Parse("127.0.0.1"); // 服务器地址

		Socket sock = new Socket( // 构建一个套接字服务
				AddressFamily.InterNetwork,
				SocketType.Stream,
				ProtocolType.Tcp
			);

		try
		{
			sock.Connect(new IPEndPoint(ip, clientPort)); // 连接服务器

			sock.Send(Encoding.Default.GetBytes("服务器，在吗？")); // 发送数据到服务器

			byte[] buffer = new byte[512]; // 接收缓冲区
			int offset = sock.Receive(buffer, buffer.Length, SocketFlags.None); // 接收服务器传入的数据
			Debug.LogWarning(string.Format("收到服务器消息：{0}", Encoding.Default.GetString(buffer, 0, offset)));

			sock.Close(); // 再见
		}
		catch (SocketException e)
		{
			Debug.LogError(e.Message); // 输出发生错误的信息
		}
	}

	bool DoConnect()
    {
		string strCmd = "adb shell am broadcast -a NotifyServiceStop";
		Execute(strCmd, wait_ms);
		Thread.Sleep(wait_ms);
		strCmd = "adb forward tcp:12580 tcp:10086";
		Execute(strCmd, wait_ms);
		Thread.Sleep(wait_ms);
		strCmd = "adb shell am broadcast -a NotifyServiceStart";
		Execute(strCmd, wait_ms);
		Thread.Sleep(wait_ms);

		IPAddress ipaddress = IPAddress.Parse("127.0.0.1");

		tcpClient.Connect(ipaddress, 12580);
		Thread.Sleep(wait_ms);
		if (tcpClient != null)
		{
			NetworkStream networkkStream = tcpClient.GetStream();
			networkkStream.ReadTimeout = timeOut;
			networkkStream.WriteTimeout = timeOut;
			reader = new BinaryReader(networkkStream);
			writer = new BinaryWriter(networkkStream);
			return true;
		}
		else
			return false;
	}

	private string Execute(string command, int seconds)
	{
		string output = ""; 
		if (command != null && !command.Equals(""))
		{
			Process process = new Process();
			ProcessStartInfo startInfo = new ProcessStartInfo();
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = "/C " + command;
			startInfo.UseShellExecute = false;
			startInfo.RedirectStandardInput = false;
			startInfo.RedirectStandardOutput = true; 
			startInfo.CreateNoWindow = true;
			process.StartInfo = startInfo;
			try
			{
				if (process.Start())
				{
					if (seconds == 0)
					{
						process.WaitForExit();
					}
					else
					{
						process.WaitForExit(seconds); 
					}
					output = process.StandardOutput.ReadToEnd();
				}
			}
			finally
			{
				if (process != null)
					process.Close();
			}
		}
		return output;
	}
}
