using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types;
using Telegram.Bot;
using MyWishMarket.Entities;
using MyWishMarket.EnityFramework.Business;
using User = MyWishMarket.Entities.User;
using MyWishMarket.Modes;

namespace MyWishMarket
{
    public class ProductManager
    {
        ITelegramBotClient _client;
        
        Chat _chat;
        
        CallbackQuery _callbackQuery;
        
        AddWishHandlerMode mode;
        
        EntityDataLayer efDataLayer = new EntityDataLayer();
        
        User _user;

        InlineKeyboardMarkup inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Название","Title"),
                InlineKeyboardButton.WithCallbackData("Описание","Description")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Цена","Price"),
                InlineKeyboardButton.WithCallbackData("Ссылка","Link")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Изображение","Image"),
                InlineKeyboardButton.WithCallbackData("Статус покупки","Status")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Предварительный просмотр","Preview"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Назад","Back"),
            }
        });

        InlineKeyboardMarkup inlineKeyboardStatus = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("Куплен","Purchased"),
                InlineKeyboardButton.WithCallbackData("Не куплен","Not purchased")
            }
        });

        public ProductManager(ITelegramBotClient client, Message message = null, CallbackQuery callbackQuery = null, User user = null)
        {
            this._client = client;
            if (message != null)
            {
                this._chat = message.Chat;
            }
            else if (callbackQuery != null)
            {
                _callbackQuery = callbackQuery;
                this._chat = callbackQuery.Message.Chat;
            }
            _user = user;
        }

        public async Task AddNewWish(Product product)
        {
            await Task.Delay(500);
            efDataLayer.AddNewProduct(product);
            await _client.SendTextMessageAsync(_chat.Id, "Отлично! Желание создано. Выбери что хочешь добавить/изменить:", replyMarkup: inlineKeyboard);
        }

        public async Task ChangeWish(Product product, string text)
        {
            await Task.Delay(500);
            var tMode = Enum.Parse<AddWishHandlerMode>(_user.AddWishHandlerMode);
            efDataLayer.ChangeProduct(product, tMode);
            await _client.SendTextMessageAsync(_chat.Id, $"{text} Выбери что хочешь добавить/изменить:", replyMarkup: inlineKeyboard);
        }

        public async Task SendKeyBoardSetting()
        {
            await Task.Delay(500);
            await _client.SendTextMessageAsync(_chat.Id, $"Выбери что хочешь добавить/изменить:", replyMarkup: inlineKeyboard);
        }

        public async Task OnAnswer()
        {
            CallbackQuery callbackQuery = _callbackQuery;

            switch (callbackQuery.Data)
            {
                case "Title":
                    mode = Modes.AddWishHandlerMode.AddTitle;
                    await _client.SendTextMessageAsync(_chat.Id, "Введите название:");
                    break;
                case "Description":
                    mode = Modes.AddWishHandlerMode.AddDescription;
                    await _client.SendTextMessageAsync(_chat.Id, "Введите описание:");
                    break;
                case "Price":
                    mode = Modes.AddWishHandlerMode.AddPrice;
                    await _client.SendTextMessageAsync(_chat.Id, "Введите цену:");
                    break;
                case "Link":
                    mode = Modes.AddWishHandlerMode.AddLink;
                    await _client.SendTextMessageAsync(_chat.Id, "Введите ссылку на товар:");
                    break;
                case "Image":
                    mode = Modes.AddWishHandlerMode.Default;
                    //mode = Modes.AddWishHandlerMode.AddImage;
                    //await _client.SendTextMessageAsync(_chat.Id, "Вставьте изображение товара:");
                    await _client.SendTextMessageAsync(_chat.Id, "Функция ещё на тестировании) Ждите обновлений!");
                    break;
                case "Status":
                    mode = Modes.AddWishHandlerMode.AddStatus;
                    await _client.SendTextMessageAsync(_chat.Id, "Укажите статус товара", replyMarkup: inlineKeyboardStatus);
                    break;
                case "Purchased":
                case "Not purchased":
                    mode = AddWishHandlerMode.AddStatus;
                    var _product = new Product
                    {
                        PurchaseStatus = callbackQuery.Data == "Purchased" ? true : false,
                        ProductId = (long)_user.CurrentProductId
                    };
                    await ChangeWish(_product, "Статус товара установлен!");
                    break;
                case "Preview":
                    await Preview();
                    break;
                case "Back":
                    _user.AppMode = AppMode.Default.ToString();
                    _user.AddWishHandlerMode = AddWishHandlerMode.Default.ToString();
                    break;
                default:
                    break;
            }
            _user.AddWishHandlerMode = mode.ToString();
            efDataLayer.SetAddWishHandlerMode(_user);
        }

        public async Task Preview()
        {
            Product userProduct = new Product();
            userProduct = efDataLayer.GetCurrentProduct(_user.UserId);
            string status = userProduct.PurchaseStatus ? "Куплен" : "Не куплен";
            await _client.SendTextMessageAsync(_chat.Id,
                    $"Идентификатор товара: {userProduct.ProductId}\n" +
                    $"Название: {userProduct.Name}\n" +
                    $"Описание: {userProduct.Description}\n" +
                    $"Ссылка: {userProduct.Url}\n" +
                    $"Цена: {userProduct.Price}\n" +
                    $"Статус покупки: {status}\n");
            await _client.SendTextMessageAsync(_chat.Id, $"Выбери что хочешь добавить/изменить:", replyMarkup: inlineKeyboard);
        }

        public async Task AllProducts()
        {
            List<Product> userProducts = efDataLayer.GetAllProducts(_user.UserId);
            float priceSum = 0;
            if(userProducts.Count == 0)
            {
                await _client.SendTextMessageAsync(_chat.Id, $"Список товаров не найден!");
                return;
            }
            foreach (var product in userProducts)
            {
                string status = product.PurchaseStatus ? "Куплен" : "Не куплен";
                await _client.SendTextMessageAsync(_chat.Id,
                    $"Идентификатор товара: {product.ProductId}\n" +
                    $"Название: {product.Name}\n" +
                    $"Описание: {product.Description}\n" +
                    $"Ссылка: {product.Url}\n" +
                    $"Цена: {product.Price}\n" +
                    $"Статус покупки: {status}\n");
                if (product.Price.HasValue)
                {
                    priceSum += product.Price.Value;
                }
            }
            await _client.SendTextMessageAsync(_chat.Id, $"Ваш бюджет: {_user.Budget} руб.\nНеобходимая сумма: {priceSum} руб.");
        }
    }
}
