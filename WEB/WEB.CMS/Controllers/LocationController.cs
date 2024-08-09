using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Caching.RedisWorker;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Repositories.IRepositories;
using Utilities;
using Utilities.Contants;
using WEB.CMS.Customize;
using WEB.CMS.Models;
using WEB.CMS.Models.Location;

namespace CMS.Controllers
{
    [CustomAuthorize]
    public class LocationController : Controller
    {
        private readonly IProvinceRepository _provinceRepository;
        private readonly IDistrictRepository _districtRepository;
        private readonly IWardRepository _wardRepository;
        private readonly RedisConn _RedisService;
        private readonly IConfiguration _Configuration;

        public LocationController(IConfiguration configuration, IProvinceRepository provinceRepository, IDistrictRepository districtRepository, IWardRepository wardRepository, RedisConn redisService)
        {
            _provinceRepository = provinceRepository;
            _districtRepository = districtRepository;
            _wardRepository = wardRepository;
            _RedisService = redisService;
            _Configuration = configuration;

        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        public async Task<IActionResult> LoadProvince()
        {
            int stt_code = -1;
            string msg = "Error On Excution";
            List<Province> data = null;
            try
            {
                data = await _provinceRepository.GetProvincesList();
                stt_code = 1;
                msg = "Success";
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("LoadProvince - LocationController: " + ex.ToString());

            }
            return Ok(new
            {
                stt_code = stt_code,
                msg = msg,
                data =  data
            });
        }
        [HttpGet]
        public async Task<IActionResult> LoadDistrict()
        {
            int stt_code = -1;
            string msg = "Error On Excution";
            List<District> data = null;
            try
            {
                data = await _districtRepository.GetDistrictList();
                stt_code = 1;
                msg = "Success";
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("LoadDistrict - LocationController: " + ex.ToString());

            }
            return Ok(new
            {
                stt_code = stt_code,
                msg = msg,
                data = data
            });
        }
        [HttpGet]
        public async Task<IActionResult> LoadWard()
        {
            int stt_code = -1;
            string msg = "Error On Excution";
            List<Ward> data = null;
            try
            {
                data = await _wardRepository.GetWardList();
                stt_code = 1;
                msg = "Success";
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("LoadWard - LocationController: " + ex.ToString());

            }
            return Ok(new
            {
                stt_code = stt_code,
                msg = msg,
                data = data
            });
        }
        //Thêm mới Location
        [HttpPost]
        public async Task<IActionResult> Add(string location_type, string location_data_json)
        {
            string msg = "";
            int stt_code = 0;
            dynamic data = null;
            try
            {
                if (location_type == null || location_type == "" || location_type.Length < 1 || location_data_json == null || location_data_json == "")
                {
                    stt_code = -1;
                    msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại";
                    return new JsonResult(new
                    {
                        stt_code = stt_code,
                        msg = msg
                    });
                }
                switch (location_type)
                {
                    case "p":
                        {
                            Province p = JsonConvert.DeserializeObject<Province>(location_data_json);
                            if (p.Name == "" || p.Name == null || p.Type == null || p.Type == "")
                            {
                                stt_code = -1;
                                msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại";
                            }
                            else
                            {
                                var exists = _provinceRepository.CheckProvinceExists(p);
                                switch (exists)
                                {
                                    case 0:
                                        {
                                            var lastest_province = await _provinceRepository.GetLastestProvinceWithIDAsync();
                                            p.ProvinceId = Convert.ToInt32(lastest_province.ProvinceId) > 0 ? (Convert.ToInt32(lastest_province.ProvinceId) + 1).ToString() : p.ProvinceId;
                                            string result = await _provinceRepository.AddNewProvince(p);
                                            if (result != null)
                                            {
                                                p.Id = Convert.ToInt32(result);
                                                msg = "Thêm mới " + p.Type + " " + p.Name + " thành công.";
                                                stt_code = 1;
                                                var push_data_to_old = (OkObjectResult)SyncData(JsonConvert.SerializeObject(p), 0).Result;
                                                msg += ".\nSync: " + (push_data_to_old.Value.ToString());
                                                data = p;
                                            }
                                            else
                                            {
                                                msg = "Thêm mới " + p.Type + " " + p.Name + " thất bại.";
                                                stt_code = -1;
                                            }
                                        }
                                        break;
                                    case 1:
                                        {

                                            stt_code = -1;
                                            msg = p.Type + " " + p.Name + " đã tồn tại, không thể thêm mới.";
                                        }
                                        break;
                                    default:
                                        {
                                            stt_code = -1;
                                            msg = "Có lỗi trong quá trình xử lý, vui lòng liên hệ với bộ phận IT.";
                                        }
                                        break;
                                }

                            }
                        }
                        break;
                    case "d":
                        {
                            District d = JsonConvert.DeserializeObject<District>(location_data_json);

                            if (d.Name == "" || d.Name == null || d.Type == null || d.Type == "" || d.ProvinceId == null || d.ProvinceId == "")
                            {
                                stt_code = -1;
                                msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại";
                            }
                            else
                            {
                                var exists = _districtRepository.CheckDistrictExists(d);
                                switch (exists)
                                {
                                    case 0:
                                        {
                                            var lastest_district = await _districtRepository.GetLastestDistrictWithIDAsync();
                                            d.DistrictId = Convert.ToInt32(lastest_district.DistrictId) > 0 ? (Convert.ToInt32(lastest_district.DistrictId) + 1).ToString() : d.DistrictId;
                                            string result = await _districtRepository.AddNewDistrict(d);
                                            if (result != null)
                                            {
                                                d.Id = Convert.ToInt32(result);
                                                msg = "Thêm mới " + d.Type + " " + d.Name + " thành công.";
                                                stt_code = 1;
                                                var push_data_to_old = (OkObjectResult)SyncData(JsonConvert.SerializeObject(d), 1).Result;
                                                msg += ".\nSync: " + (push_data_to_old.Value.ToString());
                                                data = d;
                                            }
                                            else
                                            {
                                                msg = "Thêm mới " + d.Type + " " + d.Name + " thất bại.";
                                                stt_code = -1;

                                            }
                                        }
                                        break;
                                    case 1:
                                        {

                                            stt_code = -1;
                                            msg = d.Type + " " + d.Name + " đã tồn tại, không thể thêm mới.";
                                        }
                                        break;
                                    default:
                                        {
                                            stt_code = -1;
                                            msg = "Có lỗi trong quá trình xử lý, vui lòng liên hệ với bộ phận IT.";
                                        }
                                        break;
                                }

                            }

                        }
                        break;
                    case "w":
                        {
                            Ward w = JsonConvert.DeserializeObject<Ward>(location_data_json);
                            if (w.Name == "" || w.Name == null || w.Type == null || w.Type == "" || w.DistrictId == null || w.DistrictId == "")
                            {
                                stt_code = -1;
                                msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại";
                            }
                            else
                            {
                                var exists = _wardRepository.CheckWardExists(w);
                                switch (exists)
                                {
                                    case 0:
                                        {
                                            var lastest_ward = await _wardRepository.GetLastestWardWithIDAsync();
                                            w.DistrictId = Convert.ToInt32(lastest_ward.WardId) > 0 ? (Convert.ToInt32(lastest_ward.WardId) + 1).ToString() : w.DistrictId;
                                            string result = await _wardRepository.AddNewWard(w);
                                            if (result != null)
                                            {
                                                w.Id = Convert.ToInt32(result);
                                                msg = "Thêm mới " + w.Type + " " + w.Name + " thành công.";
                                                stt_code = 1;
                                                var push_data_to_old = (OkObjectResult)SyncData(JsonConvert.SerializeObject(w), 2).Result;
                                                msg += ".\nSync: " + (push_data_to_old.Value.ToString());
                                                data = w;
                                            }
                                            else
                                            {
                                                msg = "Thêm mới " + w.Type + " " + w.Name + " thất bại.";
                                                stt_code = -1;

                                            }
                                        }
                                        break;
                                    case 1:
                                        {

                                            stt_code = -1;
                                            msg = w.Type + " " + w.Name + " đã tồn tại, không thể thêm mới.";
                                        }
                                        break;
                                    default:
                                        {
                                            stt_code = -1;
                                            msg = "Có lỗi trong quá trình xử lý, vui lòng liên hệ với bộ phận IT.";
                                        }
                                        break;
                                }

                            }

                        }
                        break;
                    default: break;
                }
                return Ok(new
                {
                    stt_code = stt_code,
                    msg = msg,
                    data = data
                });
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("AddNewLocation - LocationController with '"+ location_data_json + "' - location_type:" + location_type + " : \n" + ex.ToString());

                return  Ok(new
                {
                    stt_code = -2,
                    msg = "Error: On Excution"
                });

            }
        }
        //Update Location:
        [HttpPost]
        public async Task<IActionResult> Update(string location_type, string location_data_json)
        {

            string msg = "";
            int stt_code = 0;
            dynamic data = null;
            try
            {
                if (location_type == null || location_type == "" || location_type.Length < 1 || location_data_json == null || location_data_json == "")
                {
                    stt_code = -1;
                    msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại";
                    return Ok(new { stt_code = stt_code, msg = msg });
                }
                switch (location_type)
                {
                    case "p":
                        {
                            Province p = JsonConvert.DeserializeObject<Province>(location_data_json);
                            if (p.Name == "" || p.Name == null || p.Type == null || p.Type == "" || p.Id < 1)
                            {
                                stt_code = -1;
                                msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại";
                            }
                            else
                            {
                                var exists = _provinceRepository.CheckProvinceExists(p,true);
                                switch (exists)
                                {
                                    case 0:
                                        {
                                            string result = await _provinceRepository.UpdateProvince(p);
                                            if (result != null)
                                            {
                                                msg = "Province Updated: " + result;
                                                int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_common"]) > -1 ? Convert.ToInt32(_Configuration["Redis:Database:db_common"]) : 0;
                                                _RedisService.clear("PROVINCE", db_index);
                                                stt_code = 1;
                                                var push_data_to_old = (OkObjectResult)SyncData(location_data_json, 0).Result;
                                                string sync_rs = push_data_to_old.Value.ToString();
                                                msg += ".\nSync: " + (push_data_to_old.Value.ToString());
                                                data = p;
                                            }
                                            else
                                            {
                                                stt_code = -1;
                                                msg = "Cannot Update Province: " + p.Name;
                                            }

                                        } break;
                                    case 1:
                                        {
                                            stt_code = -1;
                                            msg = "Tỉnh / Thành phố với tên '" + p.Name + "' đã tồn tại, hoặc không có thay đổi nào trong thông tin cập nhật mới.";
                                        }
                                        break;
                                    default:
                                        {
                                            stt_code = -1;
                                            msg = "Có lỗi trong quá trình xử lý, vui lòng liên hệ với bộ phận IT.";
                                        }
                                        break;
                                }
                               
                            }
                        }
                        break;
                    case "d":
                        {
                            District d = JsonConvert.DeserializeObject<District>(location_data_json);
                            if (d.Id < 1 || d.Name == "" || d.Name == null || d.Type == null || d.Type == "" || d.ProvinceId == null || d.ProvinceId == "")
                            {
                                stt_code = -1;
                                msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại";
                            }
                            else
                            {
                                var exists = _districtRepository.CheckDistrictExists(d,true);
                                switch (exists)
                                {
                                    case 0:
                                        {
                                            string result = await _districtRepository.UpdateDistrict(d);
                                            if (result != null)
                                            {
                                                int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_common"]) > -1 ? Convert.ToInt32(_Configuration["Redis:Database:db_common"]) : 0;
                                                _RedisService.clear("DISTRICT_" + d.Id, db_index);
                                                stt_code = 1;
                                                msg = "District Updated: " + result;
                                                var push_data_to_old = (OkObjectResult)SyncData(location_data_json, 1).Result;
                                                msg += ".\nSync: " + (push_data_to_old.Value.ToString());
                                                data = d;

                                            }
                                            else
                                            {
                                                stt_code = -1;
                                                msg = "Cannot Update District: " + d.Name;
                                            }
                                        } break;
                                    case 1:
                                        {

                                            stt_code = -1;
                                            msg =d.Type+ " với tên '" + d.Name + "' đã tồn tại, hoặc không có thay đổi nào trong thông tin cập nhật mới.";
                                        }
                                        break;
                                    default:
                                        {
                                            stt_code = -1;
                                            msg = "Có lỗi trong quá trình xử lý, vui lòng liên hệ với bộ phận IT.";
                                        }
                                        break;
                                }
                                           
                            }

                        }
                        break;
                    case "w":
                        {
                            Ward w = JsonConvert.DeserializeObject<Ward>(location_data_json);
                            var exists = _wardRepository.CheckWardExists(w, true);
                            switch (exists)
                            {
                                case 0:
                                    {
                                        if (w.Id < 1 || w.Name == "" || w.Name == null || w.Type == null || w.Type == "" || w.DistrictId == null || w.DistrictId == "")
                                        {
                                            stt_code = -1;
                                            msg = "Dữ liệu gửi lên không chính xác, vui lòng kiểm tra lại";
                                        }
                                        else
                                        {
                                            string result = await _wardRepository.UpdateWard(w);
                                            if (result != null)
                                            {
                                                int db_index = Convert.ToInt32(_Configuration["Redis:Database:db_common"]) > -1 ? Convert.ToInt32(_Configuration["Redis:Database:db_common"]) : 0;
                                                _RedisService.clear("WARD_" + w.Id, db_index);
                                                stt_code = 1;
                                                msg = "Ward Updated: " + result;
                                                var push_data_to_old = (OkObjectResult)SyncData(location_data_json, 2).Result;
                                                string sync_rs = push_data_to_old.Value.ToString();
                                                msg += ".\nSync: " + (push_data_to_old.Value.ToString());
                                                data = w;
                                            }
                                            else
                                            {
                                                stt_code = -1;
                                                msg = "Cannot Update Ward: " + w.Name;
                                            }
                                        }
                                    }
                                    break;
                                case 1:
                                    {

                                        stt_code = -1;
                                        msg = w.Type+ " với tên '" + w.Name + "' đã tồn tại, hoặc không có thay đổi nào trong thông tin cập nhật mới.";
                                    }
                                    break;
                                default:
                                    {
                                        stt_code = -1;
                                        msg = "Có lỗi trong quá trình xử lý, vui lòng liên hệ với bộ phận IT.";
                                    }
                                    break;
                            }
                           

                        }
                        break;
                    default: break;
                }

                return Ok(new { stt_code = stt_code, msg = msg, data = data });
            }
            catch(Exception ex)
            {
                LogHelper.InsertLogTelegram("UpdateLocation - LocationController with '" + location_data_json + "' - location_type:" + location_type + " : \n" + ex.ToString());
                return Ok(new { stt_code = -2, msg = "Error On Excution" });
            }
        }
        public IActionResult AddLocation(string id)
        {
            string[] a = id.Split("-");
            if ((a.Length == 2) && (a[0] == "a"))
            {
                ViewBag.IsIDExsist = 1;
                switch (a[1])
                {
                    case "p":
                        {
                            ViewBag.LocationTypeList = new List<string> { "Tỉnh", "Thành phố" };
                            ViewBag.Label = "Thêm mới Tỉnh";

                        }
                        break;
                    case "d":
                        {
                            ViewBag.LocationTypeList = new List<string> { "Quận", "Huyện", "Thành phố" };
                            ViewBag.Label = "Thêm mới Quận";

                        }
                        break;
                    case "w":
                        {
                            ViewBag.LocationTypeList = new List<string> { "Phường", "Xã", "Đường" };
                            ViewBag.Label = "Thêm mới Phường";

                        }
                        break;
                    default: break;
                }
            }
            else
            {
                ViewBag.IsIDExsist = 0;
                ViewBag.LocationTypeList = new List<string> { "Tỉnh", "Thành phố", "Quận", "Huyện", "Phường", "Xã", "Đường" };
            }
            return View();

        }
        public IActionResult EditLocation(string location_id, string location_json)
        {
            try
            {
                string[] a = location_id.Split("-");
                if ((a.Length == 3) && (a[0] == "e"))
                {
                    switch (a[1])
                    {
                        case "p":
                            {
                                Province p = JsonConvert.DeserializeObject<Province>(location_json);
                                ViewBag.LocationTypeList = new List<string> { "Tỉnh", "Thành phố" };
                                ViewBag.LocationName = p.Name;
                                ViewBag.LocationType = p.Type;
                                if (p.Status == null)
                                    ViewBag.Status = 0;
                                else
                                    ViewBag.Status = (int)p.Status;
                            }
                            break;
                        case "d":
                            {
                                District d = JsonConvert.DeserializeObject<District>(location_json);

                                ViewBag.LocationTypeList = new List<string> { "Quận", "Huyện", "Thành phố" };
                                ViewBag.LocationName = d.Name;
                                ViewBag.LocationType = d.Type;
                                if (d.Status == null)
                                    ViewBag.Status = 0;
                                else
                                    ViewBag.Status = (int)d.Status;
                            }
                            break;
                        case "w":
                            {
                                Ward w = JsonConvert.DeserializeObject<Ward>(location_json);

                                ViewBag.LocationTypeList = new List<string> { "Phường", "Xã", "Đường" };
                                ViewBag.LocationName = w.Name;
                                ViewBag.LocationType = w.Type;
                                if (w.Status == null)
                                    ViewBag.Status = 0;
                                else
                                    ViewBag.Status = (int)w.Status;
                            }
                            break;
                        default: break;
                    }

                }
                else
                {
                    ViewBag.LocationTypeList = new List<string> { "Tỉnh", "Thành phố", "Quận", "Huyện", "Phường", "Xã", "Đường" };
                }
                return View();
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("OpenEditForm - LocationController with '" + location_json + "' - location_id:" + location_id + " : \n" + ex.ToString());
                string error = "Error: " + ex.ToString();
                return Content(error);
            }
        }
        [HttpPost]
        public async Task<IActionResult> SyncData(string data, int location_type)
        {
            int status = (int)ResponseType.FAILED;
            string msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
            try
            {
                if (data == null || data == "" || location_type < 0)
                {
                    return Ok(new
                    {
                        status = status,
                        msg = msg,
                    });
                }
                RegionModel item = null;
                switch (location_type)
                {
                    case 0:
                        {
                            Province p = JsonConvert.DeserializeObject<Province>(data);
                            if (p == null || p.Name == "" || p.Name == null || p.Type == null || p.Type == "" || p.Id < 0)
                            {

                            }
                            else
                            {
                                item = new RegionModel()
                                {
                                    id = p.ProvinceId,
                                    name = p.Name,
                                    typename = p.Type,
                                    parentid = "-1",
                                    type = 0
                                };
                            }

                        }
                        break;
                    case 1:
                        {
                            District p = JsonConvert.DeserializeObject<District>(data);
                            if (p == null || p.Id < 0 || p.Name == "" || p.Name == null || p.Type == null || p.Type == "" || p.ProvinceId == null || p.ProvinceId == "")
                            {

                            }
                            else
                            {
                                item = new RegionModel()
                                {
                                    id = p.DistrictId,
                                    name = p.Name,
                                    typename = p.Type,
                                    parentid = p.ProvinceId,
                                    type = 1
                                };
                            }

                        }
                        break;
                    case 2:
                        {
                            Ward p = JsonConvert.DeserializeObject<Ward>(data);
                            if (p == null || p.Id < 0 || p.Name == "" || p.Name == null || p.Type == null || p.Type == "" || p.DistrictId == null || p.DistrictId == "")
                            {

                            }
                            else
                            {
                                item = new RegionModel()
                                {
                                    id = p.WardId,
                                    name = p.Name,
                                    typename = p.Type,
                                    parentid = p.DistrictId,
                                    type = 2
                                };
                            }
                        }
                        break;
                    default:
                        {
                            status = (int)ResponseType.FAILED;
                            msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
                        }
                        break;
                }
                if (item != null)
                {
                    string url = ReadFile.LoadConfig().API_USEXPRESS + ReadFile.LoadConfig().API_SYNC_LOCATION;
                    var key_token_api = ReadFile.LoadConfig().KEY_TOKEN_API;
                    HttpClient httpClient = new HttpClient();
                    var j_param = JsonConvert.SerializeObject(item);
                    string token = CommonHelper.Encode(JsonConvert.SerializeObject(j_param), key_token_api);
                    var content = new FormUrlEncodedContent(new[]
                    {
                             new KeyValuePair<string, string>("token", token),
                        });
                    try
                    {
                        var result = await httpClient.PostAsync(url, content);
                        if (result.StatusCode != System.Net.HttpStatusCode.OK)
                        {
                            status = (int)ResponseType.FAILED;

                            msg = "Cannot Connect API UsExpress-OLD.";
                        }
                        else
                        {
                            dynamic resultContent = Newtonsoft.Json.Linq.JObject.Parse(result.Content.ReadAsStringAsync().Result);
                            if (resultContent != null && result.StatusCode == System.Net.HttpStatusCode.OK && resultContent.status == "success")
                            {

                                status = (int)ResponseType.SUCCESS;
                                msg = resultContent.msg;
                            }
                            else if (resultContent != null && resultContent.status == "error")
                            {
                                LogHelper.InsertLogTelegram("Sync Data - LocationController - From API: " + resultContent.msg);
                                msg = resultContent.msg;
                            }
                            else
                            {
                                msg = resultContent.msg;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        status = (int)ResponseType.FAILED;

                        msg = "Cannot Connect or Excute API UsExpress-OLD.";

                    }
                }
                else
                {
                    status = (int)ResponseType.FAILED;
                    msg = "Dữ liệu gửi lên không chính xác, vui lòng thử lại";
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("Sync Data - LocationController: " + ex);
                status = (int)ResponseType.ERROR;
                msg = "Error on Excution";
            }
            return Ok(new
            {
                status = status,
                msg = msg,
            });
        }
    }
}
