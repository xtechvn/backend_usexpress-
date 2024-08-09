//--Background Config:
const exprire_time_store = 24 * 60 * 60 * 1000;
const remove_check_time = 60 * 60 * 1000;
const get_fee_new_url = 'http://api.usexpressvn.com/api/ServicePublic/extension/get-fee.json';
const get_xpath_url = 'http://api.usexpressvn.com/api/ServicePublic/extension/get-xpath';
const key_token_api = 'kq1jnJAJShgdRPYjiMyi';
const xpath_key = 'od5XHJ1tIJQlSET9aTAulNO5ES1XTirfI2epe1QV';
//-- Init Value:
var xpath_list = undefined;
var excute_flag = 0;

chrome.runtime.onInstalled.addListener(details => {
  _common.FetchAPI(function (value) {

  });
  setInterval(function () {
    _background_service.RemoveProductDetail();
  }, remove_check_time);
  setInterval(function () {
    _common.FetchAPI(function (value) {

    });
  }, exprire_time_store);
});
chrome.runtime.onMessage.addListener(
  function (request, sender, sendResponse) {
    switch (request.function_name) {

      case "get_fee_new": {
        try {
          //-- Get From Local
          var key = "us_" + _common.GetAmzASINFromURL(request.url) + '_' + request.label_id;
          _background_service.GetProductDetailByKey(key, function (value) {
            if (value != undefined && value != null && value.data != undefined && value.data != {} && value.data.PRICE > 0) {
              value = JSON.parse(value);
              // debugger;
              sendResponse(
                {
                  response: "Success",
                  PRICE: value.data.product_price == null ? 0 : value.data.product_price,
                  FIRST_POUND_FEE: value.data.first_pound_fee == null ? 0 : value.data.first_pound_fee,
                  NEXT_POUND_FEE: value.data.next_pound_fee == null ? 0 : value.data.next_pound_fee,
                  LUXURY_FEE: value.data.luxury_fee == null ? 0 : value.data.luxury_fee,
                  DISCOUNT_FIRST_FEE: value.data.discount_first_fee == null ? 0 : value.data.discount_first_fee,
                  TOTAL_SHIPPING_FEE: value.data.total_shipping_fee == null ? 0 : value.data.total_shipping_fee,
                  PRICE_LAST: value.data.price_last == null ? 0 : value.data.price_last,
                  PRICE_LAST_VND: value.data.price_last_vnd == null ? 0 : value.data.price_last_vnd,
                  url_usexpress_detail: value.data.url
                });
            }
            else {
              var apiPrefix = get_fee_new_url;
              var j_param = {
                price: request.price,
                label_id: request.label_id,
                pound: request.item_weight,
                shipping_fee: request.shipping_fee,
                unit: request.unit,
                product_name: request.product_name,
                product_code: _common.GetAmzASINFromURL(request.url),
              };
              //-- From API, switch label:
              switch (request.label_id) {
                case 7: {
                  j_param.product_name = 'jomashop';
                  j_param.product_code = _common.GetOtherLabelASINFromURL(request.url, request.label_id);;
                } break;
              }
              var base64String = _common.KeyEncode(j_param, key_token_api);
              var token = base64String;
              fetch(apiPrefix,
                {
                  method: 'POST',
                  headers: {
                    'Accept': 'application/json, application/xml, text/plain, text/html, *.*',
                    'Content-Type': 'application/x-www-form-urlencoded; charset=utf-8'
                  },
                  body: 'token=' + token
                })
                .then(function (response) {
                  if (response.status !== 200) {
                    sendResponse
                      ({
                        response: "Failed",

                      });
                  }
                  response.json().then(function (data) {
                    var value =
                    {
                      data: data.data,
                      created_time: Date.now()
                    };
                    // debugger;
                    _background_service.SetProductDetail(key, value);
                    var response = {
                      response: "Success",
                      PRICE: value.data.product_price == null ? 0 : value.data.product_price,
                      FIRST_POUND_FEE: value.data.first_pound_fee == null ? 0 : value.data.first_pound_fee,
                      NEXT_POUND_FEE: value.data.next_pound_fee == null ? 0 : value.data.next_pound_fee,
                      LUXURY_FEE: value.data.luxury_fee == null ? 0 : value.data.luxury_fee,
                      DISCOUNT_FIRST_FEE: value.data.discount_first_fee == null ? 0 : value.data.discount_first_fee,
                      TOTAL_SHIPPING_FEE: value.data.total_shipping_fee == null ? 0 : value.data.total_shipping_fee,
                      PRICE_LAST: value.data.price_last == null ? 0 : value.data.price_last,
                      PRICE_LAST_VND: value.data.price_last_vnd == null ? 0 : value.data.price_last_vnd,
                      url_usexpress_detail: value.data.url
                    };
                    sendResponse(response);
                  });
                }).catch(function (err) {
                  // console.log('Error:', err)
                  sendResponse
                    ({
                      response: "Failed",

                    });
                });
            }
          });
        } catch (err) {
          // console.log('Error:', err)
          sendResponse
            ({
              response: "Failed",

            });
        };
      } break;
      case "get_product_code": {
        switch (request.label_id) {
          case 1: {
            var product_code = _common.GetAmzASINFromURL(request.url);
            sendResponse
              ({
                response: "Success",
                product_code: product_code,
              });
          } break;
          case 7: {
            var product_code = _common.GetOtherLabelASINFromURL(request.url, request.label_id);
            sendResponse
              ({
                response: "Success",
                product_code: product_code,
              });
          } break;
        }
      } break;
      case "get_xpath": {
        try {
          if (excute_flag == 0) {
            _common.FetchAPI(function (value) {
              if (value == undefined) {
                sendResponse
                  ({
                    response: "Failed",
                  });
              }
              else {
                sendResponse
                  ({
                    response: "Success",
                    data: value[request.label_name.toLowerCase()],
                  });
              }
            });
          } else {
            sendResponse
              ({
                response: "Success",
                data: xpath_list[request.label_name.toLowerCase()],
              });
          }
        } catch (err) {
          sendResponse
            ({
              response: "Failed"
            });
        }
      } break;
      default: break;
    }
    return true;
  });
var _background_service = {

  GetProductDetailByKey: function (key, callback) {
    chrome.storage.local.get("" + key, function (result) {
      callback(result[key]);
    });
  },
  SetProductDetail: function (key, object) {
    var value = JSON.stringify(object);
    if (object.data != undefined) {
      chrome.storage.local.set({ ["" + key]: value }, function () {
        // console.log('Value set to ' + key);
      });
    }
  },
  RemoveProductDetail: function () {
    chrome.storage.local.get(null, function (items) {
      for (key in items) {
        // console.log("Check key: " + key);
        var data = JSON.parse(items[key]);
        if (data.created_time + exprire_time_store < Date.now()) {
          chrome.storage.local.remove(["" + key], function () {
            var error = chrome.runtime.lastError;
            if (error) {
              console.error(error);
            }
            else {
              // console.log('Deleted ' + key);

            }
          });
        }
      }
    });

  }

};

var _common = {
  base64abc: [
    "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M",
    "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
    "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
    "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
    "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "+", "/"
  ],
  KeyED: function (param, key) {
    var strStringLength = param.length;
    var strKeyPhraseLength = key.length;
    var unit8array = [];
    for (i = 0; i < strStringLength; i++) {
      var pos = i % strKeyPhraseLength;
      var var1 = param.charCodeAt(i);
      var var2 = key.charCodeAt(pos);
      var xorCurrPos = var1 ^ var2;
      unit8array[i] = String.fromCharCode(xorCurrPos);

    }
    var result_char = unit8array.join('');
    return result_char;
  },
  KeyEncode: function (strString, strKeyPhrase) {
    var encode_ouput = _common.KeyED(JSON.stringify(strString), strKeyPhrase);
    var utf8bytestr = _common.stringToUtf8ByteArray(encode_ouput);
    strString = _common.bytesToBase64(utf8bytestr);
    return strString;
  },

  bytesToBase64: function (bytes) {
    let result = '', i, l = bytes.length;
    for (i = 2; i < l; i += 3) {
      result += _common.base64abc[bytes[i - 2] >> 2];
      result += _common.base64abc[((bytes[i - 2] & 0x03) << 4) | (bytes[i - 1] >> 4)];
      result += _common.base64abc[((bytes[i - 1] & 0x0F) << 2) | (bytes[i] >> 6)];
      result += _common.base64abc[bytes[i] & 0x3F];
    }
    if (i === l + 1) { // 1 octet yet to write
      result += _common.base64abc[bytes[i - 2] >> 2];
      result += _common.base64abc[(bytes[i - 2] & 0x03) << 4];
      result += "==";
    }
    if (i === l) { // 2 octets yet to write
      result += _common.base64abc[bytes[i - 2] >> 2];
      result += _common.base64abc[((bytes[i - 2] & 0x03) << 4) | (bytes[i - 1] >> 4)];
      result += _common.base64abc[(bytes[i - 1] & 0x0F) << 2];
      result += "=";
    }
    return result;
  },


  stringToUtf8ByteArray: function (str) {
    var out = [], p = 0;
    for (var i = 0; i < str.length; i++) {
      var c = str.charCodeAt(i);
      if (c < 128) {
        out[p++] = c;
      } else if (c < 2048) {
        out[p++] = (c >> 6) | 192;
        out[p++] = (c & 63) | 128;
      } else if (
        ((c & 0xFC00) == 0xD800) && (i + 1) < str.length &&
        ((str.charCodeAt(i + 1) & 0xFC00) == 0xDC00)) {
        // Surrogate Pair
        c = 0x10000 + ((c & 0x03FF) << 10) + (str.charCodeAt(++i) & 0x03FF);
        out[p++] = (c >> 18) | 240;
        out[p++] = ((c >> 12) & 63) | 128;
        out[p++] = ((c >> 6) & 63) | 128;
        out[p++] = (c & 63) | 128;
      } else {
        out[p++] = (c >> 12) | 224;
        out[p++] = ((c >> 6) & 63) | 128;
        out[p++] = (c & 63) | 128;
      }
    }
    return out;
  },
  // Get ASIN From URL
  GetAmzASINFromURL: function (link) {
    try {
      if (link.split('/')[0].indexOf("http") == -1) {
        link = "https://www.amazon.com" + link;
      }
      // Convert to Single Line
      link = link.replace(/\n/g, '');
      var regex_url_case_1 = "https://www.amazon.com/([\\w-]+/)?(dp|gp/product|gp/aw/d)/(\\w+/)?(\\w{10})";
      var regex_url_case_2 = "(?:[/dp/]|$)([A-Z0-9]{10})";
      var match = link.match(regex_url_case_1);
      var asin_match;
      if (match != null) {
        asin_match = match[0];
      }
      else {
        match = link.match(regex_url_case_2);
        asin_match = match[0];
      }
      var array = asin_match.split('/');
      var asin = array[array.length - 1];
      if (asin.indexOf("B0") >= 1) {
        array = asin.split("B0");
        asin = "B0" + array[array.length - 1];
      }
      return (asin);
    }
    catch (err) {
      // console.log(err);
      return (null);
    }
  },
  // Get ASIN From URL
  GetOtherLabelASINFromURL: function (link, label_id) {
    switch (label_id) {
      case 1: {
        return _common.GetAmzASINFromURL(link);
      }
      case 7: {
        var path = link.split('/');
        return path[path.length - 1];
      }
    }
  },
  FetchAPI: function (callback) {
    var apiPrefix = get_xpath_url;
    fetch(apiPrefix,
      {
        method: 'POST',
        headers: {
          'Accept': 'application/json, application/xml, text/plain, text/html, *.*',
          'Content-Type': 'application/x-www-form-urlencoded; charset=utf-8'
        },
        body: 'key=' + xpath_key
      })
      .then(function (response) {
        response.json().then(function (data) {
          xpath_list = {};
          if (data.status == 0) {
            xpath_list = data.data;
            excute_flag = 1;
          }
          callback(xpath_list);
        })

      })
      .catch(function (error) {
        callback(undefined);

      });
  }
};