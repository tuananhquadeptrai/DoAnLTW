using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
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

        var inputData = new SortedDictionary<string, string>
    {
        {"vnp_Version", "2.1.0"},
        {"vnp_Command", "pay"},
        {"vnp_TmnCode", vnp_TmnCode },
        {"vnp_Amount", (amount * 100).ToString() },
        {"vnp_CurrCode", "VND" },
        {"vnp_TxnRef", orderId },
        {"vnp_OrderInfo", orderInfo },
        {"vnp_OrderType", "billpayment" },
        {"vnp_Locale", "vn" },
        {"vnp_ReturnUrl", vnp_ReturnUrl },
        {"vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss") },
        {"vnp_IpAddr", clientIp }
    };

        // Build hash data (raw value)
        var hashData = new StringBuilder();
        foreach (var kv in inputData)
        {
            if (hashData.Length > 0) hashData.Append('&');
            hashData.Append(kv.Key + "=" + kv.Value); // KHÔNG ENCODE
        }

        string vnp_SecureHash = HmacSHA512(vnp_HashSecret, hashData.ToString());

        // Build query string (url encode)
        var query = new StringBuilder();
        foreach (var kv in inputData)
        {
            if (query.Length > 0) query.Append('&');
            query.Append(kv.Key + "=" + HttpUtility.UrlEncode(kv.Value));
        }

        string paymentUrl = vnp_Url + "?" + query + "&vnp_SecureHash=" + vnp_SecureHash;
        return paymentUrl;

    }


    private string HmacSHA512(string key, string input)
    {
        var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
        byte[] hashValue = hmac.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hashValue).Replace("-", "").ToLower();
    }

}
