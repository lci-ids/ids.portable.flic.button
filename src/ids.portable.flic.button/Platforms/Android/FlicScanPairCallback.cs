using System.Threading.Tasks;
using IDS.Portable.Common;
using IDS.Portable.Flic.Button.Platforms.Shared;
using IO.Flic.Flic2libandroid;

namespace IDS.Portable.Flic.Button.Platforms.Android
{
    public class FlicScanPairCallback : Java.Lang.Object, IFlic2ScanCallback
    {
        private const string LogTag = "FlicScanPairCallback";
        private readonly TaskCompletionSource<FlicButtonDeviceData?> _tcs;

        public FlicScanPairCallback(TaskCompletionSource<FlicButtonDeviceData?> tcs)
        {
            _tcs = tcs;
        }

        public void OnDiscoveredAlreadyPairedButton(Flic2Button? button)
        {
            // Found a button that's already paired.
            TaggedLog.Debug(LogTag, $"Found already paired button.");

            if (button is null)
            {
                _tcs.SetResult(null);
                return;
            }

            _tcs.SetResult(new FlicButtonDeviceData(button.SerialNumber, button.BdAddr, button.FirmwareVersion, button.Uuid));
        }

        public void OnDiscovered(string? bdAddr)
        {
            // Found Flic, attempting to connect.
            TaggedLog.Debug(LogTag, $"Discovered Flic: {bdAddr}.");
        }

        public void OnConnected()
        {
            // Connected. Now pairing.
            TaggedLog.Debug(LogTag, $"Successfully connected to Flic, attempting to pair.");
        }

        public void OnComplete(int result, int subCode, Flic2Button? button)
        {
            if (button is not null && result == Flic2ScanCallback.ResultSuccess)
            {
                // We're paired, the button is good to go.
                TaggedLog.Debug(LogTag, $"Pair result success, ready to go.");

                _tcs.SetResult(new FlicButtonDeviceData(button.SerialNumber, button.BdAddr, button.FirmwareVersion, button.Uuid));
            }
            else
            {
                // Failure.
                TaggedLog.Debug(LogTag, $"Failed to pair, result: {result} subCode: {subCode}.");

                _tcs.SetResult(null);
            }
        }
    }
}
