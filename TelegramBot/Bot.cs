using System;
using System.Diagnostics;
using System.Speech.Synthesis;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using System.Media;

namespace TelegramBot
{
    internal static class Bot
    {
        private static readonly string botToken = Keys.telegApiToken;

        private static async Task Main()
        {
            TelegramBotClient botClient = new TelegramBotClient(botToken);

            var me = await botClient.GetMeAsync();
            Console.WriteLine(
                $"Hello, World! I am user {me.Id} and my name is {me.FirstName}."
            );

            var cts = new CancellationTokenSource();

            // StartReceiving does not block the caller thread. Receiving is done on the ThreadPool.
            botClient.StartReceiving(
                new DefaultUpdateHandler(HandleUpdateAsync, HandleErrorAsync),
                cts.Token);

            Console.WriteLine($"Start listening for @{me.Username}");
            Console.ReadLine();
        }

        private static Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(errorMessage);
            return Task.CompletedTask;
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            if (update.Type != UpdateType.Message)
                return;
            if (update.Message.Type != MessageType.Text)
                return;

            var chatId = update.Message.Chat.Id;

            var senderName = update.Message.From.FirstName + " " + update.Message.From.LastName;

            Console.WriteLine($"Received a '{update.Message.Text}' message in chat {chatId} by {senderName}");

            await botClient.SendTextMessageAsync(
                chatId: chatId,
                text: "You said:\n" + update.Message.Text, cancellationToken: cancellationToken);

            string message = update.Message.Text;

            if (message.Contains("Открой папку"))
            {
                Process.Start("explorer", "E:\\" + message.Substring(13, message.Length - 13));
                message = "Открываю";
            }

            var synthesizer = new SpeechSynthesizer();
            synthesizer.SetOutputToDefaultAudioDevice();
            synthesizer.Speak(message);
        }
    }
}