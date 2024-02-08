using System.Text;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace Telegram;

public static class TelegramUtils 
{
    public static async void ConfigureBotCommands(this TelegramBotClient bot)
    {
        var commands = new List<BotCommand>
        {
            new() { Command = "start", Description = "Inicia a conversa com o bot" },
            new() { Command = "buscar", Description = "Busca a cidade e o estado de um CEP" },
            new() { Command = "ajuda", Description = "Mostra a ajuda sobre o bot" }
        };
        
        await bot.SetMyCommandsAsync(commands);
    }
    
    public static async Task WelcomeClient(this ITelegramBotClient bot, long chatId)
    {
        try
        {
            var message = "Olá, eu sou um bot que busca a cidade e o estado de um CEP. Digite /buscar seguido do CEP para começar. (Exemplo: /buscar 0000-0000).";
            
            await bot.SendTextMessageAsync(chatId, message);
        }
        catch (Exception e)
        {
            var errorMessage = "Desculpe, ocorreu um erro interno. Tente novamente mais tarde!";

            await bot.SendTextMessageAsync(chatId, errorMessage);
        }
    }

    private static string StateDetail(string uf)
    {
        return uf switch
        {
            "AC" => "Acre",
            "AL" => "Alagoas",
            "AM" => "Amazonas",
            "AP" => "Amapá",
            "BA" => "Bahia",
            "CE" => "Ceará",
            "DF" => "Distrito Federal",
            "ES" => "Espírito Santo",
            "GO" => "Goiás",
            "MA" => "Maranhão",
            "MG" => "Minas Gerais",
            "MS" => "Mato Grosso do Sul",
            "MT" => "Mato Grosso",
            "PA" => "Pará",
            "PB" => "Paraíba",
            "PE" => "Pernambuco",
            "PI" => "Piauí",
            "PR" => "Paraná",
            "RJ" => "Rio de Janeiro",
            "RN" => "Rio Grande do Norte",
            "RO" => "Rondônia",
            "RR" => "Roraima",
            "RS" => "Rio Grande do Sul",
            "SC" => "Santa Catarina",
            "SE" => "Sergipe",
            "SP" => "São Paulo",
            "TO" => "Tocantins",
            _ => ""
        };
    }

    private static async void SendCepDetails(ITelegramBotClient bot, Endereco endereco, long chatId)
    {
        var stringBuilder = new StringBuilder(64);
        
        stringBuilder.AppendLine($"CEP - {endereco.Cep}");
        stringBuilder.AppendLine($"Cidade - {endereco.Localidade}");
        stringBuilder.AppendLine($"Estado - {StateDetail(endereco.Uf)}");
        stringBuilder.AppendLine($"Rua - {endereco.Logradouro}");
        stringBuilder.AppendLine($"Bairro - {endereco.Bairro}");

        var message = stringBuilder.ToString();

        await bot.SendTextMessageAsync(chatId, message);
    }

    public static async void SendCepErrorMessage(this ITelegramBotClient bot, long chatId)
    {
        await bot.SendTextMessageAsync(chatId, "CEP inválido, por favor, tente novamente!");
    }
    public static void SearchCepAsync(this ITelegramBotClient bot, long chatId, string cep)
    {
        try
        {
            var url = $"https://viacep.com.br/ws/{cep}/json/";
            
            var httpClient = new HttpClient();
            
            if (httpClient.GetAsync(url).Result.IsSuccessStatusCode)
            {
                var result = httpClient.GetStringAsync(url).Result;
                
                var endereco = JsonConvert.DeserializeObject<Endereco>(result);
                
                SendCepDetails(bot, endereco, chatId);
            }
        }
        
        catch
        {
            bot.SendCepErrorMessage(chatId);
        }
    }

    public static async Task SendHelpMessageAsync(this ITelegramBotClient bot, long chatId)
    {
        const string message = "Eu sou um bot que busca todos os detalhes de um CEP. Você pode usar os seguintes comandos:\n/start - Inicia a conversa com o bot\n/buscar - Busca a cidade e o estado de um CEP\n/ajuda - Mostra a ajuda sobre o bot. \nDigite /buscar seguido do CEP para começar. (Exemplo: /buscar 0000-0000).";

        await bot.SendTextMessageAsync(chatId, message);
    }
}