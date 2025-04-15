using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices.Sensors;

namespace App;

public partial class MainPage : ContentPage
{
    
    ViewModel vm = new ViewModel();
    public MainPage()
    {
        InitializeComponent();
        BindingContext = vm;
    }

    
}