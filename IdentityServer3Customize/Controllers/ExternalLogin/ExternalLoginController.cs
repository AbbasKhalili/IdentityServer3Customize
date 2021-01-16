using DomainClass.Models.ExternalLogin;
using DomainClass.Services.Persons;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using System.Security.Claims;
using System.IdentityModel.Tokens;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.Web.Configuration;
using System.Net;
using Jeton.Interface.RestApi;
using JahanGostar.IdentityServer;
using System.Web.UI.WebControls;
using System.Web;
using Microsoft.Owin.Security;
using Microsoft.Ajax.Utilities;
using IdentityModel.Client;
using DomainClass.Models;
using System.Linq;
using System.Net.Http;
using IdentityServer3.Core.Models;
using Newtonsoft.Json.Linq;
using DomainClass.ViewModels.Students;
using Microsoft.AspNet.Identity;

namespace JahanGostar.ExternalLogin
{
    public class ExternalLoginController : BaseController
    {
        private readonly IStudentService _studentService;
        private static string SehatsanjiUrl = WebConfigurationManager.AppSettings["SehatsanjiUrl"];
        public ExternalLoginController(IStudentService studentservice)
        {
            _studentService = studentservice;
        }

        private const string secret = "u0LSERK0VhBmwTU3q52f9GaiByqni7l6VLRwjIIXFbFLn0GzM2tivzGZ134A8DdQzt27eIT3pamNJtGGS2A1Za5Q9gLDxoxhqYLUKEpEC5yY4B3glCv7eQG6wx9GF2EW";

        public async Task Post(string request)
        {
            //SHA256Managed hasher = new SHA256Managed();
            //byte[] pwdBytes = new UTF8Encoding().GetBytes(secret);
            //byte[] key = hasher.ComputeHash(pwdBytes);
            //hasher.Dispose();
            //request.Request = request.Request.Replace("_", "/");
            //byte[] coddedBytes = Convert.FromBase64String(request.Request);
            //string roundtrip = DecryptStringFromBytes_Aes(coddedBytes, key);
            //string jsonData = JsonConvert.DeserializeObject(roundtrip).ToString();
            //var ResultData = new ExternalLoginConfig(jsonData);

            /////////////////check time and Continuation
            //DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            //var registerDateTime = dateTime.AddSeconds(Convert.ToDouble(ResultData.Time)).ToLocalTime();

            //long ExpTimestamp = (long)(registerDateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
            //var expDatetime = dateTime.AddSeconds(Convert.ToDouble(ExpTimestamp)).ToLocalTime();

            //bool checkTime = expDatetime >= DateTime.Now ? true : false;

            //JetonUserService jetonservice = new JetonUserService();
            //ExternalAuthenticationModel extenalAuth = new ExternalAuthenticationModel();
            ////if (checkTime)        
            ////{
            //var existUser = await _studentService.GetByPersoneli(ResultData.Personeli);
            //if (existUser != null)
            //{
            //    var token = GenerateToken(ResultData.Session_id, ResultData.Time);
            //    if (token != null)
            //    {
            //        var encodedResult = JG.Application.CallAPIs.CallApiClass.CallApi(SehatsanjiUrl, token);
            //        Stream receiveStream = await encodedResult.Content.ReadAsStreamAsync();
            //        StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
            //        var result = readStream.ReadToEnd();
            //        var externalLoginResultData = DecryptJWT(result);

            if (true)
            {

                //   await UserManager.CreateAsync()
            }
        }

        //            }
        //        }
        //    }
        //    //}
        //}
        //public ExternalLoginResultData DecryptJWT(string stream)
        //{
        //    var handler = new JwtSecurityTokenHandler();
        //    var jsonToken = handler.ReadToken(stream);
        //    var token = handler.ReadToken(stream) as JwtSecurityToken;
        //    var resultData = new ExternalLoginResultData()
        //    {
        //        Status = Convert.ToBoolean(token.Payload["status"].ToString()),
        //    };
        //    return resultData;
        //}
        //public string GenerateToken(string Session_id, string time)
        //{
        //    var signingKey = new InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        //    var now = DateTime.UtcNow;
        //    var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);

        //    DateTime registerDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        //    registerDateTime = registerDateTime.AddSeconds(Convert.ToDouble(time)).ToLocalTime();

        //    long ExpTimestamp = (long)(registerDateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
        //    var expDatetime = registerDateTime.AddSeconds(Convert.ToDouble(ExpTimestamp)).ToLocalTime();

        //    var claimsIdentity = new ClaimsIdentity(new List<Claim>()
        //    {
        //        new Claim("session_id", Session_id),
        //        new Claim("exp",ExpTimestamp.ToString()),
        //        new Claim("secret_key",secret),
        //    }, "Custom");

        //    var securityTokenDescriptor = new SecurityTokenDescriptor()
        //    {
        //        Subject = claimsIdentity,
        //        SigningCredentials = signingCredentials,
        //        Lifetime = new Lifetime(registerDateTime, expDatetime),
        //    };

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
        //    var token = tokenHandler.WriteToken(plainToken);
        //    return token;
        //}
        //public string GenerateToken1(string Personneli, string time)
        //{
        //    var signingKey = new InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        //    var now = DateTime.UtcNow;
        //    var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest);

        //    DateTime registerDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        //    registerDateTime = registerDateTime.AddSeconds(Convert.ToDouble(time)).ToLocalTime();

        //    long ExpTimestamp = (long)(registerDateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalSeconds;
        //    var expDatetime = registerDateTime.AddSeconds(Convert.ToDouble(ExpTimestamp)).ToLocalTime();

        //    var claimsIdentity = new ClaimsIdentity(new List<Claim>()
        //    {
        //        new Claim("personeli", Personneli),
        //        new Claim("exp",ExpTimestamp.ToString()),
        //        new Claim("secret_key",secret),
        //    }, "Custom");

        //    var securityTokenDescriptor = new SecurityTokenDescriptor()
        //    {
        //        Subject = claimsIdentity,
        //        SigningCredentials = signingCredentials,
        //        Lifetime = new Lifetime(registerDateTime, expDatetime),
        //    };

        //    var tokenHandler = new JwtSecurityTokenHandler();
        //    var plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
        //    var token = tokenHandler.WriteToken(plainToken);
        //    return token;
        //}
        //static string DecryptStringFromBytes_Aes(byte[] cipherTextCombined, byte[] Key)
        //{
        //    // Declare the string used to hold 
        //    // the decrypted text. 
        //    string plaintext = null;

        //    // Create an Aes object 
        //    // with the specified key and IV. 
        //    using (Aes aesAlg = Aes.Create())
        //    {
        //        aesAlg.Key = Key;

        //        byte[] IV = new byte[aesAlg.BlockSize / 8];
        //        byte[] hashPart = new byte[32];
        //        byte[] cipherText = new byte[cipherTextCombined.Length - IV.Length - hashPart.Length];

        //        Array.Copy(cipherTextCombined, IV, IV.Length);
        //        Array.Copy(cipherTextCombined, IV.Length, cipherText, 0, hashPart.Length);
        //        Array.Copy(cipherTextCombined, IV.Length + hashPart.Length, cipherText, 0, cipherText.Length);

        //        aesAlg.IV = IV;

        //        aesAlg.Mode = CipherMode.CBC;

        //        // Create a decrytor to perform the stream transform.
        //        ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        //        // Create the streams used for decryption. 
        //        using (var msDecrypt = new MemoryStream(cipherText))
        //        {
        //            using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
        //            {
        //                using (var srDecrypt = new StreamReader(csDecrypt))
        //                {
        //                    // Read the decrypted bytes from the decrypting stream
        //                    // and place them in a string.
        //                    plaintext = srDecrypt.ReadToEnd();
        //                }
        //            }
        //        }

        //    }
        //    return plaintext;
        //}
    }
}