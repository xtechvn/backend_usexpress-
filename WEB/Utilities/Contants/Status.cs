using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
    public enum StatusType
    {
        BINH_THUONG = 0,
        KHOA_TAM_DUNG = 1
    }
    public enum ResponseType
    {
        SUCCESS = 0,
        FAILED = 1,
        ERROR = 2,
        EXISTS = 3,
        PROCESSING = 4,
        EMPTY = 5,
        CONFIRM = 6,
        NOT_EXISTS=7
    }
    public enum Status
    {
        HOAT_DONG = 0,
        KHONG_HOAT_DONG = 1
    }

    public class ApiStatusType
    {
        public const string SUCCESS = "success";
        public const string FAILED = "failed";
        public const string EMPTY = "empty";
        public const string PROCESSING = "processing";
        public const string ERROR = "error";
    }

    public struct ArticleStatus
    {
        public const int PUBLISH = 0; // BÀI XUẤT BẢN
        public const int SAVE = 1; // BÀI LƯU TẠM
        public const int REMOVE = 2; // BÀI BỊ HẠ
    }
}
