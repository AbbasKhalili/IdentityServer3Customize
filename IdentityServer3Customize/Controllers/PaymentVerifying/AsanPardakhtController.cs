using System;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Mvc;
using BusinessLayer.Finances;
using OnlinePayment;
using OnlinePayment.Contract;

namespace JahanGostar.Controllers.PaymentVerifying
{
    [System.Web.Http.Authorize]
    public class AsanPardakhtController : RootController
    {
        private readonly AndroidReturnUrl _androidReturnUrl;

        public AsanPardakhtController(IIpgConfigoration ipgConfigoration)
        {
            _androidReturnUrl = new AndroidReturnUrl();
        }

        [HttpPost]
        public async Task<ActionResult> Index(string applicant)
        {
            var verify = new VerifyResult { Description = "لطفا دوباره تلاش کنید. از بانک مقصد جوابی دریافت نشد." };
            try
            {
                //چون مقدار برگشتی از صفحه پرداخت یک رشته کد شده است مجبور شدم مشخصات پرداخت اینترنتی را برای وریفیکشن در وب کانفیگ بذارم
                var returningParams = Request.Form["returningParams"];

                var config = new PaymentParameters
                {
                    IpgId = 1007,
                    MerchantId = WebConfigurationManager.AppSettings["AP_merchantId"],
                    Username = WebConfigurationManager.AppSettings["AP_username"],
                    Password = WebConfigurationManager.AppSettings["AP_password"],
                    Key = WebConfigurationManager.AppSettings["AP_key"],
                    Iv = WebConfigurationManager.AppSettings["AP_iv"],
                    ReturningParams = returningParams
                };

                var payment = PaymentFactory.GetIpgPayment(config);
                payment.Refund(verify);
                var state = 2;
                if ((int.Parse(verify.Status) == 500) || (int.Parse(verify.Status) == 600))
                    state = 1;

                try
                {
                    var done = await new BIncreaseCredit().FinalTransaction((int)verify.OrderId, (int)verify.Amount, verify.RefrenceNumber,
                        verify.Status, state, verify.Description + " شماره کارت " + verify.CustomerCardNumber + " رسید بانک " + verify.BankReciptNumber
                                   + " کد پیگیری " + verify.RefrenceNumber);
                    verify.CurrentCredit = done.CurrentCredit;
                    verify.PreCredit = done.PreCredit;
                }
                catch (Exception)
                {
                    verify.Description += " حتما از قسمت پیگیری تراکنش ها اقدام به تایید نهایی فرمایید.";
                    throw;
                }
                return _androidReturnUrl.GetUrl(verify);

            }
            catch (Exception)
            {
                verify.Description = "عدم توانایی برقراری ارتباط با بانک جهت دریافت تاییدیه نهایی. تراکنش نا موفق بود.";
            }
            return _androidReturnUrl.GetUrl(verify);
        }
    }
}