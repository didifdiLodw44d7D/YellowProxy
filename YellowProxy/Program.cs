using YellowProxy;

Dictionary<string, string> config = new Dictionary<string, string>();

void SplitParameters(string ParameterName, string line)
{
    var split = line.Split(':');
    if(split.Length == 2)
    {
        config.Add(ParameterName + "-IP", split[0]);
        config.Add(ParameterName + "-Port", split[1]);
    }
}

using(var sr = new StreamReader("Config.txt"))
{
    string RewriteURL = string.Empty;
    string ListenIP = string.Empty;
    string ListenURL = string.Empty;

    var line = sr.ReadLine();
    while(line != null)
    {
        if("ListenIP" == line.Split("=")[0])
        {
            ListenIP = line.Split("=")[1];
            ListenIP = ListenIP.Replace("\"", "");
            SplitParameters("ListenIP", ListenIP);
        }
        else if("RewriteURL" == line.Split("=")[0])
        {
            RewriteURL = line.Split("=")[1];
            RewriteURL = RewriteURL.Replace("\"", "");
            SplitParameters("RewriteURL", RewriteURL);
        }
        else if("ListenURL" == line.Split("=")[0])
        {
            ListenURL = line.Split("=")[1];
            ListenURL = ListenURL.Replace("\"", "");
            SplitParameters("ListenURL", ListenURL);
        }

        line = sr.ReadLine();
    }
}

var nowSize = 0;

while (true)
{
    var srv = new HTTPServer(config, nowSize);
    nowSize = srv.nowSize;
}