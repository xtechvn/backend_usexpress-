using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WEB.UI.ViewModels;

namespace WEB.UI.Cache
{
    public class MainCache
    {
      //  private readonly IConfiguration Configuration;
      //  // GET api/values
      ////  private static string sConnection = ConfigurationManager.ConnectionStrings["RedisHost"].ConnectionString.ToString(); // Tham so Connecting
      //  //private static string sConnectionInsert = ConfigurationManager.ConnectionStrings["RedisHostMaster"].ConnectionString.ToString(); // Tham so Connecting
      //  private static int iSecondExpired = 86400;  // Cache trong 1 ngay
      //  //private static string sPathLog = Path.GetFullPath("../../Log");
      //  //private static string sCacheChange_SelectAll = "CacheChange_SelectAll";


      //  // Xóa Cache
      //  public static bool DeleteKeyInCache(string sKey, CacheConnViewModel CacheConn, RedisConnection redisClient)
      //  {
      //      try
      //      {
      //        //  ErrorWriter.WriteLog(System.Web.HttpContext.Current.Server.MapPath("~"), "CacheConnModel CacheConn ===>" + CacheConn.sHost);

      //          redisClient.Keys.Remove(CacheConn.iDB, sKey).Wait();
      //          return true;
      //      }
      //      catch (Exception ex)
      //      {
      //         // ErrorWriter.WriteLog(HttpContext.Current.Server.MapPath("~"), "DeleteKeyInCache :(" + sKey + ") ===>" + ex.ToString());
      //          return false;
      //      }

      //  }

      //  /// <summary>
      //  /// Hệ thống sẽ trỏ vào hàm này để lấy dữ liệu
      //  // Lấy dữ liệu từ Cache
      //  // Dữ liệu trả ra dưới dạng 1 Datatable
      //  // Tên TOKLEN chính là sKey của Cache
      //  /// </summary>
      //  /// <param name="sKey"></param>
      //  /// <returns></returns>
      //  public static string GetDataCache(string sKey)
      //  {
      //      string msg = "";
      //      bool bResult = false;
      //      try
      //      {
      //          var objResult = new ResultModel();

      //          string jsonCache = string.Empty;


      //          // Lấy ra tên Store theo Tên Cache

      //          using (var redisClient = new RedisConnection(CacheConn().sHost, CacheConn().iPort))
      //          {
      //              try
      //              {

      //                  //IDatabase cache = Connection.GetDatabase();

      //                  // Mở Cache
      //                  redisClient.Open();
      //                  //var sStoreName = sKey;

      //                  //Kiểm tra Cache có tồn tại hay ko
      //                  if (redisClient.Keys.Exists(CacheConn().iDB, sKey).Result == false)
      //                  {
      //                      msg = "Không tìm thấy dữ liệu này trong cache";
      //                  }
      //                  else
      //                  {

      //                      // Đọc từ Cache
      //                      var res = redisClient.Strings.Get(CacheConn().iDB, sKey).Result;

      //                      // Convert từ kiểu Byte sang kiểu Datatable
      //                      var jsonResult = (string)CacheLIB.ByteArrayToObject(res);

      //                      if (jsonResult != string.Empty)
      //                      {
      //                          jsonCache = jsonResult;
      //                      }
      //                      //  Đóng Cache
      //                      redisClient.Close(true);

      //                      bResult = true;
      //                  }

      //              }
      //              catch (Exception ex)
      //              {

      //                  ErrorWriter.WriteLog(HttpContext.Current.Server.MapPath("~"), "Lỗi thực thi khi lấy dữ liệu từ Cache:(" + sKey + ") ===>" + ex.ToString());
      //                  redisClient.Close(true);
      //              }
      //              finally
      //              {
      //                  redisClient.Close(true);
      //              }
      //          }


      //          return jsonCache;
      //      }
      //      catch (Exception ex)
      //      {
      //          ErrorWriter.WriteLog(sPathLog, "GetDataCache:" + ex.ToString());
      //          return null;
      //      }
      //  }



      //  // Hàm này sẽ tự động lấy từ db ra nếu ko có từ cache. sau đó tự nạp lại
      //  //CacheName là token của api
      //  // tra ra json
      //  // Cache 1 Chuỗi JSON sau khi đã tính toán xong hết
      //  public static string LoadCacheByType(int iType, JArray objParr, string CacheName)
      //  {
      //      string jsonCache = string.Empty;

      //      try
      //      {
      //          switch (iType)
      //          {
      //              case CacheType.WEBSITE:
      //                  int _iAdsSale = Convert.ToInt32(objParr[0]["iAdsSale"].ToString().Replace("\"", "")); // Loại sản phẩm

      //                  var Website = new WebsiteController();
      //                  // lấy trong database
      //                  var objWeblistList = Website.GetAllWebsite(_iAdsSale);

      //                  // chuyển nó sang json

      //                  var objResult = new ResultModel
      //                  {
      //                      bResult = true,
      //                      Msg = "Success !!!",
      //                      obj = objWeblistList
      //                  };

      //                  jsonCache = JsonConvert.SerializeObject(objResult);

      //                  // nạp object này vào cache
      //                  InsertAllDataToCache(CacheName, jsonCache, CacheConn());

      //                  // đọc từ cache ra
      //                  jsonCache = mainCache.GetDataCache(CacheName); // tra ra json

      //                  if (jsonCache == string.Empty)
      //                  {
      //                      //  trường hợp đọc từ cache ra ko có thì lấy luôn từ db
      //                      objResult = new ResultModel
      //                      {
      //                          bResult = true,
      //                          Msg = "Dữ liệu lấy từ db thành công",
      //                          obj = objWeblistList
      //                      };
      //                      return JsonConvert.SerializeObject(objResult);
      //                  }
      //                  else
      //                  {
      //                      // trả dữ liệu từ cache
      //                      return jsonCache;

      //                  }
      //                  break;
      //          }
      //          return null;
      //      }
      //      catch (Exception ex)
      //      {
      //          ErrorWriter.WriteLog(HttpContext.Current.Server.MapPath("~"), "Lỗi LoadCacheByType:(" + iType + ") ===>" + ex.ToString());
      //          return string.Empty;
      //      }
      //  }



      //  // Cache 1 Object 
      //  public static bool SetObjectCache(Object obj, string cacheName)
      //  {
      //      try
      //      {
      //          string jsonCache = JsonConvert.SerializeObject(obj);

      //          //1. nạp object này vào cache
      //          InsertAllDataToCache(cacheName, jsonCache, CacheConnMaster());

      //          //2. đọc từ cache ra
      //          var objCache = mainCache.GetDataCache(cacheName); // tra ra json

      //          // 3. Kiểm tra cache có tồn tài không
      //          if (objCache == null)
      //          {
      //              ErrorWriter.WriteLog(System.Web.HttpContext.Current.Server.MapPath("~"), "Khong get duoc cahe co ten ===>" + cacheName);
      //              return false;
      //          }
      //          return true;

      //      }
      //      catch (Exception ex)
      //      {

      //          ErrorWriter.WriteLog(System.Web.HttpContext.Current.Server.MapPath("~"), "SetObjectCache" + ex.ToString());
      //          return false;
      //      }
      //  }



      //  // Nạp dữ liệu vào Cache 
      //  //objCache: đối tượng cần lưu vào cache
      //  public static bool InsertAllDataToCache(string sCacheKey, Object objCache, CacheConnModel CacheConn)
      //  {
      //      bool bSuccess = true;
      //      try
      //      {
      //          // Lấy dữ liệu gốc từ DB                
      //          // var objDtList = DBConnect.ExecuteQuery(sStoreName).Tables[0];

      //          // Convert danh sách các item trong datatable sang kieu byte
      //          byte[] bCacheList = Utils.ObjectToByteArray(objCache);

      //          // Nạp vào Cache
      //          using (var redisClient = new RedisConnection(CacheConn.sHost, CacheConn.iPort))
      //          {
      //              redisClient.Open();

      //              // Set Cache
      //              redisClient.Strings.Set(CacheConn.iDB, sCacheKey, bCacheList).Wait();

      //              // Kiểm tra thời gian tồn tại CACHE

      //              // Cập nhật thời gian lưu Cache
      //              redisClient.Keys.Expire(CacheConn.iDB, sCacheKey, iSecondExpired);


      //              redisClient.Close(true);
      //          }
      //      }
      //      catch (Exception ex)
      //      {
      //          bSuccess = false;
      //          ErrorWriter.WriteLog(sPathLog, "InsertDataToCache:" + ex.ToString());
      //      }
      //      return bSuccess;
      //  }



      //  //private static Constans Status = new Constans();
      //  //Mặc định sau 30s sẽ quét toàn bộ sự thay đổi mới trong DB , sau đó nạp Cache       
      //  public static CacheConnModel CacheConn()
      //  {
      //      return objCache(sConnection);

      //  }

      //  public static CacheConnModel CacheConnMaster()
      //  {
      //      return objCacheMaster(sConnectionInsert);

      //  }

      //  // Hàm chứa chuỗi kết nối Redis
      //  private static CacheConnModel objCache(string sConn)
      //  {
      //      try
      //      {


      //          string[] sCon = sConnection.Split(':');
      //          var objCache = new CacheConnModel()
      //          {
      //              sHost = sCon[0].ToString(),
      //              iPort = Convert.ToInt32(sCon[1]),
      //              iDB = Convert.ToInt32(sCon[2])
      //          };
      //          return objCache;
      //      }
      //      catch (Exception ex)
      //      {
      //         // ErrorWriter.WriteLog(System.Web.HttpContext.Current.Server.MapPath("~"), "sConn ===>" + sConn + ex.ToString());
      //          return null;
      //      }

      //  }
      //  private static CacheConnViewModel objCacheMaster(string sConn)
      //  {
      //      try
      //      {


      //          string[] sCon = sConn.Split(':');
      //          var objCache = new CacheConnModel()
      //          {
      //              sHost = sCon[0].ToString(),
      //              iPort = Convert.ToInt32(sCon[1]),
      //              iDB = Convert.ToInt32(sCon[2])
      //          };
      //          return objCache;
      //      }
      //      catch (Exception ex)
      //      {
      //          //ErrorWriter.WriteLog(System.Web.HttpContext.Current.Server.MapPath("~"), "sConn ===>" + sConn + ex.ToString());
      //          return null;
      //      }

      //  }
    }
}
