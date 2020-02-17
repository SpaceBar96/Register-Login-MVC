using RegisterLogin.DAL;
using RegisterLogin.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace RegisterLogin.Controllers
{
    public class RegisterController : Controller
    {
        #region entity connection
        RegisterLoginEntities objCon = new RegisterLoginEntities();
        #endregion
        // GET: Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(User objUsr)
        {
            //email not verified on registration time
            objUsr.EmailVerification = false;

            //email exists or not
            var IsExists = IsEmailExists(objUsr.Email);
            if (IsExists)
            {
                ModelState.AddModelError("EmailExists", "Email Already Exists");
                return View();
            }

            //it generate unique code
            objUsr.ActivationCode = Guid.NewGuid();
            //password convert
            objUsr.Password = PasswordEncrypt.textToEncrypt(objUsr.Password);
            objCon.Users.Add(objUsr);
            objCon.SaveChanges();
            //ViewBag.Saved = "You have Registered Successfully";
            //return View("Registration");

            SendEmailToUser(objUsr.Email, objUsr.ActivationCode.ToString());
            var Message = "Registration Completed. Please check your Email : " + objUsr.Email;
            ViewBag.Message = Message;

            return View("Registration");

        }


        //to check Email exist in DB or not
        public bool IsEmailExists(string eMail)
        {
            var IsCheck = objCon.Users.Where(email => email.Email == eMail).FirstOrDefault();
            return IsCheck != null;
        }

        //to send email verification link to user after registered
        public void SendEmailToUser(string Email, string activationCode)
        {
            var GenerateUserVerificationLink = "/Register/UserVerification/" + activationCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, GenerateUserVerificationLink);
            var fromMail = new MailAddress(" "); // set your email  
            var fromEmailpassword = " "; // Set your password   
            var toEmail = new MailAddress(Email);

            var smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com";
            smtp.Port = 587;
            smtp.EnableSsl = true;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(fromMail.Address, fromEmailpassword);

            var Message = new MailMessage(fromMail, toEmail);
            Message.Subject = "Registration Completed-Demo";
            Message.Body = "<br/> Your registration completed succesfully." +
                           "<br/> please click on the below link for account verification" +
                           "<br/><br/><a href=" + link + ">" + link + "</a>";
            Message.IsBodyHtml = true;
            smtp.Send(Message);
        }

        public ActionResult UserVerification(string id)
        {
            bool Status = false;

            objCon.Configuration.ValidateOnSaveEnabled = false; //Ignore to password confirmation
            var IsVerify = objCon.Users.Where(u => u.ActivationCode == new Guid(id)).FirstOrDefault();

            if (IsVerify != null)
            {
                IsVerify.EmailVerification = true;
                objCon.SaveChanges();
                ViewBag.Message = "Email Verification Completed";
                Status = true;
            }
            else
            {
                ViewBag.Message = "Invalid Request---Email not verify!";
                ViewBag.Status = false;
            }
            return View();
        }

        //
    }
}