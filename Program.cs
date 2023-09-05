using amexefevoo.Security.Authentication;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;

public class Program
{
    public enum HttpMethod { POST, DELETE, PUT }
    private static readonly string apiKey = "GPsHKzr5QfCLhEImDfjCBY72qGGPvgks";
    private static readonly string secret = "XtSviAnPzE4Cm6SOA83k91b5lBUTM08Q";
    private static readonly string urlsb = "https://api.qasb.americanexpress.com/sb/merchant/v1/acquisitions/sellers";
    private static readonly string urlqa = "https://apigateway2sma-qa.americanexpress.com/merchant/v1/acquisitions/sellers";
    
    public static void Main(string[] args)
    {
        string orgEnrollPayload = File.ReadAllText(@"C:\\Users\admin\Documents\seller.txt");

        string seller_id = "SELLEREFEVOOPAY";

        string enrollResponse = InvokeResource(urlqa, orgEnrollPayload, HttpMethod.POST);
        Console.WriteLine("ENROLL: " + enrollResponse);

        //string orgUpdatePayload = File.ReadAllText(@"C:\\Users\admin\Documents\seller.txt");

        //string updateResponse = InvokeResource(string.Format("{0}/{1}", url, seller_id), orgUpdatePayload, HttpMethod.PUT);
        //Console.WriteLine("UPDATE: " + updateResponse);

        //string deleteResponse = InvokeResource(string.Format("{0}/{1}", url, seller_id), string.Empty, HttpMethod.DELETE);
        //Console.WriteLine("DELETE: " + deleteResponse);

    }
    private static string InvokeResource(string url, string payload, HttpMethod method)
    {
        string result = null;
        string contentType = "application/json";
        var authProvider = new HmacAuthProvider();
        var headers = authProvider.GenerateAuthHeaders(apiKey, secret, payload, url, method.ToString());


        var httpClientHandler = new HttpClientHandler()
        {
            SslProtocols = SslProtocols.Tls12,
            ClientCertificateOptions = ClientCertificateOption.Manual
        };
        httpClientHandler.ClientCertificates.Add(new X509Certificate2(@"C:\\Users\admin\Downloads\amexqa\b69c65d132592bf7.crt", "8bMrnfnwx2E5duNR"));

        var task = Task.Factory.StartNew(() => {
        using (var client = new HttpClient(httpClientHandler))
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
                HttpResponseMessage response;
                switch (method)
                {
                    case HttpMethod.PUT:
                        response = client.PutAsync(url, new StringContent(payload, Encoding.UTF8, contentType)).Result;
                        break;
                    case HttpMethod.DELETE:
                        response = client.DeleteAsync(url).Result;
                        break;
                    default:
                        response = client.PostAsync(url, new StringContent(payload, Encoding.UTF8, contentType)).Result;
                        break;
                }
                result = string.Format("success = {0}, response = {1}", response.IsSuccessStatusCode, response.Content.ReadAsStringAsync().Result);
            }
        });
        task.Wait();
        return result;
    }
}