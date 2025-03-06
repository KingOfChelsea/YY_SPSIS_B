using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;

namespace sales_managers.Common
{
    /// <summary>
    /// JSON格式化对象
    /// </summary>
    public class JsonResponseHelper
    {
        /// <summary>
        ///  返回JSON格式数据 create by zane 2024-12-12
        /// </summary>
        /// <param name="data">传入对象数据，转为JSON格式</param>
        /// <returns></returns>
        public static HttpResponseMessage CreateJsonResponse(object data)
        {
            string json = JsonConvert.SerializeObject(data);//转为Json 
            HttpResponseMessage response = new HttpResponseMessage();
            response.Content = new StringContent(json, Encoding.UTF8, "application/json");
            response.Headers.Add("Access-Control-Allow-Origin", "*"); //添加对象头，防止跨域
            return response; //返回数据
        }
    }
}