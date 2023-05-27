using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

public static class Hash
{
    /// <summary>
    /// Compute a SHA1 Hash, using the key and the text provided.
    /// </summary>
    /// <param name="secretKey"></param>
    /// <param name="textToHash"></param>
    /// <returns></returns>
    public static string ComputeHash(string secretKey, string textToHash)
    {
        byte[] secret = Encoding.UTF8.GetBytes(secretKey);
        var hasher = new HMACSHA1(secret);

        byte[] textBytes = Encoding.UTF8.GetBytes(textToHash);

        return ToHex(hasher.ComputeHash(textBytes));
    }

    /// <summary>
    /// Converts a <see cref="T:byte[]"/> to a hex-encoded string.
    /// </summary>
    public static string ToHex(byte[] data)
    {
        if (data == null)
        {
            return string.Empty;
        }

        char[] content = new char[data.Length * 2];
        int output = 0;
        byte d;
        for (int input = 0; input < data.Length; input++)
        {
            d = data[input];
            content[output++] = HexLookup[d / 0x10];
            content[output++] = HexLookup[d % 0x10];
        }
        return new string(content);
    }

    private static readonly char[]
        HexLookup =
            new char[] {
                '0',
                '1',
                '2',
                '3',
                '4',
                '5',
                '6',
                '7',
                '8',
                '9',
                'A',
                'B',
                'C',
                'D',
                'E',
                'F'
            };
}

public class FbUpdateToken
{
    public string Token { get; set; }
    public string FbUserId { get; set; }
    public string UserId { get; set; }
}

public class FbUpdateObject
{
    public ObjectEnum Object { get; set; }
    public Entry[] Entry { get; set; }
    public string Json { get; set; }
}

public class Entry
{
    public string Id { get; set; }

    public string Time { get; set; }

    public Change[] Changes { get; set; }
    public Messaging[] Messaging { get; set; }
}

public class UserInfo {
    public string Id { get; set; }
    public string Name { get; set; }

}

public class Messaging
{
    public UserInfo Sender { get; set; }
    public UserInfo Recipient { get; set; }

    public string Timestamp { get; set; }
    public FbMessage Message { get; set; }
}

public class FbMessage
{
    [JsonProperty("mid")]
    public string MessageId { get; set; }
    public string Text { get; set; }
}

public class Change
{
    public string Field { get; set; }

    public Value Value { get; set; }
}

public class Value
{
    public UserInfo From { get; set; }

    public string Item { get; set; }

    public string Verb { get; set; }

    [JsonProperty("comment_id")]
    public string CommentId { get; set; }

    [JsonProperty("post_id")]
    public string PostId { get; set; }

    [JsonProperty("parent_id")]
    public string ParentId { get; set; }

    [JsonProperty("created_time")]
    public string CreatedTime { get; set; }

    public string Message { get; set; }
}

public enum ObjectEnum
{
    Unknown,
    User,
    Page,
    Permissions,
    Payments
}
