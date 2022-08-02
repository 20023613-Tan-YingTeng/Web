using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;

namespace xShapes.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            ViewData["title"] = "Home";
            // To check if user is log in, if logged in, pass name of user 
            if (User?.Identity?.IsAuthenticated == true)
            {
                ViewData["name"] = User.FindFirst(ClaimTypes.Name)!.Value;
            }
            return View();
        }

        [HttpPost]
        public IActionResult SendEmail(IFormCollection EmailForm)
        {
            string myEmail = "20023613@myrp.edu.sg";
            string custname = EmailForm["Name"].ToString().Trim();
            string email = EmailForm["Email"].ToString().Trim();
            string subject = "[Do Not Reply] xShapes Client Request Form";
            string message = EmailForm["Description"].ToString().Trim();
            string template = @"Hi,
                                <p>There is an interested client.</p>
                                <p>Name: {0}</p>
                                <p>Email: {1}</p>
                               <p>Description: {2}</b></p>";
            string body = String.Format(template, custname, email, message);
            string result;

            try
            {
                var smtp = new SmtpClient
                {
                    Host = "smtp.office365.com",
                    Port = 587,
                    EnableSsl = true,
                    Timeout = 100000,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(myEmail, "T0329722a!!!")
                };

                using (var msg = new MailMessage(myEmail, myEmail)
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                })
                {
                    smtp.Send(msg);
                }
                TempData["Message"] = "Email Successfully Sent";
                TempData["MsgType"] = "success";
            }
            catch (Exception e)
            {
                result = e.Message;
                TempData["Message"] = result;
                TempData["MsgType"] = "warning";
            }
            return RedirectToAction("Index");
        }
    }
}
