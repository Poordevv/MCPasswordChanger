using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp;

namespace MCPasswordChanger
{
    class Program
    {
        static void Main(string[] args)
        {
            List<string> accounts = File.ReadAllLines("accounts.txt").ToList<string>(); 

            var Username = string.Empty;
            var Password = string.Empty;
            var accessToken = (string)null;


            ///////////////////////////////////////////////////////////////////////////////////////////////////////////
            try
            {
                foreach (string account in accounts)
                {
                    Username = account.Split(new string[] { ":" }, StringSplitOptions.None)[0];
                    Password = account.Split(new string[] { ":" }, StringSplitOptions.None)[1];

                    var oldpassword = Password;
                    var newpassword = "Sub To Poordev on Yt"; //New Password here

                    string loginPostdata = "{\"agent\":{\"name\":\"Minecraft\",\"version\":1},\"username\":\"" + Username + "\",\"password\":\"" + Password + "\",\"clientToken\":\"clientidentifier\",\"requestUser\":true}";
                    string pwpostData = "{\"oldPassword\":\"" + oldpassword + "\",\"password\":\"" + newpassword + "\"}";

                    /////////////////////////////////////////////////////////////////////////////////////////////////////
                    
                    var clientbearer = new RestClient("https://authserver.mojang.com/authenticate");
                    var requestbearer = new RestRequest(Method.POST);

                    requestbearer.AddHeader("Content-Type", "application/json");
                    requestbearer.AddParameter("text/xml", loginPostdata, ParameterType.RequestBody);

                    IRestResponse responsebearer = clientbearer.Execute(requestbearer);
                    var contentbearer = responsebearer.Content;

                    if (contentbearer.Contains("accessToken"))
                    {
                        var jsonData = JsonConvert.DeserializeObject<dynamic>(contentbearer);
                        accessToken = jsonData.accessToken;

                        ///////////////////////////////////////////////////////////////////////////////////

                        var clientq = new RestClient("https://api.mojang.com/user/security/challenges");
                        var requestq = new RestRequest(Method.GET);

                        requestq.AddHeader("Authorization", $"Bearer " + accessToken);
                        requestq.AddHeader("Connection", "Keep-Alive");
                        requestq.AddHeader("Content-Type", "application/json");

                        IRestResponse responseq = clientq.Execute(requestq);
                        {
                            var clienttwoo = new RestClient($"https://api.mojang.com/users/password")
                            {
                                Proxy = null
                            };

                            var requesttwoo = new RestRequest(Method.PUT);
                            requesttwoo.AddHeader("Authorization", "Bearer " + accessToken);
                            requesttwoo.AddHeader("Content-Type", "application/json");
                            requesttwoo.AddParameter("text/xml", pwpostData, ParameterType.RequestBody);

                            IRestResponse responsetwoo = clienttwoo.Execute(requesttwoo);
                            {
                                Console.WriteLine("Successfully changed the password.");
                                using (FileStream newpwfile = new FileStream("NewPasswords.txt", FileMode.Append, FileAccess.Write, FileShare.None))
                                {
                                    using (StreamWriter writer = new StreamWriter(newpwfile))
                                    {
                                        writer.WriteLine($"Current Email: {Username} [-] Old Password: {Password} [-] New Password: {newpassword}\n");
                                        newpwfile.Flush();
                                    }

                                }
                            }
                        }
                    }
                    else
                    {
                        if (contentbearer.Contains("Invalid"))
                        {
                            Console.WriteLine("Either your accounts are invalid or your IP is currently temp-banned. Please try again later, use a VPN or input correct accounts.");
                        }
                        else { Console.WriteLine("If you actually see this, I have no clue what happened... "); }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error has occured. Error message: " + e.Message);
            }
            Console.ReadKey();

        }
    }
}
