using DeafHearingAID.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace DeafHearingAID.ViewModels
{
    [QueryProperty(nameof(AudioText), nameof(AudioText))]
    public class ItemDetailViewModel : BaseViewModel
    {
        private string audiotext;
       
      

        public ItemDetailViewModel()
        {
           
        }

        //public string ItemId
        //{
        //    get
        //    {
        //        return itemId;
        //    }
        //    set
        //    {
        //        itemId = value;
        //        LoadItemId(value);
        //    }
        //}

        

        public string AudioText
        {
            get
            {
                return audiotext;
            }
            set
            {
                SetProperty(ref audiotext, value);
            }
        }

       

        public void GetAIDItems(string adtext)
        {
            try
            {
                AudioText = adtext;

            }
            catch (Exception)
            {
                Debug.WriteLine("Failed to Load Item");
            }
        }
    }
}
