using Telegram.Bot;
using Telegram.Bot.Types;

namespace Telegram;

public class Program
{
    static Task Main(string[] args)
    {
        var token = Environment.GetEnvironmentVariable("key");
        var bot = new TelegramBotClient(token);

        bot.ConfigureBotCommands();

        bot.StartReceiving(UpdateHandler, PollingErrorHandler);

        Console.ReadLine();

        return Task.CompletedTask;
    }

    private static Task PollingErrorHandler(ITelegramBotClient bot, Exception e, CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }

    private static async Task UpdateHandler(ITelegramBotClient bot, Update update, CancellationToken cancellation)
    {
        try
        {
            if (update.Message is not { } message)
                return;

            if (message.Text is not { } messageText)
                return;

            var chatId = message.Chat.Id;

            if (messageText.StartsWith('/'))
            {
                var command = messageText.Substring(1, 5);

                switch (command)
                {
                    case "start":
                        {
                            await bot.WelcomeClient(chatId);

                            break;
                        }

                    case "busca":
                        {
                            try
                            {
                                string cep = messageText.Substring(8).Trim();

                                bot.SearchCepAsync(chatId, cep);

                            }
                            catch
                            {
                                bot.SendCepErrorMessage(chatId);
                            }

                            break;
                        }

                    case "ajuda":
                        {
                            await bot.SendHelpMessageAsync(chatId);

                            break;
                        }
                }
            }

        }
        catch
        {
            if (update.Message?.Chat.Id != null)
                await bot.SendTextMessageAsync(update.Message.Chat.Id,
                    "Descupe, ocorreu um erro interno.Tente novamente mais tarde!", cancellationToken: cancellation);
        }
    }
}