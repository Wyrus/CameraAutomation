using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EclipseAutomation.Tasks;

namespace EclipseAutomation.Camera
{
   public interface ICameraHandler : IDisposable
   {
      CameraStates CameraState { get; }

      void TakePhotos(IEnumerable<PhotoSettings> settings, bool saveToPCNotCameraSD, int tries = 3);

      bool StartLiveView(PhotoSettings settings);

      void StopLiveView();

      Statistic TakePhotoStatistics { get; }
      Statistic AwaitCaptureStatistics { get; }
      Statistic AwaitDownloadStatistics { get; }
      Statistic LiveViewFrameCaptureStatistics { get; }
   }
}