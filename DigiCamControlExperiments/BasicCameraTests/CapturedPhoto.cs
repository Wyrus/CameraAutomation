using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicCameraTests
{
    internal class CapturedPhoto
    {
        public CapturedPhoto(CameraControl.Devices.Classes.PhotoCapturedEventArgs eventArgs)
        {
            Handle = eventArgs.Handle;
            FileName = eventArgs.FileName;
        }

        public object Handle { get; }
        public string FileName { get; }
    }
}
