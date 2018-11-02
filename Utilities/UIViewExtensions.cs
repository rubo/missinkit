// Copyright 2017 Ruben Buniatyan
// Licensed under the MIT License. For full terms, see LICENSE in the project root.

using System;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace MissinKit.Utilities
{
    public static class UIViewExtensions
    {
        #region Fields
        private const string FrameLayoutGuideId = "MKFrameLayoutGuide";
        private static readonly bool IsIos11OrLater = UIDevice.CurrentDevice.CheckSystemVersion(11, 0);
        #endregion

        #region UIScrollView Methods
        /// <summary>
        /// Returns <see cref="UIScrollView.AdjustedContentInset"/> of the current <see cref="UIScrollView"/> object if available,
        /// or falls back to <see cref="UIScrollView.ContentInset"/> of the current <see cref="UIScrollView"/> object.
        /// </summary>
        /// <param name="scrollView"></param>
        /// <returns>
        /// <see cref="UIScrollView.AdjustedContentInset"/> of the current <see cref="UIScrollView"/> object if available;
        /// otherwise, <see cref="UIScrollView.ContentInset"/>.
        /// </returns>
        public static UIEdgeInsets AdjustedContentInset(this UIScrollView scrollView)
        {
            if (scrollView == null)
                throw new ArgumentNullException(nameof(scrollView));

            return IsIos11OrLater ? scrollView.AdjustedContentInset : scrollView.ContentInset;
        }

        #endregion

        #region UIView Methods
        /// <summary>
        /// Returns <see cref="UIView.SafeAreaInsets"/> of the current <see cref="UIView"/> object if available,
        /// or falls back to <see cref="UIEdgeInsets.Zero"/>.
        /// </summary>
        /// <param name="view"></param>
        /// <returns>
        /// <see cref="UIView.SafeAreaInsets"/> of the current <see cref="UIView"/> object if available;
        /// otherwise, <see cref="UIEdgeInsets.Zero"/>.
        /// </returns>
        public static UIEdgeInsets SafeAreaInsets(this UIView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            return IsIos11OrLater ? view.SafeAreaInsets : UIEdgeInsets.Zero;
        }

        /// <summary>
        /// Returns <see cref="UIView.SafeAreaLayoutGuide"/> of the current <see cref="UIView"/> object if available,
        /// or falls back to a layout guide based on the frame rectangle of the current <see cref="UIView"/> object.
        /// </summary>
        /// <param name="view"></param>
        /// <returns>
        /// <see cref="UIView.SafeAreaLayoutGuide"/> of the current <see cref="UIView"/> object if available;
        /// otherwise, <see cref="FrameLayoutGuide"/>.
        /// </returns>
        public static UILayoutGuide SafeAreaLayoutGuide(this UIView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            return IsIos11OrLater ? view.SafeAreaLayoutGuide : view.FrameLayoutGuide();
        }

        /// <summary>
        /// Creates a layout guide based on the frame rectangle of the current <see cref="UIView"/> object.
        /// Subsequent calls to this method returns the previously created <see cref="UILayoutGuide"/> object
        /// that has been added to the array of layout guides of the current <see cref="UIView"/> object.
        /// </summary>
        /// <remarks>
        /// For <see cref="UIScrollView"/>, returns the <see cref="UIScrollView.FrameLayoutGuide"/>.
        /// </remarks>
        /// <param name="view"></param>
        /// <returns>
        /// A layout guide based on the frame rectangle of the current <see cref="UIView"/> object.
        /// </returns>
        public static UILayoutGuide FrameLayoutGuide(this UIView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            if (IsIos11OrLater && view is UIScrollView scrollView)
                return scrollView.FrameLayoutGuide;

            var frameLayoutGuide = view.LayoutGuides.FirstOrDefault(l => l.Identifier == FrameLayoutGuideId);

            if (frameLayoutGuide == null)
            {
                frameLayoutGuide = new InternalFrameLayoutGuide { Identifier = FrameLayoutGuideId };

                view.AddLayoutGuide(frameLayoutGuide);
            }

            return frameLayoutGuide;
        }

        private class InternalFrameLayoutGuide : UILayoutGuide
        {
            public override NSLayoutYAxisAnchor BottomAnchor => OwningView.BottomAnchor;

            public override NSLayoutXAxisAnchor CenterXAnchor => OwningView.CenterXAnchor;

            public override NSLayoutYAxisAnchor CenterYAnchor => OwningView.CenterYAnchor;

            public override NSLayoutDimension HeightAnchor => OwningView.HeightAnchor;

            public override CGRect LayoutFrame => OwningView.Bounds;

            public override NSLayoutXAxisAnchor LeadingAnchor => OwningView.LeadingAnchor;

            public override NSLayoutXAxisAnchor LeftAnchor => OwningView.LeftAnchor;

            public override NSLayoutXAxisAnchor RightAnchor => OwningView.RightAnchor;

            public override NSLayoutYAxisAnchor TopAnchor => OwningView.TopAnchor;

            public override NSLayoutXAxisAnchor TrailingAnchor => OwningView.TrailingAnchor;

            public override NSLayoutDimension WidthAnchor => OwningView.WidthAnchor;
        }
        #endregion
    }
}
