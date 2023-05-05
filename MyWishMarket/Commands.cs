using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWishMarket
{
    public enum Commands
    {
        /// <summary>
        /// Запустить бота.
        /// </summary>
        start,

        /// <summary>
        /// Создать желание (указать товар, примерную стоимость, добавить ссылку на товар и тд.).
        /// </summary>
        make_a_wish,

        /// <summary>
        /// Вывести список желаний.
        /// </summary>
        my_wishes,

        /// <summary>
        /// Поделиться списком желаний.
        /// </summary>
        share,

        /// <summary>
        /// Получить код обмена.
        /// </summary>
        get_share_code, 

        /// <summary>
        /// Изменить желание.
        /// </summary>
        change_the_wish,

        /// <summary>
        /// Установить бюджет.
        /// </summary>
        set_budget,

        /// <summary>
        /// Добавить событие.
        /// </summary>
        set_event,

        /// <summary>
        /// Получить информацию о боте.
        /// </summary>
        info,

        /// <summary>
        ///  Назад
        /// </summary>
        back,

        /// <summary>
        /// Выход
        /// </summary>
        exit
    }
}
