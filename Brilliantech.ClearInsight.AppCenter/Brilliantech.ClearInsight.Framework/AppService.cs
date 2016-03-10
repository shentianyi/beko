using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Web;
using ScmWcfService.Model.Message;
using Brilliantech.ClearInsight.Framework.API;
using RestSharp;
using Brilliantech.ClearInsight.Framework.Config;
using Brilliantech.Framwork.Utils.JsonUtil;
using Brilliantech.ClearInsight.Framework.Model;

namespace Brilliantech.ClearInsight.Framework
{
    public class AppService
    {
        public ResponseMessage<object> PostPlcData(List<string> codes, List<string> values)
        {
            var msg = new ResponseMessage<object>();
            try
            {
                var client = new ApiClient();
                var req = client.GenRequest(ApiConfig.PlcPostAction, Method.POST);
                req.AddParameter("codes", string.Join(",", codes.ToArray()));
                req.AddParameter("values", string.Join(",", values.ToArray()));

                var res = client.Execute(req);
                msg = JsonUtil.parse<ResponseMessage<object>>(res.Content);
            }
            catch (WebFaultException<string> e)
            {
                msg.http_error = true;
                msg.meta.error_message = e.Detail;
            }
            catch (Exception e)
            {
                msg.http_error = true;
                msg.meta.error_message = "系统服务错误，请联系管理员";
            }

            return msg;
        }


        public ResponseMessage<List<ProductionPlan>> GetPlans(string product_line, string date)
        {
            var msg = new ResponseMessage<List<ProductionPlan>>();
            try
            {
                var client = new ApiClient();
                var req = client.GenRequest(ApiConfig.PlanAction, Method.GET);

                req.AddParameter("product_line", product_line);
                req.AddParameter("date", date);
               
                var res = client.Execute(req);
                msg = JsonUtil.parse<ResponseMessage<List<ProductionPlan>>>(res.Content);
            }
            catch (WebFaultException<string> e)
            {
                msg.http_error = true;
                msg.meta.error_message = e.Detail;
            }
            catch (Exception e)
            {
                msg.http_error = true;
                msg.meta.error_message = "系统服务错误，请联系管理员";
            }

            return msg;
        }
    }
}
