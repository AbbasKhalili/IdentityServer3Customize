using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using BusinessLayer.Finances;
using JG.Application;
using OnlinePayment;
using OnlinePayment.Contract;

namespace JahanGostar.Controllers.PaymentVerifying
{
    //[System.Web.Http.Authorize]
    public class SadadSwitch1Controller : Controller
    {
        private readonly IIpgConfigoration _ipgConfigoration;
        private readonly AndroidReturnUrl _androidReturnUrl;
        private readonly IFaildPaymentLog _faildPaymentLog;

        public SadadSwitch1Controller(IIpgConfigoration ipgConfigoration, IFaildPaymentLog faildPaymentLog)
        {
            _ipgConfigoration = ipgConfigoration;
            _faildPaymentLog = faildPaymentLog;
            _androidReturnUrl = new AndroidReturnUrl();
        }

        [HttpPost]
        public async Task<ActionResult> Index(string applicant)
        {
            var orderid = int.Parse(Request.Form["OrderId"]);

            var localinfo = await new BIncreaseCredit().GetPayment(orderid);

            var paymentLog = new FaildPaymentModel()
            {
                Date = localinfo.Date,
                PaymentId = localinfo.ID,
                Amount = localinfo.Amount,
                CardNumber = localinfo.CardNumber,
                Description = localinfo.Description,
                EIPGBankId = localinfo.EIPGBankID,
                ReferenceNumber = localinfo.ReferenceNumber,
                RegDateTime = localinfo.RegDateTime,
                ResultCode = localinfo.ResultCode,
                Status = localinfo.Status,
                ResultMessage = localinfo.ResultMessage,
                StudentId = localinfo.StudentIDNum,
                Time = localinfo.Time,
                Token = localinfo.Token,
                UpdateDateTime = localinfo.UpdateDateTime,
                AddationalData = localinfo.AddationalData,
                VerifyStatus = localinfo.VerifyStatus
            };

            //if (localinfo.StudentIDNum != CurrentUser.StudentId)
            //    return RedirectToAction("Index", "Home");

            if (localinfo.ResultCode != "")
                return RedirectToAction("Index", "Home");


            var verify = new VerifyResult
                         {
                             Description = "تراکنش ناموفق.",
                             Applicant = applicant,
                             Amount = localinfo?.Amount ?? 0,
                             OrderId = orderid,
                         };
            
            try
            {


                var request = new PaymentRequest() { InvoiceNumber = orderid, Applicant = applicant };
                var result = await _ipgConfigoration.GetConfig(request, "");

                var pay = await new BIncreaseCredit().GetPayment(orderid);
                result.Amount = pay.Amount;
                result.RequestKey = pay.Token;
                
                var payment = PaymentFactory.GetIpgPayment(result);
                payment.Refund(verify);


                if (int.Parse(verify.Status) != 0)
                {
                    return _androidReturnUrl.GetUrl(verify);
                }

                var state = 2;
                if (int.Parse(verify.Status) == 0)
                    state = 1;

                var description = verify.Description + " شماره کارت " + verify.CustomerCardNumber + " رسید بانک " +
                                  verify.BankReciptNumber + " کد پیگیری " + verify.RefrenceNumber;

                paymentLog.ReferenceNumber = verify.RefrenceNumber;
                paymentLog.Status = verify.Status.ToInt();
                paymentLog.ResultCode = state.ToString();
                paymentLog.Description = description;
                paymentLog.AddationalData = verify.BankReciptNumber;
                try
                {
                    var done = await new BIncreaseCredit().FinalTransaction(orderid, pay.Amount, verify.RefrenceNumber,
                        verify.Status, state, description, verify.BankReciptNumber);
                    verify.CurrentCredit = done.CurrentCredit;
                    verify.PreCredit = done.PreCredit;
                    verify.Reserves = done.Reserves;
                }
                catch (Exception ex)
                {
                    paymentLog.ExceptionMessage = ex.Message;
                    verify.Description += " حتما از قسمت پیگیری تراکنش ها اقدام به تایید نهایی فرمایید.";
                    throw;
                }
            }
            catch (Exception ex)
            {
                await _faildPaymentLog.DoLog(paymentLog, ex);
                return _androidReturnUrl.GetUrl(verify);
            }

            return _androidReturnUrl.GetUrl(verify);

        }
    }
}