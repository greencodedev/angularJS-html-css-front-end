using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BaseWebApp.Maven.Utils.Api
{
    public class ProviderResponse
    {
        public object Data;
        public List<string> Messages = new List<string>();
        public int TotalCount;

        private bool _success = true;
        public bool Success
        {
            get
            {
                if (Messages.Any())
                {
                    return false;
                }

                return _success;
            }
            set
            {
                _success = value;
            }
        }
    }
}