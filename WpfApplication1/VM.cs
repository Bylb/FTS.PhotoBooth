using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Touchless.Vision.Camera;

namespace FTS.PhotoBooth
{
    public class VM : INotifyPropertyChanged
    {

        private CameraFrameSource _frameSource;

        public List<Camera> Cameras {get; set;}

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
                NotifyPropertyChanged("SelectedCamera");
            }
        }

        public BitmapSource ImgSource
        {

            get
            {
                if (_latestFrame != null)
                    try
                    {
                        return VM.loadBitmap(_latestFrame);
                    }
                    catch (Exception ex)
                    {
                        return null;
                    }
                return null;
            }
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


        public VM()
        {
            Cameras = new List<Camera>();
            foreach (Camera cam in CameraService.AvailableCameras)
            {
                Cameras.Add(cam);
            }

            if (Cameras.Count > 0)
                SelectedCamera = Cameras.FirstOrDefault(); 

        }

        private Bitmap _latestFrame;
     

        public void OnImageCaptured(Touchless.Vision.Contracts.IFrameSource frameSource, Touchless.Vision.Contracts.Frame frame, double fps)
        {
            _latestFrame = (Bitmap)frame.Image;
            
            if (_latestFrame != null)
            {             
                NotifyPropertyChanged("ImgSource");              
            }
          
            
        
        }


        #region  bitmap to bitmapsource

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public static BitmapSource loadBitmap(System.Drawing.Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }



        #endregion



        private void setFrameSource(CameraFrameSource cameraFrameSource)
        {
            if (_frameSource == cameraFrameSource)
                return;
            _frameSource = cameraFrameSource;
        }

   


        #region notification

        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }


        #endregion

    }
}
