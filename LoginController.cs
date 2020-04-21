using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Wechat.Api.Extensions;
using Wechat.Api.Helper;
using Wechat.Api.Request.Common;
using Wechat.Api.Request.Login;
using Wechat.Api.Response.Login;
using Wechat.Protocol;
using static MMPro.MM;

namespace Wechat.Api.Controllers
{
    /// <summary>
    /// 登陆
    /// </summary> 
    public class LoginController : ApiController
    {
        private WechatHelper _wechat = null;

        /// <summary>
        /// 构造
        /// </summary>
        public LoginController()
        {
            _wechat = new WechatHelper();
        }
    
        /// <summary>
        /// 获取登陆二维码
        /// </summary>
        /// <returns></returns> 
        [HttpPost()]
        [Route("api/Login/GetQrCode")]
        public Task<HttpResponseMessage> GetQrCode(GetQrCode getQrCode)
        {
            ResponseBase<QrCodeResponse> response = new ResponseBase<QrCodeResponse>();

            var result = _wechat.GetLoginQRcode(0, getQrCode?.ProxyIp, getQrCode?.ProxyUserName, getQrCode?.ProxyPassword);
            if (result != null && result.baseResponse.ret == MMPro.MM.RetConst.MM_OK)
            {
                QrCodeResponse qrCodeResponse = new QrCodeResponse();
                qrCodeResponse.QrBase64 = $"data:image/jpg;base64,{Convert.ToBase64String(result.qRCode.src)}";
                qrCodeResponse.Uuid = result.uuid;
                qrCodeResponse.ExpiredTime = DateTime.Now.AddSeconds(result.expiredTime);
                response.Data = qrCodeResponse;
            }
            else
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "获取二维码失败";
            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 检查是否登陆
        /// </summary>
        /// <param name="uuid">UUid</param>
        /// <returns></returns> 
        [HttpPost]
        [Route("api/Login/CheckLogin/{Uuid}")]
        public Task<HttpResponseMessage> CheckLogin(string uuid)
        {
            ResponseBase<CheckLoginResponse> response = new ResponseBase<CheckLoginResponse>();

            var result = _wechat.CheckLoginQRCode(uuid);
            CheckLoginResponse checkLoginResponse = new CheckLoginResponse();
            checkLoginResponse.State = result.State;
            checkLoginResponse.Uuid = result.Uuid;
            checkLoginResponse.WxId = result.WxId;
            checkLoginResponse.NickName = result.NickName;
            checkLoginResponse.Device = result.Device;
            checkLoginResponse.HeadUrl = result.HeadUrl;
            checkLoginResponse.Mobile = result.BindMobile;
            checkLoginResponse.Email = result.BindEmail;
            checkLoginResponse.Alias = result.Alias;
            checkLoginResponse.Data62 = result.Remark;

            response.Data = checkLoginResponse;

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// Data62登陆
        /// </summary>
        /// <param name="data62Login"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/Data62Login")]
        public Task<HttpResponseMessage> Data62Login(Data62Login data62Login)
        {
            ResponseBase<ManualAuthResponse> response = new ResponseBase<ManualAuthResponse>();

            var result = _wechat.UserLogin(data62Login.UserName, data62Login.Password, data62Login.Data62, data62Login.ProxyIp, data62Login.ProxyUserName, data62Login.ProxyPassword);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// Data62登陆破
        /// </summary>
        /// <param name="data62Login2"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/Data62Login2")]
        public Task<HttpResponseMessage> Data62Login2(Data62Login data62Login2)

        {
            ResponseBase<ManualAuthResponse> response = new ResponseBase<ManualAuthResponse>();

            var result = _wechat.ManualAuth2(data62Login2.Password, data62Login2.UserName, data62Login2.Data62);

            response.Data = result;
            return response.ToHttpResponseAsync();
        }



        /// <summary>
        /// 二次登陆
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/TwiceLogin/{wxId}")]
        public Task<HttpResponseMessage> TwiceLogin(string wxId)
        {

            ResponseBase<ManualAuthResponse> response = new ResponseBase<ManualAuthResponse>();
            var result = _wechat.TwiceLogin(wxId);
            if (result == null || result.baseResponse.ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.baseResponse.errMsg.@string ?? "登陆失败";
            }
            else
            {
                response.Data = result;
                response.Message = "登陆成功";
            }

            return response.ToHttpResponseAsync();
        }





        /// <summary>
        /// 退出登录
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/LogOut/{wxId}")]
        public Task<HttpResponseMessage> LogOut(string wxId)
        {
            ResponseBase<InitResponse> response = new ResponseBase<InitResponse>();

            var result = _wechat.logOut(wxId);
            if (result == null || result.BaseResponse.Ret != (int)MMPro.MM.RetConst.MM_OK)
            {
                response.Success = false;
                response.Code = "501";
                response.Message = result.BaseResponse.ErrMsg.String ?? "退出失败";
            }
            else
            {
                response.Message = "退出成功";
            }

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 初始化好友信息
        /// </summary>
        /// <param name="initMsg"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/InitUser")]
        public Task<HttpResponseMessage> InitUser(InitUser initMsg)
        {
            ResponseBase<InitUserResponse> response = new ResponseBase<InitUserResponse>();
            var result = _wechat.InitUser(initMsg.WxId, initMsg.SyncKey, initMsg.Buffer);

            InitUserResponse initMsgResponse = new InitUserResponse()
            {
                InitResponse = result.Item1,
                Buffer = result.Item2,
                SyncKey = (int)result.Item3
            };

            response.Data = initMsgResponse;

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 初始化用户信息
        /// </summary>
        /// <param name="wxId">微信Id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/NewInit/{wxId}")]
        public Task<HttpResponseMessage> NewInit(string wxId)
        {
            ResponseBase<InitResponse> response = new ResponseBase<InitResponse>();
            var result = _wechat.Init(wxId);
            response.Data = result;

            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取新的朋友的列表
        /// </summary>
        /// <param name="wxId">微信id</param>
        /// <param name="type">微信id</param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/GetMFriend/{wxId}/{type?}")]
        public Task<HttpResponseMessage> GetMFriend(string wxId, int type = 0)
        {
            ResponseBase<micromsg.GetMFriendResponse> response = new ResponseBase<micromsg.GetMFriendResponse>();
            var result = _wechat.GetMFriend(wxId, type);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取62数据
        /// </summary>
        /// <param name="wxId"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/Get62Data/{wxId}")]
        public Task<HttpResponseMessage> Get62Data(string wxId)
        {
            ResponseBase<string> response = new ResponseBase<string>();
            var result = _wechat.Get62Data(wxId);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 获取登陆Url
        /// </summary>
        /// <param name="getLoginUrl"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/GetLoginUrl")]
        public Task<HttpResponseMessage> GetLoginUrl(GetLoginUrl getLoginUrl)
        {
            ResponseBase<micromsg.GetLoginURLResponse> response = new ResponseBase<micromsg.GetLoginURLResponse>();
            var result = _wechat.GetLoginURL(getLoginUrl.WxId, getLoginUrl.Uuid);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }


        /// <summary>
        /// PC设备登陆扫码
        /// </summary>
        /// <param name="extDeviceLoginConfirmOK"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/ExtDeviceLoginConfirmGet")]
        public Task<HttpResponseMessage> ExtDeviceLoginConfirmGet(ExtDeviceLoginConfirmOK extDeviceLoginConfirmOK)
        {
            ResponseBase<micromsg.ExtDeviceLoginConfirmGetResponse> response = new ResponseBase<micromsg.ExtDeviceLoginConfirmGetResponse>();
            var result = _wechat.ExtDeviceLoginConfirmGet(extDeviceLoginConfirmOK.WxId, extDeviceLoginConfirmOK.LoginUrl);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }


        /// <summary>
        /// PC设备登陆确认
        /// </summary>
        /// <param name="extDeviceLoginConfirmOK"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/ExtDeviceLoginConfirmOK")]
        public Task<HttpResponseMessage> ExtDeviceLoginConfirmOK(ExtDeviceLoginConfirmOK extDeviceLoginConfirmOK)
        {
            ResponseBase<micromsg.ExtDeviceLoginConfirmOKResponse> response = new ResponseBase<micromsg.ExtDeviceLoginConfirmOKResponse>();
            var result = _wechat.ExtDeviceLoginConfirmOK(extDeviceLoginConfirmOK.WxId, extDeviceLoginConfirmOK.LoginUrl);
            response.Data = result;
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 辅助登录新手机设备
        /// </summary>
        /// <param name="phoneLogin"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/PhoneDeviceLogin")]
        public Task<HttpResponseMessage> PhoneDeviceLogin(PhoneLogin phoneLogin)
        {
            ResponseBase response = new ResponseBase();
            var result = _wechat.GetA8Key(phoneLogin.WxId,"", phoneLogin.Url);
            Util.Log.Logger.GetLog(this.GetType()).Info("################PhoneDeviceLogin_FullURL：" + result.fullURL + "################");
            if (result.fullURL.Contains("https://login.weixin.qq.com"))
            {
                SeleniumHelper seleniumHelper = new SeleniumHelper(Browsers.Chrome);
                try
                {
                    seleniumHelper.GoToUrl(result.fullURL);
                    seleniumHelper.ClickElement(seleniumHelper.FindElementByXPath("/html/body/form/div[3]/p/button"));
                    response.Message = "辅助成功，请在手机再次登录";
                }
                catch (Exception e)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = "登录失败,二维码已过期-" + e.Message;
                }
                seleniumHelper.Cleanup();
            }
            else
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "登录失败";

            }
            return response.ToHttpResponseAsync();
        }

        /// <summary>
        /// 辅助登录其他应用(https://open.weixin.qq.com/)
        /// </summary>
        /// <param name="phoneLogin"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("api/Login/OtherDeviceLogin")]
        public Task<HttpResponseMessage> OtherDeviceLogin(PhoneLogin phoneLogin)
        {
            ResponseBase response = new ResponseBase();
            var result = _wechat.GetA8Key(phoneLogin.WxId, "", phoneLogin.Url);
            Util.Log.Logger.GetLog(this.GetType()).Info("################OtherDeviceLogin_FullURL：" + result.fullURL + "################");
            if (result.fullURL.Contains("https://open.weixin.qq.com/"))
            {
                SeleniumHelper seleniumHelper = new SeleniumHelper(Browsers.Chrome);
                try
                {
                    seleniumHelper.GoToUrl(result.fullURL);
                    seleniumHelper.ClickElement(seleniumHelper.FindElementByXPath(@"//*[@id=""js_allow""]"));
                    response.Message = "登录成功";
                }
                catch(Exception e)
                {
                    response.Success = false;
                    response.Code = "501";
                    response.Message = "登录失败,二维码已过期-"+ e.Message;
                }
                seleniumHelper.Cleanup();
            }
            else
            {
                response.Success = false;
                response.Code = "501";
                response.Message = "登录失败";

            }
            return response.ToHttpResponseAsync();
        }

    }
}
