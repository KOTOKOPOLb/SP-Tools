using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace SPTools
{
    public class SPTool
    {
        /// <summary>
        /// Получает баланс карты.
        /// </summary>
        /// <param name="id">ID Карты.</param>
        /// <param name="token">Токен карты.</param>
        public static async Task<string> GetBalance(string id, string token)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Convert.ToBase64String(Encoding.UTF8.GetBytes(id + ":" + token)));
                HttpResponseMessage response = await client.GetAsync("https://spworlds.ru/api/public/card");
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonObject = JsonConvert.DeserializeObject(responseBody);
                    return jsonObject.balance;
                }
                else
                {
                    if (response.StatusCode.ToString() == "Unauthorized")
                        return "Error in ID or Card Token";
                    else
                        return "Error: " + response.StatusCode.ToString();
                }
            }
        }

        /// <summary>
        /// Перевод с одной карты на другую.
        /// </summary>
        /// <param name="id">ID Карты.</param>
        /// <param name="token">Токен карты.</param>
        /// <param name="card">Номер карты получателя.</param>
        /// <param name="cound">Колличество АРов.</param>
        /// <param name="comment">Комментарий.</param>
        public static async Task<string> Transaction(string id, string token, int card, int cound, string comment = "-")
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Convert.ToBase64String(Encoding.UTF8.GetBytes(id + ":" + token)));
                StringContent content = new StringContent("{\"receiver\":" + " \"" + card + "\"," + " \"amount\": " + cound + ", " + "\"comment\":" + " \"" + comment + "\"}", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("https://spworlds.ru/api/public/transactions", content);
                if (response.IsSuccessStatusCode)
                    return "OK";
                else
                {
                    if (response.StatusCode.ToString() == "Unauthorized")
                        return "Error in ID or Card Token";
                    else
                        return "Error: " + response.StatusCode.ToString();
                }
            }
        }

        /// <summary>
        /// Создаёт ссылку для оплаты.
        /// </summary>
        /// <param name="cound">Колличество АРов.</param>
        /// <param name="redirectUrl">URL страницы, на которую попадет пользователь после оплаты.</param>
        /// <param name="webhookUrl">URL, куда наш сервер направит запрос, чтобы оповестить ваш сервер об успешной оплате.</param>
        /// <param name="data">Строка до 100 символов, сюда можно поместить любые полезные данных.</param>
        public static async Task<string> Payment(int cound, string redirectUrl, string webhookUrl, string data)
        {
            using (HttpClient client = new HttpClient())
            {
                StringContent content = new StringContent("{\"cound\":" + " \"" + cound + "\"," + " \"amount\": " + cound + ", " + "\"redirectUrl\":" + " \"" + redirectUrl + "\", " + "\"webhookUrl\":" + "\"" + webhookUrl + "\", " + "\"data\":" + " \"" + data + "\",", Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync("https://spworlds.ru/api/public/payment", content);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonObject = JsonConvert.DeserializeObject(responseBody);
                    return jsonObject.url;
                }
                else
                    return "Error: " + response.StatusCode.ToString();
            }
        }

        /// <summary>
        /// Получает ник игрока по Discord ID.
        /// </summary>
        /// <param name="id">ID Карты.</param>
        /// <param name="token">Токен карты.</param>
        /// <param name="DiscordID">Discord ID игрока.</param>
        public static async Task<string> GetUserName(string id, string token, long DiscordID)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Convert.ToBase64String(Encoding.UTF8.GetBytes(id + ":" + token)));
                HttpResponseMessage response = await client.GetAsync("https://spworlds.ru/api/public/users/" + DiscordID.ToString());
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    dynamic jsonObject = JsonConvert.DeserializeObject(responseBody);
                    return jsonObject.username;
                }
                else
                {
                    if (response.StatusCode.ToString() == "NotFound")
                        return "User not found";
                    else
                        return "Error: " + response.StatusCode.ToString();
                }
            }
        }
    }
}