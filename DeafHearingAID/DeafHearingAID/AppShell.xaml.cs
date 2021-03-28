using DeafHearingAID.ViewModels;
using DeafHearingAID.Views;
using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace DeafHearingAID
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            

        }

    }
}
