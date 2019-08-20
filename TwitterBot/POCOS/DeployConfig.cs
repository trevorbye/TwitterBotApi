using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TwitterBot.POCOS
{
    public class DeployConfig
    {
        // one of ["test", "prod"]
        private const string _env = "test";
        public DeployConfig() { }

        public string OauthCallbackUrl { get
            {
                if (_env == "test")
                {
                    return "http://localhost:52937/add-account-redirect";
                }
                else
                {
                    return "";
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
                    return "";
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
                    return "";
                }
            }
        }
    }
}