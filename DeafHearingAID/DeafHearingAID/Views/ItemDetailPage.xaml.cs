using DeafHearingAID.ViewModels;
using System.ComponentModel;
using Xamarin.Forms;

namespace DeafHearingAID.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }
    }
}