using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helper.Helper.Common
{
    public class Enums
    {
        public enum ImageType
        {
            GROUP = 1,
            CATEGORY = 2,
            SUBCATEGORY = 3,
            ITEM = 4
        }
        public enum OrderStatus
        {
            New = 1,
            Accepted = 2,
            CookAssigned = 3,
            Ready = 4,
            DeliveryAssigned = 5,
            Completed = 6,
            Cancelled = 7
        }

    }
}
