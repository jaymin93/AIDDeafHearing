using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeafHearingAID
{
    public interface IMicrophoneService
    {
        Task<bool> GetPermissionsAsync();
        void OnRequestPermissionsResult(bool isGranted);
    }
}
