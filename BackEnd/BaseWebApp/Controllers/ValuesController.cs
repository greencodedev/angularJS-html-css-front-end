using BaseWebApp.Maven.PrintNode;
using BaseWebApp.Maven.Utils.Email;
using BaseWebApp.Maven.Utils.Sql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace BaseWebApp.Controllers
{
    //[Authorize]
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            List<string> res = new List<string>();

            string query = @"select * 
                            from Products p
	                            join Categories c on (c.CategoryId = p.CategoryId)
                            where 1 = 1 ";

            SqlQuery sqlQuery = new SqlQuery();

            using (Sql sql = new Sql())
            using (SqlReader reader = sqlQuery.ExecuteReader(sql, query))
            {
                while (reader.HasNext())
                {
                    res.Add(reader.GetInt("TestId").ToString());
                    res.Add(reader.GetString("Name"));
                    res.Add(reader.GetString("Value"));
                    res.Add(reader.GetTime("CreatedTime").ToString());
                }
            }

            return res;
        }

        // GET api/values/5
        public string Get(int id)
        {
            EmailClient.SendEmail("ari@mavensoftwaresolutions.com", "EMB SIGN UP",
                            "Please set up your new EMB account password by clicking here: <a href=\""  + "\">link</a>");
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
