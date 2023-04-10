using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YellowProxy
{
    internal class HTTPClient
    {
        Dictionary<string, string> config = new Dictionary<string, string>();
        public byte[] contents = new byte[0];
        public string httprequest = string.Empty;
        public string ContentType = string.Empty;

        public string ContentLength = string.Empty;
        public HTTPClient(Dictionary<string, string> config, string httprequest)
        {
            this.config = config;
            this.httprequest = httprequest;

            //サーバーのIPアドレス（または、ホスト名）とポート番号
            string ipOrHost = config["RewriteURL-IP"];
            //string ipOrHost = "localhost";
            int port = int.Parse(config["RewriteURL-Port"]);

	        string sendMsg = Rewrite(this.httprequest);

            //TcpClientを作成し、サーバーと接続する
            System.Net.Sockets.TcpClient tcp =
                new System.Net.Sockets.TcpClient(ipOrHost, port);
                
            //NetworkStreamを取得する
            System.Net.Sockets.NetworkStream ns = tcp.GetStream();

            //サーバーにデータを送信する
            //文字列をByte型配列に変換
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            byte[] sendBytes = enc.GetBytes(sendMsg);
            //データを送信する
            ns.Write(sendBytes, 0, sendBytes.Length);
            //Console.WriteLine(sendMsg);

            //サーバーから送られたデータを受信する
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            byte[] resBytes = new byte[2000000];
            int resSize = 0;
            
            int cn = 0;

            do
            {
                //データの一部を受信する
                resSize = ns.Read(resBytes, 0, resBytes.Length);
                //Readが0を返した時はサーバーが切断したと判断
                if (resSize == 0)
                {
                    Console.WriteLine("サーバーが切断しました。");
                    break;
                }
                //受信したデータを蓄積する
                ms.Write(resBytes, 0, resSize);
                //まだ読み取れるデータがあるか、データの最後が\nでない時は、
                // 受信を続ける

                cn += resSize;

                Console.WriteLine(cn);

            } while (ns.DataAvailable);
            //受信したデータを文字列に変換
            string resMsg = enc.GetString(ms.GetBuffer(), 0, (int)ms.Length);

            //末尾の\nを削除
            //Console.WriteLine(resMsg);

            //閉じる
            ns.Close();
            tcp.Close();
            Console.WriteLine("切断しました。");

            this.ContentLength = GetContentLength(resMsg);
            this.ContentType = GetContentType(resMsg);
            this.contents = ms.GetBuffer();
            ms.Close();
        }

        public string Rewrite(string httprequest)
        {
            string str = httprequest.Replace(config["ListenIP-IP"], config["RewriteURL-IP"]);

            return str;
        }

        private string GetContentType(string httpresponse)
        {
            var sentenseArray = httpresponse.Split("\r\n");

            foreach(var s in sentenseArray)
            {
                if(s.Contains("Content-Type:"))
                {
                    var wordArray = s.Split(" ");

                    return wordArray[1].Replace(";", "");
                }
            }

            return string.Empty;
        }

        private string GetContentLength(string httpresponse)
        {
            var sentenseArray = httpresponse.Split("\r\n");

            foreach(var s in sentenseArray)
            {
                if(s.Contains("Content-Length:"))
                {
                    var wordArray = s.Split(" ");

                    
                    Console.WriteLine("Hello = " + wordArray[1].Replace(";", ""));


                    return wordArray[1].Replace(";", "");
                }
            }

            return string.Empty;
        }
    }
}
