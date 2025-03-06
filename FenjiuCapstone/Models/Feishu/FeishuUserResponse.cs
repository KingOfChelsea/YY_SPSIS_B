using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FenjiuCapstone.Models.Feishu
{
    /// <summary>
    /// 飞书API响应模型 
    /// 注意：字段结构需与实际API响应保持一致，建议定期对照官方文档更新 
    /// 文档地址：https://open.feishu.cn/document/server-docs/contact-v3/user/batch_get_id  
    /// </summary>
    public class FeishuUserResponse
    {
        /// <summary>
        /// 业务状态码（0表示成功）
        /// </summary>
        [JsonProperty("code")]
        public int Code { get; set; }

        /// <summary>
        /// 响应数据主体 
        /// </summary>
        [JsonProperty("data")]
        public UserData Data { get; set; }

        /// <summary>
        /// 用户数据容器 
        /// </summary>
        public class UserData
        {
            /// <summary>
            /// 用户列表（可能包含多个匹配结果）
            /// </summary>
            [JsonProperty("user_list")]
            public System.Collections.Generic.List<UserInfo> UserList { get; set; }
        }

        /// <summary>
        /// 用户详细信息 
        /// </summary>
        public class UserInfo
        {
            /// <summary>
            /// 用户唯一标识（open_id/user_id/union_id）
            /// </summary>
            [JsonProperty("user_id")]
            public string UserId { get; set; }
        }
    }
}