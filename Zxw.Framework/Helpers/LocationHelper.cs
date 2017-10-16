using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Zxw.Framework.Helpers
{
    public class LocationHelper
    {
        public static string GetUserLocation(string x, string y, out string errorMsg)
        {
            errorMsg = string.Empty;
            try
            {
                var getCityUrl = string.Format("http://apis.map.qq.com/ws/geocoder/v1/?location={0},{1}&key=EGKBZ-XAUKF-PX6JN-JOJLB-5WOKT-QUBHI&get_poi=0", x, y);
                var response = HttpRequestHelper.HttpGet(getCityUrl);
                var geo = (JObject)JsonConvert.DeserializeObject(response);
                if (geo != null && geo["status"].ToString() == "0" && geo["result"] != null)
                {
                    var resultJson = geo["result"].ToString();
                    var result = (JObject)JsonConvert.DeserializeObject(resultJson);
                    if (result != null && result["address_component"] != null)
                    {
                        var addressComponent = result["address_component"].ToString();
                        var xx = (JObject)JsonConvert.DeserializeObject(addressComponent);

                        if (xx != null && xx["city"] != null)
                        {
                            var city = xx["city"].ToString();
                            return city;
                        }
                    }

                }
            }
            catch (Exception e)
            {
                errorMsg = e.Message;
            }
            return string.Empty;
        }
    }
}
