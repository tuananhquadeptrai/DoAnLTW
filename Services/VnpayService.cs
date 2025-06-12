using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;
using System.Web;

public class VnpayService
{
    private readonly IConfiguration _config;
    public VnpayService(IConfiguration config)
    {
        _config = config;
    }

    public string CreatePaymentUrl(long amount, string orderId, string orderInfo, string clientIp)
    {
        var config = _config.GetSection("VNPAY");
        var vnp_Url = config["Url"];
        var vnp_ReturnUrl = config["ReturnUrl"];
        var vnp_TmnCode = config["TmnCode"];
        var vnp_HashSecret = config["HashSecret"];

        var vnp_Version = "2.1.0";
        var vnp_Command = "pay";
        var vnp_CurrCode = "VND";
        var vnp_TxnRef = orderId;
        var vnp_OrderInfo = orderInfo;
        var vnp_OrderType = "billpayment";
        var vnp_Amount = (amount * 100).ToString(); // VNPay yêu cầu nhân 100
        var vnp_Locale = "vn";
        var vnp_IpAddr = clientIp;
        if (vnp_IpAddr == "::1") vnp_IpAddr = "127.0.0.1";

        var vnp_CreateDate = DateTime.Now.ToString("yyyyMMddHHmmss");

        var inputData = new SortedList<string, string>
        {
            { "vnp_Version", vnp_Version },
            { "vnp_Command", vnp_Command },
            { "vnp_TmnCode", vnp_TmnCode },
            { "vnp_Amount", vnp_Amount },
            { "vnp_CurrCode", vnp_CurrCode },
            { "vnp_TxnRef", vnp_TxnRef },
            { "vnp_OrderInfo", vnp_OrderInfo },
            { "vnp_OrderType", vnp_OrderType },
            { "vnp_Locale", vnp_Locale },
            { "vnp_ReturnUrl", vnp_ReturnUrl },
            { "vnp_IpAddr", vnp_IpAddr },
            { "vnp_CreateDate", vnp_CreateDate }
        };

        // 1. Build chuỗi ký (KHÔNG encode bất cứ thứ gì)
        var signData = string.Join("&", inputData.Select(kv => kv.Key + "=" + kv.Value));
        var hash = HmacSHA512(signData, vnp_HashSecret);

        // 2. Build query string gửi sang VNPay (cần encode từng key và value)
        var query = new StringBuilder();
        foreach (var item in inputData)
        {
            if (query.Length > 0) query.Append('&');
            query.Append(HttpUtility.UrlEncode(item.Key) + "=" + HttpUtility.UrlEncode(item.Value));
        }
        query.Append("&vnp_SecureHash=" + hash);
        Console.WriteLine("Chuoi ky VNPAY:");
        Console.WriteLine(signData);
        Console.WriteLine("Hash:");
        Console.WriteLine(hash);
        Console.WriteLine("Full URL:");
        Console.WriteLine(vnp_Url + "?" + query.ToString());

        return vnp_Url + "?" + query.ToString();
    }

    private string HmacSHA512(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(dataBytes);
            return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
        }
    }
}
