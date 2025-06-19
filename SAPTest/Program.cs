using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

class Program
{
    private static readonly string apiKey = "MBKAxs2AzQgcVHb78UcF1fpc8dTFoFHG";
    private static readonly string baseUrl = "https://sandbox.api.sap.com/s4hanacloud/sap/opu/odata/sap/API_MRP_MATERIALS_SRV_01/";

    static async Task Main()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
        };

        using HttpClient client = new(handler);

        client.BaseAddress = new Uri(baseUrl);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        client.DefaultRequestHeaders.Add("APIKey", apiKey);
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
        client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

        string requestURL = "A_MRPMaterial?$top=1000&$format=json";

        HttpResponseMessage response = await client.GetAsync(requestURL);
        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();

            JObject jsonResponse = JObject.Parse(responseBody);
            JToken? resultsToken = jsonResponse["d"]?["results"];
            var result = resultsToken != null && resultsToken.HasValues ? resultsToken.First : null;


            string RecommendMRPType(string? materialType, string? procurementType, string? forecastIndicator)
            {
                if (procurementType == "F" && materialType == "ROH")
                {
                    return "PD"; // Raw material, externally procured
                }
                if (procurementType == "E" && materialType == "HALB")
                {
                    return "P1"; // In-house semi-finished
                }
                if (forecastIndicator == "X")
                {
                    return "VV"; // Forecast-based planning
                }

                return "ND"; // No planning
            }


            Console.WriteLine("Response received successfully:");
            if (resultsToken != null && resultsToken.HasValues)
            {
                foreach (var item in resultsToken)
                {
                    string? mrpType = item["MRPType"]?.ToString();
                    string? plant = item["PlantName"]?.ToString();
                    string? material = item["Material"]?.ToString();
                    string? procurementType = item["ProcurementType"]?.ToString();
                    string? forecastIndicator = item["ForecastIndicator"]?.ToString();
                    string? materialType = item["MaterialType"]?.ToString();

                    Console.WriteLine($"MRP Type (DISMM): {mrpType}");
                    Console.WriteLine($"Plant: {plant}");
                    Console.WriteLine($"Material: {material}");

                    string recommendedMRP = RecommendMRPType(materialType, procurementType, forecastIndicator);
                    Console.WriteLine($"Recommended MRP Type: {recommendedMRP}\n");
                }
            }
            
            //Console.WriteLine(responseBody);
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
            string errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Error details: {errorContent}");
        }
    }
}
