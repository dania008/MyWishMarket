using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using System.Globalization;
using Microsoft.VisualBasic;
using MyWishMarket.Entities;
using User = Telegram.Bot.Types.User;
using MyWishMarket.Modes;
using MyWishMarket.Handlers;
using MyWishMarket.EnityFramework.Business;
using static System.Net.Mime.MediaTypeNames;
using Npgsql.Replication.PgOutput.Messages;

namespace MyWishMarket
{
    public class BotManager
    {
        #region BotToken
        const string token = "token";
        #endregion
        const string commands = $"\n- /make_a_wish - создать желание\n- /my_wishes - посмотреть список желаний\n- /share - поделиться\n- " +
            $"/get_share_code - получить код обмена\n- /change_the_wish - изменить желание\n- /set_budget - добавить/изменить бюджет\n- /set_event - добавить событие\n- /info - информация о боте";

        TelegramBotClient client = new TelegramBotClient(token);

        AppMode mode = new AppMode();

        Chat userChat = new Chat();

        Entities.User user;

        public static AddWishHandler addWishHandler;

        EntityDataLayer efDataLayer = new EntityDataLayer();

        async public Task Start()
        {
            User bot = await client.GetMeAsync();
            client.StartReceiving(HandleUpdateAsync, ErrorHandler);
            Console.ReadLine();
        }

        public static Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken ct)
        {
            var ErrorMessage = exception.ToString();
            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Обработчик каждого события
        /// </summary>
        public async Task HandleUpdateAsync(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            try
            {
                efDataLayer = new EntityDataLayer();
                user = efDataLayer.GetUser(update);

                if (Enum.TryParse(user.AppMode, out AppMode tmpMode))
                {
                    mode = tmpMode;
                }
                else
                {
                    mode = default;
                };

                if (update.Message != null)
                {
                    var message = update.Message;
                    userChat = update.Message.Chat;

                    if (Enum.TryParse(message.Text.Substring(1).ToLower(), out Commands command) && !int.TryParse(message.Text.Substring(1).ToLower(), out int result))
                    {
                        mode = default;
                    }

                    if (user.AppMode != mode.ToString())
                    {
                        user.AppMode = mode.ToString();
                        efDataLayer.SetAppMode(user);
                    }

                    Console.WriteLine($"Пришло сообщение '*{message.Text}*' дата {message.Date}");
                    if (message.Text == "/exit")
                    {
                        await Exit(client, update.Message);
                        return;
                    }
                }
                else if (update.CallbackQuery.Message != null)
                {
                    userChat = update.CallbackQuery.Message.Chat;
                    var message = update.CallbackQuery.Message;
                    Console.WriteLine($"Пришла команда от клавиатуры: '*{message.Text}*' дата {message.Date}");
                }
                else
                {
                    return;
                }

                if (user.AppMode != mode.ToString())
                {
                    user.AppMode = mode.ToString();
                    efDataLayer.SetAppMode(user);
                }


                switch (update.Type)
                {
                    case UpdateType.Message:
                        await DefaultMessageHandler(client, update, ct);
                        break;
                    case UpdateType.CallbackQuery:
                        await DefaultCallbackQueryHandler(client, update, ct);
                        break;
                    case UpdateType.MyChatMember:
                        // Cообщает боту, когда один из участников чата (в том числе и бот) меняет свой статус в этом чате.
                        return;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                await ErrorHandler(client, ex, ct);
                throw;
            }
        }

        /// <summary>
        /// Обработчик каждого CallbackQuery.
        /// </summary>
        async Task DefaultCallbackQueryHandler(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            switch (mode)
            {
                case AppMode.Default:
                    break;
                case AppMode.AddWish:
                case AppMode.ChangeWish:
                    addWishHandler = new AddWishHandler(client, update, user);
                    await addWishHandler.CallbackQueryHandler();
                    if (efDataLayer.GetUser(update).AppMode == AppMode.Default.ToString())
                    {
                        await client.SendTextMessageAsync(userChat, GetGreeting(userChat, false).Result);
                    }
                    break;
            }
        }

        /// <summary>
        /// Обработчик каждого сообщения.
        /// </summary>
        async Task DefaultMessageHandler(ITelegramBotClient client, Update update, CancellationToken ct)
        {
            switch (mode)
            {
                case AppMode.Default:
                    await MainMenuMessageMode(client, update);
                    break;
                case AppMode.AddWish:
                case AppMode.ChangeWish:
                    AddWishHandler addWishHandler = new AddWishHandler(client, update, user);
                    await addWishHandler.MessageHandler();
                    break;
                case AppMode.ChooseWish:
                    try
                    {
                        if (efDataLayer.ChooseProduct(long.Parse(update.Message.Text), user.UserId))
                        {
                            AddWishHandler wishHandler = new AddWishHandler(client, update, user);
                            await wishHandler.GetOptionsMenu();
                        }
                        else
                        {
                            await client.SendTextMessageAsync(update.Message.Chat.Id, "Товар с заданным Id не найден. Попробуйте ещё раз или /exit");
                        }
                    }
                    catch (Exception)
                    {
                        await client.SendTextMessageAsync(update.Message.Chat.Id, "Что-то пошло не так. Попробуйте ещё раз или /exit");
                    }
                    break;
                case AppMode.SetBudget:
                    var correctBudget = int.TryParse(update.Message.Text, out int money);
                    if (correctBudget)
                    {
                        user.Budget = money;
                        user.AppMode = AppMode.Default.ToString();
                        efDataLayer.SetBudget(user);
                        await client.SendTextMessageAsync(update.Message.Chat.Id, $"Информация успешна обновлена!\nВаш бюджет составляет: {user.Budget} руб.");
                        await client.SendTextMessageAsync(update.Message.Chat.Id, GetGreeting(update.Message.Chat, false).Result);
                    }
                    else
                    {
                        await client.SendTextMessageAsync(update.Message.Chat.Id, "Некорректный формат. Попробуйте ещё раз или /exit");
                    }
                    break;
                case AppMode.Sendwish:
                    try
                    {
                        string[] values = update.Message.Text.Split(' ');
                        long userId = long.Parse(values[0]);
                        string shareCode = values[1];
                        var recipient = efDataLayer.GetRecipient(userId, shareCode);
                        List<Product> products = efDataLayer.GetAllProducts(user.UserId);
                        if (products.Count > 0)
                        {
                            await client.SendTextMessageAsync(recipient.ChatId, $"Вам пришёл список товаров от пользователя {recipient.Name} :");
                            foreach (var product in products)
                            {
                                string status = product.PurchaseStatus ? "Куплен" : "Не куплен";
                                await client.SendTextMessageAsync(recipient.ChatId,
                                    $"Идентификатор товара: {product.ProductId}\n" +
                                    $"Название: {product.Name}\n" +
                                    $"Описание: {product.Description}\n" +
                                    $"Ссылка: {product.Url}\n" +
                                    $"Цена: {product.Price}\n" +
                                    $"Статус покупки: {status}\n");
                            }
                            user.AppMode = AppMode.Default.ToString();
                            efDataLayer.SetAppMode(user);
                        }
                        else
                        {
                            await client.SendTextMessageAsync(recipient.ChatId, $"Ваш список товаров пуст( Добавьте в желания хотя бы один товар!");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "Not found")
                        {
                            await client.SendTextMessageAsync(update.Message.Chat.Id, "Пользователь не найден! Попробуйте ещё раз или /exit");
                        }
                        else
                        {
                            await client.SendTextMessageAsync(update.Message.Chat.Id, "Что-то пошло не так. Попробуйте ещё раз или /exit");
                        }
                    }
                    break;
                case AppMode.WishInfo:
                    break;

            }
        }

        /// <summary>
        /// Главное меню
        /// </summary>
        async Task MainMenuMessageMode(ITelegramBotClient client, Update update)
        {
            Message message = update.Message;
            var text = message.Text;
            Enum.TryParse(text.Substring(1), out Commands command);
            switch (command)
            {
                case Commands.start:
                    await client.SendTextMessageAsync(message.Chat.Id, GetGreeting(message.Chat, true).Result);
                    return;
                case Commands.make_a_wish:
                    user.AppMode = AppMode.AddWish.ToString();
                    await client.SendTextMessageAsync(message.Chat.Id, "Введите название товара или /exit");
                    break;
                case Commands.my_wishes:
                    ProductManager productManager = new ProductManager(client, message, user: user);
                    await productManager.AllProducts();
                    break;
                case Commands.share:
                    user.AppMode = AppMode.Sendwish.ToString();
                    await client.SendTextMessageAsync(message.Chat.Id, "Для того, чтобы человек смог получить список, необходимо ввести код обмена и идентификатор пользователя.\nКод и идентификатор, получатель сможет увидеть, выполнив команду /get_share_code.\nВведите идентификатор пользователя и код обмена через пробел или /exit");
                    break;
                case Commands.get_share_code:
                    var shareCode = efDataLayer.GetNewShareCode(user);
                    await client.SendTextMessageAsync(message.Chat.Id, $"Id пользователя: {user.UserId}\nКод обмена:{shareCode}");
                    break;
                case Commands.change_the_wish:
                    user.AppMode = AppMode.ChooseWish.ToString();
                    await client.SendTextMessageAsync(message.Chat.Id, "Введите Id вашего товара или /exit");
                    break;
                case Commands.set_budget:
                    user.AppMode = AppMode.SetBudget.ToString();
                    await client.SendTextMessageAsync(message.Chat.Id, $"Ваш бюджет составляет: {user.Budget} руб.\nВведите новое значение, если хотите обновить финансы или /exit");
                    break;
                case Commands.set_event:
                    await client.SendTextMessageAsync(message.Chat.Id, "События станут доступны в будущих обновлениях!");
                    break;
                case Commands.info:
                    await client.SendTextMessageAsync(message.Chat.Id, "Бот ещё в разработке! Информация со всеми возможностями MyWishMarketBot будет позднее!)");
                    break;
                case Commands.back:
                    break;
                case Commands.exit: //todo: скорее всего не нужно
                    user.AppMode = AppMode.Default.ToString();
                    await client.SendTextMessageAsync(message.Chat.Id, GetGreeting(message.Chat, false).Result);
                    return;
                default:
                    await client.SendTextMessageAsync(message.Chat.Id, "Я не понял команду :( отправь пожалуйста ещё раз");
                    break;
            }
            efDataLayer.SetAppMode(user);
        }

        async Task Exit(ITelegramBotClient client, Message message)
        {
            mode = AppMode.Default;
            await client.SendTextMessageAsync(message.Chat.Id, GetGreeting(message.Chat, false).Result);
        }

        /// <summary>
        /// Приветствие пользователя
        /// </summary>
        async Task<string> GetGreeting(Chat chat, bool isFirstGreeting)
        {
            User bot = await client.GetMeAsync();
            if (isFirstGreeting)
            {
                return $"Привет, {chat.FirstName} {chat.LastName}! \nМеня зовут {bot.Username}, и я могу работать с твоим список желанных товаров! Вот что я умею:\n {commands}";
            }
            else
            {
                return $"Привет, {chat.FirstName} {chat.LastName}! \nЧем могу быть полезен?\n {commands}";
            }
        }

    }
}
