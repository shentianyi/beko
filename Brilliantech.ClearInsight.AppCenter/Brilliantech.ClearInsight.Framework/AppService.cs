﻿using System;
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

        public ResponseMessage<object> PostPlcData(string kpiCode, List<string> codes, List<string> values, string time, string from_time = null, string to_time = null)
        {
            var msg = new ResponseMessage<object>() { http_error=false};
            try
            {
                var client = new ApiClient();
                var req = client.GenRequest(ApiConfig.PlcOnOffPostAction, Method.POST);
                req.AddParameter("kpi_code", kpiCode);
                req.AddParameter("codes", string.Join(",", codes.ToArray()));
                req.AddParameter("values", string.Join(",", values.ToArray()));
                req.AddParameter("time", time);
                if (from_time != null && to_time != null)
                {
                    req.AddParameter("from_time", from_time);
                    req.AddParameter("to_time", to_time);
                }
                //  client.ExecuteSync(req);
                var res = client.Execute(req);
                msg = JsonUtil.parse<ResponseMessage<object>>(res.Content);

                if (msg != null)
                {
                    msg.http_error = false;
                }
            }
            catch (WebFaultException<string> e)
            {
                if (msg != null)
                {
                    msg.http_error = true;
                    msg.meta.error_message = e.Detail;
                }
            }
            catch (Exception e)
            {
                if (msg != null)
                {
                    msg.http_error = true;
                    msg.meta.error_message = "系统服务错误，请联系管理员";
                }
            }

            return msg;
        }


        public ResponseMessage<object> SyncPostOnOffData(string kpiCode,string code, string value, string time,string from_time=null,string to_time=null)
        {
            var msg = new ResponseMessage<object>() { http_error = false };
            try
            {
                var client = new ApiClient();
                var req = client.GenRequest(ApiConfig.PlcOnOffPostAction, Method.POST);
                req.AddParameter("kpi_code", kpiCode);
                req.AddParameter("codes", code);
                req.AddParameter("values",  value);
                req.AddParameter("time", time);
                if (from_time != null && to_time != null) {
                    req.AddParameter("from_time", from_time);
                    req.AddParameter("to_time", to_time);
                }
                client.ExecuteSync(req);
                //var res = client.Execute(req);
                // msg = JsonUtil.parse<ResponseMessage<object>>(res.Content);

                if (msg != null)
                {
                    msg.http_error = false;
                }
            }
            catch (WebFaultException<string> e)
            {
                if (msg != null)
                {
                    msg.http_error = true;
                    msg.meta.error_message = e.Detail;
                }
            }
            catch (Exception e)
            {
                if (msg != null)
                {
                    msg.http_error = true;
                    msg.meta.error_message = "系统服务错误，请联系管理员";
                }
            }

            return msg;
        }

        public ResponseMessage<object> SyncPostPlcData(List<string> codes, List<string> values, string time)
        {
            var msg = new ResponseMessage<object>() { http_error = false };
            try
            {
                var client = new ApiClient();
                var req = client.GenRequest(ApiConfig.PlcOnOffPostAction, Method.POST);
                req.AddParameter("codes", string.Join(",",codes.ToArray()));
                req.AddParameter("values", string.Join(",", values.ToArray()));
                req.AddParameter("time", time);

                client.ExecuteSync(req);
                //var res = client.Execute(req);
                // msg = JsonUtil.parse<ResponseMessage<object>>(res.Content);

                if (msg != null)
                {
                    msg.http_error = false;
                }
            }
            catch (WebFaultException<string> e)
            {
                if (msg != null)
                {
                    msg.http_error = true;
                    msg.meta.error_message = e.Detail;
                }
            }
            catch (Exception e)
            {
                if (msg != null)
                {
                    msg.http_error = true;
                    msg.meta.error_message = "系统服务错误，请联系管理员";
                }
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

        public ResponseMessage<object> ConfirmPlans(List<int> ids)
        {
            var msg = new ResponseMessage<object>();
            try
            {
                var client = new ApiClient();
                var req = client.GenRequest(ApiConfig.ConfirmPlanAction, Method.POST);

                req.AddParameter("ids", string.Join(",", ids.ToArray()));

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
    }
}
