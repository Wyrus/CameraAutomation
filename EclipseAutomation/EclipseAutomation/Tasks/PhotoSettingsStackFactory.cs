using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EclipseAutomation.Config;

namespace EclipseAutomation.Tasks
{
    public class PhotoSettingsStackFactory
    {
        PhotoSettings _defaultSettings;
        public PhotoSettingsStackFactory(PhotoSettings defaultSettings) 
        {
            _defaultSettings = defaultSettings;
        }
        public PhotoSettingsStackFactory(StackDetails stack)
        {
            _defaultSettings = new PhotoSettings
            {
                IsoNumber = stack.IsoNumber,
                FNumber = stack.FNumber,
                ExposureCompensation = stack.ExposureCompensation,
                ShutterSpeed = ""
            };
        }

        public List<PhotoSettings> GetShutterStack(string shutterSettings)
        {
            return GetShutterStack(shutterSettings.Split(new char[] { ';',',' }));
        }
        /// <summary>
        /// Make sure you use shutter settings the are compatible with your camera.
        /// </summary>
        /// <param name="shutterSettings"></param>
        /// <returns></returns>
        public List<PhotoSettings> GetShutterStack(params string[] shutterSettings)
        {
            if(shutterSettings==null|| shutterSettings.Length==0)
                throw new ArgumentNullException(nameof(shutterSettings));

            List<PhotoSettings> result = new List<PhotoSettings>();

            foreach (var shutter in shutterSettings)
            {
                PhotoSettings settings = new PhotoSettings(_defaultSettings);
                settings.ShutterSpeed = shutter.Trim();
                result.Add(settings);
            }
            return result;
        }

    }
}
