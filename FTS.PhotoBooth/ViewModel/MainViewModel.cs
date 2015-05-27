using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using AForge.Video.DirectShow;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;

namespace FTS.PhotoBooth.ViewModel
{
    /// <summary>
    /// This class contains properties that the main View can data bind to.
    /// <para>
    /// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
    /// </para>
    /// <para>
    /// You can also use Blend to data bind with the tool's support.
    /// </para>
    /// <para>
    /// See http://www.galasoft.ch/mvvm
    /// </para>
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        /// <summary>
        /// Initializes a new instance of the MainViewModel class.
        /// </summary>
        public MainViewModel()
        {
            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
                SetBorderColor(Colors.Red); 
            }
            else
            {
                // Code runs "for real"
                currentDispatch = Application.Current.Dispatcher;


                Messenger.Default.Register<ExitMsg>
                 (
                   this,
                       (action) => ExitMsgReceived(action)
                  );


                SetBorderColor(Colors.Black); 

                //setting timer
                captureTimer = new System.Timers.Timer();
                captureTimer.Interval = 1000;
                captureTimer.Stop();
                captureTimer.Elapsed += captureTimer_Elapsed;

              
                Images = new ObservableCollection<string>();
                PopulateImages();
                            
                if (Cameras.Count > 0)
                    SelectedCamera = Cameras.First();                
               

            }

           
        }

        Dispatcher currentDispatch;
      

        #region Camera
        public List<MediaInformation> Cameras
        {
            get
            {
                var filterVideoDeviceCollection = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                return (from FilterInfo filterInfo in filterVideoDeviceCollection select new MediaInformation { DisplayName = filterInfo.Name, UsbId = filterInfo.MonikerString }).ToList();
            }
        }


        VideoCaptureDevice videoCaptureDevice; 

        private MediaInformation selectedCamera;
        public MediaInformation SelectedCamera
        {
            get { 
                return selectedCamera; }
            set
            {
                if (selectedCamera == null)
                    ReleaseVideoDevice(); 
                selectedCamera = value;
                InitCamera(); 
                RaisePropertyChanged(() => SelectedCamera);

            }
        }





        private void InitCamera()
        {
            this.videoCaptureDevice = new VideoCaptureDevice(SelectedCamera.UsbId);
            var test = this.videoCaptureDevice.VideoCapabilities;
            this.videoCaptureDevice.VideoResolution = test.Last(); // todo change is to something else....
            this.videoCaptureDevice.NewFrame += videoCaptureDevice_NewFrame;
            this.videoCaptureDevice.Start(); 

        }

        /// <summary>
        ///  Disconnect video source device.
        /// </summary>
        private void ReleaseVideoDevice()
        {           
            if (null == this.videoCaptureDevice)
            {
                return;
            }
            this.videoCaptureDevice.NewFrame -= videoCaptureDevice_NewFrame; 
            this.videoCaptureDevice.SignalToStop();
            this.videoCaptureDevice.WaitForStop();
            this.videoCaptureDevice.Stop();        
            this.videoCaptureDevice = null;
        }


        private object ExitMsgReceived(ExitMsg e)
        {
            this.ReleaseVideoDevice();
            return null; 
        }



        #endregion


        #region List of Tumbnails
        public ObservableCollection<string> Images { get; set; }
        private void PopulateImages()
        {
            currentDispatch.BeginInvoke(DispatcherPriority.Background, new Action(() => Images.Clear()));

            DirectoryInfo folder = new DirectoryInfo(FolderTo);
            var images = folder.GetFiles("*.png").OrderByDescending(fi => fi.LastWriteTime);
            foreach (FileInfo img in images)
            {
                currentDispatch.BeginInvoke(DispatcherPriority.Background, new Action(() => Images.Add(img.FullName)));
            }

        }

        #endregion



        #region image capture and Timer



        void videoCaptureDevice_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {

            currentImage = (Bitmap)eventArgs.Frame.Clone();
            currentSnapshot = (Bitmap)eventArgs.Frame.Clone();
            RaisePropertyChanged(() => ImageDatas);
        }



        private Bitmap currentImage;
        private Bitmap currentSnapshot;

        public Byte[] ImageDatas
        {
            get
            {
                byte[] data = null;
                if (currentImage != null)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        currentImage.Save(memoryStream, ImageFormat.Bmp);
                        data = memoryStream.ToArray();
                    }
                }
                return data;
            }
        }


        private int timer;

        #region Display Timer
        public String DisplayTimer
        {
            get
            {
                if (timer > 0) return timer.ToString();
                return "";
            }
        }

        private SolidColorBrush borderColor; 
        public SolidColorBrush BorderColor
        {

            get { return borderColor; }
            set { borderColor = value;  RaisePropertyChanged(() => BorderColor); }
        }

        private void SetBorderColor(System.Windows.Media.Color c)
        {
            currentDispatch.BeginInvoke(DispatcherPriority.Render, new Action(() => BorderColor = new SolidColorBrush(c)));
        }

        #endregion




        private System.Timers.Timer captureTimer;
        private void captureTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer--;
            RaisePropertyChanged(() => DisplayTimer);
            if (timer <= 0)
            {
                captureTimer.Stop();
                var destFile = FolderTo + '\\' + "Photo" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
                SetBorderColor(Colors.Green); 
                Thread.Sleep(1000); 
                currentSnapshot.Save(destFile, ImageFormat.Png);
                PopulateImages();
                SetBorderColor(Colors.Black); 
                currentDispatch.BeginInvoke(DispatcherPriority.Render, new Action(() => CmdCapture.RaiseCanExecuteChanged()));
            }
        }



        private RelayCommand cmdCapture;
        public RelayCommand CmdCapture
        {
            get
            {

                if (!IsInDesignMode)
                    return cmdCapture ?? (
                        cmdCapture = new RelayCommand(() =>
                                {
                                   SetBorderColor(Colors.Red);

                                    timer = InitTimer;
                                    RaisePropertyChanged(() => DisplayTimer);
                                    captureTimer.Start();
                                    CmdCapture.RaiseCanExecuteChanged();
                                },
                                 () => !captureTimer.Enabled
                         )
                        );
                return new RelayCommand(() => { });
            }
        }




        #endregion


        #region settings

        #region Settings

        // private string folderTo;
        public string FolderTo
        {
            get
            {

                return Properties.Settings.Default.FolderToSave;
            }
            set
            {
                Properties.Settings.Default.FolderToSave = value;
                Properties.Settings.Default.Save();
                PopulateImages();
                RaisePropertyChanged(() => FolderTo);
            }
        }


        public int InitTimer
        {

            get { return Properties.Settings.Default.InitTimer; }

            set
            {
                Properties.Settings.Default.InitTimer = value;
                Properties.Settings.Default.Save();
                RaisePropertyChanged(() => InitTimer);
            }

        }

        #endregion
        private RelayCommand openSettings;
        public RelayCommand OpenSettings
        {
            get
            {
                return openSettings ?? (
                    openSettings = new RelayCommand(() =>
                    {
                        FTS.PhotoBooth.Settings st = new FTS.PhotoBooth.Settings();
                      
                        st.ShowDialog();
                    }
                     )
                    );

            }
        }
        #endregion

    }
}