using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWishMarket.Modes
{
    /// <summary>
    /// Состояние товара
    /// </summary>
    public enum AddWishHandlerMode
    {
        Default,
        AddTitle,
        AddDescription,
        AddPrice,
        AddLink,
        AddImage,
        AddStatus,
    }
}
