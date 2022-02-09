﻿using System;
using System.Windows;
using System.Windows.Media.Imaging;

using ControlCore.Model;
using OpenCvSharp;

namespace ImageLabelingControl_OpenCV.Draw
{
    public class DrawEllipse : DrawLabelBase
    {
        private bool _IsFirstDraw;
        private IntPoint _DrawingStartPos;
        private IntPoint _DrawingLastPos;

        public override void OnMouseDown(System.Windows.Point mousePos, int imageWidth, 
            int imageHeight, int imageSize, int imageStride, int thickness, Scalar color)
        {
            this.imageWidth = imageWidth;
            this.imageHeight = imageHeight;
            this.imageSize = imageSize;
            this.imageStride = imageStride;
            this.thickness = thickness;
            this.color = color;

            _IsFirstDraw = true;
            _DrawingStartPos.Set(mousePos);
            tempLabelImage = new Mat(new OpenCvSharp.Size(imageWidth, imageHeight), MatType.CV_8UC4, new Scalar(0, 0, 0, 0));
        }

        public override void OnMouseMove(System.Windows.Point mousePos, WriteableBitmap writeableBitmap, ref Int32Rect roiRect)
        {
            int curX = (int)mousePos.X;
            int curY = (int)mousePos.Y;

            int centerX = (_DrawingStartPos.X + curX) / 2;
            int centerY = (_DrawingStartPos.Y + curY) / 2;
            int width = Math.Abs(_DrawingStartPos.X - curX);
            int height = Math.Abs(_DrawingStartPos.Y - curY);

            if (!_IsFirstDraw)
            {
                Cv2.Rectangle(tempLabelImage, new OpenCvSharp.Point(_DrawingStartPos.X, _DrawingStartPos.Y),
                    new OpenCvSharp.Point(_DrawingLastPos.X, _DrawingLastPos.Y), eraserColor, -1, LineTypes.Link8);


                Cv2.Ellipse(tempLabelImage, new RotatedRect(new Point2f(centerX, centerY),
                    new Size2f(width, height), 0), color, -1, LineTypes.Link8);

                writeableBitmap.WritePixels(roiRect, tempLabelImage.Data, imageSize, imageStride, roiRect.X, roiRect.Y);
                UpdateWriteableBitmapRoi(ref roiRect, _DrawingStartPos.X, _DrawingStartPos.Y, curX, curY);
            }
            else
            {
                Cv2.Ellipse(tempLabelImage, new RotatedRect(new Point2f(centerX, centerY),
                    new Size2f(width, height), 0), color, -1, LineTypes.Link8);

                UpdateWriteableBitmapRoi(ref roiRect, _DrawingStartPos.X, _DrawingStartPos.Y, curX, curY);
                writeableBitmap.WritePixels(roiRect, tempLabelImage.Data, imageSize, imageStride, roiRect.X, roiRect.Y);
            }

            _IsFirstDraw = false;
            _DrawingLastPos.Set(mousePos);
        }

        protected override void UpdateWriteableBitmapRoi(ref Int32Rect roiRect, int x1, int y1, int x2, int y2)
        {
            int startX = Math.Min(x1, x2);
            int startY = Math.Min(y1, y2);

            roiRect.X = startX;
            roiRect.Y = startY;
            roiRect.Width = Math.Abs(x1 - x2) + 2;
            roiRect.Height = Math.Abs(y1 - y2) + 2;
        }

        public override void OnMouseUp(Mat labelImage, WriteableBitmap writeableBitmap, 
            WriteableBitmap TempWriteableBitmap, Int32Rect roiRect)
        {
            if (!_IsFirstDraw)
            {
                Cv2.Rectangle(tempLabelImage, new OpenCvSharp.Point(_DrawingStartPos.X, _DrawingStartPos.Y),
                    new OpenCvSharp.Point(_DrawingLastPos.X, _DrawingLastPos.Y), eraserColor, -1, LineTypes.Link8);
                TempWriteableBitmap.WritePixels(roiRect, tempLabelImage.Data, imageSize, imageStride, roiRect.X, roiRect.Y);

                Cv2.Ellipse(labelImage, new RotatedRect(new OpenCvSharp.Point(roiRect.X + (roiRect.Width - 2) / 2,
                    roiRect.Y + (roiRect.Height -2) / 2), new Size2f(roiRect.Width -2, roiRect.Height -2), 0), color, -1, LineTypes.Link8);
                writeableBitmap.WritePixels(roiRect, labelImage.Data, imageSize, imageStride, roiRect.X, roiRect.Y);
            }

            _IsFirstDraw = true;
            tempLabelImage.Dispose();
        }
    }
}