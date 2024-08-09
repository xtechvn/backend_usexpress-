using System;
using System.Collections.Generic;
using System.Text;

namespace Utilities.Contants
{
   public class ClientSourceType
    {
        public enum SourceType
        {
            PC = 1, // dang ky tu Fronend ban PC
            SYSTEM_OLD = 2, // dang ky tu HE THONG CU
            APP = 3, // dang ky tu APP
            GOOGLE = 4, // dang ky tu GL
            FACEBOOK = 5 // dang ky tu FB
        }
    }
}
