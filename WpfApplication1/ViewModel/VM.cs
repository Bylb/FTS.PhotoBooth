using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Timers;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Touchless.Vision.Camera;

namespace FTS.PhotoBooth.ViewModel
{
    public class VM : ViewModelBase
    {

        public VM()
        {

            if (IsInDesignMode)
            {
                // Code runs in Blend --> create design time data.
            }
            else
            {
                // Code runs "for real"
                currentDispatch = Application.Current.Dispatcher;
                
                //setting timer
                captureTimer = new Timer();
                captureTimer.Interval = 1000;
                captureTimer.Stop();
                captureTimer.Elapsed += captureTimer_Elapsed;

                TimerValue = 4;
                FolderTo = "c:\\";

                Images = new ObservableCollection<string>();
                PopulateImages();

                Cameras = new List<Camera>();
                foreach (Camera cam in CameraService.AvailableCameras)
                {
                    Cameras.Add(cam);
                }
                if (Cameras.Count > 0)
                    SelectedCamera = Cameras.FirstOrDefault();

            }

           

         
        }

        Dispatcher currentDispatch; 


        #region Camera 

        
        private CameraFrameSource _frameSource;

        public List<Camera> Cameras { get; set; }

        private Camera selectedCamera;
        public Camera SelectedCamera
        {
            get { return selectedCamera; }
            set
            {
                selectedCamera = value;

                //start Camera listening
                if (_frameSource != null)
                {
                    _frameSource.NewFrame -= OnImageCaptured;
                    _frameSource.Camera.Dispose();
                    setFrameSource(null);
                }
                setFrameSource(new CameraFrameSource(selectedCamera));
                _frameSource.Camera.CaptureWidth = 1280;
                _frameSource.Camera.CaptureHeight = 720;
                _frameSource.Camera.Fps = 50;
                _frameSource.NewFrame += OnImageCaptured;
                _frameSource.StartFrameCapture();
                RaisePropertyChanged(() => SelectedCamera);
                
            }
        }




        private void setFrameSource(CameraFrameSource cameraFrameSource)
        {
            if (_frameSource == cameraFrameSource)
                return;
            _frameSource = cameraFrameSource;
        }

   

      



        #endregion 

        #region image source

        private Byte[] latestFrameB;

        public Byte[] ImgCaptured
        {
            get { return latestFrameB; }
            private set { latestFrameB = value; RaisePropertyChanged(() => ImgCaptured);  }
        } 

        public void OnImageCaptured(Touchless.Vision.Contracts.IFrameSource frameSource, Touchless.Vision.Contracts.Frame frame, double fps)
        {
            ImgCaptured = frame.ImageData; 
        
        }

    

        #endregion

        #region List of Tumbnails


        public ObservableCollection<string> Images { get; set; }

        private void PopulateImages()
        {
            currentDispatch.BeginInvoke(  DispatcherPriority.Background,  new Action(() =>  Images.Clear()));
           
            DirectoryInfo folder = new DirectoryInfo(FolderTo);
           var images = folder.GetFiles("*.png").OrderByDescending(fi => fi.LastWriteTime);
            foreach (FileInfo img in images)
            {
                currentDispatch.BeginInvoke(DispatcherPriority.Background, new Action(() => Images.Add(img.FullName))); 
            }

        }

        #endregion



        #region image capture and Timer


        private string folderTo;
        public string FolderTo
        {
            get { return folderTo; }
            set { folderTo = value; RaisePropertyChanged(() => FolderTo); }
        } 



        private int timer;
        public String DisplayTimer
        {
            get
            {
                if (timer > 0) return timer.ToString();
                return "";
            }
        }



        private Timer captureTimer;
        private void captureTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer--;
            RaisePropertyChanged(() => DisplayTimer);
            if (timer <= 0)
            {
                captureTimer.Stop(); 
                using (MemoryStream ms = new MemoryStream(ImgCaptured) )
                {
                    Image dest = Image.FromStream(ms);
                    var destFile  = FolderTo + '\\' + "Photo"+DateTime.Now.ToString("yyyyMMddHHmmss") +".png"; 
                    dest.Save(destFile, ImageFormat.Png);
                    
                    PopulateImages();
                    currentDispatch.BeginInvoke(DispatcherPriority.Background, new Action(() =>  CmdCapture.RaiseCanExecuteChanged())); 

                }
            }
        }



        private RelayCommand cmdCapture;
        public RelayCommand CmdCapture
        {
            get
            {
                return cmdCapture ?? (
                    cmdCapture = new RelayCommand(() => {
                        timer = TimerValue;
                        RaisePropertyChanged(() => DisplayTimer);
                        captureTimer.Start();
                        CmdCapture.RaiseCanExecuteChanged(); 
                            },
                             () => !captureTimer.Enabled
                     )
                    ); 

            }
        }

        


        #endregion


        #region settings


        public int TimerValue {get; set;}

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
