using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices.Sensors;
using Microsoft.Maui.Storage;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using Client = Supabase.Client;
using System.Text.Json;
using System.IO;
using Microsoft.Maui.Controls;


namespace App;

public partial class ViewModel : ObservableObject, INotifyPropertyChanged
{
    [ObservableProperty]
    public string title;

    [ObservableProperty] 
    public double progressValue;
    
    
    

    
    

    
    
    
    public ViewModel()
    {
        ProgressValue = 0;
        LoadBase();
        OpenNote();
    }

    
    
    [RelayCommand]
    private async Task GetLocationAsync() // Метод пользовательского интерфейса для сохранения при благоприятных условиях (если стоишь на нужных координатах).
    {
        try
        {
            var request = new GeolocationRequest(GeolocationAccuracy.Best);
            var location = await Geolocation.GetLocationAsync(request);

            if (location == null)
            {
                Title = "Ошибка";
            }
            else if ((location.Latitude >= 51.19360 && location.Latitude <= 51.19369) &&
                (location.Longitude >= 58.63510 && location.Longitude <= 58.63519))         // Если стоишь на правельных координтах - сохраняется
            {
                // Title = $"Гооол!\n{location.Latitude}\n{location.Longitude}\n{DateTime.Now:HH:mm}\n{DateTime.Today.Date}";
                LoatTime();
                ProgressValue += 0.25;
                SaveNote();
            }
            else if ((location.Latitude < 51.19360 || location.Latitude > 51.19369) &&
                                 (location.Longitude < 58.63510 || location.Longitude > 58.63519)) // Стоищь не на правельных координатах
            {
                //Title = $"Не Гооол!\n{location.Latitude}\n{location.Longitude}";
            }
        }
        catch (FeatureNotSupportedException)
        {
            Title = "Геолокация не поддерживается на этом устройстве.";
        }
        catch (PermissionException)
        {
            Title = "Нет разрешения на доступ к геолокации.";
        }
        catch (Exception ex)
        {
            Title = $"Ошибка: {ex.Message}";
        }
    }
    
//=====================Base=============================================================================================    
    
    public async Task LoadBase() // Подключение к базе данных
    {
        // Присваиваем URL и ключ 
        supabase = new Client(url, key, options);
        await supabase.InitializeAsync();

        Console.WriteLine("Инициализация прошла успешно!");
    }


    public async Task LoatTime() // Метод для сохранения в базу данных время и дату прибытия
    {
        var model = new Time
        {
            Id = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            bdate = DateTime.UtcNow.Date,
            btime = DateTime.Now.TimeOfDay
        };

        await supabase.From<Time>().Insert(model);

    }
    
    
    
//=====================Save=============================================================================================    

    private async Task SaveNote()
    {
        
        
        var path = FileSystem.Current.AppDataDirectory;
        var fullPath = Path.Combine(path, "MyFile.txt");

        File.WriteAllText(fullPath, ProgressValue.ToString());

        await Shell.Current.DisplayAlert("Saved!", $"Note has been saved!{path}", "OK");
    }
    
    private async Task OpenNote()
    {
        var path = FileSystem.Current.AppDataDirectory;
        var fullPath = Path.Combine(path, "MyFile.txt");

        // Проверяем, существует ли файл
        if (!File.Exists(fullPath))
        {
            await Shell.Current.DisplayAlert("Error", "File not found.", "OK");
            return;
        }

        try
        {
            string textFromFile = File.ReadAllText(fullPath); // Считываем содержимое файла


            ProgressValue = Convert.ToDouble(textFromFile); // Преобразуем текст в строку разделение(.Split), так как разделены адреса - пробелом 

            if (ProgressValue == 0)
            {
                await Shell.Current.DisplayAlert("Error", "File is empty.", "OK");
                return;
            }

            

            await Shell.Current.DisplayAlert("Loaded!", "File has been loaded!", "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }






    
    
    
    private string url = "https://kqzirhgsubllbsynoyep.supabase.co";
    private string key =
        "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6ImtxemlyaGdzdWJsbGJzeW5veWVwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDQ0NDQ1NzQsImV4cCI6MjA2MDAyMDU3NH0.HYNGd7PWI_e_9ntzefOG03UtyyPQbKfriot_5Us_v14";
    private SupabaseOptions options = new SupabaseOptions
    {
        AutoConnectRealtime = false
    };
    private Client supabase;
    private Time timeClass;
}



[Table("Time")] // Название таблицы
public class Time : BaseModel // Наследуемся от BaseModel
{
    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("data")]
    public DateTime bdate { get ; set; }
    
    [Column("time")]
    public TimeSpan btime { get ; set;  }
}

