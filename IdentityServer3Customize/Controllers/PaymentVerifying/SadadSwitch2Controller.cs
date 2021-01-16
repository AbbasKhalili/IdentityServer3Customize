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
    public class SadadSwitch2Controller : Controller
    {
        private readonly IIpgConfigoration _ipgConfigoration;
        private readonly AndroidReturnUrl _androidReturnUrl;
        private readonly IFaildPaymentLog _faildPaymentLog;

        public SadadSwitch2Controller(IIpgConfigoration ipgConfigoration, IFaildPaymentLog faildPaymentLog)
        {
            _ipgConfigoration = ipgConfigoration;
            _faildPaymentLog = faildPaymentLog;
            _androidReturnUrl = new AndroidReturnUrl();
        }

        [HttpPost]
        public async Task<ActionResult> Index(string applicant)
        {
            var orderid = int.Parse(Request.Form["OrderId"]);
            var token = Request.Form["Token"];
            var resCode = int.Parse(Request.Form["ResCode"]);

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
                             OrderId = orderid
                         };
            try
            {
                var bPayment = new BPayment();
                var payModel = await bPayment.GetPayment(orderid);
                if (resCode != 0)
                {
                    payModel.ResultCode = resCode.ToString();
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

                if (resCode == 0 && payModel.Status == 0)
                {
                    var request = new PaymentRequest { InvoiceNumber = orderid, Applicant = applicant };
                    var info = await _ipgConfigoration.GetConfig(request, "");
                    info.Token = token;

                    var payment = PaymentFactory.GetIpgPayment(info);
                    payment.Refund(verify);

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
                        var done = await new BIncreaseCredit().FinalTransaction(orderid, (int)verify.Amount, verify.RefrenceNumber,
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