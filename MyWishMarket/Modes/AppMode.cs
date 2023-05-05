using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWishMarket.Modes
{
    /// <summary>
    /// Состояние меню пользователя
    /// </summary>
    public enum AppMode
    {
        Default = 0,
        AddWish = 1,
        ChooseWish = 2,
        ChangeWish = 3,
        WishInfo = 4,
        SetBudget = 5,
        Sendwish = 6,
    }
}
