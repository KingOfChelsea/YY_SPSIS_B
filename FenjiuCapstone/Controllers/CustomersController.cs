using sales_managers.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FenjiuCapstone.Controllers
{
    public class CustomersController : ApiController
    {
        [HttpGet]
        [Route("api/customers/get-customers")]
        public HttpResponseMessage GetCustomers()
        {
            List<Dictionary<string, object>> customers = new List<Dictionary<string, object>>();
            string sql = "SELECT * FROM Customers";
            using (var reader = DbAccess.read(sql))
            {
                while (reader.Read())
                {
                    var customer = new Dictionary<string, Object>
                    {
                        ["CustomerID"] = reader["CustomerID"],
                        ["CustomerName"] = reader["CustomerName"],
                        ["ContactNumber"] = reader["ContactNumber"],
                        ["Email"] = reader["Email"],
                        ["Address"] = reader["Address"],
                        ["CreatedAt"] = reader["CreatedAt"]
                    };
                    customers.Add(customer);
                };
            }
            return JsonResponseHelper.CreateJsonResponse(new { success = false,data=customers});
        }


    }
}
