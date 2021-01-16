using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using BusinessLayer;
using BusinessLayer.Finances;
using JG.Application;
using OnlinePayment;
using OnlinePayment.Contract;

namespace JahanGostar.Controllers.PaymentVerifying
{
    //[System.Web.Http.Authorize]
    public class SamanKishSw1Controller : Controller
    {
        private readonly IIpgConfigoration _ipgConfigoration;
        private readonly AndroidReturnUrl _androidReturnUrl;
        private readonly IFaildPaymentLog _faildPaymentLog;

        public SamanKishSw1Controller(IIpgConfigoration ipgConfigoration, IFaildPaymentLog faildPaymentLog)
        {
            _androidReturnUrl = new AndroidReturnUrl(); ;
            _ipgConfigoration = ipgConfigoration;
            _faildPaymentLog = faildPaymentLog;
        }

        [HttpPost]
        public async Task<ActionResult> Index(string applicant)
        {
            var orderid = int.Parse(Request.Form["ResNum"]);
            var mid = Request.Form["MID"];
            var state1 = Request.Form["State"];
            var stateCode = Request.Form["StateCode"];
            var refNum = Request.Form["RefNum"];
            var cardNo = Request.Form["SecurePan"];
            var rrn = Request.Form["RRN"];
            //var amount = Request.Form["Amount"] != null ? int.Parse(Request.Form["Amount"]) : 0;
            var traceNo = Request.Form["TraceNo"];

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
                             RefrenceNumber = refNum,
                             CustomerCardNumber = cardNo,
                             BankReciptNumber = traceNo
                         };
            

            try
            {
                var bPayment = new BPayment();
                var payModel = await bPayment.GetPayment(orderid);
                if (state1.Trim().ToLower() != "ok")
                {
                    payModel.ReferenceNumber = refNum;
                    payModel.ResultCode = state1;
                    payModel.Status = 2;
                    payModel.UpdateDateTime = DateTime.Now;
                    payModel.Description = "";

                    paymentLog.ReferenceNumber = payModel.ReferenceNumber;
                    paymentLog.ResultCode = payModel.ResultCode;
                    paymentLog.Status = payModel.Status;
                    paymentLog.UpdateDateTime = payModel.UpdateDateTime;
                    paymentLog.Description = payModel.Description;

                    await bPayment.UpdatePayment(payModel);
                }

                if (state1.Trim().ToLower() == "ok" && payModel.Status == 0)
                {
                    var request = new PaymentRequest() { Amount = localinfo.Amount, InvoiceNumber = orderid, Applicant = applicant };

                    var refundparams = await _ipgConfigoration.GetConfig(request, "");
                    refundparams.Token = "";
                    refundparams.ReferenceNumber = refNum;
                    
                    var payment = PaymentFactory.GetIpgPayment(refundparams);
                    payment.Refund(verify);
                    verify.Description = payment.GetDescription(refundparams.IpgId, verify.Status.ToInt());

                    if (verify.Status.ToInt() != 0)
                        return _androidReturnUrl.GetUrl(verify);

                    var state = 2;
                    if (verify.Status.ToInt() == 0)
                        state = 1;

                    var description = verify.Description + " شماره کارت " + cardNo + " رسید بانک " +
                                      verify.BankReciptNumber + " کد پیگیری " + verify.RefrenceNumber +
                                      "با شماره مرجع تراکنش " + rrn + " کد پیگیری سامان کیش " + traceNo;
                    
                    paymentLog.ReferenceNumber = verify.RefrenceNumber;
                    paymentLog.Status = verify.Status.ToInt();
                    paymentLog.ResultCode = state.ToString();
                    paymentLog.Description = description;
                    paymentLog.CardNumber = cardNo;
                    paymentLog.AddationalData = rrn;
                    try
                    {
                        var done = await new BIncreaseCredit().FinalTransaction(orderid, (int)verify.Amount, verify.RefrenceNumber,
                            verify.Status, state, description, rrn, cardNo);
                        verify.CurrentCredit = done.CurrentCredit;
                        verify.PreCredit = done.PreCredit;
                        verify.Reserves = done.Reserves;
                    }
                    catch (Exception ex)
                    {
                        paymentLog.ExceptionMessage = ex.Message;
                        verify.Description = " حتما از قسمت پیگیری تراکنش ها اقدام به تایید نهایی فرمایید.";
                        throw;
                    }
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