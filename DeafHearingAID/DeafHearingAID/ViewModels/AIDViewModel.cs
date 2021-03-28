using DeafHearingAID.Models;
using Microsoft.CognitiveServices.Speech;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace DeafHearingAID.ViewModels
{
    public class AIDViewModel : BaseViewModel
    {

        SpeechRecognizer speechrecognizer;

        SpeechConfig config;

        TaskCompletionSource<int> stopRecognition;

        internal const string TableName = "AID";

        private static TimeZoneInfo INDIAN_ZONE = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");



        private string textidentified;
        public string TextIdentified
        {
            get => textidentified;
            set => SetProperty(ref textidentified, value);
        }

        private bool startbuttonenabled = true;

        public bool StartButtonEnabled
        {
            get => startbuttonenabled;
            set => SetProperty(ref startbuttonenabled, value);
        }

        private bool stopbuttonenabled = false;

        public bool StopButtonEnabled
        {
            get => stopbuttonenabled;
            set => SetProperty(ref stopbuttonenabled, value);
        }


        private bool stopbuttoninitialvisible = false;

        public bool Stopbuttoninitialvisible
        {
            get => stopbuttoninitialvisible;
            set => SetProperty(ref stopbuttoninitialvisible, value);
        }

        private string currentlang = string.Empty;

        public string CurrentLang
        {
            get => currentlang;
            set => SetProperty(ref currentlang, value);
        }

        public AIDViewModel()
        {
            Title = "Hearing AID";

            stopRecognition = new TaskCompletionSource<int>();

            ListningStartCommand = new Command(async () => await StartAudioRecordingAsync());

            EndListningCommand = new Command(async () => await StopAudioRecordingAsync());

            ClearTextCommand = new Command(() => ClearText());

            SaveDataToAzureTableCommand = new Command(async () => await InsertAIDDeatilsTOAzureTable(textidentified, CurrentLang));

        }

        public void ClearText()
        {
            TextIdentified = string.Empty;
        }

        public ICommand ListningStartCommand { get; }

        public ICommand EndListningCommand { get; }

        public ICommand ClearTextCommand { get; }

        public ICommand SaveDataToAzureTableCommand { get; }

        private async Task StartAudioRecordingAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentLang))
                {
                    await Application.Current.MainPage.DisplayAlert("Language", "Please select Language.", "ok");
                }
                else
                {
                    StopButtonEnabled = true;
                    StartButtonEnabled = false;
                    Stopbuttoninitialvisible = true;

                    bool micenabled = await GetMicroPhoneEnabledStatusAsync();

                    if (config == null)
                    {
                        //please enter correct values from azure portal
                        config = SpeechConfig.FromSubscription("***", "***");
                    }

                    config.SpeechRecognitionLanguage = GetUserSelectedLang(CurrentLang);



                    speechrecognizer = new SpeechRecognizer(config);

                    speechrecognizer.Recognized += (s, e) =>
                    {


                        if (e.Result.Reason == ResultReason.RecognizedKeyword)
                        {
                            TextIdentified += e.Result.Text;
                        }
                        else if (e.Result.Reason == ResultReason.RecognizedSpeech)
                        {
                            TextIdentified += e.Result.Text;
                        }
                        else if (e.Result.Reason == ResultReason.NoMatch)
                        {
                            TextIdentified += "unable to get text from the speech.";
                        }
                    };

                    speechrecognizer.Canceled += (s, e) =>
                    {
                        var cancellation = CancellationDetails.FromResult(e.Result);

                        textidentified = $"CANCELED: Reason={cancellation.Reason} ErrorDetails={cancellation.ErrorDetails}";

                        if (cancellation.Reason == CancellationReason.Error)
                        {
                            
                        }

                        stopRecognition.TrySetResult(0);
                    };

                    speechrecognizer.SessionStarted += (s, e) =>
                    {
                        
                    };

                    speechrecognizer.SessionStopped += (s, e) =>
                    {
                        stopRecognition.TrySetResult(0);
                    };

                    await speechrecognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);

                    Task.WaitAny(new[] { stopRecognition.Task });

                    await speechrecognizer.StopKeywordRecognitionAsync().ConfigureAwait(false);

                }
            }
            catch (Exception ex)
            {
                
            }
        }

        private string GetUserSelectedLang(string userselectedlang)
        {
            switch (userselectedlang)
            {
                case "English":
                    return "en-IN";

                case "Gujarati":
                    return "gu-IN";

                case "Hindi":
                    return "hi-IN";

                case "Marathi":
                    return "mr-IN";

                case "Tamil":
                    return "ta-IN";

                case "Telugu":
                    return "te-IN";

                //english as default language

                default:
                    return "en-IN";

            }
        }

        private async Task StopAudioRecordingAsync()
        {
            try
            {
                StopButtonEnabled = false;
                StartButtonEnabled = true;

                if (speechrecognizer != null)
                {
                    await speechrecognizer.StopContinuousRecognitionAsync();
                }
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<bool> GetMicroPhoneEnabledStatusAsync()
        {
            bool micAccessGranted = await DependencyService.Get<IMicrophoneService>().GetPermissionsAsync();
            if (!micAccessGranted)
            {
                
            }

            return micAccessGranted;
        }

        public async Task<bool> InsertAIDDeatilsTOAzureTable(string audiotext, string language)
        {
            try
            {
                if (string.IsNullOrEmpty(audiotext) || string.IsNullOrEmpty(language))
                {
                    await Application.Current.MainPage.DisplayAlert("Text Recorded , Language", "Please select Language and make sure you have text recorded", "ok");
                    return false;
                }
                else
                {
                    CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=deafaid;AccountKey=ch8fbsZmxgSbFGfLw5lGNXgGlRUBN+Actts2M09bIUgomisMHySQv7xuiVDbj5k//BwpVF7V6TMGniOkhFn17Q==;EndpointSuffix=core.windows.net");

                    CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                    CloudTable table = tableClient.GetTableReference(TableName);

                    AIDItem details;

                    details = new AIDItem($"{TableName}{DateTime.Now:dd-MM-yyyy}", $"{TableName}{DateTime.Now:dd-MM-yyyy-HH-mm-ss}");

                    details.SavedTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, INDIAN_ZONE);

                    details.AudioText = audiotext;
                    details.Language = language;

                    await table.CreateIfNotExistsAsync();

                    TableOperation insertOperation = TableOperation.Insert(details);

                    var insertoperationresult = await table.ExecuteAsync(insertOperation);

                    await Application.Current.MainPage.DisplayAlert("Data saved", "Data has been saved successfully.", "ok");

                    ClearText();

                    return true;
                }

            }
            catch (Exception ex)
            {
                return default;
            }

        }

    }
}