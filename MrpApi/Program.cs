using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/recommendations", async () =>
{
    var apiKey = "MBKAxs2AzQgcVHb78UcF1fpc8dTFoFHG";
    var baseUrl = "https://sandbox.api.sap.com/s4hanacloud/sap/opu/odata/sap/API_MRP_MATERIALS_SRV_01/";
    var requestURL = "A_MRPMaterial?$top=100&$format=json";

    var handler = new HttpClientHandler { AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate };
    using HttpClient client = new(handler);

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.Add("APIKey", apiKey);

    var response = await client.GetAsync(requestURL);
    var data = new List<object>();

    if (response.IsSuccessStatusCode)
    {
        string responseBody = await response.Content.ReadAsStringAsync();
        JObject json = JObject.Parse(responseBody);
        var results = json["d"]?["results"];

        if (results != null)
        {
            foreach (var item in results)
            {
                string? material = item["Material"]?.ToString();
                string? materialType = item["MaterialType"]?.ToString();
                string? procurementType = item["ProcurementType"]?.ToString();
                string? forecastIndicator = item["ForecastIndicator"]?.ToString();
                string? plant = item["PlantName"]?.ToString();
                string? mrpType = item["MRPType"]?.ToString();

                string recommended = RecommendMRPType(materialType, procurementType, forecastIndicator);

                data.Add(new
                {
                    material,
                    materialType,
                    procurementType,
                    forecastIndicator,
                    plant,
                    currentMrp = mrpType,
                    recommendedMrp = recommended
                });
            }
        }
    }

    return Results.Ok(data);
});

app.Run();

string RecommendMRPType(string? materialType, string? procurementType, string? forecastIndicator)
{
    if (procurementType == "F" && materialType == "ROH") return "PD";
    if (procurementType == "E" && materialType == "HALB") return "P1";
    if (forecastIndicator == "X") return "VV";
    return "ND";
}
