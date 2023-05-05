using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyWishMarket.Entities
{
    public class Product
    {
        /// <summary>
        /// Идентификатор товара.
        /// </summary>
        public long ProductId { get; set; }
        
        /// <summary>
        /// Название товара.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Описание товара.
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Ссылка на товар.
        /// </summary>
        public string? Url { get; set; }
        
        /// <summary>
        /// Цена на товар. 
        /// </summary>
        public float? Price { get; set; }
        
        /// <summary>
        /// Изображение товара.
        /// </summary>
        public byte[]? Image { get; set; }
        
        /// <summary>
        /// Приоритет товара.
        /// </summary>
        public int? Priority { get; set; }
        
        /// <summary>
        /// Статус товара.
        /// </summary>
        public bool PurchaseStatus { get; set; }
        
        /// <summary>
        /// Идентификатор пользователя.
        /// </summary>
        public long UserId { get; set; }


    }
}
