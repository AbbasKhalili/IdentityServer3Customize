using OnlinePayment.Contract;
using System.Web.Mvc;

namespace JahanGostar.Controllers.PaymentVerifying
{
    public class AndroidReturnUrl : Controller
    {
        public ActionResult GetUrl(VerifyResult verify)
        {
            if (verify.Applicant != "android")
            {
                return PartialView("_VerificationPayment", verify);
            }
            var url = string.Format("jahangostarandroid://open?refrencenumber=" + verify.RefrenceNumber + 
                "&amount=" + verify.Amount + "&description=" + verify.Description);
            return Redirect(url);
        }
    }
}