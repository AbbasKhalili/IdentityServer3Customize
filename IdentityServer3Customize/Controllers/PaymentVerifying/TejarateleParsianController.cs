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
    public class TejarateleParsianController : Controller
    {
        private readonly IIpgConfigoration _ipgConfigoration;
        private readonly AndroidReturnUrl _androidReturnUrl;
        private readonly IFaildPaymentLog _faildPaymentLog;

        public TejarateleParsianController(IIpgConfigoration ipgConfigoration, IFaildPaymentLog faildPaymentLog)
        {
            _ipgConfigoration = ipgConfigoration;
            _faildPaymentLog = faildPaymentLog;
            _androidReturnUrl = new AndroidReturnUrl();
        }

        [HttpPost]
        public async Task<ActionResult> Index(string applicant)
        {
            var token = Request.Form["Token"];
            var status = int.Parse(Request.Form["status"]);
            var orderId = int.Parse(Request.Form["OrderId"]);
            var terminalNo = Request.Form["TerminalNo"];
            var prn = Request.Form["RRN"];
            var amount = int.Parse(Request.Form["Amount"]?.Replace(",", "") ?? "0");

            var localinfo = await new BIncreaseCredit().GetPayment(orderId);

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
                             OrderId = orderId
                         };
            try
            {
                var bPayment = new BPayment();
                var payModel = await bPayment.GetPayment(orderId);
                if (status != 0)
                {
                    payModel.ReferenceNumber = prn;
                    payModel.ResultCode = status.ToString();
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


                if (long.Parse(token) > 0 && status == 0 && !string.IsNullOrEmpty(prn) && payModel.Status == 0)
                {
                    var request = new PaymentRequest() { Token = token, Amount= amount, Applicant = applicant, InvoiceNumber = orderId };
                    var result = await _ipgConfigoration.GetConfig(request, "");
                    result.Token = token;

                    var payment = PaymentFactory.GetIpgPayment(result);
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
                        var done = await new BIncreaseCredit().FinalTransaction(result.OrderId, (int)verify.Amount, verify.RefrenceNumber,
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