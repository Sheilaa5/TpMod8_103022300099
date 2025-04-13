using System;
using System.IO;
using System.Text.Json;

public class CovidConfig
{

    private const string ConfigFilePath = "covid_config.json";

    public string satuan_suhu { get; set; }
    public int batas_hari_deman { get; set; }
    public string pesan_ditolak { get; set; }
    public string pesan_diterima { get; set; }


    public CovidConfig()
    {
        SetDefaultValues();
    }


    private void SetDefaultValues()
    {
        this.satuan_suhu = "celcius";
        this.batas_hari_deman = 14;
        this.pesan_ditolak = "Anda tidak diperbolehkan masuk ke dalam gedung ini";
        this.pesan_diterima = "Anda dipersilahkan untuk masuk ke dalam gedung ini";
    }


    public static CovidConfig LoadOrDefault()
    {
        CovidConfig config;

        if (File.Exists(ConfigFilePath))
        {
            try
            {
                string json = File.ReadAllText(ConfigFilePath);
                config = JsonSerializer.Deserialize<CovidConfig>(json);
                if (config == null)
                {
                    Console.WriteLine($"Peringatan: File '{ConfigFilePath}' tidak valid atau kosong. Menggunakan konfigurasi default.");
                    config = new CovidConfig();
                    config.Save();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saat membaca atau deserialisasi '{ConfigFilePath}': {ex.Message}. Menggunakan konfigurasi default.");
                config = new CovidConfig();
                config.Save();
            }
        }
        else
        {
            Console.WriteLine($"File konfigurasi '{ConfigFilePath}' tidak ditemukan. Membuat file dengan nilai default.");
            config = new CovidConfig();
            config.Save();
        }

        return config;
    }


    public void Save()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(ConfigFilePath, json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saat menyimpan konfigurasi ke '{ConfigFilePath}': {ex.Message}");
        }
    }

    public void UbahSatuan()
    {
        if (satuan_suhu.ToLower() == "celcius")
        {
            satuan_suhu = "fahrenheit";
        }
        else
        {
            satuan_suhu = "celcius";
        }
        Save();
    }
}

class Program
{
    static void Main(string[] args)
    {
        CovidConfig config = CovidConfig.LoadOrDefault();

        config.UbahSatuan();
        Console.WriteLine($"Satuan suhu sekarang adalah: {config.satuan_suhu}");

        double suhu = 0;
        int hariDemam = 0;
        bool inputSuhuValid = false;
        bool inputHariDemamValid = false;

        while (!inputSuhuValid)
        {
            Console.Write($"Berapa suhu badan anda saat ini? Dalam nilai {config.satuan_suhu}: ");
            if (double.TryParse(Console.ReadLine(), out suhu))
            {
                inputSuhuValid = true;
            }
            else
            {
                Console.WriteLine("Input suhu tidak valid. Harap masukkan angka.");
            }
        }

        while (!inputHariDemamValid)
        {
            Console.Write("Berapa hari yang lalu (perkiraan) anda terakhir memiliki gejala demam? ");
            if (int.TryParse(Console.ReadLine(), out hariDemam))
            {
                inputHariDemamValid = true;
            }
            else
            {
                Console.WriteLine("Input hari demam tidak valid. Harap masukkan angka.");
            }
        }


        bool suhuNormal = false;

        if (config.satuan_suhu.ToLower() == "celcius")
        {
            suhuNormal = suhu >= 36.5 && suhu <= 37.5;
        }
        else if (config.satuan_suhu.ToLower() == "fahrenheit")
        {
            suhuNormal = suhu >= 97.7 && suhu <= 99.5;
        }

        bool hariAman = hariDemam < config.batas_hari_deman;


        if (suhuNormal && hariAman)
        {
            Console.WriteLine(config.pesan_diterima);
        }
        else
        {
            Console.WriteLine(config.pesan_ditolak);
        }

        Console.WriteLine("\nTekan tombol apa saja untuk keluar...");
        Console.ReadKey();
    }
}