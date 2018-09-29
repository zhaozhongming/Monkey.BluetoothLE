using Xamarin.Forms;
using Robotics.Mobile.Core.Bluetooth.LE;
using System.Diagnostics;
using System.Linq;
using System;
using System.Collections.ObjectModel;

namespace BLEExplorer.Pages
{	
	public partial class CharacteristicDetail : ContentPage
	{
        IAdapter _adapter;
        IDevice _device;
        IService _service;
        ICharacteristic characteristicWrite;
        ICharacteristic characteristicNotify;
        ObservableCollection<IService> services = new ObservableCollection<IService>();
        
        const string sid = "0003abcd-0000-1000-8000-00805f9b0131";
        static readonly Guid sguid = new Guid(sid);

        const string nid = "00031234-0000-1000-8000-00805F9B0130";
        static readonly Guid nguid = new Guid(nid);
        const string wid = "00031234-0000-1000-8000-00805F9B0131";
        static readonly Guid wguid = new Guid(wid);

        public CharacteristicDetail (IAdapter adapter, IDevice device)
		{
            _adapter = adapter;
            _device = device;

            // when device is connected
            adapter.DeviceConnected += AdapterDeviceConnected;

            adapter.ConnectToDevice(device);

            InitializeComponent();

            btnSendAny.Clicked += BtnSendAny;
            btnListData.Clicked += BtnListData;
            btnStop.Clicked += BtnStopReading;
            btnExit.Clicked += BtnExit;
            btnReset.Clicked += BtnReset;
            btnStart.Clicked += BtnStart;
        }

        void AdapterDeviceConnected(object sender, DeviceConnectionEventArgs e)
        {
            _device = e.Device; // do we need to overwrite this?

            // when services are discovered
            _device.ServicesDiscovered += (object se, EventArgs ea) =>
            {
                Debug.WriteLine("device.ServicesDiscovered");
               
                _service = _device.Services.Where(x => x.ID == sguid).FirstOrDefault();

                //get the characteristic for notify and write
                characteristicNotify = _service.Characteristics.Where(x => x.ID == nguid).FirstOrDefault();

                if (characteristicNotify.CanUpdate)
                {
                    characteristicNotify.ValueUpdated += CharacteristicValueUpdated;
                    characteristicNotify.StartUpdates();
                }

                characteristicWrite =
                    _service.Characteristics.Where(x => x.ID == wguid).FirstOrDefault();
            };

            // start looking for services
            _device.DiscoverServices();
        }
        private void CharacteristicValueUpdated(object sender, CharacteristicReadEventArgs e)
        {
            string msg = string.Empty;

            msg = new string(System.Text.Encoding.UTF8.GetChars(e.Characteristic.Value));

            if (msg.EndsWith("OK")
                || msg.EndsWith("***"))
            {
                msg += "\r\n";
            }

            Device.BeginInvokeOnMainThread(() => entryReceived.Text += msg);
        }

        void BtnSendAny(object sender, System.EventArgs e)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(" ");

            if (characteristicWrite.CanWrite)
                characteristicWrite.Write(data);
        }

        void BtnListData(object sender, System.EventArgs e)
        {
            var data = System.Text.Encoding.UTF8.GetBytes("L");

            if (characteristicWrite.CanWrite)
                characteristicWrite.Write(data);
        }
        void BtnExit(object sender, System.EventArgs e)
        {
            var data = System.Text.Encoding.UTF8.GetBytes("X");

            if (characteristicWrite.CanWrite)
                characteristicWrite.Write(data);
        }

        void BtnReset(object sender, System.EventArgs e)
        {
            var data = System.Text.Encoding.UTF8.GetBytes("R");

            if (characteristicWrite.CanWrite)
                characteristicWrite.Write(data);
        }
        void BtnStopReading(object sender, System.EventArgs e)
        {
            if (characteristicNotify.CanUpdate)
            {
                characteristicNotify.StopUpdates();
                characteristicNotify.ValueUpdated -= CharacteristicValueUpdated;
            }
        }

        void BtnStart(object sender, System.EventArgs e)
        {
            if (characteristicNotify.CanUpdate)
            {
                characteristicNotify.StartUpdates();
                characteristicNotify.ValueUpdated += CharacteristicValueUpdated;
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (characteristicNotify.CanUpdate)
            {
                characteristicNotify.StopUpdates();
                characteristicNotify.ValueUpdated -= CharacteristicValueUpdated;
            }
        }
    }
}