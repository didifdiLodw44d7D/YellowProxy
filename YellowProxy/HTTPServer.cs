using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YellowProxy
{
    internal class HTTPServer
    {
        public int nowSize;

        private int total;
        Dictionary<string, string> config = new Dictionary<string, string>();
        public HTTPServer(Dictionary<string, string> config, int BridgeNowSize)
        {
            this.config = config;
            this.total = BridgeNowSize;
            
            //ListenするIPアドレス
            string ipString = this.config["ListenIP-IP"];
            System.Net.IPAddress ipAdd = System.Net.IPAddress.Parse(ipString);

            //Listenするポート番号
            int port = int.Parse(this.config["ListenIP-Port"]);

            //TcpListenerオブジェクトを作成する
            System.Net.Sockets.TcpListener listener =
                new System.Net.Sockets.TcpListener(ipAdd, port);

            //Listenを開始する
            listener.Start();

            Console.WriteLine("Listenを開始しました({0}:{1})。",
                ((System.Net.IPEndPoint)listener.LocalEndpoint).Address,
                ((System.Net.IPEndPoint)listener.LocalEndpoint).Port);

            //接続要求があったら受け入れる
            System.Net.Sockets.TcpClient client = listener.AcceptTcpClient();
            Console.WriteLine("クライアント({0}:{1})と接続しました。",
                ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Address,
                ((System.Net.IPEndPoint)client.Client.RemoteEndPoint).Port);

            //NetworkStreamを取得
            System.Net.Sockets.NetworkStream ns = client.GetStream();

            //クライアントから送られたデータを受信する
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            bool disconnected = false;
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] resBytes = new byte[2000000];
            int resSize = 0;
            do
            {
                //データの一部を受信する
                resSize = ns.Read(resBytes, 0, resBytes.Length);
                //Readが0を返した時はクライアントが切断したと判断
                if (resSize == 0)
                {
                    disconnected = true;
                    Console.WriteLine("クライアントが切断しました。");
                    break;
                }
                //受信したデータを蓄積する
                ms.Write(resBytes, 0, resSize);
                //まだ読み取れるデータがあるか、データの最後が\nでない時は、
                // 受信を続ける
            } while (ns.DataAvailable);
            //受信したデータを文字列に変換
            string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);

            int msLength = (int)ms.Length;

            ms.Close();

            var httpclient = new HTTPClient(this.config, resMsg);

	    string sendMsg = string.Empty;
	    byte[] sendBytes = new byte[httpclient.contents.Length+2];

            if (!disconnected)
            {
                //クライアントにデータを送信する
                //クライアントに送信する文字列を作成
		
                sendMsg = Encoding.UTF8.GetString(httpclient.contents);

                if("text/html" == httpclient.ContentType)
                {
                    sendBytes = httpclient.contents;

                    Array.Copy(Encoding.UTF8.GetBytes("\r\n"), 0, sendBytes, sendBytes.Length - 2, 2);

                    sendMsg = enc.GetString(sendBytes);
                }
                else if(nowSize < 0)
                {
                    sendBytes = httpclient.contents;

                    Array.Copy(Encoding.UTF8.GetBytes("\r\n"), 0, sendBytes, sendBytes.Length - 2, 2);
                }
                else
                {
                    sendBytes = new byte[httpclient.contents.Length];
                    sendBytes = httpclient.contents;
                }

                ns.Write(sendBytes, 0, sendBytes.Length);

                try{
                this.total = int.Parse(httpclient.ContentLength);
                
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                this.total -= resSize;

                GetNowSize(this.total);
            }

            //閉じる
            ns.Close();
            client.Close();
            Console.WriteLine("クライアントとの接続を閉じました。");

            //リスナを閉じる
            listener.Stop();
            Console.WriteLine("Listenerを閉じました。");
        }

        private void GetNowSize(int nowSize)
        {
            this.nowSize = nowSize;
        }
    }
}