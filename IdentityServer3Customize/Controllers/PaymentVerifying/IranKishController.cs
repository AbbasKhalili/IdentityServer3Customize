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
    public class IranKishController : Controller
    {
        private readonly IIpgConfigoration _ipgConfigoration;
        private readonly AndroidReturnUrl _androidReturnUrl;
        private readonly IFaildPaymentLog _faildPaymentLog;

        public IranKishController(IIpgConfigoration ipgConfigoration, IFaildPaymentLog faildPaymentLog)
        {
            _ipgConfigoration = ipgConfigoration;
            _faildPaymentLog = faildPaymentLog;
            _androidReturnUrl = new AndroidReturnUrl();
        }

        [HttpPost]
        public async Task<ActionResult> Index(string applicant)
        {
            var orderid = int.Parse(Request.Form["InvoiceNumber"]);
            var token = Request.Form["Token"];
            var resultCode = int.Parse(Request.Form["ResultCode"]);
            var referenceId = Request.Form["ReferenceId"];
            var cardNo = Request.Form["cardNo"];
            var amount = long.Parse(Request.Form["amount"]);

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
                             Description = "لطفا دوباره تلاش کنید. از بانک مقصد جوابی دریافت نشد.",
                             Applicant = applicant,
                             Amount = localinfo?.Amount ?? 0,
                             OrderId = orderid
                         };

            try
            {
                var bPayment = new BPayment();
                var payModel = await bPayment.GetPayment(orderid);
                if (resultCode != 100)
                {
                    payModel.ReferenceNumber = referenceId;
                    payModel.ResultCode = resultCode.ToString();
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

                if (resultCode == 100 && payModel.Status == 0)
                {
                    var request = new PaymentRequest() {Amount = localinfo.Amount, InvoiceNumber = orderid, Applicant = applicant};

                    var refundparams = await _ipgConfigoration.GetConfig(request, "");
                    refundparams.Token = token;
                    refundparams.ReferenceNumber = referenceId;
                    
                    var payment = PaymentFactory.GetIpgPayment(refundparams);
                    verify.Description = payment.GetDescription(refundparams.IpgId, resultCode);
                    payment.Refund(verify);
                    
                    var state = 2;
                    if (long.Parse(verify.Status) == amount)
                        state = 1;

                    var description = verify.Description + " شماره کارت " + cardNo + " رسید بانک " +
                                      verify.BankReciptNumber + " کد پیگیری " + verify.RefrenceNumber;

                    paymentLog.ReferenceNumber = verify.RefrenceNumber;
                    paymentLog.Status = verify.Status.ToInt();
                    paymentLog.ResultCode = state.ToString();
                    paymentLog.Description = description;
                    paymentLog.CardNumber = cardNo;
                    paymentLog.AddationalData = verify.BankReciptNumber;
                    try
                    {
                        var done = await new BIncreaseCredit().FinalTransaction(orderid, (int)verify.Amount, verify.RefrenceNumber,
                            verify.Status, state, description, verify.BankReciptNumber, cardNo);
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