using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Media;
using System.Linq; 
using FTS.PhotoBooth.Video;

using GalaSoft.MvvmLight;
using System;
using System.Drawing.Imaging;

namespace FTS.PhotoBooth.ViewModel
{
    /// <summary>


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
            }
            else
            {
                // Code runs "for real"
                this.MediaDeviceList = new List<MediaInformation>( WebcamDevice.GetVideoDevices);
                this.SelectedVideoDevice = null;
                if (this.MediaDeviceList.Count > 0)
                    this.SelectedVideoDevice = this.MediaDeviceList.First();
                FolderTo = "c:\\";
            }
        }


        #region snapshot

        private int timeBeforeSnapshot;

        public int TimeBeforeSnapshot
        {
            get { return timeBeforeSnapshot; }
            set { timeBeforeSnapshot = value; this.RaisePropertyChanged(() => this.TimeBeforeSnapshot); }
        }





        private string folderTo;
        public string FolderTo
        {
            get { return folderTo; }
            set { folderTo = value; RaisePropertyChanged(() => FolderTo); }
        } 


        /// <summary>
        /// Byte array of snapshot image.
        /// </summary>
        private Bitmap snapshotBitmap;

        /// <summary>
        /// Gets or sets snapshot bitmap.
        /// </summary>
        public Bitmap SnapshotBitmap
        {
            get
            {
                return this.snapshotBitmap;
            }

            set
            {
                this.snapshotBitmap = value;

                var destFile = FolderTo + '\\' + "Photo" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
                snapshotBitmap.Save(destFile, ImageFormat.Png);

                this.RaisePropertyChanged(() => this.SnapshotBitmap);
            }
        }

        #endregion


        /// <summary>
        /// Gets or sets selected media video device.
        /// </summary>
        /// 
        private MediaInformation selectedVideoDevice;
        public MediaInformation SelectedVideoDevice
        {
            get
            {
                return this.selectedVideoDevice;
            }

            set
            {
                this.selectedVideoDevice = value;
                this.RaisePropertyChanged(() => this.SelectedVideoDevice);
            }
        }



        private List<MediaInformation> mediaDeviceList;
        /// <summary>
        /// Gets or sets media device list.
        /// </summary>
        public List<MediaInformation> MediaDeviceList
        {
            get
            {
                return this.mediaDeviceList;
            }

            set
            {
                this.mediaDeviceList = value;
                this.RaisePropertyChanged(() => this.MediaDeviceList);
            }
        }

    }
}