using Ork.Utils;
using System.Text.Json;

public class Database
{
    public async static Task<List<DatabaseLevelEntry>> GetLevels(string query)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{Secrets.GetSecrets().databaseUrl}/rest/v1/level_metadata?select={query}"),
            Headers =
            {
                { "apiKey", $"{Secrets.GetSecrets().databaseKey}" },
                { "Authorization", $"Bearer {Secrets.GetSecrets().databaseKey}" }
            },
        };

        HttpResponseMessage message = await client.SendAsync(request);
        string response = await message.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<DatabaseLevelEntry>>(response)!;
    }

    public async static Task<DatabaseUserEntry> GetUser(string uuid)
    {
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{Secrets.GetSecrets().databaseUrl}/rest/v1/users??select=*&user_id=eq.{uuid}"),
            Headers =
            {
                { "apiKey", $"{Secrets.GetSecrets().databaseKey}" },
                { "Authorization", $"Bearer {Secrets.GetSecrets().databaseKey}" }
            },
        };

        HttpResponseMessage message = await client.SendAsync(request);
        string response = await message.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<DatabaseUserEntry>>(response)![0];
    }
}

public class DatabaseUserEntry
{
    public DateTime created_at { get; set; }
    public string user_id { get; set; }
    public string username { get; set; }
}


public class DatabaseLevelEntry
{
    public DateTime created_at { get; set; }
    public string name { get; set; }
    public string path { get; set; }
    public string level_id { get; set; }
    public string author { get; set; }

    public async Task<DatabaseUserEntry> GetAuthor()
    {
        return await Database.GetUser(author);
    }
}