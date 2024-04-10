using PhEngine.Core.JSON;

namespace PhEngine.Network
{
    public static class JWT
    {
        public static JSONObject Decrypt(string jwtString)
        {
            var parts = jwtString.Split('.');
            var decode = parts[1];
            var padLength = 4 - decode.Length % 4;
            if (padLength < 4)
                decode += new string('=', padLength);

            var bytes = System.Convert.FromBase64String(decode);
            var userInfo = System.Text.Encoding.ASCII.GetString(bytes);
            return new JSONObject(userInfo);
        }
    }
}