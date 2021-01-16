using System.Threading.Tasks;
using System.Web.Mvc;

namespace IdentityServer3Customize.Controllers
{
    public class RecoverPasswordController : Controller
    {
        private readonly UserService _service;
        
        public RecoverPasswordController()
        {
            _service = new UserService();
        }

        public ActionResult Index()
        {
            var model = new RecoverPassModel();
            return View("Index", model);
        }

        [HttpPost]
        public async Task<ActionResult> FoundUser(RecoverPassModel model)
        {
            model.NationalCode = model.NationalCode.Sanitizing();
            model.Username = model.Username.Sanitizing();

            var exists = await _studentService.GetForgetPassword(model.NationalCode, model.Username);

            if (exists.Email.IsNullOrEmpty() && exists.Mobile.IsNullOrEmpty())
            {
                model.Message = "اطلاعات بازیابی رمز عبور در سیستم ثبت نشده است. لطفا به مدیر سیستم مراجعه کنید.";
                model.MessageColor = "callout-danger";
            }
            else
            {
                if (!exists.Mobile.IsNullOrEmpty())
                {
                    model.Mobile = exists.Mobile;
                    model.IsActiveSms = true;
                }
                model.Email = exists.Email;
                model.Step = 1;
            }

            return View("Index", model);
        }

        [HttpPost]
        public async Task<ActionResult> RecoverCode(RecoverPassModel model)
        {
            model.FullEmail = model.FullEmail.Sanitizing();
            model.FullMobile = model.FullMobile.Sanitizing();
            model.Username = model.Username.Sanitizing();
            var result = "";
            if (model.SendEmail && model.FullEmail.Length < 5)
            {
                result = "ایمیل خود را به طور کامل وارد نمایید.";
            }
            else if (model.SendSms && model.FullMobile.Length != 11)
            {
                result = "شماره همراه خود را به طور کامل وارد نمایید.";
            }
            else
            {
                result = await _service.GetSendEmailOrSms(model.SendEmail ? model.FullEmail : "", model.SendSms ? model.FullMobile : "", model.NationalCode, model.Username, Request.UserHostAddress);
            }
            model.Message = result;
            model.MessageColor = "callout-danger";

            if (result != "")
            {
                model.FullEmail = "";
                model.FullMobile = "";
                model.Step = 1;
                return View("Index", model);
            }

            model.Message = "کد بازیابی رمز عبور برای شما ارسال شد.";
            model.MessageColor = "callout-success";
            model.Step = 2;
            return View("Index", model);
        }

        [HttpPost]
        public async Task<ActionResult> Verification(RecoverPassModel model)
        {
            model.Username = model.Username.Sanitizing();
            model.RecoverCode = model.RecoverCode.Sanitizing();
            var result = await _service.VerifyRecoveCode(model.Username, model.RecoverCode);
            model.Message = result;
            model.MessageColor = "callout-danger";

            if (result != "")
            {
                model.Step = 2;
                return View("Index", model);
            }

            model.Message = "کد بازیابی رمز عبور پذیرفته شد.";
            model.MessageColor = "callout-success";
            model.Step = 3;
            return View("Index", model);
        }

        [HttpPost]
        public async Task<ActionResult> SetNewPassword(RecoverPassModel model)
        {
            model.Username = model.Username.Sanitizing();
            model.Password = model.Password.Sanitizing();
            model.RePassword = model.RePassword.Sanitizing();
            var result = await _service.SetNewPassword(model.Username, model.Password, model.RePassword);
            model.Message = result;
            model.MessageColor = "callout-danger";

            if (result != "")
            {
                model.Step = 3;
                return View("Index", model);
            }

            model.Message = "کلمه عبور جدید با موفقیت ثبت شد.";
            model.MessageColor = "callout-success";
            model.Step = 4;
            return View("Index", model);
        }
    }

    public class RecoverPassModel
    {
        public int Step { get; set; }
        public string NationalCode { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Mobile { get; set; }
        public string FullMobile { get; set; }
        public string FullEmail { get; set; }
        public string Message { get; set; }
        public string MessageColor { get; set; }
        public string RecoverCode { get; set; }
        public string Password { get; set; }
        public string RePassword { get; set; }
        public bool IsActiveSms { get; set; }
        public bool SendEmail { get; set; }
        public bool SendSms { get; set; }
    }
}