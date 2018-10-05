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
            _adapter.DeviceConnected -= AdapterDeviceConnected;
            _adapter.DeviceConnected += AdapterDeviceConnected;

            InitializeComponent();

            btnSendAny.Clicked += BtnSendAny;
            btnListData.Clicked += BtnListData;
            btnStop.Clicked += BtnStopReading;
            btnExit.Clicked += BtnExit;
            btnReset.Clicked += BtnReset;
            btnStart.Clicked += BtnStart;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _adapter.ConnectToDevice(_device);
        }

        private void AdapterDeviceConnected(object sender, DeviceConnectionEventArgs e)
        {
            _device = e.Device; // do we need to overwrite this?

            // when services are discovered
            _device.ServicesDiscovered -= ServicesDiscovered;
            _device.ServicesDiscovered += ServicesDiscovered;

            // start looking for services
            _device.DiscoverServices();
        }

        private void ServicesDiscovered(object se, EventArgs ea)
        {
            if (characteristicNotify == null || characteristicWrite == null)
            {
                Debug.WriteLine("device.ServicesDiscovered");

                //_service = _device.Services.Where(x => x.ID == sguid).FirstOrDefault();
                _service = ((IDevice)se).Services.Where(x => x.ID == sguid).FirstOrDefault();

                //get the characteristic for notify and write
                characteristicNotify = _service.Characteristics.Where(x => x.ID == nguid).FirstOrDefault();

                StartUpdateHandler(characteristicNotify);

                characteristicWrite =
                    _service.Characteristics.Where(x => x.ID == wguid).FirstOrDefault();
            }
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
            WriteCharacteristicCmd(characteristicWrite, " ");
        }

        void BtnListData(object sender, System.EventArgs e)
        {
            WriteCharacteristicCmd(characteristicWrite, "L");
        }
        void BtnExit(object sender, System.EventArgs e)
        {
            WriteCharacteristicCmd(characteristicWrite, "X");
        }

        void BtnReset(object sender, System.EventArgs e)
        {
            WriteCharacteristicCmd(characteristicWrite, "R");
        }
        void BtnStopReading(object sender, System.EventArgs e)
        {
            StopUpdateHandler(characteristicNotify);
        }

        void BtnStart(object sender, System.EventArgs e)
        {
            StartUpdateHandler(characteristicNotify);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            StopUpdateHandler(characteristicNotify);

            _adapter.DisconnectDevice(_device);
        }

        private void WriteCharacteristicCmd(ICharacteristic icharacter, string cmd)
        {
            var data = System.Text.Encoding.UTF8.GetBytes(cmd);
            if (icharacter.CanWrite)
                icharacter.Write(data);
        }
        private void StartUpdateHandler(ICharacteristic icharacter)
        {
            if (icharacter.CanUpdate)
            {
                icharacter.StartUpdates();
                icharacter.ValueUpdated -= CharacteristicValueUpdated;
                icharacter.ValueUpdated += CharacteristicValueUpdated;
            }
        }
        private void StopUpdateHandler(ICharacteristic icharacter)
        {
            if (icharacter.CanUpdate)
            {
                icharacter.StartUpdates();
                icharacter.ValueUpdated -= CharacteristicValueUpdated;
            }
        }
    }
}