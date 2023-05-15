using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Numerics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeCalcMaster.Models;


//await ProcessRepositoriesAsync();

//[HttpPost]
//static async Task ProcessRepositoriesAsync()
//{
//    BigInteger value = BigInteger.Pow(2, 3217) - 1;

//    PrimeIn primeIn = new()
//    {
//        index = 0,
//        value = value.ToString(),
//    };

//    PrimeOut primeOut = new PrimeOut();


//    WinHttpHandler handler = new WinHttpHandler
//    {
//        SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
//        TcpKeepAliveEnabled = true
//    };

//    using (var httpClient = new HttpClient(handler))
//    {
//        var jsonString = JsonConvert.SerializeObject(primeIn);
//        var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

//        using (var response = await httpClient.PostAsync("https://localhost:8001/Prime", content))
//        {
//            string apiResponse = await response.Content.ReadAsStringAsync();

//            primeOut = JsonConvert.DeserializeObject<PrimeOut>(apiResponse);
//        }
//    }


//    Console.Write(primeOut.ToString());
//}


class Program
{
    static void Main(string[] args)
    {
        //RunAsync().Wait();

        int[] array = { 1, 2, 3, 4, 5, 6 };

        Parallel.For(0, array.Length, i =>
        {
            array[i] = array[i] * 2;

        });

        for (int i = 0; i < array.Length; i++)
        {
            Console.WriteLine(array[i]);
        }
    }


    static async Task RunAsync()
    {
        BigInteger value = BigInteger.Pow(2, 3217) - 1;

        PrimeIn primeIn = new()
        {
            index = 0,
            value = value.ToString(),
        };

        PrimeOut primeOut = new PrimeOut();

        using (var httpClient = new HttpClient())
        {
            var jsonString = JsonConvert.SerializeObject(primeIn);
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            using (var response = await httpClient.PostAsync("https://localhost:8001/Prime", content))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                primeOut = JsonConvert.DeserializeObject<PrimeOut>(apiResponse);
            }
            Console.WriteLine(primeOut.isPrime.ToString());
        }
    }

}


