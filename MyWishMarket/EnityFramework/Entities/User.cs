using MyWishMarket.Modes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWishMarket.Entities
{
    public class User
    {
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public long UserId { get; set; }
        
        /// <summary>
        /// Идентификатор чата.
        /// </summary>
        public long ChatId { get; set; }
        
        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Состояние пользователя.
        /// </summary>
        public string AppMode { get; set; }
        
        /// <summary>
        /// Состояние товара. 
        /// </summary>
        public string AddWishHandlerMode { get; set; }
        
        /// <summary>
        /// Изменяемый товар.
        /// </summary>
        public long? CurrentProductId { get; set; }
        
        /// <summary>
        /// Бюджет.
        /// </summary>
        public int Budget { get; set; }
        
        /// <summary>
        /// Код обмена.
        /// </summary>
        public string? ShareCode { get; set; }
    }
}
