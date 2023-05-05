using Microsoft.EntityFrameworkCore;
using MyWishMarket.EnityFramework.DBOptions;
using NLog.Fluent;
using System.Runtime.Caching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyWishMarket.Entities;
using MyWishMarket.Modes;

namespace MyWishMarket.EnityFramework.Business
{
    public class EntityDataLayer /*: IDisposable*/
    {
        public DataContext Db { get; set; }
        private MemoryCache cache { get; set; }

        /// <summary>
        /// Основной конструктор
        /// </summary>
        public EntityDataLayer()
        {
            Db = new DataContext();
            Db.Database.SetCommandTimeout(120);
            cache = MemoryCache.Default;
        }

        //private bool disposed = false;

        //// реализация интерфейса IDisposable.
        //public void Dispose()
        //{
        //    Dispose(true);
        //    // подавляем финализацию
        //    GC.SuppressFinalize(this);
        //}

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposed)
        //    {
        //        if (disposing)
        //        {
        //            Db.Dispose();
        //            Db = null;
        //        }
        //        // освобождаем неуправляемые объекты
        //        disposed = true;
        //    }
        //}

        //// Деструктор
        //~EntityDataLayer()
        //{
        //    Dispose(false);
        //}

        public User GetRecipient(long userId, string shareCode)
        {
            var user = Db.User.Where(x => x.UserId == userId).FirstOrDefault(x => x.ShareCode == shareCode);
            if (user != null)
            {
                return user;
            }
            else
            {
                throw new Exception("Not found");
            }
        }

        public List<Product> GetAllProducts(long userId)
        {
            return Db.Product.Where(x => x.UserId == userId).OrderBy(x => x.ProductId).ToList();
        }

        public Product GetCurrentProduct(long userId)
        {
            var currentProductId = Db.User.FirstOrDefault(x => x.UserId == userId).CurrentProductId;
            return Db.Product.FirstOrDefault(x => x.ProductId == currentProductId);
        }

        public User GetUser(Telegram.Bot.Types.Update update)
        {
            User isKnown;
            long id = 0;
            if (update.Message != null)
            {
                isKnown = Db.User.FirstOrDefault(x => x.ChatId == update.Message.Chat.Id);
            }
            else if (update.CallbackQuery != null)
            {
                isKnown = Db.User.FirstOrDefault(x => x.ChatId == update.CallbackQuery.Message.Chat.Id);
            }
            else
            {
                throw new Exception("Непредвиденная обработка");
            }
            if (isKnown is null)
            {
                Db.User.Add(new User()
                {
                    ChatId = update.Message.Chat.Id,
                    AddWishHandlerMode = "Default",
                    AppMode = "Default",
                    Name = update.Message.Chat.FirstName == null ? "" : update.Message.Chat.FirstName
                });
                Db.SaveChanges();
                isKnown = Db.User.FirstOrDefault(x => x.ChatId == update.Message.Chat.Id);
                Log.Info("Добавлен новый пользователь");
            }
            return Db.User.First(x => x.UserId == isKnown.UserId);
        }

        public void SetBudget(User user)
        {
            var userInfo = Db.User.First(x => x.UserId == user.UserId);
            userInfo.Budget = user.Budget;
            userInfo.AppMode = user.AppMode;
            Db.SaveChanges();
        }

        public string GetNewShareCode(User user)
        {
            string guid;
            do
            {
                guid = Guid.NewGuid().ToString().Substring(0, 5);
            }
            while (Db.User.FirstOrDefault(x => x.ShareCode == guid) != null);
            Db.User.FirstOrDefault(x => x.UserId == user.UserId).ShareCode = guid;
            Db.SaveChanges();
            return guid;

        }

        public void SetAppMode(User user)
        {
            Db.User.First(x => x.UserId == user.UserId).AppMode = user.AppMode;
            Db.SaveChanges();
        }

        public void SetAddWishHandlerMode(User user)
        {
            var userInfo = Db.User.First(x => x.UserId == user.UserId);
            userInfo.AddWishHandlerMode = user.AddWishHandlerMode;
            userInfo.AppMode = user.AppMode;
            Db.SaveChanges();
        }

        public void AddNewProduct(Product product)
        {
            Db.Product.Add(product);
            Db.SaveChanges();
            var tmpProduct = Db.Product.Where(x => product.UserId == x.UserId).OrderBy(x => x.ProductId).Last();
            var user = Db.User.FirstOrDefault(x => x.UserId == product.UserId);
            user.CurrentProductId = tmpProduct.ProductId;
            user.AppMode = "ChangeWish";
            user.AddWishHandlerMode = AddWishHandlerMode.Default.ToString();
            Db.SaveChanges();
        }

        public bool ChooseProduct(long productId, long userId)
        {
            var product = Db.Product.FirstOrDefault(x => x.ProductId == productId && x.UserId == userId);
            if (product == null)
            {
                return false;
            }
            else
            {
                var user = Db.User.FirstOrDefault(x => x.UserId == userId);
                user.CurrentProductId = product.ProductId;
                user.AppMode = AppMode.ChangeWish.ToString();
                Db.SaveChanges();
                return true;
            }
        }

        public void ChangeProduct(Product product, AddWishHandlerMode mode)
        {
            var tmpProduct = Db.Product.First(x => x.ProductId == product.ProductId);
            switch (mode)
            {
                case AddWishHandlerMode.AddTitle:
                    tmpProduct.Name = product.Name;
                    break;
                case AddWishHandlerMode.AddDescription:
                    tmpProduct.Description = product.Description;
                    break;
                case AddWishHandlerMode.AddPrice:
                    tmpProduct.Price = product.Price;
                    break;
                case AddWishHandlerMode.AddLink:
                    tmpProduct.Url = product.Url;
                    break;
                case AddWishHandlerMode.AddImage:
                    tmpProduct.Image = product.Image;
                    break;
                case AddWishHandlerMode.AddStatus:
                    tmpProduct.PurchaseStatus = product.PurchaseStatus;
                    break;
            }
            Db.User.First(x => x.UserId == tmpProduct.UserId).AddWishHandlerMode = AddWishHandlerMode.Default.ToString();
            Db.SaveChanges();
        }
    }
}
