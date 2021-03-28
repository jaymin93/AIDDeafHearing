using DeafHearingAID.Models;
using DeafHearingAID.Views;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DeafHearingAID.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private AIDItem _selectedItem;

        public ObservableCollection<AIDItem> Items { get; }
        public Command LoadItemsCommand { get; }

        public Command GetCustomDateHistoryCommand { get; }
        
        public Command<AIDItem> ItemTapped { get; }


        private DateTime dt = DateTime.Now.Date;
        public DateTime DT
        {
            get
            {
                return dt;
            }
            set
            {
                SetProperty(ref dt, value);
            }
        }

        public ItemsViewModel()
        {
            Title = "History";
            Items = new ObservableCollection<AIDItem>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand(DT).ConfigureAwait(false));
            GetCustomDateHistoryCommand =  new Command(async () => await ExecuteLoadItemsCommand(DT).ConfigureAwait(false));

            ItemTapped = new Command<AIDItem>(OnItemSelected);


        }

        async Task ExecuteLoadItemsCommand(DateTime dt = default)
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var items = await GetHearingAIDDetailsasynv(dt).ConfigureAwait(false);
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public static async Task<List<AIDItem>> GetHearingAIDDetailsasynv(DateTime dateTime = default)
        {
            try
            {
                List<AIDItem> hearingaidrecords = new List<AIDItem>();

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=deafaid;AccountKey=ch8fbsZmxgSbFGfLw5lGNXgGlRUBN+Actts2M09bIUgomisMHySQv7xuiVDbj5k//BwpVF7V6TMGniOkhFn17Q==;EndpointSuffix=core.windows.net");

                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudTable _linkTable = tableClient.GetTableReference(AIDViewModel.TableName);


                TableQuery<AIDItem> query;

                if (dateTime != default(DateTime))
                {
                    query = new TableQuery<AIDItem>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{AIDViewModel.TableName}{dateTime:dd-MM-yyyy}"));
                }
                else
                {
                    query = new TableQuery<AIDItem>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, $"{AIDViewModel.TableName}{DateTime.Now:dd-MM-yyyy}"));
                }


                TableContinuationToken token = null;
                do
                {
                    TableQuerySegment<AIDItem> resultSegment = await _linkTable.ExecuteQuerySegmentedAsync(query, token).ConfigureAwait(false);
                    token = resultSegment.ContinuationToken;

                    foreach (var entity in resultSegment.Results)
                    {
                        AIDItem aiddetails = new AIDItem
                        {
                            AudioText = entity.AudioText,
                            Language = entity.Language,
                            SavedTime = entity.SavedTime

                        };

                        hearingaidrecords.Add(aiddetails);
                    }
                } while (token != null);


                return hearingaidrecords;
            }
            catch (Exception exp)
            {
                Debug.Write(exp);
                return default;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;

        }

        public AIDItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        async void OnItemSelected(AIDItem item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.AudioText)}={item.AudioText}");
        }
    }
}