using Microsoft.Maui.Controls.Shapes;
using System.Globalization;
using System.Net.Http;

namespace MAUIThreadingTask
{
    public class apiEvent
    {
        public delegate void apiEventHandler(object sender, apiEventArgs e);

        public event apiEventHandler? OnResponse;

        public void OnApiResponse(string content)
        {
            apiEventHandler? handler = OnResponse;

            if (handler != null)
            {
                handler(this, new apiEventArgs(content));
            }
        }
    }

    public class apiEventArgs : EventArgs
    {
        public string Link { get; }

        public apiEventArgs(string content)
        {
            this.Link = content;
        }
    }

    public partial class MainPage : ContentPage
    {
        public apiEvent apiService = new apiEvent();

        Thread? mainTimer;
        long ticks;

        public MainPage()
        {
            InitializeComponent();

            getImageButton.Clicked += GetImage;
            apiService.OnResponse += SetImage;

            ticks = 0;
            mainTimer = new Thread(new ThreadStart(timerUpdate));
            mainTimer.Start();
        }

        public void timerUpdate()
        {
            while (true)
            {
                ticks++;
                Thread.Sleep(50);
                MainThread.BeginInvokeOnMainThread(() => {
                    timerLabel.Text = $"Tick Count: {ticks}";
                });
            }
        }

        private void SetImage(object? sender, apiEventArgs e)
        {
            mainImage.Source = e.Link;
        }

        private void GetImage(object? sender, EventArgs e)
        {
            string apiUrl = "https://random.imagecdn.app/v1/image";

            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync(apiUrl).Result;

                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    apiService.OnApiResponse(content);
                }
            }
        }
    }
}