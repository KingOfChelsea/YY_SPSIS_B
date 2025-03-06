using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Feishu
{
    /// <summary>
    /// 飞书返回的 Token 响应对象
    /// </summary>
    public class FeishuTokenResponse
    {
        public int code { get; set; }
        public int expire { get; set; }
        public string msg { get; set; }
        public string tenant_access_token { get; set; }
    }
}