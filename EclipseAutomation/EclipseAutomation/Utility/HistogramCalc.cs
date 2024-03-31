using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;

namespace EclipseAutomation.Utility
{
   /// <summary>
   /// NOTE: The resultant histograms are targeted at solar images with lots of darkness. 
   /// The darkest colors are NOT included in the histogram.
   /// </summary>
   internal class HistogramCalc
   {
      float _histPercent;
      public HistogramCalc(float histPercent = 0.25f)
      {
         if(histPercent<0.01 || histPercent>1)
            throw new ArgumentOutOfRangeException(nameof(histPercent),"0.01 <= histPercent <= 1.0");

         _histPercent = histPercent;
      }

      public Bitmap Compute(Bitmap srcImg)
      {
         if(srcImg.Width * _histPercent < 256)
         {
            throw new ArithmeticException("Image.Width * _histPercent must not be < 256");
         }

         Mat src = BitmapConverter.ToMat(srcImg);

         var srcDepth = src.Depth();

         var rawHist = ComputeHistogram(src);

         Mat hist = new Mat(rawHist.Rows, rawHist.Cols, rawHist.Depth());
         rawHist.ConvertTo(hist, srcDepth);

         var result = BitmapConverter.ToBitmap(hist);

         return result;
      }

      public Bitmap OverlayHistogram(Bitmap srcImg)
      {
         Bitmap hist = Compute(srcImg);

         //Bitmap clone = new Bitmap(srcImg.Width, srcImg.Height, PixelFormat.Format32bppArgb);
         using (var gr = Graphics.FromImage(srcImg))
         {
            int left = srcImg.Width - hist.Width - 2;
            int top = srcImg.Height - hist.Height - 2;

            // doesn't need a border
            //using (var pen = new Pen(Color.White, 2f))
            //{
            //   gr.DrawRectangle(pen, left-1, top-1, hist.Width + 2, hist.Height + 2);
            //}

            gr.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;
            gr.DrawImage(hist, left, top, hist.Width, hist.Height);
         }

         return srcImg;
      }

   // Most of this code comes from the Histogram sample at https://github.com/shimat/opencvsharp/wiki/Histogram
   private Mat ComputeHistogram(Mat srcMat)
      {
         var gray = srcMat.CvtColor(ColorConversionCodes.BGR2GRAY);

         Scalar background = Scalar.All(0);
         Scalar foreground = new Scalar(255, 255, 255, 192);   // 25% transparent white

         // Histogram view
         int width = (int)(srcMat.Width * _histPercent);
         int height = (int)(srcMat.Height * _histPercent);

         Mat render = new Mat(new OpenCvSharp.Size(width, height), MatType.CV_8UC4, background);

         // Calculate histogram
         Mat hist = new Mat();
         int[] hdims = { 256 }; // Histogram size for each dimension
         Rangef[] ranges = { new Rangef(5, 256), }; // min/max 
         Cv2.CalcHist(
             new Mat[] { gray },
             new int[] { 0 },
             null,
             hist,
             1,
             hdims,
             ranges);

         // Get the max value of histogram
         double minVal, maxVal;
         Cv2.MinMaxLoc(hist, out minVal, out maxVal);
         //System.Diagnostics.Debug.WriteLine($"Min={minVal}, Max={maxVal}, hdims={String.Join(",",hdims.Select(i=>i.ToString()))}");

         // Scales and draws histogram
         hist = hist * (maxVal != 0 ? height / maxVal : 0.0);
         for (int j = 0; j < hdims[0]; ++j)
         {
            int binW = (int)((double)width / hdims[0]);
            //System.Diagnostics.Debug.WriteLine($"j={j}, hist={render.Rows - (int)(hist.Get<float>(j))}");
            render.Rectangle(
                new OpenCvSharp.Point(j * binW, render.Rows - (int)(hist.Get<float>(j))),
                new OpenCvSharp.Point((j + 1) * binW, render.Rows),
                foreground,
                -1);
         }

         return render;
      }
   }
}
