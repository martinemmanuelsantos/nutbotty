using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace nutbotty.APIs
{
    static class Pastebin
    {

        public static string api_dev_key = "7ba01119da01fcceea6b7cf715c3391a";
        public static string api_user_name = "nutella4eva";
        public static string api_user_password = "0F7xR3aAemzXbAOQ2z4R";
        public static string api_user_key;
        public static string response_string;

        public static async Task<string> Post(string title, string text_content)
        {

            return await Task.Run(async () =>
            {

                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                {
                   { "api_dev_key", api_dev_key },
                   { "api_user_name", api_user_name },
                   { "api_user_password", api_user_password }
                };

                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync("http://pastebin.com/api/api_login.php", content);

                    api_user_key = await response.Content.ReadAsStringAsync();

                }

                using (var client = new HttpClient())
                {
                    var values = new Dictionary<string, string>
                {
                   { "api_dev_key", api_dev_key },
                   { "api_user_key", api_user_key },
                   { "api_option", "paste" },
                   { "api_paste_name", title },
                   { "api_paste_code", text_content }
                };

                    var content = new FormUrlEncodedContent(values);

                    var response = await client.PostAsync("http://pastebin.com/api/api_post.php", content);

                    response_string = await response.Content.ReadAsStringAsync();

                }

                return response_string;

            });

        }

    }
}