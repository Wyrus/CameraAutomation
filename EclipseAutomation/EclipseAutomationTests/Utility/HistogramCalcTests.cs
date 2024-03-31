using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static System.Net.Mime.MediaTypeNames;

namespace EclipseAutomation.Utility
{
   [TestClass]
   public class HistogramCalcTests
   {
      [TestMethod]
      public void ExistingImageHistogram_ManualTest()
      {
         //string existingSourceImage = @"C:\temp\EclipseAutomation\test\Hist152049a.JPG";
         string existingSourceImage = @"C:\temp\EclipseAutomation\test\D7K_2140.JPG";
         if(!File.Exists(existingSourceImage))
         {
            Assert.Inconclusive("Expected source file doesn't exist.");
         }

         var histCalc = new HistogramCalc(0.5f);
         Bitmap img = (Bitmap)Bitmap.FromFile(existingSourceImage);

         var result = histCalc.Compute(img);

         string outfile = Path.ChangeExtension(existingSourceImage, "png");
         result.Save(outfile, System.Drawing.Imaging.ImageFormat.Png);
      }

      [TestMethod]
      public void ExistingImageWithHistogramOverlay_ManualTest()
      {
         //string existingSourceImage = @"C:\temp\EclipseAutomation\test\Hist152049a.JPG";
         string existingSourceImage = @"C:\temp\EclipseAutomation\test\D7K_2140.JPG";
         if (!File.Exists(existingSourceImage))
         {
            Assert.Inconclusive("Expected source file doesn't exist.");
         }

         var histCalc = new HistogramCalc(0.5f);
         Bitmap img = (Bitmap)Bitmap.FromFile(existingSourceImage);

         var result = histCalc.OverlayHistogram(img);

         string outfile = Path.ChangeExtension(existingSourceImage, "png");
         result.Save(outfile, System.Drawing.Imaging.ImageFormat.Png);
      }
   }
}
