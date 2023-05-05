using MyWishMarket.Entities;
using Telegram.Bot.Types;
using Telegram.Bot;
using MyWishMarket.Modes;
using User = MyWishMarket.Entities.User;
using System.Globalization;

namespace MyWishMarket.Handlers
{
    public class AddWishHandler
    {
        User _user;
        
        AddWishHandlerMode _mode = new AddWishHandlerMode();
        
        private ITelegramBotClient _client { get; }
        
        private Product _product { get; set; }
        
        private Update _update { get; }

        public AddWishHandler(ITelegramBotClient client, Update update, User user)
        {
            _client = client;
            _update = update;
            _user = user;
            if (Enum.TryParse(user.AddWishHandlerMode, out AddWishHandlerMode tmpMode) && user.AppMode != AppMode.AddWish.ToString())
            {
                _mode = tmpMode;
            }
            else
            {
                _mode = AddWishHandlerMode.Default;
            }
        }

        public async Task GetOptionsMenu()
        {
            ProductManager productManager = new ProductManager(_client, _update.Message, user: _user);
            await productManager.SendKeyBoardSetting();
        }

        public async Task MessageHandler()
        {
            ProductManager productManager = new ProductManager(_client, _update.Message, user: _user);
            switch (_mode)
            {
                case AddWishHandlerMode.AddTitle:
                    _product = new Product { Name = _update.Message.Text, ProductId = (long)_user.CurrentProductId };
                    await productManager.ChangeWish(_product, "Название готово!");
                    break;
                case AddWishHandlerMode.AddDescription:
                    _product = new Product { Description = _update.Message.Text, ProductId = (long)_user.CurrentProductId };
                    await productManager.ChangeWish(_product, "Описание добавлено 👌");
                    break;
                case AddWishHandlerMode.AddPrice:
                    _product = new Product { Price = float.Parse(_update.Message.Text, CultureInfo.InvariantCulture), ProductId = (long)_user.CurrentProductId };
                    await productManager.ChangeWish(_product, "Цена установлена!");
                    break;
                case AddWishHandlerMode.AddLink:
                    _product = new Product { Url = _update.Message.Text, ProductId = (long)_user.CurrentProductId };
                    await productManager.ChangeWish(_product, "Ссылка записана)");
                    break;
                case AddWishHandlerMode.AddImage:
                    await _client.SendTextMessageAsync(_update.Message.Chat.Id, "Изображение сохранено");
                    break;
                case AddWishHandlerMode.AddStatus:
                    await productManager.ChangeWish(_product, "Статус обновлен!");
                    break;
                case AddWishHandlerMode.Default: // После обновление информации товары, сбрасывается мод. И следующее сообщение будет воспринято как добавление товара
                default:
                    if (_user.AppMode == AppMode.ChangeWish.ToString())
                    {
                        await _client.SendTextMessageAsync(_update.Message.Chat.Id, "Команда не распознана! Попробуйте ещё раз или /exit");
                        break;
                    }
                    _product = new Product { Name = _update.Message.Text, UserId = _user.UserId };
                    await productManager.AddNewWish(_product);
                    break;
            }
        }
        public async Task CallbackQueryHandler()
        {
            ProductManager productManager = new ProductManager(client: _client, callbackQuery: _update.CallbackQuery, user: _user);
            await productManager.OnAnswer();
        }
    }
}
