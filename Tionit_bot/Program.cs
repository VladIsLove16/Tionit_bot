using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Polling;
using System;
using System.Linq;
using Tionit_bot;
using Telegram.Bot.Requests;
class Program
{
    // Это клиент для работы с Telegram Bot API, который позволяет отправлять сообщения, управлять ботом, подписываться на обновления и многое другое.
    private static ITelegramBotClient _botClient;

    // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
    private static ReceiverOptions _receiverOptions;
    private static List<Info> Chats = new();
    private static Repository Repository=new();
    private static ChatId OperatorsGroup = -1002087832593;
    static async Task Main()
    {

        _botClient = new TelegramBotClient("6606038493:AAFM4KJE2yfJ2jR-l0WXLWbw5_Oab6FLeL8"); // Присваиваем нашей переменной значение, в параметре передаем Token, полученный от BotFather
        _receiverOptions = new ReceiverOptions // Также присваем значение настройкам бота
        {
            AllowedUpdates = new[] // Тут указываем типы получаемых Update`ов, о них подробнее расказано тут https://core.telegram.org/bots/api#update
            {
                UpdateType.Message, // Сообщения (текст, фото/видео, голосовые/видео сообщения и т.д.)
            },
            // Параметр, отвечающий за обработку сообщений, пришедших за то время, когда ваш бот был оффлайн
            // True - не обрабатывать, False (стоит по умолчанию) - обрабаывать
            ThrowPendingUpdates = true,
        };

        using var cts = new CancellationTokenSource();

        // UpdateHander - обработчик приходящих Update`ов
        // ErrorHandler - обработчик ошибок, связанных с Bot API
        _botClient.StartReceiving(UpdateHandler, ErrorHandler, _receiverOptions, cts.Token); // Запускаем бота

        var me = await _botClient.GetMeAsync(); // Создаем переменную, в которую помещаем информацию о нашем боте.
        
       // Chats = Repository.GetChats();
        Console.WriteLine("Список чатов получен");
        Console.WriteLine($"{me.FirstName} запущен!");

        await Task.Delay(-1); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно

        // repostitory
    }
    private static async Task UpdateHandler(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        // Обязательно ставим блок try-catch, чтобы наш бот не "падал" в случае каких-либо ошибок
        try
        {
            // Сразу же ставим конструкцию switch, чтобы обрабатывать приходящие Update
            switch (update.Type)
            {
                case UpdateType.Message:
                    {
                        var message = update.Message;

                        User? user = message.From;
                        // Chat - содержит всю информацию о чате
                        Chat chat = message.Chat;
                        if (chat.Id==OperatorsGroup)
                        //cообщение от оператора
                        {
                            await botClient.SendTextMessageAsync(
                                FindPairInListByThresh((int)message.MessageThreadId).ChatId,
                                message.Text
                                );

                        }
                        else
                        //cообщение от пользователя
                        {
                            bool contains = false;
                            foreach(Info pair in Chats)
                            {
                                if (pair.Username == user.Username)
                                {
                                    contains = true;
                                    break;
                                }
                            }
                            if (!contains)
                            {
                                Task<ForumTopic> topic= botClient.CreateForumTopicAsync(OperatorsGroup, user.Username);
                                ForumTopic forumTopic = topic.Result;
                                Info info = new Info()
                                {
                                    Username = user.Username,
                                    TheshId = forumTopic.MessageThreadId,
                                    ChatId = (int)chat.Id
                                };
                                Chats.Add(info);
                                Console.WriteLine("chatid: "+info.ChatId + "   " + info.TheshId + "   " +info.Username);
                                //Repository.Save(Chats);
                            }
                            //сообщение пользователю
                            await botClient.SendTextMessageAsync(
                                chat.Id,
                                "Запрос получен, ожидаем ответа оператора" // отправляем то, что написал пользователь
                                );  

                            Info Pair = FindPairInList(user.Username);
                            //сообщение в группу Операторов
                            await botClient.ForwardMessageAsync(
                                 OperatorsGroup,
                                 chat.Id,
                                 message.MessageId,
                                 Pair.TheshId
                                 );
                        }
                       
                        return;
                    }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    private static Info FindPairInList(string username)
    {
        foreach (Info pair in Chats)
        {
            if (pair.Username == username)
            {
                return pair;
            }
        }
        return new Info(); ;

    }
    private static Info FindPairInListByThresh(int Theshid)
    {
        foreach (Info pair in Chats)
        {
            if (pair.TheshId == Theshid)
            {
                return pair;
            }
        }
        return new Info(); ;

    }

    private static Task ErrorHandler(ITelegramBotClient botClient, Exception error, CancellationToken cancellationToken)
    {
        // Тут создадим переменную, в которую поместим код ошибки и её сообщение 
        var ErrorMessage = error switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => error.ToString()
        };

        Console.WriteLine(ErrorMessage);
        return Task.CompletedTask;
    }
}
