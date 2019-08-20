
namespace TwitterBot.POCOS
{
    public class DeployConfig
    {
        // one of ["test", "prod"]
        private const string _env = "prod";
        public DeployConfig() { }

        public string OauthCallbackUrl { get
            {
                if (_env == "test")
                {
                    return "http://localhost:52937/add-account-redirect";
                }
                else
                {
                    return "https://mstwitterbot.azurewebsites.net/add-account-redirect";
                }
            }
        }

        public string OauthConsumerKey { get
            {
                if (_env == "test")
                {
                    return "OmlAUrh2VxAKX6Qp2bYzwxBwI";
                }
                else
                {
                    return "Z56vS7LzR2KlEN44FCGDz6g3g";
                }
            }
        }

        public string SigningKey { get
            {
                if (_env == "test")
                {
                    return "4HmFBAX1w6xuz4onP0lHwq7ANMQ6vswEokIiNnJY6kHuUd51ek";
                }
                else
                {
                    return "7WXse93IFMia1Lj3VvGh83L1wCFfJqQM4Cj9mkkZClqHNSfs5M";
                }
            }
        }
    }
}