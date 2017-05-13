using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using System.Reflection;

namespace SqlServer
{
    public class Global : System.Web.HttpApplication
    {
        public override void Init()
        {
            base.Init();
            //foreach (string moduleName in this.Modules)
            //{
            //    string appName = "APPNAME";
            //    IHttpModule module = this.Modules[moduleName];
            //    SessionStateModule ssm = module as SessionStateModule;
            //    if (ssm != null)
            //    {
            //        FieldInfo storeInfo = typeof(SessionStateModule).GetField("_store", BindingFlags.Instance | BindingFlags.NonPublic);
            //        SessionStateStoreProviderBase store = (SessionStateStoreProviderBase)storeInfo.GetValue(ssm);
            //        if (store == null)//In IIS7 Integrated mode, module.Init() is called later
            //        {
            //            FieldInfo runtimeInfo = typeof(HttpRuntime).GetField("_theRuntime", BindingFlags.Static | BindingFlags.NonPublic);
            //            HttpRuntime theRuntime = (HttpRuntime)runtimeInfo.GetValue(null);
            //            FieldInfo appNameInfo = typeof(HttpRuntime).GetField("_appDomainAppId", BindingFlags.Instance | BindingFlags.NonPublic);
            //            appNameInfo.SetValue(theRuntime, appName);
            //        }
            //        else
            //        {
            //            Type storeType = store.GetType();
            //            if (storeType.Name.Equals("OutOfProcSessionStateStore"))
            //            {
            //                FieldInfo uribaseInfo = storeType.GetField("s_uribase", BindingFlags.Static | BindingFlags.NonPublic);
            //                uribaseInfo.SetValue(storeType, appName);
            //            }
            //        }
            //    }
            //}
        }

        protected void Application_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {
            //支持web集群共享session

            //第一步:设置cookie主域
            //Response.Cookies["ASP.NET_SessionId"].Value = Session.SessionID;
            //Response.Cookies["ASP.NET_SessionId"].Domain = "SqlServer_Session.com";

            //第二步:设置IIS网站ID一致(通过IIS配置)
            //重写Init();
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}