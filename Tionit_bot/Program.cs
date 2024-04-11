using Telegram.Bot.Exceptions;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Polling;
using System;
using System.Linq;
using Tionit_bot;
using Telegram.Bot.Requests;
using System.Threading;
class Program
{
    private static ITelegramBotClient _botClient;
    // Это объект с настройками работы бота. Здесь мы будем указывать, какие типы Update мы будем получать, Timeout бота и так далее.
    private static ReceiverOptions _receiverOptions;
    private static List<Info> Chats = new();
    private static ChatId OperatorsGroupID = -1002087832593;
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
        
        //получаем некую базу данных
       // Chats = Repository.GetChats();
        Console.WriteLine($"{me.FirstName} запущен!");

        await Task.Delay(-1); // Устанавливаем бесконечную задержку, чтобы наш бот работал постоянно
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
                        Message? message = update.Message;
                        if (message == null) throw new ArgumentException("Message is null");

                        User? user = message.From;
                        if(user==null) throw new ArgumentNullException("User is null");
                        // Chat - содержит всю информацию о чате
                        Chat chat = message.Chat;
                        if (chat.Id==OperatorsGroupID)
                            //сообщение сообщение от Оператора
                        {
                            ChatId? userChatId = FindInfodByThread(message.MessageThreadId).ChatID;
                            await botClient.SendTextMessageAsync(
                                userChatId,
                                message.Text??"Message text was Null"
                                );
                        }
                        else 
                        //сообщение от Пользователя
                        {
                            if (user.Username == null) { Console.WriteLine("Пользователь без имени пользователя написал сообщение"); return; }
                            Info? UserInfo = FindInfoByUsername(user.Username);
                            if (UserInfo==null)
                            //создаем топик для нового пользователя
                            {
                                Task<ForumTopic> topic= botClient.CreateForumTopicAsync(OperatorsGroupID, user.Username);
                                ForumTopic forumTopic = topic.Result;
                                //а так можно делать??
                                UserInfo = new Info()
                                {
                                    Username = user.Username,
                                    ThreadID = forumTopic.MessageThreadId,
                                    ChatID = (int)chat.Id
                                };
                                Chats.Add(UserInfo);
                                Console.WriteLine("Topic Created: chatid: "+ UserInfo.ChatID + "   " + UserInfo.ThreadID + "   " + UserInfo.Username);
                                //Repository.SaveChats(Chats);
                            }
                            //сообщение пользователю
                            await botClient.SendTextMessageAsync(
                                chat.Id,
                                "Запрос получен, ожидаем ответа оператора" 
                                );
                            //сообщение Оператору
                            await botClient.ForwardMessageAsync(
                                 OperatorsGroupID,
                                 chat.Id,
                                 message.MessageId,
                                 UserInfo.ThreadID
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
    private static Info? FindInfoByUsername(string username)
    {
        foreach (Info pair in Chats)
        {
            if (pair.Username == username)
            {
                return pair;
            }
        }
        return null;

    }
    private static Info? FindInfodByThread(int? threadId)
    {
        if( !threadId.HasValue )return null;
        foreach (Info chat in Chats)
        {
            if (chat.ThreadID == threadId)
            {
                return chat;
            }
        }
        return null;

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
