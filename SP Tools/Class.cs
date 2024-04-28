using Newtonsoft.Json;
using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace SPTools
{
    public class SPTool
    {
        private static HttpClient GetClient(string cardid, string token)
        {
            if (string.IsNullOrEmpty(cardid) || string.IsNullOrEmpty(token))
            {
                throw new ArgumentException("Card ID and Token cannot be empty");
            }

            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Convert.ToBase64String(Encoding.UTF8.GetBytes(cardid + ":" + token)));
            return client;
        }

        /// <summary>
        /// Получает баланс карты.
        /// </summary>
        /// <param name="cardid">ID Карты.</param>
        /// <param name="token">Токен карты.</param>
        public static async Task<string> GetBalance(string cardid, string token)
        {
            try
            {
                using (HttpClient client = GetClient(cardid, token))
                {
                    HttpResponseMessage response = await client.GetAsync("https://spworlds.ru/api/public/card");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var jsonObject = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    return jsonObject.balance;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error: " + ex.StatusCode);
            }
        }

        /// <summary>
        /// Перевод с одной карты на другую.
        /// </summary>
        /// <param name="cardid">ID Карты.</param>
        /// <param name="token">Токен карты.</param>
        /// <param name="card">Номер карты получателя.</param>
        /// <param name="cound">Колличество АРов.</param>
        /// <param name="comment">Комментарий.</param>
        public static async Task<string> Transaction(string cardid, string token, int card, int count, string comment = "Нет комментария")
        {
            try
            {
                using (HttpClient client = GetClient(cardid, token))
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(new { receiver = card, amount = count, comment }), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("https://spworlds.ru/api/public/transactions", content);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonObject = JsonConvert.DeserializeObject(responseBody);
                    return jsonObject.balance;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error: " + ex.StatusCode);
            }
        }

        /// <summary>
        /// Создаёт ссылку для оплаты.
        /// </summary>
        /// <param name="cardid">ID Карты на которую придут АРы.</param>
        /// <param name="token">Токен карты на которую придут АРы.</param>
        /// <param name="cound">Колличество АРов.</param>
        /// <param name="redirectUrl">URL страницы, на которую попадет пользователь после оплаты.</param>
        /// <param name="webhookUrl">URL, куда сервер направит запрос, чтобы оповестить ваш сервер об успешной оплате.</param>
        /// <param name="data">Строка до 100 символов, сюда можно поместить любые полезные данных.</param>
        public static async Task<string> Payment(string cardid, string token, List<Item> items, string redirectUrl, string webhookUrl, string data)
        {
            try
            {
                using (HttpClient client = GetClient(cardid, token))
                {
                    dynamic requestBody = new
                    {
                        items,
                        redirectUrl,
                        webhookUrl,
                        data
                    };

                    StringContent content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("https://spworlds.ru/api/public/payments", content);

                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonObject = JsonConvert.DeserializeObject(responseBody);
                    return jsonObject.url;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error: " + ex.StatusCode);
            }
        }

        public class Item
        {
            public string name { get; set; }
            public int count { get; set; }
            public int price { get; set; }
            public string comment { get; set; }
        }

        /// <summary>
        /// Получает ник игрока по ID в Discord.
        /// </summary>
        /// <param name="cardid">ID Карты.</param>
        /// <param name="token">Токен карты.</param>
        /// <param name="DiscordId">ID пользователя в Discord.</param>
        public static async Task<(string nickname, string uuid)> GetUserName(string cardid, string token, long DiscordId)
        {
            using (HttpClient client = GetClient(cardid, token))
            {
                HttpResponseMessage response = await client.GetAsync("https://spworlds.ru/api/public/users/" + DiscordId.ToString());
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonObject = JsonConvert.DeserializeObject(responseBody);
                    string username = jsonObject.username;
                    string uuid = jsonObject.uuid;
                    return (username, uuid);
                }
                else
                {
                    if (response.StatusCode == HttpStatusCode.NotFound)
                         throw new Exception("Error: User not found");
                    else
                        throw new Exception("Error: " + response.StatusCode);
                }
            }
        }

        public class Card
        {
            public string Name { get; set; }
            public string ID { get; set; }
        }

        /// <summary>
        /// Получение карт игрока.
        /// </summary>
        /// <param name="cardid">ID Карты.</param>
        /// <param name="token">Токен карты.</param>
        /// <param name="nickname">Ник пользователя.</param>
        public static async Task<List<Card>> GetUserCards(string cardid, string token, string nickname)
        {
            try
            {
                using (HttpClient client = GetClient(cardid, token))
                {
                    HttpResponseMessage response = await client.GetAsync($"https://spworlds.ru/api/public/accounts/{nickname}/cards");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonObject = JsonConvert.DeserializeObject(responseBody);

                    List<Card> cards = new List<Card>();

                    foreach (var card in jsonObject)
                    {
                        Card newCard = new Card
                        {
                            Name = card.name,
                            ID = card.number
                        };

                        cards.Add(newCard);
                    }

                    return cards;
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error: " + ex.StatusCode);
            }
        }

        public class Account
        {
            public string id { get; set; }
            public string username { get; set; }
            public string minecraftUUID { get; set; }
            public string status { get; set; }
            public List<string> roles { get; set; }
            public City city { get; set; }
            public List<Cards> cards { get; set; }
            public string createdAt { get; set; }
        }

        public class City
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public int x { get; set; }
            public int z { get; set; }
            public bool isMayor { get; set; }
        }

        public class Cards
        {
            public string id { get; set; }
            public string name { get; set; }
            public string number { get; set; }
            public int color { get; set; }
        }

        /// <summary>
        /// Информация по токену.
        /// </summary>
        /// <param name="cardid">ID Карты.</param>
        /// <param name="token">Токен карты.</param>
        public static async Task<Account> GetInfo(string cardid, string token)
        {
            try
            {
                using (HttpClient client = GetClient(cardid, token))
                {
                    HttpResponseMessage response = await client.GetAsync("https://spworlds.ru/api/public/accounts/me");
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Account>(responseBody);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error: " + ex.StatusCode);
            }
        }

        /// <summary>
        /// Изменение вебхука карты.
        /// </summary>
        /// <param name="cardid">ID Карты.</param>
        /// <param name="token">Токен карты.</param>
        /// <param name="url">Адрес, куда отправлять webhook.</param>
        public static async Task<(string id, string webhook)> ChangeWebhook(string cardid, string token, string url)
        {
            try
            {
                using (HttpClient client = GetClient(cardid, token))
                {
                    StringContent content = new StringContent(JsonConvert.SerializeObject(new { url }), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await client.PostAsync("https://spworlds.ru/api/public/card/webhook", content);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonObject = JsonConvert.DeserializeObject(responseBody);
                    return (jsonObject.id, jsonObject.webhook);
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Error: " + ex.StatusCode);
            }
        }
    }
}
