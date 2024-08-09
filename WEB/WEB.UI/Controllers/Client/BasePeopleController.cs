using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WEB.UI.Common;

namespace WEB.UI.Controllers
{
    public class BasePeopleController : Controller
    {
        
        //public static string GROUP_ID_TELEGRAM = ReadFile.LoadConfig().GROUP_ID_TELEGRAM;
        //public static string BOT_TOKEN_TELEGRAM = ReadFile.LoadConfig().BOT_TOKEN_TELEGRAM;
        
        private static int token_expires_day = 20;
        #region VALIDATION
        /// <summary>
        /// Hàm này sẽ kiểm tra khi user vào bổ sung thông tin. Cho những trường nào cần bắt nhập
        /// </summary>
        /// <param name="value_field_extend_client"></param>
        /// <param name="key_id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult CheckUpdateEmptyField(string value_field_extend_client, int key_id)
        {
            try
            {
                bool isValid = true;
                if (key_id > 0)
                {
                    if (value_field_extend_client.Length <= 0)
                    {
                        isValid = false;
                    }
                }
                return Json(isValid);
            }
            catch (Exception)
            {
                return Json(false);
            }
        }
        #endregion

        /// <summary>
        ///Token Provider Thuật toán mã hóa chuẩn Json Webtoken
        /// </summary>
        /// <returns></returns>
        public string genTokenJwt(IEnumerable<Claim> claims )
        {          

            var secretBytes = Encoding.UTF8.GetBytes(Constants.Secret);
            var key = new SymmetricSecurityKey(secretBytes);
            var algorithm = SecurityAlgorithms.HmacSha256;

            var signingCredentials = new SigningCredentials(key, algorithm);

            var token = new JwtSecurityToken(
                issuer: ReadFile.LoadConfig().API_GEN_TOKEN,
                audience: ReadFile.LoadConfig().API_GEN_TOKEN,
                claims: claims,
                notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                expires: DateTime.UtcNow.AddDays(token_expires_day),                
                //Using HS256 Algorithm to encrypt Token - JRozario
                signingCredentials: signingCredentials);

            var tokenJson = new JwtSecurityTokenHandler().WriteToken(token);           
            
            return tokenJson;
        }


        
        

    }
}